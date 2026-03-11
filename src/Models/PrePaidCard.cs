using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class PrePaidCard
{
    [DataMember(Name = "requestedAmount", EmitDefaultValue = false)]
    public string? RequestedAmount { get; set; }

    [DataMember(Name = "approvedAmount", EmitDefaultValue = false)]
    public string? ApprovedAmount { get; set; }

    [DataMember(Name = "balanceOnCard", EmitDefaultValue = false)]
    public string? BalanceOnCard { get; set; }
}
