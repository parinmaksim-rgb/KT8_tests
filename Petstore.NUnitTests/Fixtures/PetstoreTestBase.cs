using System.Net;
using System.Net.Http.Headers;
using Petstore.NUnitTests.Constants;

namespace Petstore.NUnitTests.Fixtures;

public abstract class PetstoreTestBase
{
    private static long _idSequence = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 500000;

    protected HttpClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(ApiConstants.BaseUrl),
            Timeout = TimeSpan.FromSeconds(ApiConstants.HttpTimeoutSeconds)
        };
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.DefaultRequestHeaders.Add(ApiConstants.ApiKeyHeader, ApiConstants.ApiKeyValue);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Client.Dispose();
    }

    protected long GenerateUniqueId()
    {
        return Interlocked.Increment(ref _idSequence);
    }

    protected string GenerateUniqueUsername(string prefix)
    {
        return $"{prefix}_{GenerateUniqueId()}";
    }

    protected async Task<HttpResponseMessage> GetWithRetryAsync(string relativeUri)
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
}
