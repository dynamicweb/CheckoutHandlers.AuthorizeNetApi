using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class MessagesType
{
    [DataMember(Name = "resultCode", EmitDefaultValue = false)]
    public string? ResultCode { get; set; }

    [DataMember(Name = "message", EmitDefaultValue = false)]
    public Message[]? Message { get; set; }
}
