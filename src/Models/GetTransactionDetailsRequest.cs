using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class GetTransactionDetailsRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();
        
    [DataMember(Name = "refId", EmitDefaultValue = false)]
    public string RefId { get; set; } = "";

    [DataMember(Name = "transId")]
    public string TransId { get; set; } = "";
}