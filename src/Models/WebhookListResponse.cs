namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Response containing list of webhooks
/// </summary>
internal sealed class WebhookListResponse
{
    /// <summary>
    /// Array of webhooks (NOTE: Response is direct array, not wrapped in object)
    /// </summary>
    public WebhookResponse[] Webhooks { get; set; } = [];
}