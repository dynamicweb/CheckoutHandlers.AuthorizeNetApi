using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "error")]
internal sealed class Error
{
    [DataMember(Name = "errorCode")]
    public string ErrorCode { get; set; } = "";

    [DataMember(Name = "errorText")]
    public string ErrorText { get; set; } = "";
}


