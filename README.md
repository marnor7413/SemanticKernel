# LLMChatApp

Console app for playing around with Semantic Kernel against a self-hosted Ollama instance.

## What this is

A first pass at using Semantic Kernel's `IChatCompletionService` against a local model instead of a cloud API. Started out on the experimental Ollama connector, ended up switching to the OpenAI connector pointed at Ollama's OpenAI-compatible endpoint instead, mostly because tool calling behaved more consistently that way. Grew a bit past "quick test script" along the way, now has config, DI and live-reload of settings.

## Before you run it

- .NET 10 SDK
- Ollama running somewhere reachable on your network, with a model already in place
- The model name has to match exactly what `ollama list` shows

## Setup

Everything that used to be hardcoded now lives in `Appsettings.json`:

```json
{
  "Logging": {
    "MinimumLevel": "Information"
  },
  "Ollama": {
    "Endpoint": "http://your-ollama-host:11434/v1",
    "ApiKey": "SetupInUserSecrets",
    "Temperature": 0.3,
    "MaxTokens": 4096
  }
}
```

Point `Endpoint` at your own Ollama instance. Don't forget the `/v1` suffix, that's the OpenAI-compatible path, not Ollama's native API.

`ApiKey` is a placeholder. Ollama doesn't check it, the OpenAI connector just refuses to run without something non-empty in that field. If you ever point this at something that actually validates the key, move it to user secrets or an environment variable instead of leaving it in the json file.

Model choice is still set in code, in `Constants/LLM.cs` and referenced from `KernelFactory`.

## Running it

```bash
dotnet run
```

Type your question, get a streamed answer back. Type `quit` or hit enter on an empty line to exit.

## How it works

Config gets loaded once at startup from `Appsettings.json`, with environment variables layered on top. `OllamaOptions` and `LogLevelOptions` are bound from config sections and registered through `IOptionsMonitor`, not just read once and stashed in a variable.

`KernelFactory` owns building the actual `Kernel`. It builds one, caches it and hands out the same instance until something changes. It's subscribed to `OllamaOptions` changes, so editing `Temperature`, `MaxTokens` or `Endpoint` in the json file while the app is running and saving it means the next question you ask uses the new values, no restart needed. Same logger factory gets threaded into the kernel's own service container, so Semantic Kernel's internal logging actually goes somewhere instead of silently defaulting to nothing.

Conversation history sits in a `ChatHistory`, seeded with a system prompt. Responses stream in through `GetStreamingChatMessageContentsAsync` and get printed as they arrive, while also getting collected into a `StringBuilder` so the full reply can go back into history once it's done.

Tool calling is wired up through a `TimePlugin`, registered via `builder.Plugins.AddFromType<TimePlugin>()`, with `ITimeService` injected through the constructor. `FunctionChoiceBehavior.Auto()` is set on the execution settings so the model decides on its own when to call it.

If Ollama is unreachable, the chat loop catches it, prints a message, drops the unanswered question from history so it doesn't leave a dangling user turn, and lets you keep going instead of crashing the whole app.

## A note on model choice

Not every model handles tool calling the same way. `qwen2.5-coder:32b` kept generating the right JSON payload but skipped the `<tool_call>` tags Ollama's chat template expects, so Ollama never picked it up as an actual function call. It just came back as plain text. Switched to `qwen3-coder:30b` instead, which has been more reliable so far.

## Packages

- `Microsoft.SemanticKernel.Connectors.OpenAI`
- `Microsoft.Extensions.Configuration.Json` / `.EnvironmentVariables` / `.Binder`
- `Microsoft.Extensions.Logging.Console`

## Not handled yet

- No `keep_alive` set explicitly, so it falls back to Ollama's 5 minute default before the model unloads from VRAM
- Model choice is a hardcoded constant, not something you can swap without a rebuild
- `ApiKey` placeholder sits in plain json instead of user secrets

## Maybe later

- Move model selection into config too, alongside endpoint and temperature
- Actually wire up user secrets for the api key placeholder
- Check available models against Ollama at startup instead of trusting a hardcoded constant