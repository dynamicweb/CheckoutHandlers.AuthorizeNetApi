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
public class AuthorizeNetCheckoutHandler : CheckoutHandler, ICancelOrder, IFullReturn, IPartialReturn, IRemoteCapture, IRemotePartialCapture, ISavedCard, IParameterOptions, ICheckoutHandlerCallback
{
    private TransactionType _transactionType = TransactionType.AuthCaptureTransaction;

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

    [AddInLabel("Enable debug logging"), AddInParameter("DebugLogging"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool DebugLogging
    {
        get => _debugLogging;
        set => _debugLogging = value;
    }

    [AddInLabel("Enable auto webhook registration"), AddInParameter("EnableAutoWebhookRegistration"), AddInParameterGroup("Webhook settings"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool EnableAutoWebhookRegistration { get; set; }

    [AddInLabel("Force webhook re-registration"), AddInParameter("ForceWebhookRegistration"), AddInParameterGroup("Webhook settings"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool ForceWebhookRegistration { get; set; }

    #endregion

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

            // Auto-register webhooks if needed
            if (ShouldRegisterWebhooks(order))
            {
                try
                {
                    RegisterWebhooksAutomatically(order);
                }
                catch (Exception exception)
                {
                    LogError(order, exception, "Webhook auto-registration failed, continuing with payment: {0}", exception.Message);
                }
            }

            return RedirectToHostedForm(order, service);
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
                LogEvent(order, "GatewayResult found, but action is empty.");
                return ContentOutputResult.Empty;
            }

            return action switch
            {
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
        double orderAmount = OrderHelper.GetOrderAmount(order);

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

        TransactionRequestType transactionRequest = CreateHostedFormRequest(order, orderAmount);
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
        double orderAmount = OrderHelper.GetOrderAmount(order);

        TransactionRequestType transactionRequest = profileToCharge is not null
            ? CreateSavedCardPaymentRequest(order, orderAmount, profileToCharge)
            : CreateDirectPaymentRequest(order, orderAmount);
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
                SaveCard(order, cardName);
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

    /// <summary>
    /// Creates a transaction request for hosted form payments (no direct payment data)
    /// </summary>
    /// <param name="order">Order to create the request for</param>
    /// <param name="orderAmount">Amount of the order</param>
    private TransactionRequestType CreateHostedFormRequest(Order order, double orderAmount)
    {
        var request = CreateBaseTransactionRequest(order, orderAmount);

        // Include customer data for hosted form
        AddCustomerData(request, order);

        // Set stored credentials flag if card needs to be saved
        if (NeedSaveCard(order, out _))
        {
            request.ProcessingOptions = new ProcessingOptionsType
            {
                IsStoredCredentials = true
            };
        }

        return request;
    }

    /// <summary>
    /// Creates a transaction request for direct payments with new card data
    /// </summary>
    /// <param name="order">Order to create the request for</param>
    /// <param name="orderAmount">Amount of the order</param>
    /// <param name="payment">Payment information</param>
    private TransactionRequestType CreateDirectPaymentRequest(Order order, double orderAmount)
    {
        var request = CreateBaseTransactionRequest(order, orderAmount);

        // Set stored credentials flag if card needs to be saved
        if (NeedSaveCard(order, out _))
        {
            request.ProcessingOptions = new ProcessingOptionsType
            {
                IsStoredCredentials = true
            };
        }

        return request;
    }

    /// <summary>
    /// Creates a transaction request for payments with saved card (Customer Profile)
    /// </summary>
    /// <param name="order">Order to create the request for</param>
    /// <param name="orderAmount">Amount of the order</param>
    /// <param name="profileToCharge">Customer profile to charge</param>
    private TransactionRequestType CreateSavedCardPaymentRequest(Order order, double orderAmount, CustomerProfilePaymentType profileToCharge)
    {
        var request = CreateBaseTransactionRequest(order, orderAmount);
        request.Profile = profileToCharge;

        // For Customer Profiles, Authorize.Net automatically handles COF indicators
        // No need for additional processing options or subsequent auth information

        return request;
    }

    /// <summary>
    /// Creates the base transaction request with common fields
    /// </summary>
    /// <param name="order">Order to create the request for</param>
    /// <param name="orderAmount">Amount of the order</param>
    private TransactionRequestType CreateBaseTransactionRequest(Order order, double orderAmount)
    {
        return new TransactionRequestType
        {
            Amount = orderAmount,
            CurrencyCode = order.CurrencyCode,
            LineItems = AuthorizeNetModelFactory.CreateLineItems(order),
            Order = new Models.OrderType
            {
                InvoiceNumber = order.Id
            },
            CustomerIp = StringHelper.Crop(order.Ip, 15),
            TransactionType = _transactionType is TransactionType.AuthCaptureTransaction
                ? TransactionTypeEnum.AuthCaptureTransaction
                : TransactionTypeEnum.AuthOnlyTransaction
        };
    }

    /// <summary>
    /// Adds customer billing and shipping data to the request
    /// </summary>
    /// <param name="request">Transaction request to add customer data to</param>
    /// <param name="order">Order containing customer data</param>
    private void AddCustomerData(TransactionRequestType request, Order order)
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
                string cardName = order.GatewayPaymentStatus.Substring(SecuritySettings.SavedCardNamePlaceholder.Length);
                order.GatewayPaymentStatus = string.Empty;
                SaveCard(order, cardName);
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
    /// <param name="order">Order to void</param>
    /// <param name="service">Authorize.Net service instance</param>
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

            double orderAmount = OrderHelper.GetOrderAmount(order);

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

    private (bool isValid, string? errorMessage) ValidatePartialCapture(Order order, long amount, bool final)
    {
        if (amount <= 0)
            return (false, "Capture amount must be greater than zero");

        // Check that we don't exceed original authorized amount
        double totalRequestedAmount = order.CaptureAmount + (amount / 100.0);
        double authorizedAmount = OrderHelper.GetOrderAmount(order);

        if (totalRequestedAmount > authorizedAmount)
            return (false, $"Total capture amount ({totalRequestedAmount:C}) cannot exceed authorized amount ({authorizedAmount:C})");

        return (true, null);
    }

    /// <summary>
    /// Performs a full refund for the order
    /// </summary>
    /// <param name="order">Order to refund</param>
    public void FullReturn(Order order)    
      => ProceedReturn(order, null);

    /// <summary>
    /// Performs a partial refund for the order
    /// </summary>
    /// <param name="order">Order to refund</param>
    /// <param name="originalOrder">Original order for reference</param>
    public void PartialReturn(Order order, Order originalOrder) =>
        ProceedReturn(originalOrder, order?.Price?.PricePIP);
    
    private void ProceedReturn(Order order, long? amount)
    {
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

        double operationAmount = amount is null 
            ? order.CaptureAmount 
            : Converter.ToDouble(amount) / 100;

        if (order.CaptureInfo is null || order.CaptureInfo.State is not OrderCaptureInfo.OrderCaptureState.Success || order.CaptureAmount <= 0.00)
        {
            string errorMessage = "Order must be captured before return.";
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorMessage, order.CaptureAmount, order);
            LogError(null, errorMessage);
            return;
        }

        if (amount is not null && order.CaptureAmount < operationAmount)
        {
            string formattedAmount = Ecommerce.Services.Currencies.Format(order.Currency, order.CaptureAmount);
            string formattedRequestedAmount = Ecommerce.Services.Currencies.Format(order.Currency, operationAmount);

            OrderReturnInfo.SaveReturnOperation
            (
                OrderReturnOperationState.Failed,
                $"Order captured amount ({formattedAmount}) less than amount requested for return({formattedRequestedAmount}).",
                operationAmount,
                order
            );
            LogError(order, "Order captured amount less then amount requested for return.");
            return;
        }

        if (string.IsNullOrWhiteSpace(order.TransactionCardNumber) || order.TransactionCardNumber.Length < 4)
        {
            string errorMessage = "Invalid card number for refund. Last 4 digits of the card number are required.";
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorMessage, operationAmount, order);
            LogError(order, errorMessage);
            return;
        }

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
          
            TransactionRequestType transactionRequest = CreateRefundRequest(order, operationAmount);
            CreateTransactionResponse? response = service.CreateTransaction(transactionRequest);

            if (IsResponseSuccessful(response, order))
            {
                LogEvent(order, $"Refund successful for amount: {operationAmount:C}");
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.FullyReturned, PreparedMessages.RefundSuccessMessage, operationAmount, order);
            }
            else
            {
                string infoText = response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
                                 ?? response?.Messages?.Message?.FirstOrDefault()?.Text
                                 ?? "Refund failed";

                LogError(order, infoText);
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, infoText, operationAmount, order);
            }
           
            var operationState = amount is null
                ? OrderReturnOperationState.FullyReturned
                : OrderReturnOperationState.PartiallyReturned;
            OrderReturnInfo.SaveReturnOperation(operationState, "Authorize.Net has refunded payment.", operationAmount, order);
            
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
    /// <param name="order">Order to refund</param>
    /// <param name="refundAmount">Amount to refund</param>
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
                      
        transactionRequest.Payment = new PaymentType
        {
            CreditCard = new CreditCardType
            {
                CardNumber = order.TransactionCardNumber[^4..],
                ExpirationDate = SecuritySettings.CreditCardExpirationMask
            }
        };        

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

        string? cardToken = savedCard.Token;
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

            DeleteCustomerPaymentProfileResponse? response = service.DeleteCustomerPaymentProfile(profile.CustomerProfileId, cardToken);
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

        string? cardToken = savedCard.Token;
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
    /// Please remove this code after making the necessary changes, when we have a new redirect method (which returns RedirectOutputResult instead of a string).
    /// </summary>
    private void RedirectToCart(RedirectOutputResult redirectResult)
    {
        Context.Current?.Response?.Redirect(redirectResult.RedirectUrl);
    }

    private void Log(string message) =>
        Ecommerce.Services.OrderDebuggingInfos.Save(null, message, "Authorize.Net checkout handler", DebuggingInfoType.Undefined);

    private void LogError(Order? order, Exception exception, string message) =>
        Ecommerce.Services.OrderDebuggingInfos.Save(order, $"{message}. Exception: {exception}", "Authorize.Net checkout handler", DebuggingInfoType.Undefined);

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

    private void SaveCard(Order order, string cardName)
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

        PaymentCardToken? existingCard = Ecommerce.Services.PaymentCard.GetByUserId(order.CustomerAccessUserId)
            .FirstOrDefault(token => string.Equals(token.Token, cardToken, StringComparison.Ordinal));

        if (existingCard is null)
        {
            existingCard = Ecommerce.Services.PaymentCard.CreatePaymentCard(
                order.CustomerAccessUserId, order.PaymentMethodId, cardName,
                order.TransactionCardType, order.TransactionCardNumber, cardToken
            );
        }

        order.SavedCardId = existingCard.ID;
        Save(order);
        LogEvent(order, "Saved card: {0}", existingCard.Name);
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

    /// <summary>
    /// Checks if response from Authorize.Net API is successful
    /// </summary>
    /// <param name="response">Response from Authorize.Net API</param>
    /// <param name="order">Order for logging context</param>
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
    /// <param name="response">Response from Authorize.Net API</param>
    /// <param name="order">Order for logging context</param>
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
    /// <param name="order">Order to check</param>
    public bool CaptureSupported(Order order) => true;

    /// <summary>
    /// Checks if split capture is supported for the given order
    /// </summary>
    /// <param name="order">Order to check</param>
    public bool SplitCaptureSupported(Order order) => true;

    #region Webhook Management

    /// <summary>
    /// Determines if webhook registration should be performed
    /// </summary>
    /// <param name="order">Order to check</param>
    private bool ShouldRegisterWebhooks(Order order)
    {
        // Force re-registration always takes priority
        if (ForceWebhookRegistration)
            return true;

        // Auto registration only if enabled AND no webhooks exist yet
        if (EnableAutoWebhookRegistration)
        {
            try
            {
                string webhookUrl = BuildWebhookUrl(order);
                AuthorizeNetService service = GetAuthorizeNetService();

                // Check if webhooks already exist for this URL
                WebhookListResponse existingWebhooks = service.GetWebhooks();
                List<WebhookResponse> ourWebhooks = existingWebhooks.Webhooks
                    .Where(w => webhookUrl.Equals(w.Url, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Only register if no webhooks exist for our URL
                return ourWebhooks.Count == 0;
            }
            catch (Exception ex)
            {
                // If we can't check existing webhooks, skip auto-registration to be safe
                var logger = new AuthorizeNetLogger(DebugLogging, order);
                logger.LogError(ex, "Failed to check existing webhooks, skipping auto-registration: {0}", ex.Message);
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Automatically registers webhooks if needed and updates parameters
    /// </summary>
    /// <param name="order">Order for logging context</param>
    private void RegisterWebhooksAutomatically(Order order)
    {
        string operation = ForceWebhookRegistration ? "Force re-registering" : "Auto-registering";
        LogEvent(order, "{0} webhooks", operation);

        try
        {
            string webhookUrl = BuildWebhookUrl(order);
            var service = GetAuthorizeNetService(order);

            var response = service.EnsureWebhooksRegistered(webhookUrl, ForceWebhookRegistration);

            if (response is not null && !string.IsNullOrEmpty(response.WebhookId))
            {
                var payment = Ecommerce.Services.Payments.GetPayment(order.PaymentMethodId);
                if (payment is not null)
                {
                    // Reset force re-registration flag after successful registration
                    if (ForceWebhookRegistration)
                        ForceWebhookRegistration = false;

                    // Save updated parameters back to payment method
                    payment.CheckoutParameters = GetParametersToXml();
                    Ecommerce.Services.Payments.Save(payment);

                    LogEvent(order, "Webhooks {0} successfully. Webhook ID: {1}", operation, response.WebhookId);
                }
                else
                {
                    LogError(order, "Failed to retrieve payment method to save webhook parameters");
                }
            }
            else
            {
                LogEvent(order, "No webhook changes were needed");
            }
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Webhook {0} failed: {1}", operation, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Builds webhook callback URL for webhook registration using DW10 centralized ExternalCallback architecture
    /// </summary>
    private string BuildWebhookUrl(Order order)
    {
        /*

        // Get base URL (this may include Default.aspx and query parameters)
        string baseUrl = GetBaseUrl(order);

        // Extract protocol, domain, and port from the base URL
        var uri = new Uri(baseUrl);
        string baseUrlClean = $"{uri.Scheme}://{uri.Authority}";

        // Build DW10 ExternalCallback URL
        // Format: /dwapi/ecommerce/carts/callback/{checkoutHandlerName}
        string handlerName = AddInManager.GetAddInName(GetType()); // Must match AddInName exactly
        string webhookUrl = $"{baseUrlClean}/dwapi/ecommerce/carts/callback/{Uri.EscapeDataString(handlerName)}";

        return webhookUrl; 
        */

        //Returns the test webhook URL for testing purposes. Please replace it with the actual URL when using the webhook functionality.

        return "https://webhook.site/4a746d3d-4deb-4412-9736-f1dafb2f1f87";
    }

    /// <summary>
    /// Gets AuthorizeNetService instance
    /// </summary>
    private AuthorizeNetService GetAuthorizeNetService() =>
        new AuthorizeNetService(ApiLoginId, TransactionKey, TestMode, DebugLogging, null, null);

    /// <summary>
    /// Gets AuthorizeNetService instance with Order context for enhanced logging
    /// </summary>
    /// <param name="order">Order for logging context</param>
    private AuthorizeNetService GetAuthorizeNetService(Order order)
    {
        var logger = new AuthorizeNetLogger(DebugLogging, order);

        return new AuthorizeNetService(ApiLoginId, TransactionKey, TestMode, DebugLogging, logger, order);
    }

    #endregion

    #region ICheckoutHandlerCallback Implementation

    /// <summary>
    /// Extracts order from webhook callback data
    /// <param name="data">The callback data containing the webhook payload</param>
    /// </summary>
    public static Order? GetOrderFromCallback(CallbackData data)
    {
        var logger = new AuthorizeNetLogger(true);

        try
        {
            // Parse the webhook payload from the body
            var webhookPayload = Converter.Deserialize<NotificationItem>(data.Body);
            if (webhookPayload?.Payload is null)
            {
                logger.LogError("Invalid webhook payload: missing notification data");
                return null;
            }

            string orderId = webhookPayload.Payload.OrderId;
            if (string.IsNullOrEmpty(orderId))
            {
                logger.LogError("Invalid webhook payload: missing order ID");
                return null;
            }

            var order = Ecommerce.Services.Orders.GetById(orderId);
            if (order is null)
            {
                logger.LogError("Order not found: {0}", orderId);
                return null;
            }

            // Store the webhook payload in GatewayResult for processing
            order.GatewayResult = data.Body;

            return order;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract order from callback data: {0}", ex.Message);

            return null;
        }
    }

    /// <summary>
    /// Handles webhook callback processing
    /// <param name="order">The order associated with the webhook callback</param>
    /// <param name="data">The callback data containing the webhook payload</param>
    /// </summary>
    public OutputResult HandleCallback(Order order, CallbackData data)
    {
        try
        {
            if (order is null)
            {
                LogError(order, "Webhook processing skipped. Order is not set");
                return ContentOutputResult.Empty;
            }

            LogEvent(order, "Webhook callback processing started via DW10 architecture");

            // Validate HMAC signature
            if (string.IsNullOrEmpty(SignatureKey))
            {
                LogError(order, "Cannot handle webhook: Signature key not configured");
                return ContentOutputResult.Empty;
            }

            string? hmacSignature = data.Headers
                .FirstOrDefault(h => h.Key.Equals("X-ANET-Signature", StringComparison.OrdinalIgnoreCase))
                .Value?.FirstOrDefault()?.Replace("sha512=", string.Empty);

            if (!HmacValidator.IsValid(SignatureKey.Trim(), data.Body, hmacSignature))
            {
                string logSignature = string.IsNullOrEmpty(hmacSignature) ? "null" : hmacSignature.Substring(0, Math.Min(8, hmacSignature.Length)) + "...";
                LogError(order, "Webhook HMAC validation failed. Signature: {0}", logSignature);
                return ContentOutputResult.Empty;
            }

            // Clear GatewayResult to prevent reprocessing
            order.GatewayResult = string.Empty;
            Ecommerce.Services.Orders.UpdateGatewayResult(order, false);

            // Parse the webhook payload
            var webhookPayload = Converter.Deserialize<NotificationItem>(data.Body);
            if (webhookPayload?.Payload is null)
            {
                LogError(order, "Invalid webhook payload: missing notification data");
                return ContentOutputResult.Empty;
            }

            var payload = webhookPayload.Payload;
            var eventType = webhookPayload.GetEventType();

            LogEvent(order, "Processing webhook event: {0} for transaction: {1}", eventType, payload.Id);

            // Process the webhook based on event type
            switch (eventType)
            {
                case NotificationEventType.AuthCaptureCreated:
                case NotificationEventType.AuthCreated:
                    HandleAuthOrCaptureCallback(order, payload, eventType == NotificationEventType.AuthCaptureCreated);
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

                default:
                    LogEvent(order, "Unhandled webhook event type: {0}", eventType);
                    break;
            }

            LogEvent(order, "Webhook callback processing completed successfully");
            return ContentOutputResult.Empty;
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Webhook callback processing failed: {0}", ex.Message);
            return ContentOutputResult.Empty;
        }
    }

    #endregion

}
