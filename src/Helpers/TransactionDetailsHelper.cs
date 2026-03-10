using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal static class TransactionDetailsHelper
{
    public static string GetResponseTextByCode(string? responseCode)
    {
        if (!int.TryParse(responseCode, out int code))
            return "Unknown";

        return GetResponseTextByCode(code);
    }

    public static string GetResponseTextByCode(int responseCode) => responseCode switch
    {
        ResponseCodes.Approved => "Approved",
        ResponseCodes.Declined => "Declined",
        ResponseCodes.Error => "Error",
        ResponseCodes.HeldForReview => "Held for Review",
        _ => "Unknown"
    };

    internal static string GetTransactionStatus(TransactionStatusEnum? status) => status switch
    {
        TransactionStatusEnum.AuthorizedPendingCapture => "Authorized, pending capture",
        TransactionStatusEnum.CapturedPendingSettlement => "Captured, pending settlement",
        TransactionStatusEnum.CommunicationError => "Communication error",
        TransactionStatusEnum.RefundSettledSuccessfully => "Refund settled successfully",
        TransactionStatusEnum.RefundPendingSettlement => "Refund pending settlement",
        TransactionStatusEnum.ApprovedReview => "Approved review",
        TransactionStatusEnum.Declined => "Declined",
        TransactionStatusEnum.CouldNotVoid => "Could not void",
        TransactionStatusEnum.Expired => "Expired",
        TransactionStatusEnum.GeneralError => "General error",
        TransactionStatusEnum.PendingFinalSettlement => "Pending final settlement",
        TransactionStatusEnum.PendingSettlement => "Pending settlement",
        TransactionStatusEnum.FailedReview => "Failed review",
        TransactionStatusEnum.SettledSuccessfully => "Settled successfully",
        TransactionStatusEnum.SettlementError => "Settlement error",
        TransactionStatusEnum.UnderReview => "Under review",
        TransactionStatusEnum.UpdatingSettlement => "Updating settlement",
        TransactionStatusEnum.Voided => "Voided",
        TransactionStatusEnum.FDSPendingReview => "FDS pending review",
        TransactionStatusEnum.FDSAuthorizedPendingReview => "FDS authorized, pending review",
        TransactionStatusEnum.ReturnedItem => "Returned item",
        TransactionStatusEnum.Chargeback => "Chargeback",
        TransactionStatusEnum.ChargebackReversal => "Chargeback reversal",
        TransactionStatusEnum.AuthorizedPendingRelease => "Authorized, pending release",
        _ => "Unknown"
    };
}
