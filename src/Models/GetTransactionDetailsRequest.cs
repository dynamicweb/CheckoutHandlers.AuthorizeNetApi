using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "getTransactionDetailsRequest")]
internal sealed class GetTransactionDetailsRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [DataMember(Name = "clientId")]
    public string ClientId { get; set; } = "";

    [DataMember(Name = "refId")]
    public string RefId { get; set; } = "";

    [DataMember(Name = "transrefId")]
    public string TransrefId { get; set; } = "";
}




