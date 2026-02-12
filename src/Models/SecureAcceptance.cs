using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "secureAcceptance")]
internal sealed class SecureAcceptance
{
    [DataMember(Name = "secureAcceptanceUrl")]
    public string SecureAcceptanceUrl { get; set; } = "";

    [DataMember(Name = "payerID")]
    public string PayerID { get; set; } = "";

    [DataMember(Name = "payerEmail")]
    public string PayerEmail { get; set; } = "";
}


