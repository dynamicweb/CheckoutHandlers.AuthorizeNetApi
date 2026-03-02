namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

internal static class PreparedMessages
{
    internal const string CaptureSuccessMessage = "Capture successful";
    internal const string RefundSuccessMessage = "Authorize.Net has full refunded payment.";

    internal const string OrderNotSetMessage = "Order not set";
    internal const string OrderIdNotSetMessage = "Order id not set";

    internal const string TransactionNumberNotSetMessage = "Transaction number not set";
    internal const string TransactionNumberRequiredMessage = "Transaction number is required";

    internal const string UnexpectedErrorMessage = "Unexpected error during operation";
}
