using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "paymentEmvType")]
internal sealed class PaymentEmvType
{
    [DataMember(Name = "emvData")]
    public string EmvData { get; set; } = "";

    [DataMember(Name = "emvDescriptor")]
    public string EmvDescriptor { get; set; } = "";

    [DataMember(Name = "emvVersion")]
    public string EmvVersion { get; set; } = "";
}


