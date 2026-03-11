using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class Error
{
    [DataMember(Name = "errorCode", EmitDefaultValue = false)]
    public string? ErrorCode { get; set; }

    [DataMember(Name = "errorText", EmitDefaultValue = false)]
    public string? ErrorText { get; set; }
}
