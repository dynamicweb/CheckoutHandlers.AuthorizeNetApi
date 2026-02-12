using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "otherTaxType")]
internal sealed class OtherTaxType
{
    [DataMember(Name = "nationalTaxAmount")]
    public double NationalTaxAmount { get; set; }

    [DataMember(Name = "localTaxAmount")]
    public double LocalTaxAmount { get; set; }

    [DataMember(Name = "alternateTaxAmount")]
    public double AlternateTaxAmount { get; set; }

    [DataMember(Name = "alternateTaxId")]
    public string AlternateTaxId { get; set; } = "";

    [DataMember(Name = "vatTaxRate")]
    public double VatTaxRate { get; set; }

    [DataMember(Name = "vatTaxAmount")]
    public double VatTaxAmount { get; set; }
}


