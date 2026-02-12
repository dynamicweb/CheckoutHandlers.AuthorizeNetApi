using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerAddressExType")]
internal sealed class CustomerAddressExType
{
    [DataMember(Name = "firstName")]
    public string FirstName { get; set; } = "";

    [DataMember(Name = "lastName")]
    public string LastName { get; set; } = "";

    [DataMember(Name = "company")]
    public string Company { get; set; } = "";

    [DataMember(Name = "address")]
    public string Address { get; set; } = "";

    [DataMember(Name = "city")]
    public string City { get; set; } = "";

    [DataMember(Name = "state")]
    public string State { get; set; } = "";

    [DataMember(Name = "zip")]
    public string Zip { get; set; } = "";

    [DataMember(Name = "country")]
    public string Country { get; set; } = "";

    [DataMember(Name = "phoneNumber")]
    public string PhoneNumber { get; set; } = "";

    [DataMember(Name = "email")]
    public string Email { get; set; } = "";

    [DataMember(Name = "customerAddressId")]
    public string CustomerAddressId { get; set; } = "";
}


