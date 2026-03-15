# MCP Adapters

MCP (Model Context Protocol) adapters expose tools, resources, and prompts to Claude Code via the MCP standard.
Adapters in this folder are implemented as .NET 10 single-file C# programs using stdio transport.

## Running an MCP Adapter

```bash
dotnet run mcp/<adapter-name>.cs
```

## Registering with Claude Code

Add to your `~/.claude.json` (user-level) or `.mcp.json` (project-level):

```json
{
  "mcpServers": {
    "my-adapter": {
      "command": "dotnet",
      "args": ["run", "mcp/my-adapter.cs"]
    }
  }
}
```

## Convention

```
mcp/
├── README.md
└── <adapter-name>.cs
```

## Anatomy of an MCP Adapter Script

MCP servers communicate via JSON-RPC 2.0 over stdio.

```csharp
#:sdk Microsoft.NET.Sdk
#:property TargetFramework net10.0
#:package ModelContextProtocol@0.3.0-preview.2

using ModelContextProtocol.Server;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<MyTools>();

await builder.Build().RunAsync();
```

## References

- [Model Context Protocol Documentation](https://modelcontextprotocol.io)
- [MCP C# SDK (ModelContextProtocol NuGet)](https://www.nuget.org/packages/ModelContextProtocol)
- [Claude Code MCP Documentation](https://code.claude.com/docs/en/mcp)
