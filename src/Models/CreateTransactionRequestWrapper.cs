using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreateTransactionRequestWrapper
{
    [DataMember(Name = "createTransactionRequest")]
    public CreateTransactionRequest CreateTransactionRequest { get; set; } = new();
}