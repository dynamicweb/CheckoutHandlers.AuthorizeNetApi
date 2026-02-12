using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "batchStatisticType")]
internal sealed class BatchStatisticType
{
    [DataMember(Name = "accountType")]
    public AccountTypeEnum AccountType { get; set; }

    [DataMember(Name = "chargeAmount")]
    public double ChargeAmount { get; set; }

    [DataMember(Name = "chargeCount")]
    public int ChargeCount { get; set; }

    [DataMember(Name = "refundAmount")]
    public double RefundAmount { get; set; }

    [DataMember(Name = "refundCount")]
    public int RefundCount { get; set; }

    [DataMember(Name = "voidCount")]
    public int VoidCount { get; set; }

    [DataMember(Name = "declineCount")]
    public int DeclineCount { get; set; }

    [DataMember(Name = "errorCount")]
    public int ErrorCount { get; set; }

    [DataMember(Name = "returnedItemAmount")]
    public double ReturnedItemAmount { get; set; }

    [DataMember(Name = "returnedItemCount")]
    public int ReturnedItemCount { get; set; }

    [DataMember(Name = "chargebackAmount")]
    public double ChargebackAmount { get; set; }

    [DataMember(Name = "chargebackCount")]
    public int ChargebackCount { get; set; }

    [DataMember(Name = "correctionNoticeCount")]
    public int CorrectionNoticeCount { get; set; }

    [DataMember(Name = "chargeChargeBackAmount")]
    public double ChargeChargeBackAmount { get; set; }

    [DataMember(Name = "chargeChargeBackCount")]
    public int ChargeChargeBackCount { get; set; }

    [DataMember(Name = "refundChargeBackAmount")]
    public double RefundChargeBackAmount { get; set; }

    [DataMember(Name = "refundChargeBackCount")]
    public int RefundChargeBackCount { get; set; }

    [DataMember(Name = "chargeReturnedItemsAmount")]
    public double ChargeReturnedItemsAmount { get; set; }

    [DataMember(Name = "chargeReturnedItemsCount")]
    public int ChargeReturnedItemsCount { get; set; }

    [DataMember(Name = "refundReturnedItemsAmount")]
    public double RefundReturnedItemsAmount { get; set; }

    [DataMember(Name = "refundReturnedItemsCount")]
    public int RefundReturnedItemsCount { get; set; }
}


