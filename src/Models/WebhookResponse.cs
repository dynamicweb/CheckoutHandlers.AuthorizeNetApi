using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Represents a webhook response from Authorize.Net Webhooks API
/// </summary>
[DataContract]
internal sealed class WebhookResponse
{
    /// <summary>
    /// Unique identifier for the webhook
    /// </summary>
    [DataMember(Name = "webhookId", EmitDefaultValue = false)]
    public string WebhookId { get; set; } = "";

    /// <summary>
    /// Name of the webhook
    /// </summary>
    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Array of event types the webhook is subscribed to
    /// </summary>
    [DataMember(Name = "eventTypes", EmitDefaultValue = false)]
    public string[] EventTypes { get; set; } = [];

    /// <summary>
    /// Current status of the webhook
    /// </summary>
    [DataMember(Name = "status", EmitDefaultValue = false)]
    public string Status { get; set; } = "";

    /// <summary>
    /// URL where webhook notifications are sent
    /// </summary>
    [DataMember(Name = "url", EmitDefaultValue = false)]
    public string Url { get; set; } = "";

    /// <summary>
    /// Links section from response
    /// </summary>
    [DataMember(Name = "_links", EmitDefaultValue = false)]
    public WebhookLinks? Links { get; set; }
}