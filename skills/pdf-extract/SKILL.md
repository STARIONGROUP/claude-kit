---
name: pdf-extract
description: Extract text from PDF files or folders of PDFs and save as sidecar .txt files
---

# PDF Extract

Extract text from PDF files — either a single `.pdf` file or all PDFs recursively under a folder — and save each result as a `.txt` file alongside the source PDF.

## Instructions

When the user asks to extract text from a PDF file or from PDFs in a folder, run the following command:

```bash
dotnet run skills/pdf-extract/pdf-extract.cs <rootFolder|file.pdf>
```

Replace `<rootFolder|file.pdf>` with the absolute or relative path to a directory or a single `.pdf` file.

### Behaviour

- **Directory mode:** recursively finds every `.pdf` file under the given folder and all subfolders.
- **Single-file mode:** processes only the specified `.pdf` file directly.
- For each PDF, creates a sidecar text file named `<filename>.pdf.txt` in the same directory as the source PDF.
- Skips any PDF that already has a corresponding `.txt` file (idempotent — safe to re-run).
- Logs one tagged line per file: `[OK]`, `[SKIP]`, or `[ERROR]`.
- Prints a summary at the end: total files found, processed, skipped, and errors.

### Output Example

```
Scanning: /data/documents

[SKIP]  /data/documents/archive/old-report.pdf
[OK]    /data/documents/2025/annual-report.pdf
[ERROR] /data/documents/corrupt.pdf
        PdfDocumentEncryptedException: The document is encrypted and cannot be read without a password.

Done. Total: 3 | Processed: 1 | Skipped: 1 | Errors: 1
```

### When to Use

- Bulk PDF ingestion pipelines where extracted text is needed for indexing or analysis.
- Direct extraction from a single PDF file without scanning a whole directory.
- Pre-processing step before feeding document content to Claude.
- Any workflow requiring plaintext versions of PDF files without manual extraction.

### Prerequisites

- .NET 10 SDK installed and `dotnet` available on the PATH.
- The `UglyToad.PdfPig@0.1.13` NuGet package is restored automatically on first run.
- Write access to the directories containing the PDFs (for creating `.txt` output files).

### Limitations

- Does not extract text from scanned/image-only PDFs (no OCR). Those will produce empty or near-empty `.txt` files.
- Password-protected PDFs will produce an `[ERROR]` entry and be skipped.
- Very large PDFs may take noticeable time; extraction is sequential and single-threaded.
