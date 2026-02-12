using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "solutionType")]
internal sealed class SolutionType
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = "";

    [DataMember(Name = "name")]
    public string Name { get; set; } = "";

    [DataMember(Name = "vendorName")]
    public string VendorName { get; set; } = "";
}


