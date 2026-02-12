using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "customerPaymentProfileBaseType")]
internal sealed class CustomerPaymentProfileBaseType
{
    [DataMember(Name = "customerType")]
    public CustomerTypeEnum CustomerType { get; set; }

    [DataMember(Name = "billTo")]
    public CustomerAddressType BillTo { get; set; } = new();
}


