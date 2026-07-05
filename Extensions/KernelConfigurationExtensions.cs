using LLMChatApp.Interfaces;
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


    internal static OpenAIPromptExecutionSettings RegisterModelForOpenAIApiExtension(this IKernelBuilder builder, string model, string endpoint)
    {
        builder.AddOpenAIChatCompletion(
            modelId: model,
            endpoint: new Uri(endpoint),
            apiKey: "ollama");

        return new OpenAIPromptExecutionSettings
        {
            Temperature = 0.3f,
            MaxTokens = 4096,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }
}
