using Dynamicweb.Core;
using Dynamicweb.Ecommerce.Cart;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Orders.Gateways;
using Dynamicweb.Ecommerce.Prices;
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
using static Dynamicweb.Ecommerce.Orders.OrderCaptureInfo;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi;

/// <summary>
/// AuthorizeNet API Checkout Handler
/// </summary>
[AddInName("Authorize.Net API"), AddInDescription("AuthorizeNet API Checkout Handler"), AddInUseParameterGrouping(true)]
public class AuthorizeNetCheckoutHandler : CheckoutHandler, ICancelOrder, IFullReturn, IPartialReturn, IRemoteCapture, ISavedCard, IParameterOptions, ICheckoutHandlerCallback
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
            using AuthorizeNetService service = GetAuthorizeNetService(order);

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
                "Receipt" => OrderCompleted(order, OrderHelper.GetOrderAmount(order), null),
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

        GetHostedPaymentPageResponse? response = service.GetHostedPaymentPage
        (
            order, orderAmount, _transactionType,
            NeedSaveCard(order, out _),
            settings
        );

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
        using AuthorizeNetService? ownedService = service is null
            ? GetAuthorizeNetService(order)
            : null;

        service ??= ownedService!;
        LogEvent(order, "Create payment transaction");
        double orderAmount = OrderHelper.GetOrderAmount(order);

        TransactionRequestType transactionRequest = profileToCharge is not null
            ? service.CreateSavedCardPaymentRequest(order, orderAmount, _transactionType, profileToCharge)
            : service.CreateDirectPaymentRequest(order, orderAmount, _transactionType, NeedSaveCard(order, out _));

        CreateTransactionResponse? response = service.CreateTransaction(transactionRequest);

        if (IsResponseSuccessful(response, order))
            return OrderCompleted(order, orderAmount, response);

        return OrderRefused(order, response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText);
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
            order.CaptureInfo = new OrderCaptureInfo(OrderCaptureState.Success, "Capture successful");
            order.CaptureAmount = transactionAmount;
        }

        if (!order.Complete)
        {
            SetOrderComplete(order, order.TransactionNumber);
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
    /// Synchronizes order state with actual Authorize.Net API data for capture operations
    /// Webhooks are triggers - we track DELTA (changes) between API and current Order state
    /// </summary>
    /// <param name="order">Order to synchronize</param>
    /// <param name="transactionId">Transaction ID from webhook</param>
    private void SynchronizeOrderWithCapture(Order order, string transactionId)
    {
        try
        {
            using AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType? transactionDetails = service.GetTransactionDetails(transactionId);

            if (transactionDetails is null)
            {
                LogError(order, "Failed to retrieve transaction details for ID: {0}", transactionId);
                return;
            }

            LogEvent(order, "Synchronizing order with API data for capture transaction: {0}", transactionId);
            UpdateBasicTransactionInfo(order, transactionDetails);

            // Update card information if missing
            UpdateCardInformation(order, transactionDetails);

            // Handle capture delta using API data
            if (transactionDetails.SettleAmount > 0)
            {
                double apiCaptureAmount = Convert.ToDouble(transactionDetails.SettleAmount);
                HandleCaptureDelta(order, apiCaptureAmount, transactionDetails.TransId);
            }

            Save(order);
            LogEvent(order, "Capture synchronization completed");
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Failed to synchronize capture with API data: {0}", ex.Message);
        }
    }

    /// <summary>
    /// Synchronizes order state with actual Authorize.Net API data for refund operations.
    /// The transactionId here is the REFUND transaction's ID (not the original).
    /// In Authorize.Net, each refund is a separate transaction - its SettleAmount is the
    /// amount of this individual refund, not a cumulative total.
    /// </summary>
    /// <param name="order">Order to synchronize</param>
    /// <param name="transactionId">Refund transaction ID from webhook</param>
    private void SynchronizeOrderWithRefund(Order order, string transactionId)
    {
        try
        {
            using AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType? transactionDetails = service.GetTransactionDetails(transactionId);

            if (transactionDetails is null)
            {
                LogError(order, "Failed to retrieve refund transaction details for ID: {0}", transactionId);
                return;
            }

            LogEvent(order, "Synchronizing order with API data for refund transaction: {0}", transactionId);

            // SettleAmount on a refund transaction = the amount of THIS refund operation
            if (transactionDetails.SettleAmount > 0)
            {
                double refundAmount = Convert.ToDouble(transactionDetails.SettleAmount);
                HandleRefundDelta(order, refundAmount, transactionDetails.TransId);
            }

            Save(order);
            LogEvent(order, "Refund synchronization completed");
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Failed to synchronize refund with API data: {0}", ex.Message);
        }
    }

    /// <summary>
    /// Synchronizes order state with actual Authorize.Net API data for void operations
    /// Webhooks are triggers - we track DELTA (changes) between API and current Order state
    /// </summary>
    /// <param name="order">Order to synchronize</param>
    /// <param name="transactionId">Transaction ID from webhook</param>
    private void SynchronizeOrderWithVoid(Order order, string transactionId)
    {
        try
        {
            using AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType? transactionDetails = service.GetTransactionDetails(transactionId);

            if (transactionDetails is null)
            {
                LogError(order, "Failed to retrieve transaction details for ID: {0}", transactionId);
                return;
            }

            LogEvent(order, "Synchronizing order with API data for void transaction: {0}", transactionId);
            UpdateBasicTransactionInfo(order, transactionDetails);
            HandleVoidDelta(order, transactionDetails.TransId);

            Save(order);
            LogEvent(order, "Void synchronization completed");
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Failed to synchronize void with API data: {0}", ex.Message);
        }
    }

    /// <summary>
    /// Updates basic transaction information from API data
    /// </summary>
    /// <param name="order">Order to update</param>
    /// <param name="transactionDetails">Transaction details from API</param>
    private void UpdateBasicTransactionInfo(Order order, TransactionDetailsType transactionDetails)
    {
        if (!string.IsNullOrEmpty(transactionDetails.TransId))
            OrderHelper.UpdateTransactionNumber(order, transactionDetails.TransId);

        if (transactionDetails.ResponseCode > 0)
            order.TransactionStatus = AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(transactionDetails.ResponseCode);
    }

    /// <summary>
    /// Updates card information from transaction details if missing in order
    /// </summary>
    /// <param name="order">Order to update card information for</param>
    /// <param name="transactionDetails">Transaction details containing card information</param>
    private void UpdateCardInformation(Order order, TransactionDetailsType transactionDetails)
    {
        if (string.IsNullOrEmpty(order.TransactionCardNumber) && transactionDetails.Payment?.CreditCard is not null)
        {
            order.TransactionCardType = transactionDetails.Payment.CreditCard.CardType?.ToString();
            order.TransactionCardNumber = transactionDetails.Payment.CreditCard.CardNumber;

            // Handle saved card logic
            if (order.GatewayPaymentStatus?.StartsWith(SecuritySettings.SavedCardNamePlaceholder) is true)
            {
                string cardName = order.GatewayPaymentStatus.Substring(SecuritySettings.SavedCardNamePlaceholder.Length);
                order.GatewayPaymentStatus = string.Empty;
                SaveCard(order, cardName);
            }
        }
    }

    /// <summary>
    /// Handles capture delta - only processes new capture amount
    /// </summary>
    /// <param name="order">Order to update</param>
    /// <param name="apiCaptureAmount">Capture amount from API</param>
    /// <param name="transactionId">Transaction ID associated with the capture</param>
    private void HandleCaptureDelta(Order order, double apiCaptureAmount, string? transactionId)
    {
        double currentCaptureAmount = order.CaptureAmount;
        double captureDelta = apiCaptureAmount - currentCaptureAmount;

        LogEvent(order, "Capture delta check: API={0:C}, Current={1:C}, Delta={2:C}",
                apiCaptureAmount, currentCaptureAmount, captureDelta, DebuggingInfoType.CaptureResult);

        if (captureDelta <= 0.01) // Allow for small rounding differences
        {
            LogEvent(order, "No new capture amount detected (Delta: {0:C})", captureDelta, DebuggingInfoType.CaptureResult);
            return;
        }

        // We have a new capture amount - update order
        double authAmount = OrderHelper.GetOrderAmount(order);
        double newTotalCaptured = apiCaptureAmount;

        // Determine if this results in partial or full capture
        bool isPartialCapture = Math.Abs(newTotalCaptured - authAmount) > 0.01;
        var captureState = isPartialCapture
            ? OrderCaptureState.Split
            : OrderCaptureState.Success;

        string message = isPartialCapture
            ? $"Additional capture: {captureDelta:C} (Total: {newTotalCaptured:C} of {authAmount:C})"
            : $"Final capture: {captureDelta:C} (Total: {newTotalCaptured:C})";

        order.CaptureInfo = new OrderCaptureInfo(captureState, message);
        order.CaptureAmount = newTotalCaptured;
        order.TransactionAmount = newTotalCaptured;
        order.TransactionStatus = captureState == OrderCaptureState.Success
            ? "Captured"
            : "Partially Captured";

        // Update any existing return operations state if needed
        UpdateReturnOperationStatesAfterCapture(order, newTotalCaptured);

        LogEvent(order, "Processed new capture: {0}. Amount changed from {1:C} to {2:C}", message, currentCaptureAmount, newTotalCaptured, DebuggingInfoType.CaptureResult);

        // Complete order if this is a full capture and order isn't complete yet
        if (!order.Complete && captureState == OrderCaptureState.Success)
        {
            SetOrderComplete(order, transactionId ?? order.TransactionNumber);
        }
    }

    /// <summary>
    /// Handles a refund operation from a webhook.
    /// In Authorize.Net, each refund is a SEPARATE transaction with its own transId.
    /// The refund transaction's SettleAmount is the amount of THIS SPECIFIC refund, not a cumulative total.
    /// There is no "total refunded" field on the original transaction in the API.
    /// </summary>
    /// <param name="order">Order to update</param>
    /// <param name="refundOperationAmount">The amount of this specific refund operation (from refund transaction's SettleAmount)</param>
    /// <param name="transactionId">The refund transaction ID (used for idempotency check)</param>
    private void HandleRefundDelta(Order order, double refundOperationAmount, string? transactionId)
    {
        double capturedAmount = order.CaptureAmount;

        if (capturedAmount <= 0)
        {
            LogEvent(order, "Skipping refund - no captured amount to refund", DebuggingInfoType.ReturnResult);
            return;
        }

        // Calculate previously refunded amount from existing return operations
        double previousRefundedAmount = 0.0;
        if (order.ReturnOperations?.Any() == true)
        {
            foreach (OrderReturnInfo refundOperation in order.ReturnOperations)
            {
                if (refundOperation.State is OrderReturnOperationState.PartiallyReturned ||
                    refundOperation.State is OrderReturnOperationState.FullyReturned)
                {
                    previousRefundedAmount += refundOperation.Amount;
                }
            }
        }

        LogEvent(order, "Refund check: This refund={0:C}, Previous refunded={1:C}, Captured={2:C}",
                refundOperationAmount, previousRefundedAmount, capturedAmount, DebuggingInfoType.ReturnResult);

        if (refundOperationAmount <= 0.01)
        {
            LogEvent(order, "Skipping refund - amount is zero or negative: {0:C}", refundOperationAmount, DebuggingInfoType.ReturnResult);
            return;
        }

        // Idempotency: check if this refund transaction was already recorded by ProceedReturn
        if (!string.IsNullOrEmpty(transactionId) && order.ReturnOperations?.Any() == true)
        {
            string marker = $"[RefundTransId:{transactionId}]";
            bool alreadyRecorded = order.ReturnOperations.Any(op =>
                op.Message?.Contains(marker, StringComparison.Ordinal) == true);

            if (alreadyRecorded)
            {
                LogEvent(order, "Skipping refund - transaction {0} already recorded by direct operation.",
                        transactionId, DebuggingInfoType.ReturnResult);
                return;
            }
        }

        // Total refunded = previous refunds + this new refund
        double totalRefundedAfterOperation = previousRefundedAmount + refundOperationAmount;

        // Safety net: if adding this refund would exceed captured amount, skip
        if (totalRefundedAfterOperation > capturedAmount + 0.01)
        {
            LogEvent(order, "Skipping refund - would exceed captured amount. Total after: {0:C}, Captured: {1:C}.",
                    totalRefundedAfterOperation, capturedAmount, DebuggingInfoType.ReturnResult);
            return;
        }
        bool isPartialRefund = Math.Abs(totalRefundedAfterOperation - capturedAmount) > 0.01
                               && totalRefundedAfterOperation < capturedAmount;
        OrderReturnOperationState refundState = isPartialRefund
            ? OrderReturnOperationState.PartiallyReturned
            : OrderReturnOperationState.FullyReturned;

        string markerSuffix = !string.IsNullOrEmpty(transactionId) ? $" [RefundTransId:{transactionId}]" : "";
        string message = isPartialRefund
            ? $"Partial refund: {refundOperationAmount:C} (Total refunded: {totalRefundedAfterOperation:C} of {capturedAmount:C}){markerSuffix}"
            : $"Full refund: {refundOperationAmount:C} (Total refunded: {totalRefundedAfterOperation:C}){markerSuffix}";

        OrderReturnInfo.SaveReturnOperation(refundState, message, refundOperationAmount, order);

        LogEvent(order, "Processed refund: {0}. Previous: {1:C}, New total: {2:C}, This operation: {3:C}",
                message, previousRefundedAmount, totalRefundedAfterOperation, refundOperationAmount, DebuggingInfoType.ReturnResult);
    }

    /// <summary>
    /// Handles void delta - only processes if not already voided
    /// </summary>
    private void HandleVoidDelta(Order order, string? transactionId)
    {
        if (order.CaptureInfo?.State is OrderCaptureState.Cancel)
        {
            LogEvent(order, "Transaction already voided - no delta to process");
            return;
        }

        order.CaptureInfo = new OrderCaptureInfo(OrderCaptureState.Cancel, "Transaction voided");
        LogEvent(order, "Processed void operation");
    }

    /// <summary>
    /// Updates return operation states after successful capture
    /// If capture amount increases after refunds were marked as "FullyReturned", 
    /// they should be changed to "PartiallyReturned"
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="totalCapturedAmount">New total captured amount</param>
    private void UpdateReturnOperationStatesAfterCapture(Order order, double totalCapturedAmount)
    {
        if (order.ReturnOperations?.Any() != true)
            return;

        List<OrderReturnInfo> fullyReturnedOperations = order.ReturnOperations
            .Where(operation => operation.State is OrderReturnOperationState.FullyReturned)
            .ToList();

        if (!fullyReturnedOperations.Any())
            return;

        double totalRefundedAmount = order.ReturnOperations
            .Where(operation =>
                operation.State is OrderReturnOperationState.PartiallyReturned ||
                operation.State is OrderReturnOperationState.FullyReturned)
            .Sum(operation => operation.Amount);

        if (totalCapturedAmount <= totalRefundedAmount)
            return;

        List<OrderReturnInfo> otherOperations = order.ReturnOperations
            .Where(operation => operation.State is not OrderReturnOperationState.FullyReturned)
            .ToList();

        foreach (OrderReturnInfo orderReturnInfo in fullyReturnedOperations)
            orderReturnInfo.State = OrderReturnOperationState.PartiallyReturned;

        order.ReturnOperations = otherOperations.Concat(fullyReturnedOperations).ToList();

        LogEvent(order, "Updated {0} return operations after new capture. Total captured: {1:C}, Total refunded: {2:C}",
            fullyReturnedOperations.Count, totalCapturedAmount, totalRefundedAmount, DebuggingInfoType.CaptureResult);
    }

    /// <summary>
    /// Handles authorization and capture callbacks from Authorize.Net webhooks
    /// Webhook payload is just a trigger - API data is the source of truth
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="payload">The notification payload</param>
    /// <param name="isCaptured">Indicates if the transaction is captured</param>
    private void HandleAuthOrCaptureCallback(Order order, NotificationPayload payload, bool isCaptured)
    {
        if (payload.ResponseCode != 1)
        {
            order.TransactionStatus = $"Authorisation failed: {AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode)}";
            Save(order);
            return;
        }

        if (isCaptured)
        {
            // This is auth_capture - process as capture
            SynchronizeOrderWithCapture(order, payload.Id);
        }
        else
        {
            // This is auth_only - just update basic info
            try
            {
                using AuthorizeNetService service = GetAuthorizeNetService(order);
                TransactionDetailsType? transactionDetails = service.GetTransactionDetails(payload.Id);

                if (transactionDetails is not null)
                {
                    UpdateBasicTransactionInfo(order, transactionDetails);
                    UpdateCardInformation(order, transactionDetails);

                    // For auth-only, just update transaction amount if needed
                    double authAmount = OrderHelper.GetOrderAmount(order);
                    if (Math.Abs(order.TransactionAmount - authAmount) > 0.01)
                    {
                        order.TransactionAmount = authAmount;
                        LogEvent(order, "Updated authorization amount: {0:C}", authAmount);
                    }

                    Save(order);
                }
            }
            catch (Exception ex)
            {
                LogError(order, ex, "Failed to process authorization callback: {0}", ex.Message);
            }
        }
    }

    /// <summary>
    /// Handles capture callbacks from Authorize.Net webhooks
    /// Uses API data synchronization approach
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="payload">The notification payload</param>
    private void HandleCaptureCallback(Order order, NotificationPayload payload)
    {
        if (payload.ResponseCode != 1)
        {
            order.CaptureInfo = new OrderCaptureInfo
            (
                OrderCaptureState.Failed,
                $"Capture failed: {AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode)}"
            );
            Save(order);

            return;
        }

        // Use API as source of truth, webhook is just a trigger
        SynchronizeOrderWithCapture(order, payload.Id);
    }

    /// <summary>
    /// Handles refund callbacks from Authorize.Net webhooks
    /// Uses API data synchronization approach
    /// </summary>
    /// <param name="order">Order to update</param>
    /// <param name="payload">The notification payload</param>
    private void HandleRefundCallback(Order order, NotificationPayload payload)
    {
        if (payload.ResponseCode != 1)
        {
            OrderReturnInfo.SaveReturnOperation
            (
                OrderReturnOperationState.Failed,
                $"Order refund failed: {AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode)}",
                0, order
            );

            return;
        }

        // Use API as source of truth, webhook is just a trigger
        SynchronizeOrderWithRefund(order, payload.Id);
    }

    /// <summary>
    /// Handles void callbacks from Authorize.Net webhooks
    /// Uses API data synchronization approach
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="payload">The notification payload</param>
    private void HandleVoidCallback(Order order, NotificationPayload payload)
    {
        if (payload.ResponseCode != 1)
        {
            order.CaptureInfo = new OrderCaptureInfo
            (
                OrderCaptureState.Cancel,
                $"Cancel order failed: {AuthorizeNetErrorMessageBuilder.GetResponseTextByCode(payload.ResponseCode)}"
            );
            Save(order);

            return;
        }

        // Use API as source of truth, webhook is just a trigger
        SynchronizeOrderWithVoid(order, payload.Id);
    }

    /// <summary>
    /// Cancels an order (performs Void transaction in Authorize.Net)
    /// Void is only possible on the day of the transaction before batch closing time
    /// </summary>
    /// <param name="order">Order to cancel</param>
    public bool CancelOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        try
        {
            using AuthorizeNetService service = GetAuthorizeNetService(order);
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

            LogEvent(order, "Void transaction failed");
            return false;
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
        CreateTransactionResponse? response = service.Void(order);

        if (IsResponseSuccessful(response, order))
            return true;

        string errorMessage = response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
                              ?? response?.Messages?.Message?.FirstOrDefault()?.Text
                              ?? "Cancel order: Void transaction failed";

        LogError(order, errorMessage);
        return false;
    }

    /// <summary>
    /// Captures the full order amount
    /// </summary>
    /// <param name="order">Order to capture</param>
    /// <returns>Capture operation result information</returns>
    public OrderCaptureInfo Capture(Order order)
    {
        LogEvent(order, "Full capture requested");
        long fullAmount = order.Price.PricePIP + Ecommerce.Prices.PriceHelper.ConvertToPIP(order.Currency, order.ExternalPaymentFee);

        return Capture(order, fullAmount, true);
    }

    /// <summary>
    /// Captures a specific amount from an authorized payment
    /// </summary>
    /// <param name="order">Order to capture</param>
    /// <param name="amount">Amount to capture in minor currency units</param>
    /// <param name="final">Whether this is the final capture (true) or allows additional captures (false)</param>
    /// <returns>Capture operation result information</returns>
    public OrderCaptureInfo Capture(Order order, long amount, bool final)
    {
        LogEvent(order, "Remote capture requested for amount: {0}", amount / 100d);

        try
        {
            if (order is null)
            {
                LogError(null, "Order not set");

                return new OrderCaptureInfo(OrderCaptureState.Failed, "Order not set");
            }

            if (string.IsNullOrEmpty(order.Id))
            {
                LogError(null, "Order id not set");

                return new OrderCaptureInfo(OrderCaptureState.Failed, "Order id not set");
            }

            if (string.IsNullOrEmpty(order.TransactionNumber))
            {
                LogError(null, "Transaction number not set");

                return new OrderCaptureInfo(OrderCaptureState.Failed, "Transaction number not set");
            }

            long orderTotalPIP = order.Price.PricePIP + PriceHelper.ConvertToPIP(order.Currency, order.ExternalPaymentFee);
            if (amount > orderTotalPIP)
            {
                LogError(null, "Amount to capture should be less or equal to order total");
                return new OrderCaptureInfo(OrderCaptureState.Failed, "Amount to capture should be less or equal to order total");
            }

            string transactionId = order.TransactionNumber;
            using AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType? preOperationDetails = service.GetTransactionDetails(transactionId);

            if (preOperationDetails is null)
            {
                LogError(order, "Failed to retrieve transaction details for ID: {0}", transactionId);
                return new OrderCaptureInfo(OrderCaptureState.Failed, "Failed to retrieve transaction details");
            }

            double preOperationCapturedAmount = preOperationDetails.SettleAmount;
            double operationCaptureAmount = amount / 100d;

            CreateTransactionResponse? response = service.Capture(order, operationCaptureAmount);

            if (!IsResponseSuccessful(response, order) || response?.TransactionResponse is null)
            {
                string message = response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
                            ?? response?.Messages?.Message?.FirstOrDefault()?.Text
                            ?? "Capture failed";
                LogEvent(order, message, DebuggingInfoType.CaptureResult);

                return new OrderCaptureInfo(OrderCaptureState.Failed, message);
            }

            LogEvent(order, PreparedMessages.CaptureSuccessMessage, DebuggingInfoType.CaptureResult);
            OrderHelper.UpdateTransactionNumber(order, response.TransactionResponse.TransId ?? "");
            transactionId = order.TransactionNumber;

            TransactionDetailsType? postOperationDetails = service.GetTransactionDetails(transactionId);
            if (postOperationDetails is null)
            {
                LogError(order, "Failed to retrieve transaction details for ID: {0}", transactionId);
                return new OrderCaptureInfo(OrderCaptureState.Failed, "Failed to retrieve transaction details");
            }

            double postOperationCapturedAmount = postOperationDetails.SettleAmount;

            // Check if captured amount actually changed.
            // This is important for idempotency - if the same capture request is sent multiple times, we don't want to treat it as a new capture if the captured amount hasn't changed.
            bool actuallyChanged = postOperationCapturedAmount > preOperationCapturedAmount;

            OrderCaptureInfo captureState = DetermineCaptureState(postOperationDetails);

            if (actuallyChanged && (captureState.State is OrderCaptureState.Success ||
                captureState.State is OrderCaptureState.Split))
            {
                UpdateReturnOperationStatesAfterCapture(order, postOperationCapturedAmount);
                Ecommerce.Services.Orders.Save(order);

                LogEvent(order, "Capture operation completed. Amount changed from {0} to {1}",
                    preOperationCapturedAmount, postOperationCapturedAmount);
            }
            else if (!actuallyChanged)
            {
                LogEvent(order, "Capture operation idempotent - no changes made. Server amount remains: {0}", postOperationCapturedAmount);
                return new OrderCaptureInfo(OrderCaptureState.Cancel, "Capture request is idempotent - no changes made");
            }

            return captureState;
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Remote capture failed: {0}", ex.Message);
            return new OrderCaptureInfo(OrderCaptureState.Failed, "Remote capture failed");
        }
    }

    /// <summary>
    /// Determines capture state based on transaction details
    /// </summary>
    private OrderCaptureInfo DetermineCaptureState(TransactionDetailsType transactionDetails)
    {
        double totalCapturedAmount = transactionDetails.SettleAmount;
        double totalAuthorizedAmount = transactionDetails.AuthAmount;

        if (totalCapturedAmount >= totalAuthorizedAmount)
            return new OrderCaptureInfo(OrderCaptureState.Success, "Full capture completed");
        else if (totalCapturedAmount > 0)
            return new OrderCaptureInfo(OrderCaptureState.Split, "Partial capture completed");

        return new OrderCaptureInfo(OrderCaptureState.Failed, "No amount captured");
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

        if (order.CaptureInfo is null || (order.CaptureInfo.State is not OrderCaptureState.Success and not OrderCaptureState.Split) || order.CaptureAmount <= 0.00)
        {
            string errorMessage = "Order must be captured before return.";
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorMessage, order.CaptureAmount, order);
            LogError(null, errorMessage);
            return;
        }

        // Calculate remaining refundable amount (captured minus already refunded)
        double previouslyRefunded = 0.0;
        if (order.ReturnOperations?.Any() is true)
        {
            foreach (OrderReturnInfo refundOperation in order.ReturnOperations)
            {
                if (refundOperation.State is OrderReturnOperationState.PartiallyReturned ||
                    refundOperation.State is OrderReturnOperationState.FullyReturned)
                {
                    previouslyRefunded += refundOperation.Amount;
                }
            }
        }

        double remainingRefundable = order.CaptureAmount - previouslyRefunded;

        double operationAmount = amount is null
            ? remainingRefundable
            : Converter.ToDouble(amount) / 100;

        if (operationAmount <= 0.01)
        {
            string errorMessage = $"Nothing left to refund. Captured: {order.CaptureAmount:C}, Already refunded: {previouslyRefunded:C}.";
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorMessage, 0, order);
            LogError(order, errorMessage);
            return;
        }

        if (operationAmount > remainingRefundable + 0.01)
        {
            string formattedRemaining = Ecommerce.Services.Currencies.Format(order.Currency, remainingRefundable);
            string formattedRequestedAmount = Ecommerce.Services.Currencies.Format(order.Currency, operationAmount);

            OrderReturnInfo.SaveReturnOperation
            (
                OrderReturnOperationState.Failed,
                $"Remaining refundable amount ({formattedRemaining}) less than amount requested for return ({formattedRequestedAmount}).",
                operationAmount,
                order
            );
            LogError(order, "Remaining refundable amount less than amount requested for return.");
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
            using AuthorizeNetService service = GetAuthorizeNetService(order);
            CreateTransactionResponse? response = service.Refund(order, operationAmount);

            if (!IsResponseSuccessful(response, order))
            {
                string message = response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
                               ?? response?.Messages?.Message?.FirstOrDefault()?.Text
                               ?? "Refund failed";

                LogError(order, message);
                OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, message, operationAmount, order);

                return;
            }

            string refundTransId = response?.TransactionResponse?.TransId ?? "";
            double totalRefundedAfter = previouslyRefunded + operationAmount;
            bool isFullRefund = Math.Abs(totalRefundedAfter - order.CaptureAmount) <= 0.01;

            var operationState = isFullRefund
                ? OrderReturnOperationState.FullyReturned
                : OrderReturnOperationState.PartiallyReturned;

            string refundMessage = string.IsNullOrEmpty(refundTransId)
                ? "Authorize.Net has refunded payment."
                : $"Authorize.Net has refunded payment. [RefundTransId:{refundTransId}]";

            OrderReturnInfo.SaveReturnOperation(operationState, refundMessage, operationAmount, order);

        }
        catch (Exception ex)
        {
            string errorMessage = $"{PreparedMessages.UnexpectedErrorMessage}: {ex.Message}";
            LogError(order, ex, errorMessage);
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorMessage, 0, order);
        }
    }

    /// <summary>
    /// Deletes a saved card from the customer profile in Authorize.Net
    /// </summary>
    /// <param name="savedCardId">ID of the saved card to delete</param>
    public void DeleteSavedCard(int savedCardId)
    {
        LogOrderDebuggingInfo($"Attempting to delete saved card ID: {savedCardId}", DebuggingInfoType.UseSavedCard);

        PaymentCardToken? savedCard = Ecommerce.Services.PaymentCard.GetById(savedCardId);
        if (savedCard is null)
        {
            LogOrderDebuggingInfo($"Saved card not found with ID: {savedCardId}", DebuggingInfoType.UseSavedCard);
            return;
        }

        string? cardToken = savedCard.Token;
        int userId = savedCard.UserID;

        if (userId <= 0)
        {
            LogOrderDebuggingInfo($"Invalid user ID for saved card: {userId}", DebuggingInfoType.UseSavedCard);
            return;
        }

        if (string.IsNullOrEmpty(cardToken))
        {
            LogOrderDebuggingInfo($"Invalid card token for saved card ID: {savedCardId}", DebuggingInfoType.UseSavedCard);
            return;
        }

        try
        {
            using AuthorizeNetService service = GetAuthorizeNetService();

            CustomerProfileMaskedType? profile = GetCustomerProfile(service, userId, false);
            if (string.IsNullOrEmpty(profile?.CustomerProfileId))
            {
                LogOrderDebuggingInfo($"Customer profile not found for user ID: {userId}", DebuggingInfoType.UseSavedCard);
                return;
            }

            DeleteCustomerPaymentProfileResponse? response = service.DeleteCustomerPaymentProfile(profile.CustomerProfileId, cardToken);
            if (response is not null && Enum.TryParse(response.Messages?.ResultCode, true, out MessageTypeEnum resultCode) &&
                resultCode == MessageTypeEnum.Ok)
            {
                LogOrderDebuggingInfo($"Successfully deleted payment profile {cardToken} for customer {profile.CustomerProfileId}", DebuggingInfoType.UseSavedCard);
            }
            else
            {
                LogOrderDebuggingInfo($"Failed to delete payment profile. Response: {response?.Messages?.Message?.FirstOrDefault()?.Text}", DebuggingInfoType.UseSavedCard);
            }
        }
        catch (Exception ex)
        {
            LogOrderDebuggingInfo($"Exception occurred while deleting saved card: {ex.Message}", DebuggingInfoType.UseSavedCard);
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

        using AuthorizeNetService service = GetAuthorizeNetService(order);
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

    private void LogOrderDebuggingInfo(string message, DebuggingInfoType debuggingInfoType) =>
        Ecommerce.Services.OrderDebuggingInfos.Save(null, message, "Authorize.Net checkout handler", debuggingInfoType);

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

        using AuthorizeNetService service = GetAuthorizeNetService(order);
        CustomerProfileMaskedType? profile = GetCustomerProfile(service, order.CustomerAccessUserId, true);
        if (profile is null)
            return;

        string? cardToken = service.CreatePaymentProfileFromTransaction(order.TransactionNumber, profile.CustomerProfileId ?? "");
        if (string.IsNullOrEmpty(cardToken))
            return;

        PaymentCardToken? paymentCard = Ecommerce.Services.PaymentCard.CreatePaymentCard(
            order.CustomerAccessUserId, order.PaymentMethodId, cardName,
            order.TransactionCardType, order.TransactionCardNumber, cardToken
        );

        order.SavedCardId = paymentCard.ID;
        Save(order);
        LogEvent(order, "Saved card: {0}", paymentCard.Name);
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
                errorMessage, response.TransactionResponse.ResponseCode ?? "");
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
                using AuthorizeNetService service = GetAuthorizeNetService();

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
            using var service = GetAuthorizeNetService(order);

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

            string? orderId = webhookPayload?.Payload?.OrderId;
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
                    HandleAuthOrCaptureCallback(order, payload, eventType is NotificationEventType.AuthCaptureCreated);
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
