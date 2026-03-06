using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Event type response from /eventtypes endpoint
/// </summary>
[DataContract]
internal sealed class EventTypeResponse
{
    /// <summary>
    /// Name of the event type
    /// </summary>
    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string? Name { get; set; }
}
