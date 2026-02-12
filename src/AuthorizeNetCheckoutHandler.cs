using Dynamicweb.Core;
using Dynamicweb.Ecommerce.Cart;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Orders.Gateways;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Extensibility.Editors;
using Dynamicweb.Frontend;
using Dynamicweb.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi;

/// <summary>
/// AuthorizeNet API Checkout Handler
/// </summary>
[AddInName("Authorize.Net API"), AddInDescription("AuthorizeNet API Checkout Handler"), AddInUseParameterGrouping(true)]
public class AuthorizeNetCheckoutHandler : CheckoutHandler, ICancelOrder, IFullReturn, IRemoteCapture, ISavedCard, IParameterOptions
{
    private RenderFormMode _formMode = RenderFormMode.Hosted;
    private TransactionType _transactionType = TransactionType.AuthCaptureTransaction;

    private const string SavedCardNamePlaceholder = "NeedToSaveCardWithName:";
    private const string PaymentFormTemplateFolder = "eCom7/CheckoutHandler/AuthorizeNet/Post";
    private const string ErrorTemplateFolder = "eCom7/CheckoutHandler/AuthorizeNet/Error";
    private const string CancelTemplateFolder = "eCom7/CheckoutHandler/AuthorizeNet/Cancel";

    private AuthorizeNetService? _authorizeNetService;
    private AuthorizeNetService AuthorizeNetService =>
        _authorizeNetService ??= new AuthorizeNetService(ApiLoginId, TransactionKey, TestMode);

    #region AddIn parameters

    [AddInParameter("API login ID"), AddInParameterEditor(typeof(TextParameterEditor), "")]
    public string ApiLoginId { get; set; } = "";

    [AddInParameter("Transaction key"), AddInParameterEditor(typeof(TextParameterEditor), "")]
    public string TransactionKey { get; set; } = "";

    [AddInParameter("Signature key"), AddInParameterEditor(typeof(TextParameterEditor), "TextArea=true")]
    public string SignatureKey { get; set; } = "";

    [AddInLabel("Public client key"), AddInParameter("PublicClientKey"), AddInParameterEditor(typeof(TextParameterEditor), "")]
    public string PublicClientKey { get; set; } = "";

    [AddInParameter("Allow save cards"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool AllowSaveCards { get; set; }

    [AddInLabel("The type of credit card transaction"), AddInParameter("TypeOfTransaction"), AddInParameterEditor(typeof(RadioParameterEditor), "SortBy=Key")]
    public string TypeOfTransaction
    {
        get => _transactionType.ToString();
        set
        {
            if (Enum.TryParse(value, out TransactionType parsed))
                _transactionType = parsed;
        }
    }

    [AddInLabel("Test mode"), AddInParameter("TestMode"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool TestMode { get; set; }

    [AddInLabel("Payment form render mode"), AddInParameter("PaymentFormMode"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(RadioParameterEditor), "SortBy=Key")]
    public string PaymentFormMode
    {
        get => _formMode.ToString();
        set
        {
            if (Enum.TryParse(value, out RenderFormMode parsed))
                _formMode = parsed;
        }
    }

    private string _paymentFormTemplate = "";

    [AddInLabel("Payment form template"), AddInParameter("PaymentFormTemplate"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(TemplateParameterEditor), $"folder=Templates/{PaymentFormTemplateFolder}")]
    public string PaymentFormTemplate
    {
        get => TemplateHelper.GetTemplateName(_paymentFormTemplate);
        set => _paymentFormTemplate = value;
    }

    private string _cancelTemplate = "";

    [AddInLabel("Cancel template"), AddInParameter("CancelTemplate"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(TemplateParameterEditor), $"folder=Templates/{CancelTemplateFolder}")]
    public string CancelTemplate
    {
        get => TemplateHelper.GetTemplateName(_cancelTemplate);
        set => _cancelTemplate = value;
    }

    private string _errorTemplate = "";

    [AddInLabel("Error template"), AddInParameter("ErrorTemplate"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(TemplateParameterEditor), $"folder=Templates/{ErrorTemplateFolder}")]
    public string ErrorTemplate
    {
        get => TemplateHelper.GetTemplateName(_errorTemplate);
        set => _errorTemplate = value;
    }

    #endregion

    public override OutputResult BeginCheckout(Order order, CheckoutParameters parameters)
    {
        LogEvent(order, "Checkout started");

        if (_formMode is RenderFormMode.Hosted)
            return RedirectToHostedForm(order);

        string javaScriptUrl = _formMode is RenderFormMode.Manual
            ? AuthorizeNetEndpoints.GetAcceptJsUrl(TestMode)
            : AuthorizeNetEndpoints.GetAcceptUiUrl(TestMode);

        var template = new Template(TemplateHelper.GetTemplatePath(PaymentFormTemplate, PaymentFormTemplateFolder));
        template.SetTag(Tags.ApiLoginId, ApiLoginId);
        template.SetTag(Tags.AuthorizeNetJavaScriptUrl, javaScriptUrl);
        template.SetTag(Tags.FormAction, $"{GetBaseUrl(order)}&action=FormPost");
        template.SetTag(Tags.PublicClientKey, PublicClientKey);

        return new ContentOutputResult
        {
            Content = Render(order, template)
        };
    }

    public override OutputResult HandleRequest(Order order)
    {
        LogEvent(order, "Redirected to AuthorizeNet CheckoutHandler");
        var action = Converter.ToString(Context.Current?.Request["action"]);

        if (string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(order.GatewayResult))
        {
            Callback(order);
            return ContentOutputResult.Empty;
        }

        try
        {
            return action switch
            {
                "FormPost" => CreatePaymentTransaction(order, null),
                "Receipt" => OrderCompleted(order, 0d, null),
                "Cancel" => OrderCancelled(order),
                _ => ContentOutputResult.Empty
            };
        }
        catch (Exception ex)
        {
            return OnError(order, ex.Message, ex);
        }
    }

    private OutputResult RedirectToHostedForm(Order order)
    {
        LogEvent(order, "Redirect to Hosted Form");
        double orderAmount = Ecommerce.Services.Currencies.Round(order.Currency, order.Price.Price);

        string baseUrl = GetBaseUrl(order);
        string encodedBaseUrl = baseUrl.Replace("&", "%26");
        string returnUrlPattern = $"{encodedBaseUrl}%26action={{0}}";
        string receiptUrl = string.Format(returnUrlPattern, "Receipt");
        string cancelUrl = string.Format(returnUrlPattern, "Cancel");

        var settings = new HostedPaymentSettings
        {
            Setting = new List<Setting> {
                new Setting {
                    SettingName = SettingEnum.HostedPaymentReturnOptions.ToEnumMemberValue(),
                    SettingValue = Converter.SerializeCompact(new HostedPaymentReturnOptions
                    {
                        Url = receiptUrl,
                        CancelUrl = cancelUrl
                    })
                },
                new Setting {
                    SettingName = SettingEnum.HostedPaymentPaymentOptions.ToEnumMemberValue(),
                    SettingValue = Converter.SerializeCompact(new HostedPaymentPaymentOptions
                    {
                        ShowBankAccount = false
                    })
                },
            }
        };

        var transactionRequest = CreateTransactionRequest(order, orderAmount, null, null, true);
        var response = AuthorizeNetService.GetHostedPaymentPage(transactionRequest, settings);

        if (Enum.TryParse(response?.Messages.ResultCode, true, out MessageTypeEnum resultCode) is true &&
            resultCode is MessageTypeEnum.Ok)
        {
            var formUrl = AuthorizeNetEndpoints.GetHostedFormUrl(TestMode);
            return GetSubmitFormResult(formUrl, new Dictionary<string, string>
            {
                ["token"] = response!.Token
            });
        }

        var message = response?.Messages.Message.FirstOrDefault();
        return OnError(order, $"Failed to get hosted payment page ({message?.Code}): {message?.Text}");
    }

    private OutputResult CreatePaymentTransaction(Order order, CustomerProfilePaymentType? profileToCharge)
    {
        LogEvent(order, "Create payment transaction");
        double orderAmount = Ecommerce.Services.Currencies.Round(order.Currency, order.Price.Price);

        PaymentType? payment = null;
        if (profileToCharge is null)
        {
            payment = new PaymentType
            {
                OpaqueData = new OpaqueDataType
                {
                    DataValue = Converter.ToString(Context.Current?.Request["dataValue"]),
                    DataDescriptor = Converter.ToString(Context.Current?.Request["dataDescriptor"]),
                }
            };
        }

        var transactionRequest = CreateTransactionRequest(order, orderAmount, payment, profileToCharge, profileToCharge == null);
        var response = AuthorizeNetService.CreateTransaction(transactionRequest);

        ;
        if (response?.TransactionResponse?.Messages is not null &&
            Enum.TryParse(response?.Messages.ResultCode, true, out MessageTypeEnum resultCode) &&
            resultCode is MessageTypeEnum.Ok)
            return OrderCompleted(order, orderAmount, response);

        return OrderRefused(order, response?.TransactionResponse?.Errors.FirstOrDefault()?.ErrorText);
    }

    private OutputResult OrderCompleted(Order order, double transactionAmount, CreateTransactionResponse? response)
    {
        LogEvent(order, "State ok");
        string cardName;
        var needSaveCard = NeedSaveCard(order, out cardName);

        order.TransactionAmount = transactionAmount;
        if (response?.TransactionResponse is not null)
        {
            OrderHelper.UpdateTransactionNumber(order, response.TransactionResponse.TransId);
            order.TransactionStatus = AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(response.TransactionResponse.ResponseCode);
            order.TransactionCardType = response.TransactionResponse.AccountType;
            order.TransactionCardNumber = response.TransactionResponse.AccountNumber;

            if (needSaveCard)
                SaveCard(order, cardName, response.TransactionResponse.NetworkTransId, transactionAmount);
        }
        else if (needSaveCard)
            order.GatewayPaymentStatus = $"{SavedCardNamePlaceholder}{cardName}";

        if (_transactionType is TransactionType.AuthCaptureTransaction)
        {
            order.CaptureInfo = new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Success, "Capture successful");
            order.CaptureAmount = transactionAmount;
        }

        if (!order.Complete)
        {
            SetOrderComplete(order);
            CheckoutDone(order);
        }
        else
            Save(order);

        return PassToCart(order);
    }

    private OutputResult OrderCancelled(Order order)
    {
        LogEvent(order, "Order cancelled");
        order.TransactionStatus = "Cancelled";
        CheckoutDone(order);

        var cancelTemplate = new Template(TemplateHelper.GetTemplatePath(CancelTemplate, CancelTemplateFolder));
        cancelTemplate.SetTag("CheckoutHandler:CancelMessage", "Payment has been cancelled before processing was completed");

        var orderRenderer = new Frontend.Renderer();
        orderRenderer.RenderOrderDetails(cancelTemplate, order, true);

        return new ContentOutputResult
        {
            Content = cancelTemplate.Output()
        };
    }

    private OutputResult OrderRefused(Order order, string? refusalReason) => OnError(order, $"Payment was refused. Refusal reason: {refusalReason}");

    private OutputResult PaymentError(Order order, string reason) => OnError(order, $"There was an error when the payment was being processed. Reason: {reason}");

    private TransactionRequestType CreateTransactionRequest(Order order, double orderAmount, PaymentType? payment, CustomerProfilePaymentType? profileToCharge, bool includeCustomerData)
    {
        var request = new TransactionRequestType
        {
            Amount = orderAmount,
            CurrencyCode = order.CurrencyCode,
            Payment = payment,
            LineItems = AuthorizeNetModelFactory.CreateLineItems(order),
            Order = new Models.OrderType { InvoiceNumber = order.Id },
            CustomerIp = StringHelper.Crop(order.Ip, 15),
            Profile = profileToCharge,
            TransactionType = _transactionType == TransactionType.AuthCaptureTransaction
                ? TransactionTypeEnum.AuthCaptureTransaction
                : TransactionTypeEnum.AuthOnlyTransaction
        };

        if (profileToCharge != null)
        {
            var savedCard = Ecommerce.Services.PaymentCard.GetById(order.SavedCardId);
            if (savedCard?.Token is not null)
            {
                var (_, networkTransId, originalAmount) = ExtractCofData(savedCard.Token);
                request.ProcessingOptions = new ProcessingOptionsType
                {
                    IsSubsequentAuth = true,
                    IsStoredCredentials = true
                };

                if (!string.IsNullOrEmpty(networkTransId))
                {
                    request.SubsequentAuthInformation = new SubsequentAuthInformationType
                    {
                        OriginalNetworkTransId = networkTransId,
                        OriginalAuthAmount = originalAmount ?? orderAmount,
                        Reason = SubsequentAuthReasonEnum.Resubmission.ToEnumMemberValue()
                    };
                }
            }
        }
        else if (NeedSaveCard(order, out _))
            SetFirstTransactionFlags(request, true);

        if (includeCustomerData)
        {
            request.BillTo = AuthorizeNetModelFactory.CreateBillAddress(order);
            request.Customer = new CustomerDataType
            {
                Id = order.CustomerAccessUserId.ToString(),
                Email = order.CustomerEmail
            };

            var shipAddress = AuthorizeNetModelFactory.CreateShipAddress(order);
            if (!string.IsNullOrEmpty(shipAddress.Address))
                request.ShipTo = shipAddress;
        }

        return request;
    }

    private void Callback(Order order)
    {
        LogEvent(order, "Notification callback started");
        if (string.IsNullOrEmpty(SignatureKey))
            throw new ArgumentNullException(nameof(SignatureKey), "Specify Signature key to handle notifications");

        string? gatewayResult = order.GatewayResult;
        order.GatewayResult = string.Empty;
        Ecommerce.Services.Orders.UpdateGatewayResult(order, false);

        var hmacSignature = Context.Current?.Request?.Headers["X-ANET-Signature"]?.Replace("sha512=", string.Empty);
        if (!HmacValidator.IsValid(SignatureKey.Trim(), gatewayResult, hmacSignature))
        {
            LogError(order, "Cannot handle notification item: HMAC validation failed");
            return;
        }

        var requestData = Converter.Deserialize<NotificationItem>(gatewayResult);
        if (requestData?.Payload is null)
            return;

        NotificationPayload payload = requestData.Payload;

        switch (requestData.GetEventType())
        {
            case NotificationEventType.AuthCaptureCreated:
            case NotificationEventType.AuthCreated:
                HandleAuthOrCaptureCallback(order, payload, requestData.GetEventType() == NotificationEventType.AuthCaptureCreated);
                break;

            case NotificationEventType.CaptureCreated:
            case NotificationEventType.PriorAuthCaptureCreated:
                HandleCaptureCallback(order, payload);
                break;

            case NotificationEventType.RefundCreated:
                HandleRefundCallback(order, payload);
                break;

            case NotificationEventType.VoidCreated:
                HandleVoidCallback(order, payload);
                break;
        }
    }

    private void HandleAuthOrCaptureCallback(Order order, NotificationPayload payload, bool isCaptured)
    {
        if (payload.ResponseCode != 1)
        {
            order.TransactionStatus = $"Authorisation failed: {AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode)}";
            Save(order);

            return;
        }

        UpdateTransactionData(order, payload);
        if (string.IsNullOrEmpty(order.TransactionCardNumber))
        {
            TransactionDetailsType? transactionDetails = AuthorizeNetService.GetTransactionDetails(payload.Id);
            if (transactionDetails?.Payment.CreditCard is not null)
            {
                order.TransactionCardType = transactionDetails.Payment.CreditCard.CardType.ToString();
                order.TransactionCardNumber = transactionDetails.Payment.CreditCard.CardNumber;
            }

            if (order.GatewayPaymentStatus?.StartsWith(SavedCardNamePlaceholder) is true)
            {
                var cardName = order.GatewayPaymentStatus.Substring(SavedCardNamePlaceholder.Length);
                order.GatewayPaymentStatus = string.Empty;
                SaveCard(order, cardName, transactionDetails?.NetworkTransId, payload.Amount);
            }
        }

        if (isCaptured)
        {
            order.CaptureInfo = new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Success, "Capture successful");
            order.CaptureAmount = payload.Amount;
        }

        if (!order.Complete)
            SetOrderComplete(order);
        else
            Save(order);
    }

    private void HandleCaptureCallback(Order order, NotificationPayload payload)
    {
        if (payload.ResponseCode == 1)
        {
            UpdateTransactionData(order, payload);
            order.CaptureInfo = new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Success, "Capture successful");
            order.CaptureAmount = payload.Amount;
        }
        else
        {
            order.CaptureInfo = new OrderCaptureInfo(
                OrderCaptureInfo.OrderCaptureState.Failed,
                $"Capture failed: {AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode)}"
            );
        }

        Save(order);
    }

    private void HandleRefundCallback(Order order, NotificationPayload payload)
    {
        if (payload.ResponseCode == 1)
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.FullyReturned, "Order refund successful", payload.Amount, order);
        else
        {
            OrderReturnInfo.SaveReturnOperation(
                OrderReturnOperationState.Failed,
                $"Order refund failed: {AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode)}",
                payload.Amount, order
            );
        }
    }

    private void HandleVoidCallback(Order order, NotificationPayload payload)
    {
        order.CaptureInfo = payload.ResponseCode == 1
            ? new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Cancel, "Cancel successful")
            : new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Cancel, $"Cancel order failed: {AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode)}");

        Save(order);
    }

    private void UpdateTransactionData(Order order, NotificationPayload payload)
    {
        OrderHelper.UpdateTransactionNumber(order, payload.Id);
        order.TransactionAmount = payload.Amount;
        order.TransactionStatus = AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode);
    }

    public bool CancelOrder(Order order)
    {
        LogEvent(order, "Attempting cancel");
        var errorText = OrderHelper.GetOrderError(order);
        if (!string.IsNullOrEmpty(errorText))
        {
            LogError(order, errorText);
            return false;
        }

        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.VoidTransaction,
            RefTransId = order.TransactionNumber,
        };

        var response = AuthorizeNetService.CreateTransaction(transactionRequest);

        if (response?.TransactionResponse?.Messages is not null &&
            Enum.TryParse(response?.Messages.ResultCode, true, out MessageTypeEnum resultCode) &&
            resultCode is MessageTypeEnum.Ok)
        {
            LogEvent(order, "Cancel order succeed");
            return true;
        }

        LogError(order, AuthorizeNetErrorMessageBuilder.Create("Cancel order failed with message", response));
        return false;
    }

    public OrderCaptureInfo Capture(Order order)
    {
        LogEvent(order, "Attempting capture");
        string errorText = OrderHelper.GetOrderError(order);
        if (!string.IsNullOrEmpty(errorText))
        {
            LogError(order, errorText);
            return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, errorText);
        }

        double orderAmount = Converter.ToDouble(Ecommerce.Services.Currencies.Round(order.Currency, order.Price.Price));
        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.PriorAuthCaptureTransaction,
            Amount = orderAmount,
            CurrencyCode = order.CurrencyCode,
            RefTransId = order.TransactionNumber,
        };

        var response = AuthorizeNetService.CreateTransaction(transactionRequest);

        if (response?.TransactionResponse?.Messages is not null &&
            Enum.TryParse(response?.Messages.ResultCode, true, out MessageTypeEnum resultCode) &&
            resultCode is MessageTypeEnum.Ok)
        {
            LogEvent(order, "Capture successful", DebuggingInfoType.CaptureResult);
            OrderHelper.UpdateTransactionNumber(order, response.TransactionResponse.TransId);
            return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Success, "Capture successful");
        }

        string infoText = AuthorizeNetErrorMessageBuilder.Create("Payment was unsucceeded with error", response);
        LogEvent(order, infoText, DebuggingInfoType.CaptureResult);
        order.CaptureInfo.Message = infoText;
        order.CaptureInfo.State = OrderCaptureInfo.OrderCaptureState.Failed;
        Save(order);

        return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, infoText);
    }

    public void FullReturn(Order order)
    {
        var errorText = OrderHelper.GetOrderError(order);
        if (!string.IsNullOrEmpty(errorText))
        {
            LogError(order, errorText);
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorText, 0, order);

            return;
        }

        double refundAmount = order.TransactionAmount;
        var creditCard = new CreditCardType
        {
            CardNumber = order.TransactionCardNumber.Substring(Math.Max(0, order.TransactionCardNumber.Length - 4)),
            ExpirationDate = "XXXX",
        };

        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.RefundTransaction,
            Payment = new PaymentType { CreditCard = creditCard },
            Amount = Converter.ToDouble(refundAmount),
            CurrencyCode = order.CurrencyCode,
            Order = new Models.OrderType { InvoiceNumber = order.Id },
            RefTransId = order.TransactionNumber
        };

        var response = AuthorizeNetService.CreateTransaction(transactionRequest);

        if (response?.TransactionResponse?.Messages is not null &&
            Enum.TryParse(response.Messages?.ResultCode, true, out MessageTypeEnum resultCode) &&
            resultCode is MessageTypeEnum.Ok)
        {
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.FullyReturned, "Authorize.Net has full refunded payment.", refundAmount, order);
        }
        else
        {
            string infoText = AuthorizeNetErrorMessageBuilder.Create("Refund was unsucceeded with message", response);
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, infoText, refundAmount, order);
        }
    }

    public void DeleteSavedCard(int savedCardId)
    {
        PaymentCardToken? savedCard = Ecommerce.Services.PaymentCard.GetById(savedCardId);
        if (savedCard is null)
            return;

        var (cardToken, _, _) = ExtractCofData(savedCard.Token);
        int userId = savedCard.UserID;

        if (userId <= 0 || string.IsNullOrEmpty(cardToken))
            return;

        CustomerProfileMaskedType? profile = GetCustomerProfile(userId, false);
        if (string.IsNullOrEmpty(profile?.CustomerProfileId) || string.IsNullOrEmpty(cardToken))
            return;

        AuthorizeNetService.DeleteCustomerPaymentProfile(profile.CustomerProfileId, cardToken);
    }

    public string UseSavedCard(Order order)
    {
        PaymentCardToken? savedCard = Ecommerce.Services.PaymentCard.GetById(order.SavedCardId);
        if (!string.IsNullOrEmpty(savedCard?.Token) && order.CustomerAccessUserId == savedCard.UserID)
        {
            CustomerProfileMaskedType? profile = GetCustomerProfile(order.CustomerAccessUserId, false);
            if (!string.IsNullOrEmpty(profile?.CustomerProfileId))
            {
                var (cardToken, _, _) = ExtractCofData(savedCard.Token);
                if (!string.IsNullOrEmpty(cardToken))
                {
                    var profileToCharge = new CustomerProfilePaymentType
                    {
                        CustomerProfileId = profile.CustomerProfileId,
                        PaymentProfile = new PaymentProfile
                        {
                            PaymentProfileId = cardToken
                        },
                    };
                    if (CreatePaymentTransaction(order, profileToCharge) is ContentOutputResult result)
                        return result.Content;
                }
            }
        }

        return BeginCheckout(order) is ContentOutputResult checkoutResult
            ? checkoutResult.Content
            : string.Empty;
    }

    public bool SavedCardSupported(Order order) => AllowSaveCards;

    private bool NeedSaveCard(Order order, out string cardName)
    {
        cardName = string.Empty;
        if (AllowSaveCards && order.CustomerAccessUserId > 0 && (order.DoSaveCardToken || !string.IsNullOrEmpty(order.SavedCardDraftName)))
        {
            cardName = !string.IsNullOrEmpty(order.SavedCardDraftName) ? order.SavedCardDraftName : order.Id;
            return true;
        }

        return false;
    }

    private void SaveCard(Order order, string cardName, string? networkTransId, double? originalAmount)
    {
        if (!AllowSaveCards || order.CustomerAccessUserId <= 0)
            return;

        CustomerProfileMaskedType? profile = GetCustomerProfile(order.CustomerAccessUserId, true);
        if (profile is null)
            return;

        string? cardToken = AuthorizeNetService.CreatePaymentProfileFromTransaction(order.TransactionNumber, profile.CustomerProfileId);
        if (string.IsNullOrEmpty(cardToken))
            return;

        var finalToken = !string.IsNullOrEmpty(networkTransId) && originalAmount.HasValue
            ? $"{cardToken}|{networkTransId}|{originalAmount:F2}"
            : cardToken;

        PaymentCardToken? existingCard = Ecommerce.Services.PaymentCard.GetByUserId(order.CustomerAccessUserId)
            .FirstOrDefault(t => ExtractCofData(t.Token).cardToken == cardToken);

        if (existingCard is null)
        {
            existingCard = Ecommerce.Services.PaymentCard.CreatePaymentCard(
                order.CustomerAccessUserId, order.PaymentMethodId, cardName,
                order.TransactionCardType, order.TransactionCardNumber, finalToken
            );
        }
        else if (!string.IsNullOrEmpty(networkTransId) && !string.Equals(existingCard.Token, finalToken, StringComparison.Ordinal))
        {
            existingCard.Token = finalToken;
        }

        order.SavedCardId = existingCard.ID;
        Save(order);
        LogEvent(order, "Saved card with COF data: {0}", existingCard.Name);
    }

    private CustomerProfileMaskedType? GetCustomerProfile(int userId, bool tryCreate) => AuthorizeNetService.GetCustomerProfile(userId, tryCreate);

    private string CreatePaymentProfileFromTransaction(string transactionId, string customerProfileId) => AuthorizeNetService.CreatePaymentProfileFromTransaction(transactionId, customerProfileId);

    public IEnumerable<ParameterOption> GetParameterOptions(string parameterName)
    {
        return parameterName switch
        {
            "PaymentFormMode" =>
            [
                new("Hosted (Use Authorize.Net hosted form)", nameof(RenderFormMode.Hosted)),
                new("Partial Hosted (Show Authorize.Net hosted form in pop-up on your own template)", nameof(RenderFormMode.HostedPartial)),
                new("Manual (Use your own payment form. SSL required)", nameof(RenderFormMode.Manual))
            ],
            "TypeOfTransaction" =>
            [
                new("Authorization and Capture", nameof(TransactionType.AuthCaptureTransaction)),
                new("Authorization only", nameof(TransactionType.AuthOnlyTransaction))
            ],
            _ => []
        };
    }

    private OutputResult OnError(Order order, string message, Exception? exception = null)
    {
        if (exception != null)
        {
            LogError(order, exception, message);
        }
        else
        {
            LogError(order, message);
        }

        Ecommerce.Services.Orders.DowngradeToCart(order);
        Common.Context.SetCart(order);

        if (string.IsNullOrWhiteSpace(ErrorTemplate))
            return PassToCart(order);

        var errorTemplate = new Template(TemplateHelper.GetTemplatePath(ErrorTemplate, ErrorTemplateFolder));
        errorTemplate.SetTag("CheckoutHandler:ErrorMessage", message);

        return new ContentOutputResult { Content = Render(order, errorTemplate) };
    }

    private void Save(Order order) => Ecommerce.Services.Orders.Save(order);

    private void SetFirstTransactionFlags(TransactionRequestType request, bool isCardOnFile)
    {
        if (isCardOnFile)
        {
            if (request.ProcessingOptions is null)
                request.ProcessingOptions = new ProcessingOptionsType();

            request.ProcessingOptions.IsStoredCredentials = true;
        }
    }

    private (string? cardToken, string? networkTransId, double? originalAmount) ExtractCofData(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return (token, null, null);

        string[] parts = token.Split('|');

        return parts.Length >= 3
            ? (parts[0], parts[1], Converter.ToNullableDouble(parts[2]))
            : (token, null, null);
    }
}
