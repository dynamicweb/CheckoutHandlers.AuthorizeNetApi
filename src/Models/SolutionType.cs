using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class SolutionType
{
    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string Id { get; set; } = "";

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; } = "";

    [DataMember(Name = "vendorName", EmitDefaultValue = false)]
    public string VendorName { get; set; } = "";
}