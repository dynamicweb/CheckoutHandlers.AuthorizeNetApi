using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Security helper for protecting sensitive data
/// </summary>
internal static class SecurityHelper
{
    /// <summary>
    /// Masks credit card number for logging
    /// </summary>
    public static string MaskCreditCardNumber(string? cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber))
            return "";

        var cleaned = Regex.Replace(cardNumber, @"[\s\-]", "");

        if (cleaned.Length < 4)
            return new string('X', cleaned.Length);

        if (cleaned.Length <= 8)
            return new string('X', cleaned.Length - 4) + cleaned[^4..];

        // Show only last 4 digits (PCI DSS compliance)
        return SecuritySettings.CardNumberMask + cleaned[^4..];
    }

    /// <summary>
    /// Masks API keys for logging
    /// </summary>
    public static string MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return "";

        if (apiKey.Length <= 4)
            return SecuritySettings.ApiKeyMask;

        // Show first 2 and last 2 characters
        return apiKey[..2] + SecuritySettings.ApiKeyMask + apiKey[^2..];
    }

    /// <summary>
    /// Masks sensitive data in JSON for logging
    /// Only masks financially critical data: card numbers, CVV codes, and API keys
    /// </summary>
    public static string MaskSensitiveDataInContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return "";

        var masked = content;

        // Mask card numbers (JSON format)
        masked = Regex.Replace(masked, @"(cardNumber[""']?[:\s=]+[""']?)(\d{13,19})([""']?)",
            match => match.Groups[1].Value + MaskCreditCardNumber(match.Groups[2].Value) + match.Groups[3].Value,
            RegexOptions.IgnoreCase);

        // Mask CVV/CVC codes (JSON format)
        masked = Regex.Replace(masked, @"(cardCode[""']?[:\s=]+[""']?)(\d{3,4})([""']?)",
            match => match.Groups[1].Value + "XXX" + match.Groups[3].Value,
            RegexOptions.IgnoreCase);

        // Mask API transaction keys
        masked = Regex.Replace(masked, @"(transactionKey[""']?[:\s=]+[""']?)([A-Za-z0-9]+)([""']?)",
            match => match.Groups[1].Value + MaskApiKey(match.Groups[2].Value) + match.Groups[3].Value,
            RegexOptions.IgnoreCase);

        // Mask passwords
        masked = Regex.Replace(masked, @"(password[""']?[:\s=]+[""']?)([^""'\s]+)([""']?)",
            match => match.Groups[1].Value + "***HIDDEN***" + match.Groups[3].Value,
            RegexOptions.IgnoreCase);

        return masked;
    }

    /// <summary>
    /// Validates security configuration settings
    /// </summary>
    public static SecurityValidationResult ValidateSecurityConfiguration(
        string apiLoginId,
        string transactionKey,
        string signatureKey,
        bool isTestMode)
    {
        var issues = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(apiLoginId))
            issues.Add("API Login ID is required");

        if (string.IsNullOrWhiteSpace(transactionKey))
            issues.Add("Transaction Key is required");

        if (string.IsNullOrWhiteSpace(signatureKey))
            issues.Add("Signature Key is required");

        return new SecurityValidationResult
        {
            IsValid = !issues.Any(),
            SecurityIssues = issues,
            SecurityWarnings = warnings
        };
    }
}