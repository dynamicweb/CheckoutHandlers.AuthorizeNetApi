using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "getHostedPaymentPageRequest")]
internal sealed class GetHostedPaymentPageRequest
{
    [DataMember(Name = "merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [DataMember(Name = "clientId")]
    public string ClientId { get; set; } = "";

    [DataMember(Name = "refId")]
    public string RefId { get; set; } = "";

    [DataMember(Name = "transactionRequest")]
    public TransactionRequestType TransactionRequest { get; set; } = new();

    [DataMember(Name = "hostedPaymentSettings")]
    public HostedPaymentSettings HostedPaymentSettings { get; set; } = new();
}




