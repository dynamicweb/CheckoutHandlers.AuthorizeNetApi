using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "notificationPayload")]
public sealed class NotificationPayload
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = "";

    [DataMember(Name = "responseCode")]
    public int ResponseCode { get; set; }

    [DataMember(Name = "authCode")]
    public string AuthCode { get; set; } = "";

    [DataMember(Name = "avsResponse")]
    public string AvsResponse { get; set; } = "";

    [DataMember(Name = "authAmount")]
    public double Amount { get; set; }

    [DataMember(Name = "invoiceNumber")]
    public string OrderId { get; set; } = "";

    [DataMember(Name = "entityName")]
    public string Name { get; set; } = "";
}


