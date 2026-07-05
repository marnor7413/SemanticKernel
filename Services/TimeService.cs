using LLMChatApp.Interfaces;

namespace LLMChatApp.Services;

internal class TimeService : ITimeService
{
    public DateTime GetCurrentDateTime() => DateTime.Now;
    public DateTime GetCurrentDateTimeUtc() => DateTime.UtcNow;
}
