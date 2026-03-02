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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi;

/// <summary>
/// AuthorizeNet API Checkout Handler
/// </summary>
[AddInName("Authorize.Net API"), AddInDescription("AuthorizeNet API Checkout Handler"), AddInUseParameterGrouping(true)]
public class AuthorizeNetCheckoutHandler : CheckoutHandler, ICancelOrder, IFullReturn, IRemoteCapture, IRemotePartialCapture, ISavedCard, IParameterOptions
{
    private RenderFormMode _formMode = RenderFormMode.Hosted;
    private TransactionType _transactionType = TransactionType.AuthCaptureTransaction;

    /// <summary>
    /// Gets AuthorizeNetService instance
    /// </summary>
    private AuthorizeNetService GetAuthorizeNetService() =>
        new AuthorizeNetService(ApiLoginId, TransactionKey, TestMode, DebugLogging, null, null);


    /// <summary>
    /// Gets AuthorizeNetService instance with Order context for enhanced logging
    /// </summary>
    private AuthorizeNetService GetAuthorizeNetService(Order order) =>
        new AuthorizeNetService(ApiLoginId, TransactionKey, TestMode, DebugLogging, null, order);

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

    [AddInLabel("Payment form template"), AddInParameter("PaymentFormTemplate"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(TemplateParameterEditor), $"folder=Templates/{TemplateFolders.PaymentFormTemplateFolder}")]
    public string PaymentFormTemplate
    {
        get => TemplateHelper.GetTemplateName(_paymentFormTemplate);
        set => _paymentFormTemplate = value;
    }

    private string _cancelTemplate = "";

    [AddInLabel("Cancel template"), AddInParameter("CancelTemplate"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(TemplateParameterEditor), $"folder=Templates/{TemplateFolders.CancelTemplateFolder}")]
    public string CancelTemplate
    {
        get => TemplateHelper.GetTemplateName(_cancelTemplate);
        set => _cancelTemplate = value;
    }

    private string _errorTemplate = "";

    [AddInLabel("Error template"), AddInParameter("ErrorTemplate"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(TemplateParameterEditor), $"folder=Templates/{TemplateFolders.ErrorTemplateFolder}")]
    public string ErrorTemplate
    {
        get => TemplateHelper.GetTemplateName(_errorTemplate);
        set => _errorTemplate = value;
    }

    private bool _debugLogging = false;

    [AddInLabel("Enable debug logging"), AddInParameter("DebugLogging"), AddInParameterGroup("Advanced settings"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool DebugLogging
    {
        get => _debugLogging;
        set => _debugLogging = value;
    }

    #endregion

    /// <summary>
    /// Default constructor
    /// </summary>
    public AuthorizeNetCheckoutHandler()
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
    }

    public override OutputResult BeginCheckout(Order order, CheckoutParameters parameters)
    {
        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);

            LogEvent(order, "Checkout started");

            if (_formMode is RenderFormMode.Hosted)
                return RedirectToHostedForm(order, service);

            string javaScriptUrl = _formMode is RenderFormMode.Manual
                ? AuthorizeNetEndpoints.GetAcceptJsUrl(TestMode)
                : AuthorizeNetEndpoints.GetAcceptUiUrl(TestMode);

            var template = new Template(TemplateHelper.GetTemplatePath(PaymentFormTemplate, TemplateFolders.PaymentFormTemplateFolder));
            template.SetTag(Tags.ApiLoginId, ApiLoginId);
            template.SetTag(Tags.AuthorizeNetJavaScriptUrl, javaScriptUrl);
            template.SetTag(Tags.FormAction, $"{GetBaseUrl(order)}&action=FormPost");
            template.SetTag(Tags.PublicClientKey, PublicClientKey);

            return new ContentOutputResult
            {
                Content = Render(order, template)
            };
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Unhandled exception with message: {0}", ex.Message);
            return OnError(order, ex.Message, ex);
        }
    }

    public override OutputResult HandleRequest(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);

            LogEvent(order, "Redirected to AuthorizeNet CheckoutHandler");

            SecurityValidationResult securityValidation = SecurityHelper.ValidateSecurityConfiguration(
                ApiLoginId, TransactionKey, SignatureKey, TestMode);

            if (!securityValidation.IsValid)
            {
                string issues = string.Join("; ", securityValidation.SecurityIssues);
                string message = $"Security validation failed for Order {order.Id}: {issues}";
                LogError(order, message);

                return OnError(order, message);
            }

            string action = Converter.ToString(Context.Current?.Request["action"]);

            if (string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(order.GatewayResult))
            {
                Callback(order);
                return ContentOutputResult.Empty;
            }

            return action switch
            {
                "FormPost" => CreatePaymentTransaction(order, null, service),
                "Receipt" => OrderCompleted(order, 0, null),
                "Cancel" => OrderCancelled(order),
                _ => ContentOutputResult.Empty
            };
        }
        catch (Exception ex)
        {
            return OnError(order, ex.Message, ex);
        }
    }

    private OutputResult RedirectToHostedForm(Order order, AuthorizeNetService service)
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
            Setting = new List<Setting>
            {
                new()
                {
                    SettingName = SettingEnum.HostedPaymentReturnOptions.ToEnumMemberValue(),
                    SettingValue = Converter.SerializeCompact(new HostedPaymentReturnOptions
                    {
                        Url = receiptUrl,
                        CancelUrl = cancelUrl
                    })
                },
                new()
                {
                    SettingName = SettingEnum.HostedPaymentPaymentOptions.ToEnumMemberValue(),
                    SettingValue = Converter.SerializeCompact(new HostedPaymentPaymentOptions
                    {
                        ShowBankAccount = false
                    })
                },
            }
        };

        TransactionRequestType transactionRequest = CreateTransactionRequest(order, orderAmount, null, null, true);
        GetHostedPaymentPageResponse? response = service.GetHostedPaymentPage(transactionRequest, settings);

        if (IsResponseSuccessful(response, order))
        {
            string formUrl = AuthorizeNetEndpoints.GetHostedFormUrl(TestMode);

            return GetSubmitFormResult(formUrl, new Dictionary<string, string>
            {
                ["token"] = response.Token
            });
        }

        Message? message = response?.Messages?.Message?.FirstOrDefault();
        return OnError(order, $"Failed to get hosted payment page ({message?.Code}): {message?.Text}");
    }

    private OutputResult CreatePaymentTransaction(Order order, CustomerProfilePaymentType? profileToCharge, AuthorizeNetService? service)
    {
        service ??= GetAuthorizeNetService(order);
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

        TransactionRequestType transactionRequest = CreateTransactionRequest(order, orderAmount, payment, profileToCharge, profileToCharge is null);
        CreateTransactionResponse? response = service.CreateTransaction(transactionRequest);

        if (IsResponseSuccessful(response, order))
            return OrderCompleted(order, orderAmount, response);

        return OrderRefused(order, response?.TransactionResponse?.Errors.FirstOrDefault()?.ErrorText);
    }

    private OutputResult OrderCompleted(Order order, double transactionAmount, CreateTransactionResponse? response)
    {
        LogEvent(order, "State ok");
        bool needSaveCard = NeedSaveCard(order, out string cardName);

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
            order.GatewayPaymentStatus = $"{SecuritySettings.SavedCardNamePlaceholder}{cardName}";

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

        var cancelTemplate = new Template(TemplateHelper.GetTemplatePath(CancelTemplate, TemplateFolders.CancelTemplateFolder));
        cancelTemplate.SetTag("CheckoutHandler:CancelMessage", "Payment has been cancelled before processing was completed");

        return new ContentOutputResult
        {
            Content = Render(order, cancelTemplate)
        };
    }

    private OutputResult OrderRefused(Order order, string? refusalReason) => OnError(order, $"Payment was refused. Refusal reason: {refusalReason}");

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
            TransactionType = _transactionType is TransactionType.AuthCaptureTransaction
                ? TransactionTypeEnum.AuthCaptureTransaction
                : TransactionTypeEnum.AuthOnlyTransaction
        };

        if (profileToCharge is not null)
        {
            PaymentCardToken? savedCard = Ecommerce.Services.PaymentCard.GetById(order.SavedCardId);
            if (savedCard?.Token is not null)
            {
                (_, string? networkTransId, double? originalAmount) = ExtractCofData(savedCard.Token);
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

        string? hmacSignature = Context.Current?.Request?.Headers["X-ANET-Signature"]?.Replace("sha512=", string.Empty);
        if (!HmacValidator.IsValid(SignatureKey.Trim(), gatewayResult, hmacSignature))
        {
            LogError(order, "Cannot handle notification item: HMAC validation failed");
            return;
        }

        NotificationItem? requestData = Converter.Deserialize<NotificationItem>(gatewayResult);
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
            AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType? transactionDetails = service.GetTransactionDetails(payload.Id);
            if (transactionDetails?.Payment.CreditCard is not null)
            {
                order.TransactionCardType = transactionDetails.Payment.CreditCard.CardType.ToString();
                order.TransactionCardNumber = transactionDetails.Payment.CreditCard.CardNumber;
            }

            if (order.GatewayPaymentStatus?.StartsWith(SecuritySettings.SavedCardNamePlaceholder) is true)
            {
                var cardName = order.GatewayPaymentStatus.Substring(SecuritySettings.SavedCardNamePlaceholder.Length);
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
            order.CaptureInfo = new OrderCaptureInfo
            (
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
            OrderReturnInfo.SaveReturnOperation
            (
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

    /// <summary>
    /// Cancels an order (performs Void transaction in Authorize.Net)
    /// Void is only possible on the day of the transaction before batch closing time
    /// If void is not possible, the system will automatically perform a refund
    /// </summary>
    /// <param name="order">Order to cancel</param>
    public bool CancelOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            LogEvent(order, "Cancel order attempt for Order {0}, Amount {1}", order.Id, order.TransactionAmount);

            if (string.IsNullOrEmpty(order.Id))
            {
                LogError(order, PreparedMessages.OrderIdNotSetMessage);
                return false;
            }

            if (string.IsNullOrEmpty(order.TransactionNumber))
            {
                LogError(order, PreparedMessages.TransactionNumberNotSetMessage);
                return false;
            }

            string errorText = OrderHelper.GetOrderError(order);
            if (!string.IsNullOrEmpty(errorText))
            {
                LogError(order, errorText);
                return false;
            }

            if (TryVoidTransaction(order, service))
            {
                LogEvent(order, "Void transaction successful");
                return true;
            }

            LogEvent(order, "Void failed, attempting refund instead");
            FullReturn(order);
            LogEvent(order, "Order refunded");

            return true;
        }
        catch (Exception ex)
        {
            LogError(order, ex, $"{PreparedMessages.UnexpectedErrorMessage}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Attempt to perform transaction void
    /// </summary>
    private bool TryVoidTransaction(Order order, AuthorizeNetService service)
    {
        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.VoidTransaction,
            RefTransId = order.TransactionNumber,
            Amount = order.TransactionAmount,
            CurrencyCode = order.CurrencyCode
        };

        CreateTransactionResponse? response = service.CreateTransaction(transactionRequest);

        if (IsResponseSuccessful(response, order))
            return true;

        string errorMessage = response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
                              ?? response?.Messages?.Message?.FirstOrDefault()?.Text
                              ?? "Cancel order: Void transaction failed";

        LogError(order, errorMessage);
        return false;
    }


    /// <summary>
    /// Captures the authorized amount
    /// Capture must occur within 30 days of authorization
    /// </summary>
    /// <param name="order">Order to capture funds for</param>
    /// <returns>Information about the capture operation result</returns>
    public OrderCaptureInfo Capture(Order order)
    {
        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);

            LogEvent(order, "Attempting capture");

            if (order is null)
            {
                LogError(null, PreparedMessages.OrderNotSetMessage);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, PreparedMessages.OrderNotSetMessage);
            }

            if (string.IsNullOrEmpty(order.Id))
            {
                LogError(order, PreparedMessages.OrderIdNotSetMessage);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, PreparedMessages.OrderIdNotSetMessage);
            }

            if (string.IsNullOrEmpty(order.TransactionNumber))
            {
                LogError(order, PreparedMessages.TransactionNumberNotSetMessage);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, PreparedMessages.TransactionNumberRequiredMessage);
            }

            string errorText = OrderHelper.GetOrderError(order);
            if (!string.IsNullOrEmpty(errorText))
            {
                LogError(order, errorText);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, errorText);
            }

            double orderAmount = order.Price.Price;

            var transactionRequest = new TransactionRequestType
            {
                TransactionType = TransactionTypeEnum.PriorAuthCaptureTransaction,
                Amount = orderAmount,
                CurrencyCode = order.CurrencyCode,
                RefTransId = order.TransactionNumber,
                Order = new Models.OrderType
                {
                    InvoiceNumber = order.Id
                }
            };

            CreateTransactionResponse? response = service.CreateTransaction(transactionRequest);

            if (IsResponseSuccessful(response, order) && response?.TransactionResponse is not null)
            {
                LogEvent(order, PreparedMessages.CaptureSuccessMessage, DebuggingInfoType.CaptureResult);
                OrderHelper.UpdateTransactionNumber(order, response.TransactionResponse.TransId ?? "");

                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Success, PreparedMessages.CaptureSuccessMessage);
            }

            string infoText = response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
                             ?? response?.Messages?.Message?.FirstOrDefault()?.Text
                             ?? "Capture failed";
            LogEvent(order, infoText, DebuggingInfoType.CaptureResult);

            return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, infoText);
        }
        catch (Exception ex)
        {
            string errorMessage = $"{PreparedMessages.UnexpectedErrorMessage}: {ex.Message}";
            LogError(order, ex, errorMessage);

            return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, errorMessage);
        }
    }

    /// <summary>
    /// Partial capture of authorized amount
    /// </summary>
    /// <param name="order">Order to capture funds for</param>
    /// <param name="amount">Amount to capture</param>
    /// <param name="final">True if this is the final capture (no more captures will follow)</param>
    public OrderCaptureInfo Capture(Order order, long amount, bool final)
    {
        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            LogEvent(order, $"Attempting partial capture: {amount / 100.0:C}, final: {final}");

            if (order is null)
            {
                LogError(null, PreparedMessages.OrderNotSetMessage);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, PreparedMessages.OrderNotSetMessage);
            }

            if (string.IsNullOrEmpty(order.Id))
            {
                LogError(order, PreparedMessages.OrderIdNotSetMessage);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, PreparedMessages.OrderIdNotSetMessage);
            }

            if (string.IsNullOrEmpty(order.TransactionNumber))
            {
                LogError(order, PreparedMessages.TransactionNumberNotSetMessage);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, PreparedMessages.TransactionNumberRequiredMessage);
            }

            if (amount > order.Price.PricePIP)
            {
                string errorMsg = "Amount to capture should be less or equal to order total";
                LogError(order, errorMsg);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, errorMsg);
            }

            string errorText = OrderHelper.GetOrderError(order);
            if (!string.IsNullOrEmpty(errorText))
            {
                LogError(order, errorText);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, errorText);
            }

            (bool isValid, string? errorMessage) = ValidatePartialCapture(order, amount, final);
            if (!isValid)
            {
                LogError(order, errorMessage);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, errorMessage);
            }

            double captureAmount = amount / 100.0d;

            var transactionRequest = new TransactionRequestType
            {
                TransactionType = TransactionTypeEnum.PriorAuthCaptureTransaction,
                Amount = captureAmount,
                CurrencyCode = order.CurrencyCode,
                RefTransId = order.TransactionNumber,
                Order = new Models.OrderType
                {
                    InvoiceNumber = order.Id
                }
            };

            CreateTransactionResponse? response = service.CreateTransaction(transactionRequest);

            if (IsResponseSuccessful(response, order))
            {
                string captureMessage = final
                    ? $"Final partial capture successful: {captureAmount:C}"
                    : $"Partial capture successful: {captureAmount:C}";

                LogEvent(order, captureMessage, DebuggingInfoType.CaptureResult);
                return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Success, captureMessage);
            }

            string infoText = response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
                             ?? response?.Messages?.Message?.FirstOrDefault()?.Text
                             ?? "Partial capture failed";

            LogEvent(order, infoText, DebuggingInfoType.CaptureResult);

            return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, infoText);
        }
        catch (Exception ex)
        {
            string errorMessage = $"{PreparedMessages.UnexpectedErrorMessage}: {ex.Message}";
            LogError(order, ex, errorMessage);

            return new OrderCaptureInfo(OrderCaptureInfo.OrderCaptureState.Failed, errorMessage);
        }
    }

    /// <summary>
    /// Basic validation for partial capture
    /// </summary>
    private (bool isValid, string? errorMessage) ValidatePartialCapture(Order order, long amount, bool final)
    {
        if (amount <= 0)
            return (false, "Capture amount must be greater than zero");

        // Check that we don't exceed original authorized amount
        double totalRequestedAmount = order.CaptureAmount + (amount / 100.0);
        double authorizedAmount = order.Price.Price;

        if (totalRequestedAmount > authorizedAmount)
            return (false, $"Total capture amount ({totalRequestedAmount:C}) cannot exceed authorized amount ({authorizedAmount:C})");

        return (true, null);
    }

    /// <summary>
    /// Performs a full refund for the order (Refund transaction)
    /// </summary>
    /// <param name="order">Order to refund</param>
    public void FullReturn(Order order)
    {
        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);

            if (order is null)
            {
                LogError(null, PreparedMessages.OrderNotSetMessage);
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, PreparedMessages.OrderNotSetMessage, 0, order);
                return;
            }

            if (string.IsNullOrEmpty(order.Id))
            {
                LogError(order, PreparedMessages.OrderIdNotSetMessage);
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, PreparedMessages.OrderIdNotSetMessage, 0, order);
                return;
            }

            if (string.IsNullOrEmpty(order.TransactionNumber))
            {
                LogError(order, PreparedMessages.TransactionNumberNotSetMessage);
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, PreparedMessages.TransactionNumberRequiredMessage, 0, order);
                return;
            }

            string errorText = OrderHelper.GetOrderError(order);
            if (!string.IsNullOrEmpty(errorText))
            {
                LogError(order, errorText);
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorText, 0, order);
                return;
            }

            double refundAmount = order.TransactionAmount;
            TransactionRequestType transactionRequest = CreateRefundRequest(order, refundAmount);
            CreateTransactionResponse? response = service.CreateTransaction(transactionRequest);

            if (IsResponseSuccessful(response, order))
            {
                LogEvent(order, $"Refund successful for amount: {refundAmount:C}");
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.FullyReturned, PreparedMessages.RefundSuccessMessage, refundAmount, order);
            }
            else
            {
                string infoText = response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
                                 ?? response?.Messages?.Message?.FirstOrDefault()?.Text
                                 ?? "Refund failed";

                LogError(order, infoText);
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, infoText, refundAmount, order);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"{PreparedMessages.UnexpectedErrorMessage}: {ex.Message}";
            LogError(order, ex, errorMessage);
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorMessage, 0, order);
        }
    }

    /// <summary>
    /// Creates a refund request
    /// </summary>
    private TransactionRequestType CreateRefundRequest(Order order, double refundAmount)
    {
        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.RefundTransaction,
            Amount = refundAmount,
            CurrencyCode = order.CurrencyCode,
            RefTransId = order.TransactionNumber,
            Order = new Models.OrderType
            {
                InvoiceNumber = order.Id
            }
        };

        // Add payment card information if available
        if (!string.IsNullOrEmpty(order.TransactionCardNumber) && order.TransactionCardNumber.Length >= 4)
        {
            transactionRequest.Payment = new PaymentType
            {
                CreditCard = new CreditCardType
                {
                    CardNumber = order.TransactionCardNumber,
                    ExpirationDate = SecuritySettings.CreditCardExpirationMask
                }
            };
        }

        return transactionRequest;
    }



    /// <summary>
    /// Deletes a saved card from the customer profile in Authorize.Net
    /// </summary>
    /// <param name="savedCardId">ID of the saved card to delete</param>
    public void DeleteSavedCard(int savedCardId)
    {
        Log($"Attempting to delete saved card ID: {savedCardId}");

        PaymentCardToken? savedCard = Ecommerce.Services.PaymentCard.GetById(savedCardId);
        if (savedCard is null)
        {
            Log($"Saved card not found with ID: {savedCardId}");
            return;
        }

        (string? cardToken, _, _) = ExtractCofData(savedCard.Token);
        int userId = savedCard.UserID;

        if (userId <= 0)
        {
            Log($"Invalid user ID for saved card: {userId}");
            return;
        }

        if (string.IsNullOrEmpty(cardToken))
        {
            Log($"Invalid card token for saved card ID: {savedCardId}");
            return;
        }

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService();

            CustomerProfileMaskedType? profile = GetCustomerProfile(service, userId, false);
            if (string.IsNullOrEmpty(profile?.CustomerProfileId))
            {
                Log($"Customer profile not found for user ID: {userId}");
                return;
            }

            var response = service.DeleteCustomerPaymentProfile(profile.CustomerProfileId, cardToken);
            if (response is not null && Enum.TryParse(response.Messages?.ResultCode, true, out MessageTypeEnum resultCode) &&
                resultCode == MessageTypeEnum.Ok)
            {
                Log($"Successfully deleted payment profile {cardToken} for customer {profile.CustomerProfileId}");
            }
            else
            {
                Log($"Failed to delete payment profile. Response: {response?.Messages?.Message?.FirstOrDefault()?.Text}");
            }
        }
        catch (Exception ex)
        {
            Log($"Exception occurred while deleting saved card: {ex.Message}");
        }
    }

    /// <summary>
    /// Uses a saved card to pay for an order
    /// </summary>
    /// <param name="order">Order to pay for</param>
    public string UseSavedCard(Order order)
    {
        try
        {
            if (UseSavedCardInternal(order) is RedirectOutputResult redirectResult)
            {
                RedirectToCart(redirectResult);
                return string.Empty;
            }

            return string.Empty;
        }
        catch (ThreadAbortException)
        {
            return string.Empty;
        }
        catch (Exception ex)
        {
            LogError(order, ex, $"Exception occurred while processing saved card: {order.SavedCardId}");
            OutputResult errorResult = OnError(order, ex.Message, ex);

            if (errorResult is ContentOutputResult contentErrorResult)
                return contentErrorResult.Content;

            if (errorResult is RedirectOutputResult redirectErrorResult)
                RedirectToCart(redirectErrorResult);

            return string.Empty;
        }
    }

    private OutputResult UseSavedCardInternal(Order order)
    {
        LogEvent(order, $"Attempting to use saved card ID: {order.SavedCardId}");

        if (order.SavedCardId <= 0)
            throw new ArgumentException($"Invalid saved card ID: {order.SavedCardId}");

        PaymentCardToken? savedCard = Ecommerce.Services.PaymentCard.GetById(order.SavedCardId);
        if (savedCard is null)
            throw new ArgumentException($"Saved card not found with ID: {order.SavedCardId}");

        // Security check - card must belong to the current user
        if (order.CustomerAccessUserId != savedCard.UserID)
            throw new UnauthorizedAccessException($"Security violation: Card {order.SavedCardId} does not belong to user {order.CustomerAccessUserId}");

        if (string.IsNullOrEmpty(savedCard.Token))
            throw new ArgumentException($"Invalid token for saved card ID: {order.SavedCardId}");

        AuthorizeNetService service = GetAuthorizeNetService(order);
        CustomerProfileMaskedType? profile = GetCustomerProfile(service, order.CustomerAccessUserId, false);
        if (string.IsNullOrEmpty(profile?.CustomerProfileId))
            throw new InvalidOperationException($"Customer profile not found for user: {order.CustomerAccessUserId}");

        (string? cardToken, string? networkTransId, double? originalAmount) = ExtractCofData(savedCard.Token);
        if (string.IsNullOrEmpty(cardToken))
            throw new ArgumentException($"Invalid card token extracted from saved card: {order.SavedCardId}");

        var profileToCharge = new CustomerProfilePaymentType
        {
            CustomerProfileId = profile.CustomerProfileId,
            PaymentProfile = new PaymentProfile
            {
                PaymentProfileId = cardToken
            }
        };

        LogEvent(order, $"Processing payment with saved card profile: {profile.CustomerProfileId}, payment profile: {cardToken}");

        OutputResult result = CreatePaymentTransaction(order, profileToCharge, null);

        if (result is RedirectOutputResult)
        {
            // If CreatePaymentTransaction returned RedirectOutputResult, this means success
            LogEvent(order, "Successfully processed payment with saved card");
            return result;
        }

        // If we got ContentOutputResult, this means error
        return result;
    }

    /// <summary>
    /// A temporary method to maintain previous behavior. Redirects to cart by Response.Redirect.
    /// Please remove it when the needed changes will be done.
    /// </summary>
    private void RedirectToCart(RedirectOutputResult redirectResult)
    {
        Context.Current?.Response?.Redirect(redirectResult.RedirectUrl);
    }

    /// <summary>
    /// Helper method for logging without Order object
    /// </summary>
    private void Log(string message)
    {
        Ecommerce.Services.OrderDebuggingInfos.Save(null, message, "Authorize.Net checkout handler", DebuggingInfoType.Undefined);
    }

    /// <summary>
    /// Enhanced error logging method with formatting support
    /// </summary>
    private void LogError(Order? order, Exception exception, string message)
    {
        Ecommerce.Services.OrderDebuggingInfos.Save(order, $"{message}. Exception: {exception}", "Authorize.Net checkout handler", DebuggingInfoType.Undefined);
    }

    /// <summary>
    /// Checks if saved cards are supported for the given order
    /// </summary>
    /// <param name="order">Order to check</param>
    public bool SavedCardSupported(Order order) => AllowSaveCards;

    private bool NeedSaveCard(Order order, out string cardName)
    {
        cardName = string.Empty;
        if (AllowSaveCards && order.CustomerAccessUserId > 0 && (order.DoSaveCardToken || !string.IsNullOrEmpty(order.SavedCardDraftName)))
        {
            cardName = !string.IsNullOrEmpty(order.SavedCardDraftName)
                ? order.SavedCardDraftName
                : order.Id;

            return true;
        }

        return false;
    }

    private void SaveCard(Order order, string cardName, string? networkTransId, double? originalAmount)
    {
        if (!AllowSaveCards || order.CustomerAccessUserId <= 0)
            return;

        AuthorizeNetService service = GetAuthorizeNetService(order);
        CustomerProfileMaskedType? profile = GetCustomerProfile(service, order.CustomerAccessUserId, true);
        if (profile is null)
            return;

        string? cardToken = service.CreatePaymentProfileFromTransaction(order.TransactionNumber, profile.CustomerProfileId);
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

    private CustomerProfileMaskedType? GetCustomerProfile(AuthorizeNetService service, int userId, bool tryCreate) => service.GetCustomerProfile(userId, tryCreate);

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
        LogEvent(order, "Printing error template");

        if (exception is not null)
            LogError(order, exception, message);
        else
            LogError(order, message);

        order.TransactionAmount = 0;
        order.TransactionStatus = "Failed";
        order.Errors.Add(message);
        Save(order);

        Ecommerce.Services.Orders.DowngradeToCart(order);
        order.TransactionStatus = "";
        Common.Context.SetCart(order);

        if (string.IsNullOrWhiteSpace(ErrorTemplate))
            return PassToCart(order);

        var errorTemplate = new Template(TemplateHelper.GetTemplatePath(ErrorTemplate, TemplateFolders.ErrorTemplateFolder));
        errorTemplate.SetTag("CheckoutHandler:ErrorMessage", message);

        return new ContentOutputResult
        {
            Content = Render(order, errorTemplate)
        };
    }

    private void Save(Order order) => Ecommerce.Services.Orders.Save(order);

    private void SetFirstTransactionFlags(TransactionRequestType request, bool isCardOnFile)
    {
        if (!isCardOnFile)
            return;

        if (request.ProcessingOptions is null)
            request.ProcessingOptions = new ProcessingOptionsType();

        request.ProcessingOptions.IsStoredCredentials = true;
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

    /// <summary>
    /// Checks if response from Authorize.Net API is successful
    /// </summary>
    private bool IsResponseSuccessful(CreateTransactionResponse? response, Order? order)
    {
        if (response?.TransactionResponse is null)
        {
            LogError(order, "No transaction response received from Authorize.Net");
            return false;
        }

        bool isSuccess = Converter.ToInt32(response.TransactionResponse.ResponseCode) == ResponseCodes.Approved;

        if (!isSuccess)
        {
            string errorMessage = response.TransactionResponse.Errors?.FirstOrDefault()?.ErrorText
                ?? response.Messages?.Message?.FirstOrDefault()?.Text
                ?? "Transaction failed";

            LogError(order, "Transaction failed: {0} (Response Code: {1})",
                errorMessage, response.TransactionResponse.ResponseCode);
        }

        return isSuccess;
    }

    /// <summary>
    /// Checks if response from Authorize.Net API is successful (for GetHostedPaymentPage)
    /// </summary>
    private bool IsResponseSuccessful([NotNullWhen(true)] GetHostedPaymentPageResponse? response, Order? order)
    {
        if (response is null)
        {
            LogError(order, "No hosted payment response received from Authorize.Net");
            return false;
        }

        var isSuccess = Enum.TryParse(response.Messages?.ResultCode, true, out MessageTypeEnum resultCode)
                       && resultCode is MessageTypeEnum.Ok;

        if (!isSuccess)
        {
            var errorMessage = response.Messages?.Message?.FirstOrDefault()?.Text ?? "Hosted payment page creation failed";
            LogError(order, "Hosted payment page creation failed: {0}", errorMessage);
        }

        return isSuccess;
    }

    /// <summary>
    /// Checks if capture is supported for the given order
    /// </summary>
    public bool CaptureSupported(Order order) => true;


    /// <summary>
    /// Checks if split capture is supported for the given order
    /// </summary>
    public bool SplitCaptureSupported(Order order) => true;

}
