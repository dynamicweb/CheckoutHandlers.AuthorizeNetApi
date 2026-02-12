using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "createCustomerProfileFromTransactionRequest")]
internal sealed class CreateCustomerProfileFromTransactionRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [DataMember(Name = "clientId")]
    public string ClientId { get; set; } = "";

    [DataMember(Name = "refId")]
    public string RefId { get; set; } = "";

    [DataMember(Name = "transId")]
    public string TransId { get; set; } = "";

    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";
}