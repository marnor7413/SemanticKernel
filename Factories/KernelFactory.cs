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
    private readonly IOptionsMonitor<OllamaOptions> optionsMonitor;
    private readonly ILoggerFactory loggerFactory;
    private readonly IDisposable? changeSubscriptions;
    private readonly object lockObject = new();

    private Kernel? kernel;
    private OpenAIPromptExecutionSettings? openAIOptions;

    public KernelFactory(IOptionsMonitor<OllamaOptions> optionsMonitor, ILoggerFactory loggerFactory)
    {
        this.optionsMonitor = optionsMonitor;
        this.loggerFactory = loggerFactory;
        changeSubscriptions = optionsMonitor.OnChange(_ => Invalidate());
    }

    private void Invalidate()
    {
        lock (lockObject)
        {
            kernel = null;
            openAIOptions = null;
        }
    }

    public (Kernel kernel, OpenAIPromptExecutionSettings options) GetCurrent()
    {
        lock (lockObject)
        {
            if (kernel is null)
            {
                var builder = Kernel.CreateBuilder();
                builder.Services.AddSingleton(loggerFactory);
                builder.RegisterServices();
                builder.RegisterPlugins();
                builder.RegisterModel(optionsMonitor.CurrentValue);
                openAIOptions = builder.CreatePromptSettings(optionsMonitor.CurrentValue);
                kernel = builder.Build();
            }

            return (kernel, openAIOptions!);
        }
    }

    public void Dispose() => changeSubscriptions?.Dispose();
}