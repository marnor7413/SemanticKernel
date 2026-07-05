using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;

namespace LLMChatApp.Connectors;

internal class OpenAiConnector
{
    private const string BeginningOfQuestion = "Me: ";
    private const string BeginningOfReply = "AI: ";
    private const string EndApplicationCommand = "quit";
    private const string ExitApplicationMessage = "#### END OF CHAT";

    public async Task SimpleChat(Kernel kernel, OpenAIPromptExecutionSettings options)
    {
        var chatKlient = kernel.GetRequiredService<IChatCompletionService>();

        const string SystemPrompt = @"Key Requirements:
            1. Always verify answers using available tools before responding.
            2. If you make an error, explain it and correct it immediately.
            3. All calculations must be accurate and logical.
            4. Do not provide incorrect information.
            5. Always double-check your work before giving a response.
            6. If you are unsure, always state this clearly and verify with the tools before proceeding.
            7. If you do not know the answer, let that be your response, instead of making an assumption or guesswork.

            Critical: Your primary goal is to provide reliable, accurate, and correct responses at all times.";
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);

        while (true)
        {
            Console.Write(BeginningOfQuestion);
            var prompt = Console.ReadLine();
            if (ExitApplication(prompt))
            {
                break;
            }

            history.AddUserMessage(prompt);

            var LlmResponse = new StringBuilder();
            Console.Write(BeginningOfReply);
            await foreach (var messageChunk in chatKlient.GetStreamingChatMessageContentsAsync(history, options, kernel))
            {
                Console.Write(messageChunk.Content);
                LlmResponse.Append(messageChunk.Content);
            }

            history.AddAssistantMessage(LlmResponse.ToString());
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine(ExitApplicationMessage);
    }

    private static bool ExitApplication(string? prompt)
    {
        return string.IsNullOrWhiteSpace(prompt) || prompt.Equals(EndApplicationCommand, StringComparison.OrdinalIgnoreCase);
    }
}