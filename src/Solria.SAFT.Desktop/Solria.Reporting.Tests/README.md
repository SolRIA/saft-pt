# Reporting integration checks

Run from the solution directory:

```powershell
dotnet run --project Solria.Reporting.Tests/Solria.Reporting.Tests.csproj
```

This executable exits with a nonzero code on failure. It uses Avalonia Headless
and does not open native windows. It checks snapshot isolation, Excel types and
formulas, CSV escaping, Portuguese formatting, grouped and multi-sheet reports,
pagination, landscape orientation, actual PDF generation, viewer loading,
export commands, file replacement, cancellation, failure/retry, concurrent export
availability, and repeated window opening/closing.
It also invokes all 20 existing export routes with representative data and
renders their output, including documents with missing optional customer/tax fields.
Each route must map to a different external RDL. The checks verify stable dataset
schemas, title/company parameters, and reload a modified template without rebuilding.
Missing files, invalid XML, invalid expressions and missing dataset fields must
produce Portuguese errors naming the template. Tests only modify copies inside
their output directory and restore the original bytes in a `finally` block.

Generated sample PDFs and a viewer rendering are in `bin/<configuration>/net10.0-windows/artifacts/`.
The checks reuse the application's existing Avalonia license at build time.
