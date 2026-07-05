# LLMChatApp

Console app for playing around with Semantic Kernel against a self-hosted Ollama instance.

## What this is

A first pass at using Semantic Kernel's `IChatCompletionService` against a local model instead of a cloud API. Started out on the experimental Ollama connector, ended up switching to the OpenAI connector pointed at Ollama's OpenAI-compatible endpoint instead, mostly because tool calling behaved more consistently that way.

## Before you run it

- .NET 10 SDK
- Ollama running somewhere reachable on your network, with a model already pulled
- The model name has to match exactly what `ollama list` shows

## Setup

Model and endpoint are passed together when registering the kernel, in `KernelConfigurationExtensions`:

```csharp
var options = builder.RegisterModelForOpenAIApiExtension(LLM.Qwen3Coder30b, new Uri("http://your-ollama-host:11434/v1"));
```

Point the URI at your own Ollama instance. Don't forget the `/v1` suffix, that's the OpenAI-compatible path, not Ollama's native API.

## Running it

```bash
dotnet run
```

Type your question, get a streamed answer back. Type `quit` or hit enter on an empty line to exit.

## How it works

Kernel gets built with the OpenAI connector (`AddOpenAIChatCompletion`), pointed at Ollama instead of actual OpenAI. Conversation history sits in a `ChatHistory`, seeded with a system message. Responses stream in through `GetStreamingChatMessageContentsAsync` and get printed as they arrive, while also getting collected into a `StringBuilder` so the full reply can go back into history once it's done.

Tool calling is wired up through a `TimePlugin`, registered via `builder.Plugins.AddFromType<TimePlugin>()`, with `ITimeService` injected through the constructor and registered as a singleton. `FunctionChoiceBehavior.Auto()` is set on the execution settings so the model can decide on its own when to call it.

## A note on model choice

Not every model handles tool calling the same way. `qwen2.5-coder:32b` kept generating the right JSON payload but skipped the `<tool_call>` tags Ollama's chat template expects, so Ollama never picked it up as an actual function call. It just came back as plain text. Switched to `qwen3-coder:30b` instead, which has been more reliable so far. 

## Packages

- `Microsoft.SemanticKernel.Connectors.OpenAI`

## Not handled yet

- No error handling if Ollama is unreachable or the model doesn't exist
- No `keep_alive` set explicitly, so it falls back to Ollama's 5 minute default before the model unloads from VRAM

## Maybe later

- Pull config into `appsettings.json` instead of passing things around in code
- Actual error handling
- Check available models against Ollama at startup instead of trusting a hardcoded list