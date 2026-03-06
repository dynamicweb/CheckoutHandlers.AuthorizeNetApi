using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class UserField
{
    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string? Name { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string? Value { get; set; }
}
