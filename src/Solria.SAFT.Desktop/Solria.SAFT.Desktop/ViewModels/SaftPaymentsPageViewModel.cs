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

public partial class SaftPaymentsPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;

    public SaftPaymentsPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        IsLoading = true;

        var payments = saftValidator.SaftFile?.SourceDocuments?.Payments?.Payment ?? [];

        if (payments.Any() == false) return;

        Documents = new List<SourceDocumentsPaymentsPayment>(payments);
        Lines = new List<SourceDocumentsPaymentsPaymentLine>();

        DocNumberOfEntries = payments.Length;
        DocTotalCredit = payments
            .Where(i => i.DocumentStatus.PaymentStatus != PaymentStatus.A)
            .Sum(i => i.Line.Sum(l => l.CreditAmount ?? 0))
            .ToString("c");

        DocTotalDebit = payments
            .Where(i => i.DocumentStatus.PaymentStatus != PaymentStatus.A)
            .Sum(i => i.Line.Sum(l => l.DebitAmount ?? 0))
            .ToString("c");

        if (saftValidator?.SaftFile?.SourceDocuments?.Payments != null)
        {
            NumberOfEntries = saftValidator.SaftFile.SourceDocuments.Payments.NumberOfEntries;
            TotalCredit = saftValidator.SaftFile.SourceDocuments.Payments.TotalCredit.ToString("c");
            TotalDebit = saftValidator.SaftFile.SourceDocuments.Payments.TotalDebit.ToString("c");
        }

        FiltroDataInicio = payments.Min(i => i.SystemEntryDate);
        FiltroDataFim = payments.Max(i => i.SystemEntryDate);

        IsLoading = false;
    }

    [ObservableProperty]
    private IList<SourceDocumentsPaymentsPayment> documents;

    [ObservableProperty]
    private IList<SourceDocumentsPaymentsPaymentLine> lines;

    [ObservableProperty]
    private SourceDocumentsPaymentsPayment currentPayment;

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

    partial void OnCurrentPaymentChanged(SourceDocumentsPaymentsPayment value)
    {
        if (value != null && string.IsNullOrEmpty(FilterLines))
        {
            DocGrossTotal = value.Line.Sum(c => (c.CreditAmount ?? c.DebitAmount ?? 0) * (1 + (c.Tax.TaxPercentage ?? 0) * 0.01m) * Operation(c)).ToString("c");
            DocNetTotal = value.Line.Sum(c => (c.CreditAmount ?? c.DebitAmount ?? 0) * Operation(c)).ToString("c");
            DocTaxPayable = value.Line.Sum(c => (c.CreditAmount ?? c.DebitAmount ?? 0) * (c.Tax.TaxPercentage ?? 0) * 0.01m * Operation(c)).ToString("c");

            GrossTotal = value.DocumentTotals.GrossTotal.ToString("c");
            NetTotal = value.DocumentTotals.NetTotal.ToString("c");
            TaxPayable = value.DocumentTotals.TaxPayable.ToString("c");
        }
    }

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Documents == null || Documents.Count == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar Documentos Pagamento",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Documentos Pagamento.xlsx",
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
                sheet.Cell(rowIndex, 2).Value = c.PaymentType.ToString();
                sheet.Cell(rowIndex, 3).Value = c.PaymentRefNo;
                sheet.Cell(rowIndex, 4).Value = c.DocumentStatus.PaymentStatus.ToString();
                sheet.Cell(rowIndex, 5).Value = c.SystemEntryDate;
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
                    sheet.Cell(rowIndex, 3).Value = l.CreditAmount;
                    sheet.Cell(rowIndex, 4).Value = l.DebitAmount;
                    sheet.Cell(rowIndex, 5).Value = l.SettlementAmount;
                    sheet.Cell(rowIndex, 6).Value = l.Tax.TaxPercentage;
                    sheet.Cell(rowIndex, 7).Value = l.TaxExemptionReason;
                    sheet.Cell(rowIndex, 8).Value = l.TaxExemptionCode;
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
            "Guardar Documentos Pagammento",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Documentos Pagamentos - Impostos.xlsx",
            ".xlsx");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Impostos");

            var taxes_selling_group = Documents
                .SelectMany(i => i.Line)
                .Where(c => c.CreditAmount > 0)
                .GroupBy(l => new { l.DocNo, l.Tax.TaxPercentage })
                .Select(g => new { g.Key.DocNo, Tax = g.Key.TaxPercentage, NetTotal = g.Sum(l => l.CreditAmount ?? 0) })
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
    private void OnSearch()
    {
        var payments = saftValidator.SaftFile?.SourceDocuments?.Payments?.Payment ?? [];

        if (string.IsNullOrWhiteSpace(Filter))
        {
            Documents = payments;

            return;
        }

        Documents = payments
            .Where(d => FilterEntries(d, Filter))
            .Where(d => FilterEntriesByDate(d, FiltroDataInicio, FiltroDataFim))
            .ToArray();
    }
    private static bool FilterEntries(SourceDocumentsPaymentsPayment entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(entry.ATCUD) == false && entry.ATCUD.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.CustomerID) == false && entry.CustomerID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.PaymentRefNo) == false && entry.PaymentRefNo.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Period) == false && entry.Period.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.SourceID) == false && entry.SourceID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.TransactionID) == false && entry.TransactionID.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || entry.DocumentStatus.PaymentStatus.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || entry.DocumentStatus.Reason.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || entry.PaymentType.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
    private static bool FilterEntriesByDate(SourceDocumentsPaymentsPayment entry, DateTimeOffset start, DateTimeOffset end)
    {
        if (entry.TransactionDate >= start && entry.TransactionDate <= end)
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
        var allLines = ShowAllLines ? saftValidator.SaftFile?.SourceDocuments?.Payments?.Payment?.SelectMany(i => i.Line) ?? [] : CurrentPayment?.Line ?? [];

        if (string.IsNullOrWhiteSpace(FilterLines))
        {
            Lines = allLines.ToArray();
            return;
        }

        Lines = allLines.Where(l => FilterDetails(l, FilterLines)).ToArray();
    }
    private static bool FilterDetails(SourceDocumentsPaymentsPaymentLine line, string filter)
    {
        if (string.IsNullOrWhiteSpace(line.TaxExemptionCode) == false && line.TaxExemptionCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
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

    }
    
    [RelayCommand]
    private async Task OnDoOpenExcel()
    {
        if (CurrentPayment == null)
            return;

        var file = await dialogManager.SaveFileDialog(
                "Guardar Documento Pagamento",
                directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                initialFileName: "Documento Pagamento.xlsx",
                "xlsx");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Documento");

            DocHeader(sheet, 1);

            var rowIndex = 2;

            sheet.Cell(rowIndex, 1).Value = CurrentPayment.ATCUD;
            sheet.Cell(rowIndex, 2).Value = CurrentPayment.PaymentType.ToString();
            sheet.Cell(rowIndex, 3).Value = CurrentPayment.PaymentRefNo;
            sheet.Cell(rowIndex, 4).Value = CurrentPayment.DocumentStatus.PaymentStatus.ToString();
            sheet.Cell(rowIndex, 5).Value = CurrentPayment.SystemEntryDate;
            sheet.Cell(rowIndex, 6).Value = CurrentPayment.CustomerID;
            sheet.Cell(rowIndex, 7).Value = CurrentPayment.DocumentTotals.NetTotal;
            sheet.Cell(rowIndex, 8).Value = CurrentPayment.DocumentTotals.TaxPayable;
            sheet.Cell(rowIndex, 9).Value = CurrentPayment.DocumentTotals.GrossTotal;

            rowIndex += 2;

            //create lines header
            LineHeader(sheet, rowIndex);

            foreach (var l in CurrentPayment.Line)
            {
                rowIndex++;

                sheet.Cell(rowIndex, 2).Value = l.LineNumber;
                sheet.Cell(rowIndex, 3).Value = l.CreditAmount;
                sheet.Cell(rowIndex, 4).Value = l.DebitAmount;
                sheet.Cell(rowIndex, 5).Value = l.SettlementAmount;
                sheet.Cell(rowIndex, 6).Value = l.Tax.TaxPercentage;
                sheet.Cell(rowIndex, 7).Value = l.TaxExemptionReason;
                sheet.Cell(rowIndex, 8).Value = l.TaxExemptionCode;
            }

            sheet.Columns().AdjustToContents();

            workbook.SaveAs(file);
        }
    }

    private static int Operation(SourceDocumentsPaymentsPaymentLine l)
    {
        return l.CreditAmount > 0 ? 1 : -1;
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
        sheet.Cell(row, 3).Value = "Crédito";
        sheet.Cell(row, 4).Value = "Débito";
        sheet.Cell(row, 5).Value = "Desconto";
        sheet.Cell(row, 6).Value = "Imposto";
        sheet.Cell(row, 7).Value = "Isenção";
        sheet.Cell(row, 8).Value = "Cód. Isenção";
    }
}
