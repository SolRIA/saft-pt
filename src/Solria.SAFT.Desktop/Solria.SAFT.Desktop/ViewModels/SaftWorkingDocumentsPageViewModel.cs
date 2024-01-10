using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftWorkingDocumentsPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;

    public SaftWorkingDocumentsPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        IsLoading = true;

        var documents = saftValidator.SaftFile?.SourceDocuments?.WorkingDocuments?.WorkDocument ?? [];

        if (documents.Any() == false) return;

        Documents = new List<SourceDocumentsWorkingDocumentsWorkDocument>(documents);
        Lines = new List<SourceDocumentsWorkingDocumentsWorkDocumentLine>();

        DocNumberOfEntries = documents.Length;
        DocTotalCredit = documents
            .Where(i => i.DocumentStatus.WorkStatus != WorkStatus.A && i.DocumentStatus.WorkStatus != WorkStatus.F)
            .Sum(i => i.Line.Sum(l => l.CreditAmount ?? 0))
            .ToString("c");

        DocTotalDebit = documents
            .Where(i => i.DocumentStatus.WorkStatus != WorkStatus.A && i.DocumentStatus.WorkStatus != WorkStatus.F)
            .Sum(i => i.Line.Sum(l => l.DebitAmount ?? 0))
            .ToString("c");

        if (saftValidator?.SaftFile?.SourceDocuments?.WorkingDocuments != null)
        {
            NumberOfEntries = saftValidator.SaftFile.SourceDocuments.WorkingDocuments.NumberOfEntries;
            TotalCredit = saftValidator.SaftFile.SourceDocuments.WorkingDocuments.TotalCredit.ToString("c");
            TotalDebit = saftValidator.SaftFile.SourceDocuments.WorkingDocuments.TotalDebit.ToString("c");
        }

        FiltroDataInicio = documents.Min(i => i.WorkDate);
        FiltroDataFim = documents.Max(i => i.WorkDate);

        IsLoading = false;
    }

    [ObservableProperty]
    private IList<SourceDocumentsWorkingDocumentsWorkDocument> documents;

    [ObservableProperty]
    private IList<SourceDocumentsWorkingDocumentsWorkDocumentLine> lines;

    [ObservableProperty]
    private SourceDocumentsWorkingDocumentsWorkDocument currentDocument;

    [ObservableProperty]
    private bool showAllLines;

    [ObservableProperty]
    private DateTimeOffset filtroDataInicio;

    [ObservableProperty]
    private DateTimeOffset filtroDataFim;

    [ObservableProperty]
    private string filterLines;

    [ObservableProperty]
    private string docGrossTotal;
    [ObservableProperty]
    private string docNetTotal;
    [ObservableProperty]
    private string docTaxPayable;
    [ObservableProperty]
    private int docNumberOfEntries;
    [ObservableProperty]
    private string docTotalCredit;
    [ObservableProperty]
    private string docTotalDebit;
    [ObservableProperty]
    private string grossTotal;
    [ObservableProperty]
    private string netTotal;
    [ObservableProperty]
    private string taxPayable;
    [ObservableProperty]
    private string numberOfEntries;
    [ObservableProperty]
    private string totalCredit;
    [ObservableProperty]
    private string totalDebit;

    partial void OnCurrentDocumentChanged(SourceDocumentsWorkingDocumentsWorkDocument value)
    {
        if (value == null) return;

        if (ShowAllLines == false)
            Lines = value.Line;

        DocGrossTotal = value.Line.Sum(c => c.UnitPrice * (1 + (c.Tax.TaxPercentage ?? 0) * 0.01m) * c.Quantity * Operation(value, c)).ToString("c");
        DocNetTotal = value.Line.Sum(c => c.UnitPrice * c.Quantity * Operation(value, c)).ToString("c");
        DocTaxPayable = value.Line.Sum(c => c.UnitPrice * (c.Tax.TaxPercentage ?? 0) * 0.01m * c.Quantity * Operation(value, c)).ToString("c");

        GrossTotal = value.DocumentTotals.GrossTotal.ToString("c");
        NetTotal = value.DocumentTotals.NetTotal.ToString("c");
        TaxPayable = value.DocumentTotals.TaxPayable.ToString("c");
    }

    partial void OnShowAllLinesChanged(bool value)
    {
        OnSearchDetails();
    }

    partial void OnFiltroDataInicioChanged(DateTimeOffset value)
    {
        if (IsLoading) return;

        OnSearch();
    }

    partial void OnFiltroDataFimChanged(DateTimeOffset value)
    {
        if (IsLoading) return;

        OnSearch();
    }

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Documents == null || Documents.Count == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar Documentos Conferência",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Documentos Conferência.xlsx",
            ".xlsx");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Documentos");

            DocHeader(sheet, 1);

            var rowIndex = 2;
            foreach (var c in Documents)
            {
                sheet.Cell(rowIndex, 1).Value = c.ATCUD;
                sheet.Cell(rowIndex, 2).Value = c.WorkType.ToString();
                sheet.Cell(rowIndex, 3).Value = c.DocumentNumber;
                sheet.Cell(rowIndex, 4).Value = c.DocumentStatus.WorkStatus.ToString();
                sheet.Cell(rowIndex, 5).Value = c.WorkDate;
                sheet.Cell(rowIndex, 6).Value = c.CustomerID;
                sheet.Cell(rowIndex, 7).Value = c.DocumentTotals.NetTotal;
                sheet.Cell(rowIndex, 8).Value = c.DocumentTotals.TaxPayable;
                sheet.Cell(rowIndex, 9).Value = c.DocumentTotals.GrossTotal;

                rowIndex += 2;

                //create lines header
                LineHeader(sheet, rowIndex);

                foreach (var l in c.Line)
                {
                    rowIndex++;

                    sheet.Cell(rowIndex, 2).Value = l.LineNumber;
                    sheet.Cell(rowIndex, 3).Value = l.ProductCode;
                    sheet.Cell(rowIndex, 4).Value = l.ProductDescription;
                    sheet.Cell(rowIndex, 5).Value = l.Quantity;
                    sheet.Cell(rowIndex, 6).Value = l.UnitPrice;
                    sheet.Cell(rowIndex, 7).Value = l.CreditAmount;
                    sheet.Cell(rowIndex, 8).Value = l.DebitAmount;
                    sheet.Cell(rowIndex, 9).Value = l.SettlementAmount;
                    sheet.Cell(rowIndex, 10).Value = l.Tax.TaxPercentage;
                    sheet.Cell(rowIndex, 11).Value = l.TaxExemptionReason;
                    sheet.Cell(rowIndex, 12).Value = l.TaxExemptionCode;
                    sheet.Cell(rowIndex, 13).Value = l.References != null && l.References.Length > 0 ? l.References[0].Reference : string.Empty;
                    sheet.Cell(rowIndex, 14).Value = l.References != null && l.References.Length > 0 ? l.References[0].Reason : string.Empty;
                    sheet.Cell(rowIndex, 15).Value = l.UnitOfMeasure;
                    sheet.Cell(rowIndex, 16).Value = l.TaxPointDate;
                    sheet.Cell(rowIndex, 17).Value = l.Description;
                }

                rowIndex += 2;
            }

            sheet.Columns().AdjustToContents();

            workbook.SaveAs(file);
        }
    }

    [RelayCommand]
    private async Task OnDoPrintTaxes()
    {
        if (Documents == null || Documents.Count == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar Documentos Conferência",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Documentos Conferência - Impostos.xlsx",
            ".xlsx");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Impostos");

            var taxes_selling_group = Documents
                .SelectMany(i => i.Line)
                .Where(c => c.CreditAmount > 0)
                .GroupBy(l => new { l.DocNo, l.Tax.TaxPercentage })
                .Select(g => new { g.Key.DocNo, Tax = g.Key.TaxPercentage, NetTotal = g.Sum(l => l.Quantity * l.UnitPrice) })
                .OrderBy(g => g.DocNo)
                .ThenBy(g => g.Tax);

            var rowIndex = 1;

            sheet.Cell(rowIndex, 1).Value = "Documento";
            sheet.Cell(rowIndex, 2).Value = "Imposto";
            sheet.Cell(rowIndex, 3).Value = "Incidência";
            sheet.Cell(rowIndex, 4).Value = "Total";

            rowIndex++;
            foreach (var tax in taxes_selling_group)
            {
                sheet.Cell(rowIndex, 1).Value = tax.DocNo;
                sheet.Cell(rowIndex, 2).Value = tax.Tax;
                sheet.Cell(rowIndex, 3).Value = tax.NetTotal;
                sheet.Cell(rowIndex, 4).Value = Math.Round(Math.Round(tax.NetTotal, 2, MidpointRounding.AwayFromZero) * tax.Tax.GetValueOrDefault(0) * 0.01m, 2, MidpointRounding.AwayFromZero);

                rowIndex++;
            }

            sheet.Cell(rowIndex, 4).FormulaA1 = $"=SUM(D2:D{rowIndex - 1})";

            sheet.Columns().AdjustToContents();

            workbook.SaveAs(file);
        }
    }

    [RelayCommand]
    private void OnShowDependencies()
    {
        if (CurrentDocument == null) return;

        var doc = new Models.Reporting.Document
        {
            Pk = CurrentDocument.DocumentNumber,
            Number = CurrentDocument.DocumentNumber,
            ATCUD = CurrentDocument.ATCUD,
            Date = CurrentDocument.WorkDate.ToLongDateString(),
            SystemDate = CurrentDocument.SystemEntryDate.ToLongDateString(),
            Total = CurrentDocument.DocumentTotals.GrossTotal.ToString("C"),
            NetTotal = CurrentDocument.DocumentTotals.NetTotal.ToString("C"),
            VatTotal = CurrentDocument.DocumentTotals.TaxPayable.ToString("C")
        };

        var vm = new DialogDocumentReferencesViewModel(doc);

        vm.InitWorkingDocumentDescendents(CurrentDocument.DocumentNumber);
        vm.InitWorkingDocumentParents(CurrentDocument.DocumentNumber);
        vm.InitWorkingDocumentInvoice();

        dialogManager.ShowChildDialog(vm);
    }

    [RelayCommand]
    private void OnSearch()
    {
        var docs = saftValidator.SaftFile?.SourceDocuments?.WorkingDocuments?.WorkDocument ?? [];

        if (string.IsNullOrWhiteSpace(Filter))
        {
            Documents = docs
                .Where(d => FilterDocumentsByDate(d, FiltroDataInicio, FiltroDataFim))
                .ToArray();

            return;
        }

        Documents = docs
            .Where(d => FilterDocuments(d, Filter))
            .Where(d => FilterDocumentsByDate(d, FiltroDataInicio, FiltroDataFim))
            .ToArray();
    }
    private static bool FilterDocuments(SourceDocumentsWorkingDocumentsWorkDocument doc, string filter)
    {
        if (doc.ATCUD != null && doc.ATCUD.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.CustomerID != null && doc.CustomerID.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.DocumentStatus != null && doc.DocumentStatus.WorkStatus.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.DocumentStatus != null && doc.DocumentStatus.Reason != null && doc.DocumentStatus.Reason.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.EACCode != null && doc.EACCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.DocumentNumber != null && doc.DocumentNumber.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.WorkType.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.Period != null && doc.Period.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.SourceID != null && doc.SourceID.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || doc.TransactionID != null && doc.TransactionID.Contains(filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
    private static bool FilterDocumentsByDate(SourceDocumentsWorkingDocumentsWorkDocument doc, DateTimeOffset start, DateTimeOffset end)
    {
        if (doc.WorkDate >= start && doc.WorkDate <= end)
            return true;

        return false;
    }

    [RelayCommand]
    private void OnSearchClear()
    {
        Filter = null;
        OnSearch();
    }

    [RelayCommand]
    private void OnSearchDetails()
    {
        var allLines = ShowAllLines ? saftValidator.SaftFile?.SourceDocuments?.WorkingDocuments?.WorkDocument?.SelectMany(i => i.Line) ?? [] : CurrentDocument?.Line ?? [];

        if (string.IsNullOrWhiteSpace(FilterLines))
        {
            Lines = allLines.ToArray();
            return;
        }

        Lines = allLines.Where(l => FilterDocumentLines(l, FilterLines)).ToArray();
    }
    private static bool FilterDocumentLines(SourceDocumentsWorkingDocumentsWorkDocumentLine line, string filter)
    {
        if (string.IsNullOrWhiteSpace(line.Description) == false && line.Description.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(line.ProductCode) == false && line.ProductCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(line.ProductDescription) == false && line.ProductDescription.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || line.ProductSerialNumber != null && line.ProductSerialNumber.Any(l => l.Contains(filter, StringComparison.OrdinalIgnoreCase))
            || line.OrderReferences != null && line.OrderReferences.Any(l => l.OriginatingON.Contains(filter, StringComparison.OrdinalIgnoreCase))
            || string.IsNullOrWhiteSpace(line.TaxExemptionCode) == false && line.TaxExemptionCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(line.TaxExemptionReason) == false && line.TaxExemptionReason.Contains(filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    [RelayCommand]
    private void OnSearchDetailsClear()
    {
        FilterLines = null;
        OnSearchDetails();
    }

    [RelayCommand]
    private void OnShowCustomer()
    {

    }

    [RelayCommand]
    private void OnShowInvoiceDetails()
    {
        //create report instance

        // register the array
        var lines = CurrentDocument.Line.Select(l => new Models.Reporting.DocumentLine
        {
            LineNumber = l.LineNumber,
            ProductCode = l.ProductCode,
            ProductDescription = l.ProductDescription,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            TaxBase = l.TaxBase,
            Description = l.Description,
            TaxCode = l.Tax.TaxCode,
            Reference = l.OrderReferences != null && l.OrderReferences.Length > 0 ? l.OrderReferences[0].OriginatingON : null,
        }).ToArray();

        var taxes = new List<Models.Reporting.Tax>();
        foreach (var l in CurrentDocument.Line)
        {
            var existing = taxes
                .Where(t => t.TaxType == l.Tax.TaxType.ToString() && t.TaxCountryRegion == l.Tax.TaxCountryRegion && t.TaxPercentage == l.Tax.TaxPercentage)
                .FirstOrDefault();

            if (existing != null)
            {
                existing.TaxAmount += l.CreditAmount * l.Tax.TaxPercentage * 0.01m;
            }
            else
            {
                existing = new Models.Reporting.Tax
                {
                    TaxAmount = l.CreditAmount * l.Tax.TaxPercentage * 0.01m,
                    TaxCode = l.Tax.TaxCode,
                    TaxCountryRegion = l.Tax.TaxCountryRegion,
                    TaxPercentage = l.Tax.TaxPercentage,
                    TaxType = l.Tax.TaxType.ToString()
                };
                taxes.Add(existing);
            }
        }
        var document = new Models.Reporting.Document
        {
            Number = CurrentDocument.DocumentNumber,
            ATCUD = CurrentDocument.ATCUD,
            Date = CurrentDocument.WorkDate.ToLongDateString(),
            SystemDate = CurrentDocument.SystemEntryDate.ToLongDateString(),
            Total = CurrentDocument.DocumentTotals.GrossTotal.ToString("C"),
            NetTotal = CurrentDocument.DocumentTotals.NetTotal.ToString("C"),
            VatTotal = CurrentDocument.DocumentTotals.TaxPayable.ToString("C"),
            Taxes = taxes.ToArray(),
            Lines = lines
        };

        //var qrCode = GetATQrCode(header, CurrentInvoice.Customer, CurrentInvoice);

        var vm = new DialogSaftDocumentDetailViewModel(document);

        dialogManager.ShowChildDialog(vm);
    }

    [RelayCommand]
    private async Task OnSaveExcel()
    {
        if (CurrentDocument == null)
            return;

        var file = await dialogManager.SaveFileDialog(
                "Guardar Documento Faturação",
                directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                initialFileName: "Documento Faturação.xlsx",
                "xlsx");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Documento");

            DocHeader(sheet, 1);

            var rowIndex = 2;

            sheet.Cell(rowIndex, 1).Value = CurrentDocument.ATCUD;
            sheet.Cell(rowIndex, 2).Value = CurrentDocument.WorkType.ToString();
            sheet.Cell(rowIndex, 3).Value = CurrentDocument.DocumentNumber;
            sheet.Cell(rowIndex, 4).Value = CurrentDocument.DocumentStatus.WorkStatus.ToString();
            sheet.Cell(rowIndex, 5).Value = CurrentDocument.WorkDate;
            sheet.Cell(rowIndex, 6).Value = CurrentDocument.CustomerID;
            sheet.Cell(rowIndex, 7).Value = CurrentDocument.DocumentTotals.NetTotal;
            sheet.Cell(rowIndex, 8).Value = CurrentDocument.DocumentTotals.TaxPayable;
            sheet.Cell(rowIndex, 9).Value = CurrentDocument.DocumentTotals.GrossTotal;

            rowIndex += 2;

            //create lines header
            LineHeader(sheet, rowIndex);

            foreach (var l in CurrentDocument.Line)
            {
                rowIndex++;

                sheet.Cell(rowIndex, 2).Value = l.LineNumber;
                sheet.Cell(rowIndex, 3).Value = l.ProductCode;
                sheet.Cell(rowIndex, 4).Value = l.ProductDescription;
                sheet.Cell(rowIndex, 5).Value = l.Quantity;
                sheet.Cell(rowIndex, 6).Value = l.UnitPrice;
                sheet.Cell(rowIndex, 7).Value = l.CreditAmount;
                sheet.Cell(rowIndex, 8).Value = l.DebitAmount;
                sheet.Cell(rowIndex, 9).Value = l.SettlementAmount;
                sheet.Cell(rowIndex, 10).Value = l.Tax.TaxPercentage;
                sheet.Cell(rowIndex, 11).Value = l.TaxExemptionReason;
                sheet.Cell(rowIndex, 12).Value = l.TaxExemptionCode;
                sheet.Cell(rowIndex, 13).Value = l.References != null && l.References.Length > 0 ? l.References[0].Reference : string.Empty;
                sheet.Cell(rowIndex, 14).Value = l.References != null && l.References.Length > 0 ? l.References[0].Reason : string.Empty;
                sheet.Cell(rowIndex, 15).Value = l.UnitOfMeasure;
                sheet.Cell(rowIndex, 16).Value = l.TaxPointDate;
                sheet.Cell(rowIndex, 17).Value = l.Description;
            }

            sheet.Columns().AdjustToContents();

            workbook.SaveAs(file);
        }
    }

    [RelayCommand]
    private async Task OnTestHash()
    {
        if (Documents == null || Documents.Count == 0) return;

        //parse the document number property to get the current document number
        string[] docNo = CurrentDocument.DocumentNumber.Split('/');
        if (docNo != null && docNo.Length == 2)
        {
            int.TryParse(docNo[1], out int num);
            num -= 1;

            if (num > 0)
            {
                //found a valid number, try to find the previous document
                var previousDocument = Documents
                    .Where(i => i.DocumentNumber.StartsWith(string.Format("{0}/{1}", docNo[0], num), StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                //encontramos um documento, vamos obter a hash
                if (previousDocument != null)
                {
                    var vm = new DialogHashTestViewModel();
                    vm.Init();
                    vm.InitFromWorking(CurrentDocument, previousDocument.Hash);

                    await dialogManager.ShowChildDialogAsync(vm);
                }
            }
        }
    }

    private static int Operation(SourceDocumentsWorkingDocumentsWorkDocument i, SourceDocumentsWorkingDocumentsWorkDocumentLine l)
    {
        if (i.WorkType == WorkType.CM || i.WorkType == WorkType.FC || i.WorkType == WorkType.FO || i.WorkType == WorkType.NE || i.WorkType == WorkType.OU || i.WorkType == WorkType.OR || i.WorkType == WorkType.PF || i.WorkType == WorkType.DC || i.WorkType == WorkType.RP || i.WorkType == WorkType.RE || i.WorkType == WorkType.CS || i.WorkType == WorkType.LD || i.WorkType == WorkType.RA)
            return l.CreditAmount > 0 ? 1 : -1;
        else if (i.WorkType == WorkType.CC)
            return l.DebitAmount > 0 ? 1 : -1;

        return 1;
    }

    private static void DocHeader(ClosedXML.Excel.IXLWorksheet sheet, int row)
    {
        sheet.Cell(row, 1).Value = "ATCUD";
        sheet.Cell(row, 2).Value = "Tipo";
        sheet.Cell(row, 3).Value = "Nº";
        sheet.Cell(row, 4).Value = "Estado";
        sheet.Cell(row, 5).Value = "Data";
        sheet.Cell(row, 6).Value = "Cliente";
        sheet.Cell(row, 7).Value = "Incidência";
        sheet.Cell(row, 8).Value = "IVA";
        sheet.Cell(row, 9).Value = "Total";
    }

    private static void LineHeader(ClosedXML.Excel.IXLWorksheet sheet, int row)
    {
        sheet.Cell(row, 2).Value = "Nº linha";
        sheet.Cell(row, 3).Value = "Código produto";
        sheet.Cell(row, 4).Value = "Descrição produto";
        sheet.Cell(row, 5).Value = "Quantidade";
        sheet.Cell(row, 6).Value = "Preço";
        sheet.Cell(row, 7).Value = "Crédito";
        sheet.Cell(row, 8).Value = "Débito";
        sheet.Cell(row, 9).Value = "Desconto";
        sheet.Cell(row, 10).Value = "Imposto";
        sheet.Cell(row, 11).Value = "Isenção";
        sheet.Cell(row, 12).Value = "Cód. Isenção";
        sheet.Cell(row, 13).Value = "Referência";
        sheet.Cell(row, 14).Value = "Razão";
        sheet.Cell(row, 15).Value = "Unidade medida";
        sheet.Cell(row, 16).Value = "Data";
        sheet.Cell(row, 17).Value = "Descrição";
    }
}
