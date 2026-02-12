using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerProfilePaymentType")]
internal sealed class CustomerProfilePaymentType
{
    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";

    [DataMember(Name = "paymentProfile")]
    public PaymentProfile PaymentProfile { get; set; } = new();
}


