using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Text;

namespace LLMChatApp;

internal class Chat
{
    public async Task SimpleChat()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOllamaChatCompletion(
            modelId: "qwen2.5-coder:32b",
            endpoint: new Uri("http://192.168.50.3:11434"));
        var kernel = builder.Build();
        var chatKlient = kernel.GetRequiredService<IChatCompletionService>();

        var options = new OllamaPromptExecutionSettings
        {
            Temperature = 0.8f,
            NumPredict = 512
        };

        var history = new ChatHistory();
        history.AddSystemMessage("Du är en hjälpsam assistent.");

        while (true) 
        {
            Console.Write("Me: ");
            var prompt = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(prompt) || prompt.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            history.AddUserMessage(prompt);

            var completeResponse = new StringBuilder();
            Console.Write("AI: ");
            await foreach (var messageChunk in chatKlient.GetStreamingChatMessageContentsAsync(history, options))
            {
                Console.Write(messageChunk);
                completeResponse.Append(messageChunk.Content);
            }

            history.AddAssistantMessage(completeResponse.ToString());
            Console.WriteLine();
        }

        Console.WriteLine("#### END OF CHAT");
    }
}
