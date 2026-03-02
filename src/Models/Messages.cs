using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class Messages
{
    [DataMember(Name = "resultCode")]
    public string? ResultCode { get; set; }

    [DataMember(Name = "message")]
    public Message[]? Message { get; set; }
}
