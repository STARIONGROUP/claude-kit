// -------------------------------------------------------------------------------------------------
//   <copyright file="xl-extract.cs" company="Starion Group S.A.">
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
#:package ClosedXML@0.102.3

using System;
using System.IO;
using System.Linq;
using System.Text;

using ClosedXML.Excel;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run skills/xl-extract.cs <rootFolder|file.xlsx>");
    Environment.Exit(1);
}

var inputPath = Path.GetFullPath(args[0]);

IEnumerable<string> excelFiles;

if (Directory.Exists(inputPath))
{
    Console.WriteLine($"Scanning: {inputPath}");
    Console.WriteLine();
    var xlsx = Directory.EnumerateFiles(inputPath, "*.xlsx", SearchOption.AllDirectories);
    var xlsm = Directory.EnumerateFiles(inputPath, "*.xlsm", SearchOption.AllDirectories);
    var xls  = Directory.EnumerateFiles(inputPath, "*.xls",  SearchOption.AllDirectories);
    excelFiles = xlsx.Concat(xlsm).Concat(xls);
}
else if (File.Exists(inputPath))
{
    Console.WriteLine($"Processing: {inputPath}");
    Console.WriteLine();
    excelFiles = [inputPath];
}
else
{
    Console.Error.WriteLine($"Error: path not found: {inputPath}");
    Environment.Exit(1);
    return;
}

int total = 0, processed = 0, skipped = 0, errors = 0;

string SafeName(string name) =>
    string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

string CsvQuote(string value)
{
    var flat = value.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
    return $"\"{flat.Replace("\"", "\"\"")}\"";
}

foreach (var filePath in excelFiles)
{
    total++;
    var dir          = Path.GetDirectoryName(filePath)!;
    var workbookName = Path.GetFileName(filePath);

    try
    {
        using var workbook = new XLWorkbook(filePath);

        foreach (var worksheet in workbook.Worksheets)
        {
            var sheetName  = worksheet.Name;
            var label      = $"{filePath} :: {sheetName}";
            var outputPath = Path.Combine(dir, $"{workbookName}-{SafeName(sheetName)}.txt");

            if (File.Exists(outputPath))
            {
                Console.WriteLine($"[SKIP]  {label}");
                skipped++;
                continue;
            }

            try
            {
                var sb = new StringBuilder();

                var lastCell = worksheet.LastCellUsed();
                if (lastCell != null)
                {
                    int lastCol = lastCell.Address.ColumnNumber;

                    foreach (var row in worksheet.RowsUsed())
                    {
                        var cells = Enumerable.Range(1, lastCol)
                            .Select(col => CsvQuote(row.Cell(col).CachedValue.ToString() ?? string.Empty));
                        sb.AppendLine(string.Join(",", cells));
                    }
                }

                await File.WriteAllTextAsync(outputPath, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                Console.WriteLine($"[OK]    {label}");
                processed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {label}");
                Console.WriteLine($"        {ex.GetType().Name}: {ex.Message}");
                errors++;
            }
        }
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
