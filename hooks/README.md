# Hooks

Hooks are commands or scripts that Claude Code executes in response to lifecycle events.
Scripts in this folder are implemented as .NET 10 single-file C# programs.

> **Hook scripts are not auto-discovered.** Putting a `.cs` file in this folder does nothing on its own — you must register it in `.claude/settings.json` (project) or `~/.claude/settings.json` (user) under the `hooks` key.

## Registering a Hook

Each hook entry binds an event (and optional matcher) to a command:

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          { "type": "command", "command": "dotnet run hooks/pre-bash-guard.cs" }
        ]
      }
    ]
  }
}
```

- `matcher` — restricts the hook to specific tool names (e.g. `"Bash"`, `"Edit"`, `"Write"`). Omit to match all.
- `type: "command"` runs a shell command. Other types include `prompt`, `agent`, `http`, and `mcp_tool`.
- `command` is invoked from the project root, so relative paths like `hooks/<name>.cs` work.

## Common Hook Events

| Event | Trigger |
|---|---|
| `SessionStart` | Claude Code session starts |
| `UserPromptSubmit` | User submits a prompt |
| `PreToolUse` | Before any tool call |
| `PostToolUse` | After a tool call completes |
| `PostToolUseFailure` | After a tool call fails |
| `PermissionRequest` | Claude requests permission for a tool |
| `Notification` | Claude Code sends a notification |
| `SubagentStart` / `SubagentStop` | Subagent lifecycle |
| `Stop` | Claude finishes a response turn |
| `PreCompact` / `PostCompact` | Around context compaction |
| `SessionEnd` | Session ends |

The full list is in the [hooks reference](https://code.claude.com/docs/en/hooks).

## Anatomy of a Hook Script

Hook scripts receive a JSON payload on stdin and write a JSON response (or nothing) on stdout. Exit code `0` allows the action; exit code `2` plus a stderr message blocks it.

```csharp
#!/usr/bin/env dotnet-script
#:sdk Microsoft.NET.Sdk
#:property TargetFramework net10.0

using System.Text.Json;

var input = await Console.In.ReadToEndAsync();
var payload = JsonSerializer.Deserialize<JsonElement>(input);

// Inspect the event payload, then either:
//   - exit 0 to allow,
//   - exit 2 with a stderr message to block.

Environment.Exit(0);
```

To share `#load` utilities, scripts in this folder use the relative path `#load "../shared/<name>.cs"`.

## References

- [Claude Code Hooks Documentation](https://code.claude.com/docs/en/hooks)
