﻿using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Parser.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class SaftPaymentsPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;

        public SaftPaymentsPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_PAYMENTS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            DoPrintCommand = ReactiveCommand.CreateFromTask(OnDoPrint);
            DoPrintTaxesCommand = ReactiveCommand.CreateFromTask(OnDoPrintTaxes);
            SearchCommand = ReactiveCommand.Create<string>(OnSearch);
            SearchClearCommand = ReactiveCommand.Create(OnSearchClear);
            SearchDetailsCommand = ReactiveCommand.Create<string>(OnSearchDetails);
            SearchDetailsClearCommand = ReactiveCommand.Create(OnSearchDetailsClear);
            ShowCustomerCommand = ReactiveCommand.Create(OnShowCustomer);
            ShowInvoiceDetailsCommand = ReactiveCommand.Create(OnShowInvoiceDetails);
            DoOpenExcelCommand = ReactiveCommand.CreateFromTask(OnDoOpenExcel);
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            IsLoading = true;

                var payments = saftValidator.SaftFile?.SourceDocuments?.Payments?.Payment ?? Array.Empty<SourceDocumentsPaymentsPayment>();

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

                CollectionView = new DataGridCollectionView(payments)
                {
                    Filter = o =>
                    {
                        if (string.IsNullOrWhiteSpace(Filter))
                            return true;

                        if (o is SourceDocumentsPaymentsPayment payment)
                        {
                            if (payment.ATCUD != null && payment.ATCUD.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.CustomerID != null && payment.CustomerID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.DocumentStatus?.PaymentStatus != null && payment.DocumentStatus.PaymentStatus.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.DocumentStatus != null && payment.DocumentStatus.Reason != null && payment.DocumentStatus.Reason.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.PaymentRefNo != null && payment.PaymentRefNo.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.PaymentType.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.Period != null && payment.Period.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.SourceID != null && payment.SourceID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.TransactionID != null && payment.TransactionID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }

                        return false;
                    }
                };
                CollectionViewDetails = new DataGridCollectionView(payments.SelectMany(d => d.Line))
                {
                    Filter = o =>
                    {
                        if (CurrentPayment == null)
                            return false;

                        if (o is SourceDocumentsPaymentsPaymentLine line)
                        {
                            if (ShowAllLines == false && line.DocNo.Equals(CurrentPayment.PaymentRefNo, StringComparison.OrdinalIgnoreCase) == false)
                                return false;

                            if (string.IsNullOrWhiteSpace(FilterLines))
                                return true;
                            if (line.TaxExemptionCode != null && line.TaxExemptionCode.Contains(FilterLines, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (line.TaxExemptionReason != null && line.TaxExemptionReason.Contains(FilterLines, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }

                        return false;
                    }
                };

                CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("PaymentType"));

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

        SourceDocumentsPaymentsPayment currentPayment;
        public SourceDocumentsPaymentsPayment CurrentPayment
        {
            get => currentPayment;
            set
            {
                this.RaiseAndSetIfChanged(ref currentPayment, value);

                if (currentPayment != null && string.IsNullOrEmpty(FilterLines))
                {
                    DocGrossTotal = currentPayment.Line.Sum(c => (c.CreditAmount ?? c.DebitAmount ?? 0) * (1 + (c.Tax.TaxPercentage ?? 0) * 0.01m) * Operation(c)).ToString("c");
                    DocNetTotal = currentPayment.Line.Sum(c => (c.CreditAmount ?? c.DebitAmount ?? 0) * Operation(c)).ToString("c");
                    DocTaxPayable = currentPayment.Line.Sum(c => (c.CreditAmount ?? c.DebitAmount ?? 0) * (c.Tax.TaxPercentage ?? 0) * 0.01m * Operation(c)).ToString("c");

                    GrossTotal = currentPayment.DocumentTotals.GrossTotal.ToString("c");
                    NetTotal = currentPayment.DocumentTotals.NetTotal.ToString("c");
                    TaxPayable = currentPayment.DocumentTotals.TaxPayable.ToString("c");

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

        private async Task OnDoPrint()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsPaymentsPayment> invoice)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar Documentos Pagamento",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documentos Pagamento.xlsx",
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
                        sheet.Cell(rowIndex, 2).Value = c.PaymentType;
                        sheet.Cell(rowIndex, 3).Value = c.PaymentRefNo;
                        sheet.Cell(rowIndex, 4).Value = c.DocumentStatus.PaymentStatus;
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
        }
        private async Task OnDoPrintTaxes()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsPaymentsPayment> documents)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar Documentos Pagammento",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documentos Pagamentos - Impostos.xlsx",
                    ".xlsx");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    using var workbook = new ClosedXML.Excel.XLWorkbook(ClosedXML.Excel.XLEventTracking.Disabled);
                    var sheet = workbook.Worksheets.Add("Impostos");

                    var taxes_selling_group = documents
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

        }
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
                using var workbook = new ClosedXML.Excel.XLWorkbook(ClosedXML.Excel.XLEventTracking.Disabled);
                var sheet = workbook.Worksheets.Add("Documento");

                DocHeader(sheet, 1);

                var rowIndex = 2;

                sheet.Cell(rowIndex, 1).Value = CurrentPayment.ATCUD;
                sheet.Cell(rowIndex, 2).Value = CurrentPayment.PaymentType;
                sheet.Cell(rowIndex, 3).Value = CurrentPayment.PaymentRefNo;
                sheet.Cell(rowIndex, 4).Value = CurrentPayment.DocumentStatus.PaymentStatus;
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
}
