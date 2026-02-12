using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class GetCustomerProfileRequestWrapper
{
    [DataMember(Name = "getCustomerProfileRequest")]
    public GetCustomerProfileRequest GetCustomerProfileRequest { get; set; } = new();
}


