using LLMChatApp.Filters;
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
    internal static void RegisterServices(this IKernelBuilder builder)
    {
        builder.Services.AddTransient<ITimeService, TimeService>();
        builder.Services.AddSingleton<IFunctionInvocationFilter, ChangeTimeOnUsersComputerApprovalFilter>();
    }

    internal static void RegisterPlugins(this IKernelBuilder builder)
    {
        builder.Plugins.AddFromType<TimePlugin>();
        builder.Plugins.AddFromType<CalculatorPlugin>();
    }

    internal static OpenAIPromptExecutionSettings CreatePromptSettings(this IKernelBuilder builder, OllamaOptions options)
    {
        return new OpenAIPromptExecutionSettings
        {
            Temperature = options.Temperature,
            MaxTokens = options.MaxTokens,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }

    internal static void RegisterModel(this IKernelBuilder builder, OllamaOptions options)
    {
        builder.AddOpenAIChatCompletion(
            modelId: options.Model,
            endpoint: new Uri(options.Endpoint),
            apiKey: options.ApiKey);
    }
}
