using LLMChatApp.Constants;
using LLMChatApp.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LLMChatApp.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static void RegisterLogging(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.Services.AddSingleton<IConfigureOptions<LoggerFilterOptions>>(sp =>
            {
                var logLevelOptions = sp.GetRequiredService<IOptionsMonitor<LogLevelOptions>>();
                return new ConfigureOptions<LoggerFilterOptions>(options =>
                {
                    options.MinLevel = Enum.TryParse<LogLevel>(logLevelOptions.CurrentValue.MinimumLevel, ignoreCase: true, out var level)
                        ? level
                        : LogLevel.Information;
                });
            });
        });
    }

}
