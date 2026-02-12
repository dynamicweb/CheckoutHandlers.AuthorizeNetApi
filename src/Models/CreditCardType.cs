using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "creditCardType")]
internal sealed class CreditCardType
{
    [DataMember(Name = "cardNumber")]
    public string CardNumber { get; set; } = "";

    [DataMember(Name = "expirationDate")]
    public string ExpirationDate { get; set; } = "";
}


