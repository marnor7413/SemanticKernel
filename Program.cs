using LLMChatApp.Connectors;
using LLMChatApp.Constants;
using LLMChatApp.Extensions;
using LLMChatApp.Factories;
using LLMChatApp.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(Config.AppsettingsFilename, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.RegisterLogging();
services.Configure<OllamaOptions>(configuration.GetSection(Config.OllamaConfigSection));
services.Configure<LogLevelOptions>(configuration.GetSection(Config.LoggingConfigSection));
services.AddSingleton<KernelFactory>();

var serviceProvider = services.BuildServiceProvider();
var kernelFactory = serviceProvider.GetRequiredService<KernelFactory>();
await new OpenAiConnector().SimpleChat(kernelFactory);