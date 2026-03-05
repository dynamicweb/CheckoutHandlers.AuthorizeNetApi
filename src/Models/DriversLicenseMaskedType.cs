using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class DriversLicenseMaskedType
{
    [DataMember(Name = "number", EmitDefaultValue = false)]
    public string Number { get; set; } = "";

    [DataMember(Name = "state", EmitDefaultValue = false)]
    public string State { get; set; } = "";

    [DataMember(Name = "dateOfBirth", EmitDefaultValue = false)]
    public string DateOfBirth { get; set; } = "";
}