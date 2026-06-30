# LLMChatApp

A small console-based test project for exploring [Semantic Kernel](https://github.com/microsoft/semantic-kernel) against a locally hosted LLM via [Ollama](https://ollama.com).

## Purpose

This project is a first exploration of Semantic Kernel's `IChatCompletionService` interface, connected to a self-hosted Ollama instance in a home lab (Proxmox + RTX 5090) instead of a cloud API. The goal is to build on this in later projects, for example a LocalLLMChat Visual Studio extension.

## Prerequisites

- .NET 10 SDK
- A running Ollama instance, reachable over the network, with at least one model pulled (`ollama pull <model>`)
- The model referenced in the code must match the exact name reported by `ollama list`, including case

## Configuration

Endpoint and model are set in `Chat.cs`:

```csharp
builder.AddOllamaChatCompletion(
    modelId: "qwen2.5-coder:32b",
    endpoint: new Uri("http://192.168.50.3:11434"));
```

Adjust `endpoint` to your own Ollama instance IP and port (Ollama's default port is `11434`).

## Running the project

```bash
dotnet run
```

Type questions in the console. Type `quit` or an empty line to exit.

## What the code does

1. Builds a `Kernel` using Semantic Kernel's Ollama connector (`AddOllamaChatCompletion`), which registers SK's native `IChatCompletionService`.
2. Sets `OllamaPromptExecutionSettings` with `Temperature` and `NumPredict` to control creativity and maximum response length.
3. Keeps conversation history in a `ChatHistory`, with a system message set at startup.
4. Streams the response token by token via `GetStreamingChatMessageContentsAsync`, printing each fragment immediately while simultaneously building the full response in a `StringBuilder` so it can be saved to history afterward.
5. Loops until the user types `quit` or an empty line.

## Packages used

- `Microsoft.SemanticKernel.Connectors.Ollama` (experimental, requires `<NoWarn>SKEXP0070</NoWarn>` in `.csproj`)

## Known limitations / things to keep in mind

- The Ollama connector is still marked experimental by Microsoft (`SKEXP0070`) and the API may change in future versions.
- No error handling for interrupted network calls or if the Ollama instance is unreachable.
- The model name must match exactly, including case, against what the Ollama instance has actually pulled.
- No explicit `keep_alive` set on the call, so Ollama's default of 5 minutes applies for how long the model stays loaded in VRAM between calls.