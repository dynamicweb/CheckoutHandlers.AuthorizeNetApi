using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CustomerProfileIdType
{
    [DataMember(Name = "customerProfileId", EmitDefaultValue = false)]
    public string? CustomerProfileId { get; set; }

    [DataMember(Name = "customerPaymentProfileId", EmitDefaultValue = false)]
    public string? CustomerPaymentProfileId { get; set; }

    [DataMember(Name = "customerAddressId", EmitDefaultValue = false)]
    public string? CustomerAddressId { get; set; }
}
