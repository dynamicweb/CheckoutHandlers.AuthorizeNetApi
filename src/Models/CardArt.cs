using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CardArt
{
    [DataMember(Name = "cardBrand", EmitDefaultValue = false)]
    public string CardBrand { get; set; } = "";

    [DataMember(Name = "cardImageHeight", EmitDefaultValue = false)]
    public string CardImageHeight { get; set; } = "";

    [DataMember(Name = "cardImageUrl", EmitDefaultValue = false)]
    public string CardImageUrl { get; set; } = "";

    [DataMember(Name = "cardImageWidth", EmitDefaultValue = false)]
    public string CardImageWidth { get; set; } = "";

    [DataMember(Name = "cardType", EmitDefaultValue = false)]
    public string CardType { get; set; } = "";
}


