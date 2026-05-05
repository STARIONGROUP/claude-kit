# claude-kit

A team-shareable collection of Claude Code skills, hooks, and MCP adapters built as .NET 10 single-file C# scripts. Designed for the .NET ecosystem but not limited to it.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — NuGet packages are restored automatically on first run.

## Repository Structure

| Path | Contents | Discovery |
|---|---|---|
| `.claude/skills/<name>/` | `SKILL.md` + `<name>.cs` | Auto-discovered as `/<name>` slash command |
| `hooks/<name>.cs` | Hook scripts | Register in `.claude/settings.json` under `hooks` |
| `mcp/<name>.cs` | MCP stdio servers | Register in `.mcp.json` or `~/.claude.json` |
| `shared/*.cs` | `#load` includes | Not run directly |

Only `.claude/skills/` is auto-discovered. Hooks and MCP servers must be explicitly registered (see below).

## Available Skills

| Skill | Slash Command | Description |
|---|---|---|
| `pdf-extract` | `/pdf-extract` | Extract text from PDF files or folders of PDFs as sidecar `.txt` files |
| `xl-extract` | `/xl-extract` | Extract Excel worksheets as CSV-formatted `.txt` files from workbooks or folders |
| `switcher` | `/switcher` | Switch `.csproj` references between NuGet packages and local projects using a JSON config |

## Installation

A skill folder is the unit of distribution — copy `.claude/skills/<name>/` to one of:

```bash
# Per-machine (available in every project)
cp -r .claude/skills/<name> ~/.claude/skills/<name>

# Per-project (checked into a different repo)
cp -r .claude/skills/<name> /path/to/other-repo/.claude/skills/<name>
```

Or work directly in this clone — Claude Code auto-discovers `.claude/skills/` from the project root.

Skill scripts use `${CLAUDE_SKILL_DIR}` for self-paths, so no edits are needed after copying.

## Usage

Once installed, invoke skills via the slash command in Claude Code:

```
/pdf-extract /path/to/documents
/pdf-extract /path/to/single-file.pdf

/xl-extract /path/to/spreadsheets
/xl-extract /path/to/workbook.xlsx

/switcher status path/to/switcher.json
/switcher to-project path/to/switcher.json
/switcher to-nuget path/to/switcher.json
```

Or run the underlying script directly:

```bash
dotnet run ".claude/skills/pdf-extract/pdf-extract.cs" /path/to/documents
dotnet run ".claude/skills/xl-extract/xl-extract.cs" /path/to/spreadsheets
dotnet run ".claude/skills/switcher/switcher.cs" status path/to/switcher.json
```

## Registering Hooks

Hooks aren't auto-discovered. Add them to your `.claude/settings.json`:

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          { "type": "command", "command": "dotnet run hooks/<name>.cs" }
        ]
      }
    ]
  }
}
```

See `hooks/README.md` for the supported events and the script template.

## Registering MCP Adapters

Add to `.mcp.json` (project-level, checked in) or `~/.claude.json` (user-level):

```json
{
  "mcpServers": {
    "<name>": {
      "command": "dotnet",
      "args": ["run", "mcp/<name>.cs"]
    }
  }
}
```

See `mcp/README.md` for the adapter template.

## License

Copyright (c) 2025-2026 — All rights reserved.
