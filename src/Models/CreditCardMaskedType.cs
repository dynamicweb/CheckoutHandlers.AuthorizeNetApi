using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreditCardMaskedType
{
    [DataMember(Name = "cardNumber", EmitDefaultValue = false)]
    public string CardNumber { get; set; } = "";

    [DataMember(Name = "expirationDate", EmitDefaultValue = false)]
    public string ExpirationDate { get; set; } = "";

    [DataMember(Name = "cardType", EmitDefaultValue = false)]
    public CardTypeEnum? CardType { get; set; }

    [DataMember(Name = "cardArt", EmitDefaultValue = false)]
    public CardArt CardArt { get; set; } = new();

    [DataMember(Name = "issuerNumber", EmitDefaultValue = false)]
    public string IssuerNumber { get; set; } = "";

    [DataMember(Name = "isPaymentToken", EmitDefaultValue = false)]
    public bool IsPaymentToken { get; set; }
}