using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreateCustomerProfileFromTransactionRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [DataMember(Name = "refId", EmitDefaultValue = false)]
    public string? RefId { get; set; }

    [DataMember(Name = "transId")]
    public string TransId { get; set; } = "";

    [DataMember(Name = "customerProfileId", EmitDefaultValue = false)]
    public string? CustomerProfileId { get; set; }
}
