﻿using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Model;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.API
{
    internal class transactionRequestType
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string transactionType { get; set; } = "";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public decimal amount { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public paymentType? payment { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public customerProfilePaymentType? profile { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string refTransId { get; set; } = "";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public orderType order { get; set; } = new();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public lineItems lineItems { get; set; } = new();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public customerDataType customer { get; set; } = new();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public customerAddressType billTo { get; set; } = new();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public nameAndAddressType shipTo { get; set; } = new();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string customerIP { get; set; } = "";
    }
}
