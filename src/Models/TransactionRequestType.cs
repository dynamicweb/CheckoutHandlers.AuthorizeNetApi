using System.Runtime.Serialization;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class TransactionRequestType
{
    private double _amount;

    [DataMember(Name = "transactionType")]
    public TransactionTypeEnum TransactionType { get; set; }

    [DataMember(Name = "amount", EmitDefaultValue = false)]
    public double Amount 
    { 
        get => _amount;
        set => _amount = AmountHelper.AdjustAmount(value);
    }

    [DataMember(Name = "currencyCode", EmitDefaultValue = false)]
    public string CurrencyCode { get; set; } = "";

    [DataMember(Name = "payment", EmitDefaultValue = false)]
    public PaymentType? Payment { get; set; }

    [DataMember(Name = "profile", EmitDefaultValue = false)]
    public CustomerProfilePaymentType? Profile { get; set; }

    [DataMember(Name = "refTransId", EmitDefaultValue = false)]
    public string RefTransId { get; set; } = "";

    [DataMember(Name = "order", EmitDefaultValue = false)]
    public OrderType? Order { get; set; }

    [DataMember(Name = "lineItems", EmitDefaultValue = false)]
    public LineItems? LineItems { get; set; }

    [DataMember(Name = "customer", EmitDefaultValue = false)]
    public CustomerDataType? Customer { get; set; }

    [DataMember(Name = "billTo", EmitDefaultValue = false)]
    public CustomerAddressType? BillTo { get; set; }

    [DataMember(Name = "shipTo", EmitDefaultValue = false)]
    public NameAndAddressType? ShipTo { get; set; }

    [DataMember(Name = "customerIP", EmitDefaultValue = false)]
    public string CustomerIp { get; set; } = "";

    [DataMember(Name = "processingOptions", EmitDefaultValue = false)]
    public ProcessingOptionsType? ProcessingOptions { get; set; }

    [DataMember(Name = "subsequentAuthInformation", EmitDefaultValue = false)]
    public SubsequentAuthInformationType? SubsequentAuthInformation { get; set; }
}