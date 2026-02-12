using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "payPalType")]
internal sealed class PayPalType
{
    [DataMember(Name = "successUrl")]
    public string SuccessUrl { get; set; } = "";

    [DataMember(Name = "cancelUrl")]
    public string CancelUrl { get; set; } = "";

    [DataMember(Name = "paypalLc")]
    public string PaypalLc { get; set; } = "";

    [DataMember(Name = "paypalHdrImg")]
    public string PaypalHdrImg { get; set; } = "";

    [DataMember(Name = "paypalPayflowcolor")]
    public string PaypalPayflowcolor { get; set; } = "";

    [DataMember(Name = "payerID")]
    public string PayerID { get; set; } = "";
}


