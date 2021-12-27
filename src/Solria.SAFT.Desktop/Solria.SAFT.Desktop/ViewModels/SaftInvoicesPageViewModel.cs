using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Desktop.Views;
using Solria.SAFT.Parser.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class SaftInvoicesPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;
        private readonly IReportService reportService;

        private Header header;

        public SaftInvoicesPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_INVOICES_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();
            reportService = Locator.Current.GetService<IReportService>();

            DoPrintCommand = ReactiveCommand.CreateFromTask(OnDoPrint);
            DoPrintTaxesCommand = ReactiveCommand.CreateFromTask(OnDoPrintTaxes);
            SearchCommand = ReactiveCommand.Create<string>(OnSearch);
            SearchClearCommand = ReactiveCommand.Create(OnSearchClear);
            SearchDetailsCommand = ReactiveCommand.Create<string>(OnSearchDetails);
            SearchDetailsClearCommand = ReactiveCommand.Create(OnSearchDetailsClear);
            ShowCustomerCommand = ReactiveCommand.Create(OnShowCustomer);
            ShowInvoiceDetailsCommand = ReactiveCommand.Create(OnShowInvoiceDetails);
            DoOpenExcelCommand = ReactiveCommand.CreateFromTask(OnSaveExcel);
            TestHashCommand = ReactiveCommand.CreateFromTask(OnTestHash);
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            IsLoading = true;

            ToolTip = new SourceDocumentsToolTipService();
            ToolTipLine = new SourceDocumentsSalesInvoicesInvoiceLineToolTipService();

            header = saftValidator.SaftFile?.Header;


            var invoices = saftValidator.SaftFile?.SourceDocuments?.SalesInvoices?.Invoice ?? Array.Empty<SourceDocumentsSalesInvoicesInvoice>();

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

            CollectionView = new DataGridCollectionView(invoices)
            {
                Filter = o =>
                {
                    if (string.IsNullOrWhiteSpace(Filter))
                        return true;

                    if (o is SourceDocumentsSalesInvoicesInvoice invoice)
                    {
                        if (invoice.ATCUD != null && invoice.ATCUD.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.CustomerID != null && invoice.CustomerID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.DocumentStatus != null && invoice.DocumentStatus.InvoiceStatus.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.DocumentStatus != null && invoice.DocumentStatus.Reason != null && invoice.DocumentStatus.Reason.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.EACCode != null && invoice.EACCode.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.InvoiceNo != null && invoice.InvoiceNo.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.InvoiceType.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.Period != null && invoice.Period.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.SourceID != null && invoice.SourceID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (invoice.TransactionID != null && invoice.TransactionID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    return false;
                }
            };
            CollectionViewDetails = new DataGridCollectionView(invoices.SelectMany(d => d.Line))
            {
                Filter = o =>
                {
                    if (CurrentInvoice == null)
                        return false;

                    if (o is SourceDocumentsSalesInvoicesInvoiceLine line)
                    {
                        if (ShowAllLines == false && line.InvoiceNo.Equals(CurrentInvoice.InvoiceNo, StringComparison.OrdinalIgnoreCase) == false)
                            return false;

                        if (string.IsNullOrWhiteSpace(FilterLines))
                            return true;

                        if (line.Description != null && line.Description.Contains(FilterLines, StringComparison.OrdinalIgnoreCase))
                            return true;

                        if (line.ProductCode != null && line.ProductCode.Contains(FilterLines, StringComparison.OrdinalIgnoreCase))
                            return true;

                        if (line.ProductDescription != null && line.ProductDescription.Contains(FilterLines, StringComparison.OrdinalIgnoreCase))
                            return true;

                        if (line.ProductSerialNumber != null && line.ProductSerialNumber.Any(l => l.Contains(FilterLines, StringComparison.OrdinalIgnoreCase)))
                            return true;

                        if (line.TaxExemptionCode != null && line.TaxExemptionCode.Contains(FilterLines, StringComparison.OrdinalIgnoreCase))
                            return true;

                        if (line.TaxExemptionReason != null && line.TaxExemptionReason.Contains(FilterLines, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    return false;
                }
            };

            CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("InvoiceType"));

            this.WhenAnyValue(x => x.Filter)
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand)
                .DisposeWith(disposables);
            this.WhenAnyValue(x => x.FilterLines)
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchDetailsCommand)
                .DisposeWith(disposables);

            IsLoading = false;
        }

        protected override void HandleDeactivation()
        {
        }

        private SourceDocumentsToolTipService toolTip;
        public SourceDocumentsToolTipService ToolTip
        {
            get => toolTip;
            set => this.RaiseAndSetIfChanged(ref toolTip, value);
        }

        private SourceDocumentsSalesInvoicesInvoiceLineToolTipService toolTipLine;
        public SourceDocumentsSalesInvoicesInvoiceLineToolTipService ToolTipLine
        {
            get => toolTipLine;
            set => this.RaiseAndSetIfChanged(ref toolTipLine, value);
        }

        SourceDocumentsSalesInvoicesInvoice currentInvoice;
        public SourceDocumentsSalesInvoicesInvoice CurrentInvoice
        {
            get => currentInvoice;
            set
            {
                this.RaiseAndSetIfChanged(ref currentInvoice, value);

                if (currentInvoice != null && string.IsNullOrEmpty(FilterLines))
                {
                    DocGrossTotal = currentInvoice.Line.Sum(c => c.UnitPrice * (1 + (c.Tax.TaxPercentage ?? 0) * 0.01m) * c.Quantity * Operation(currentInvoice, c)).ToString("c");
                    DocNetTotal = currentInvoice.Line.Sum(c => c.UnitPrice * c.Quantity * Operation(currentInvoice, c)).ToString("c");
                    DocTaxPayable = currentInvoice.Line.Sum(c => c.UnitPrice * (c.Tax.TaxPercentage ?? 0) * 0.01m * c.Quantity * Operation(currentInvoice, c)).ToString("c");

                    GrossTotal = currentInvoice.DocumentTotals.GrossTotal.ToString("c");
                    NetTotal = currentInvoice.DocumentTotals.NetTotal.ToString("c");
                    TaxPayable = currentInvoice.DocumentTotals.TaxPayable.ToString("c");

                    CollectionViewDetails?.Refresh();
                }
            }
        }

        private DataGridCollectionView collectionViewDetails;
        public DataGridCollectionView CollectionViewDetails
        {
            get => collectionViewDetails;
            set => this.RaiseAndSetIfChanged(ref collectionViewDetails, value);
        }

        bool showAllLines;
        public bool ShowAllLines
        {
            get => showAllLines;
            set
            {
                this.RaiseAndSetIfChanged(ref showAllLines, value);

                CollectionViewDetails.Refresh();
            }
        }

        DateTime filtroDataInicio;
        public DateTime FiltroDataInicio
        {
            get => filtroDataInicio;
            set
            {
                this.RaiseAndSetIfChanged(ref filtroDataInicio, value);
                CollectionView?.Refresh();
            }
        }
        DateTime filtroDataFim;
        public DateTime FiltroDataFim
        {
            get => filtroDataFim;
            set
            {
                this.RaiseAndSetIfChanged(ref filtroDataFim, value);
                CollectionView?.Refresh();
            }
        }

        private string filterLines;
        public string FilterLines
        {
            get => filterLines;
            set => this.RaiseAndSetIfChanged(ref filterLines, value);
        }


        string docGrossTotal;
        string docNetTotal;
        string docTaxPayable;
        int docNumberOfEntries;
        string docTotalCredit;
        string docTotalDebit;
        string grossTotal;
        string netTotal;
        string taxPayable;
        string numberOfEntries;
        string totalCredit;
        string totalDebit;

        public string DocGrossTotal { get => docGrossTotal; set => this.RaiseAndSetIfChanged(ref docGrossTotal, value); }
        public string DocNetTotal { get => docNetTotal; set => this.RaiseAndSetIfChanged(ref docNetTotal, value); }
        public string DocTaxPayable { get => docTaxPayable; set => this.RaiseAndSetIfChanged(ref docTaxPayable, value); }
        public int DocNumberOfEntries { get => docNumberOfEntries; set => this.RaiseAndSetIfChanged(ref docNumberOfEntries, value); }
        public string DocTotalCredit { get => docTotalCredit; set => this.RaiseAndSetIfChanged(ref docTotalCredit, value); }
        public string DocTotalDebit { get => docTotalDebit; set => this.RaiseAndSetIfChanged(ref docTotalDebit, value); }
        public string GrossTotal { get => grossTotal; set => this.RaiseAndSetIfChanged(ref grossTotal, value); }
        public string NetTotal { get => netTotal; set => this.RaiseAndSetIfChanged(ref netTotal, value); }
        public string TaxPayable { get => taxPayable; set => this.RaiseAndSetIfChanged(ref taxPayable, value); }
        public string NumberOfEntries { get => numberOfEntries; set => this.RaiseAndSetIfChanged(ref numberOfEntries, value); }
        public string TotalCredit { get => totalCredit; set => this.RaiseAndSetIfChanged(ref totalCredit, value); }
        public string TotalDebit { get => totalDebit; set => this.RaiseAndSetIfChanged(ref totalDebit, value); }

        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }
        public ReactiveCommand<Unit, Unit> DoPrintTaxesCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchClearCommand { get; }
        public ReactiveCommand<string, Unit> SearchDetailsCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchDetailsClearCommand { get; }

        public ReactiveCommand<Unit, Unit> ShowCustomerCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowInvoiceDetailsCommand { get; }
        public ReactiveCommand<Unit, Unit> DoOpenExcelCommand { get; }
        public ReactiveCommand<Unit, Unit> TestHashCommand { get; }

        private async Task OnDoPrint()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsSalesInvoicesInvoice> invoice)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar Documentos Faturação",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documentos Faturação.xlsx",
                    ".xlsx");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    using var workbook = new ClosedXML.Excel.XLWorkbook(ClosedXML.Excel.XLEventTracking.Disabled);
                    var sheet = workbook.Worksheets.Add("Documentos");

                    DocHeader(sheet, 1);

                    var rowIndex = 2;
                    foreach (var c in invoice)
                    {
                        sheet.Cell(rowIndex, 1).Value = c.ATCUD;
                        sheet.Cell(rowIndex, 2).Value = c.InvoiceType;
                        sheet.Cell(rowIndex, 3).Value = c.InvoiceNo;
                        sheet.Cell(rowIndex, 4).Value = c.DocumentStatus.InvoiceStatus;
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
        }
        private async Task OnDoPrintTaxes()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsSalesInvoicesInvoice> invoices)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar Documentos Faturação",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documentos Faturação - Impostos.xlsx",
                    ".xlsx");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    using var workbook = new ClosedXML.Excel.XLWorkbook(ClosedXML.Excel.XLEventTracking.Disabled);
                    var sheet = workbook.Worksheets.Add("Impostos");

                    var taxes_selling_group = invoices
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
        }

        private void OnSearch(string _)
        {
            CollectionView?.Refresh();
        }
        private void OnSearchClear()
        {
            Filter = null;
        }
        private void OnSearchDetails(string _)
        {
            CollectionViewDetails?.Refresh();
        }
        private void OnSearchDetailsClear()
        {
            FilterLines = null;
        }
        private void OnShowCustomer()
        {

        }
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
                TaxCode = l.Tax.TaxCode
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
            var document = new Models.Reporting.Document { Taxes = taxes.ToArray(), Lines = lines };

            var qrCode = GetATQrCode(header, CurrentInvoice.Customer, CurrentInvoice);
        }
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
                using var workbook = new ClosedXML.Excel.XLWorkbook(ClosedXML.Excel.XLEventTracking.Disabled);
                var sheet = workbook.Worksheets.Add("Documento");

                DocHeader(sheet, 1);

                var rowIndex = 2;

                sheet.Cell(rowIndex, 1).Value = CurrentInvoice.ATCUD;
                sheet.Cell(rowIndex, 2).Value = CurrentInvoice.InvoiceType;
                sheet.Cell(rowIndex, 3).Value = CurrentInvoice.InvoiceNo;
                sheet.Cell(rowIndex, 4).Value = CurrentInvoice.DocumentStatus.InvoiceStatus;
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
        private async Task OnTestHash()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsSalesInvoicesInvoice> invoices)
            {
                //parse the InvoiceNo property to get the current invoice number
                string[] invoiceNo = CurrentInvoice.InvoiceNo.Split('/');
                if (invoiceNo != null && invoiceNo.Length == 2)
                {
                    int.TryParse(invoiceNo[1], out int num);

                    //found a valid number, try to find the previous document
                    var previousDocument = invoices
                        .Where(i => i.InvoiceNo.IndexOf(string.Format("{0}/{1}", invoiceNo[0], num - 1), StringComparison.OrdinalIgnoreCase) == 0)
                        .FirstOrDefault();

                    //encontramos um documento, vamos obter a hash
                    if (previousDocument != null || num == 1)
                    {
                        var view = new DialogHashTest();
                        var vm = new DialogHashTestViewModel();
                        vm.Init();
                        vm.InitFromInvoice(CurrentInvoice, previousDocument?.Hash ?? string.Empty);

                        view.DataContext = vm;

                        await dialogManager.ShowChildDialogAsync(view);
                    }
                    else
                    {
                        dialogManager.ShowNotification("Hash", "Não é possível testar a Hash deste documento, falta o documento anterior", Avalonia.Controls.Notifications.NotificationType.Warning);
                    }
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
}
