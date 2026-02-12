using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "emvResponse")]
internal sealed class EmvResponse
{
    [DataMember(Name = "tlvData")]
    public string TlvData { get; set; } = "";

    [DataMember(Name = "tags")]
    public EmvTag Tags { get; set; } = new();
}


