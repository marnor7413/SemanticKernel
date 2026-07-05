using LLMChatApp.Connectors;
using LLMChatApp.Constants;
using LLMChatApp.Extensions;
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();
builder.RegisterServicesExtension();
builder.RegisterPluginsExtension();
var options = builder.RegisterModelForOpenAIApiExtension(LLM.Qwen3Coder30b, "http://192.168.50.3:11434/v1");

var kernel = builder.Build();

await new OpenAiConnector().SimpleChat(kernel, options);