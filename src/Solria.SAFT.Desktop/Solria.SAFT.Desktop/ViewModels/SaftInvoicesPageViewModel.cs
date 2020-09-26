using Avalonia.Collections;
using FastReport;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Saft;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Desktop.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

            Task.Run(() =>
            {
                header = SaftHeaderPageViewModel.GetHeader(saftValidator);

                var invoices = new List<SourceDocumentsSalesInvoicesInvoice>();
                if (saftValidator?.SaftFileV4?.SourceDocuments?.SalesInvoices != null)
                {
                    var saft_invoices = saftValidator.SaftFileV4.SourceDocuments?.SalesInvoices.Invoice;

                    foreach (var c in saft_invoices)
                    {
                        var customer = saftValidator.SaftFileV4.MasterFiles?.Customer?.Where(t => t.CustomerID == c.CustomerID).FirstOrDefault();

                        invoices.Add(new SourceDocumentsSalesInvoicesInvoice
                        {
                            ATCUD = c.ATCUD,
                            CustomerID = c.CustomerID,
                            Customer = new Customer
                            {
                                AccountID = customer?.AccountID,
                                BillingAddress = new AddressStructure
                                {
                                    AddressDetail = customer?.BillingAddress?.AddressDetail,
                                    BuildingNumber = customer?.BillingAddress?.BuildingNumber,
                                    City = customer?.BillingAddress?.City,
                                    Country = customer?.BillingAddress?.Country,
                                    PostalCode = customer?.BillingAddress?.PostalCode,
                                    Region = customer?.BillingAddress?.Region,
                                    StreetName = customer?.BillingAddress?.StreetName
                                },
                                CompanyName = customer?.CompanyName,
                                Contact = customer?.Contact,
                                CustomerID = customer?.CustomerID,
                                CustomerTaxID = customer?.CustomerTaxID,
                                Email = customer?.Email,
                                Fax = customer?.Fax,
                                SelfBillingIndicator = customer?.SelfBillingIndicator,
                                ShipToAddress = customer?.ShipToAddress?.Select(b => new AddressStructure
                                {
                                    AddressDetail = b.AddressDetail,
                                    BuildingNumber = b.BuildingNumber,
                                    City = b.City,
                                    Country = b.Country,
                                    PostalCode = b.PostalCode,
                                    Region = b.Region,
                                    StreetName = b.StreetName
                                }).ToArray(),
                                Telephone = customer?.Telephone,
                                Website = customer?.Website
                            },
                            DocumentStatus = new SourceDocumentsSalesInvoicesInvoiceDocumentStatus
                            {
                                InvoiceStatus = c.DocumentStatus?.InvoiceStatus.ToString(),
                                InvoiceStatusDate = c.DocumentStatus?.InvoiceStatusDate ?? DateTime.MinValue,
                                Reason = c.DocumentStatus?.Reason,
                                SourceBilling = c.DocumentStatus?.SourceBilling.ToString(),
                                SourceID = c.DocumentStatus?.SourceID
                            },
                            DocumentTotals = new SourceDocumentsSalesInvoicesInvoiceDocumentTotals
                            {
                                Currency = new Currency
                                {
                                    CurrencyAmount = c.DocumentTotals?.Currency?.CurrencyAmount ?? 0,
                                    CurrencyCode = c.DocumentTotals?.Currency?.CurrencyCode,
                                    ExchangeRate = c.DocumentTotals?.Currency?.ExchangeRate ?? 0
                                },
                                GrossTotal = c.DocumentTotals?.GrossTotal ?? 0,
                                NetTotal = c.DocumentTotals?.NetTotal ?? 0,
                                Payment = c.DocumentTotals?.Payment?.Select(p => new PaymentMethod
                                {
                                    PaymentAmount = p.PaymentAmount,
                                    PaymentDate = p.PaymentDate,
                                    PaymentMechanism = p.PaymentMechanism.ToString()
                                }).ToArray(),
                                Settlement = c.DocumentTotals?.Settlement?.Select(s => new Settlement
                                {
                                    PaymentTerms = s.PaymentTerms,
                                    SettlementAmount = s.SettlementAmount,
                                    SettlementDate = s.SettlementDate,
                                    SettlementDiscount = s.SettlementDiscount
                                }).ToArray(),
                                TaxPayable = c.DocumentTotals?.TaxPayable ?? 0
                            },
                            EACCode = c.EACCode,
                            Hash = c.Hash,
                            HashControl = c.HashControl,
                            InvoiceDate = c.InvoiceDate,
                            InvoiceNo = c.InvoiceNo,
                            InvoiceType = c.InvoiceType.ToString(),
                            Line = c.Line?.Select(l => new SourceDocumentsSalesInvoicesInvoiceLine
                            {
                                CreditAmount = l.ItemElementName == Models.SaftV4.ItemChoiceType4.CreditAmount ? l.Item : 0,
                                DebitAmount = l.ItemElementName == Models.SaftV4.ItemChoiceType4.DebitAmount ? l.Item : 0,
                                CustomsInformation = new CustomsInformation
                                {
                                    ARCNo = l.CustomsInformation?.ARCNo,
                                    IECAmount = l.CustomsInformation?.IECAmount ?? 0
                                },
                                Description = l.Description,
                                InvoiceNo = l.InvoiceNo,
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
                                UnitPrice = l.UnitPrice,
                                TooltipLineNumber = l.TooltipLineNumber,
                                TooltipOrderDate = l.TooltipOrderDate,
                                TooltipOrderReferences = l.TooltipOrderReferences,
                                TooltipOriginatingON = l.TooltipOriginatingON,
                                TooltipProductCode = l.TooltipProductCode,
                                TooltipProductDescription = l.TooltipProductDescription,
                                TooltipQuantity = l.TooltipQuantity,
                                TooltipTaxPointDate = l.TooltipTaxPointDate,
                                TooltipUnitOfMeasure = l.TooltipUnitOfMeasure,
                                TooltipUnitPrice = l.TooltipUnitPrice
                            }).ToArray(),
                            MovementEndTime = c.MovementEndTime,
                            MovementStartTime = c.MovementStartTime,
                            Period = c.Period,
                            ShipFrom = new ShippingPointStructure
                            {
                                Address = new AddressStructure
                                {
                                    AddressDetail = c.ShipFrom?.Address?.AddressDetail,
                                    BuildingNumber = c.ShipFrom?.Address?.BuildingNumber,
                                    City = c.ShipFrom?.Address?.City,
                                    Country = c.ShipFrom?.Address?.Country,
                                    PostalCode = c.ShipFrom?.Address?.PostalCode,
                                    Region = c.ShipFrom?.Address?.Region,
                                    StreetName = c.ShipFrom?.Address?.StreetName
                                },
                                DeliveryDate = c.ShipFrom?.DeliveryDate,
                                DeliveryID = c.ShipFrom?.DeliveryID,
                                LocationID = c.ShipFrom?.LocationID,
                                WarehouseID = c.ShipFrom?.WarehouseID
                            },
                            ShipTo = new ShippingPointStructure
                            {
                                Address = new AddressStructure
                                {
                                    AddressDetail = c.ShipTo?.Address?.AddressDetail,
                                    BuildingNumber = c.ShipTo?.Address?.BuildingNumber,
                                    City = c.ShipTo?.Address?.City,
                                    Country = c.ShipTo?.Address?.Country,
                                    PostalCode = c.ShipTo?.Address?.PostalCode,
                                    Region = c.ShipTo?.Address?.Region,
                                    StreetName = c.ShipTo?.Address?.StreetName
                                },
                                DeliveryDate = c.ShipTo?.DeliveryDate,
                                DeliveryID = c.ShipTo?.DeliveryID,
                                LocationID = c.ShipTo?.LocationID,
                                WarehouseID = c.ShipTo?.WarehouseID
                            },
                            SourceID = c.SourceID,
                            SpecialRegimes = new SpecialRegimes
                            {
                                CashVATSchemeIndicator = c.SpecialRegimes?.CashVATSchemeIndicator,
                                SelfBillingIndicator = c.SpecialRegimes?.SelfBillingIndicator,
                                ThirdPartiesBillingIndicator = c.SpecialRegimes?.ThirdPartiesBillingIndicator
                            },
                            SystemEntryDate = c.SystemEntryDate,
                            TransactionID = c.TransactionID,
                            WithholdingTax = c.WithholdingTax?.Select(w => new WithholdingTax
                            {
                                WithholdingTaxAmount = w.WithholdingTaxAmount,
                                WithholdingTaxDescription = w.WithholdingTaxDescription,
                                WithholdingTaxType = w.WithholdingTaxType.ToString()
                            }).ToArray(),
                            TooltipATDocCodeID = c.TooltipATDocCodeID,
                            TooltipCashVATSchemeIndicator = c.TooltipCashVATSchemeIndicator,
                            TooltipCreditAmount = c.TooltipCreditAmount,
                            TooltipCreditNote = c.TooltipCreditNote,
                            TooltipCurrency = c.TooltipCurrency,
                            TooltipCurrencyAmount = c.TooltipCurrencyAmount,
                            TooltipCustomerID = c.TooltipCustomerID,
                            TooltipDebitAmount = c.TooltipDebitAmount,
                            TooltipDescription = c.TooltipDescription,
                            TooltipDocumentStatus = c.TooltipDocumentStatus,
                            TooltipDocumentTotals = c.TooltipDocumentTotals,
                            TooltipExchangeRate = c.TooltipExchangeRate,
                            TooltipGeneratedDocumentUserSourceID = c.TooltipGeneratedDocumentUserSourceID,
                            TooltipGrossTotal = c.TooltipGrossTotal,
                            TooltipHash = c.TooltipHash,
                            TooltipHashControl = c.TooltipHashControl,
                            TooltipInvoiceDate = c.TooltipInvoiceDate,
                            TooltipInvoiceNo = c.TooltipInvoiceNo,
                            TooltipInvoiceStatus = c.TooltipInvoiceStatus,
                            TooltipInvoiceStatusDate = c.TooltipInvoiceStatusDate,
                            TooltipInvoiceType = c.TooltipInvoiceType,
                            TooltipLineReason = c.TooltipLineReason,
                            TooltipLineSettlementAmount = c.TooltipLineSettlementAmount,
                            TooltipMovementEndTime = c.TooltipMovementEndTime,
                            TooltipMovementStartTime = c.TooltipMovementStartTime,
                            TooltipNetTotal = c.TooltipNetTotal,
                            TooltipPaymentMechanism = c.TooltipPaymentMechanism,
                            TooltipPaymentTerms = c.TooltipPaymentTerms,
                            TooltipPeriod = c.TooltipPeriod,
                            TooltipReason = c.TooltipReason,
                            TooltipReference = c.TooltipReference,
                            TooltipReferences = c.TooltipReferences,
                            TooltipResponsableUserSourceID = c.TooltipResponsableUserSourceID,
                            TooltipSelfBillingIndicator = c.TooltipSelfBillingIndicator,
                            TooltipSettlement = c.TooltipSettlement,
                            TooltipSettlementAmount = c.TooltipSettlementAmount,
                            TooltipSettlementDate = c.TooltipSettlementDate,
                            TooltipSettlementDiscount = c.TooltipSettlementDiscount,
                            TooltipShipFrom = c.TooltipShipFrom,
                            TooltipShipFromAddress = c.TooltipShipFromAddress,
                            TooltipShipFromAddressDetail = c.TooltipShipFromAddressDetail,
                            TooltipShipFromBuildingNumber = c.TooltipShipFromBuildingNumber,
                            TooltipShipFromCity = c.TooltipShipFromCity,
                            TooltipShipFromCountry = c.TooltipShipFromCountry,
                            TooltipShipFromDeliveryDate = c.TooltipShipFromDeliveryDate,
                            TooltipShipFromDeliveryID = c.TooltipShipFromDeliveryID,
                            TooltipShipFromLocationID = c.TooltipShipFromLocationID,
                            TooltipShipFromPostalCode = c.TooltipShipFromPostalCode,
                            TooltipShipFromRegion = c.TooltipShipFromRegion,
                            TooltipShipFromStreetName = c.TooltipShipFromStreetName,
                            TooltipShipFromWarehouseID = c.TooltipShipFromWarehouseID,
                            TooltipShipTo = c.TooltipShipTo,
                            TooltipShipToAddress = c.TooltipShipToAddress,
                            TooltipShipToAddressDetail = c.TooltipShipToAddressDetail,
                            TooltipShipToBuildingNumber = c.TooltipShipToBuildingNumber,
                            TooltipShipToCity = c.TooltipShipToCity,
                            TooltipShipToCountry = c.TooltipShipToCountry,
                            TooltipShipToDeliveryDate = c.TooltipShipToDeliveryDate,
                            TooltipShipToDeliveryID = c.TooltipShipToDeliveryID,
                            TooltipShipToLocationID = c.TooltipShipToLocationID,
                            TooltipShipToPostalCode = c.TooltipShipToPostalCode,
                            TooltipShipToRegion = c.TooltipShipToRegion,
                            TooltipShipToStreetName = c.TooltipShipToStreetName,
                            TooltipShipToWarehouseID = c.TooltipShipToWarehouseID,
                            TooltipSourceBilling = c.TooltipSourceBilling,
                            TooltipSystemEntryDate = c.TooltipSystemEntryDate,
                            TooltipTax = c.TooltipTax,
                            TooltipTaxAmount = c.TooltipTaxAmount,
                            TooltipTaxCode = c.TooltipTaxCode,
                            TooltipTaxCountryRegion = c.TooltipTaxCountryRegion,
                            TooltipTaxExemptionReason = c.TooltipTaxExemptionReason,
                            TooltipTaxPayable = c.TooltipTaxPayable,
                            TooltipTaxPercentage = c.TooltipTaxPercentage,
                            TooltipTaxType = c.TooltipTaxType,
                            TooltipThirdPartiesBillingIndicator = c.TooltipThirdPartiesBillingIndicator,
                            TooltipTransactionID = c.TooltipTransactionID,
                            TooltipWithholdingTax = c.TooltipWithholdingTax,
                            TooltipWithholdingTaxAmount = c.TooltipWithholdingTaxAmount,
                            TooltipWithholdingTaxDescription = c.TooltipWithholdingTaxDescription,
                            TooltipWithholdingTaxType = c.TooltipWithholdingTaxType
                        });
                    }
                }
                else if (saftValidator?.SaftFileV3?.SourceDocuments?.SalesInvoices != null)
                {
                    var saft_invoices = saftValidator.SaftFileV3.SourceDocuments.SalesInvoices.Invoice;

                    foreach (var c in saft_invoices)
                    {
                        var customer = saftValidator.SaftFileV3.MasterFiles?.Customer?.Where(c => c.CustomerID == CurrentInvoice.CustomerID).FirstOrDefault();

                        invoices.Add(new SourceDocumentsSalesInvoicesInvoice
                        {
                            CustomerID = c.CustomerID,
                            Customer = new Customer
                            {
                                AccountID = customer?.AccountID,
                                BillingAddress = new AddressStructure
                                {
                                    AddressDetail = customer?.BillingAddress?.AddressDetail,
                                    BuildingNumber = customer?.BillingAddress?.BuildingNumber,
                                    City = customer?.BillingAddress?.City,
                                    Country = customer?.BillingAddress?.Country,
                                    PostalCode = customer?.BillingAddress?.PostalCode,
                                    Region = customer?.BillingAddress?.Region,
                                    StreetName = customer?.BillingAddress?.StreetName
                                },
                                CompanyName = customer?.CompanyName,
                                Contact = customer?.Contact,
                                CustomerID = customer?.CustomerID,
                                CustomerTaxID = customer?.CustomerTaxID,
                                Email = customer?.Email,
                                Fax = customer?.Fax,
                                SelfBillingIndicator = customer?.SelfBillingIndicator,
                                ShipToAddress = customer?.ShipToAddress?.Select(b => new AddressStructure
                                {
                                    AddressDetail = b.AddressDetail,
                                    BuildingNumber = b.BuildingNumber,
                                    City = b.City,
                                    Country = b.Country,
                                    PostalCode = b.PostalCode,
                                    Region = b.Region,
                                    StreetName = b.StreetName
                                }).ToArray(),
                                Telephone = customer?.Telephone,
                                Website = customer?.Website
                            },
                            DocumentStatus = new SourceDocumentsSalesInvoicesInvoiceDocumentStatus
                            {
                                InvoiceStatus = c.DocumentStatus?.InvoiceStatus.ToString(),
                                InvoiceStatusDate = c.DocumentStatus?.InvoiceStatusDate ?? DateTime.MinValue,
                                Reason = c.DocumentStatus?.Reason,
                                SourceBilling = c.DocumentStatus?.SourceBilling.ToString(),
                                SourceID = c.DocumentStatus?.SourceID
                            },
                            DocumentTotals = new SourceDocumentsSalesInvoicesInvoiceDocumentTotals
                            {
                                Currency = new Currency
                                {
                                    CurrencyAmount = c.DocumentTotals?.Currency?.CurrencyAmount ?? 0,
                                    CurrencyCode = c.DocumentTotals?.Currency?.CurrencyCode,
                                    ExchangeRate = c.DocumentTotals?.Currency?.ExchangeRate ?? 0
                                },
                                GrossTotal = c.DocumentTotals?.GrossTotal ?? 0,
                                NetTotal = c.DocumentTotals?.NetTotal ?? 0,
                                Payment = c.DocumentTotals?.Payment?.Select(p => new PaymentMethod
                                {
                                    PaymentAmount = p.PaymentAmount,
                                    PaymentDate = p.PaymentDate,
                                    PaymentMechanism = p.PaymentMechanism.ToString()
                                }).ToArray(),
                                Settlement = c.DocumentTotals?.Settlement?.Select(s => new Settlement
                                {
                                    PaymentTerms = s.PaymentTerms,
                                    SettlementAmount = s.SettlementAmount,
                                    SettlementDate = s.SettlementDate,
                                    SettlementDiscount = s.SettlementDiscount
                                }).ToArray(),
                                TaxPayable = c.DocumentTotals?.TaxPayable ?? 0
                            },
                            EACCode = c.EACCode,
                            Hash = c.Hash,
                            HashControl = c.HashControl,
                            InvoiceDate = c.InvoiceDate,
                            InvoiceNo = c.InvoiceNo,
                            InvoiceType = c.InvoiceType.ToString(),
                            Line = c.Line?.Select(l => new SourceDocumentsSalesInvoicesInvoiceLine
                            {
                                CreditAmount = l.ItemElementName == Models.SaftV3.ItemChoiceType5.CreditAmount ? l.Item : 0,
                                DebitAmount = l.ItemElementName == Models.SaftV3.ItemChoiceType5.DebitAmount ? l.Item : 0,
                                Description = l.Description,
                                InvoiceNo = l.InvoiceNo,
                                LineNumber = l.LineNumber,
                                OrderReferences = l.OrderReferences?.Select(o => new OrderReferences
                                {
                                    OrderDate = o.OrderDate,
                                    OriginatingON = o.OriginatingON
                                }).ToArray(),
                                ProductCode = l.ProductCode,
                                ProductDescription = l.ProductDescription,
                                Quantity = l.Quantity,
                                References = l.References?.Select(r => new References
                                {
                                    Reason = r.Reason,
                                    Reference = r.Reference
                                }).ToArray(),
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
                                UnitPrice = l.UnitPrice,
                                TooltipLineNumber = l.TooltipLineNumber,
                                TooltipOrderDate = l.TooltipOrderDate,
                                TooltipOrderReferences = l.TooltipOrderReferences,
                                TooltipOriginatingON = l.TooltipOriginatingON,
                                TooltipProductCode = l.TooltipProductCode,
                                TooltipProductDescription = l.TooltipProductDescription,
                                TooltipQuantity = l.TooltipQuantity,
                                TooltipTaxPointDate = l.TooltipTaxPointDate,
                                TooltipUnitOfMeasure = l.TooltipUnitOfMeasure,
                                TooltipUnitPrice = l.TooltipUnitPrice
                            }).ToArray(),
                            MovementEndTime = c.MovementEndTime,
                            MovementStartTime = c.MovementStartTime,
                            Period = c.Period,
                            ShipFrom = new ShippingPointStructure
                            {
                                Address = new AddressStructure
                                {
                                    AddressDetail = c.ShipFrom?.Address?.AddressDetail,
                                    BuildingNumber = c.ShipFrom?.Address?.BuildingNumber,
                                    City = c.ShipFrom?.Address?.City,
                                    Country = c.ShipFrom?.Address?.Country,
                                    PostalCode = c.ShipFrom?.Address?.PostalCode,
                                    Region = c.ShipFrom?.Address?.Region,
                                    StreetName = c.ShipFrom?.Address?.StreetName
                                },
                                DeliveryDate = c.ShipFrom?.DeliveryDate,
                                DeliveryID = c.ShipFrom?.DeliveryID,
                                LocationID = c.ShipFrom?.LocationID,
                                WarehouseID = c.ShipFrom?.WarehouseID
                            },
                            ShipTo = new ShippingPointStructure
                            {
                                Address = new AddressStructure
                                {
                                    AddressDetail = c.ShipTo?.Address?.AddressDetail,
                                    BuildingNumber = c.ShipTo?.Address?.BuildingNumber,
                                    City = c.ShipTo?.Address?.City,
                                    Country = c.ShipTo?.Address?.Country,
                                    PostalCode = c.ShipTo?.Address?.PostalCode,
                                    Region = c.ShipTo?.Address?.Region,
                                    StreetName = c.ShipTo?.Address?.StreetName
                                },
                                DeliveryDate = c.ShipTo?.DeliveryDate,
                                DeliveryID = c.ShipTo?.DeliveryID,
                                LocationID = c.ShipTo?.LocationID,
                                WarehouseID = c.ShipTo?.WarehouseID
                            },
                            SourceID = c.SourceID,
                            SpecialRegimes = new SpecialRegimes
                            {
                                CashVATSchemeIndicator = c.SpecialRegimes?.CashVATSchemeIndicator,
                                SelfBillingIndicator = c.SpecialRegimes?.SelfBillingIndicator,
                                ThirdPartiesBillingIndicator = c.SpecialRegimes?.ThirdPartiesBillingIndicator
                            },
                            SystemEntryDate = c.SystemEntryDate,
                            TransactionID = c.TransactionID,
                            WithholdingTax = c.WithholdingTax?.Select(w => new WithholdingTax
                            {
                                WithholdingTaxAmount = w.WithholdingTaxAmount,
                                WithholdingTaxDescription = w.WithholdingTaxDescription,
                                WithholdingTaxType = w.WithholdingTaxType.ToString()
                            }).ToArray(),
                            TooltipATDocCodeID = c.TooltipATDocCodeID,
                            TooltipCashVATSchemeIndicator = c.TooltipCashVATSchemeIndicator,
                            TooltipCreditAmount = c.TooltipCreditAmount,
                            TooltipCreditNote = c.TooltipCreditNote,
                            TooltipCurrency = c.TooltipCurrency,
                            TooltipCurrencyAmount = c.TooltipCurrencyAmount,
                            TooltipCustomerID = c.TooltipCustomerID,
                            TooltipDebitAmount = c.TooltipDebitAmount,
                            TooltipDescription = c.TooltipDescription,
                            TooltipDocumentStatus = c.TooltipDocumentStatus,
                            TooltipDocumentTotals = c.TooltipDocumentTotals,
                            TooltipExchangeRate = c.TooltipExchangeRate,
                            TooltipGeneratedDocumentUserSourceID = c.TooltipGeneratedDocumentUserSourceID,
                            TooltipGrossTotal = c.TooltipGrossTotal,
                            TooltipHash = c.TooltipHash,
                            TooltipHashControl = c.TooltipHashControl,
                            TooltipInvoiceDate = c.TooltipInvoiceDate,
                            TooltipInvoiceNo = c.TooltipInvoiceNo,
                            TooltipInvoiceStatus = c.TooltipInvoiceStatus,
                            TooltipInvoiceStatusDate = c.TooltipInvoiceStatusDate,
                            TooltipInvoiceType = c.TooltipInvoiceType,
                            TooltipLineReason = c.TooltipLineReason,
                            TooltipLineSettlementAmount = c.TooltipLineSettlementAmount,
                            TooltipMovementEndTime = c.TooltipMovementEndTime,
                            TooltipMovementStartTime = c.TooltipMovementStartTime,
                            TooltipNetTotal = c.TooltipNetTotal,
                            TooltipPaymentMechanism = c.TooltipPaymentMechanism,
                            TooltipPaymentTerms = c.TooltipPaymentTerms,
                            TooltipPeriod = c.TooltipPeriod,
                            TooltipReason = c.TooltipReason,
                            TooltipReference = c.TooltipReference,
                            TooltipReferences = c.TooltipReferences,
                            TooltipResponsableUserSourceID = c.TooltipResponsableUserSourceID,
                            TooltipSelfBillingIndicator = c.TooltipSelfBillingIndicator,
                            TooltipSettlement = c.TooltipSettlement,
                            TooltipSettlementAmount = c.TooltipSettlementAmount,
                            TooltipSettlementDate = c.TooltipSettlementDate,
                            TooltipSettlementDiscount = c.TooltipSettlementDiscount,
                            TooltipShipFrom = c.TooltipShipFrom,
                            TooltipShipFromAddress = c.TooltipShipFromAddress,
                            TooltipShipFromAddressDetail = c.TooltipShipFromAddressDetail,
                            TooltipShipFromBuildingNumber = c.TooltipShipFromBuildingNumber,
                            TooltipShipFromCity = c.TooltipShipFromCity,
                            TooltipShipFromCountry = c.TooltipShipFromCountry,
                            TooltipShipFromDeliveryDate = c.TooltipShipFromDeliveryDate,
                            TooltipShipFromDeliveryID = c.TooltipShipFromDeliveryID,
                            TooltipShipFromLocationID = c.TooltipShipFromLocationID,
                            TooltipShipFromPostalCode = c.TooltipShipFromPostalCode,
                            TooltipShipFromRegion = c.TooltipShipFromRegion,
                            TooltipShipFromStreetName = c.TooltipShipFromStreetName,
                            TooltipShipFromWarehouseID = c.TooltipShipFromWarehouseID,
                            TooltipShipTo = c.TooltipShipTo,
                            TooltipShipToAddress = c.TooltipShipToAddress,
                            TooltipShipToAddressDetail = c.TooltipShipToAddressDetail,
                            TooltipShipToBuildingNumber = c.TooltipShipToBuildingNumber,
                            TooltipShipToCity = c.TooltipShipToCity,
                            TooltipShipToCountry = c.TooltipShipToCountry,
                            TooltipShipToDeliveryDate = c.TooltipShipToDeliveryDate,
                            TooltipShipToDeliveryID = c.TooltipShipToDeliveryID,
                            TooltipShipToLocationID = c.TooltipShipToLocationID,
                            TooltipShipToPostalCode = c.TooltipShipToPostalCode,
                            TooltipShipToRegion = c.TooltipShipToRegion,
                            TooltipShipToStreetName = c.TooltipShipToStreetName,
                            TooltipShipToWarehouseID = c.TooltipShipToWarehouseID,
                            TooltipSourceBilling = c.TooltipSourceBilling,
                            TooltipSystemEntryDate = c.TooltipSystemEntryDate,
                            TooltipTax = c.TooltipTax,
                            TooltipTaxAmount = c.TooltipTaxAmount,
                            TooltipTaxCode = c.TooltipTaxCode,
                            TooltipTaxCountryRegion = c.TooltipTaxCountryRegion,
                            TooltipTaxExemptionReason = c.TooltipTaxExemptionReason,
                            TooltipTaxPayable = c.TooltipTaxPayable,
                            TooltipTaxPercentage = c.TooltipTaxPercentage,
                            TooltipTaxType = c.TooltipTaxType,
                            TooltipThirdPartiesBillingIndicator = c.TooltipThirdPartiesBillingIndicator,
                            TooltipTransactionID = c.TooltipTransactionID,
                            TooltipWithholdingTax = c.TooltipWithholdingTax,
                            TooltipWithholdingTaxAmount = c.TooltipWithholdingTaxAmount,
                            TooltipWithholdingTaxDescription = c.TooltipWithholdingTaxDescription,
                            TooltipWithholdingTaxType = c.TooltipWithholdingTaxType
                        });
                    }
                }

                return invoices;
            }).ContinueWith(async c =>
            {
                var invoices = await c;

                DocNumberOfEntries = invoices.Count();
                DocTotalCredit = invoices
                    .Where(i => i.DocumentStatus.InvoiceStatus != "A" && i.DocumentStatus.InvoiceStatus != "F")
                    .Sum(i => i.Line.Sum(l => l.CreditAmount))
                    .ToString("c");

                DocTotalDebit = invoices
                    .Where(i => i.DocumentStatus.InvoiceStatus != "A" && i.DocumentStatus.InvoiceStatus != "F")
                    .Sum(i => i.Line.Sum(l => l.DebitAmount))
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
                            if (invoice.DocumentStatus != null && invoice.DocumentStatus.InvoiceStatus != null && invoice.DocumentStatus.InvoiceStatus.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (invoice.DocumentStatus != null && invoice.DocumentStatus.Reason != null && invoice.DocumentStatus.Reason.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (invoice.EACCode != null && invoice.EACCode.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (invoice.InvoiceNo != null && invoice.InvoiceNo.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                            if (invoice.InvoiceType != null && invoice.InvoiceType.Contains(Filter, StringComparison.OrdinalIgnoreCase))
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
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void HandleDeactivation()
        {
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
            // create report instance
            Report report = new Report();

            // load the existing report
            report.Load(Path.Combine(Environment.CurrentDirectory, "Reports", "BillingDocument_A4.frx"));

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
                    .Where(t => t.TaxType == l.Tax.TaxType && t.TaxCountryRegion == l.Tax.TaxCountryRegion && t.TaxPercentage == l.Tax.TaxPercentage)
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
                        TaxType = l.Tax.TaxType
                    };
                    taxes.Add(existing);
                }
            }
            var document = new Models.Reporting.Document { Taxes = taxes.ToArray(), Lines = lines };

            var qrCode = GetATQrCode(header, CurrentInvoice.Customer, CurrentInvoice);

            ReportRegisterData(report, document);
            ReportRegisterParameters(report, "Original", qrCode, CurrentInvoice.Customer);

            // prepare the report
            report.Prepare();
            report.Load(Path.Combine(Environment.CurrentDirectory, "Reports", "BillingDocument_A4.frx"));
            ReportRegisterData(report, document);
            ReportRegisterParameters(report, "Duplicado", qrCode, CurrentInvoice.Customer);
            report.Prepare(true);

            //save prepared report
            string preparedReport = Path.Combine(Path.GetTempPath(), $"{DateTime.Now:yyyyMMddHHmmss}_report.fpx");
            report.SavePrepared(preparedReport);

            //var pdfexport = new FastReport.Export.PdfSimple.PDFSimpleExport();
            //report.Export(pdfexport, Path.Combine(Path.GetTempPath(), $"{DateTime.Now:yyyyMMddHHmmss}_report.pdf"));

            reportService.View(preparedReport);
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
                    num -= 1;

                    if (num > 0)
                    {
                        //found a valid number, try to find the previous document
                        var previousDocument = invoices
                            .Where(i => i.InvoiceNo.IndexOf(string.Format("{0}/{1}", invoiceNo[0], num), StringComparison.OrdinalIgnoreCase) == 0)
                            .FirstOrDefault();

                        //encontramos um documento, vamos obter a hash
                        if (previousDocument != null)
                        {
                            var view = new DialogHashTest();
                            var vm = new DialogHashTestViewModel();
                            vm.Init();
                            vm.InitFromInvoice(CurrentInvoice, previousDocument.Hash);

                            view.DataContext = vm;

                            await dialogManager.ShowChildDialogAsync(view);
                        }
                    }
                }
            }
        }

        private int Operation(SourceDocumentsSalesInvoicesInvoice i, SourceDocumentsSalesInvoicesInvoiceLine l)
        {
            if (i.InvoiceType == "FT" || i.InvoiceType == "VD" || i.InvoiceType == "ND" || i.InvoiceType == "FR" || i.InvoiceType == "FS" || i.InvoiceType == "TV" || i.InvoiceType == "AA")
                return l.CreditAmount > 0 ? 1 : -1;
            else if (i.InvoiceType == "NC" || i.InvoiceType == "TD" || i.InvoiceType == "DA" || i.InvoiceType == "RE")
                return l.DebitAmount > 0 ? 1 : -1;

            return 1;
        }

        private void DocHeader(ClosedXML.Excel.IXLWorksheet sheet, int row)
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

        private void LineHeader(ClosedXML.Excel.IXLWorksheet sheet, int row)
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

        private void ReportRegisterData(Report report, Models.Reporting.Document document)
        {
            report.RegisterData(new Models.Reporting.Document[] { document }, "Document", 2);
        }
        private void ReportRegisterParameters(Report report, string copyNumber, string qrCode, Customer customer)
        {
            report.SetParameterValue("CopyNumber", copyNumber);
            report.SetParameterValue("CompanyName", saftValidator?.SaftFileV4?.Header?.CompanyName);
            report.SetParameterValue("BusinessName", saftValidator?.SaftFileV4?.Header?.BusinessName);
            report.SetParameterValue("TaxRegistrationNumber", saftValidator?.SaftFileV4?.Header?.TaxRegistrationNumber);
            report.SetParameterValue("Address", saftValidator?.SaftFileV4?.Header?.CompanyAddress?.AddressDetail);
            report.SetParameterValue("DocNo", CurrentInvoice.InvoiceNo);
            report.SetParameterValue("ATCUD", CurrentInvoice.ATCUD);
            report.SetParameterValue("Status", CurrentInvoice.DocumentStatus.InvoiceStatus);
            report.SetParameterValue("Date", CurrentInvoice.InvoiceDate);
            report.SetParameterValue("CustomerTaxID", customer?.CustomerTaxID);
            report.SetParameterValue("CustomerName", customer?.CompanyName);
            report.SetParameterValue("GrossTotal", CurrentInvoice.DocumentTotals.GrossTotal);
            report.SetParameterValue("NetTotal", CurrentInvoice.DocumentTotals.NetTotal);
            report.SetParameterValue("TaxPayable", CurrentInvoice.DocumentTotals.TaxPayable);
            report.SetParameterValue("Hash", $"{CurrentInvoice.Hash[0]}{CurrentInvoice.Hash[10]}{CurrentInvoice.Hash[20]}{CurrentInvoice.Hash[30]} - {saftValidator?.SaftFileV4?.Header?.SoftwareCertificateNumber}");
            report.SetParameterValue("QrCode", qrCode);
        }

        public string GetATQrCode(Header header, Customer customer, SourceDocumentsSalesInvoicesInvoice invoice)
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
