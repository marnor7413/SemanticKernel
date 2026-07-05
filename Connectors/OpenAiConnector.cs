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

        string systemPrompt = @"Key Requirements:
            1. Always verify answers using available tools before responding.
            2. If you make an error, explain it and correct it immediately.
            3. All calculations must be accurate and logical.
            4. Do not provide incorrect information.
            5. Always double-check your work before giving a response.
            6. If you are unsure, always state this clearly and verify with the tools before proceeding.
            7. If you do not know the answer, let that be your response, instead of making an assumption or guesswork.

            Critical: Your primary goal is to provide reliable, accurate, and correct responses at all times.";
        var history = new ChatHistory();
        history.AddSystemMessage(systemPrompt);

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