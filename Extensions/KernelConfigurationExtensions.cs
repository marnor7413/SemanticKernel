using LLMChatApp.Constants;
using LLMChatApp.Interfaces;
using LLMChatApp.Options;
using LLMChatApp.Plugins;
using LLMChatApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace LLMChatApp.Extensions;

internal static class KernelConfigurationExtensions
{
    internal static void RegisterServicesExtension(this IKernelBuilder builder)
    {
        builder.Services.AddTransient<ITimeService, TimeService>();
    }

    internal static void RegisterPluginsExtension(this IKernelBuilder builder)
    {
        builder.Plugins.AddFromType<TimePlugin>();
    }

    internal static void AddLogging(this IKernelBuilder builder, LogLevelOptions options)
    {
        if (!Enum.TryParse<LogLevel>(options.MinimumLevel, ignoreCase: true, out var level))
        {
            throw new InvalidOperationException(Config.LoggingConfigSectionErrorMessage);
        }

        builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(level));
    }

    internal static OpenAIPromptExecutionSettings RegisterModelForOpenAIApiExtension(
        this IKernelBuilder builder, 
        string model, 
        OllamaOptions options)
    {
        builder.AddOpenAIChatCompletion(
            modelId: model,
            endpoint: new Uri(options.Endpoint),
            apiKey: options.ApiKey);

        return new OpenAIPromptExecutionSettings
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }
}
