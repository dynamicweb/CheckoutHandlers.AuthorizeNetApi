using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "emvTag")]
internal sealed class EmvTag
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = "";

    [DataMember(Name = "value")]
    public string Value { get; set; } = "";

    [DataMember(Name = "formatted")]
    public string Formatted { get; set; } = "";
}


