using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class GetTransactionDetailsRequestWrapper
{
    [DataMember(Name = "getTransactionDetailsRequest")]
    public GetTransactionDetailsRequest GetTransactionDetailsRequest { get; set; } = new();
}


