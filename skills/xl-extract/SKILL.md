---
name: xl-extract
description: Extract Excel worksheets as CSV-formatted .txt files from workbooks or folders
---

# Excel Extract

Extract each worksheet from Excel workbooks as a CSV-formatted `.txt` file, placed alongside the source file. Accepts a folder (recursive) or a single `.xlsx`/`.xls` file.

## Instructions

When the user asks to extract or convert Excel spreadsheets to CSV or text, run the following command:

```bash
dotnet run skills/xl-extract/xl-extract.cs <rootFolder|file.xlsx>
```

Replace the argument with the absolute or relative path to a directory or a single Excel file.

### Behaviour

- **Folder mode**: recursively finds every `.xlsx` and `.xls` file under the given directory and all subfolders.
- **Single-file mode**: processes the specified workbook directly.
- For each worksheet, creates a sidecar file named `<workbookname>-<sheetname>.txt` in the same directory as the source workbook.
- Output is RFC 4180-compliant CSV: cells containing commas, double-quotes, or newlines are wrapped in double-quotes, and embedded double-quotes are escaped as `""`.
- Skips any sheet whose output file already exists (idempotent — safe to re-run).
- Logs one tagged line per sheet: `[OK]`, `[SKIP]`, or `[ERROR]`.
- Prints a summary at the end: total workbooks found, sheets processed, skipped, and errors.

### Output Example

```
Scanning: /data/spreadsheets

[OK]    /data/spreadsheets/Budget.xlsx :: Q1 Sales
[OK]    /data/spreadsheets/Budget.xlsx :: Q2 Sales
[SKIP]  /data/spreadsheets/Budget.xlsx :: Summary
[ERROR] /data/spreadsheets/Locked.xlsx :: Sheet1
        InvalidOperationException: The workbook is password protected.

Done. Total: 2 | Processed: 2 | Skipped: 1 | Errors: 1
```

### When to Use

- Bulk ingestion pipelines where spreadsheet data is needed as structured text for indexing or analysis.
- Pre-processing step before feeding tabular data to Claude.
- Any workflow requiring plaintext CSV versions of Excel sheets without manual export.

### Prerequisites

- .NET 10 SDK installed and `dotnet` available on the PATH.
- The `ClosedXML@0.102.3` NuGet package is restored automatically on first run.
- Write access to the directories containing the workbooks (for creating `.txt` output files).

### Limitations

- `.xlsm` macro-enabled workbooks are opened in read-only mode; macros are not executed.
- Charts, images, and other embedded objects are ignored — only cell values are exported.
- Formula results use the cached values stored in the workbook; formulas are not recalculated.
- Password-protected workbooks will produce an `[ERROR]` entry and be skipped.
