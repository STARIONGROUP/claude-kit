# claude-kit

A collection of Claude Code skills, hooks, slash commands, and MCP adapters built as .NET 10 single-file C# scripts. Designed for the .NET ecosystem but not limited to it.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — NuGet packages are restored automatically on first run.

## Repository Structure

| Folder | Contents | Invocation |
|---|---|---|
| `skills/` | Skill subdirectories, each with a `SKILL.md` + `.cs` script | `dotnet run skills/name/name.cs <args>` |
| `hooks/` | `.cs` scripts triggered by Claude Code hook events | `dotnet run hooks/name.cs` (stdin/stdout JSON) |
| `mcp/` | `.cs` MCP server adapters using stdio transport | `dotnet run mcp/name.cs` |
| `commands/` | `.md` slash command definitions | Loaded by Claude Code automatically |
| `shared/` | Utility `.cs` files for `#load` inclusion | Not run directly |

## Available Skills

| Skill | Description |
|---|---|
| `pdf-extract` | Extract text from PDF files or folders of PDFs and save as sidecar `.txt` files |
| `xl-extract` | Extract Excel worksheets as CSV-formatted `.txt` files from workbooks or folders |

## Usage

### Extract text from PDFs

```bash
dotnet run skills/pdf-extract/pdf-extract.cs /path/to/documents
dotnet run skills/pdf-extract/pdf-extract.cs /path/to/single-file.pdf
```

### Extract Excel worksheets to CSV

```bash
dotnet run skills/xl-extract/xl-extract.cs /path/to/spreadsheets
dotnet run skills/xl-extract/xl-extract.cs /path/to/workbook.xlsx
```

### Register an MCP adapter

Add to `~/.claude.json` (user-level) or `.mcp.json` (project-level):

```json
{
  "mcpServers": {
    "adapter-name": {
      "command": "dotnet",
      "args": ["run", "mcp/adapter-name.cs"]
    }
  }
}
```

## License

Copyright (c) 2025-2026 — All rights reserved.
