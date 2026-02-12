using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreateCustomerProfileRequestWrapper
{
    [DataMember(Name = "createCustomerProfileRequest")]
    public CreateCustomerProfileRequest CreateCustomerProfileRequest { get; set; } = new();
}