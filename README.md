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
| `DEVELOPMENT_STANDARDS.md` | Org-wide C#/.NET engineering conventions | Copied into a repo and referenced from its `CLAUDE.md` |

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

## Using the Engineering Standards

`DEVELOPMENT_STANDARDS.md` holds org-wide C#/.NET engineering conventions (code style, LINQ, test conventions, exceptions, documentation, repo hygiene, branch/PR workflow, AI agent collaboration, generated-code policy). Unlike skills/hooks/MCP adapters, it isn't invoked — it's meant to be copied into a repo and pulled into that repo's `CLAUDE.md` by reference.

Copy the file into the target repo's root:

```bash
cp DEVELOPMENT_STANDARDS.md /path/to/other-repo/DEVELOPMENT_STANDARDS.md
```

Then reference it near the top of that repo's own `CLAUDE.md`:

```markdown
## Engineering standards (MANDATORY — non-negotiable)
@DEVELOPMENT_STANDARDS.md
```

Claude Code resolves the `@` reference relative to the repo root and inlines the file's contents automatically — no manual copy-pasting into `CLAUDE.md`. Project-specific overrides or extensions belong below the reference, in the repo's own `CLAUDE.md`, per the "Project-specific overrides" section of the standards doc itself.

## License

Copyright (c) 2025-2026 — All rights reserved.
