using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftInvoicesPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;
    private readonly IReportService reportService;
    private readonly INavigationService navigationService;

    private Header header;

    public SaftInvoicesPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();
        reportService = AppBootstrap.Resolve<IReportService>();
        navigationService = AppBootstrap.Resolve<INavigationService>();

        if (saftValidator == null) return;

        Init();
    }

    private void Init()
    {
        IsLoading = true;

        ToolTip = new SourceDocumentsToolTipService();
        ToolTipLine = new SourceDocumentsSalesInvoicesInvoiceLineToolTipService();

        header = saftValidator.SaftFile?.Header;

        var invoices = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice ?? [];

        if (invoices.Any() == false) return;

        Documents = new List<SourceDocumentsSalesInvoicesInvoice>(invoices);
        Lines = new List<SourceDocumentsSalesInvoicesInvoiceLine>();

        DocNumberOfEntries = invoices.Length;
        DocTotalCredit = invoices
            .Where(i => i.DocumentStatus.InvoiceStatus != InvoiceStatus.A && i.DocumentStatus.InvoiceStatus != InvoiceStatus.F)
            .Sum(i => i.Line.Sum(l => l.CreditAmount ?? 0))
            .ToString("c");

        DocTotalDebit = invoices
            .Where(i => i.DocumentStatus.InvoiceStatus != InvoiceStatus.A && i.DocumentStatus.InvoiceStatus != InvoiceStatus.F)
            .Sum(i => i.Line.Sum(l => l.DebitAmount ?? 0))
            .ToString("c");

        if (saftValidator?.SaftFile?.SourceDocuments?.SalesInvoices != null)
        {
            NumberOfEntries = saftValidator.SaftFile.SourceDocuments.SalesInvoices.NumberOfEntries;
            TotalCredit = saftValidator.SaftFile.SourceDocuments.SalesInvoices.TotalCredit.ToString("c");
            TotalDebit = saftValidator.SaftFile.SourceDocuments.SalesInvoices.TotalDebit.ToString("c");
        }

        FiltroDataInicio = invoices.Min(i => i.InvoiceDate);
        FiltroDataFim = invoices.Max(i => i.InvoiceDate);

        IsLoading = false;
    }

    [ObservableProperty]
    private SourceDocumentsToolTipService toolTip;

    [ObservableProperty]
    private SourceDocumentsSalesInvoicesInvoiceLineToolTipService toolTipLine;

    [ObservableProperty]
    private IList<SourceDocumentsSalesInvoicesInvoice> documents;

    [ObservableProperty]
    private SourceDocumentsSalesInvoicesInvoice currentInvoice;

    [ObservableProperty]
    private IList<SourceDocumentsSalesInvoicesInvoiceLine> lines;

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

    partial void OnCurrentInvoiceChanged(SourceDocumentsSalesInvoicesInvoice value)
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
            "Guardar Documentos Faturação",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Documentos Faturação.xlsx",
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
                sheet.Cell(rowIndex, 2).Value = c.InvoiceType.ToString();
                sheet.Cell(rowIndex, 3).Value = c.InvoiceNo;
                sheet.Cell(rowIndex, 4).Value = c.DocumentStatus.InvoiceStatus.ToString();
                sheet.Cell(rowIndex, 5).Value = c.InvoiceDate;
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

                    sheet.Cell(rowIndex, 1).Value = l.LineNumber;
                    sheet.Cell(rowIndex, 2).Value = l.ProductCode;
                    sheet.Cell(rowIndex, 3).Value = l.ProductDescription;
                    sheet.Cell(rowIndex, 4).Value = l.Quantity;
                    sheet.Cell(rowIndex, 5).Value = l.UnitPrice;
                    sheet.Cell(rowIndex, 6).Value = l.CreditAmount;
                    sheet.Cell(rowIndex, 7).Value = l.DebitAmount;
                    sheet.Cell(rowIndex, 8).Value = l.SettlementAmount;
                    sheet.Cell(rowIndex, 9).Value = l.Tax.TaxPercentage;
                    sheet.Cell(rowIndex, 10).Value = l.TaxExemptionReason;
                    sheet.Cell(rowIndex, 11).Value = l.TaxExemptionCode;
                    sheet.Cell(rowIndex, 12).Value = l.References != null && l.References.Length > 0 ? l.References[0].Reference : string.Empty;
                    sheet.Cell(rowIndex, 13).Value = l.References != null && l.References.Length > 0 ? l.References[0].Reason : string.Empty;
                    sheet.Cell(rowIndex, 14).Value = l.UnitOfMeasure;
                    sheet.Cell(rowIndex, 15).Value = l.Description;
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
            "Guardar Documentos Faturação",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Documentos Faturação - Impostos.xlsx",
            ".xlsx");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Impostos");

            var taxes_selling_group = Documents
                .SelectMany(i => i.Line)
                .Where(c => c.CreditAmount > 0)
                .GroupBy(l => new { l.InvoiceNo, l.Tax.TaxPercentage })
                .Select(g => new { g.Key.InvoiceNo, Tax = g.Key.TaxPercentage, NetTotal = g.Sum(l => l.Quantity * l.UnitPrice) })
                .OrderBy(g => g.InvoiceNo)
                .ThenBy(g => g.Tax);

            var rowIndex = 1;

            sheet.Cell(rowIndex, 1).Value = "Documento";
            sheet.Cell(rowIndex, 2).Value = "Imposto";
            sheet.Cell(rowIndex, 3).Value = "Incidência";
            sheet.Cell(rowIndex, 4).Value = "Total";

            rowIndex++;
            foreach (var tax in taxes_selling_group)
            {
                sheet.Cell(rowIndex, 1).Value = tax.InvoiceNo;
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
        var docs = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice ?? [];

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
    private static bool FilterDocuments(SourceDocumentsSalesInvoicesInvoice invoice, string filter)
    {
        if (invoice.ATCUD != null && invoice.ATCUD.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.CustomerID != null && invoice.CustomerID.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.DocumentStatus != null && invoice.DocumentStatus.InvoiceStatus.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.DocumentStatus != null && invoice.DocumentStatus.Reason != null && invoice.DocumentStatus.Reason.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.EACCode != null && invoice.EACCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.InvoiceNo != null && invoice.InvoiceNo.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.InvoiceType.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.Period != null && invoice.Period.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.SourceID != null && invoice.SourceID.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || invoice.TransactionID != null && invoice.TransactionID.Contains(filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
    private static bool FilterDocumentsByDate(SourceDocumentsSalesInvoicesInvoice invoice, DateTimeOffset start, DateTimeOffset end)
    {
        if (invoice.InvoiceDate >= start && invoice.InvoiceDate <= end)
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
        var allLines = ShowAllLines ? saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice?.SelectMany(i => i.Line) ?? [] : CurrentInvoice?.Line ?? [];

        if (string.IsNullOrWhiteSpace(FilterLines))
        {
            Lines = allLines.ToArray();
            return;
        }

        Lines = allLines.Where(l => FilterDocumentLines(l, FilterLines)).ToArray();
    }
    private static bool FilterDocumentLines(SourceDocumentsSalesInvoicesInvoiceLine line, string filter)
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
        var lines = CurrentInvoice.Line.Select(l => new Models.Reporting.DocumentLine
        {
            LineNumber = l.LineNumber,
            ProductCode = l.ProductCode,
            ProductDescription = l.ProductDescription,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            TaxBase = l.TaxBase,
            Description = l.Description,
            TaxCode = l.Tax.TaxCode,
            Reference = l.References != null && l.References.Length > 0 ? l.References[0].Reference : null,
        }).ToArray();

        var taxes = new List<Models.Reporting.Tax>();
        foreach (var l in CurrentInvoice.Line)
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
            Number = CurrentInvoice.InvoiceNo,
            ATCUD = CurrentInvoice.ATCUD,
            Date = CurrentInvoice.InvoiceDate.ToLongDateString(),
            SystemDate = CurrentInvoice.SystemEntryDate.ToLongDateString(),
            Total = CurrentInvoice.DocumentTotals.GrossTotal.ToString("C"),
            NetTotal = CurrentInvoice.DocumentTotals.NetTotal.ToString("C"),
            VatTotal = CurrentInvoice.DocumentTotals.TaxPayable.ToString("C"),
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
        if (CurrentInvoice == null)
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

            sheet.Cell(rowIndex, 1).Value = CurrentInvoice.ATCUD;
            sheet.Cell(rowIndex, 2).Value = CurrentInvoice.InvoiceType.ToString();
            sheet.Cell(rowIndex, 3).Value = CurrentInvoice.InvoiceNo;
            sheet.Cell(rowIndex, 4).Value = CurrentInvoice.DocumentStatus.InvoiceStatus.ToString();
            sheet.Cell(rowIndex, 5).Value = CurrentInvoice.InvoiceDate;
            sheet.Cell(rowIndex, 6).Value = CurrentInvoice.CustomerID;
            sheet.Cell(rowIndex, 7).Value = CurrentInvoice.DocumentTotals.NetTotal;
            sheet.Cell(rowIndex, 8).Value = CurrentInvoice.DocumentTotals.TaxPayable;
            sheet.Cell(rowIndex, 9).Value = CurrentInvoice.DocumentTotals.GrossTotal;

            rowIndex += 2;

            //create lines header
            LineHeader(sheet, rowIndex);

            foreach (var l in CurrentInvoice.Line)
            {
                rowIndex++;

                sheet.Cell(rowIndex, 1).Value = l.LineNumber;
                sheet.Cell(rowIndex, 2).Value = l.ProductCode;
                sheet.Cell(rowIndex, 3).Value = l.ProductDescription;
                sheet.Cell(rowIndex, 4).Value = l.Quantity;
                sheet.Cell(rowIndex, 5).Value = l.UnitPrice;
                sheet.Cell(rowIndex, 6).Value = l.CreditAmount;
                sheet.Cell(rowIndex, 7).Value = l.DebitAmount;
                sheet.Cell(rowIndex, 8).Value = l.SettlementAmount;
                sheet.Cell(rowIndex, 9).Value = l.Tax.TaxPercentage;
                sheet.Cell(rowIndex, 10).Value = l.TaxExemptionReason;
                sheet.Cell(rowIndex, 11).Value = l.TaxExemptionCode;
                sheet.Cell(rowIndex, 12).Value = l.References != null && l.References.Length > 0 ? l.References[0].Reference : string.Empty;
                sheet.Cell(rowIndex, 13).Value = l.References != null && l.References.Length > 0 ? l.References[0].Reason : string.Empty;
                sheet.Cell(rowIndex, 14).Value = l.UnitOfMeasure;
                sheet.Cell(rowIndex, 15).Value = l.Description;
            }

            sheet.Columns().AdjustToContents();

            workbook.SaveAs(file);
        }
    }

    [RelayCommand]
    private async Task OnTestHash()
    {
        if (Documents == null || Documents.Count == 0) return;

        //parse the InvoiceNo property to get the current invoice number
        string[] invoiceNo = CurrentInvoice.InvoiceNo.Split('/');
        if (invoiceNo != null && invoiceNo.Length == 2)
        {
            int.TryParse(invoiceNo[1], out int num);

            //found a valid number, try to find the previous document
            var previousDocument = Documents
                .Where(i => i.InvoiceNo.IndexOf(string.Format("{0}/{1}", invoiceNo[0], num - 1), StringComparison.OrdinalIgnoreCase) == 0)
                .FirstOrDefault();

            //encontramos um documento, vamos obter a hash
            if (previousDocument != null || num == 1)
            {
                var vm = new DialogHashTestViewModel();
                vm.Init();
                vm.InitFromInvoice(CurrentInvoice, previousDocument?.Hash ?? string.Empty);

                await dialogManager.ShowChildDialogAsync(vm);
            }
            else
            {
                dialogManager.ShowNotification("Hash", "Não é possível testar a Hash deste documento, falta o documento anterior", Avalonia.Controls.Notifications.NotificationType.Warning);
            }
        }
    }

    private static int Operation(SourceDocumentsSalesInvoicesInvoice i, SourceDocumentsSalesInvoicesInvoiceLine l)
    {
        if (i.InvoiceType == InvoiceType.FT || i.InvoiceType == InvoiceType.VD || i.InvoiceType == InvoiceType.ND || i.InvoiceType == InvoiceType.FR || i.InvoiceType == InvoiceType.FS || i.InvoiceType == InvoiceType.TV || i.InvoiceType == InvoiceType.AA)
            return l.CreditAmount > 0 ? 1 : -1;
        else if (i.InvoiceType == InvoiceType.NC || i.InvoiceType == InvoiceType.TD || i.InvoiceType == InvoiceType.DA || i.InvoiceType == InvoiceType.RE)
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
        sheet.Cell(row, 1).Value = "Nº linha";
        sheet.Cell(row, 2).Value = "Código produto";
        sheet.Cell(row, 3).Value = "Descrição produto";
        sheet.Cell(row, 4).Value = "Quantidade";
        sheet.Cell(row, 5).Value = "Preço";
        sheet.Cell(row, 6).Value = "Crédito";
        sheet.Cell(row, 7).Value = "Débito";
        sheet.Cell(row, 8).Value = "Desconto";
        sheet.Cell(row, 9).Value = "Imposto";
        sheet.Cell(row, 10).Value = "Isenção";
        sheet.Cell(row, 11).Value = "Cód. Isenção";
        sheet.Cell(row, 12).Value = "Referência";
        sheet.Cell(row, 13).Value = "Razão";
        sheet.Cell(row, 14).Value = "Unidade medida";
        sheet.Cell(row, 15).Value = "Descrição";
    }

    public static string GetATQrCode(Header header, Customer customer, SourceDocumentsSalesInvoicesInvoice invoice)
    {
        var taxes = invoice.Line.Select(l => l.Tax).ToArray();

        var qrCode = new StringBuilder();
        qrCode.AppendFormat("A:{0}", header.TaxRegistrationNumber);
        qrCode.AppendFormat("*B:{0}", customer.CustomerTaxID);
        qrCode.AppendFormat("*C:{0}", customer.BillingAddress.Country);
        qrCode.AppendFormat("*D:{0}", invoice.InvoiceType);
        qrCode.AppendFormat("*E:{0}", invoice.DocumentStatus.InvoiceStatus);
        qrCode.AppendFormat("*F:{0:yyyyMMdd}", invoice.InvoiceDate);
        qrCode.AppendFormat("*G:{0}", invoice.InvoiceNo);
        qrCode.AppendFormat("*H:{0}", invoice.ATCUD);

        if (taxes != null && taxes.Length > 0)
        {
            var taxesGrouped = taxes.OrderBy(t => t.TaxCode).GroupBy(t => t.TaxCountryRegion);
            foreach (var group in taxesGrouped)
            {
                string countryLetter = "I";
                if (group.Key == "PT")
                    qrCode.Append("*I1:PT");
                else if (group.Key == "PT-AC")
                {
                    countryLetter = "J";
                    qrCode.Append("*J1:PT-AC");
                }
                else if (group.Key == "PT-MA")
                {
                    countryLetter = "K";
                    qrCode.Append("*K1:PT-MA");
                }

                foreach (var tax in group)
                {
                    var valueSum = invoice.Line.Where(l => l.Tax.TaxPercentage == tax.TaxPercentage).Sum(l => l.CreditAmount);

                    if (tax.TaxPercentage == 0)
                        qrCode.AppendFormat(CultureInfo.InvariantCulture, "*{0}2:{1:N2}", countryLetter, valueSum);
                    else if (tax.TaxCode == "NOR")
                        qrCode.AppendFormat(CultureInfo.InvariantCulture, "*{0}7:{1:N2}*{0}8:{2:N2}", countryLetter, valueSum, tax.TaxAmount);
                    else if (tax.TaxCode == "INT")
                        qrCode.AppendFormat(CultureInfo.InvariantCulture, "*{0}5:{1:N2}*{0}6:{2:N2}", countryLetter, valueSum, tax.TaxAmount);
                    else if (tax.TaxCode == "RED")
                        qrCode.AppendFormat(CultureInfo.InvariantCulture, "*{0}3:{1:N2}*{0}4:{2:N2}", countryLetter, valueSum, tax.TaxAmount);
                }
            }
        }
        else
            qrCode.Append("*I1:0");

        qrCode.AppendFormat(CultureInfo.InvariantCulture, "*N:{0:N2}", invoice.DocumentTotals.TaxPayable);
        qrCode.AppendFormat(CultureInfo.InvariantCulture, "*O:{0:N2}", invoice.DocumentTotals.GrossTotal);

        if (invoice.WithholdingTax != null && invoice.WithholdingTax.Length > 0)
            qrCode.AppendFormat(CultureInfo.InvariantCulture, "*P:{0:N2}", invoice.WithholdingTax.Sum(r => r.WithholdingTaxAmount));

        var hash = invoice.Hash;
        qrCode.AppendFormat(CultureInfo.InvariantCulture, "*Q:{0}", $"{hash[0]}{hash[10]}{hash[20]}{hash[30]}");
        qrCode.AppendFormat(CultureInfo.InvariantCulture, "*R:{0}", header.SoftwareCertificateNumber);

        return qrCode.ToString();
    }

}
