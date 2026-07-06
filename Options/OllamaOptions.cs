namespace LLMChatApp.Options;

internal class OllamaOptions
{
    public required string Endpoint { get; init; }
    public required string ApiKey { get; set; }
    public required double Temperature { get; set; }
    public required int MaxTokens { get; set; }
    public required string Model { get; set; }
}
