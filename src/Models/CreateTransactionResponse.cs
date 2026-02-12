using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "createTransactionResponse")]
internal sealed class CreateTransactionResponse
{
    [DataMember(Name = "transactionResponse")]
    public TransactionResponse TransactionResponse { get; set; } = new();

    [DataMember(Name = "messages")]
    public MessagesType Messages { get; set; } = new();
}


