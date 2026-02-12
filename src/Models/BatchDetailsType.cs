using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "batchDetailsType")]
internal sealed class BatchDetailsType
{
    [DataMember(Name = "batchId")]
    public string BatchId { get; set; } = "";

    [DataMember(Name = "settlementTimeUTC")]
    public string SettlementTimeUTC { get; set; } = "";

    [DataMember(Name = "settlementTimeLocal")]
    public string SettlementTimeLocal { get; set; } = "";

    [DataMember(Name = "settlementState")]
    public SettlementStateEnum SettlementState { get; set; }

    [DataMember(Name = "paymentMethod")]
    public PaymentMethodEnum PaymentMethod { get; set; }

    [DataMember(Name = "marketType")]
    public string MarketType { get; set; } = "";

    [DataMember(Name = "product")]
    public string Product { get; set; } = "";

    [DataMember(Name = "statistics")]
    public IEnumerable<BatchStatisticType> Statistics { get; set; } = [];
}


