using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CustomerProfilePaymentType
{
    [DataMember(Name = "customerProfileId", EmitDefaultValue = false)]
    public string? CustomerProfileId { get; set; }

    [DataMember(Name = "paymentProfile", EmitDefaultValue = false)]
    public PaymentProfile? PaymentProfile { get; set; }
}
