# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Purpose

`claude-kit` is a collection of Claude Code skills, hooks, and MCP server adapters intended for sharing within a team. All executable scripts are .NET 10 single-file C# programs invoked directly via `dotnet run`. Some exceptions use Python, but the preferred language is C#.

The kit supports two usage patterns:
1. **In-repo** — clone the repo and work in it; skills under `.claude/skills/` are auto-discovered by Claude Code.
2. **Copy out** — copy a single skill folder to `~/.claude/skills/<name>/` (user-level) or another project's `.claude/skills/<name>/` (project-level). Each skill is self-contained and location-independent.

## Folder Structure & Discovery

| Path | Contents | Discovery |
|---|---|---|
| `.claude/skills/<name>/` | `SKILL.md` + `<name>.cs` | **Auto-discovered** as `/<name>` slash command |
| `hooks/<name>.cs` | Hook scripts (event handlers) | **Not** auto-discovered — register in `.claude/settings.json` under `hooks` |
| `mcp/<name>.cs` | MCP stdio server adapters | **Not** auto-discovered — register in `.mcp.json` (project) or `~/.claude.json` (user) |
| `shared/*.cs` | Utility files for `#load` inclusion | Not run directly |

Anything under `.claude/skills/` is picked up by Claude Code automatically — no manifest, no settings entry. Hooks and MCP servers exist in this repo as a script library; their location is irrelevant to discovery, only the explicit registration is.

## Running Scripts

```bash
# Skill (run from anywhere — ${CLAUDE_SKILL_DIR} is injected by Claude Code)
dotnet run "${CLAUDE_SKILL_DIR}/<name>.cs" [args]

# Hook (invoked by Claude Code per the matching event in settings.json; reads JSON from stdin)
dotnet run hooks/<name>.cs

# MCP server (started by Claude Code per .mcp.json; stdio transport)
dotnet run mcp/<name>.cs
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

Shared code is included via `#load`. Path is relative to the script:
- From a hook: `#load "../shared/Utility.cs"`
- From a skill: `#load "../../../shared/Utility.cs"` (skills are nested deeper under `.claude/skills/<name>/`)

## Skills

Each skill lives in `.claude/skills/<name>/`:
- `SKILL.md` — YAML frontmatter (`name`, `description`) plus instructions for Claude.
- `<name>.cs` — the executable script the SKILL.md tells Claude to run.

The `name` in frontmatter and the folder name should match. Skill instructions invoke the bundled script using `${CLAUDE_SKILL_DIR}` so the skill works wherever it's installed:

```bash
dotnet run "${CLAUDE_SKILL_DIR}/<name>.cs" <args>
```

### Sharing a skill

A skill folder is the unit of distribution. To share:

```bash
# Make available in every project on this machine
cp -r .claude/skills/<name> ~/.claude/skills/<name>

# Drop into another project
cp -r .claude/skills/<name> /path/to/other-repo/.claude/skills/<name>
```

No path patching required — `${CLAUDE_SKILL_DIR}` resolves correctly in any of those locations.

## Hooks

Hook scripts receive a JSON event payload via stdin and write a response (or nothing) to stdout. Exit code `0` allows the action; exit code `2` plus a stderr message blocks it.

Hooks are not auto-discovered. Register each one in `.claude/settings.json`:

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

See `hooks/README.md` for the event vocabulary and the script template.

## MCP Adapters

MCP servers use the `ModelContextProtocol` NuGet package with stdio transport. Register in `.mcp.json` (project-level, checked in) or `~/.claude.json` (user-level):

```json
{
  "mcpServers": {
    "<name>": { "command": "dotnet", "args": ["run", "mcp/<name>.cs"] }
  }
}
```

Claude Code starts the process and speaks to it over stdin/stdout.

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
