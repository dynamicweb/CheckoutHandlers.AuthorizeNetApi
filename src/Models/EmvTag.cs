using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class EmvTag
{
    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; } = "";

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string Value { get; set; } = "";

    [DataMember(Name = "formatted", EmitDefaultValue = false)]
    public string Formatted { get; set; } = "";
}