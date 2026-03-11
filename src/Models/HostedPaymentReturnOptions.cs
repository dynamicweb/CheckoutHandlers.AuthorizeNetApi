using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class HostedPaymentReturnOptions
{
    [DataMember(Name = "url", EmitDefaultValue = false)]
    public string? Url { get; set; }

    [DataMember(Name = "cancelUrl", EmitDefaultValue = false)]
    public string? CancelUrl { get; set; }
}
