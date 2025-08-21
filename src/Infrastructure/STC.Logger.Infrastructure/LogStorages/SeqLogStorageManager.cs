using Microsoft.Extensions.Configuration;
using STC.Logger.Application;
using STC.Logger.Application.Features.LogStorages.Services;

namespace STC.Logger.Infrastructure.LogStorages;

public class SeqLogStorageManager(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    : ILogStorageService
{
    private HttpClient? _httpClient;

    public async Task<bool> SaveAsync(string logMessage, CancellationToken cancellationToken)
    {
        if (_httpClient is null)
        {
            _httpClient = httpClientFactory.CreateClient(name: "LoggerClient");
            _httpClient.BaseAddress = new Uri(configuration.GetConnectionString(name: "Seq") ??
                                              throw new InvalidOperationException(
                                                  "Seq connection string is not configured."));
        }

        HttpResponseMessage httpResult = await _httpClient
            .PostAsync(requestUri: "ingest/clef",
                content: new StringContent(logMessage),
                cancellationToken: cancellationToken);

        return httpResult.IsSuccessStatusCode;
    }
}