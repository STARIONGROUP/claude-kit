# MCP Adapters

MCP (Model Context Protocol) adapters expose tools, resources, and prompts to Claude Code via the MCP standard.
Adapters in this folder are implemented as .NET 10 single-file C# programs using stdio transport.

> **MCP adapters are not auto-discovered.** A `.cs` file in this folder does nothing until it is registered.

## Registering with Claude Code

Add to `.mcp.json` at the project root (checked into version control, shared with the team):

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

For a personal/cross-project adapter, register in `~/.claude.json` instead — or use the CLI:

```bash
claude mcp add my-adapter dotnet run mcp/my-adapter.cs
```

Claude Code starts the process and speaks JSON-RPC 2.0 over stdin/stdout.

## Anatomy of an MCP Adapter Script

```csharp
#!/usr/bin/env dotnet-script
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

Verify the `ModelContextProtocol` package version against the [latest on NuGet](https://www.nuget.org/packages/ModelContextProtocol) when adding a new adapter — the SDK is still in preview and the API may shift.

## References

- [Model Context Protocol Documentation](https://modelcontextprotocol.io)
- [MCP C# SDK (ModelContextProtocol NuGet)](https://www.nuget.org/packages/ModelContextProtocol)
- [Claude Code MCP Documentation](https://code.claude.com/docs/en/mcp)
