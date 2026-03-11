namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

/// <summary>
/// Response codes according to official Authorize.Net documentation
/// </summary>
internal static class ResponseCodes
{
    /// <summary>
    /// Transaction approved
    /// </summary>
    internal const int Approved = 1;

    /// <summary>
    /// Transaction declined
    /// </summary>
    internal const int Declined = 2;

    /// <summary>
    /// Transaction error
    /// </summary>
    internal const int Error = 3;

    /// <summary>
    /// Transaction held for review
    /// </summary>
    internal const int HeldForReview = 4;
}