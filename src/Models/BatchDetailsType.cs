using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class BatchDetailsType
{
    [DataMember(Name = "batchId", EmitDefaultValue = false)]
    public string BatchId { get; set; } = "";

    [DataMember(Name = "settlementTimeUTC", EmitDefaultValue = false)]
    public string SettlementTimeUTC { get; set; } = "";

    [DataMember(Name = "settlementTimeLocal", EmitDefaultValue = false)]
    public string SettlementTimeLocal { get; set; } = "";

    [DataMember(Name = "settlementState", EmitDefaultValue = false)]
    public SettlementStateEnum? SettlementState { get; set; }

    [DataMember(Name = "paymentMethod", EmitDefaultValue = false)]
    public PaymentMethodEnum? PaymentMethod { get; set; }

    [DataMember(Name = "marketType", EmitDefaultValue = false)]
    public string MarketType { get; set; } = "";

    [DataMember(Name = "product", EmitDefaultValue = false)]
    public string Product { get; set; } = "";

    [DataMember(Name = "statistics", EmitDefaultValue = false)]
    public IEnumerable<BatchStatisticType> Statistics { get; set; } = [];
}


