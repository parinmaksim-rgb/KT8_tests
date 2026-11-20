using System.Net;
using System.Net.Http.Headers;
using Petstore.XUnitTests.Constants;

namespace Petstore.XUnitTests.Fixtures;

public class PetstoreApiFixture : IDisposable
{
    private static long _idSequence = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public HttpClient Client { get; }

    public PetstoreApiFixture()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(ApiConstants.BaseUrl),
            Timeout = TimeSpan.FromSeconds(ApiConstants.HttpTimeoutSeconds)
        };
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.DefaultRequestHeaders.Add(ApiConstants.ApiKeyHeader, ApiConstants.ApiKeyValue);
    }

    public long GenerateUniqueId()
    {
        return Interlocked.Increment(ref _idSequence);
    }

    public string GenerateUniqueUsername(string prefix)
    {
        return $"{prefix}_{GenerateUniqueId()}";
    }

    public async Task<HttpResponseMessage> GetWithRetryAsync(string relativeUri)
    {
        HttpResponseMessage response = await Client.GetAsync(relativeUri);
        for (int attempt = 1; attempt < ApiConstants.MaxRetryAttempts && response.StatusCode == HttpStatusCode.NotFound; attempt++)
        {
            response.Dispose();
            await Task.Delay(ApiConstants.RetryDelayMilliseconds);
            response = await Client.GetAsync(relativeUri);
        }

        return response;
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}
