using LLMChatApp.Filters;
using LLMChatApp.Interfaces;
using LLMChatApp.Options;
using LLMChatApp.Plugins;
using LLMChatApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;

namespace LLMChatApp.Extensions;

internal static class KernelConfigurationExtensions
{
    private const string ApiSpecDirectory = "ApiSpecifications/";
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

    /// <summary> Let's semantic kernel configure itself against an api by reading the OpenApi spec.
    ///     OpenApi spec can be read directly from a location at Open-meteo 
    ///     but they do not accept requests from robots. Due to this
    ///     the spec is instead stored in the repo.
    /// </summary>
    internal static async Task ConfigureOpenMeteoGeocodingApiAsync(this Kernel kernel, IHttpClientFactory httpClientFactory)
    {
        const string OpenMeteoSpec = "OpenMeteoGeocodingApi.yml";
        var directoryPath = Path.Combine(AppContext.BaseDirectory, ApiSpecDirectory);
        var fullPath = Path.Combine(directoryPath, OpenMeteoSpec);
        await using var stream = File.OpenRead(fullPath);
        var http = httpClientFactory.CreateClient("open-meteo");
        
        await kernel.ImportPluginFromOpenApiAsync(
            pluginName: "GeocodingAPi",
            stream: stream,
            executionParameters: new OpenApiFunctionExecutionParameters
            {
                ServerUrlOverride = new Uri("https://geocoding-api.open-meteo.com"),
                EnableDynamicPayload = true,
                HttpClient = http
            });
    }

    /// <summary> Let's semantic kernel configure itself against an api by reading the OpenApi spec.
    ///     OpenApi spec can be read directly from a location at Open-meteo 
    ///     but they do not accept requests from robots. Due to this
    ///     the spec is instead stored in the repo.
    /// </summary>
    internal static async Task ConfigureOpenMeteoWeatherForecastApiAsync(this Kernel kernel, IHttpClientFactory httpClientFactory)
    {
        const string OpenMeteoSpec = "OpenMeteoWeatherForecastApi.yml";
        var directoryPath = Path.Combine(AppContext.BaseDirectory, ApiSpecDirectory);
        var fullPath = Path.Combine(directoryPath, OpenMeteoSpec);
        await using var stream = File.OpenRead(fullPath);
        var http = httpClientFactory.CreateClient("open-meteo");
        
        await kernel.ImportPluginFromOpenApiAsync(
            pluginName: "WeatherAPi",
            stream: stream,
            executionParameters: new OpenApiFunctionExecutionParameters
            {
                ServerUrlOverride = new Uri("https://api.open-meteo.com"),
                EnableDynamicPayload = true,
                HttpClient = http
            });
    }
}
