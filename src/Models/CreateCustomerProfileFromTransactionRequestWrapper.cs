using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreateCustomerProfileFromTransactionRequestWrapper
{
    [DataMember(Name = "createCustomerProfileFromTransactionRequest")]
    public CreateCustomerProfileFromTransactionRequest CreateCustomerProfileFromTransactionRequest { get; set; } = new();
}


