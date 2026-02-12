using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "processingOptionsType")]
internal sealed class ProcessingOptionsType
{
    [DataMember(Name = "isSubsequentAuth")]
    public bool IsSubsequentAuth { get; set; }

    [DataMember(Name = "isStoredCredentials")]
    public bool IsStoredCredentials { get; set; }
}

