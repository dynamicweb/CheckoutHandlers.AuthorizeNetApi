using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "opaqueDataType")]
internal sealed class OpaqueDataType
{
    [DataMember(Name = "dataDescriptor")]
    public string DataDescriptor { get; set; } = "";

    [DataMember(Name = "dataValue")]
    public string DataValue { get; set; } = "";
}


