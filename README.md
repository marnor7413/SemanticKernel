# LLMChatApp
A console playground for Semantic Kernel talking to a self-hosted Ollama box.

## What this is
It started as a throwaway test: point `IChatCompletionService` at a local model instead of a cloud endpoint and see whether tool calling even works. It grew from there. Now it has config with live reload, a few native plugins, two REST APIs wired in through their OpenAPI specs, chat history summarization and an approval gate on the one function that shouldn't run unattended.

None of this is production code. It's a place to try things and learn where Semantic Kernel and local models disagree with each other.

## What you need
- .NET 10 SDK
- An Ollama instance reachable over the network with a model already pulled
- The model name spelled exactly the way `ollama list` prints it

## Setup
See `Appsettings.json`:

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
    "Model": "qwen3:32b"
  }
}
```

Two things worth knowing. The `/v1` on the endpoint is deliberate. That is Ollama's OpenAI-compatible surface, which is what the OpenAI connector expects. Drop it and you end up on Ollama's native API instead, which speaks a different shape and won't behave. The `ApiKey` is a placeholder and stays ignored for as long as your Ollama doesn't check it. Move it into user secrets before you ever point this at something that does.

## Running it
```bash
dotnet run
```

Type a question. `quit` or an empty line gets you out.

## How it works
Config binds into `OllamaOptions` and `LogLevelOptions` through `IOptionsMonitor`. `KernelFactory` builds a `Kernel` once and caches it, and it stays subscribed to config changes. Edit `Model` or `Temperature` in the json while the app is running and the next question rebuilds against the new values.

The rebuild sits behind a `SemaphoreSlim` rather than a plain `lock`. The build awaits, because the two OpenAPI specs get imported while the kernel is being assembled, and you can't await inside a `lock`. The cached kernel and its prompt settings are published together as a single record, so a reader on the fast path either sees a fully built pair or nothing, never something half assembled.

The `ILoggerFactory` from the outer container is pushed into the kernel's own service container as well. Skip that and Semantic Kernel's internal logging just goes nowhere.

Chat history is summarized by `ChatHistorySummarizationReducer` once it grows past the thresholds in `MessageSummarySettings`. That keeps a long session from dragging the entire transcript along on every call.

Network errors against Ollama are caught, the unanswered question is pulled back out of history so there is no dangling user turn left behind, and the loop keeps going instead of dying on you.

## Plugins
Four native functions plus two REST APIs, all handed to the model with `FunctionChoiceBehavior.Auto()`.

Native:

- `CalculatorPlugin`: add, subtract, multiply and divide
- `TimePlugin`: current time, day of the week and a `ChangeTimeOnUsersComputer` function that is deliberately gated (see below)

From OpenAPI specs:

- weather forecast against Open-Meteo
- geocoding against Open-Meteo

### The approval gate
`ChangeTimeOnUsersComputerApprovalFilter` is an `IFunctionInvocationFilter`. It watches for one specific call, the time change on `TimePlugin`, and stops to ask for a y/n on the console before letting it through. Everything else passes straight past it. The function doesn't actually touch the system clock, it just returns a string, but it stands in for the kind of call you would never want a model firing off on its own.

### The two Open-Meteo APIs
Both specs live in `ApiSpecifications/` and are read off disk at startup instead of fetched over the wire. There is a reason for that. Open-Meteo's docs site at `open-meteo.com` answers a 404 to anything that doesn't look like a browser, so a plain `HttpClient` can't pull the spec down. The data hosts, `api.open-meteo.com` and `geocoding-api.open-meteo.com`, don't care and respond fine. So the specs are committed to the repo and the runtime calls go straight to those hosts through a named `HttpClient` from `IHttpClientFactory`.

Two things that took a while to pin down are now baked into the specs.

Coordinates are declared as `string`, not `number`. Local models tend to hand tool-call arguments back as JSON strings like `"57.7089"`, and SK's type converter refuses to turn a string into a `number` parameter and throws before the request is even built. Declaring latitude and longitude as strings skips that conversion, and Open-Meteo parses them server side either way. The same fix applies to any numeric query parameter the model might fill in.

Forecast only takes coordinates. It has no idea what "Gunnarskog" is. That is what the geocoding tool is for. It turns a place name into latitude and longitude, and the descriptions in both specs spell out the order, look the name up first then feed the coordinates into the forecast. Stronger models chain the two on their own. Smaller ones sometimes call geocoding and then forget to carry the coordinates across, which is the model's tool chaining coming up short and not the wiring.

## On picking a model
Set it in `Appsettings.json`. `qwen3:32b` has been the reliable one so far.

`qwen2.5-coder:32b` was a strange one. It produced the correct tool-call payload but left off the `<tool_call>` tags that Ollama's template expects, so the whole thing came back as plain text and never registered as a function call. Worth remembering if your tool calls quietly turn into chatter.

## Maybe later
- Validate the model against Ollama at startup instead of failing on first use
- Set a real `keep_alive` so the model stays warm between questions