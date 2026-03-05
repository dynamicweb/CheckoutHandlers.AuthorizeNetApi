using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class EmvResponse
{
    [DataMember(Name = "tlvData", EmitDefaultValue = false)]
    public string TlvData { get; set; } = "";

    [DataMember(Name = "tags", EmitDefaultValue = false)]
    public EmvTag Tags { get; set; } = new();
}