using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "creditCardMaskedType")]
internal sealed class CreditCardMaskedType
{
    [DataMember(Name = "cardNumber")]
    public string CardNumber { get; set; } = "";

    [DataMember(Name = "expirationDate")]
    public string ExpirationDate { get; set; } = "";

    [DataMember(Name = "cardType")]
    public CardTypeEnum CardType { get; set; }

    [DataMember(Name = "cardArt")]
    public CardArt CardArt { get; set; } = new();

    [DataMember(Name = "issuerNumber")]
    public string IssuerNumber { get; set; } = "";

    [DataMember(Name = "isPaymentToken")]
    public bool IsPaymentToken { get; set; }
}


