using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class NotificationPayload
{
    [DataMember(Name = "id")]
    public string Id { get; set; } = "";

    [DataMember(Name = "responseCode", EmitDefaultValue = false)]
    public int ResponseCode { get; set; }

    [DataMember(Name = "authCode", EmitDefaultValue = false)]
    public string? AuthCode { get; set; }

    [DataMember(Name = "avsResponse", EmitDefaultValue = false)]
    public string? AvsResponse { get; set; }

    private double _amount;

    [DataMember(Name = "authAmount", EmitDefaultValue = false)]
    public double Amount
    {
        get => _amount;
        set => _amount = AmountHelper.AdjustAmount(value);
    }

    [DataMember(Name = "invoiceNumber", EmitDefaultValue = false)]
    public string? OrderId { get; set; }

    [DataMember(Name = "entityName", EmitDefaultValue = false)]
    public string? Name { get; set; }

    [DataMember(Name = "eventType", EmitDefaultValue = false)]
    public string? EventType { get; set; }
}
