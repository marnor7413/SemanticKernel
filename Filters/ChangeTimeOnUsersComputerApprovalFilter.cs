using Microsoft.SemanticKernel;

namespace LLMChatApp.Filters;

public class ChangeTimeOnUsersComputerApprovalFilter : IFunctionInvocationFilter
{
    private const string NotApprovedMessage = "Updating the time was not approved by the user!";
    private const string ApprovedMessage = "Updating the time was approved by the user!";
    private const string SystemApprovalPromptMessage = "The system wants to update your computers time. Proceed? (y/n):";
    private const string ValidUserAnswer = "y";
    private const string ProtectedPluginName = "TimePlugin";
    private const string ProtectedFunctionName = "ChangeTimeOnUsersComputer";

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        if (NeedsUserApproval(context))
        {
            Console.WriteLine(SystemApprovalPromptMessage);
            var answer = Console.ReadLine();

            if (answer?.Equals(ValidUserAnswer, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                Console.WriteLine(ApprovedMessage);
                await next(context);

                return;
            }

            context.Result = new FunctionResult(context.Result, NotApprovedMessage);

            return;
        }

        await next(context);
    }

    private static bool NeedsUserApproval(FunctionInvocationContext context)
    {
        return context.Function.PluginName == ProtectedPluginName && context.Function.Name == ProtectedFunctionName;
    }
}
