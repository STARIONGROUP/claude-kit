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
| `PreToolUse` | Before Claude calls any tool |
| `PostToolUse` | After a tool call completes |
| `PostToolUseFailure` | After a tool call fails |
| `UserPromptSubmit` | When the user submits a prompt |
| `Stop` | When Claude finishes a response turn |
| `SubagentStop` | When a subagent finishes |
| `SubagentStart` | When a subagent starts |
| `SessionStart` | When a Claude Code session starts |
| `SessionEnd` | When a Claude Code session ends |
| `PermissionRequest` | When Claude requests permission for a tool |
| `Notification` | When Claude Code sends a notification |
| `PreCompact` | Before context is compacted |

## Convention

```
hooks/
├── README.md
└── <event>-<description>.cs
```

Example: `pre-tool-use-logger.cs`, `post-tool-use-validator.cs`

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

- [Claude Code Hooks Documentation](https://code.claude.com/docs/en/hooks)
