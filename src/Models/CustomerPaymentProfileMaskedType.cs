using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerPaymentProfileMaskedType")]
internal sealed class CustomerPaymentProfileMaskedType
{
    [DataMember(Name = "customerType")]
    public CustomerTypeEnum CustomerType { get; set; }

    [DataMember(Name = "billTo")]
    public CustomerAddressType BillTo { get; set; } = new();

    [DataMember(Name = "customerProfileId")]
    public string CustomerProfileId { get; set; } = "";

    [DataMember(Name = "customerPaymentProfileId")]
    public string CustomerPaymentProfileId { get; set; } = "";

    [DataMember(Name = "defaultPaymentProfile")]
    public bool DefaultPaymentProfile { get; set; }

    [DataMember(Name = "payment")]
    public PaymentMaskedType Payment { get; set; } = new();

    [DataMember(Name = "driversLicense")]
    public DriversLicenseMaskedType DriversLicense { get; set; } = new();

    [DataMember(Name = "taxId")]
    public string TaxId { get; set; } = "";

    [DataMember(Name = "subscriptionIds")]
    public IEnumerable<string> SubscriptionIds { get; set; } = [];

    [DataMember(Name = "originalNetworkTransId")]
    public string OriginalNetworkTransId { get; set; } = "";

    [DataMember(Name = "originalAuthAmount")]
    public double OriginalAuthAmount { get; set; }
}


