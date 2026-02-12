using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class HostedPaymentReturnOptions
{
    [DataMember(Name = "url")]
    public string Url { get; set; } = "";

    [DataMember(Name = "cancelUrl")]
    public string CancelUrl { get; set; } = "";
}

