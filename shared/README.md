# Shared Utilities

Common C# code shared across hooks and MCP adapter scripts.

## Usage

Reference shared files from other scripts using `#load`:

```csharp
#load "../shared/JsonHelper.cs"
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
