using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CustomerProfileMaskedType
{
    [DataMember(Name = "customerProfileId", EmitDefaultValue = false)]
    public string? CustomerProfileId { get; set; }
}
