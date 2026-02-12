using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "FDSFilter")]
internal sealed class FDSFilter
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = "";

    [DataMember(Name = "action")]
    public FdsFilterActionEnum Action { get; set; }
}


