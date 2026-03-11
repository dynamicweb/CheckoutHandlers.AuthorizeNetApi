using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class GetHostedPaymentPageRequestWrapper
{
    [DataMember(Name = "getHostedPaymentPageRequest")]
    public GetHostedPaymentPageRequest GetHostedPaymentPageRequest { get; set; } = new();
}