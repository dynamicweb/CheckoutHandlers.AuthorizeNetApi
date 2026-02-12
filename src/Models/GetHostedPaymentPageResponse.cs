using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "getHostedPaymentPageResponse")]
internal sealed class GetHostedPaymentPageResponse
{
    [DataMember(Name = "token")]
    public string Token { get; set; } = "";

    [DataMember(Name = "messages")]
    public MessagesType Messages { get; set; } = new();
}


