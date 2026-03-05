using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreateTransactionRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();
       
    [DataMember(Name = "refId", EmitDefaultValue = false)]
    public string RefId { get; set; } = "";

    [DataMember(Name = "transactionRequest")]
    public TransactionRequestType TransactionRequest { get; set; } = new();
}