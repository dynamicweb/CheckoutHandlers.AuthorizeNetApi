using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class PayPalType
{
    [DataMember(Name = "successUrl", EmitDefaultValue = false)]
    public string? SuccessUrl { get; set; }

    [DataMember(Name = "cancelUrl", EmitDefaultValue = false)]
    public string? CancelUrl { get; set; }

    [DataMember(Name = "paypalLc", EmitDefaultValue = false)]
    public string? PaypalLc { get; set; }

    [DataMember(Name = "paypalHdrImg", EmitDefaultValue = false)]
    public string? PaypalHdrImg { get; set; }

    [DataMember(Name = "paypalPayflowcolor", EmitDefaultValue = false)]
    public string? PaypalPayflowcolor { get; set; }

    [DataMember(Name = "payerID", EmitDefaultValue = false)]
    public string? PayerID { get; set; }
}
