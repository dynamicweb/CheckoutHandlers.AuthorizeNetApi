using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class HostedPaymentPaymentOptions
{
    [DataMember(Name = "showBankAccount", EmitDefaultValue = false)]
    public bool ShowBankAccount { get; set; }
}