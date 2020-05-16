using Avalonia.Collections;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Saft;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Desktop.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class PaymentsPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;

        public PaymentsPageViewModel(IScreen screen) : base(screen, MenuIds.INVOICES_PAGE)
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

        protected override void HandleActivation()
        {
            IsLoading = true;

            Task.Run(() =>
            {
                var payments = new List<SourceDocumentsPaymentsPayment>();
                if (saftValidator?.SaftFileV4?.SourceDocuments?.Payments != null)
                {
                    var saft_payments = saftValidator.SaftFileV4.SourceDocuments?.Payments.Payment;

                    foreach (var c in saft_payments)
                    {
                        payments.Add(new SourceDocumentsPaymentsPayment
                        {
                            ATCUD = c.ATCUD,
                            CustomerID = c.CustomerID,
                            DocumentStatus = new SourceDocumentsPaymentsPaymentDocumentStatus
                            {
                                PaymentStatus = c.DocumentStatus?.PaymentStatus.ToString(),
                                PaymentStatusDate = c.DocumentStatus?.PaymentStatusDate ?? DateTime.MinValue,
                                Reason = c.DocumentStatus?.Reason,
                                SourceID = c.DocumentStatus?.SourceID,
                                SourcePayment = c.DocumentStatus?.SourcePayment.ToString()
                            },
                            DocumentTotals = new SourceDocumentsPaymentsPaymentDocumentTotals
                            {
                                Currency = new Currency
                                {
                                    CurrencyAmount = c.DocumentTotals?.Currency?.CurrencyAmount ?? 0,
                                    CurrencyCode = c.DocumentTotals?.Currency?.CurrencyCode,
                                    ExchangeRate = c.DocumentTotals?.Currency?.ExchangeRate ?? 0
                                },
                                GrossTotal = c.DocumentTotals?.GrossTotal ?? 0,
                                NetTotal = c.DocumentTotals?.NetTotal ?? 0,
                                Settlement = new SourceDocumentsPaymentsPaymentDocumentTotalsSettlement
                                {
                                    SettlementAmount = c.DocumentTotals?.Settlement?.SettlementAmount ?? 0
                                },
                                TaxPayable = c.DocumentTotals?.TaxPayable ?? 0
                            },
                            Description = c.Description,
                            PaymentMethod = c.PaymentMethod?.Select(p => new PaymentMethod
                            {
                                PaymentAmount = p.PaymentAmount,
                                PaymentDate = p.PaymentDate,
                                PaymentMechanism = p.PaymentMechanism.ToString()
                            }).ToArray(),
                            PaymentRefNo = c.PaymentRefNo,
                            PaymentType = c.PaymentType.ToString(),
                            SystemID = c.SystemID,
                            TransactionDate = c.TransactionDate,
                            Line = c.Line?.Select(l => new SourceDocumentsPaymentsPaymentLine
                            {
                                PaymentRefNo = c.PaymentRefNo,
                                CreditAmount = l.ItemElementName == Models.SaftV4.ItemChoiceType8.CreditAmount ? l.Item : 0,
                                DebitAmount = l.ItemElementName == Models.SaftV4.ItemChoiceType8.DebitAmount ? l.Item : 0,
                                LineNumber = l.LineNumber,
                                Tax = new PaymentTax
                                {
                                    TaxAmount = l.Tax?.ItemElementName == Models.SaftV4.ItemChoiceType.TaxAmount ? l.Tax?.Item : 0,
                                    TaxPercentage = l.Tax?.ItemElementName == Models.SaftV4.ItemChoiceType.TaxPercentage ? l.Tax?.Item : 0,
                                    TaxCode = l.Tax?.TaxCode,
                                    TaxCountryRegion = l.Tax?.TaxCountryRegion,
                                    TaxType = l.Tax?.TaxType.ToString()
                                },
                                TaxExemptionCode = l.TaxExemptionCode,
                                TaxExemptionReason = l.TaxExemptionReason,
                                SettlementAmount = l.SettlementAmount,
                                SourceDocumentID = l.SourceDocumentID?.Select(s => new SourceDocumentsPaymentsPaymentLineSourceDocumentID
                                {
                                    Description = s.Description,
                                    InvoiceDate = s.InvoiceDate,
                                    OriginatingON = s.OriginatingON
                                }).ToArray(),
                            }).ToArray(),
                            Period = c.Period,
                            SourceID = c.SourceID,
                            SystemEntryDate = c.SystemEntryDate,
                            TransactionID = c.TransactionID,
                            WithholdingTax = c.WithholdingTax?.Select(w => new WithholdingTax
                            {
                                WithholdingTaxAmount = w.WithholdingTaxAmount,
                                WithholdingTaxDescription = w.WithholdingTaxDescription,
                                WithholdingTaxType = w.WithholdingTaxType.ToString()
                            }).ToArray()
                        });
                    }
                }
                else if (saftValidator?.SaftFileV3?.SourceDocuments?.Payments != null)
                {
                    var saft_payments = saftValidator.SaftFileV3.SourceDocuments.Payments.Payment;

                    foreach (var c in saft_payments)
                    {
                        payments.Add(new SourceDocumentsPaymentsPayment
                        {
                            CustomerID = c.CustomerID,
                            DocumentStatus = new SourceDocumentsPaymentsPaymentDocumentStatus
                            {
                                PaymentStatus = c.DocumentStatus?.PaymentStatus.ToString(),
                                PaymentStatusDate = c.DocumentStatus?.PaymentStatusDate ?? DateTime.MinValue,
                                Reason = c.DocumentStatus?.Reason,
                                SourceID = c.DocumentStatus?.SourceID,
                                SourcePayment = c.DocumentStatus?.SourcePayment.ToString()
                            },
                            DocumentTotals = new SourceDocumentsPaymentsPaymentDocumentTotals
                            {
                                Currency = new Currency
                                {
                                    CurrencyAmount = c.DocumentTotals?.Currency?.CurrencyAmount ?? 0,
                                    CurrencyCode = c.DocumentTotals?.Currency?.CurrencyCode,
                                    ExchangeRate = c.DocumentTotals?.Currency?.ExchangeRate ?? 0
                                },
                                GrossTotal = c.DocumentTotals?.GrossTotal ?? 0,
                                NetTotal = c.DocumentTotals?.NetTotal ?? 0,
                                Settlement = new SourceDocumentsPaymentsPaymentDocumentTotalsSettlement
                                {
                                    SettlementAmount = c.DocumentTotals?.Settlement?.SettlementAmount ?? 0
                                },
                                TaxPayable = c.DocumentTotals?.TaxPayable ?? 0
                            },
                            Description = c.Description,
                            PaymentMethod = c.PaymentMethod?.Select(p => new PaymentMethod
                            {
                                PaymentAmount = p.PaymentAmount,
                                PaymentDate = p.PaymentDate,
                                PaymentMechanism = p.PaymentMechanism.ToString()
                            }).ToArray(),
                            PaymentRefNo = c.PaymentRefNo,
                            PaymentType = c.PaymentType.ToString(),
                            SystemID = c.SystemID,
                            TransactionDate = c.TransactionDate,
                            Line = c.Line?.Select(l => new SourceDocumentsPaymentsPaymentLine
                            {
                                CreditAmount = l.ItemElementName == Models.SaftV3.ItemChoiceType9.CreditAmount ? l.Item : 0,
                                DebitAmount = l.ItemElementName == Models.SaftV3.ItemChoiceType9.DebitAmount ? l.Item : 0,
                                LineNumber = l.LineNumber,
                                Tax = new PaymentTax
                                {
                                    TaxAmount = l.Tax?.ItemElementName == Models.SaftV3.ItemChoiceType.TaxAmount ? l.Tax?.Item : 0,
                                    TaxPercentage = l.Tax?.ItemElementName == Models.SaftV3.ItemChoiceType.TaxPercentage ? l.Tax?.Item : 0,
                                    TaxCode = l.Tax?.TaxCode,
                                    TaxCountryRegion = l.Tax?.TaxCountryRegion,
                                    TaxType = l.Tax?.TaxType.ToString()
                                },
                                TaxExemptionReason = l.TaxExemptionReason,
                                SettlementAmount = l.SettlementAmount,
                                SourceDocumentID = l.SourceDocumentID?.Select(s => new SourceDocumentsPaymentsPaymentLineSourceDocumentID
                                {
                                    Description = s.Description,
                                    InvoiceDate = s.InvoiceDate,
                                    OriginatingON = s.OriginatingON
                                }).ToArray(),
                            }).ToArray(),
                            Period = c.Period,
                            SourceID = c.SourceID,
                            SystemEntryDate = c.SystemEntryDate,
                            TransactionID = c.TransactionID,
                            WithholdingTax = c.WithholdingTax?.Select(w => new WithholdingTax
                            {
                                WithholdingTaxAmount = w.WithholdingTaxAmount,
                                WithholdingTaxDescription = w.WithholdingTaxDescription,
                                WithholdingTaxType = w.WithholdingTaxType.ToString()
                            }).ToArray()
                        });
                    }
                }

                return payments;
            }).ContinueWith(async c =>
            {
                var payments = await c;

                DocNumberOfEntries = payments.Count();
                DocTotalCredit = payments
                    .Where(i => i.DocumentStatus.PaymentStatus != "A" && i.DocumentStatus.PaymentStatus != "F")
                    .Sum(i => i.Line.Sum(l => l.CreditAmount ?? 0))
                    .ToString("c");

                DocTotalDebit = payments
                    .Where(i => i.DocumentStatus.PaymentStatus != "A" && i.DocumentStatus.PaymentStatus != "F")
                    .Sum(i => i.Line.Sum(l => l.DebitAmount ?? 0))
                    .ToString("c");

                if (saftValidator?.SaftFileV4?.SourceDocuments?.SalesInvoices != null)
                {
                    NumberOfEntries = saftValidator.SaftFileV4.SourceDocuments.SalesInvoices.NumberOfEntries;
                    TotalCredit = saftValidator.SaftFileV4.SourceDocuments.SalesInvoices.TotalCredit.ToString("c");
                    TotalDebit = saftValidator.SaftFileV4.SourceDocuments.SalesInvoices.TotalDebit.ToString("c");
                }
                else if (saftValidator?.SaftFileV3?.SourceDocuments?.SalesInvoices != null)
                {
                    NumberOfEntries = saftValidator.SaftFileV3.SourceDocuments.SalesInvoices.NumberOfEntries;
                    TotalCredit = saftValidator.SaftFileV3.SourceDocuments.SalesInvoices.TotalCredit.ToString("c");
                    TotalDebit = saftValidator.SaftFileV3.SourceDocuments.SalesInvoices.TotalDebit.ToString("c");
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
                            if (payment.DocumentStatus != null && payment.DocumentStatus.PaymentStatus != null && payment.DocumentStatus.PaymentStatus.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.DocumentStatus != null && payment.DocumentStatus.Reason != null && payment.DocumentStatus.Reason.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.PaymentRefNo != null && payment.PaymentRefNo.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (payment.PaymentType != null && payment.PaymentType.Contains(Filter, StringComparison.OrdinalIgnoreCase))
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
                            if (ShowAllLines == false && line.PaymentRefNo.Equals(CurrentPayment.PaymentRefNo, StringComparison.OrdinalIgnoreCase) == false)
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
                    .InvokeCommand(SearchCommand);
                this.WhenAnyValue(x => x.FilterLines)
                    .Throttle(TimeSpan.FromSeconds(1))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .InvokeCommand(SearchDetailsCommand);

                IsLoading = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
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
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsSalesInvoicesInvoice> invoice)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar Documentos Faturação",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documentos Faturação.xlsx",
                    ".xlsx");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    // Create a new workbook and a sheet named "User Accounts"
                    var workbook = new XSSFWorkbook();
                    var sheet = workbook.CreateSheet("Doumentos");

                    // Create the style object
                    var detailSubtotalCellStyle = workbook.CreateCellStyle();
                    // Define a thin border for the top and bottom of the cell
                    detailSubtotalCellStyle.BorderTop = BorderStyle.Thin;
                    detailSubtotalCellStyle.BorderBottom = BorderStyle.Thin;

                    var row_header = sheet.CreateRow(0);
                    GenerateHeaderExcel(row_header, detailSubtotalCellStyle);

                    var rowIndex = 1;
                    foreach (var c in invoice)
                    {
                        //create a new row
                        var row = sheet.CreateRow(rowIndex);

                        row.CreateCell(0).SetCellValue(c.ATCUD);
                        row.CreateCell(1).SetCellValue(c.InvoiceType);
                        row.CreateCell(2).SetCellValue(c.InvoiceNo);
                        row.CreateCell(3).SetCellValue(c.DocumentStatus.InvoiceStatus);
                        row.CreateCell(4).SetCellValue(c.InvoiceDate);
                        row.CreateCell(5).SetCellValue(c.CustomerID);
                        row.CreateCell(6).SetCellValue(Convert.ToDouble(c.DocumentTotals.NetTotal));
                        row.CreateCell(7).SetCellValue(Convert.ToDouble(c.DocumentTotals.TaxPayable));
                        row.CreateCell(8).SetCellValue(Convert.ToDouble(c.DocumentTotals.GrossTotal));

                        rowIndex += 2;

                        //create a new row
                        var row_line_header = sheet.CreateRow(rowIndex);

                        //create lines header
                        GenerateLineHeaderExcel(row_line_header, detailSubtotalCellStyle);

                        foreach (var l in c.Line)
                        {
                            rowIndex++;

                            //create a new row
                            var row_line = sheet.CreateRow(rowIndex);

                            row_line.CreateCell(1).SetCellValue(l.LineNumber);
                            row_line.CreateCell(2).SetCellValue(l.ProductCode);
                            row_line.CreateCell(3).SetCellValue(l.ProductDescription);
                            row_line.CreateCell(4).SetCellValue(Convert.ToDouble(l.Quantity));
                            row_line.CreateCell(5).SetCellValue(Convert.ToDouble(l.UnitPrice));
                            row_line.CreateCell(6).SetCellValue(Convert.ToDouble(l.CreditAmount));
                            row_line.CreateCell(7).SetCellValue(Convert.ToDouble(l.DebitAmount));
                            row_line.CreateCell(8).SetCellValue(Convert.ToDouble(l.SettlementAmount));
                            row_line.CreateCell(9).SetCellValue(Convert.ToDouble(l.Tax.TaxPercentage));
                            row_line.CreateCell(10).SetCellValue(l.TaxExemptionReason);
                            row_line.CreateCell(11).SetCellValue(l.TaxExemptionCode);
                            row_line.CreateCell(12).SetCellValue(l.References != null && l.References.Length > 0 ? l.References[0].Reference : string.Empty);
                            row_line.CreateCell(13).SetCellValue(l.References != null && l.References.Length > 0 ? l.References[0].Reason : string.Empty);
                            row_line.CreateCell(14).SetCellValue(l.UnitOfMeasure);
                            row_line.CreateCell(15).SetCellValue(l.TaxPointDate);
                            row_line.CreateCell(16).SetCellValue(l.Description);
                        }

                        rowIndex += 2;
                    }

                    for (int i = 0; i <= 16; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }

                    using FileStream fs = new FileStream(file, FileMode.Create);
                    workbook.Write(fs);
                    fs.Close();
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
                    // Create a new workbook and a sheet named "User Accounts"
                    var workbook = new XSSFWorkbook();
                    var sheet = workbook.CreateSheet("Impostos");

                    // Create the style object
                    var detailSubtotalCellStyle = workbook.CreateCellStyle();
                    // Define a thin border for the top and bottom of the cell
                    detailSubtotalCellStyle.BorderTop = BorderStyle.Thin;
                    detailSubtotalCellStyle.BorderBottom = BorderStyle.Thin;

                    var taxes_selling_group = invoices
                        .SelectMany(i => i.Line)
                        .Where(c => c.CreditAmount > 0)
                        .GroupBy(l => new { l.InvoiceNo, l.Tax.TaxPercentage })
                        .Select(g => new { g.Key.InvoiceNo, Tax = g.Key.TaxPercentage, NetTotal = g.Sum(l => l.Quantity * l.UnitPrice) })
                        .OrderBy(g => g.InvoiceNo)
                        .ThenBy(g => g.Tax);

                    var rowIndex = 1;
                    foreach (var tax in taxes_selling_group)
                    {
                        //create a new row
                        var row = sheet.CreateRow(rowIndex);

                        row.CreateCell(0).SetCellValue(tax.InvoiceNo);
                        row.CreateCell(1).SetCellValue(Convert.ToDouble(tax.Tax));
                        row.CreateCell(2).SetCellValue(Convert.ToDouble(tax.NetTotal));
                        row.CreateCell(3).SetCellValue(Convert.ToDouble(
                            Math.Round(Math.Round(tax.NetTotal, 2, MidpointRounding.AwayFromZero) * tax.Tax.GetValueOrDefault(0) * 0.01m, 2, MidpointRounding.AwayFromZero)));

                        rowIndex++;
                    }

                    var total_row = sheet.CreateRow(rowIndex);
                    total_row.CreateCell(3).SetCellFormula($"SOMA(D2:D{rowIndex})");

                    for (int i = 0; i <= 2; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }


                    using FileStream fs = new FileStream(file, FileMode.Create);
                    workbook.Write(fs);
                    fs.Close();
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
                    "Guardar Documento Faturação",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documento Faturação.xlsx",
                    "xlsx");

            if (string.IsNullOrWhiteSpace(file) == false)
            {
                // Create a new workbook and a sheet named "User Accounts"
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Doumento");

                // Create the style object
                var detailSubtotalCellStyle = workbook.CreateCellStyle();
                // Define a thin border for the top and bottom of the cell
                detailSubtotalCellStyle.BorderTop = BorderStyle.Thin;
                detailSubtotalCellStyle.BorderBottom = BorderStyle.Thin;

                var row_header = sheet.CreateRow(0);
                GenerateHeaderExcel(row_header, detailSubtotalCellStyle);

                var rowIndex = 1;

                //create a new row
                var row = sheet.CreateRow(rowIndex);

                var c = CurrentPayment;

                row.CreateCell(0).SetCellValue(c.ATCUD);
                row.CreateCell(1).SetCellValue(c.PaymentType);
                row.CreateCell(2).SetCellValue(c.PaymentRefNo);
                row.CreateCell(3).SetCellValue(c.DocumentStatus.PaymentStatus);
                row.CreateCell(4).SetCellValue(c.SystemEntryDate);
                row.CreateCell(5).SetCellValue(c.CustomerID);
                row.CreateCell(6).SetCellValue(Convert.ToDouble(c.DocumentTotals.NetTotal));
                row.CreateCell(7).SetCellValue(Convert.ToDouble(c.DocumentTotals.TaxPayable));
                row.CreateCell(8).SetCellValue(Convert.ToDouble(c.DocumentTotals.GrossTotal));

                rowIndex += 2;

                //create a new row
                var row_line_header = sheet.CreateRow(rowIndex);

                //create lines header
                GenerateLineHeaderExcel(row_line_header, detailSubtotalCellStyle);

                foreach (var l in c.Line)
                {
                    rowIndex++;

                    //create a new row
                    var row_line = sheet.CreateRow(rowIndex);

                    row_line.CreateCell(1).SetCellValue(l.LineNumber);
                    row_line.CreateCell(2).SetCellValue(Convert.ToDouble(l.CreditAmount));
                    row_line.CreateCell(3).SetCellValue(Convert.ToDouble(l.DebitAmount));
                    row_line.CreateCell(4).SetCellValue(Convert.ToDouble(l.SettlementAmount));
                    row_line.CreateCell(5).SetCellValue(Convert.ToDouble(l.Tax.TaxPercentage));
                    row_line.CreateCell(6).SetCellValue(l.TaxExemptionReason);
                    row_line.CreateCell(7).SetCellValue(l.TaxExemptionCode);
                }

                for (int i = 0; i <= 16; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                using FileStream fs = new FileStream(file, FileMode.Create);
                workbook.Write(fs);
                fs.Close();
            }
        }
        
        private int Operation(SourceDocumentsPaymentsPaymentLine l)
        {
            return l.CreditAmount > 0 ? 1 : -1;
        }

        private void GenerateHeaderExcel(IRow row, ICellStyle cellStyle)
        {
            var cell = row.CreateCell(0);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("ATCUD");

            cell = row.CreateCell(1);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Tipo");

            cell = row.CreateCell(2);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Nº");

            cell = row.CreateCell(3);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Estado");

            cell = row.CreateCell(4);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Data");

            cell = row.CreateCell(5);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Cliente");

            cell = row.CreateCell(6);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Incidência");

            cell = row.CreateCell(7);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("IVA");

            cell = row.CreateCell(8);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Total");
        }

        private void GenerateLineHeaderExcel(IRow row, ICellStyle cellStyle)
        {
            var cell = row.CreateCell(1);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Nº linha");

            cell = row.CreateCell(2);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Crédito");

            cell = row.CreateCell(3);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Débito");

            cell = row.CreateCell(4);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Desconto");

            cell = row.CreateCell(5);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Imposto");

            cell = row.CreateCell(6);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Isenção");

            cell = row.CreateCell(7);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Cód. Isenção");
        }
    }
}
