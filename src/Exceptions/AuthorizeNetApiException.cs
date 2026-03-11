using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Exceptions;

/// <summary>
/// Exception thrown when Authorize.Net API returns an error response
/// </summary>
internal sealed class AuthorizeNetApiException : Exception
{
    public string? ErrorCode { get; }
    public ErrorResponse? ErrorResponse { get; }

    public AuthorizeNetApiException(string message) : base(message) { }

    public AuthorizeNetApiException(string message, ErrorResponse errorResponse) : base(message)
    {
        ErrorResponse = errorResponse;
        if (errorResponse.Messages?.Message?.Length > 0)
        {
            ErrorCode = errorResponse.Messages.Message[0].Code;
        }
    }

    public AuthorizeNetApiException(string message, Exception innerException)
        : base(message, innerException) { }
}