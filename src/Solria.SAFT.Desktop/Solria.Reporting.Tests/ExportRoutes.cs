using Microsoft.Extensions.DependencyInjection;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.ViewModels;
using SolRIA.SAFT.Parser.Models;
using System.Reflection;
using System.Runtime.CompilerServices;

internal static class ExportRoutes
{
    public static async Task VerifyAsync()
    {
        var reports = new CaptureReports();
        var templates = new HashSet<ReportTemplate>();
        var services = new ServiceCollection();
        services.AddSingleton<IReportService>(reports);
        AppBootstrap.ConfigureServices(services);
        var date = new DateTime(2026, 10, 20);
        // Isolate export commands from page initialization, database access and selection callbacks.
        await Verify<SaftCustomersPageViewModel>("OnDoPrint", "List", new Customer[] { new() { CustomerID = "001", CustomerTaxID = "123456789", CompanyName = "Cliente; Açores" } });
        await Verify<SaftSuppliersPageViewModel>("OnDoPrint", "Suppliers", new Supplier[] { new() { SupplierID = "001", CompanyName = "Fornecedor" } });
        await Verify<SaftProductsPageViewModel>("OnDoPrint", "Products", new Product[] { new() { ProductCode = "001", ProductDescription = "Produto" } });
        await Verify<SaftTaxesPageViewModel>("OnDoPrint", "Taxes", new TaxTableEntry[] { new() { TaxCode = "NOR", TaxPercentage = 23, TaxExpirationDate = date } });
        await Verify<SaftErrorPageViewModel>("OnDoPrint", "Errors", new ValidationError[] { new() { Field = "Código", Description = "Descrição" } });
        await Verify<StocksProductsPageViewModel>("OnDoPrint", "Products", new Stock[] { new() { ProductCode = "001", ClosingStockQuantity = 12.34m } });
        var invoice = new SourceDocumentsSalesInvoicesInvoice
        {
            InvoiceNo = "FT 2026/1", InvoiceDate = date,
            Line = [new() { LineNumber = "1", ProductCode = "001", ProductDescription = "Produto", Quantity = 2, UnitPrice = 10, CreditAmount = 20 }]
        };
        await Verify<SaftInvoicesPageViewModel>("OnDoPrint", "Documents", new[] { invoice });
        var invoiceList = reports.Last;
        if (invoiceList.Tables.Count != 2 || invoiceList.Tables[1].Data.Rows.Count != 2)
            throw new Exception("Invoice details must preserve both the document and its line.");
        await Verify<SaftInvoicesPageViewModel>("OnDoPrintTaxes", "Documents", new[] { invoice });
        await Verify<SaftInvoicesPageViewModel>("OnSaveExcel", "currentInvoice", invoice);
        var movement = new SourceDocumentsMovementOfGoodsStockMovement
        {
            DocumentNumber = "GT 2026/1", MovementDate = date,
            Line = [new() { LineNumber = "1", ProductCode = "001", Quantity = 2, UnitPrice = 10, CreditAmount = 20 }]
        };
        await Verify<SaftMovementOfGoodsPageViewModel>("OnSaveExcel", "Documents", new[] { movement });
        await Verify<SaftMovementOfGoodsPageViewModel>("OnDoPrintTaxes", "Documents", new[] { movement });
        if (reports.Last.Tables[0].Headers[0] != "Documento" || reports.Last.Tables[0].Data.Rows.Count != 2)
            throw new Exception("Movement tax exports must preserve header, tax row and total.");
        await Verify<SaftMovementOfGoodsPageViewModel>("OnOpenExcel", "currentDocument", movement);
        var working = new SourceDocumentsWorkingDocumentsWorkDocument
        {
            DocumentNumber = "PF 2026/1", WorkDate = date,
            Line = [new() { LineNumber = "1", ProductCode = "001", TaxPointDate = date, Quantity = 2, UnitPrice = 10, CreditAmount = 20 }]
        };
        await Verify<SaftWorkingDocumentsPageViewModel>("OnDoPrint", "Documents", new[] { working });
        await Verify<SaftWorkingDocumentsPageViewModel>("OnDoPrintTaxes", "Documents", new[] { working });
        await Verify<SaftWorkingDocumentsPageViewModel>("OnSaveExcel", "currentDocument", working);
        var payment = new SourceDocumentsPaymentsPayment
        {
            PaymentRefNo = "RC 2026/1", SystemEntryDate = date,
            Line = [new() { LineNumber = "1", CreditAmount = 20 }]
        };
        await Verify<SaftPaymentsPageViewModel>("OnDoPrint", "Documents", new[] { payment });
        await Verify<SaftPaymentsPageViewModel>("OnDoPrintTaxes", "Documents", new[] { payment });
        await Verify<SaftPaymentsPageViewModel>("OnDoOpenExcel", "currentPayment", payment);
        await Verify<SaftPaymentsPageViewModel>("OnExportLinesExcel", "Lines", payment.Line);
        await Verify<SaftGeneralLedgerEntriesPageViewModel>("OnSaveExcel", "Documents", new GeneralLedgerEntriesJournal[]
        {
            new() { JournalID = "001", Description = "Diário", Transaction = [new() { TransactionID = "001", TransactionDate = date, GLPostingDate = date, Item = "Cliente001" }] }
        });
        if (reports.Count != 20) throw new Exception("Expected all 20 export routes.");
        if (!templates.SetEquals(Enum.GetValues<ReportTemplate>())) throw new Exception("Each export route must use its own RDL template.");
        Console.WriteLine("PASS: all 20 export routes, including missing optional customer/tax fields");

        async Task Verify<T>(string command, string member, object value)
        {
            var vm = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
            if (typeof(T).GetProperty(member) is { } property) property.SetValue(vm, value);
            else typeof(T).GetField(member, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(vm, value);
            reports.Last = null;
            await (Task)typeof(T).GetMethod(command, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(vm, null)!;
            if (reports.Last == null || reports.Last.Tables.Count == 0) throw new Exception($"Missing report for {typeof(T).Name}.{command}");
            templates.Add(reports.Last.Template);
            var columns = ReportTemplates.ColumnCounts(reports.Last.Template);
            for (var i = 0; i < columns.Length; i++)
                if (reports.Last.Tables[i].Data.Columns.Count != columns[i] * 2 + 1)
                    throw new Exception("Dataset schemas must include optional empty columns.");
            using var report = await ReportRenderer.CreateAsync(reports.Last);
            var pages = await report.BuildPages();
            if (pages.PageCount == 0) throw new Exception($"No pages for {typeof(T).Name}.{command}");
        }
    }

    private sealed class CaptureReports : IReportService
    {
        public ReportRequest Last;
        public int Count;
        public Task ShowAsync(ReportRequest request) { Last = request; Count++; return Task.CompletedTask; }
    }
}
