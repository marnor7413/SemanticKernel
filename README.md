# LLMChatApp

Console app for playing around with Semantic Kernel against a self-hosted Ollama instance.

## What this is

Started as a quick test of `IChatCompletionService` against a local model instead of a cloud API. Turned into something with actual config, live-reload, a couple of plugins, history summarization and a human-approval gate for the one function that shouldn't just run without asking first.

## Before you run it

- .NET 10 SDK
- Ollama reachable on the network, model already pulled
- Model name has to match `ollama list` exactly

## Setup

`Appsettings.json`:

```json
{
  "Logging": {
    "MinimumLevel": "Error"
  },
  "Ollama": {
    "Endpoint": "http://your-ollama-host:11434/v1",
    "ApiKey": "SetupInUserSecrets",
    "Temperature": 0.3,
    "MaxTokens": 4096,
    "Model": "qwen3-coder:30b"
  }
}
```

`/v1` suffix matters, that's Ollama's OpenAI-compatible surface, not its native API. `ApiKey` is a dead placeholder as long as Ollama itself doesn't check it, move it to user secrets before pointing this at anything that does.

## Running it

```bash
dotnet run
```

`quit` or an empty line to exit.

## How it works

`OllamaOptions` and `LogLevelOptions` come in through `IOptionsMonitor`, bound from config. `KernelFactory` builds and caches a `Kernel`, subscribed to config changes, edit `Temperature`, `Model`, whatever, in the json while the app's running, next question picks it up. Same `ILoggerFactory` gets pushed into the kernel's own service container so Semantic Kernel's internal logging isn't just going nowhere.

History gets summarized via `ChatHistorySummarizationReducer` when it grows past the thresholds in `MessageSummarySettings`. By doing this, the application tries to preserve the chat context during long sessions.

Two plugins: `TimePlugin` and `CalculatorPlugin` with orchestration set to `FunctionChoiceBehavior.Auto()`. `TimePlugin` has one function called `ChangeTimeOnUsersComputer` gated behind a filter called  `ChangeTimeOnUsersComputerApprovalFilter`. An `IFunctionInvocationFilter` that stops and asks for a y/n before letting that specific call through. Everything else passes straight through the filter untouched.

Network failures against Ollama get caught, the unanswered question gets pulled back out of history so it doesn't leave a dangling turn, and the loop keeps going instead of dying.

## A note on model choice

`qwen2.5-coder:32b` kept generating the right tool call payload but skipped the `<tool_call>` tags Ollama's template expects, so it never registered as an actual function call, just came back as text. `qwen3-coder:30b` has been solid so far.

## Packages

- `Microsoft.SemanticKernel.Connectors.OpenAI`
- `Microsoft.Extensions.Configuration.Json` / `.EnvironmentVariables` / `.Binder`
- `Microsoft.Extensions.Logging.Console`

## Not handled yet

- No explicit `keep_alive`, falls back to Ollama's 5 minute default
- `ApiKey` placeholder still sits in plain json
- No startup check that the configured model actually exists on the Ollama instance

## Maybe later

- Validate the model against Ollama at startup instead of failing on first call
