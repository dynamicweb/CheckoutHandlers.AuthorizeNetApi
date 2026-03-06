using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class NameAndAddressType
{
    [DataMember(Name = "firstName", EmitDefaultValue = false)]
    public string? FirstName { get; set; }

    [DataMember(Name = "lastName", EmitDefaultValue = false)]
    public string? LastName { get; set; }

    [DataMember(Name = "company", EmitDefaultValue = false)]
    public string? Company { get; set; }

    [DataMember(Name = "address", EmitDefaultValue = false)]
    public string? Address { get; set; }

    [DataMember(Name = "city", EmitDefaultValue = false)]
    public string? City { get; set; }

    [DataMember(Name = "state", EmitDefaultValue = false)]
    public string? State { get; set; }

    [DataMember(Name = "zip", EmitDefaultValue = false)]
    public string? Zip { get; set; }

    [DataMember(Name = "country", EmitDefaultValue = false)]
    public string? Country { get; set; }
}
