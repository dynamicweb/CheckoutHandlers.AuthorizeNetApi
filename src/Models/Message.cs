using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class Message
{
    [DataMember(Name = "code", EmitDefaultValue = false)]
    public string? Code { get; set; }

    [DataMember(Name = "text", EmitDefaultValue = false)]
    public string? Text { get; set; }
}
