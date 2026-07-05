using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;

namespace LLMChatApp.Connectors;

internal class OpenAiConnector
{
    public async Task SimpleChat(Kernel kernel, OpenAIPromptExecutionSettings options)
    {
        var chatKlient = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage("You're a helpful assistant!");

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
            await foreach (var messageChunk in chatKlient.GetStreamingChatMessageContentsAsync(history, options, kernel))
            {
                Console.Write(messageChunk.Content);
                completeResponse.Append(messageChunk.Content);
            }

            history.AddAssistantMessage(completeResponse.ToString());
            Console.WriteLine();
        }

        Console.WriteLine("#### END OF CHAT");
    }
}