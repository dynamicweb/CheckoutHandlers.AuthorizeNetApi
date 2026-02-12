using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "getCustomerProfileRequest")]
internal sealed class GetCustomerProfileRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [DataMember(Name = "clientId")]
    public string ClientId { get; set; } = "";

    [DataMember(Name = "refId")]
    public string RefId { get; set; } = "";

    [DataMember(Name = "merchantCustomerId")]
    public string MerchantCustomerId { get; set; } = "";
}




