using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Standard error response from Authorize.Net API
/// </summary>
[DataContract]
internal sealed class ErrorResponse
{
    [DataMember(Name = "refId", EmitDefaultValue = false)]
    public string? RefId { get; set; }

    [DataMember(Name = "messages", EmitDefaultValue = false)]
    public Messages? Messages { get; set; }
}