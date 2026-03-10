namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

internal static class SecuritySettings
{
    /// <summary>
    /// Mask for hiding card number in logs (show only last 4 digits)
    /// </summary>
    internal const string CardNumberMask = "XXXX-XXXX-XXXX-";

    /// <summary>
    /// Represents the mask value used to obscure credit card expiration dates.
    /// </summary>
    internal const string CreditCardExpirationMask = "XXXX";

    /// <summary>
    /// Mask for hiding API keys in logs
    /// </summary>
    internal const string ApiKeyMask = "***HIDDEN***";

    /// <summary>
    /// Placeholder for saved card name
    /// </summary>
    internal const string SavedCardNamePlaceholder = "NeedToSaveCardWithName:";
}