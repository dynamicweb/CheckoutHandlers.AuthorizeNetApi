using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class FDSFilter
{
    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string? Name { get; set; }

    [DataMember(Name = "action", EmitDefaultValue = false)]
    public FdsFilterActionEnum? Action { get; set; }
}
