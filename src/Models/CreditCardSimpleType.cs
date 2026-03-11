using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CreditCardSimpleType
{
    [DataMember(Name = "cardNumber")]
    public string CardNumber { get; set; } = "";

    [DataMember(Name = "expirationDate")]
    public string ExpirationDate { get; set; } = "";
}