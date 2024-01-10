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

public partial class SaftMovementOfGoodsPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;

    public SaftMovementOfGoodsPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        IsLoading = true;

        var documents = saftValidator?.SaftFile?.SourceDocuments?.MovementOfGoods?.StockMovement ?? [];

        if (documents.Any() == false) return;

        Documents = new List<SourceDocumentsMovementOfGoodsStockMovement>(documents);
        Lines = new List<SourceDocumentsMovementOfGoodsStockMovementLine>();

        DocNumberOfEntries = documents.Length;
        DocTotalCredit = documents
            .Where(i => i.DocumentStatus.MovementStatus != MovementStatus.A && i.DocumentStatus.MovementStatus != MovementStatus.F)
            .Sum(i => i.Line.Sum(l => l.CreditAmount ?? 0))
            .ToString("c");

        DocTotalDebit = documents
            .Where(i => i.DocumentStatus.MovementStatus != MovementStatus.A && i.DocumentStatus.MovementStatus != MovementStatus.F)
            .Sum(i => i.Line.Sum(l => l.DebitAmount ?? 0))
            .ToString("c");

        if (saftValidator?.SaftFile?.SourceDocuments?.MovementOfGoods != null)
        {
            NumberOfEntries = saftValidator.SaftFile.SourceDocuments.MovementOfGoods.NumberOfMovementLines;
            TotalQuantity = saftValidator.SaftFile.SourceDocuments.MovementOfGoods.TotalQuantityIssued;
        }


        FiltroDataInicio = documents.Min(i => i.MovementDate);
        FiltroDataFim = documents.Max(i => i.MovementDate);

        IsLoading = false;
    }

    [ObservableProperty]
    private IList<SourceDocumentsMovementOfGoodsStockMovement> documents;

    [ObservableProperty]
    private SourceDocumentsMovementOfGoodsStockMovement currentDocument;

    [ObservableProperty]
    private IList<SourceDocumentsMovementOfGoodsStockMovementLine> lines;

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
    private decimal totalQuantity;

    partial void OnCurrentDocumentChanged(SourceDocumentsMovementOfGoodsStockMovement value)
    {
        if (value != null && string.IsNullOrEmpty(FilterLines))
        {
            DocGrossTotal = value.Line.Sum(c => c.UnitPrice * (1 + (c.Tax?.TaxPercentage ?? 0) * 0.01m) * c.Quantity * Operation(value, c)).ToString("c");
            DocNetTotal = value.Line.Sum(c => c.UnitPrice * c.Quantity * Operation(value, c)).ToString("c");
            DocTaxPayable = value.Line.Sum(c => c.UnitPrice * (c.Tax?.TaxPercentage ?? 0) * 0.01m * c.Quantity * Operation(value, c)).ToString("c");

            GrossTotal = value.DocumentTotals.GrossTotal.ToString("c");
            NetTotal = value.DocumentTotals.NetTotal.ToString("c");
            TaxPayable = value.DocumentTotals.TaxPayable.ToString("c");
        }
    }

    [RelayCommand]
    private async Task OnSaveExcel()
    {
        if (Documents == null || Documents.Count == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar Documentos Movimentação",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Documentos Movimentação.xlsx",
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
                sheet.Cell(rowIndex, 2).Value = c.MovementType.ToString();
                sheet.Cell(rowIndex, 3).Value = c.DocumentNumber;
                sheet.Cell(rowIndex, 4).Value = c.DocumentStatus.MovementStatus.ToString();
                sheet.Cell(rowIndex, 5).Value = c.MovementDate;
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
                    sheet.Cell(rowIndex, 13).Value = l.UnitOfMeasure;
                    sheet.Cell(rowIndex, 14).Value = l.Description;
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
                "Guardar Documentos Movimentação",
                directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                initialFileName: "Documentos Movimentação - Impostos.xlsx",
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

            foreach (var tax in taxes_selling_group)
            {
                sheet.Cell(rowIndex, 1).Value = tax.DocNo;
                sheet.Cell(rowIndex, 2).Value = tax.Tax;
                sheet.Cell(rowIndex, 3).Value = tax.NetTotal;
                sheet.Cell(rowIndex, 4).Value = Math.Round(Math.Round(tax.NetTotal, 2, MidpointRounding.AwayFromZero) * tax.Tax * 0.01m, 2, MidpointRounding.AwayFromZero);

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
        var documents = saftValidator?.SaftFile?.SourceDocuments?.MovementOfGoods?.StockMovement ?? [];

        if (string.IsNullOrWhiteSpace(Filter))
        {
            Documents = documents;

            return;
        }

        Documents = documents
            .Where(d => FilterEntries(d, Filter))
            .Where(d => FilterEntriesByDate(d, FiltroDataInicio, FiltroDataFim))
            .ToArray();
    }
    private static bool FilterEntries(SourceDocumentsMovementOfGoodsStockMovement entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(entry.ATCUD) == false && entry.ATCUD.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.CustomerID) == false && entry.CustomerID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.DocumentStatus?.Reason) == false && entry.DocumentStatus.Reason.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.EACCode) == false && entry.EACCode.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.DocumentNumber) == false && entry.DocumentNumber.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Period) == false && entry.Period.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.SourceID) == false && entry.SourceID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.TransactionID) == false && entry.TransactionID.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || entry.DocumentStatus != null && entry.DocumentStatus.MovementStatus.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || entry.MovementType.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
    private static bool FilterEntriesByDate(SourceDocumentsMovementOfGoodsStockMovement entry, DateTimeOffset start, DateTimeOffset end)
    {
        if (entry.MovementDate >= start && entry.MovementDate <= end)
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
        var allLines = ShowAllLines ? saftValidator?.SaftFile?.SourceDocuments?.MovementOfGoods?.StockMovement?.SelectMany(i => i.Line) ?? [] : CurrentDocument?.Line ?? [];

        if (string.IsNullOrWhiteSpace(FilterLines))
        {
            Lines = allLines.ToArray();
            return;
        }

        Lines = allLines.Where(l => FilterDetails(l, FilterLines)).ToArray();
    }
    private static bool FilterDetails(SourceDocumentsMovementOfGoodsStockMovementLine line, string filter)
    {
        if (string.IsNullOrWhiteSpace(line.Description) == false && line.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(line.ProductCode) == false && line.ProductCode.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(line.ProductDescription) == false && line.ProductDescription.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(line.TaxExemptionCode) == false && line.TaxExemptionCode.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(line.TaxExemptionReason) == false && line.TaxExemptionReason.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || line.ProductSerialNumber != null && line.ProductSerialNumber.Any(l => l.Contains(filter, StringComparison.OrdinalIgnoreCase)))
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
    private async Task OnOpenExcel()
    {
        if (CurrentDocument == null)
            return;

        var file = await dialogManager.SaveFileDialog(
                "Guardar Documento Movimentação",
                directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                initialFileName: "Documento Movimentação.xlsx",
                "xlsx");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Documento");

            DocHeader(sheet, 1);

            var rowIndex = 2;

            sheet.Cell(rowIndex, 1).Value = CurrentDocument.ATCUD;
            sheet.Cell(rowIndex, 2).Value = CurrentDocument.MovementType.ToString();
            sheet.Cell(rowIndex, 3).Value = CurrentDocument.DocumentNumber;
            sheet.Cell(rowIndex, 4).Value = CurrentDocument.DocumentStatus.MovementStatus.ToString();
            sheet.Cell(rowIndex, 5).Value = CurrentDocument.MovementDate;
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
                sheet.Cell(rowIndex, 13).Value = l.UnitOfMeasure;
                sheet.Cell(rowIndex, 14).Value = l.Description;
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
                    .Where(i => i.DocumentNumber.IndexOf(string.Format("{0}/{1}", docNo[0], num), StringComparison.OrdinalIgnoreCase) == 0)
                    .FirstOrDefault();

                //encontramos um documento, vamos obter a hash
                if (previousDocument != null)
                {
                    var vm = new DialogHashTestViewModel();
                    vm.Init();
                    vm.InitFromMovement(CurrentDocument, previousDocument.Hash);

                    await dialogManager.ShowChildDialogAsync(vm);
                }
            }
        }
    }

    private static int Operation(SourceDocumentsMovementOfGoodsStockMovement i, SourceDocumentsMovementOfGoodsStockMovementLine l)
    {
        if (i.MovementType == MovementType.GR || i.MovementType == MovementType.GT || i.MovementType == MovementType.GA || i.MovementType == MovementType.GC)
            return l.CreditAmount > 0 ? 1 : -1;
        else if (i.MovementType == MovementType.GD)
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
        sheet.Cell(row, 12).Value = "Isenção";
        sheet.Cell(row, 13).Value = "Cód. Isenção";
        sheet.Cell(row, 14).Value = "Unidade medida";
        sheet.Cell(row, 15).Value = "Descrição";
    }
}
