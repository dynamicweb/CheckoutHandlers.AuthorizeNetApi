using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "subsequentAuthInformationType")]
internal sealed class SubsequentAuthInformationType
{
    [DataMember(Name = "originalNetworkTransId")]
    public string OriginalNetworkTransId { get; set; } = "";

    [DataMember(Name = "originalAuthAmount")]
    public double OriginalAuthAmount { get; set; }

    [DataMember(Name = "reason")]
    public string Reason { get; set; } = "";
}

