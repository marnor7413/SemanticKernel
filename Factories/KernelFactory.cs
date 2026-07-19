using LLMChatApp.Extensions;
using LLMChatApp.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace LLMChatApp.Factories;

internal class KernelFactory : IDisposable
{
    private volatile CachedKernel? cachedKernel;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IOptionsMonitor<OllamaOptions> optionsMonitor;
    private readonly ILoggerFactory loggerFactory;
    private readonly IDisposable? changeSubscriptions;
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public KernelFactory(IHttpClientFactory httpClientFactory, IOptionsMonitor<OllamaOptions> optionsMonitor, ILoggerFactory loggerFactory)
    {
        this.httpClientFactory = httpClientFactory;
        this.optionsMonitor = optionsMonitor;
        this.loggerFactory = loggerFactory;
        changeSubscriptions = optionsMonitor.OnChange(_ => Invalidate());
    }

    private void Invalidate() => cachedKernel = null;

    public async Task<(Kernel kernel, OpenAIPromptExecutionSettings options)> GetCurrentAsync()
    {
        var snapshot = cachedKernel;
        if (snapshot is not null)
        {
            return (snapshot.Kernel, snapshot.Options);
        }

        await semaphore.WaitAsync();
        try
        {
            if (cachedKernel is null)
            {
                var builder = Kernel.CreateBuilder();
                builder.Services.AddSingleton(loggerFactory);
                builder.RegisterServices();
                builder.RegisterPlugins();
                builder.RegisterModel(optionsMonitor.CurrentValue);
      
                var options = builder.CreatePromptSettings(optionsMonitor.CurrentValue);
                var kernel = builder.Build();
                await kernel.ConfigureOpenMeteoGeocodingApiAsync(httpClientFactory);
                await kernel.ConfigureOpenMeteoWeatherForecastApiAsync(httpClientFactory);
                cachedKernel = new CachedKernel(kernel, options);

                return (kernel, options);
            }

            return (cachedKernel.Kernel, cachedKernel.Options);

        }
        finally
        {
            semaphore.Release();
        }
    }

    public void Dispose()
    {
        changeSubscriptions?.Dispose();
        semaphore.Dispose();
    }

    private sealed record CachedKernel(Kernel Kernel, OpenAIPromptExecutionSettings Options);
}