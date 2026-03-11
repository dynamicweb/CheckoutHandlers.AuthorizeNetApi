using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class TokenMaskedType
{
    [DataMember(Name = "tokenSource")]
    public string TokenSource { get; set; } = "";

    [DataMember(Name = "tokenNumber")]
    public string TokenNumber { get; set; } = "";

    [DataMember(Name = "expirationDate")]
    public string ExpirationDate { get; set; } = "";

    [DataMember(Name = "tokenRequestorId")]
    public string TokenRequestorId { get; set; } = "";
}