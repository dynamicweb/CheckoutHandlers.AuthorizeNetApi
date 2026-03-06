using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class BatchStatisticType
{
    [DataMember(Name = "accountType", EmitDefaultValue = false)]
    public AccountTypeEnum? AccountType { get; set; }

    [DataMember(Name = "chargeAmount", EmitDefaultValue = false)]
    public double ChargeAmount { get; set; }

    [DataMember(Name = "chargeCount", EmitDefaultValue = false)]
    public int ChargeCount { get; set; }

    [DataMember(Name = "refundAmount", EmitDefaultValue = false)]
    public double RefundAmount { get; set; }

    [DataMember(Name = "refundCount", EmitDefaultValue = false)]
    public int RefundCount { get; set; }

    [DataMember(Name = "voidCount", EmitDefaultValue = false)]
    public int VoidCount { get; set; }

    [DataMember(Name = "declineCount", EmitDefaultValue = false)]
    public int DeclineCount { get; set; }

    [DataMember(Name = "errorCount", EmitDefaultValue = false)]
    public int ErrorCount { get; set; }

    [DataMember(Name = "returnedItemAmount", EmitDefaultValue = false)]
    public double ReturnedItemAmount { get; set; }

    [DataMember(Name = "returnedItemCount", EmitDefaultValue = false)]
    public int ReturnedItemCount { get; set; }

    [DataMember(Name = "chargebackAmount", EmitDefaultValue = false)]
    public double ChargebackAmount { get; set; }

    [DataMember(Name = "chargebackCount", EmitDefaultValue = false)]
    public int ChargebackCount { get; set; }

    [DataMember(Name = "correctionNoticeCount", EmitDefaultValue = false)]
    public int CorrectionNoticeCount { get; set; }

    [DataMember(Name = "chargeChargeBackAmount", EmitDefaultValue = false)]
    public double ChargeChargeBackAmount { get; set; }

    [DataMember(Name = "chargeChargeBackCount", EmitDefaultValue = false)]
    public int ChargeChargeBackCount { get; set; }

    [DataMember(Name = "refundChargeBackAmount", EmitDefaultValue = false)]
    public double RefundChargeBackAmount { get; set; }

    [DataMember(Name = "refundChargeBackCount", EmitDefaultValue = false)]
    public int RefundChargeBackCount { get; set; }

    [DataMember(Name = "chargeReturnedItemsAmount", EmitDefaultValue = false)]
    public double ChargeReturnedItemsAmount { get; set; }

    [DataMember(Name = "chargeReturnedItemsCount", EmitDefaultValue = false)]
    public int ChargeReturnedItemsCount { get; set; }

    [DataMember(Name = "refundReturnedItemsAmount", EmitDefaultValue = false)]
    public double RefundReturnedItemsAmount { get; set; }

    [DataMember(Name = "refundReturnedItemsCount", EmitDefaultValue = false)]
    public int RefundReturnedItemsCount { get; set; }
}
