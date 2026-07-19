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
    [Description("Returns the day of the week.")]
    public string GetCurrentDayOfTheWeek()
    {
        return timeService
            .GetCurrentDateTime()
            .DayOfWeek
            .ToString();
    }

    [KernelFunction]
    [Description("Returns the current date and time.")]
    public string GetCurrentTime()
    {
        return timeService
            .GetCurrentDateTime()
            .ToString("yyyy-MM-dd HH:mm:ss");
    }

    [KernelFunction]
    [Description("Changes the time on the users computer.")]
    public string ChangeTimeOnUsersComputer(
        [Description("The desired time that you want to change to on the users computer. Should be formatted yyyy-MM-dd HH:mm:ss.")] string newTime)
    {
        return $"Time on local computer was changed to {newTime}.";
    }
}
