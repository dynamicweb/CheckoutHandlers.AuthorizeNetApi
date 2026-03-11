using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class SecureAcceptance
{
    [DataMember(Name = "secureAcceptanceUrl", EmitDefaultValue = false)]
    public string? SecureAcceptanceUrl { get; set; }

    [DataMember(Name = "payerID", EmitDefaultValue = false)]
    public string? PayerID { get; set; }

    [DataMember(Name = "payerEmail", EmitDefaultValue = false)]
    public string? PayerEmail { get; set; }
}
