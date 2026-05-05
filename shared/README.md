# Shared Utilities

Common C# code shared across hooks, MCP adapters, and skill scripts.

## Usage

Reference shared files via `#load`. The path is relative to the loading script:

```csharp
// From a hook (hooks/<name>.cs)
#load "../shared/JsonHelper.cs"

// From an MCP adapter (mcp/<name>.cs)
#load "../shared/JsonHelper.cs"

// From a skill script (.claude/skills/<name>/<name>.cs)
#load "../../../shared/JsonHelper.cs"
```

## Convention

```
shared/
├── README.md
└── <utility-name>.cs
```

## Guidelines

- Keep utilities focused and stateless where possible
- No project file — all files are designed for `#load` inclusion or direct `dotnet run`
- Target `net10.0` consistently across all scripts

Note: a skill that depends on `shared/` is no longer self-contained — copying just the skill folder to `~/.claude/skills/` will not bring the shared utilities along. Either inline the helper into the skill script or copy `shared/` separately.
