using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class OtherTaxType
{
    [DataMember(Name = "nationalTaxAmount", EmitDefaultValue = false)]
    public double NationalTaxAmount { get; set; }

    [DataMember(Name = "localTaxAmount", EmitDefaultValue = false)]
    public double LocalTaxAmount { get; set; }

    [DataMember(Name = "alternateTaxAmount", EmitDefaultValue = false)]
    public double AlternateTaxAmount { get; set; }

    [DataMember(Name = "alternateTaxId", EmitDefaultValue = false)]
    public string? AlternateTaxId { get; set; }

    [DataMember(Name = "vatTaxRate", EmitDefaultValue = false)]
    public double VatTaxRate { get; set; }

    [DataMember(Name = "vatTaxAmount", EmitDefaultValue = false)]
    public double VatTaxAmount { get; set; }
}
