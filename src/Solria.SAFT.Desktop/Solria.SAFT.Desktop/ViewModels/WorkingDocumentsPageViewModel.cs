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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class WorkingDocumentsPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;

        public WorkingDocumentsPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_WORKING_DOCUMENTS_PAGE)
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
            TestHashCommand = ReactiveCommand.CreateFromTask(OnTestHash);
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            IsLoading = true;

            Task.Run(() =>
            {
                var documents = new List<SourceDocumentsWorkingDocumentsWorkDocument>();
                if (saftValidator?.SaftFileV4?.SourceDocuments?.WorkingDocuments != null)
                {
                    var saft_documents = saftValidator.SaftFileV4.SourceDocuments?.WorkingDocuments.WorkDocument;

                    foreach (var c in saft_documents)
                    {
                        documents.Add(new SourceDocumentsWorkingDocumentsWorkDocument
                        {
                            ATCUD = c.ATCUD,
                            CustomerID = c.CustomerID,
                            DocumentStatus = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus
                            {
                                WorkStatus = c.DocumentStatus?.WorkStatus.ToString(),
                                WorkStatusDate = c.DocumentStatus?.WorkStatusDate ?? DateTime.MinValue,
                                Reason = c.DocumentStatus?.Reason,
                                SourceBilling = c.DocumentStatus?.SourceBilling.ToString(),
                                SourceID = c.DocumentStatus?.SourceID
                            },
                            DocumentTotals = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals
                            {
                                Currency = new Currency
                                {
                                    CurrencyAmount = c.DocumentTotals?.Currency?.CurrencyAmount ?? 0,
                                    CurrencyCode = c.DocumentTotals?.Currency?.CurrencyCode,
                                    ExchangeRate = c.DocumentTotals?.Currency?.ExchangeRate ?? 0
                                },
                                GrossTotal = c.DocumentTotals?.GrossTotal ?? 0,
                                NetTotal = c.DocumentTotals?.NetTotal ?? 0,
                                TaxPayable = c.DocumentTotals?.TaxPayable ?? 0
                            },
                            EACCode = c.EACCode,
                            Hash = c.Hash,
                            HashControl = c.HashControl,
                            WorkType = c.WorkType.ToString(),
                            DocumentNumber = c.DocumentNumber,
                            WorkDate = c.WorkDate,
                            Line = c.Line?.Select(l => new SourceDocumentsWorkingDocumentsWorkDocumentLine
                            {
                                DocumentNumber = l.DocNo,
                                CreditAmount = l.ItemElementName == Models.SaftV4.ItemChoiceType7.CreditAmount ? l.Item : 0,
                                DebitAmount = l.ItemElementName == Models.SaftV4.ItemChoiceType7.DebitAmount ? l.Item : 0,
                                CustomsInformation = new CustomsInformation
                                {
                                    ARCNo = l.CustomsInformation?.ARCNo,
                                    IECAmount = l.CustomsInformation?.IECAmount ?? 0
                                },
                                Description = l.Description,
                                LineNumber = l.LineNumber,
                                OrderReferences = l.OrderReferences?.Select(o => new OrderReferences
                                {
                                    OrderDate = o.OrderDate,
                                    OriginatingON = o.OriginatingON
                                }).ToArray(),
                                ProductCode = l.ProductCode,
                                ProductDescription = l.ProductDescription,
                                ProductSerialNumber = l.ProductSerialNumber,
                                Quantity = l.Quantity,
                                References = l.References?.Select(r => new References
                                {
                                    Reason = r.Reason,
                                    Reference = r.Reference
                                }).ToArray(),
                                SettlementAmount = l.SettlementAmount,
                                Tax = new Tax
                                {
                                    TaxAmount = l.Tax?.ItemElementName == Models.SaftV4.ItemChoiceType1.TaxAmount ? l.Tax?.Item : 0,
                                    TaxPercentage = l.Tax?.ItemElementName == Models.SaftV4.ItemChoiceType1.TaxPercentage ? l.Tax?.Item : 0,
                                    TaxCode = l.Tax?.TaxCode,
                                    TaxCountryRegion = l.Tax?.TaxCountryRegion,
                                    TaxType = l.Tax?.TaxType.ToString()
                                },
                                TaxBase = l.TaxBase,
                                TaxExemptionCode = l.TaxExemptionCode,
                                TaxExemptionReason = l.TaxExemptionReason,
                                TaxPointDate = l.TaxPointDate,
                                UnitOfMeasure = l.UnitOfMeasure,
                                UnitPrice = l.UnitPrice
                            }).ToArray(),
                            Period = c.Period,
                            SourceID = c.SourceID,
                            SystemEntryDate = c.SystemEntryDate,
                            TransactionID = c.TransactionID
                        });
                    }
                }
                else if (saftValidator?.SaftFileV3?.SourceDocuments?.WorkingDocuments != null)
                {
                    var saft_documents = saftValidator.SaftFileV3.SourceDocuments.WorkingDocuments.WorkDocument;

                    foreach (var c in saft_documents)
                    {
                        documents.Add(new SourceDocumentsWorkingDocumentsWorkDocument
                        {
                            CustomerID = c.CustomerID,
                            DocumentStatus = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus
                            {
                                WorkStatus = c.DocumentStatus?.WorkStatus.ToString(),
                                WorkStatusDate = c.DocumentStatus?.WorkStatusDate ?? DateTime.MinValue,
                                Reason = c.DocumentStatus?.Reason,
                                SourceBilling = c.DocumentStatus?.SourceBilling.ToString(),
                                SourceID = c.DocumentStatus?.SourceID
                            },
                            DocumentTotals = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals
                            {
                                Currency = new Currency
                                {
                                    CurrencyAmount = c.DocumentTotals?.Currency?.CurrencyAmount ?? 0,
                                    CurrencyCode = c.DocumentTotals?.Currency?.CurrencyCode,
                                    ExchangeRate = c.DocumentTotals?.Currency?.ExchangeRate ?? 0
                                },
                                GrossTotal = c.DocumentTotals?.GrossTotal ?? 0,
                                NetTotal = c.DocumentTotals?.NetTotal ?? 0,
                                TaxPayable = c.DocumentTotals?.TaxPayable ?? 0
                            },
                            EACCode = c.EACCode,
                            Hash = c.Hash,
                            HashControl = c.HashControl,
                            WorkType = c.WorkType.ToString(),
                            DocumentNumber = c.DocumentNumber,
                            WorkDate = c.WorkDate,
                            Line = c.Line?.Select(l => new SourceDocumentsWorkingDocumentsWorkDocumentLine
                            {
                                DocumentNumber = l.DocNo,
                                CreditAmount = l.ItemElementName == Models.SaftV3.ItemChoiceType8.CreditAmount ? l.Item : 0,
                                DebitAmount = l.ItemElementName == Models.SaftV3.ItemChoiceType8.DebitAmount ? l.Item : 0,
                                Description = l.Description,
                                LineNumber = l.LineNumber,
                                OrderReferences = l.OrderReferences?.Select(o => new OrderReferences
                                {
                                    OrderDate = o.OrderDate,
                                    OriginatingON = o.OriginatingON
                                }).ToArray(),
                                ProductCode = l.ProductCode,
                                ProductDescription = l.ProductDescription,
                                Quantity = l.Quantity,
                                SettlementAmount = l.SettlementAmount,
                                Tax = new Tax
                                {
                                    TaxAmount = l.Tax?.ItemElementName == Models.SaftV3.ItemChoiceType1.TaxAmount ? l.Tax?.Item : 0,
                                    TaxPercentage = l.Tax?.ItemElementName == Models.SaftV3.ItemChoiceType1.TaxPercentage ? l.Tax?.Item : 0,
                                    TaxCode = l.Tax?.TaxCode,
                                    TaxCountryRegion = l.Tax?.TaxCountryRegion,
                                    TaxType = l.Tax?.TaxType.ToString()
                                },
                                TaxExemptionReason = l.TaxExemptionReason,
                                TaxPointDate = l.TaxPointDate,
                                UnitOfMeasure = l.UnitOfMeasure,
                                UnitPrice = l.UnitPrice
                            }).ToArray(),
                            Period = c.Period,
                            SourceID = c.SourceID,
                            SystemEntryDate = c.SystemEntryDate
                        });
                    }
                }

                return documents;
            }).ContinueWith(async c =>
            {
                var documents = await c;

                DocNumberOfEntries = documents.Count();
                DocTotalCredit = documents
                    .Where(i => i.DocumentStatus.WorkStatus != "A" && i.DocumentStatus.WorkStatus != "F")
                    .Sum(i => i.Line.Sum(l => l.CreditAmount ?? 0))
                    .ToString("c");

                DocTotalDebit = documents
                    .Where(i => i.DocumentStatus.WorkStatus != "A" && i.DocumentStatus.WorkStatus != "F")
                    .Sum(i => i.Line.Sum(l => l.DebitAmount ?? 0))
                    .ToString("c");

                if (saftValidator?.SaftFileV4?.SourceDocuments?.WorkingDocuments != null)
                {
                    NumberOfEntries = saftValidator.SaftFileV4.SourceDocuments.WorkingDocuments.NumberOfEntries;
                    TotalCredit = saftValidator.SaftFileV4.SourceDocuments.WorkingDocuments.TotalCredit.ToString("c");
                    TotalDebit = saftValidator.SaftFileV4.SourceDocuments.WorkingDocuments.TotalDebit.ToString("c");
                }
                else if (saftValidator?.SaftFileV3?.SourceDocuments?.WorkingDocuments != null)
                {
                    NumberOfEntries = saftValidator.SaftFileV3.SourceDocuments.WorkingDocuments.NumberOfEntries;
                    TotalCredit = saftValidator.SaftFileV3.SourceDocuments.WorkingDocuments.TotalCredit.ToString("c");
                    TotalDebit = saftValidator.SaftFileV3.SourceDocuments.WorkingDocuments.TotalDebit.ToString("c");
                }

                FiltroDataInicio = documents.Min(i => i.WorkDate);
                FiltroDataFim = documents.Max(i => i.WorkDate);

                CollectionView = new DataGridCollectionView(documents)
                {
                    Filter = o =>
                    {
                        if (string.IsNullOrWhiteSpace(Filter))
                            return true;

                        if (o is SourceDocumentsWorkingDocumentsWorkDocument document)
                        {
                            if (document.ATCUD != null && document.ATCUD.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.CustomerID != null && document.CustomerID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.DocumentStatus != null && document.DocumentStatus.WorkStatus != null && document.DocumentStatus.WorkStatus.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.DocumentStatus != null && document.DocumentStatus.Reason != null && document.DocumentStatus.Reason.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.EACCode != null && document.EACCode.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.DocumentNumber != null && document.DocumentNumber.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.WorkType != null && document.WorkType.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.Period != null && document.Period.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.SourceID != null && document.SourceID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (document.TransactionID != null && document.TransactionID.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }

                        return false;
                    }
                };
                CollectionViewDetails = new DataGridCollectionView(documents.SelectMany(d => d.Line))
                {
                    Filter = o =>
                    {
                        if (CurrentDocument == null)
                            return false;

                        if (o is SourceDocumentsWorkingDocumentsWorkDocumentLine line)
                        {
                            if (ShowAllLines == false && line.DocumentNumber.Equals(CurrentDocument.DocumentNumber, StringComparison.OrdinalIgnoreCase) == false)
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

                CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("WorkType"));

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
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void HandleDeactivation()
        {
        }

        SourceDocumentsWorkingDocumentsWorkDocument currentDocument;
        public SourceDocumentsWorkingDocumentsWorkDocument CurrentDocument
        {
            get => currentDocument;
            set
            {
                this.RaiseAndSetIfChanged(ref currentDocument, value);

                if (currentDocument != null && string.IsNullOrEmpty(FilterLines))
                {
                    DocGrossTotal = currentDocument.Line.Sum(c => c.UnitPrice * (1 + (c.Tax.TaxPercentage ?? 0) * 0.01m) * c.Quantity * Operation(currentDocument, c)).ToString("c");
                    DocNetTotal = currentDocument.Line.Sum(c => c.UnitPrice * c.Quantity * Operation(currentDocument, c)).ToString("c");
                    DocTaxPayable = currentDocument.Line.Sum(c => c.UnitPrice * (c.Tax.TaxPercentage ?? 0) * 0.01m * c.Quantity * Operation(currentDocument, c)).ToString("c");

                    GrossTotal = currentDocument.DocumentTotals.GrossTotal.ToString("c");
                    NetTotal = currentDocument.DocumentTotals.NetTotal.ToString("c");
                    TaxPayable = currentDocument.DocumentTotals.TaxPayable.ToString("c");

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
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsWorkingDocumentsWorkDocument> documents)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar Documentos Conferência",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documentos Conferência.xlsx",
                    ".xlsx");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    var workbook = new XSSFWorkbook();
                    var sheet = workbook.CreateSheet("Documentos");

                    // Create the style object
                    var detailSubtotalCellStyle = workbook.CreateCellStyle();
                    // Define a thin border for the top and bottom of the cell
                    detailSubtotalCellStyle.BorderTop = BorderStyle.Thin;
                    detailSubtotalCellStyle.BorderBottom = BorderStyle.Thin;

                    var row_header = sheet.CreateRow(0);
                    GenerateHeaderExcel(row_header, detailSubtotalCellStyle);

                    var rowIndex = 1;
                    foreach (var c in documents)
                    {
                        //create a new row
                        var row = sheet.CreateRow(rowIndex);

                        row.CreateCell(0).SetCellValue(c.ATCUD);
                        row.CreateCell(1).SetCellValue(c.WorkType);
                        row.CreateCell(2).SetCellValue(c.DocumentNumber);
                        row.CreateCell(3).SetCellValue(c.DocumentStatus.WorkStatus);
                        row.CreateCell(4).SetCellValue(c.WorkDate);
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
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsWorkingDocumentsWorkDocument> documents)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar Documentos Conferência",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documentos Conferência - Impostos.xlsx",
                    ".xlsx");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    var workbook = new XSSFWorkbook();
                    var sheet = workbook.CreateSheet("Impostos");

                    // Create the style object
                    var detailSubtotalCellStyle = workbook.CreateCellStyle();
                    // Define a thin border for the top and bottom of the cell
                    detailSubtotalCellStyle.BorderTop = BorderStyle.Thin;
                    detailSubtotalCellStyle.BorderBottom = BorderStyle.Thin;

                    var taxes_selling_group = documents
                        .SelectMany(i => i.Line)
                        .Where(c => c.CreditAmount > 0)
                        .GroupBy(l => new { l.DocumentNumber, l.Tax.TaxPercentage })
                        .Select(g => new { g.Key.DocumentNumber, Tax = g.Key.TaxPercentage, NetTotal = g.Sum(l => l.Quantity * l.UnitPrice) })
                        .OrderBy(g => g.DocumentNumber)
                        .ThenBy(g => g.Tax);

                    var rowIndex = 1;
                    foreach (var tax in taxes_selling_group)
                    {
                        //create a new row
                        var row = sheet.CreateRow(rowIndex);

                        row.CreateCell(0).SetCellValue(tax.DocumentNumber);
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
            if (CurrentDocument == null)
                return;

            var file = await dialogManager.SaveFileDialog(
                    "Guardar Documento Faturação",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Documento Faturação.xlsx",
                    "xlsx");

            if (string.IsNullOrWhiteSpace(file) == false)
            {
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Documento");

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

                var c = CurrentDocument;

                row.CreateCell(0).SetCellValue(c.ATCUD);
                row.CreateCell(1).SetCellValue(c.WorkType);
                row.CreateCell(2).SetCellValue(c.DocumentNumber);
                row.CreateCell(3).SetCellValue(c.DocumentStatus.WorkStatus);
                row.CreateCell(4).SetCellValue(c.WorkDate);
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

                for (int i = 0; i <= 16; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                using FileStream fs = new FileStream(file, FileMode.Create);
                workbook.Write(fs);
                fs.Close();
            }
        }
        private async Task OnTestHash()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<SourceDocumentsMovementOfGoodsStockMovement> documents)
            {
                //parse the document number property to get the current document number
                string[] docNo = CurrentDocument.DocumentNumber.Split('/');
                if (docNo != null && docNo.Length == 2)
                {
                    int.TryParse(docNo[1], out int num);
                    num -= 1;

                    if (num > 0)
                    {
                        //found a valid number, try to find the previous document
                        var previousDocument = documents
                            .Where(i => i.DocumentNumber.IndexOf(string.Format("{0}/{1}", docNo[0], num), StringComparison.OrdinalIgnoreCase) == 0)
                            .FirstOrDefault();

                        //encontramos um documento, vamos obter a hash
                        if (previousDocument != null)
                        {
                            var view = new DialogHashTest();
                            var vm = new DialogHashTestViewModel();
                            vm.Init();
                            vm.InitFromWorking(CurrentDocument, previousDocument.Hash);

                            view.DataContext = vm;

                            await dialogManager.ShowChildDialogAsync(view);
                        }
                    }
                }
            }
        }

        private int Operation(SourceDocumentsWorkingDocumentsWorkDocument i, SourceDocumentsWorkingDocumentsWorkDocumentLine l)
        {
            if (i.WorkType == "CM" || i.WorkType == "FC" || i.WorkType == "FO" || i.WorkType == "NE" || i.WorkType == "OU" || i.WorkType == "OR" || i.WorkType == "PF" || i.WorkType == "DC" || i.WorkType == "RP" || i.WorkType == "RE" || i.WorkType == "CS" || i.WorkType == "LD" || i.WorkType == "RA")
                return l.CreditAmount > 0 ? 1 : -1;
            else if (i.WorkType == "CC")
                return l.DebitAmount > 0 ? 1 : -1;

            return 1;
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
            cell.SetCellValue("Código produto");

            cell = row.CreateCell(3);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Descrição produto");

            cell = row.CreateCell(4);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Quantidade");

            cell = row.CreateCell(5);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Preço");

            cell = row.CreateCell(6);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Crédito");

            cell = row.CreateCell(7);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Débito");

            cell = row.CreateCell(8);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Desconto");

            cell = row.CreateCell(9);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Imposto");

            cell = row.CreateCell(10);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Isenção");

            cell = row.CreateCell(11);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Cód. Isenção");

            cell = row.CreateCell(12);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Referência");

            cell = row.CreateCell(13);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Razão");

            cell = row.CreateCell(14);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Unidade medida");

            cell = row.CreateCell(15);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Data");

            cell = row.CreateCell(16);
            cell.CellStyle = cellStyle;
            cell.SetCellValue("Descrição");
        }
    }
}
