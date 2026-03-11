using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Links section in webhook response
/// </summary>
[DataContract]
internal sealed class WebhookLinks
{
    /// <summary>
    /// Self link
    /// </summary>
    [DataMember(Name = "self", EmitDefaultValue = false)]
    public WebhookSelfLink? Self { get; set; }
}