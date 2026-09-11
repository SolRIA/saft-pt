using Dapper;
using Microsoft.Data.Sqlite;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Parser.Services;

public class SaftDatabaseWriter : IAsyncDisposable
{
    private readonly DbConnection _connection;
    private readonly bool _ownsConnection;
    private readonly string _fileId;
    private readonly int _batchSize;

    private readonly List<Customer> _customerBuffer = new();
    private readonly List<Product> _productBuffer = new();
    private readonly List<Supplier> _supplierBuffer = new();
    private readonly List<TaxTableEntry> _taxBuffer = new();
    private readonly List<SourceDocumentsSalesInvoicesInvoice> _invoiceBuffer = new();
    private readonly List<SourceDocumentsMovementOfGoodsStockMovement> _stockMovementBuffer = new();
    private readonly List<SourceDocumentsWorkingDocumentsWorkDocument> _workDocumentBuffer = new();
    private readonly List<SourceDocumentsPaymentsPayment> _paymentBuffer = new();
    private readonly List<GeneralLedgerAccountsAccount> _glAccountBuffer = new();
    private readonly List<GeneralLedgerEntriesJournal> _glJournalBuffer = new();

    public string FileId => _fileId;

    public SaftDatabaseWriter(DbConnection connection, string fileId = null, bool ownsConnection = false, int batchSize = 500)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _fileId = string.IsNullOrWhiteSpace(fileId) ? Guid.NewGuid().ToString() : fileId;
        _ownsConnection = ownsConnection;
        _batchSize = batchSize;
    }

    public async Task InitializeAsync()
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL; PRAGMA temp_store = MEMORY;";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task WriteAuditFileRecordAsync(string fileName)
    {
        const string sql = "INSERT OR REPLACE INTO AuditFiles (Id, FileName, SystemEntryDate) VALUES (@Id, @FileName, @SystemEntryDate);";
        await _connection.ExecuteAsync(sql, new { Id = _fileId, FileName = fileName, SystemEntryDate = DateTime.Now });
    }

    public async Task WriteHeaderAsync(Header header)
    {
        if (header == null) return;

        string companyAddressId = null;
        if (header.CompanyAddress != null)
        {
            companyAddressId = Guid.NewGuid().ToString();
            const string sqlAddress = @"
                INSERT INTO AddressStructures (Id, FileId, BuildingNumber, StreetName, AddressDetail, City, PostalCode, Region, Country)
                VALUES (@Id, @FileId, @BuildingNumber, @StreetName, @AddressDetail, @City, @PostalCode, @Region, @Country);";

            await _connection.ExecuteAsync(sqlAddress, new
            {
                Id = companyAddressId,
                FileId = _fileId,
                header.CompanyAddress.BuildingNumber,
                header.CompanyAddress.StreetName,
                header.CompanyAddress.AddressDetail,
                header.CompanyAddress.City,
                header.CompanyAddress.PostalCode,
                header.CompanyAddress.Region,
                header.CompanyAddress.Country
            });
        }

        const string sqlHeader = @"
            INSERT OR REPLACE INTO Headers (
                FileId, AuditFileVersion, CompanyID, TaxRegistrationNumber, TaxAccountingBasis,
                CompanyName, BusinessName, FiscalYear, StartDate, EndDate, CurrencyCode,
                DateCreated, TaxEntity, ProductCompanyTaxID, SoftwareCertificateNumber,
                ProductID, ProductVersion, HeaderComment, Telephone, Fax, Email, Website, CompanyAddressId
            ) VALUES (
                @FileId, @AuditFileVersion, @CompanyID, @TaxRegistrationNumber, @TaxAccountingBasis,
                @CompanyName, @BusinessName, @FiscalYear, @StartDate, @EndDate, @CurrencyCode,
                @DateCreated, @TaxEntity, @ProductCompanyTaxID, @SoftwareCertificateNumber,
                @ProductID, @ProductVersion, @HeaderComment, @Telephone, @Fax, @Email, @Website, @CompanyAddressId
            );";

        await _connection.ExecuteAsync(sqlHeader, new
        {
            FileId = _fileId,
            header.AuditFileVersion,
            header.CompanyID,
            header.TaxRegistrationNumber,
            TaxAccountingBasis = header.TaxAccountingBasis.ToString(),
            header.CompanyName,
            header.BusinessName,
            header.FiscalYear,
            header.StartDate,
            header.EndDate,
            header.CurrencyCode,
            header.DateCreated,
            header.TaxEntity,
            header.ProductCompanyTaxID,
            header.SoftwareCertificateNumber,
            header.ProductID,
            header.ProductVersion,
            header.HeaderComment,
            header.Telephone,
            header.Fax,
            header.Email,
            header.Website,
            CompanyAddressId = companyAddressId
        });
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        if (customer == null) return;
        _customerBuffer.Add(customer);
        if (_customerBuffer.Count >= _batchSize)
            await FlushCustomersAsync();
    }

    public async Task FlushCustomersAsync()
    {
        if (_customerBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sqlAddress = @"
                INSERT INTO AddressStructures (Id, FileId, BuildingNumber, StreetName, AddressDetail, City, PostalCode, Region, Country)
                VALUES (@Id, @FileId, @BuildingNumber, @StreetName, @AddressDetail, @City, @PostalCode, @Region, @Country);";

            const string sqlCustomer = @"
                INSERT INTO Customers (
                    Id, FileId, CustomerID, AccountID, CustomerTaxID, CompanyName, Contact,
                    Telephone, Fax, Email, Website, SelfBillingIndicator, BillingAddressId, ShipToAddress
                ) VALUES (
                    @Id, @FileId, @CustomerID, @AccountID, @CustomerTaxID, @CompanyName, @Contact,
                    @Telephone, @Fax, @Email, @Website, @SelfBillingIndicator, @BillingAddressId, @ShipToAddress
                );";

            foreach (var c in _customerBuffer)
            {
                string billingAddressId = null;
                if (c.BillingAddress != null)
                {
                    billingAddressId = Guid.NewGuid().ToString();
                    await _connection.ExecuteAsync(sqlAddress, new
                    {
                        Id = billingAddressId,
                        FileId = _fileId,
                        c.BillingAddress.BuildingNumber,
                        c.BillingAddress.StreetName,
                        c.BillingAddress.AddressDetail,
                        c.BillingAddress.City,
                        c.BillingAddress.PostalCode,
                        c.BillingAddress.Region,
                        c.BillingAddress.Country
                    }, tx);
                }

                string shipToJson = c.ShipToAddress != null && c.ShipToAddress.Length > 0
                    ? JsonSerializer.Serialize(c.ShipToAddress)
                    : null;

                await _connection.ExecuteAsync(sqlCustomer, new
                {
                    Id = c.Pk ?? Guid.NewGuid().ToString(),
                    FileId = _fileId,
                    c.CustomerID,
                    c.AccountID,
                    c.CustomerTaxID,
                    c.CompanyName,
                    c.Contact,
                    c.Telephone,
                    c.Fax,
                    c.Email,
                    c.Website,
                    c.SelfBillingIndicator,
                    BillingAddressId = billingAddressId,
                    ShipToAddress = shipToJson
                }, tx);
            }

            await tx.CommitAsync();
            _customerBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task AddProductAsync(Product product)
    {
        if (product == null) return;
        _productBuffer.Add(product);
        if (_productBuffer.Count >= _batchSize)
            await FlushProductsAsync();
    }

    public async Task FlushProductsAsync()
    {
        if (_productBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sql = @"
                INSERT INTO Products (
                    Id, FileId, ProductType, ProductCode, ProductGroup, ProductDescription,
                    ProductNumberCode, CustomsDetails
                ) VALUES (
                    @Id, @FileId, @ProductType, @ProductCode, @ProductGroup, @ProductDescription,
                    @ProductNumberCode, @CustomsDetails
                );";

            var rows = _productBuffer.Select(p => new
            {
                Id = p.Pk ?? Guid.NewGuid().ToString(),
                FileId = _fileId,
                ProductType = p.ProductType.ToString(),
                p.ProductCode,
                p.ProductGroup,
                p.ProductDescription,
                p.ProductNumberCode,
                CustomsDetails = p.CustomsDetails != null ? JsonSerializer.Serialize(p.CustomsDetails) : null
            });

            await _connection.ExecuteAsync(sql, rows, tx);
            await tx.CommitAsync();
            _productBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task AddSupplierAsync(Supplier supplier)
    {
        if (supplier == null) return;
        _supplierBuffer.Add(supplier);
        if (_supplierBuffer.Count >= _batchSize)
            await FlushSuppliersAsync();
    }

    public async Task FlushSuppliersAsync()
    {
        if (_supplierBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sqlAddress = @"
                INSERT INTO AddressStructures (Id, FileId, BuildingNumber, StreetName, AddressDetail, City, PostalCode, Region, Country)
                VALUES (@Id, @FileId, @BuildingNumber, @StreetName, @AddressDetail, @City, @PostalCode, @Region, @Country);";

            const string sqlSupplier = @"
                INSERT INTO Suppliers (
                    Id, FileId, SupplierID, AccountID, SupplierTaxID, CompanyName, Contact,
                    Telephone, Fax, Email, Website, SelfBillingIndicator, BillingAddressId, ShipFromAddress
                ) VALUES (
                    @Id, @FileId, @SupplierID, @AccountID, @SupplierTaxID, @CompanyName, @Contact,
                    @Telephone, @Fax, @Email, @Website, @SelfBillingIndicator, @BillingAddressId, @ShipFromAddress
                );";

            foreach (var s in _supplierBuffer)
            {
                string billingAddressId = null;
                if (s.BillingAddress != null)
                {
                    billingAddressId = Guid.NewGuid().ToString();
                    await _connection.ExecuteAsync(sqlAddress, new
                    {
                        Id = billingAddressId,
                        FileId = _fileId,
                        s.BillingAddress.BuildingNumber,
                        s.BillingAddress.StreetName,
                        s.BillingAddress.AddressDetail,
                        s.BillingAddress.City,
                        s.BillingAddress.PostalCode,
                        s.BillingAddress.Region,
                        s.BillingAddress.Country
                    }, tx);
                }

                string shipFromJson = s.ShipFromAddress != null && s.ShipFromAddress.Length > 0
                    ? JsonSerializer.Serialize(s.ShipFromAddress)
                    : null;

                await _connection.ExecuteAsync(sqlSupplier, new
                {
                    Id = s.Pk ?? Guid.NewGuid().ToString(),
                    FileId = _fileId,
                    s.SupplierID,
                    s.AccountID,
                    s.SupplierTaxID,
                    s.CompanyName,
                    s.Contact,
                    s.Telephone,
                    s.Fax,
                    s.Email,
                    s.Website,
                    s.SelfBillingIndicator,
                    BillingAddressId = billingAddressId,
                    ShipFromAddress = shipFromJson
                }, tx);
            }

            await tx.CommitAsync();
            _supplierBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task AddTaxAsync(TaxTableEntry tax)
    {
        if (tax == null) return;
        _taxBuffer.Add(tax);
        if (_taxBuffer.Count >= _batchSize)
            await FlushTaxesAsync();
    }

    public async Task FlushTaxesAsync()
    {
        if (_taxBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sql = @"
                INSERT INTO Taxes (
                    Id, FileId, TaxType, TaxCountryRegion, TaxCode, TaxAmount, TaxPercentage, ItemElementName
                ) VALUES (
                    @Id, @FileId, @TaxType, @TaxCountryRegion, @TaxCode, @TaxAmount, @TaxPercentage, @ItemElementName
                );";

            var rows = _taxBuffer.Select(t => new
            {
                Id = t.Pk ?? Guid.NewGuid().ToString(),
                FileId = _fileId,
                TaxType = t.TaxType.ToString(),
                t.TaxCountryRegion,
                t.TaxCode,
                TaxAmount = t.TaxAmount ?? 0m,
                TaxPercentage = t.TaxPercentage ?? 0m,
                ItemElementName = t.ItemElementName.ToString()
            });

            await _connection.ExecuteAsync(sql, rows, tx);
            await tx.CommitAsync();
            _taxBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task WriteInfoInvoicesAsync(string numberOfEntries, decimal totalDebit, decimal totalCredit)
    {
        const string sql = "INSERT INTO InfoInvoices (Id, FileId, NumberOfEntries, TotalDebit, TotalCredit) VALUES (@Id, @FileId, @NumberOfEntries, @TotalDebit, @TotalCredit);";
        await _connection.ExecuteAsync(sql, new { Id = Guid.NewGuid().ToString(), FileId = _fileId, NumberOfEntries = numberOfEntries, TotalDebit = (double)totalDebit, TotalCredit = (double)totalCredit });
    }

    public async Task AddInvoiceAsync(SourceDocumentsSalesInvoicesInvoice invoice)
    {
        if (invoice == null) return;
        _invoiceBuffer.Add(invoice);
        if (_invoiceBuffer.Count >= _batchSize)
            await FlushInvoicesAsync();
    }

    public async Task FlushInvoicesAsync()
    {
        if (_invoiceBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sqlInvoice = @"
                INSERT INTO Invoices (
                    Id, FileId, InvoiceStatus, InvoiceStatusDate, Reason, StatusSourceID, SourceBilling,
                    TaxPayable, NetTotal, GrossTotal, Currency, Settlement, Payment, WithholdingTax,
                    SpecialRegimes, InvoiceNo, ATCUD, Hash, HashControl, Period, InvoiceDate,
                    InvoiceType, SourceID, EACCode, SystemEntryDate, TransactionID, CustomerID,
                    ShipTo, ShipFrom, MovementEndTime, MovementStartTime
                ) VALUES (
                    @Id, @FileId, @InvoiceStatus, @InvoiceStatusDate, @Reason, @StatusSourceID, @SourceBilling,
                    @TaxPayable, @NetTotal, @GrossTotal, @Currency, @Settlement, @Payment, @WithholdingTax,
                    @SpecialRegimes, @InvoiceNo, @ATCUD, @Hash, @HashControl, @Period, @InvoiceDate,
                    @InvoiceType, @SourceID, @EACCode, @SystemEntryDate, @TransactionID, @CustomerID,
                    @ShipTo, @ShipFrom, @MovementEndTime, @MovementStartTime
                );";

            const string sqlLine = @"
                INSERT INTO InvoiceLines (
                    Id, ParentId, LineNumber, ProductCode, ProductDescription, Quantity, UnitOfMeasure,
                    UnitPrice, TaxBase, TaxPointDate, Description, CreditAmount, DebitAmount,
                    ItemElementName, TaxExemptionReason, TaxExemptionCode, SettlementAmount,
                    TaxType, TaxCountryRegion, TaxCode, TaxAmount, TaxPercentage, TaxItemElementName,
                    ProductSerialNumber, `References`, OrderReferences, CustomsInformation
                ) VALUES (
                    @Id, @ParentId, @LineNumber, @ProductCode, @ProductDescription, @Quantity, @UnitOfMeasure,
                    @UnitPrice, @TaxBase, @TaxPointDate, @Description, @CreditAmount, @DebitAmount,
                    @ItemElementName, @TaxExemptionReason, @TaxExemptionCode, @SettlementAmount,
                    @TaxType, @TaxCountryRegion, @TaxCode, @TaxAmount, @TaxPercentage, @TaxItemElementName,
                    @ProductSerialNumber, @References, @OrderReferences, @CustomsInformation
                );";

            var invoiceRows = new List<object>(_invoiceBuffer.Count);
            var lineRows = new List<object>();

            foreach (var inv in _invoiceBuffer)
            {
                string invId = inv.Pk ?? Guid.NewGuid().ToString();
                inv.Pk = invId;

                invoiceRows.Add(new
                {
                    Id = invId,
                    FileId = _fileId,
                    InvoiceStatus = inv.DocumentStatus?.InvoiceStatus.ToString(),
                    InvoiceStatusDate = inv.DocumentStatus?.InvoiceStatusDate,
                    Reason = inv.DocumentStatus?.Reason,
                    StatusSourceID = inv.DocumentStatus?.SourceID,
                    SourceBilling = inv.DocumentStatus?.SourceBilling.ToString(),
                    TaxPayable = (double)(inv.DocumentTotals?.TaxPayable ?? 0m),
                    NetTotal = (double)(inv.DocumentTotals?.NetTotal ?? 0m),
                    GrossTotal = (double)(inv.DocumentTotals?.GrossTotal ?? 0m),
                    Currency = (string)null,
                    Settlement = (string)null,
                    Payment = inv.DocumentTotals?.Payment != null ? JsonSerializer.Serialize(inv.DocumentTotals.Payment) : null,
                    WithholdingTax = inv.WithholdingTax != null && inv.WithholdingTax.Length > 0 ? JsonSerializer.Serialize(inv.WithholdingTax) : null,
                    SpecialRegimes = inv.SpecialRegimes != null ? JsonSerializer.Serialize(inv.SpecialRegimes) : null,
                    inv.InvoiceNo,
                    inv.ATCUD,
                    inv.Hash,
                    inv.HashControl,
                    inv.Period,
                    inv.InvoiceDate,
                    InvoiceType = inv.InvoiceType.ToString(),
                    inv.SourceID,
                    inv.EACCode,
                    inv.SystemEntryDate,
                    inv.TransactionID,
                    inv.CustomerID,
                    ShipTo = inv.ShipTo != null ? JsonSerializer.Serialize(inv.ShipTo) : null,
                    ShipFrom = inv.ShipFrom != null ? JsonSerializer.Serialize(inv.ShipFrom) : null,
                    MovementEndTime = inv.MovementEndTimeSpecified ? (DateTime?)inv.MovementEndTime : null,
                    MovementStartTime = inv.MovementStartTimeSpecified ? (DateTime?)inv.MovementStartTime : null
                });

                if (inv.Line != null && inv.Line.Length > 0)
                {
                    foreach (var line in inv.Line)
                    {
                        string lineId = line.Pk ?? Guid.NewGuid().ToString();
                        line.Pk = lineId;

                        lineRows.Add(new
                        {
                            Id = lineId,
                            ParentId = invId,
                            line.LineNumber,
                            line.ProductCode,
                            line.ProductDescription,
                            Quantity = (double)line.Quantity,
                            line.UnitOfMeasure,
                            UnitPrice = (double)line.UnitPrice,
                            TaxBase = (double)line.TaxBase,
                            line.TaxPointDate,
                            line.Description,
                            CreditAmount = (double)(line.CreditAmount ?? 0m),
                            DebitAmount = (double)(line.DebitAmount ?? 0m),
                            ItemElementName = line.ItemElementName.ToString(),
                            line.TaxExemptionReason,
                            line.TaxExemptionCode,
                            SettlementAmount = (double)line.SettlementAmount,
                            TaxType = line.Tax?.TaxType.ToString(),
                            TaxCountryRegion = line.Tax?.TaxCountryRegion,
                            TaxCode = line.Tax?.TaxCode,
                            TaxAmount = (double)(line.Tax?.TaxAmount ?? 0m),
                            TaxPercentage = (double)(line.Tax?.TaxPercentage ?? 0m),
                            TaxItemElementName = line.Tax?.ItemElementName.ToString(),
                            ProductSerialNumber = (string)null,
                            References = (string)null,
                            OrderReferences = (string)null,
                            CustomsInformation = (string)null
                        });
                    }
                }
            }

            await _connection.ExecuteAsync(sqlInvoice, invoiceRows, tx);
            if (lineRows.Count > 0)
                await _connection.ExecuteAsync(sqlLine, lineRows, tx);

            await tx.CommitAsync();
            _invoiceBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task WriteInfoStockMovementsAsync(string numberOfMovementLines, decimal totalQuantityIssued)
    {
        const string sql = "INSERT INTO InfoStockMovements (Id, FileId, NumberOfMovementLines, TotalQuantityIssued) VALUES (@Id, @FileId, @NumberOfMovementLines, @TotalQuantityIssued);";
        await _connection.ExecuteAsync(sql, new { Id = Guid.NewGuid().ToString(), FileId = _fileId, NumberOfMovementLines = numberOfMovementLines, TotalQuantityIssued = (double)totalQuantityIssued });
    }

    public async Task AddStockMovementAsync(SourceDocumentsMovementOfGoodsStockMovement doc)
    {
        if (doc == null) return;
        _stockMovementBuffer.Add(doc);
        if (_stockMovementBuffer.Count >= _batchSize)
            await FlushStockMovementsAsync();
    }

    public async Task FlushStockMovementsAsync()
    {
        if (_stockMovementBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sqlDoc = @"
                INSERT INTO StockMovements (
                    Id, FileId, DocumentNumber, ATCUD, Hash, HashControl, Period, MovementDate,
                    MovementType, SystemEntryDate, TransactionID, Item, ItemElementName, SourceID,
                    EACCode, MovementComments, MovementEndTime, MovementStartTime, ATDocCodeID,
                    MovementStatus, MovementStatusDate, Reason, StatusSourceID, SourceBilling,
                    TaxPayable, NetTotal, GrossTotal, Currency, ShipTo, ShipFrom
                ) VALUES (
                    @Id, @FileId, @DocumentNumber, @ATCUD, @Hash, @HashControl, @Period, @MovementDate,
                    @MovementType, @SystemEntryDate, @TransactionID, @Item, @ItemElementName, @SourceID,
                    @EACCode, @MovementComments, @MovementEndTime, @MovementStartTime, @ATDocCodeID,
                    @MovementStatus, @MovementStatusDate, @Reason, @StatusSourceID, @SourceBilling,
                    @TaxPayable, @NetTotal, @GrossTotal, @Currency, @ShipTo, @ShipFrom
                );";

            const string sqlLine = @"
                INSERT INTO StockMovementLines (
                    Id, ParentId, LineNumber, ProductCode, ProductDescription, Quantity, UnitOfMeasure,
                    UnitPrice, Description, CreditAmount, DebitAmount, ItemElementName, TaxExemptionReason,
                    TaxExemptionCode, SettlementAmount, Tax, ProductSerialNumber, OrderReferences, CustomsInformation
                ) VALUES (
                    @Id, @ParentId, @LineNumber, @ProductCode, @ProductDescription, @Quantity, @UnitOfMeasure,
                    @UnitPrice, @Description, @CreditAmount, @DebitAmount, @ItemElementName, @TaxExemptionReason,
                    @TaxExemptionCode, @SettlementAmount, @Tax, @ProductSerialNumber, @OrderReferences, @CustomsInformation
                );";

            var docRows = new List<object>(_stockMovementBuffer.Count);
            var lineRows = new List<object>();

            foreach (var doc in _stockMovementBuffer)
            {
                string docId = doc.Pk ?? Guid.NewGuid().ToString();
                doc.Pk = docId;

                docRows.Add(new
                {
                    Id = docId,
                    FileId = _fileId,
                    doc.DocumentNumber,
                    doc.ATCUD,
                    doc.Hash,
                    doc.HashControl,
                    doc.Period,
                    doc.MovementDate,
                    MovementType = doc.MovementType.ToString(),
                    doc.SystemEntryDate,
                    doc.TransactionID,
                    Item = (string)null,
                    ItemElementName = (string)null,
                    doc.SourceID,
                    doc.EACCode,
                    MovementComments = (string)null,
                    MovementEndTime = doc.MovementEndTimeSpecified ? (DateTime?)doc.MovementEndTime : null,
                    MovementStartTime = doc.MovementStartTimeSpecified ? (DateTime?)doc.MovementStartTime : null,
                    doc.ATDocCodeID,
                    MovementStatus = doc.DocumentStatus?.MovementStatus.ToString(),
                    MovementStatusDate = doc.DocumentStatus?.MovementStatusDate,
                    Reason = doc.DocumentStatus?.Reason,
                    StatusSourceID = doc.DocumentStatus?.SourceID,
                    SourceBilling = doc.DocumentStatus?.SourceBilling.ToString(),
                    TaxPayable = (double)(doc.DocumentTotals?.TaxPayable ?? 0m),
                    NetTotal = (double)(doc.DocumentTotals?.NetTotal ?? 0m),
                    GrossTotal = (double)(doc.DocumentTotals?.GrossTotal ?? 0m),
                    Currency = doc.DocumentTotals?.Currency != null ? JsonSerializer.Serialize(doc.DocumentTotals.Currency) : null,
                    ShipTo = doc.ShipTo != null ? JsonSerializer.Serialize(doc.ShipTo) : null,
                    ShipFrom = doc.ShipFrom != null ? JsonSerializer.Serialize(doc.ShipFrom) : null
                });

                if (doc.Line != null)
                {
                    foreach (var line in doc.Line)
                    {
                        string lineId = line.Pk ?? Guid.NewGuid().ToString();
                        line.Pk = lineId;

                        lineRows.Add(new
                        {
                            Id = lineId,
                            ParentId = docId,
                            line.LineNumber,
                            line.ProductCode,
                            line.ProductDescription,
                            Quantity = (double)line.Quantity,
                            line.UnitOfMeasure,
                            UnitPrice = (double)line.UnitPrice,
                            line.Description,
                            CreditAmount = (double)(line.CreditAmount ?? 0m),
                            DebitAmount = (double)(line.DebitAmount ?? 0m),
                            ItemElementName = line.ItemElementName.ToString(),
                            line.TaxExemptionReason,
                            line.TaxExemptionCode,
                            SettlementAmount = (double)line.SettlementAmount,
                            Tax = (string)null,
                            ProductSerialNumber = line.ProductSerialNumber != null ? JsonSerializer.Serialize(line.ProductSerialNumber) : null,
                            OrderReferences = (string)null,
                            CustomsInformation = (string)null
                        });
                    }
                }
            }

            await _connection.ExecuteAsync(sqlDoc, docRows, tx);
            if (lineRows.Count > 0)
                await _connection.ExecuteAsync(sqlLine, lineRows, tx);

            await tx.CommitAsync();
            _stockMovementBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task WriteInfoWorkDocumentsAsync(string numberOfEntries, decimal totalDebit, decimal totalCredit)
    {
        const string sql = "INSERT INTO InfoWorkDocuments (Id, FileId, NumberOfEntries, TotalDebit, TotalCredit) VALUES (@Id, @FileId, @NumberOfEntries, @TotalDebit, @TotalCredit);";
        await _connection.ExecuteAsync(sql, new { Id = Guid.NewGuid().ToString(), FileId = _fileId, NumberOfEntries = numberOfEntries, TotalDebit = (double)totalDebit, TotalCredit = (double)totalCredit });
    }

    public async Task AddWorkDocumentAsync(SourceDocumentsWorkingDocumentsWorkDocument doc)
    {
        if (doc == null) return;
        _workDocumentBuffer.Add(doc);
        if (_workDocumentBuffer.Count >= _batchSize)
            await FlushWorkDocumentsAsync();
    }

    public async Task FlushWorkDocumentsAsync()
    {
        if (_workDocumentBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sqlDoc = @"
                INSERT INTO WorkDocuments (
                    Id, FileId, DocumentNumber, ATCUD, Hash, HashControl, Period, WorkDate,
                    WorkType, SourceID, EACCode, SystemEntryDate, TransactionID, CustomerID,
                    WorkStatus, WorkStatusDate, Reason, StatusSourceID, SourceBilling,
                    TaxPayable, NetTotal, GrossTotal, Currency
                ) VALUES (
                    @Id, @FileId, @DocumentNumber, @ATCUD, @Hash, @HashControl, @Period, @WorkDate,
                    @WorkType, @SourceID, @EACCode, @SystemEntryDate, @TransactionID, @CustomerID,
                    @WorkStatus, @WorkStatusDate, @Reason, @StatusSourceID, @SourceBilling,
                    @TaxPayable, @NetTotal, @GrossTotal, @Currency
                );";

            const string sqlLine = @"
                INSERT INTO WorkDocumentLines (
                    Id, ParentId, LineNumber, ProductCode, ProductDescription, Quantity, UnitOfMeasure,
                    UnitPrice, TaxBase, TaxPointDate, Description, CreditAmount, DebitAmount, ItemElementName,
                    TaxExemptionReason, TaxExemptionCode, SettlementAmount, Tax, `References`,
                    ProductSerialNumber, OrderReferences, CustomsInformation
                ) VALUES (
                    @Id, @ParentId, @LineNumber, @ProductCode, @ProductDescription, @Quantity, @UnitOfMeasure,
                    @UnitPrice, @TaxBase, @TaxPointDate, @Description, @CreditAmount, @DebitAmount, @ItemElementName,
                    @TaxExemptionReason, @TaxExemptionCode, @SettlementAmount, @Tax, @References,
                    @ProductSerialNumber, @OrderReferences, @CustomsInformation
                );";

            var docRows = new List<object>(_workDocumentBuffer.Count);
            var lineRows = new List<object>();

            foreach (var doc in _workDocumentBuffer)
            {
                string docId = doc.Pk ?? Guid.NewGuid().ToString();
                doc.Pk = docId;

                docRows.Add(new
                {
                    Id = docId,
                    FileId = _fileId,
                    doc.DocumentNumber,
                    doc.ATCUD,
                    doc.Hash,
                    doc.HashControl,
                    doc.Period,
                    doc.WorkDate,
                    WorkType = doc.WorkType.ToString(),
                    doc.SourceID,
                    doc.EACCode,
                    doc.SystemEntryDate,
                    doc.TransactionID,
                    doc.CustomerID,
                    WorkStatus = doc.DocumentStatus?.WorkStatus.ToString(),
                    WorkStatusDate = doc.DocumentStatus?.WorkStatusDate,
                    Reason = doc.DocumentStatus?.Reason,
                    StatusSourceID = doc.DocumentStatus?.SourceID,
                    SourceBilling = doc.DocumentStatus?.SourceBilling.ToString(),
                    TaxPayable = (double)(doc.DocumentTotals?.TaxPayable ?? 0m),
                    NetTotal = (double)(doc.DocumentTotals?.NetTotal ?? 0m),
                    GrossTotal = (double)(doc.DocumentTotals?.GrossTotal ?? 0m),
                    Currency = doc.DocumentTotals?.Currency != null ? JsonSerializer.Serialize(doc.DocumentTotals.Currency) : null
                });

                if (doc.Line != null)
                {
                    foreach (var line in doc.Line)
                    {
                        string lineId = line.Pk ?? Guid.NewGuid().ToString();
                        line.Pk = lineId;

                        lineRows.Add(new
                        {
                            Id = lineId,
                            ParentId = docId,
                            line.LineNumber,
                            line.ProductCode,
                            line.ProductDescription,
                            Quantity = (double)line.Quantity,
                            line.UnitOfMeasure,
                            UnitPrice = (double)line.UnitPrice,
                            TaxBase = (double)line.TaxBase,
                            line.TaxPointDate,
                            line.Description,
                            CreditAmount = (double)(line.CreditAmount ?? 0m),
                            DebitAmount = (double)(line.DebitAmount ?? 0m),
                            ItemElementName = line.ItemElementName.ToString(),
                            line.TaxExemptionReason,
                            line.TaxExemptionCode,
                            SettlementAmount = (double)line.SettlementAmount,
                            Tax = line.Tax != null ? JsonSerializer.Serialize(line.Tax) : null,
                            References = line.References != null ? JsonSerializer.Serialize(line.References) : null,
                            ProductSerialNumber = line.ProductSerialNumber != null ? JsonSerializer.Serialize(line.ProductSerialNumber) : null,
                            OrderReferences = (string)null,
                            CustomsInformation = (string)null
                        });
                    }
                }
            }

            await _connection.ExecuteAsync(sqlDoc, docRows, tx);
            if (lineRows.Count > 0)
                await _connection.ExecuteAsync(sqlLine, lineRows, tx);

            await tx.CommitAsync();
            _workDocumentBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task WriteInfoPaymentsAsync(string numberOfEntries, decimal totalDebit, decimal totalCredit)
    {
        const string sql = "INSERT INTO InfoPayments (Id, FileId, NumberOfEntries, TotalDebit, TotalCredit) VALUES (@Id, @FileId, @NumberOfEntries, @TotalDebit, @TotalCredit);";
        await _connection.ExecuteAsync(sql, new { Id = Guid.NewGuid().ToString(), FileId = _fileId, NumberOfEntries = numberOfEntries, TotalDebit = (double)totalDebit, TotalCredit = (double)totalCredit });
    }

    public async Task AddPaymentAsync(SourceDocumentsPaymentsPayment payment)
    {
        if (payment == null) return;
        _paymentBuffer.Add(payment);
        if (_paymentBuffer.Count >= _batchSize)
            await FlushPaymentsAsync();
    }

    public async Task FlushPaymentsAsync()
    {
        if (_paymentBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sqlDoc = @"
                INSERT INTO Payments (
                    Id, FileId, PaymentRefNo, ATCUD, Period, TransactionID, TransactionDate,
                    PaymentType, Description, SystemID, SourceID, SystemEntryDate, CustomerID,
                    PaymentStatus, PaymentStatusDate, Reason, StatusSourceID, SourcePayment,
                    TaxPayable, NetTotal, GrossTotal, Settlement, Currency, PaymentMethod, WithholdingTax
                ) VALUES (
                    @Id, @FileId, @PaymentRefNo, @ATCUD, @Period, @TransactionID, @TransactionDate,
                    @PaymentType, @Description, @SystemID, @SourceID, @SystemEntryDate, @CustomerID,
                    @PaymentStatus, @PaymentStatusDate, @Reason, @StatusSourceID, @SourcePayment,
                    @TaxPayable, @NetTotal, @GrossTotal, @Settlement, @Currency, @PaymentMethod, @WithholdingTax
                );";

            const string sqlLine = @"
                INSERT INTO PaymentLines (
                    Id, ParentId, LineNumber, SettlementAmount, CreditAmount, DebitAmount,
                    ItemElementName, TaxExemptionReason, TaxExemptionCode, OriginatingON,
                    InvoiceDate, Description, Tax
                ) VALUES (
                    @Id, @ParentId, @LineNumber, @SettlementAmount, @CreditAmount, @DebitAmount,
                    @ItemElementName, @TaxExemptionReason, @TaxExemptionCode, @OriginatingON,
                    @InvoiceDate, @Description, @Tax
                );";

            var docRows = new List<object>(_paymentBuffer.Count);
            var lineRows = new List<object>();

            foreach (var doc in _paymentBuffer)
            {
                string docId = doc.Pk ?? Guid.NewGuid().ToString();
                doc.Pk = docId;

                docRows.Add(new
                {
                    Id = docId,
                    FileId = _fileId,
                    doc.PaymentRefNo,
                    doc.ATCUD,
                    doc.Period,
                    doc.TransactionID,
                    doc.TransactionDate,
                    PaymentType = doc.PaymentType.ToString(),
                    doc.Description,
                    doc.SystemID,
                    doc.SourceID,
                    doc.SystemEntryDate,
                    doc.CustomerID,
                    PaymentStatus = doc.DocumentStatus?.PaymentStatus.ToString(),
                    PaymentStatusDate = doc.DocumentStatus?.PaymentStatusDate,
                    Reason = doc.DocumentStatus?.Reason,
                    StatusSourceID = doc.DocumentStatus?.SourceID,
                    SourcePayment = doc.DocumentStatus?.SourcePayment.ToString(),
                    TaxPayable = (double)(doc.DocumentTotals?.TaxPayable ?? 0m),
                    NetTotal = (double)(doc.DocumentTotals?.NetTotal ?? 0m),
                    GrossTotal = (double)(doc.DocumentTotals?.GrossTotal ?? 0m),
                    Settlement = doc.DocumentTotals?.Settlement != null ? JsonSerializer.Serialize(doc.DocumentTotals.Settlement) : null,
                    Currency = doc.DocumentTotals?.Currency != null ? JsonSerializer.Serialize(doc.DocumentTotals.Currency) : null,
                    PaymentMethod = doc.PaymentMethod != null ? JsonSerializer.Serialize(doc.PaymentMethod) : null,
                    WithholdingTax = doc.WithholdingTax != null ? JsonSerializer.Serialize(doc.WithholdingTax) : null
                });

                if (doc.Line != null)
                {
                    foreach (var line in doc.Line)
                    {
                        string lineId = line.Pk ?? Guid.NewGuid().ToString();
                        line.Pk = lineId;

                        string origOn = line.SourceDocumentID != null && line.SourceDocumentID.Length > 0
                            ? line.SourceDocumentID[0].OriginatingON : null;
                        DateTime? invDate = line.SourceDocumentID != null && line.SourceDocumentID.Length > 0
                            ? line.SourceDocumentID[0].InvoiceDate : null;
                        string desc = line.SourceDocumentID != null && line.SourceDocumentID.Length > 0
                            ? line.SourceDocumentID[0].Description : null;

                        lineRows.Add(new
                        {
                            Id = lineId,
                            ParentId = docId,
                            line.LineNumber,
                            SettlementAmount = (double)line.SettlementAmount,
                            CreditAmount = (double)(line.CreditAmount ?? 0m),
                            DebitAmount = (double)(line.DebitAmount ?? 0m),
                            ItemElementName = line.ItemElementName.ToString(),
                            line.TaxExemptionReason,
                            line.TaxExemptionCode,
                            OriginatingON = origOn,
                            InvoiceDate = invDate,
                            Description = desc,
                            Tax = line.Tax != null ? JsonSerializer.Serialize(line.Tax) : null
                        });
                    }
                }
            }

            await _connection.ExecuteAsync(sqlDoc, docRows, tx);
            if (lineRows.Count > 0)
                await _connection.ExecuteAsync(sqlLine, lineRows, tx);

            await tx.CommitAsync();
            _paymentBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task AddGeneralLedgerAccountAsync(GeneralLedgerAccountsAccount account, TaxonomyReference? taxonomy)
    {
        if (account == null) return;
        _glAccountBuffer.Add(account);
        if (_glAccountBuffer.Count >= _batchSize)
            await FlushGeneralLedgerAccountsAsync(taxonomy);
    }

    public async Task FlushGeneralLedgerAccountsAsync(TaxonomyReference? taxonomy = null)
    {
        if (_glAccountBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sql = @"
                INSERT INTO GeneralLedgerAccounts (
                    Id, FileId, TaxonomyReference, AccountID, AccountDescription,
                    OpeningDebitBalance, OpeningCreditBalance, ClosingDebitBalance, ClosingCreditBalance,
                    GroupingCategory, GroupingCode, TaxonomyCode
                ) VALUES (
                    @Id, @FileId, @TaxonomyReference, @AccountID, @AccountDescription,
                    @OpeningDebitBalance, @OpeningCreditBalance, @ClosingDebitBalance, @ClosingCreditBalance,
                    @GroupingCategory, @GroupingCode, @TaxonomyCode
                );";

            var rows = _glAccountBuffer.Select(a => new
            {
                Id = a.AccountID ?? Guid.NewGuid().ToString(),
                FileId = _fileId,
                TaxonomyReference = taxonomy?.ToString(),
                a.AccountID,
                a.AccountDescription,
                a.OpeningDebitBalance,
                a.OpeningCreditBalance,
                a.ClosingDebitBalance,
                a.ClosingCreditBalance,
                GroupingCategory = a.GroupingCategory.ToString(),
                a.GroupingCode,
                a.TaxonomyCode
            });

            await _connection.ExecuteAsync(sql, rows, tx);
            await tx.CommitAsync();
            _glAccountBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task WriteInfoGeneralLedgerEntriesAsync(string numberOfEntries, decimal totalDebit, decimal totalCredit)
    {
        const string sql = "INSERT INTO GeneralLedgerEntries (Id, FileId, NumberOfEntries, TotalDebit, TotalCredit) VALUES (@Id, @FileId, @NumberOfEntries, @TotalDebit, @TotalCredit);";
        await _connection.ExecuteAsync(sql, new { Id = Guid.NewGuid().ToString(), FileId = _fileId, NumberOfEntries = numberOfEntries, TotalDebit = (double)totalDebit, TotalCredit = (double)totalCredit });
    }

    public async Task AddGeneralLedgerJournalAsync(GeneralLedgerEntriesJournal journal)
    {
        if (journal == null) return;
        _glJournalBuffer.Add(journal);
        if (_glJournalBuffer.Count >= _batchSize)
            await FlushGeneralLedgerJournalsAsync();
    }

    public async Task FlushGeneralLedgerJournalsAsync()
    {
        if (_glJournalBuffer.Count == 0) return;

        using var tx = await _connection.BeginTransactionAsync();
        try
        {
            const string sqlJournal = "INSERT INTO GeneralLedgerJournals (Id, FileId, JournalID, Description) VALUES (@Id, @FileId, @JournalID, @Description);";
            const string sqlTx = @"
                INSERT INTO GeneralLedgerTransactions (
                    Id, JournalId, FileId, TransactionID, Period, TransactionDate, SourceID,
                    Description, DocArchivalNumber, TransactionType, GLPostingDate, CustomerID, SupplierID
                ) VALUES (
                    @Id, @JournalId, @FileId, @TransactionID, @Period, @TransactionDate, @SourceID,
                    @Description, @DocArchivalNumber, @TransactionType, @GLPostingDate, @CustomerID, @SupplierID
                );";
            const string sqlLine = @"
                INSERT INTO GeneralLedgerTransactionLines (
                    Id, TransactionId, RecordID, AccountID, SourceDocumentID, SystemEntryDate, Description, DebitAmount, CreditAmount
                ) VALUES (
                    @Id, @TransactionId, @RecordID, @AccountID, @SourceDocumentID, @SystemEntryDate, @Description, @DebitAmount, @CreditAmount
                );";

            foreach (var j in _glJournalBuffer)
            {
                string jId = j.Pk ?? Guid.NewGuid().ToString();
                await _connection.ExecuteAsync(sqlJournal, new { Id = jId, FileId = _fileId, j.JournalID, j.Description }, tx);

                if (j.Transaction != null)
                {
                    foreach (var t in j.Transaction)
                    {
                        string tId = t.Pk ?? Guid.NewGuid().ToString();
                        string custId = t.ItemElementName == ItemChoiceType3.CustomerID ? t.Item : null;
                        string suppId = t.ItemElementName == ItemChoiceType3.SupplierID ? t.Item : null;

                        await _connection.ExecuteAsync(sqlTx, new
                        {
                            Id = tId,
                            JournalId = jId,
                            FileId = _fileId,
                            t.TransactionID,
                            t.Period,
                            t.TransactionDate,
                            t.SourceID,
                            t.Description,
                            t.DocArchivalNumber,
                            TransactionType = t.TransactionType.ToString(),
                            t.GLPostingDate,
                            CustomerID = custId,
                            SupplierID = suppId
                        }, tx);

                        if (t.Lines?.DebitLine != null)
                        {
                            await _connection.ExecuteAsync(sqlLine, new
                            {
                                Id = Guid.NewGuid().ToString(),
                                TransactionId = tId,
                                t.Lines.DebitLine.RecordID,
                                t.Lines.DebitLine.AccountID,
                                t.Lines.DebitLine.SourceDocumentID,
                                t.Lines.DebitLine.SystemEntryDate,
                                t.Lines.DebitLine.Description,
                                DebitAmount = t.Lines.DebitLine.DebitAmount,
                                CreditAmount = 0m
                            }, tx);
                        }

                        if (t.Lines?.CreditLine != null)
                        {
                            await _connection.ExecuteAsync(sqlLine, new
                            {
                                Id = Guid.NewGuid().ToString(),
                                TransactionId = tId,
                                t.Lines.CreditLine.RecordID,
                                t.Lines.CreditLine.AccountID,
                                t.Lines.CreditLine.SourceDocumentID,
                                t.Lines.CreditLine.SystemEntryDate,
                                t.Lines.CreditLine.Description,
                                DebitAmount = 0m,
                                CreditAmount = t.Lines.CreditLine.CreditAmount
                            }, tx);
                        }
                    }
                }
            }

            await tx.CommitAsync();
            _glJournalBuffer.Clear();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task FlushAllAsync()
    {
        await FlushCustomersAsync();
        await FlushProductsAsync();
        await FlushSuppliersAsync();
        await FlushTaxesAsync();
        await FlushInvoicesAsync();
        await FlushStockMovementsAsync();
        await FlushWorkDocumentsAsync();
        await FlushPaymentsAsync();
        await FlushGeneralLedgerAccountsAsync();
        await FlushGeneralLedgerJournalsAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await FlushAllAsync();

        if (_ownsConnection)
        {
            await _connection.DisposeAsync();
        }
    }
}
