using LLMChatApp.Interfaces;
using LLMChatApp.Options;
using LLMChatApp.Plugins;
using LLMChatApp.Services;
using Microsoft.Extensions.DependencyInjection;
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

    internal static OpenAIPromptExecutionSettings RegisterModelForOpenAIApiExtension(this IKernelBuilder builder, OllamaOptions options)
    {
        builder.AddOpenAIChatCompletion(
            modelId: options.Model,
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
