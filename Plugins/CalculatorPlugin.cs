using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace LLMChatApp.Plugins;

internal class CalculatorPlugin
{
    [KernelFunction]
    [Description("Adds two numbers together and returns the sum.")]
    public double Add(
        [Description("The first number.")] double a,
        [Description("The second number.")] double b)
    {
        return a + b;
    }

    [KernelFunction]
    [Description("Subtracts the second number from the first and returns the difference.")]
    public double Subtract(
        [Description("The number to subtract from.")] double a,
        [Description("The number to subtract.")] double b)
    {
        return a - b;
    }

    [KernelFunction]
    [Description("Multiplies two numbers and returns the product.")]
    public double Multiply(
    [Description("The first number.")] double a,
    [Description("The second number.")] double b
    )
    {
        return a * b;
    }

    [KernelFunction]
    [Description("Divides one number with another and returns the quotient.")]
    public double Divide(
    [Description("The number to divide.")] double a,
    [Description("The number to divide by. Can't be zero.")] double b
    )
    {
        if(b == 0)
        {
            throw new DivideByZeroException("Can't divide by zero.");
        }

        return a / b;
    }
}
