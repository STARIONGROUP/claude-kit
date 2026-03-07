# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Purpose

`claude-kit` is a collection of Claude Code skills, hooks, slash commands, and MCP adapters. All executable scripts are .NET 10 single-file C# programs invoked directly via `dotnet run`.

## Running Scripts

```bash
dotnet run skills/<script-name>.cs [args]
dotnet run hooks/<script-name>.cs [args]
dotnet run mcp/<script-name>.cs
```

NuGet packages declared in `#:package` directives are restored automatically on first run.

## .NET 10 Single-File Script Format

Every `.cs` script in this repo uses this header pattern:

```csharp
#!/usr/bin/env dotnet-script
#:sdk Microsoft.NET.Sdk
#:property TargetFramework net10.0
#:package SomePackage@1.2.3

using System;
// top-level statements follow
```

- `#:sdk` — MSBuild SDK
- `#:property` — MSBuild property (always `TargetFramework net10.0`)
- `#:package` — NuGet package with pinned version
- Top-level statements only — no explicit `Program` class or `Main` method
- `await` is supported at top level

Shared code between scripts is included via `#load "../shared/Utility.cs"`.

## Folder Structure & Conventions

| Folder | Contents | Invocation |
|---|---|---|
| `skills/` | Runnable `.cs` scripts + companion `.md` skill files | `dotnet run skills/name.cs <args>` |
| `hooks/` | `.cs` scripts triggered by Claude Code hook events | `dotnet run hooks/name.cs` (stdin/stdout JSON) |
| `mcp/` | `.cs` MCP server adapters using stdio transport | `dotnet run mcp/name.cs` |
| `commands/` | `.md` slash command definitions | Loaded by Claude Code automatically |
| `shared/` | Utility `.cs` files for `#load` inclusion | Not run directly |

### Skills

Each skill has two files:
- `skills/<name>.cs` — the executable script
- `skills/<name>.md` — Claude Code skill markdown describing when and how to invoke the script

### Hooks

Hook scripts receive a JSON payload via stdin and write responses to stdout. Exit code 2 + stderr message blocks the action; exit code 0 allows it.

### MCP Adapters

MCP servers use `ModelContextProtocol` NuGet package with stdio transport. Register in `.claude/settings.json`:

```json
{
  "mcpServers": {
    "name": { "command": "dotnet", "args": ["run", "mcp/name.cs"] }
  }
}
```

## Output & Error Conventions

Scripts use fixed-width progress tags for scannable terminal output:

```
[OK]    path/to/file
[SKIP]  path/to/file
[ERROR] path/to/file
        ExceptionType: message
```

- All progress output goes to stdout
- Usage errors and argument validation go to stderr
- Exit code 0 on success, 1 on fatal argument/config errors
- Per-item errors are logged and processing continues (no early exit)
