// -------------------------------------------------------------------------------------------------
//   <copyright file="pdf-extract.cs" company="Starion Group S.A.">
//
//     Copyright (c) 2026 Starion Group S.A.
//
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
//     Unless required by applicable law or agreed to in writing, softwareUseCases
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
//
//   </copyright>
//   ------------------------------------------------------------------------------------------------

#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:package PdfPig@0.1.13

using System;
using System.IO;
using System.Linq;
using System.Text;

using UglyToad.PdfPig;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run skills/pdf-extract.cs <rootFolder>");
    Environment.Exit(1);
}

var rootFolder = Path.GetFullPath(args[0]);

if (!Directory.Exists(rootFolder))
{
    Console.Error.WriteLine($"Error: directory not found: {rootFolder}");
    Environment.Exit(1);
}

Console.WriteLine($"Scanning: {rootFolder}");
Console.WriteLine();

var pdfFiles = Directory.EnumerateFiles(rootFolder, "*.pdf", SearchOption.AllDirectories);

int total = 0, processed = 0, skipped = 0, errors = 0;

foreach (var filePath in pdfFiles)
{
    total++;
    var outputPath = filePath + ".txt";

    if (File.Exists(outputPath))
    {
        Console.WriteLine($"[SKIP]  {filePath}");
        skipped++;
        continue;
    }

    try
    {
        var sb = new StringBuilder();

        using (var doc = PdfDocument.Open(filePath))
        {
            foreach (var page in doc.GetPages())
            {
                var line = string.Join(" ", page.GetWords().Select(w => w.Text));
                sb.AppendLine(line);
            }
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        Console.WriteLine($"[OK]    {filePath}");
        processed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {filePath}");
        Console.WriteLine($"        {ex.GetType().Name}: {ex.Message}");
        errors++;
    }
}

Console.WriteLine();
Console.WriteLine($"Done. Total: {total} | Processed: {processed} | Skipped: {skipped} | Errors: {errors}");
