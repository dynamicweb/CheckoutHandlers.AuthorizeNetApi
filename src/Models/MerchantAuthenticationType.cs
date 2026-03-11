using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class MerchantAuthenticationType
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = "";

    [DataMember(Name = "transactionKey")]
    public string TransactionKey { get; set; } = "";
}