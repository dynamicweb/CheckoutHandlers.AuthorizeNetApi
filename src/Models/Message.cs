using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "message")]
internal sealed class Message
{
    [DataMember(Name = "code")]
    public string Code { get; set; } = "";

    [DataMember(Name = "text")]
    public string Text { get; set; } = "";
}


