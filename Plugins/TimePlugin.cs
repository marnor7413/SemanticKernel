using LLMChatApp.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace LLMChatApp.Plugins;

internal class TimePlugin
{
    private readonly ITimeService timeService;

    public TimePlugin(ITimeService timeService)
    {
        this.timeService = timeService;
    }

    [KernelFunction]
    [Description("Returns the current date and time.")]
    public string GetCurrentTime()
    {
        return timeService
            .GetCurrentDateTime()
            .ToString("yyyy-MM-dd HH:mm:ss");
    }
}
