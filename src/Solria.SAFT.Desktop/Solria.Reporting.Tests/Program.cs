using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;
using ClosedXML.Excel;
using Majorsilence.Reporting.Rdl;
using Majorsilence.Reporting.UI.RdlAvalonia.Viewer;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.ViewModels;
using SolRIA.SAFT.Desktop.Views;
using System.Text;

AppBuilder.Configure<Avalonia.Application>().UseSkia()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
    .SetupWithoutStarting();
using var stop = new CancellationTokenSource();
var failed = false;
Dispatcher.UIThread.Post(async () =>
{
    try { await RunTests(); Console.WriteLine("PASS: reporting integration checks"); }
    catch (Exception ex) { failed = true; Console.Error.WriteLine(ex); }
    finally { stop.Cancel(); }
});
Dispatcher.UIThread.MainLoop(stop.Token);
return failed ? 1 : 0;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static async Task RunTests()
{
    await ExportRoutes.VerifyAsync();
    var outputDirectory = Path.Combine(AppContext.BaseDirectory, "artifacts");
    Directory.CreateDirectory(outputDirectory);
    var rows = new List<object[]> { new object[] { "00123", "Açores; \"Olá\"\nSegunda linha", 12.34m, new DateTime(2026, 10, 20), null } };
    var request = ReportRequest.FromRows(ReportTemplate.Clientes, "Clientes", ["Código", "Descrição", "Valor", "Data", "Opcional"], rows);
    request.Company = "Empresa Açores & Filhos | NIF: 123456789";
    await TemplateFiles.VerifyAsync(request);
    rows[0][0] = "changed";
    Check(request.Tables[0].Data.Rows[0]["C0"].ToString() == "00123", "Snapshot must preserve identifiers and detach mutable rows.");
    using (var workbook = new XLWorkbook(new MemoryStream(request.Excel)))
    {
        var sheet = workbook.Worksheet(1);
        Check(sheet.Cell(2, 1).DataType == XLDataType.Text && sheet.Cell(2, 1).GetString() == "00123", "Excel identifier type.");
        Check(sheet.Cell(2, 3).DataType == XLDataType.Number && sheet.Cell(2, 3).GetDouble() == 12.34, "Excel numeric type.");
        Check(sheet.Cell(2, 4).DataType == XLDataType.DateTime, "Excel date type.");
    }
    var csv = Encoding.UTF8.GetString(request.Csv());
    Check(csv.Contains("\"Açores; \"\"Olá\"\"\nSegunda linha\""), "CSV escaping.");
    Check(csv.Contains("12,34;20/10/2026;"), "Portuguese number and date formatting.");
    using var simple = await ReportRenderer.CreateAsync(request);
    var simplePages = await simple.BuildPages();
    Check(simplePages.PageCount > 0, "Simple report must produce pages.");
    Check(TemplateFiles.Texts(simplePages).Contains(request.Title) && TemplateFiles.Texts(simplePages).Contains(request.Company), "Title and company must come from RDL parameters.");
    await SavePdf(simple, Path.Combine(outputDirectory, "clientes.pdf"));
    var repeatPages = await simple.BuildPages();
    Check(TemplateFiles.Texts(repeatPages).Contains(request.Title) && TemplateFiles.Texts(repeatPages).Contains(request.Company),
        "Repeated preview/PDF renders must preserve parameters after the engine clears its cache.");

    using var source = new XLWorkbook();
    var documents = source.AddWorksheet("Documentos");
    documents.Cell(1, 1).Value = "Documento";
    documents.Cell(1, 2).Value = "Total";
    documents.Row(1).Style.Font.Bold = true;
    documents.Cell(2, 1).Value = "FT 2026/1";
    documents.Cell(2, 2).Value = 123.45;
    documents.Cell(4, 1).Value = "Código";
    documents.Cell(4, 8).Value = "Descrição";
    documents.Row(4).Style.Font.Bold = true;
    for (var r = 5; r < 155; r++)
    {
        documents.Cell(r, 1).Value = "P" + r;
        documents.Cell(r, 8).Value = "Descrição extensa com acentuação para verificar paginação e quebra de texto. " + r;
    }
    documents.Cell(157, 1).Value = "FT 2026/2";
    documents.Cell(157, 2).Value = 5;
    var taxes = source.AddWorksheet("Impostos");
    taxes.Cell(1, 1).Value = "Taxa";
    taxes.Cell(1, 2).Value = "Total";
    taxes.Cell(2, 1).Value = 23;
    taxes.Cell(2, 2).Value = 23;
    taxes.Cell(3, 2).FormulaA1 = "SUM(B2:B2)";
    var grouped = ReportRequest.FromWorkbook(ReportTemplate.FaturacaoDocumentos, source, "Documentos.xlsx");
    Check(grouped.Tables[0].Data.Rows.Count == 152, "Group headers must not be counted as data, and every document/line must survive.");
    Check(grouped.Tables[1].Values[1][1] is double total && total == 23, "Formula value must be calculated for preview.");
    using (var workbook = new XLWorkbook(new MemoryStream(grouped.Excel)))
        Check(workbook.Worksheet(2).Cell(3, 2).HasFormula, "Excel formulas must be retained.");
    using var report = await ReportRenderer.CreateAsync(grouped);
    var pages = await report.BuildPages();
    Check(pages.PageCount > 1, "Long report must paginate.");
    var texts = pages.Cast<Page>().SelectMany(page => page.OfType<PageText>()).Select(text => text.Text).ToArray();
    Check(texts.Any(text => text.Contains("FT 2026/2")), "Last document must survive pagination.");
    Check(texts.Any(text => text == "Taxa"), "Second worksheet must be visible.");
    await SavePdf(report, Path.Combine(outputDirectory, "documentos.pdf"));

    var viewer = new AvaloniaReportViewer();
    viewer.UseEmbeddedMode();
    await viewer.SetReportAsync(report);
    Check(viewer.CurrentPages?.PageCount == pages.PageCount, "Viewer pagination must match engine pagination.");
    Check(TemplateFiles.Texts(viewer.CurrentPages).Contains(grouped.Title), "The viewer must preserve report parameters when rebuilding pages.");
    using (var thumbnail = ReportCanvas.RenderPageThumbnail(viewer.CurrentPages, 0, 1200))
        thumbnail.Save(Path.Combine(outputDirectory, "documentos.png"), Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
    viewer.ReleaseReport();
    Check(viewer.CurrentPages == null, "Closing the viewer must release pages.");

    var empty = ReportRequest.FromRows(ReportTemplate.Clientes, "Vazio", ["Código"], []);
    Check(!empty.HasData, "Empty data must not open a report.");
    using (var sparseSource = new XLWorkbook())
    {
        sparseSource.AddWorksheet("Documentos").Cell(1, 1).Value = "Documento";
        var details = sparseSource.AddWorksheet("Detalhes");
        details.Cell(1, 1).Value = "Detalhe";
        details.Cell(2, 1).Value = "Detalhe presente";
        var sparse = ReportRequest.FromWorkbook(ReportTemplate.FaturacaoDocumentos, sparseSource, "Esparso.xlsx");
        Check(sparse.HasData && sparse.Tables.Count == 2 && sparse.Tables[0].Data.Rows.Count == 0 && sparse.Tables[1].Data.Rows.Count == 1,
            "An empty first table must not change Data1's index or remove its fields.");
        using var sparseReport = await ReportRenderer.CreateAsync(sparse);
        Check(TemplateFiles.Texts(await sparseReport.BuildPages()).Contains("Detalhe presente"), "The nonempty second dataset must render.");
    }
    var wide = ReportRequest.FromRows(ReportTemplate.ConferenciaDocumento, "Largo", Enumerable.Range(0, 17).Select(i => "Coluna " + i).ToArray(),
        [Enumerable.Range(0, 17).Select(i => (object)("Valor " + i)).ToArray()]);
    using var wideReport = await ReportRenderer.CreateAsync(wide);
    var widePages = await wideReport.BuildPages();
    Check(widePages.PageWidth > widePages.PageHeight, "Wide reports must use landscape.");
    await SavePdf(wideReport, Path.Combine(outputDirectory, "largo.pdf"));

    var dialogs = new TestDialogs();
    using var vm = new ReportViewModel(request, dialogs);
    await vm.InitializeAsync(_ => Task.CompletedTask);
    Check(vm.IsReady && vm.ExportExcelCommand.CanExecute(null), "Exports enabled after initialization.");
    await vm.ExportExcelCommand.ExecuteAsync(null);
    Check(dialogs.Output.ToArray().SequenceEqual(request.Excel), "Excel export replaces existing file without trailing bytes.");
    await vm.ExportCsvCommand.ExecuteAsync(null);
    Check(dialogs.Output.ToArray().SequenceEqual(request.Csv()), "CSV command exports the snapshot.");
    await vm.ExportPdfCommand.ExecuteAsync(null);
    Check(Encoding.ASCII.GetString(dialogs.Output.ToArray(), 0, 5) == "%PDF-", "PDF command exports a real document.");
    dialogs.Cancel = true;
    await vm.ExportPdfCommand.ExecuteAsync(null);
    Check(vm.IsReady && !vm.IsBusy, "Cancel preserves the report.");
    dialogs.Cancel = false;
    dialogs.Fail = true;
    await vm.ExportExcelCommand.ExecuteAsync(null);
    Check(vm.Status.Contains("Não foi possível exportar") && vm.ExportPdfCommand.CanExecute(null), "Write failures must be visible and allow retry.");
    dialogs.Fail = false;
    dialogs.Wait = new TaskCompletionSource<bool>();
    var pending = vm.ExportPdfCommand.ExecuteAsync(null);
    Check(!vm.ExportExcelCommand.CanExecute(null) && !vm.ExportCsvCommand.CanExecute(null), "All exports disabled during a pending export.");
    dialogs.Wait.SetResult(true);
    await pending;
    using var closedVm = new ReportViewModel(request, new TestDialogs());
    var initialization = closedVm.InitializeAsync(_ => throw new Exception("A closed viewer must not display a report."));
    closedVm.Dispose();
    await initialization;
    Check(!closedVm.IsReady && !closedVm.IsBusy, "Closing during initialization releases resources.");
    for (var i = 0; i < 3; i++)
    {
        using var windowVm = new ReportViewModel(request, new TestDialogs());
        var window = new ReportWindow { DataContext = windowVm };
        window.Show();
        while (windowVm.IsBusy) await Task.Delay(10);
        Check(windowVm.IsReady, "Report window must load using compiled bindings.");
        window.Close();
    }
    Console.WriteLine("Artifacts: " + outputDirectory);
}

static async Task SavePdf(Report report, string path)
{
    using var output = new MemoryStreamGen();
    await report.RunRender(output, OutputPresentationType.PDF);
    var bytes = ((MemoryStream)output.GetStream()).ToArray();
    Check(bytes.Length > 1000 && Encoding.ASCII.GetString(bytes, 0, 5) == "%PDF-", "Valid PDF output.");
    await File.WriteAllBytesAsync(path, bytes);
}
