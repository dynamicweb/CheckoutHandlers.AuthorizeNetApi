using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class SplitTenderPayment
{
    [DataMember(Name = "transId")]
    public string TransId { get; set; } = "";

    [DataMember(Name = "responseCode", EmitDefaultValue = false)]
    public string ResponseCode { get; set; } = "";

    [DataMember(Name = "responseToCustomer", EmitDefaultValue = false)]
    public string ResponseToCustomer { get; set; } = "";

    [DataMember(Name = "authCode", EmitDefaultValue = false)]
    public string AuthCode { get; set; } = "";

    [DataMember(Name = "accountNumber", EmitDefaultValue = false)]
    public string AccountNumber { get; set; } = "";

    [DataMember(Name = "accountType", EmitDefaultValue = false)]
    public string AccountType { get; set; } = "";

    [DataMember(Name = "requestedAmount", EmitDefaultValue = false)]
    public string RequestedAmount { get; set; } = "";

    [DataMember(Name = "approvedAmount", EmitDefaultValue = false)]
    public string ApprovedAmount { get; set; } = "";

    [DataMember(Name = "balanceOnCard", EmitDefaultValue = false)]
    public string BalanceOnCard { get; set; } = "";
}