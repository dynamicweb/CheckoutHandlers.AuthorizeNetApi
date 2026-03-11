using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class GetHostedPaymentPageResponse
{
    [DataMember(Name = "token")]
    public string Token { get; set; } = "";

    [DataMember(Name = "messages", EmitDefaultValue = false)]
    public MessagesType? Messages { get; set; }
}
