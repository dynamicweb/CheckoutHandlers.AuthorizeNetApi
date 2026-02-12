using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "notificationItem")]
public partial class NotificationItem
{
    [DataMember(Name = "notificationId")]
    public string Id { get; set; } = "";

    [DataMember(Name = "eventType")]
    public string EventType { get; set; } = "";

    [DataMember(Name = "eventDate")]
    public string EventDate { get; set; } = "";

    [DataMember(Name = "webhookId")]
    public string WebhookId { get; set; } = "";

    [DataMember(Name = "payload")]
    public NotificationPayload Payload { get; set; } = new();

    public NotificationEventType? GetEventType() => EventType switch
    {
        "net.authorize.payment.authcapture.created" => NotificationEventType.AuthCaptureCreated,
        "net.authorize.payment.authorization.created" => NotificationEventType.AuthCreated,
        "net.authorize.payment.capture.created" => NotificationEventType.CaptureCreated,
        "net.authorize.payment.priorAuthCapture.created" => NotificationEventType.PriorAuthCaptureCreated,
        "net.authorize.payment.refund.created" => NotificationEventType.RefundCreated,
        "net.authorize.payment.void.created" => NotificationEventType.VoidCreated,
        _ => null
    };
}


