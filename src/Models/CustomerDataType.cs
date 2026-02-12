using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerDataType")]
internal sealed class CustomerDataType
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = "";

    [DataMember(Name = "email")]
    public string Email { get; set; } = "";
}


