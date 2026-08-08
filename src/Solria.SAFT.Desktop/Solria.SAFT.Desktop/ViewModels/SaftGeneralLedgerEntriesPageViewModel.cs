using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftGeneralLedgerEntriesPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;

    public SaftGeneralLedgerEntriesPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        IsLoading = true;

        var documents = saftValidator?.SaftFile?.GeneralLedgerEntries?.Journal ?? [];

        if (documents.Length == 0) return;

        Documents = [.. documents];
        Lines = [];

        NumberOfEntries = saftValidator?.SaftFile?.GeneralLedgerEntries?.NumberOfEntries;
        TotalCredit = saftValidator?.SaftFile?.GeneralLedgerEntries?.TotalCredit ?? 0;
        TotalDebit = saftValidator?.SaftFile?.GeneralLedgerEntries?.TotalDebit ?? 0;

        NumberOfEntriesCalc = documents.Sum(c => c.Transaction.Length).ToString();
        TotalCreditCalc = documents.Sum(d => d.Transaction?.Sum(t => t.Lines?.CreditLine?.CreditAmount ?? 0) ?? 0);
        TotalDebitCalc = documents.Sum(d => d.Transaction?.Sum(t => t.Lines?.DebitLine?.DebitAmount ?? 0) ?? 0);

        IsLoading = false;
    }

    [ObservableProperty]
    private IList<GeneralLedgerEntriesJournal> documents;

    [ObservableProperty]
    private GeneralLedgerEntriesJournal currentDocument;

    [ObservableProperty]
    private IList<GeneralLedgerEntriesJournalTransaction> lines;

    [ObservableProperty]
    private DateTimeOffset filtroDataInicio;

    [ObservableProperty]
    private DateTimeOffset filtroDataFim;

    [ObservableProperty]
    private string filterLines;

    [ObservableProperty]
    private string numberOfEntries;
    [ObservableProperty]
    private decimal totalCredit;
    [ObservableProperty]
    private decimal totalDebit;

    [ObservableProperty]
    private string numberOfEntriesCalc;
    [ObservableProperty]
    private decimal totalCreditCalc;
    [ObservableProperty]
    private decimal totalDebitCalc;

    partial void OnCurrentDocumentChanged(GeneralLedgerEntriesJournal value)
    {
        Lines = value?.Transaction ?? [];
    }

    [RelayCommand]
    private async Task OnSaveExcel()
    {
        if (Documents == null || Documents.Count == 0) return;

        var (_, stream) = await dialogManager.SaveFileDialog(
            "Guardar Documentos Movimentação",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Documentos Movimentação.xlsx",
            ".xlsx");

        if (stream == null) return;

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var sheet = workbook.Worksheets.Add("Documentos");

        DocHeader(sheet, 1);

        var rowIndex = 2;
        foreach (var c in Documents)
        {
            sheet.Cell(rowIndex, 1).Value = c.JournalID;
            sheet.Cell(rowIndex, 2).Value = c.Description;
            
            rowIndex += 2;

            //create lines header
            LineHeader(sheet, rowIndex);

            foreach (var l in c.Transaction)
            {
                rowIndex++;

                sheet.Cell(rowIndex, 2).Value = l.TransactionID;
                sheet.Cell(rowIndex, 3).Value = l.Period;
                sheet.Cell(rowIndex, 4).Value = l.TransactionDate;
                sheet.Cell(rowIndex, 5).Value = l.SourceID;
                sheet.Cell(rowIndex, 6).Value = l.Description;
                sheet.Cell(rowIndex, 7).Value = l.DocArchivalNumber;
                sheet.Cell(rowIndex, 8).Value = l.TransactionType.ToString();
                sheet.Cell(rowIndex, 9).Value = l.GLPostingDate;
                sheet.Cell(rowIndex, 10).Value = l.ItemElementName.ToString();
                sheet.Cell(rowIndex, 11).Value = l.Item;
            }

            rowIndex += 2;
        }

        sheet.Columns().AdjustToContents();

        workbook.SaveAs(stream);
        stream.Close();
        await stream.DisposeAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void OnSearch()
    {
        var documents = saftValidator?.SaftFile?.GeneralLedgerEntries?.Journal ?? [];

        if (string.IsNullOrWhiteSpace(Filter))
        {
            Documents = documents;

            return;
        }

        Documents = [.. documents
            .Where(d => FilterEntries(d, Filter))];
    }
    private static bool FilterEntries(GeneralLedgerEntriesJournal entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(entry.JournalID) == false && entry.JournalID.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(entry.Description) == false && entry.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
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
        var allLines = CurrentDocument?.Transaction ?? [];

        if (string.IsNullOrWhiteSpace(FilterLines))
        {
            Lines = [.. allLines];
            return;
        }

        Lines = [.. allLines.Where(l => FilterDetails(l, FilterLines))];
    }
    private static bool FilterDetails(GeneralLedgerEntriesJournalTransaction line, string filter)
    {
        if (string.IsNullOrWhiteSpace(line.Description) == false && line.Description.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(line.TransactionID) == false && line.TransactionID.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(line.SourceID) == false && line.SourceID.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(line.DocArchivalNumber) == false && line.DocArchivalNumber.Contains(filter, StringComparison.OrdinalIgnoreCase))
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

    private static void DocHeader(ClosedXML.Excel.IXLWorksheet sheet, int row)
    {
        sheet.Cell(row, 1).Value = "JournalID";
        sheet.Cell(row, 2).Value = "Descrição";
    }

    private static void LineHeader(ClosedXML.Excel.IXLWorksheet sheet, int row)
    {
        sheet.Cell(row, 2).Value = "TransactionID";
        sheet.Cell(row, 3).Value = "Period";
        sheet.Cell(row, 4).Value = "TransactionDate";
        sheet.Cell(row, 5).Value = "SourceID";
        sheet.Cell(row, 6).Value = "Description";
        sheet.Cell(row, 7).Value = "DocArchivalNumber";
        sheet.Cell(row, 8).Value = "TransactionType";
        sheet.Cell(row, 9).Value = "GLPostingDate";
        sheet.Cell(row, 10).Value = "ItemElementName";
        sheet.Cell(row, 12).Value = "Item";
    }
}
