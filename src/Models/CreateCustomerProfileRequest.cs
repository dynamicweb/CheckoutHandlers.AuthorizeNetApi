using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreateCustomerProfileRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [DataMember(Name = "refId", EmitDefaultValue = false)]
    public string? RefId { get; set; }

    [DataMember(Name = "profile")]
    public CustomerProfileType Profile { get; set; } = new();
}
