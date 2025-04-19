using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;

namespace VVOK12.SignalClient;

public class SignalClient
{
    private readonly string _apiUrl;
    private readonly HttpClient _httpClient;
    private readonly AsyncPolicy<HttpResponseMessage> _retryPolicy;

    public SignalClient(IHttpClientFactory httpClientFactory, string apiUrl)
    {
        if (httpClientFactory is null)
        {
            throw new ArgumentNullException(nameof(httpClientFactory));
        }
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            throw new ArgumentException("API URL cannot be null or empty.", nameof(apiUrl));
        }

        _apiUrl = apiUrl;
        _httpClient = httpClientFactory.CreateClient();
        _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .RetryAsync(3, onRetry: (outcome, retryCount, context) =>
            {
                Console.WriteLine($"Retry ({retryCount}) of {nameof(SignalClient)} encountered an error: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}. Retrying...");
            });
    }

    public async Task<HttpResponseMessage> SendAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message content cannot be null or empty.", nameof(message));
        }

        using var content = new StringContent(message, Encoding.UTF8, "application/x-www-form-urlencoded");
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.PostAsync(_apiUrl, content);
        });
    }
}