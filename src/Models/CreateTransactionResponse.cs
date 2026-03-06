using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreateTransactionResponse
{
    [DataMember(Name = "transactionResponse")]
    public TransactionResponse TransactionResponse { get; set; } = new();

    [DataMember(Name = "messages", EmitDefaultValue = false)]
    public MessagesType? Messages { get; set; }
}
