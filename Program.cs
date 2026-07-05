using LLMChatApp.Connectors;
using LLMChatApp.Constants;
using LLMChatApp.Extensions;
using LLMChatApp.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(Config.AppsettingsFilename, optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var ollamaOptions = GetSection<OllamaOptions>(configuration, Config.OllamaConfigSection, Config.OllamaConfigSectionErrorMessage);
var logLevelOptions = GetSection<LogLevelOptions>(configuration, Config.LoggingConfigSection, Config.LoggingConfigSectionErrorMessage);

var builder = Kernel.CreateBuilder();
builder.RegisterServicesExtension();
builder.RegisterPluginsExtension();
builder.AddLogging(logLevelOptions);
var options = builder.RegisterModelForOpenAIApiExtension(LLM.Qwen3Coder30b, ollamaOptions);

var kernel = builder.Build();

await new OpenAiConnector().SimpleChat(kernel, options);

static T GetSection<T>(IConfigurationRoot? config, string sectionName, string errormessage) where T : class
{
    ArgumentNullException.ThrowIfNull(config);

    return config.GetSection(sectionName).Get<T>()
        ?? throw new InvalidOperationException(errormessage);
}