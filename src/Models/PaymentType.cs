using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class PaymentType
{
    [DataMember(Name = "creditCard", EmitDefaultValue = false)]
    public CreditCardType? CreditCard { get; set; }

    [DataMember(Name = "trackData", EmitDefaultValue = false)]
    public string? TrackData { get; set; }

    [DataMember(Name = "encryptedTrackData", EmitDefaultValue = false)]
    public string? EncryptedTrackData { get; set; }

    [DataMember(Name = "payPal", EmitDefaultValue = false)]
    public PayPalType? PayPal { get; set; }

    [DataMember(Name = "emv", EmitDefaultValue = false)]
    public PaymentEmvType? Emv { get; set; }

    [DataMember(Name = "dataSource", EmitDefaultValue = false)]
    public string? DataSource { get; set; }

    [DataMember(Name = "paymentMaskedType", EmitDefaultValue = false)]
    public PaymentMaskedType? PaymentMaskedType { get; set; }

    [DataMember(Name = "tokenMaskedType", EmitDefaultValue = false)]
    public TokenMaskedType? TokenMaskedType { get; set; }

    [DataMember(Name = "tokenRequestorId", EmitDefaultValue = false)]
    public string? TokenRequestorId { get; set; }
}
