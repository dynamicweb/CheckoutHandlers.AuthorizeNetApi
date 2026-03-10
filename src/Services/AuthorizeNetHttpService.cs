using Dynamicweb.Core;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Exceptions;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

/// <summary>
/// Service to send request/response to Authorize.Net API endpoints.
/// </summary>
internal sealed class AuthorizeNetHttpService
{
    private static readonly HttpClient ProductionHttpClient = CreateHttpClient(AuthorizeNetEndpoints.ProductionApiUrl);
    private static readonly HttpClient SandboxHttpClient = CreateHttpClient(AuthorizeNetEndpoints.SandboxApiUrl);

    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly bool _debugEnabled;
    private readonly AuthorizeNetLogger? _logger;

    public AuthorizeNetHttpService(bool isTestMode, bool debugEnabled, AuthorizeNetLogger? logger)
    {
        _url = AuthorizeNetEndpoints.GetApiEndpoint(isTestMode);
        _debugEnabled = debugEnabled;
        _logger = logger;
        _httpClient = isTestMode
            ? SandboxHttpClient
            : ProductionHttpClient;
    }

    public T? Post<T>(string jsonObject)
    {
        return SendRequest<T>(HttpMethod.Post, _url, jsonObject, null);
    }

    /// <summary>
    /// Sends a POST request to specified endpoint with custom headers
    /// </summary>
    public T? Post<T>(string endpoint, string jsonObject, Dictionary<string, string>? headers)
    {
        return SendRequest<T>(HttpMethod.Post, endpoint, jsonObject, headers);
    }

    /// <summary>
    /// Sends a GET request to specified endpoint with custom headers
    /// </summary>
    public T? Get<T>(string endpoint, Dictionary<string, string>? headers)
    {
        return SendRequest<T>(HttpMethod.Get, endpoint, null, headers);
    }

    /// <summary>
    /// Sends a PUT request to specified endpoint with custom headers
    /// </summary>
    public T? Put<T>(string endpoint, string jsonObject, Dictionary<string, string>? headers)
    {
        return SendRequest<T>(HttpMethod.Put, endpoint, jsonObject, headers);
    }

    /// <summary>
    /// Sends a DELETE request to specified endpoint with custom headers
    /// </summary>
    public void Delete(string endpoint, Dictionary<string, string>? headers)
    {
        SendRequest<object>(HttpMethod.Delete, endpoint, null, headers);
    }

    private T? SendRequest<T>(HttpMethod method, string endpoint, string? jsonObject, Dictionary<string, string>? headers)
    {
        var logger = _logger is not null
            ? new AuthorizeNetRequestLogger(_logger)
            : new AuthorizeNetRequestLogger(_debugEnabled);

        string responseText = string.Empty;

        try
        {
            var requestMessage = new HttpRequestMessage(method, endpoint);

            if (jsonObject is not null)
            {
                requestMessage.Content = new StringContent(jsonObject);
                requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            // Add custom headers
            if (headers is not null)
            {
                foreach ((string name, string value) in headers)
                    requestMessage.Headers.Add(name, value);
            }

            logger.LogRequest(requestMessage, jsonObject ?? "");

            using (HttpResponseMessage response = _httpClient.SendAsync(requestMessage)
                .GetAwaiter().GetResult())
            {
                responseText = response.Content
                    .ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                logger.LogResponse(response, responseText);

                // For DELETE requests, check status code rather than parsing content
                if (method == HttpMethod.Delete)
                {
                    if (!response.IsSuccessStatusCode)
                        throw new AuthorizeNetApiException($"DELETE request failed with status: {response.StatusCode}");

                    return default(T);
                }

                // Check if response is an error before deserializing to T
                if (TryParseError(responseText) is ErrorResponse errorResponse)
                {
                    string errorMessages = string.Join("; ",
                        errorResponse.Messages?.Message?.Select(m => $"[{m.Code}] {m.Text}") ?? ["Unknown error"]);

                    if (Converter.TryDeserialize(responseText, out CreateTransactionResponse? responseWrapper)
                        && responseWrapper.TransactionResponse is TransactionResponse transactionResponse)
                    {
                        if (transactionResponse.Errors is not null)
                        {
                            string detailedErrors = string.Join("; ",
                                transactionResponse.Errors.Select(e => $"[{e.ErrorCode}] {e.ErrorText}"));
                            errorMessages += $" Details: {detailedErrors}";
                        }
                    }

                    throw new AuthorizeNetApiException($"Authorize.Net API error: {errorMessages}", errorResponse);
                }

                return Converter.Deserialize<T>(responseText);
            }
        }
        catch (Exception ex)
        {
            logger.LogException(ex);

            if (ex is AuthorizeNetApiException apiException)
                throw apiException;

            string message = ex.Message + $" Authorize.Net API response: {responseText}";
            throw new Exception(message);
        }
        finally
        {
            logger.FinalizeLog();
        }
    }

    /// <summary>
    /// Tries to parse the response as an error response
    /// </summary>
    /// <param name="responseText">The response text from API</param>
    /// <returns>ErrorResponse if it's an error, null otherwise</returns>
    private static ErrorResponse? TryParseError(string responseText)
    {
        try
        {
            var errorResponse = Converter.Deserialize<ErrorResponse>(responseText);

            // Check if this looks like an error response
            if (Enum.TryParse(errorResponse?.Messages?.ResultCode, true, out MessageTypeEnum resultCode) && resultCode is MessageTypeEnum.Error)
                return errorResponse;
        }
        catch
        {
            // Not an error response or couldn't parse - ignore
        }

        return null;
    }

    private static HttpClient CreateHttpClient(string baseAddress)
    {
        return new HttpClient(new HttpClientHandler
        {
            UseCookies = false
        })
        {
            BaseAddress = new Uri(baseAddress),
            Timeout = TimeSpan.FromSeconds(90)
        };
    }
}
