using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "driversLicenseMaskedType")]
internal sealed class DriversLicenseMaskedType
{
    [DataMember(Name = "number")]
    public string Number { get; set; } = "";

    [DataMember(Name = "state")]
    public string State { get; set; } = "";

    [DataMember(Name = "dateOfBirth")]
    public string DateOfBirth { get; set; } = "";
}


