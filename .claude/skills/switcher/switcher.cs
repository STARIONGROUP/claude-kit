// -------------------------------------------------------------------------------------------------
//   <copyright file="switcher.cs" company="Starion Group S.A.">
//
//     Copyright (c) 2026 Starion Group S.A.
//
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
//
//   </copyright>
//   ------------------------------------------------------------------------------------------------

#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: dotnet run skills/switcher/switcher.cs <to-project|to-nuget|status> <path-to-switcher.json>");
    Environment.Exit(1);
}

var command = args[0].ToLowerInvariant();

if (command is not ("to-project" or "to-nuget" or "status"))
{
    Console.Error.WriteLine($"Error: unknown command '{args[0]}'. Expected: to-project, to-nuget, or status");
    Environment.Exit(1);
}

var configPath = Path.GetFullPath(args[1]);

if (!File.Exists(configPath))
{
    Console.Error.WriteLine($"Error: config file not found: {configPath}");
    Environment.Exit(1);
}

SwitcherConfig config;

try
{
    var json = await File.ReadAllTextAsync(configPath);
    config = JsonSerializer.Deserialize<SwitcherConfig>(json, JsonContext.Default.SwitcherConfig)!;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"Error: malformed JSON in {configPath}: {ex.Message}");
    Environment.Exit(1);
    return;
}

if (config.References is null || config.References.Length == 0)
{
    Console.Error.WriteLine("Error: no references defined in config");
    Environment.Exit(1);
}

if (config.Targets is null || config.Targets.Length == 0)
{
    Console.Error.WriteLine("Error: no targets defined in config");
    Environment.Exit(1);
}

var configDir = Path.GetDirectoryName(configPath)!;
int switched = 0, skippedCount = 0, errorCount = 0;

foreach (var targetRelative in config.Targets)
{
    var targetPath = Path.GetFullPath(Path.Combine(configDir, targetRelative));

    if (!File.Exists(targetPath))
    {
        Console.WriteLine($"[ERROR] {targetRelative} :: file not found");
        errorCount++;
        continue;
    }

    XDocument doc;

    try
    {
        doc = XDocument.Load(targetPath, LoadOptions.PreserveWhitespace);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {targetRelative}");
        Console.WriteLine($"        {ex.GetType().Name}: {ex.Message}");
        errorCount++;
        continue;
    }

    var modified = false;

    foreach (var refEntry in config.References)
    {
        var refProjectPath = Path.GetFullPath(Path.Combine(configDir, refEntry.Project));

        if (command == "status")
        {
            HandleStatus(targetRelative, refEntry, doc, targetPath, refProjectPath);
        }
        else if (command == "to-project")
        {
            if (!File.Exists(refProjectPath))
            {
                Console.WriteLine($"[ERROR] {targetRelative} :: {refEntry.Package} — reference project not found: {refEntry.Project}");
                errorCount++;
                continue;
            }

            if (HandleToProject(targetRelative, refEntry, doc, targetPath, refProjectPath))
            {
                modified = true;
                switched++;
            }
        }
        else // to-nuget
        {
            if (HandleToNuget(targetRelative, refEntry, doc, targetPath, refProjectPath))
            {
                modified = true;
                switched++;
            }
        }
    }

    if (modified)
    {
        RemoveEmptyItemGroups(doc);
        doc.Save(targetPath);
    }
}

Console.WriteLine();

if (command == "status")
{
    Console.WriteLine($"Done. Errors: {errorCount}");
}
else
{
    Console.WriteLine($"Done. Switched: {switched} | Skipped: {skippedCount} | Errors: {errorCount}");
}

// ── status ──────────────────────────────────────────────────────────────────────

void HandleStatus(string targetRelative, SwitchableRef refEntry, XDocument doc, string targetPath, string refProjectPath)
{
    var packageRef = FindPackageReference(doc, refEntry.Package);
    var projectRef = FindProjectReference(doc, targetPath, refProjectPath);

    if (projectRef is not null)
    {
        var relativePath = ComputeRelativePath(targetPath, refProjectPath);
        Console.WriteLine($"[PROJECT] {targetRelative} :: {refEntry.Package} ({relativePath})");
    }
    else if (packageRef is not null)
    {
        var version = packageRef.Attribute("Version")?.Value ?? "?";
        Console.WriteLine($"[NUGET]   {targetRelative} :: {refEntry.Package} (v{version})");
    }
    else
    {
        Console.WriteLine($"[NONE]    {targetRelative} :: {refEntry.Package}");
    }
}

// ── to-project ──────────────────────────────────────────────────────────────────

bool HandleToProject(string targetRelative, SwitchableRef refEntry, XDocument doc, string targetPath, string refProjectPath)
{
    var projectRef = FindProjectReference(doc, targetPath, refProjectPath);

    if (projectRef is not null)
    {
        Console.WriteLine($"[SKIP]    {targetRelative} :: {refEntry.Package} (already switched)");
        skippedCount++;
        return false;
    }

    var packageRef = FindPackageReference(doc, refEntry.Package);

    if (packageRef is null)
    {
        Console.WriteLine($"[SKIP]    {targetRelative} :: {refEntry.Package} (not referenced)");
        skippedCount++;
        return false;
    }

    // capture extra attributes for round-tripping
    var extraAttrs = packageRef.Attributes()
        .Where(a => a.Name.LocalName is not ("Include" or "Version"))
        .Select(a => $"{a.Name.LocalName}={a.Value}")
        .ToList();

    var relativePath = ComputeRelativePath(targetPath, refProjectPath);
    var newProjectRef = new XElement("ProjectReference", new XAttribute("Include", relativePath));

    if (extraAttrs.Count > 0)
    {
        var comment = new XComment($" switcher:{refEntry.Package}|{string.Join(";", extraAttrs)} ");
        packageRef.AddBeforeSelf(comment);
        packageRef.AddBeforeSelf(new XText("\n    "));
    }

    packageRef.ReplaceWith(newProjectRef);

    Console.WriteLine($"[OK]      {targetRelative} :: {refEntry.Package} → ProjectReference");
    return true;
}

// ── to-nuget ────────────────────────────────────────────────────────────────────

bool HandleToNuget(string targetRelative, SwitchableRef refEntry, XDocument doc, string targetPath, string refProjectPath)
{
    var packageRef = FindPackageReference(doc, refEntry.Package);

    if (packageRef is not null)
    {
        Console.WriteLine($"[SKIP]    {targetRelative} :: {refEntry.Package} (already NuGet)");
        skippedCount++;
        return false;
    }

    var projectRef = FindProjectReference(doc, targetPath, refProjectPath);

    if (projectRef is null)
    {
        Console.WriteLine($"[SKIP]    {targetRelative} :: {refEntry.Package} (not referenced)");
        skippedCount++;
        return false;
    }

    var newPackageRef = new XElement("PackageReference",
        new XAttribute("Include", refEntry.Package),
        new XAttribute("Version", refEntry.Version));

    // check for switcher comment with extra attributes — skip whitespace text nodes
    var previousNode = projectRef.PreviousNode;
    XText? gapWhitespace = null;

    if (previousNode is XText ws)
    {
        gapWhitespace = ws;
        previousNode = ws.PreviousNode;
    }

    if (previousNode is XComment comment && comment.Value.TrimStart().StartsWith($"switcher:{refEntry.Package}|"))
    {
        var pipeIndex = comment.Value.IndexOf('|');
        var attrString = comment.Value[(pipeIndex + 1)..].Trim();

        foreach (var pair in attrString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eqIndex = pair.IndexOf('=');

            if (eqIndex > 0)
            {
                var name = pair[..eqIndex];
                var value = pair[(eqIndex + 1)..];
                newPackageRef.SetAttributeValue(name, value);
            }
        }

        gapWhitespace?.Remove();
        comment.Remove();
    }

    projectRef.ReplaceWith(newPackageRef);

    Console.WriteLine($"[OK]      {targetRelative} :: {refEntry.Package} → PackageReference (v{refEntry.Version})");
    return true;
}

// ── helpers ─────────────────────────────────────────────────────────────────────

XElement? FindPackageReference(XDocument doc, string packageName)
{
    return doc.Descendants("PackageReference")
        .FirstOrDefault(e => string.Equals(e.Attribute("Include")?.Value, packageName, StringComparison.OrdinalIgnoreCase));
}

XElement? FindProjectReference(XDocument doc, string targetPath, string refProjectPath)
{
    var targetDir = Path.GetDirectoryName(targetPath)!;

    return doc.Descendants("ProjectReference")
        .FirstOrDefault(e =>
        {
            var include = e.Attribute("Include")?.Value;

            if (include is null)
            {
                return false;
            }

            var resolvedPath = Path.GetFullPath(Path.Combine(targetDir, include.Replace('/', Path.DirectorySeparatorChar)));
            return string.Equals(resolvedPath, refProjectPath, StringComparison.OrdinalIgnoreCase);
        });
}

string ComputeRelativePath(string targetPath, string refProjectPath)
{
    var targetDir = Path.GetDirectoryName(targetPath)!;
    var relative = Path.GetRelativePath(targetDir, refProjectPath);
    return relative.Replace('\\', '/');
}

void RemoveEmptyItemGroups(XDocument doc)
{
    var emptyGroups = doc.Descendants("ItemGroup")
        .Where(ig => !ig.HasElements && !ig.Nodes().OfType<XComment>().Any())
        .ToList();

    foreach (var group in emptyGroups)
    {
        group.Remove();
    }
}

// ── records & JSON context ──────────────────────────────────────────────────────

record SwitcherConfig(
    [property: JsonPropertyName("references")] SwitchableRef[] References,
    [property: JsonPropertyName("targets")] string[] Targets);

record SwitchableRef(
    [property: JsonPropertyName("package")] string Package,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("project")] string Project);

[JsonSerializable(typeof(SwitcherConfig))]
partial class JsonContext : JsonSerializerContext { }
