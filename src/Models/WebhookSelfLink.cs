using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Self link in webhook response
/// </summary>
[DataContract]
internal sealed class WebhookSelfLink
{
    /// <summary>
    /// HREF to the webhook resource
    /// </summary>
    [DataMember(Name = "href", EmitDefaultValue = false)]
    public string Href { get; set; } = "";
}