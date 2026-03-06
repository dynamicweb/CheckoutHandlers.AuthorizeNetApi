using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class PaymentEmvType
{
    [DataMember(Name = "emvData", EmitDefaultValue = false)]
    public string? EmvData { get; set; }

    [DataMember(Name = "emvDescriptor", EmitDefaultValue = false)]
    public string? EmvDescriptor { get; set; }

    [DataMember(Name = "emvVersion", EmitDefaultValue = false)]
    public string? EmvVersion { get; set; }
}
