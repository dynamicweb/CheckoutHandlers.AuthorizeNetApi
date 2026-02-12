using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "transactionDetailsType")]
internal sealed class TransactionDetailsType
{
    [DataMember(Name = "transId")]
    public string TransId { get; set; } = "";

    [DataMember(Name = "refTransId")]
    public string RefTransId { get; set; } = "";

    [DataMember(Name = "splitTenderId")]
    public string SplitTenderId { get; set; } = "";

    [DataMember(Name = "submitTimeUTC")]
    public DateTime SubmitTimeUTC { get; set; }

    [DataMember(Name = "submitTimeLocal")]
    public DateTime SubmitTimeLocal { get; set; }

    [DataMember(Name = "transactionType")]
    public TransactionTypeEnum TransactionType { get; set; }

    [DataMember(Name = "transactionStatus")]
    public TransactionStatusEnum TransactionStatus { get; set; }

    [DataMember(Name = "responseCode")]
    public int ResponseCode { get; set; }

    [DataMember(Name = "responseReasonCode")]
    public int ResponseReasonCode { get; set; }

    [DataMember(Name = "subscription")]
    public SubscriptionPaymentType Subscription { get; set; } = new();

    [DataMember(Name = "responseReasonDescription")]
    public string ResponseReasonDescription { get; set; } = "";

    [DataMember(Name = "authCode")]
    public string AuthCode { get; set; } = "";

    [DataMember(Name = "AVSResponse")]
    public string AvsResponse { get; set; } = "";

    [DataMember(Name = "cardCodeResponse")]
    public string CardCodeResponse { get; set; } = "";

    [DataMember(Name = "CAVVResponse")]
    public string CavvResponse { get; set; } = "";

    [DataMember(Name = "FDSFilterAction")]
    public FdsFilterActionEnum FdsFilterAction { get; set; }

    [DataMember(Name = "FDSFilters")]
    public IEnumerable<FDSFilter> FdsFilters { get; set; } = [];

    [DataMember(Name = "batch")]
    public BatchDetailsType Batch { get; set; } = new();

    [DataMember(Name = "order")]
    public OrderExType Order { get; set; } = new();

    [DataMember(Name = "requestedAmount")]
    public double RequestedAmount { get; set; }

    [DataMember(Name = "authAmount")]
    public double AuthAmount { get; set; }

    [DataMember(Name = "settleAmount")]
    public double SettleAmount { get; set; }

    [DataMember(Name = "tax")]
    public ExtendedAmountType Tax { get; set; } = new();

    [DataMember(Name = "shipping")]
    public ExtendedAmountType Shipping { get; set; } = new();

    [DataMember(Name = "duty")]
    public ExtendedAmountType Duty { get; set; } = new();

    [DataMember(Name = "lineItems")]
    public IEnumerable<LineItem> LineItems { get; set; } = [];

    [DataMember(Name = "prepaidBalanceRemaining")]
    public double PrepaidBalanceRemaining { get; set; }

    [DataMember(Name = "taxExempt")]
    public bool TaxExempt { get; set; }

    [DataMember(Name = "payment")]
    public PaymentMaskedType Payment { get; set; } = new();

    [DataMember(Name = "customer")]
    public CustomerDataType Customer { get; set; } = new();

    [DataMember(Name = "billTo")]
    public CustomerAddressType BillTo { get; set; } = new();

    [DataMember(Name = "shipTo")]
    public NameAndAddressType ShipTo { get; set; } = new();

    [DataMember(Name = "recurringBilling")]
    public bool RecurringBilling { get; set; }

    [DataMember(Name = "customerIP")]
    public string CustomerIp { get; set; } = "";

    [DataMember(Name = "product")]
    public string Product { get; set; } = "";

    [DataMember(Name = "entryMode")]
    public string EntryMode { get; set; } = "";

    [DataMember(Name = "marketType")]
    public string MarketType { get; set; } = "";

    [DataMember(Name = "mobileDeviceId")]
    public string MobileDeviceId { get; set; } = "";

    [DataMember(Name = "customerSignature")]
    public string CustomerSignature { get; set; } = "";

    [DataMember(Name = "returnedItems")]
    public IEnumerable<ReturnedItem> ReturnedItems { get; set; } = [];

    [DataMember(Name = "solution")]
    public SolutionType Solution { get; set; } = new();

    [DataMember(Name = "profile")]
    public CustomerProfileIdType Profile { get; set; } = new();

    [DataMember(Name = "surcharge")]
    public ExtendedAmountType Surcharge { get; set; } = new();

    [DataMember(Name = "employeeId")]
    public string EmployeeId { get; set; } = "";

    [DataMember(Name = "tip")]
    public ExtendedAmountType Tip { get; set; } = new();

    [DataMember(Name = "otherTax")]
    public OtherTaxType OtherTax { get; set; } = new();

    [DataMember(Name = "shipFrom")]
    public NameAndAddressType ShipFrom { get; set; } = new();

    [DataMember(Name = "networkTransId")]
    public string NetworkTransId { get; set; } = "";

    [DataMember(Name = "originalNetworkTransId")]
    public string OriginalNetworkTransId { get; set; } = "";

    [DataMember(Name = "originalAuthAmount")]
    public double OriginalAuthAmount { get; set; }

    [DataMember(Name = "authorizationIndicator")]
    public string AuthorizationIndicator { get; set; } = "";
}

