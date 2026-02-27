namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

/// <summary>
/// Security settings
/// </summary>
internal static class SecuritySettings
{
    /// <summary>
    /// Mask for hiding card number in logs (show only last 4 digits)
    /// </summary>
    internal const string CardNumberMask = "XXXX-XXXX-XXXX-";

    /// <summary>
    /// Mask for hiding API keys in logs
    /// </summary>
    internal const string ApiKeyMask = "***HIDDEN***";

    /// <summary>
    /// Minimum length for secure passwords/keys
    /// </summary>
    internal const int MinSecureKeyLength = 8;
}