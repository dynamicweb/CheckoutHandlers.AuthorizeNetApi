using Dynamicweb.Core;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

internal sealed class AuthorizeNetService : IDisposable
{
    private readonly AuthorizeNetHttpService _httpService;
    private readonly string _apiLoginId;
    private readonly string _transactionKey;
    private bool _disposed = false;

    public AuthorizeNetService(string apiLoginId, string transactionKey, bool isTestMode)
    {
        _apiLoginId = apiLoginId;
        _transactionKey = transactionKey;
        _httpService = new AuthorizeNetHttpService(isTestMode, isTestMode);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _httpService?.Dispose();
        _disposed = true;
    }

    public GetHostedPaymentPageResponse? GetHostedPaymentPage(TransactionRequestType transactionRequest, HostedPaymentSettings settings)
    {
        var request = new GetHostedPaymentPageRequest
        {
            MerchantAuthentication = GetMerchantAuthentication(),
            TransactionRequest = transactionRequest,
            HostedPaymentSettings = settings
        };

        var wrapper = new GetHostedPaymentPageRequestWrapper
        {
            GetHostedPaymentPageRequest = request
        };

        return _httpService.Post<GetHostedPaymentPageResponse>(Converter.Serialize(wrapper));
    }

    public CreateTransactionResponse? CreateTransaction(TransactionRequestType transactionRequest)
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

        return _httpService.Post<CreateTransactionResponse>(Converter.Serialize(wrapper));
    }

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

        if (Enum.TryParse(response?.Messages.ResultCode, true, out MessageTypeEnum resultCode) && resultCode is MessageTypeEnum.Ok)
            return response?.Transaction;

        return null;
    }

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
            Enum.TryParse(response.Messages?.ResultCode, true, out MessageTypeEnum resultCode) &&
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

            if (Enum.TryParse(createResponse?.Messages.ResultCode, true, out MessageTypeEnum code) && code is MessageTypeEnum.Ok)
            {
                return new CustomerProfileMaskedType
                {
                    CustomerProfileId = createResponse?.CustomerProfileId ?? ""
                };
            }
        }

        return null;
    }

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

    private MerchantAuthenticationType GetMerchantAuthentication() => new()
    {
        Name = _apiLoginId,
        TransactionKey = _transactionKey
    };
}
