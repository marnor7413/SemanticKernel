using Microsoft.SemanticKernel;

namespace LLMChatApp.Filters;

public class ChangeTimeOnUsersComputerApprovalFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        if (context.Function.PluginName == "TimePlugin" 
            && context.Function.Name == "ChangeTimeOnUsersComputer")
        {
            Console.WriteLine("The system wants to update your computers time. Proceed? (y/n):");
            var answer = Console.ReadLine();

            if (answer.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Updating the time was approved by the user!");
                await next(context);
            }

            context.Result = new FunctionResult(context.Result, "Updating the time was not approved by the user!");
            
            return;
        }
    }
}
