using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "splitTenderPayment")]
internal sealed class SplitTenderPayment
{
    [DataMember(Name = "transId")]
    public string TransId { get; set; } = "";

    [DataMember(Name = "responseCode")]
    public string ResponseCode { get; set; } = "";

    [DataMember(Name = "responseToCustomer")]
    public string ResponseToCustomer { get; set; } = "";

    [DataMember(Name = "authCode")]
    public string AuthCode { get; set; } = "";

    [DataMember(Name = "accountNumber")]
    public string AccountNumber { get; set; } = "";

    [DataMember(Name = "accountType")]
    public string AccountType { get; set; } = "";

    [DataMember(Name = "requestedAmount")]
    public string RequestedAmount { get; set; } = "";

    [DataMember(Name = "approvedAmount")]
    public string ApprovedAmount { get; set; } = "";

    [DataMember(Name = "balanceOnCard")]
    public string BalanceOnCard { get; set; } = "";
}


