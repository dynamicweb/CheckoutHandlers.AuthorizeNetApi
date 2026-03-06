using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class GetTransactionDetailsResponse
{
    [DataMember(Name = "transaction")]
    public TransactionDetailsType Transaction { get; set; } = new();

    [DataMember(Name = "messages", EmitDefaultValue = false)]
    public MessagesType? Messages { get; set; }
}
