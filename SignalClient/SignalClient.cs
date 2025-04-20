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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AsyncPolicy<HttpResponseMessage> _retryPolicy;

    public SignalClient(IHttpClientFactory httpClientFactory, string apiUrl)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));
        ArgumentException.ThrowIfNullOrEmpty(apiUrl, nameof(apiUrl));

        _apiUrl = apiUrl;
        _httpClientFactory = httpClientFactory;
        _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .RetryAsync(3, onRetry: (outcome, retryCount, context) =>
            {
                Console.WriteLine($"Retry ({retryCount}) of {nameof(SignalClient)} encountered an error: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}. Retrying...");
            });
    }

    public async Task<HttpResponseMessage> SendAsync(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        using var httpClient = _httpClientFactory.CreateClient();    
        using var content = new StringContent(message, Encoding.UTF8, "application/x-www-form-urlencoded");
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await httpClient.PostAsync(_apiUrl, content);
        });
    }
}