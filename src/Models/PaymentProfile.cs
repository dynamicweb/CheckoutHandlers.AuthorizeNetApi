using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "paymentProfile")]
internal sealed class PaymentProfile
{
    [DataMember(Name = "paymentProfileId")]
    public string PaymentProfileId { get; set; } = "";
}


