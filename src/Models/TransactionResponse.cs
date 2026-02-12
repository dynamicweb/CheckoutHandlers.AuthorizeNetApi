using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[DataContract(Name = "transactionResponse")]
internal sealed class TransactionResponse
{
    [DataMember(Name = "responseCode")]
    public string ResponseCode { get; set; } = "";

    [DataMember(Name = "rawResponseCode")]
    public string RawResponseCode { get; set; } = "";

    [DataMember(Name = "authCode")]
    public string AuthCode { get; set; } = "";

    [DataMember(Name = "avsResultCode")]
    public string AvsResultCode { get; set; } = "";

    [DataMember(Name = "cvvResultCode")]
    public string CvvResultCode { get; set; } = "";

    [DataMember(Name = "cavvResultCode")]
    public string CavvResultCode { get; set; } = "";

    [DataMember(Name = "transId")]
    public string TransId { get; set; } = "";

    [DataMember(Name = "refTransID")]
    public string RefTransId { get; set; } = "";

    [DataMember(Name = "transHash")]
    public string TransHash { get; set; } = "";

    [DataMember(Name = "testRequest")]
    public string TestRequest { get; set; } = "";

    [DataMember(Name = "accountNumber")]
    public string AccountNumber { get; set; } = "";

    [DataMember(Name = "entryMode")]
    public string EntryMode { get; set; } = "";

    [DataMember(Name = "accountType")]
    public string AccountType { get; set; } = "";

    [DataMember(Name = "splitTenderId")]
    public string SplitTenderId { get; set; } = "";

    [DataMember(Name = "prePaidCard")]
    public PrePaidCard PrePaidCard { get; set; } = new();

    [DataMember(Name = "messages")]
    public IEnumerable<Message> Messages { get; set; } = [];

    [DataMember(Name = "errors")]
    public IEnumerable<Error> Errors { get; set; } = [];

    [DataMember(Name = "splitTenderPayments")]
    public IEnumerable<SplitTenderPayment> SplitTenderPayments { get; set; } = [];

    [DataMember(Name = "userFields")]
    public IEnumerable<UserField> UserFields { get; set; } = [];

    [DataMember(Name = "shipTo")]
    public NameAndAddressType ShipTo { get; set; } = new();

    [DataMember(Name = "secureAcceptance")]
    public SecureAcceptance SecureAcceptance { get; set; } = new();

    [DataMember(Name = "emvResponse")]
    public EmvResponse EmvResponse { get; set; } = new();

    [DataMember(Name = "transHashSha2")]
    public string TransHashSha2 { get; set; } = "";

    [DataMember(Name = "profile")]
    public CustomerProfileIdType Profile { get; set; } = new();

    [DataMember(Name = "networkTransId")]
    public string NetworkTransId { get; set; } = "";
}


