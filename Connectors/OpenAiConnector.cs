using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;

namespace LLMChatApp.Connectors;

internal class OpenAiConnector
{
    private const string QuestionRowPrefix = "Me: ";
    private const string ResponseRowPrefix = "AI: ";
    private const string EndApplicationCommand = "quit";
    private const string ExitApplicationMessage = "#### END OF CHAT";
    private const string SystemPrompt = @"Key Requirements:
            1. Before you answer, tell me what you need to know to answer well, and point out any assumptions you'd otherwise make.
            2. If you make an error, explain it and correct it immediately.
            3. Always double-check your work before giving a response.
            4. Critical: If you do not know or can't find an answer then tell me so.
            5. Critical: Your primary goal is to provide reliable, accurate, and correct responses at all times.";

    public async Task SimpleChat(Kernel kernel, OpenAIPromptExecutionSettings options)
    {
        var chatKlient = kernel.GetRequiredService<IChatCompletionService>();
        
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);

        while (true)
        {
            Console.Write(QuestionRowPrefix);
            var prompt = Console.ReadLine();
            if (ExitApplication(prompt))
            {
                break;
            }
            history.AddUserMessage(prompt);
            Console.WriteLine();

            var responseBuilder = new StringBuilder();
            Console.Write(ResponseRowPrefix);

            try
            {
                await foreach (var messageChunk in chatKlient.GetStreamingChatMessageContentsAsync(history, options, kernel))
                {
                    Console.Write(messageChunk.Content);
                    responseBuilder.Append(messageChunk.Content);
                }
            }
            catch(HttpOperationException ex)
            {
                Console.WriteLine($"\nCould not reach the model: {ex.Message}");
                continue;
            }
            catch (Exception)
            {
                throw;
            }
            

            history.AddAssistantMessage(responseBuilder.ToString());
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