using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Standard error response from Authorize.Net API
/// </summary>
[DataContract]
internal sealed class ErrorResponse
{
    [DataMember(Name = "refId")]
    public string? RefId { get; set; }

    [DataMember(Name = "messages")]
    public Messages? Messages { get; set; }
}