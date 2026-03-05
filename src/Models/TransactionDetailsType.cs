using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class TransactionDetailsType
{
    [DataMember(Name = "transId")]
    public string TransId { get; set; } = "";

    [DataMember(Name = "refTransId", EmitDefaultValue = false)]
    public string RefTransId { get; set; } = "";

    [DataMember(Name = "splitTenderId", EmitDefaultValue = false)]
    public string SplitTenderId { get; set; } = "";

    [DataMember(Name = "submitTimeUTC", EmitDefaultValue = false)]
    public DateTime SubmitTimeUTC { get; set; }

    [DataMember(Name = "submitTimeLocal", EmitDefaultValue = false)]
    public DateTime SubmitTimeLocal { get; set; }

    [DataMember(Name = "transactionType", EmitDefaultValue = false)]
    public TransactionTypeEnum? TransactionType { get; set; }

    [DataMember(Name = "transactionStatus", EmitDefaultValue = false)]
    public TransactionStatusEnum? TransactionStatus { get; set; }

    [DataMember(Name = "responseCode", EmitDefaultValue = false)]
    public int ResponseCode { get; set; }

    [DataMember(Name = "responseReasonCode", EmitDefaultValue = false)]
    public int ResponseReasonCode { get; set; }

    [DataMember(Name = "subscription", EmitDefaultValue = false)]
    public SubscriptionPaymentType Subscription { get; set; } = new();

    [DataMember(Name = "responseReasonDescription", EmitDefaultValue = false)]
    public string ResponseReasonDescription { get; set; } = "";

    [DataMember(Name = "authCode", EmitDefaultValue = false)]
    public string AuthCode { get; set; } = "";

    [DataMember(Name = "AVSResponse", EmitDefaultValue = false)]
    public string AvsResponse { get; set; } = "";

    [DataMember(Name = "cardCodeResponse", EmitDefaultValue = false)]
    public string CardCodeResponse { get; set; } = "";

    [DataMember(Name = "CAVVResponse", EmitDefaultValue = false)]
    public string CavvResponse { get; set; } = "";

    [DataMember(Name = "FDSFilterAction", EmitDefaultValue = false)]
    public FdsFilterActionEnum? FdsFilterAction { get; set; }

    [DataMember(Name = "FDSFilters", EmitDefaultValue = false)]
    public IEnumerable<FDSFilter> FdsFilters { get; set; } = [];

    [DataMember(Name = "batch", EmitDefaultValue = false)]
    public BatchDetailsType Batch { get; set; } = new();

    [DataMember(Name = "order", EmitDefaultValue = false)]
    public OrderExType Order { get; set; } = new();

    [DataMember(Name = "requestedAmount", EmitDefaultValue = false)]
    public double RequestedAmount { get; set; }

    [DataMember(Name = "authAmount", EmitDefaultValue = false)]
    public double AuthAmount { get; set; }

    [DataMember(Name = "settleAmount", EmitDefaultValue = false)]
    public double SettleAmount { get; set; }

    [DataMember(Name = "tax", EmitDefaultValue = false)]
    public ExtendedAmountType Tax { get; set; } = new();

    [DataMember(Name = "shipping", EmitDefaultValue = false)]
    public ExtendedAmountType Shipping { get; set; } = new();

    [DataMember(Name = "duty", EmitDefaultValue = false)]
    public ExtendedAmountType Duty { get; set; } = new();

    [DataMember(Name = "lineItems", EmitDefaultValue = false)]
    public IEnumerable<LineItem> LineItems { get; set; } = [];

    [DataMember(Name = "prepaidBalanceRemaining", EmitDefaultValue = false)]
    public double PrepaidBalanceRemaining { get; set; }

    [DataMember(Name = "taxExempt", EmitDefaultValue = false)]
    public bool TaxExempt { get; set; }

    [DataMember(Name = "payment", EmitDefaultValue = false)]
    public PaymentMaskedType Payment { get; set; } = new();

    [DataMember(Name = "customer", EmitDefaultValue = false)]
    public CustomerDataType Customer { get; set; } = new();

    [DataMember(Name = "billTo", EmitDefaultValue = false)]
    public CustomerAddressType BillTo { get; set; } = new();

    [DataMember(Name = "shipTo", EmitDefaultValue = false)]
    public NameAndAddressType ShipTo { get; set; } = new();

    [DataMember(Name = "recurringBilling", EmitDefaultValue = false)]
    public bool RecurringBilling { get; set; }

    [DataMember(Name = "customerIP", EmitDefaultValue = false)]
    public string CustomerIp { get; set; } = "";

    [DataMember(Name = "product", EmitDefaultValue = false)]
    public string Product { get; set; } = "";

    [DataMember(Name = "entryMode", EmitDefaultValue = false)]
    public string EntryMode { get; set; } = "";

    [DataMember(Name = "marketType", EmitDefaultValue = false)]
    public string MarketType { get; set; } = "";

    [DataMember(Name = "mobileDeviceId", EmitDefaultValue = false)]
    public string MobileDeviceId { get; set; } = "";

    [DataMember(Name = "customerSignature", EmitDefaultValue = false)]
    public string CustomerSignature { get; set; } = "";

    [DataMember(Name = "returnedItems", EmitDefaultValue = false)]
    public IEnumerable<ReturnedItem> ReturnedItems { get; set; } = [];

    [DataMember(Name = "solution", EmitDefaultValue = false)]
    public SolutionType Solution { get; set; } = new();

    [DataMember(Name = "profile", EmitDefaultValue = false)]
    public CustomerProfileIdType Profile { get; set; } = new();

    [DataMember(Name = "surcharge", EmitDefaultValue = false)]
    public ExtendedAmountType Surcharge { get; set; } = new();

    [DataMember(Name = "employeeId", EmitDefaultValue = false)]
    public string EmployeeId { get; set; } = "";

    [DataMember(Name = "tip", EmitDefaultValue = false)]
    public ExtendedAmountType Tip { get; set; } = new();

    [DataMember(Name = "otherTax", EmitDefaultValue = false)]
    public OtherTaxType OtherTax { get; set; } = new();

    [DataMember(Name = "shipFrom", EmitDefaultValue = false)]
    public NameAndAddressType ShipFrom { get; set; } = new();

    [DataMember(Name = "networkTransId", EmitDefaultValue = false)]
    public string NetworkTransId { get; set; } = "";

    [DataMember(Name = "originalNetworkTransId", EmitDefaultValue = false)]
    public string OriginalNetworkTransId { get; set; } = "";

    [DataMember(Name = "originalAuthAmount", EmitDefaultValue = false)]
    public double OriginalAuthAmount { get; set; }

    [DataMember(Name = "authorizationIndicator", EmitDefaultValue = false)]
    public string AuthorizationIndicator { get; set; } = "";
}