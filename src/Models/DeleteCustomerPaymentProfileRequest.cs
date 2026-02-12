using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "deleteCustomerPaymentProfileRequest")]
internal sealed class DeleteCustomerPaymentProfileRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [DataMember(Name = "clientId")]
    public string ClientId { get; set; } = "";

    [DataMember(Name = "refId")]
    public string RefId { get; set; } = "";

    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";

    [DataMember(Name = "customerPaymentProfileId")]
    public string CustomerPaymentProfileId { get; set; } = "";
}




