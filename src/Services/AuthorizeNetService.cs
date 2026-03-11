using Dynamicweb.Core;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Exceptions;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

internal sealed class AuthorizeNetService
{
    private readonly AuthorizeNetHttpService _httpService;
    private readonly string _apiLoginId;
    private readonly string _transactionKey;
    private readonly AuthorizeNetLogger _logger;

    /// <summary>
    /// Current order for context in logging (optional, used only for logging purposes)  
    /// </summary>
    public Order? Order { get; }

    /// <summary>
    /// Initializes a new instance of the AuthorizeNetService with the specified configuration.
    /// This service provides methods for interacting with the Authorize.Net API for payment processing,
    /// customer profile management, and transaction operations.
    /// </summary>
    /// <param name="apiLoginId">The API Login ID from your Authorize.Net merchant account</param>
    /// <param name="transactionKey">The Transaction Key from your Authorize.Net merchant account</param>
    /// <param name="isTestMode">True to use the sandbox environment, false for production</param>
    /// <param name="debugLogging">True to enable detailed HTTP request/response logging</param>
    /// <param name="logger">Optional logger instance for request logging and debugging</param>
    /// <param name="order">Optional order context for enhanced logging</param>
    public AuthorizeNetService(string apiLoginId, string transactionKey, bool isTestMode, bool debugLogging, AuthorizeNetLogger? logger, Order? order)
    {
        _apiLoginId = apiLoginId;
        _transactionKey = transactionKey;
        Order = order;

        _logger = logger ?? new AuthorizeNetLogger(debugLogging, order);

        _httpService = new AuthorizeNetHttpService(
            isTestMode: isTestMode,
            debugEnabled: debugLogging,
            logger: _logger
        );

        _logger.LogInfo("AuthorizeNet service initialized");
    }
    /// <summary>
    /// Creates a hosted payment page request to obtain a form token for use in Accept Hosted.
    /// This method generates a secure payment form that customers can use to enter their payment information.
    /// The hosted payment page handles PCI compliance and returns a transaction response.
    /// </summary>
    /// <param name="transactionRequest">The transaction request containing payment details, customer information, and order data</param>
    /// <param name="settings">Hosted payment settings including form configuration, styling options, and redirect URLs</param>   
    /// <remarks>
    /// This method corresponds to the getHostedPaymentPageRequest API call.
    /// For more information, see: https://developer.authorize.net/api/reference/index.html#accept-suite-get-an-accept-payment-page
    /// </remarks>
    public GetHostedPaymentPageResponse? GetHostedPaymentPage(
        Order order, double orderAmount, TransactionType transactionType, bool saveCard,
        HostedPaymentSettings settings)
    {
        TransactionRequestType transactionRequest = CreateHostedFormRequest(order, orderAmount, transactionType, saveCard);

        var request = new GetHostedPaymentPageRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            TransactionRequest = transactionRequest,
            HostedPaymentSettings = new HostedPaymentSettings
            {
                Setting = settings.Setting ?? []
            }
        };

        var wrapper = new GetHostedPaymentPageRequestWrapper
        {
            GetHostedPaymentPageRequest = request
        };

        string content = Converter.Serialize(wrapper);
        return _httpService.Post<GetHostedPaymentPageResponse>(content);
    }

    /// <summary>
    /// Creates a transaction request for payments with saved card (Customer Profile)
    /// </summary>
    /// <param name="order">Order to create the request for</param>
    /// <param name="orderAmount">Amount of the order</param>
    /// <param name="profileToCharge">Customer profile to charge</param>
    /// <param name="transactionType">Type of transaction to perform</param>
    /// <returns>A TransactionRequestType containing the details of the saved card payment request</returns>
    public TransactionRequestType CreateSavedCardPaymentRequest(Order order, double orderAmount, TransactionType transactionType, CustomerProfilePaymentType profileToCharge)
    {
        var request = CreateBasePaymentTransactionRequest(order, orderAmount, transactionType);
        request.Profile = profileToCharge;

        // For Customer Profiles, Authorize.Net automatically handles COF indicators
        // No need for additional processing options or subsequent auth information

        return request;
    }

    /// <summary>
    /// Creates a transaction request for direct payments with new card data
    /// </summary>
    /// <param name="order">Order to create the request for</param>
    /// <param name="orderAmount">Amount of the order</param>
    /// <param name="transactionType">Type of transaction to perform</param>
    /// <param name="saveCard">Indicates whether the card should be saved for future use</param>
    /// <returns>A TransactionRequestType containing the details of the direct payment request</returns>
    public TransactionRequestType CreateDirectPaymentRequest(Order order, double orderAmount, TransactionType transactionType, bool saveCard)
    {
        TransactionRequestType request = CreateBasePaymentTransactionRequest(order, orderAmount, transactionType);

        // Set stored credentials flag if card needs to be saved
        if (saveCard)
        {
            request.ProcessingOptions = new ProcessingOptionsType
            {
                IsStoredCredentials = true
            };
        }

        return request;
    }

    /// <summary>
    /// Creates a transaction request for hosted form payments (no direct payment data)
    /// </summary>
    /// <param name="order">Order to create the request for</param>
    /// <param name="orderAmount">Amount of the order</param>
    private TransactionRequestType CreateHostedFormRequest(Order order, double orderAmount, TransactionType transactionType, bool saveCard)
    {
        var request = CreateBasePaymentTransactionRequest(order, orderAmount, transactionType);

        // Include customer data for hosted form
        AddCustomerData(request, order);

        // Set stored credentials flag if card needs to be saved
        if (saveCard)
        {
            request.ProcessingOptions = new ProcessingOptionsType
            {
                IsStoredCredentials = true
            };
        }

        return request;
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


    /// <summary>
    /// Creates the base transaction request with common fields
    /// </summary>
    /// <param name="order">Order to create the request for</param>
    /// <param name="orderAmount">Amount of the order</param>
    private TransactionRequestType CreateBasePaymentTransactionRequest(Order order, double orderAmount, TransactionType transactionType) => new()
    {
        Amount = orderAmount,
        CurrencyCode = order.CurrencyCode,
        LineItems = AuthorizeNetModelFactory.CreateLineItems(order),
        Order = new Models.OrderType
        {
            InvoiceNumber = order.Id
        },
        CustomerIp = StringHelper.Crop(order.Ip, 15),
        TransactionType = transactionType is TransactionType.AuthCaptureTransaction
                ? TransactionTypeEnum.AuthCaptureTransaction
                : TransactionTypeEnum.AuthOnlyTransaction
    };

    /// <summary>
    /// Processes a void transaction for the specified order, reversing the original payment.
    /// </summary>
    /// <remarks>Ensure that the order is eligible for a void operation before calling this method. The order
    /// must reference a valid transaction that can be voided.</remarks>
    /// <param name="order">The order for which the void transaction is to be processed. Must contain a valid transaction number, amount,
    /// and currency code.</param>
    public CreateTransactionResponse? Void(Order order)
    {
        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.VoidTransaction,
            RefTransId = order.TransactionNumber,
            Amount = order.TransactionAmount,
            CurrencyCode = order.CurrencyCode
        };

        return CreateTransaction(transactionRequest);
    }

    /// <summary>
    /// Captures a previously authorized transaction using the PriorAuthCaptureTransaction type.
    /// </summary>
    /// <param name="order">The order containing the transaction to be captured</param>
    /// <param name="captureAmount">The amount to capture from the authorized transaction</param>
    /// <returns>A CreateTransactionResponse containing the results of the capture transaction</returns>
    public CreateTransactionResponse? Capture(Order order, double captureAmount)
    {
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

        CreateTransactionResponse? response = CreateTransaction(transactionRequest);
        return response;
    }

    /// <summary>
    /// Processes a refund for the specified order and returns the result of the transaction request.   
    /// </summary>
    /// <remarks>Ensure that the refund amount is valid and that the order is eligible for a refund. The
    /// method uses the last four digits of the credit card number associated with the order for processing.</remarks>
    /// <param name="order">The order for which the refund is being processed. Must contain valid transaction details, including the
    /// transaction number and currency code.</param>
    /// <param name="refundAmount">The amount to be refunded. Must be a positive value and cannot exceed the original transaction amount.</param>
    /// <returns>A response object containing the result of the refund transaction, or null if the transaction could not be processed.</returns>
    public CreateTransactionResponse? Refund(Order order, double refundAmount)
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

        CreateTransactionResponse? response = CreateTransaction(transactionRequest);
        return response;
    }

    /// <summary>
    /// Creates a payment transaction using the Authorize.Net API.
    /// Supports various transaction types including authorization, capture, sale, refund, void, and credit.
    /// This method handles real-time payment processing and returns immediate transaction results.
    /// </summary>
    /// <param name="transactionRequest">
    /// The transaction request containing:
    /// - Transaction type (authOnlyTransaction, authCaptureTransaction, captureOnlyTransaction, refundTransaction, voidTransaction)
    /// - Payment information (credit card, bank account, or payment profile)
    /// - Customer and billing information
    /// - Order details and line items
    /// - Custom fields and merchant-defined data
    /// </param>
    /// <returns>
    /// A CreateTransactionResponse containing transaction results including:
    /// - Transaction ID and reference number
    /// - Authorization code and AVS/CVV results
    /// - Transaction status and response messages
    /// - Account information (masked)
    /// Returns null if the request fails or authentication is invalid
    /// </returns>
    /// <remarks>
    /// This method corresponds to the createTransactionRequest API call.
    /// Transaction amounts are processed in the currency configured for the merchant account.
    /// For more information, see: https://developer.authorize.net/api/reference/index.html#payment-transactions-charge-a-credit-card
    /// </remarks>
    public CreateTransactionResponse? CreateTransaction(
        TransactionRequestType transactionRequest)
    {
        var request = new CreateTransactionRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            TransactionRequest = transactionRequest
        };

        var wrapper = new CreateTransactionRequestWrapper
        {
            CreateTransactionRequest = request
        };

        string content = Converter.Serialize(wrapper);
        return _httpService.Post<CreateTransactionResponse>(content);
    }

    /// <summary>
    /// Retrieves detailed information about a specific transaction using its transaction ID.
    /// This method provides comprehensive transaction data including payment details, 
    /// customer information, settlement status, and any associated fraud detection results.
    /// </summary>
    /// <param name="transactionId">
    /// The unique transaction identifier returned from a successful payment transaction.
    /// This can be either the transaction ID from createTransaction or the reference ID.
    /// </param>
    /// <returns>
    /// A TransactionDetailsType object containing complete transaction information including:
    /// - Transaction status, type, and response codes
    /// - Payment method details (masked for security)
    /// - Customer and billing information
    /// - Settlement date and batch information
    /// - Line items and tax details
    /// - Fraud detection service results
    /// Returns null if the transaction is not found or access is denied
    /// </returns>
    /// <remarks>
    /// This method corresponds to the getTransactionDetailsRequest API call.
    /// Transaction details are available for up to 2 years after the transaction date.
    /// For more information, see: https://developer.authorize.net/api/reference/index.html#transaction-reporting-get-transaction-details
    /// </remarks>
    public TransactionDetailsType GetTransactionDetails(string transactionId)
    {
        var request = new GetTransactionDetailsRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            TransId = transactionId,
        };

        var wrapper = new GetTransactionDetailsRequestWrapper
        {
            GetTransactionDetailsRequest = request
        };

        GetTransactionDetailsResponse? response = _httpService.Post<GetTransactionDetailsResponse>(Converter.Serialize(wrapper));
        TransactionDetailsType? transactionDetails = response?.Transaction;

        if (transactionDetails is not null && !string.IsNullOrWhiteSpace(transactionDetails.TransId))
            return transactionDetails;

        throw new AuthorizeNetApiException($"Failed to retrieve transaction details. Transaction ID is missing in the response.");
    }

    /// <summary>
    /// Retrieves a customer profile containing saved payment information and shipping addresses.
    /// If the profile doesn't exist and tryCreate is true, attempts to create a new empty customer profile.
    /// Customer profiles enable secure storage of payment information for recurring transactions.
    /// </summary>
    /// <param name="userId">
    /// The merchant-assigned customer identifier. This should be unique within your merchant account
    /// and is typically the customer's user ID from your system.
    /// </param>
    /// <param name="tryCreate">
    /// If true and the customer profile is not found, automatically creates a new empty customer profile.
    /// If false, returns null when the profile doesn't exist.
    /// </param>
    /// <returns>
    /// A CustomerProfileMaskedType containing:
    /// - Customer profile ID (required for future operations)
    /// - Merchant customer ID
    /// - Customer description and email
    /// - Payment profiles with masked account information
    /// - Shipping addresses
    /// Returns null if the profile is not found and tryCreate is false, or if the request fails
    /// </returns>
    /// <remarks>
    /// This method corresponds to the getCustomerProfileRequest API call.
    /// Customer profiles help maintain PCI compliance by storing sensitive payment data securely at Authorize.Net.
    /// Payment information is masked in the response for security purposes.
    /// For more information, see: https://developer.authorize.net/api/reference/index.html#customer-profiles-get-customer-profile
    /// </remarks>
    public CustomerProfileMaskedType? GetCustomerProfile(int userId, bool tryCreate)
    {
        var getProfileRequest = new GetCustomerProfileRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            MerchantCustomerId = userId.ToString(),
        };

        var wrapper = new GetCustomerProfileRequestWrapper
        {
            GetCustomerProfileRequest = getProfileRequest
        };

        GetCustomerProfileResponse? response = null;

        try
        {
            response = _httpService.Post<GetCustomerProfileResponse>(Converter.Serialize(wrapper));
        }
        catch (Exception ex)
        {
            string createDecision = tryCreate
                ? "Will attempt to create new profile."
                : string.Empty;

            _logger.LogError(ex, "Failed to get customer profile. {0} Error message: {1}", createDecision, ex.Message);
        }

        if (response?.Profile is not null &&
            Enum.TryParse(response?.Messages?.ResultCode, true, out MessageTypeEnum resultCode) &&
            resultCode is MessageTypeEnum.Ok)
        {
            return response.Profile;
        }

        if (!tryCreate)
            return null;

        var createProfileRequest = new CreateCustomerProfileRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            Profile = new CustomerProfileType { MerchantCustomerId = userId.ToString() },
        };

        var createWrapper = new CreateCustomerProfileRequestWrapper
        {
            CreateCustomerProfileRequest = createProfileRequest
        };

        var createResponse = _httpService.Post<CreateCustomerProfileResponse>(Converter.Serialize(createWrapper));

        if (Enum.TryParse(createResponse?.Messages?.ResultCode, true, out MessageTypeEnum code) && code is MessageTypeEnum.Ok)
        {
            return new CustomerProfileMaskedType
            {
                CustomerProfileId = createResponse?.CustomerProfileId ?? ""
            };
        }

        return null;
    }

    /// <summary>
    /// Creates a new customer payment profile from an existing successful transaction.
    /// This allows you to save the payment information used in a transaction to the customer's profile
    /// for future recurring payments or one-click checkout scenarios.
    /// </summary>
    /// <param name="transactionId">
    /// The transaction ID of a successful authOnlyTransaction or authCaptureTransaction.
    /// The transaction must have been processed successfully and contain valid payment information.
    /// </param>
    /// <param name="customerProfileId">
    /// The existing customer profile ID where the payment method should be saved.
    /// The customer profile must already exist in the system.
    /// </param>
    /// <returns>
    /// The customer payment profile ID of the newly created payment profile.
    /// This ID can be used for future transactions using the saved payment method.
    /// Returns an empty string if the operation fails or the transaction/profile is invalid.
    /// </returns>
    /// <remarks>
    /// This method corresponds to the createCustomerProfileFromTransactionRequest API call.
    /// The original transaction must have been successful and contain complete payment information.
    /// This feature helps reduce PCI scope by storing payment data securely at Authorize.Net.
    /// For more information, see: https://developer.authorize.net/api/reference/index.html#customer-profiles-create-customer-profile-from-transaction
    /// </remarks>
    public string CreatePaymentProfileFromTransaction(string transactionId, string customerProfileId)
    {
        var request = new CreateCustomerProfileFromTransactionRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            TransId = transactionId,
            CustomerProfileId = customerProfileId,
        };

        var wrapper = new CreateCustomerProfileFromTransactionRequestWrapper
        {
            CreateCustomerProfileFromTransactionRequest = request
        };
        var response = _httpService.Post<CreateCustomerProfileResponse>(Converter.Serialize(wrapper));

        return response?.CustomerPaymentProfileIdList?.FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Deletes an existing customer payment profile from a customer's profile.
    /// This permanently removes the saved payment method and cannot be undone.
    /// Use this method when customers want to remove saved cards or when payment methods expire.
    /// </summary>
    /// <param name="customerProfileId">
    /// The customer profile ID that contains the payment profile to be deleted.
    /// This must be an existing valid customer profile ID.
    /// </param>
    /// <param name="paymentProfileId">
    /// The customer payment profile ID to be deleted.
    /// This is the ID of the specific payment method within the customer profile.
    /// </param>
    /// <returns>
    /// A DeleteCustomerPaymentProfileResponse containing the operation results:
    /// - Messages indicating success or failure
    /// - Response and result codes
    /// - Any error messages if the operation failed
    /// Returns null if the request fails due to authentication or network issues.
    /// </returns>
    /// <remarks>
    /// This method corresponds to the deleteCustomerPaymentProfileRequest API call.
    /// Once deleted, the payment profile cannot be recovered and any recurring transactions
    /// using this profile will fail. Ensure you update any recurring billing before deletion.
    /// For more information, see: https://developer.authorize.net/api/reference/index.html#customer-profiles-delete-customer-payment-profile
    /// </remarks>
    public DeleteCustomerPaymentProfileResponse? DeleteCustomerPaymentProfile(string customerProfileId, string paymentProfileId)
    {
        var request = new DeleteCustomerPaymentProfileRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            CustomerProfileId = customerProfileId,
            CustomerPaymentProfileId = paymentProfileId,
        };

        var wrapper = new DeleteCustomerPaymentProfileRequestWrapper
        {
            DeleteCustomerPaymentProfileRequest = request
        };

        return _httpService.Post<DeleteCustomerPaymentProfileResponse>(Converter.Serialize(wrapper));
    }

    /// <summary>
    /// Creates the merchant authentication object required for all Authorize.Net API calls.
    /// This object contains the API credentials that authenticate the merchant account.
    /// </summary>
    /// <returns>
    /// A MerchantAuthenticationType object containing the API login ID and transaction key
    /// </returns>
    private MerchantAuthenticationType GetMerchantAuthentication() => new()
    {
        Name = _apiLoginId,
        TransactionKey = _transactionKey
    };

    #region Webhook Management

    /// <summary>
    /// Creates a new webhook subscription
    /// </summary>
    /// <param name="request">Webhook registration request</param>
    /// <remarks>
    /// See: https://developer.authorize.net/api/reference/features/webhooks.html#API_Calls
    /// </remarks>
    public WebhookResponse CreateWebhook(WebhookRequest request)
    {
        try
        {
            _logger.LogInfo("[WEBHOOK] Creating webhook for URL: {0}", request.Url ?? "");

            string endpoint = WebhookEndpoints.Base;
            string jsonRequest = Converter.SerializeCompact(request);

            var response = _httpService.Post<WebhookResponse>(endpoint, jsonRequest, GetBasicAuthHeaders());
            if (response is not null)
            {
                _logger.LogInfo("[WEBHOOK] Successfully created webhook ID: {0}", response.WebhookId ?? "");
                return response;
            }

            throw new InvalidOperationException("Failed to create webhook - no response received");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WEBHOOK] Failed to create webhook: {0}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets list of all webhooks for the merchant account
    /// </summary>
    /// See: https://developer.authorize.net/api/reference/features/webhooks.html#API_Calls
    public WebhookListResponse GetWebhooks()
    {
        try
        {
            _logger.LogInfo("[WEBHOOK] Retrieving webhook list");

            string endpoint = WebhookEndpoints.Base;
            var webhooks = _httpService.Get<WebhookResponse[]>(endpoint, GetBasicAuthHeaders());

            return new WebhookListResponse
            {
                Webhooks = webhooks ?? []
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WEBHOOK] Failed to get webhooks: {0}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets details of a specific webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID to retrieve</param>
    /// See: https://developer.authorize.net/api/reference/features/webhooks.html#API_Calls
    public WebhookResponse? GetWebhook(string webhookId)
    {
        try
        {
            _logger.LogInfo("[WEBHOOK] Retrieving webhook: {0}", webhookId);

            string endpoint = WebhookEndpoints.GetSpecific(webhookId);
            return _httpService.Get<WebhookResponse>(endpoint, GetBasicAuthHeaders());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WEBHOOK] Failed to get webhook {0}: {1}", webhookId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID to update</param>
    /// <param name="request">Updated webhook details</param>
    /// <remarks>
    /// See: https://developer.authorize.net/api/reference/features/webhooks.html#API_Calls
    /// </remarks>
    public WebhookResponse? UpdateWebhook(string webhookId, WebhookRequest request)
    {
        try
        {
            _logger.LogInfo("[WEBHOOK] Updating webhook: {0}", webhookId);

            string endpoint = WebhookEndpoints.GetSpecific(webhookId);
            string jsonRequest = Converter.SerializeCompact(request);

            return _httpService.Put<WebhookResponse>(endpoint, jsonRequest, GetBasicAuthHeaders());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WEBHOOK] Failed to update webhook {0}: {1}", webhookId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Deletes a webhook
    /// </summary>
    /// <param name="webhookId">Webhook ID to delete</param>
    /// <remarks>
    /// See: https://developer.authorize.net/api/reference/features/webhooks.html#API_Calls
    /// </remarks>
    public void DeleteWebhook(string webhookId)
    {
        if (string.IsNullOrWhiteSpace(webhookId))
        {
            _logger.LogError("[WEBHOOK] Skipping webhook deletion because webhook ID is missing.");
            return;
        }

        try
        {
            _logger.LogInfo("[WEBHOOK] Deleting webhook: {0}", webhookId);

            string endpoint = WebhookEndpoints.GetSpecific(webhookId);
            _httpService.Delete(endpoint, GetBasicAuthHeaders());

            _logger.LogInfo("[WEBHOOK] Successfully deleted webhook: {0}", webhookId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WEBHOOK] Failed to delete webhook {0}: {1}", webhookId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets all available event types for webhooks
    /// </summary>
    /// <returns>Array of available event types</returns>
    /// <remarks>
    /// See: https://developer.authorize.net/api/reference/features/webhooks.html#API_Calls
    /// </remarks>
    public EventTypeResponse[] GetEventTypes()
    {
        try
        {
            _logger.LogInfo("[WEBHOOK] Retrieving available event types");

            string endpoint = WebhookEndpoints.EventTypes;
            return _httpService.Get<EventTypeResponse[]>(endpoint, GetBasicAuthHeaders()) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WEBHOOK] Failed to get event types: {0}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Ensures all required webhooks are registered for the specified URL
    /// </summary>
    /// <param name="webhookUrl">The webhook endpoint URL</param>
    /// <param name="forceRegistration">Whether to force re-registration even if webhooks exist</param>
    /// <returns>Webhook registration response or null if no action needed</returns>
    public WebhookResponse? EnsureWebhooksRegistered(string webhookUrl, bool forceRegistration)
    {
        try
        {
            _logger.LogInfo("[WEBHOOK] Ensuring webhooks are registered for URL: {0}", webhookUrl);

            // Get current webhooks
            WebhookListResponse existingWebhooks = GetWebhooks();
            List<WebhookResponse> ourWebhooks = existingWebhooks.Webhooks
                .Where(w => webhookUrl.Equals(w.Url, StringComparison.OrdinalIgnoreCase))
                .ToList();

            WebhookRegistrationResult registrationResult = DetermineWebhookAction(ourWebhooks, forceRegistration);

            _logger.LogInfo("[WEBHOOK] Registration check result: RequiresRegistration={0}, Reason={1}",
                registrationResult.RequiresRegistration, registrationResult.Reason);

            if (!registrationResult.RequiresRegistration)
                return null;

            // Delete existing webhooks for this URL
            foreach (WebhookResponse webhook in ourWebhooks)
            {
                if (string.IsNullOrWhiteSpace(webhook.WebhookId))
                {
                    _logger.LogError("[WEBHOOK] Skipping webhook cleanup for URL {0} because webhook ID is missing.",
                        webhook.Url ?? webhookUrl);
                    continue;
                }

                _logger.LogInfo("[WEBHOOK] Cleaning up existing webhook: {0}", webhook.WebhookId);
                DeleteWebhook(webhook.WebhookId);
            }

            // Create new webhook with all required event types
            var request = new WebhookRequest
            {
                Name = "AuthorizeNetWebhook",
                Url = webhookUrl,
                EventTypes = GetRequiredPaymentEventTypes(),
                Status = "active"
            };

            WebhookResponse newWebhook = CreateWebhook(request);
            _logger.LogInfo("[WEBHOOK] Successfully registered new webhook with ID: {0}", newWebhook.WebhookId ?? "");

            return newWebhook;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WEBHOOK] Failed to ensure webhooks registration: {0}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets the required payment event types for Dynamicweb integration
    /// </summary>
    /// <returns>Array of required event type names</returns>
    private static string[] GetRequiredPaymentEventTypes()
    {
        return
        [
            "net.authorize.payment.authorization.created",
            "net.authorize.payment.authcapture.created",
            "net.authorize.payment.capture.created",
            "net.authorize.payment.refund.created",
            "net.authorize.payment.priorAuthCapture.created",
            "net.authorize.payment.void.created"
        ];
    }

    /// <summary>
    /// Determines if webhook registration is needed based on current state
    /// </summary>
    private WebhookRegistrationResult DetermineWebhookAction(List<WebhookResponse> existingWebhooks, bool forceRegistration)
    {
        if (forceRegistration)
            return new WebhookRegistrationResult(true, "Force re-registration is enabled");

        if (existingWebhooks.Count == 0)
            return new WebhookRegistrationResult(true, "No existing webhooks found for this URL");

        if (existingWebhooks.Count > 1)
            return new WebhookRegistrationResult(true, "Multiple webhooks found - consolidating into one");

        // Check if the single webhook has all required event types
        WebhookResponse webhook = existingWebhooks.First();
        string[] requiredEvents = GetRequiredPaymentEventTypes();
        var registeredEvents = new HashSet<string>(webhook.EventTypes ?? [], StringComparer.OrdinalIgnoreCase);
        List<string> missingEvents = requiredEvents.Where(e => !registeredEvents.Contains(e)).ToList();

        if (missingEvents.Count > 0)
            return new WebhookRegistrationResult(true, $"Missing required events: {string.Join(", ", missingEvents)}");

        if (!(webhook.Status?.Equals("active", StringComparison.OrdinalIgnoreCase) ?? false))
            return new WebhookRegistrationResult(true, "Existing webhook is not active");

        return new WebhookRegistrationResult(false, "All required webhooks are properly configured");
    }

    /// <summary>
    /// Gets headers for Basic Authentication required by Webhooks API
    /// </summary>
    private Dictionary<string, string> GetBasicAuthHeaders()
    {
        string credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_apiLoginId}:{_transactionKey}"));
        return new Dictionary<string, string>
        {
            ["Authorization"] = $"Basic {credentials}",
        };
    }

    #endregion
}
