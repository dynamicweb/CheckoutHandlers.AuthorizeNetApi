using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "prePaidCard")]
internal sealed class PrePaidCard
{
    [DataMember(Name = "requestedAmount")]
    public string RequestedAmount { get; set; } = "";

    [DataMember(Name = "approvedAmount")]
    public string ApprovedAmount { get; set; } = "";

    [DataMember(Name = "balanceOnCard")]
    public string BalanceOnCard { get; set; } = "";
}


