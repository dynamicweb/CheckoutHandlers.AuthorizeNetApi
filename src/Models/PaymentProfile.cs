using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class PaymentProfile
{
    [DataMember(Name = "paymentProfileId", EmitDefaultValue = false)]
    public string PaymentProfileId { get; set; } = "";
}