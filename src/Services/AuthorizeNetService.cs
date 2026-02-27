using Dynamicweb.Core;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.Orders;
using System;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

internal sealed class AuthorizeNetService : IDisposable
{
    private readonly AuthorizeNetHttpService _httpService;
    private readonly string _apiLoginId;
    private readonly string _transactionKey;
    private readonly AuthorizeNetLogger _logger;
    private bool _disposed = false;

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
    public AuthorizeNetService(string apiLoginId, string transactionKey, bool isTestMode, bool debugLogging, AuthorizeNetLogger? logger, Order? order = null)
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
    /// Disposes of the HTTP service resources and logs the service disposal.
    /// This method should be called when the service is no longer needed to free up resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _httpService?.Dispose();
        _logger.LogInfo("Service disposed");
        _disposed = true;
    }

    /// <summary>
    /// Creates a hosted payment page request to obtain a form token for use in Accept Hosted.
    /// This method generates a secure payment form that customers can use to enter their payment information.
    /// The hosted payment page handles PCI compliance and returns a transaction response.
    /// </summary>
    /// <param name="transactionRequest">The transaction request containing payment details, customer information, and order data</param>
    /// <param name="settings">Hosted payment settings including form configuration, styling options, and redirect URLs</param>
    /// <returns>
    /// A GetHostedPaymentPageResponse containing the form token and payment page URL, 
    /// or null if the request fails or authentication is invalid
    /// </returns>
    /// <remarks>
    /// This method corresponds to the getHostedPaymentPageRequest API call.
    /// The returned form token must be used within 15 minutes of generation.
    /// For more information, see: https://developer.authorize.net/api/reference/index.html#accept-suite-get-an-accept-payment-page
    /// </remarks>
    public GetHostedPaymentPageResponse? GetHostedPaymentPage(
        TransactionRequestType transactionRequest,
        HostedPaymentSettings settings)
    {
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

        var content = Converter.Serialize(wrapper);
        return _httpService.Post<GetHostedPaymentPageResponse>(content);
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

        var content = Converter.Serialize(wrapper);
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
    public TransactionDetailsType? GetTransactionDetails(string transactionId)
    {
        var request = new GetTransactionDetailsRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            TransrefId = transactionId,
        };

        var wrapper = new GetTransactionDetailsRequestWrapper
        {
            GetTransactionDetailsRequest = request
        };
        var response = _httpService.Post<GetTransactionDetailsResponse>(Converter.Serialize(wrapper));

        if (Enum.TryParse(response?.Messages?.ResultCode, true, out MessageTypeEnum resultCode) && resultCode is MessageTypeEnum.Ok)
            return response?.Transaction;

        return null;
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

        var response = _httpService.Post<GetCustomerProfileResponse>(Converter.Serialize(wrapper));

        if (response?.Profile is not null &&
            Enum.TryParse(response?.Messages?.ResultCode, true, out MessageTypeEnum resultCode) &&
            resultCode is MessageTypeEnum.Ok)
        {
            return response.Profile;
        }

        if (tryCreate)
        {
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

        return response?.CustomerPaymentProfileIdList.FirstOrDefault() ?? string.Empty;
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
}
