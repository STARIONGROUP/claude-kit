# Hooks

Hooks are shell commands (or scripts) that Claude Code executes in response to lifecycle events.
Scripts in this folder are implemented as .NET 10 single-file C# programs.

## Running a Hook Script

```bash
dotnet run hooks/<script-name>.cs
```

## Hook Events

| Event | Trigger |
|---|---|
| `PreToolCall` | Before Claude calls any tool |
| `PostToolCall` | After a tool call completes |
| `UserPromptSubmit` | When the user submits a prompt |
| `Stop` | When Claude finishes a response turn |
| `SubagentStop` | When a subagent finishes |

## Convention

```
hooks/
├── README.md
└── <event>-<description>.cs
```

Example: `pre-tool-call-logger.cs`, `post-tool-call-validator.cs`

## Anatomy of a Hook Script

Hook scripts receive a JSON payload via stdin and write a JSON response to stdout.

```csharp
#:sdk Microsoft.NET.Sdk
#:property TargetFramework net10.0

using System.Text.Json;

var input = await Console.In.ReadToEndAsync();
var payload = JsonSerializer.Deserialize<JsonElement>(input);

// Process payload...

// To block the action, exit with code 2 and write reason to stderr
// To allow, exit with code 0
Console.Error.WriteLine("Hook reason if blocking");
Environment.Exit(0);
```

## References

- [Claude Code Hooks Documentation](https://docs.anthropic.com/en/docs/claude-code/hooks)
