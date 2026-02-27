using Dynamicweb.Core;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

internal sealed class AuthorizeNetHttpService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly bool _debugEnabled;
    private readonly AuthorizeNetLogger? _logger;
    private bool _disposed = false;

    public AuthorizeNetHttpService(bool isTestMode, bool debugEnabled, AuthorizeNetLogger? logger)
    {
        _url = AuthorizeNetEndpoints.GetApiEndpoint(isTestMode);
        _debugEnabled = debugEnabled;
        _logger = logger;

        _httpClient = new HttpClient(new HttpClientHandler())
        {
            BaseAddress = new Uri(_url),
            Timeout = TimeSpan.FromSeconds(90)
        };
    }

    /// <summary>
    /// Disposes the HTTP client and releases managed resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _httpClient?.Dispose();
        _disposed = true;
    }

    public T? Post<T>(string jsonObject)
    {
        var logger = _logger is not null
            ? new AuthorizeNetRequestLogger(_logger)
            : new AuthorizeNetRequestLogger(_debugEnabled);
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Content = new StringContent(jsonObject)
            };

            requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            logger.LogRequest(requestMessage, jsonObject);

            using (HttpResponseMessage response = _httpClient.SendAsync(requestMessage).GetAwaiter().GetResult())
            {
                string responseText = response.Content
                    .ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                // Remove BOM if present
                if (responseText.StartsWith("\uFEFF"))
                    responseText = responseText.Substring(1);

                logger.LogResponse(response, responseText);
                return Converter.Deserialize<T>(responseText);
            }
        }
        catch (Exception ex)
        {
            logger.LogException(ex);
            throw;
        }
        finally
        {
            logger.FinalizeLog();
        }
    }
}