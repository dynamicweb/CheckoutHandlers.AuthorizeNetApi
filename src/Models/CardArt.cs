using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "cardArt")]
internal sealed class CardArt
{
    [DataMember(Name = "cardBrand")]
    public string CardBrand { get; set; } = "";

    [DataMember(Name = "cardImageHeight")]
    public string CardImageHeight { get; set; } = "";

    [DataMember(Name = "cardImageUrl")]
    public string CardImageUrl { get; set; } = "";

    [DataMember(Name = "cardImageWidth")]
    public string CardImageWidth { get; set; } = "";

    [DataMember(Name = "cardType")]
    public string CardType { get; set; } = "";
}


