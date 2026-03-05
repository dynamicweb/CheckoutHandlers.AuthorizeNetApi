using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class ProcessingOptionsType
{
    [DataMember(Name = "isSubsequentAuth", EmitDefaultValue = false)]
    public bool IsSubsequentAuth { get; set; }

    [DataMember(Name = "isStoredCredentials", EmitDefaultValue = false)]
    public bool IsStoredCredentials { get; set; }
}