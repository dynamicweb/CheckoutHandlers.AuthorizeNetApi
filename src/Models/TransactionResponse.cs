using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract]
internal sealed class TransactionResponse
{
    [DataMember(Name = "responseCode", EmitDefaultValue = false)]
    public string? ResponseCode { get; set; }

    [DataMember(Name = "rawResponseCode", EmitDefaultValue = false)]
    public string? RawResponseCode { get; set; }

    [DataMember(Name = "authCode", EmitDefaultValue = false)]
    public string? AuthCode { get; set; }

    [DataMember(Name = "avsResultCode", EmitDefaultValue = false)]
    public string? AvsResultCode { get; set; }

    [DataMember(Name = "cvvResultCode", EmitDefaultValue = false)]
    public string? CvvResultCode { get; set; }

    [DataMember(Name = "cavvResultCode", EmitDefaultValue = false)]
    public string? CavvResultCode { get; set; }

    [DataMember(Name = "transId", EmitDefaultValue = false)]
    public string? TransId { get; set; }

    [DataMember(Name = "refTransID", EmitDefaultValue = false)]
    public string? RefTransId { get; set; }

    [DataMember(Name = "transHash", EmitDefaultValue = false)]
    public string? TransHash { get; set; }

    [DataMember(Name = "testRequest", EmitDefaultValue = false)]
    public string? TestRequest { get; set; }

    [DataMember(Name = "accountNumber", EmitDefaultValue = false)]
    public string? AccountNumber { get; set; }

    [DataMember(Name = "entryMode", EmitDefaultValue = false)]
    public string? EntryMode { get; set; }

    [DataMember(Name = "accountType", EmitDefaultValue = false)]
    public string? AccountType { get; set; }

    [DataMember(Name = "splitTenderId", EmitDefaultValue = false)]
    public string? SplitTenderId { get; set; }

    [DataMember(Name = "prePaidCard", EmitDefaultValue = false)]
    public PrePaidCard? PrePaidCard { get; set; }

    [DataMember(Name = "messages", EmitDefaultValue = false)]
    public IEnumerable<Message>? Messages { get; set; }

    [DataMember(Name = "errors", EmitDefaultValue = false)]
    public IEnumerable<Error>? Errors { get; set; }

    [DataMember(Name = "splitTenderPayments", EmitDefaultValue = false)]
    public IEnumerable<SplitTenderPayment>? SplitTenderPayments { get; set; }

    [DataMember(Name = "userFields", EmitDefaultValue = false)]
    public IEnumerable<UserField>? UserFields { get; set; }

    [DataMember(Name = "shipTo", EmitDefaultValue = false)]
    public NameAndAddressType? ShipTo { get; set; }

    [DataMember(Name = "secureAcceptance", EmitDefaultValue = false)]
    public SecureAcceptance? SecureAcceptance { get; set; }

    [DataMember(Name = "emvResponse", EmitDefaultValue = false)]
    public EmvResponse? EmvResponse { get; set; }

    [DataMember(Name = "transHashSha2", EmitDefaultValue = false)]
    public string? TransHashSha2 { get; set; }

    [DataMember(Name = "profile", EmitDefaultValue = false)]
    public CustomerProfileIdType? Profile { get; set; }

    [DataMember(Name = "networkTransId", EmitDefaultValue = false)]
    public string? NetworkTransId { get; set; }
}
