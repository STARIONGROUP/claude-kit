---
name: switcher
description: Switch .csproj references between NuGet packages and local projects using a JSON config
---

# Switcher

Switch `.csproj` references between NuGet `PackageReference` and local `ProjectReference` entries using a `switcher.json` configuration file. Useful when developing across multiple .NET solutions that depend on each other.

## Instructions

When the user asks to switch references between NuGet packages and local projects, run the following command:

```bash
dotnet run skills/switcher/switcher.cs <to-project|to-nuget|status> <path-to-switcher.json>
```

Replace `<to-project|to-nuget|status>` with the desired command and `<path-to-switcher.json>` with the path to the configuration file.

### Config Format

The `switcher.json` file defines which references can be switched and which `.csproj` targets to modify. All paths are relative to the config file's directory.

```json
{
  "references": [
    {
      "package": "Acme.Core",
      "version": "2.1.0",
      "project": "../acme-core/src/Acme.Core/Acme.Core.csproj"
    }
  ],
  "targets": [
    "src/MyApp/MyApp.csproj"
  ]
}
```

- **`references[].package`** — NuGet package name (matches `<PackageReference Include="...">`).
- **`references[].version`** — Version to restore when switching back to NuGet.
- **`references[].project`** — Path to the local `.csproj` for `ProjectReference`.
- **`targets[]`** — `.csproj` files to modify.

### Behaviour

| Command | Action |
|---|---|
| `to-project` | Replace matching `PackageReference` entries with `ProjectReference` pointing at local source |
| `to-nuget` | Replace matching `ProjectReference` entries with `PackageReference` (restores version and extra attributes) |
| `status` | Report current state of each reference in each target — no file changes |

### Output Example

```
[OK]      src/MyApp/MyApp.csproj :: Acme.Core → ProjectReference
[SKIP]    src/MyApp/MyApp.csproj :: Acme.Logging (already switched)
[NUGET]   src/MyApp/MyApp.csproj :: Acme.Core (v2.1.0)
[PROJECT] src/MyApp/MyApp.csproj :: Acme.Core (../acme-core/src/Acme.Core/Acme.Core.csproj)

Done. Switched: 1 | Skipped: 1 | Errors: 0
```

### When to Use

- Local development across multiple .NET solutions that reference each other via NuGet.
- Quickly toggling between NuGet packages and local project references without manual `.csproj` editing.
- Ensuring a clean switch back to NuGet before committing changes.

### Prerequisites

- .NET 10 SDK installed and `dotnet` available on the PATH.
- No extra NuGet packages required — uses in-box `System.Text.Json` and `System.Xml.Linq`.
- A `switcher.json` config file describing the references and targets.

### Limitations

- Only switches references listed in `switcher.json` — unlisted references are not touched.
- Does not modify `Directory.Build.props` or `Directory.Packages.props` (central package management).
- Extra attributes on `PackageReference` (e.g. `PrivateAssets`, `IncludeAssets`) are preserved via XML comments and restored on round-trip, but only when the switch was performed by this tool.
