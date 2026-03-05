using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Represents a webhook request for Authorize.Net Webhooks API
/// </summary>
[DataContract]
internal sealed class WebhookRequest
{
    /// <summary>
    /// Name of the webhook (optional)
    /// </summary>
    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; } = "";

    /// <summary>
    /// URL to receive webhook notifications
    /// </summary>
    [DataMember(Name = "url", EmitDefaultValue = false)]
    public string Url { get; set; } = "";

    /// <summary>
    /// Array of event types to subscribe to
    /// </summary>
    [DataMember(Name = "eventTypes", EmitDefaultValue = false)]
    public string[] EventTypes { get; set; } = [];

    /// <summary>
    /// Status of the webhook (active/inactive) - optional
    /// </summary>
    [DataMember(Name = "status", EmitDefaultValue = false)]
    public string Status { get; set; } = "active";
}