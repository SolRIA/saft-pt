# Majorsilence Reporting Avalonia viewer

Source: https://github.com/majorsilence/Reporting

Revision: `69e6065811da818e26eb96a1fd92da15d76e4c82` (26.0.8).
The seven files in Viewer/ are from Majorsilence.Reporting.UI.RdlAvalonia/Viewer/.
Upstream licenses are included alongside this document.

The upstream Avalonia viewer project is not packable; the published UI package
depends on a nonexistent UI.RdlAvalonia package. This local project builds that
viewer against the published engine and the application's Avalonia version.
The engine uses the `RdlEngine.SkiaSharp` package (26.0.8), matching the viewer's
Majorsilence.Forms.Drawing backend. Custom report-item extensions are unnecessary
for these table-based reports.

Local adaptations: embedded mode hides file/open/export/print/reload actions
and their shortcuts, exposes resource cleanup, fixes bitmap/cancellation cleanup,
updates bitmap encoding to Avalonia 12, and translates the viewer's
visible controls to Portuguese. Export is managed by the host application to
preserve its original Excel data, worksheets and formulas.
