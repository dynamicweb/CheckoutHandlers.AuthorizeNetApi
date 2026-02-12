using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "getTransactionDetailsResponse")]
internal sealed class GetTransactionDetailsResponse
{
    [DataMember(Name = "transaction")]
    public TransactionDetailsType Transaction { get; set; } = new();

    [DataMember(Name = "messages")]
    public MessagesType Messages { get; set; } = new();
}


