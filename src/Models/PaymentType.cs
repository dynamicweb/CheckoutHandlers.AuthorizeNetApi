using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "paymentType")]
internal sealed class PaymentType
{
    [DataMember(Name = "opaqueData")]
    public OpaqueDataType OpaqueData { get; set; } = new();

    [DataMember(Name = "creditCard")]
    public CreditCardType CreditCard { get; set; } = new();

    [DataMember(Name = "bankAccount")]
    public string BankAccount { get; set; } = "";

    [DataMember(Name = "trackData")]
    public string TrackData { get; set; } = "";

    [DataMember(Name = "encryptedTrackData")]
    public string EncryptedTrackData { get; set; } = "";

    [DataMember(Name = "payPal")]
    public PayPalType PayPal { get; set; } = new();

    [DataMember(Name = "emv")]
    public PaymentEmvType Emv { get; set; } = new();

    [DataMember(Name = "dataSource")]
    public string DataSource { get; set; } = "";

    [DataMember(Name = "paymentMaskedType")]
    public PaymentMaskedType PaymentMaskedType { get; set; } = new();

    [DataMember(Name = "tokenMaskedType")]
    public TokenMaskedType TokenMaskedType { get; set; } = new();

    [DataMember(Name = "tokenRequestorId")]
    public string TokenRequestorId { get; set; } = "";
}


