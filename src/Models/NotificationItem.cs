using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
public partial class NotificationItem
{
    [DataMember(Name = "notificationId", EmitDefaultValue = false)]
    public string Id { get; set; } = "";

    [DataMember(Name = "eventType", EmitDefaultValue = false)]
    public string EventType { get; set; } = "";

    [DataMember(Name = "eventDate", EmitDefaultValue = false)]
    public string EventDate { get; set; } = "";

    [DataMember(Name = "webhookId", EmitDefaultValue = false)]
    public string WebhookId { get; set; } = "";

    [DataMember(Name = "payload", EmitDefaultValue = false)]
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