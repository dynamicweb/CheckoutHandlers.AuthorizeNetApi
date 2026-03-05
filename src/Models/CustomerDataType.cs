using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CustomerDataType
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = "";

    [DataMember(Name = "email", EmitDefaultValue = false)]
    public string Email { get; set; } = "";
}