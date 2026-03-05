namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

/// <summary>
/// Webhook endpoints for REST API
/// </summary>
internal static class WebhookEndpoints
{
    /// <summary>
    /// Base endpoint for webhook operations (/rest/v1/webhooks)
    /// </summary>
    public const string Base = "/rest/v1/webhooks";

    /// <summary>
    /// Endpoint for specific webhook operations (/rest/v1/webhooks/{id})
    /// </summary>
    /// <param name="webhookId">The webhook ID</param>
    /// <returns>Webhook endpoint with ID</returns>
    public static string GetSpecific(string webhookId) => $"{Base}/{webhookId}";

    /// <summary>
    /// Endpoint for event types (/rest/v1/eventtypes)
    /// </summary>
    public const string EventTypes = "/rest/v1/eventtypes";
}