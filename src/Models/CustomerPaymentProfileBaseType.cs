using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class CustomerPaymentProfileBaseType
{
    [DataMember(Name = "customerType", EmitDefaultValue = false)]
    public CustomerTypeEnum? CustomerType { get; set; }

    [DataMember(Name = "billTo", EmitDefaultValue = false)]
    public CustomerAddressType BillTo { get; set; } = new();
}