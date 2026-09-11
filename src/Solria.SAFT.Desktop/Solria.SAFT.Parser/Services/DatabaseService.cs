using Dapper;
using Microsoft.Data.Sqlite;
using SolRIA.SAFT.Parser.Models;
using SolRIA.SAFT.Parser.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Parser.Services;

public class DatabaseService(string customDatabaseFilename = null) : IDatabaseService
{
    readonly string database_filename = customDatabaseFilename ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SolRIA SAFT", "solria_saft.sqlite");
    readonly string app_version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public DbConnection CreateConnection()
    {
        return new SqliteConnection($"Data Source={database_filename}");
    }

    private DbConnection InitConnection()
    {
        return CreateConnection();
    }


    public void InitDatabase()
    {
        try
        {
            using var connection = InitConnection();
            if (File.Exists(database_filename) == false)
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SolRIA SAFT");
                if (Directory.Exists(folder) == false)
                    Directory.CreateDirectory(folder);

                FileStream fs = File.Create(database_filename);
                fs.Close();
            }

            var versionTable = connection.QueryFirstOrDefault<int>(
                "select COUNT(1) from sqlite_master where type = 'table' and name = 'DatabaseVersion'");

            if (versionTable <= 0)
            {
                string schemaPath = Path.Combine(AppContext.BaseDirectory, "SQL", "schema.sql");
                if (!File.Exists(schemaPath))
                    schemaPath = Path.Combine(Environment.CurrentDirectory, "SQL", "schema.sql");
                if (!File.Exists(schemaPath) && typeof(DatabaseService).Assembly.Location is { Length: > 0 } asmLoc)
                    schemaPath = Path.Combine(Path.GetDirectoryName(asmLoc), "SQL", "schema.sql");
                string sqlContent = File.ReadAllText(schemaPath);
                string[] sqlCommands = sqlContent.Split(';');

                if (connection.State != ConnectionState.Open)
                    connection.Open();
                //create the tables
                using var transaction = connection.BeginTransaction();
                foreach (var sql in sqlCommands)
                {
                    if (string.IsNullOrEmpty(sql))
                        continue;

                    System.Diagnostics.Debug.WriteLine($"sql update command: {sql}");
                    connection.Execute(sql, transaction: transaction);
                }

                transaction.Commit();
            }

            //check for updates on the database
            var versionDB = connection.QueryFirstOrDefault<int>(
                "SELECT Version FROM DatabaseVersion ORDER BY Version DESC LIMIT 1;");

            if (versionDB < UpdateScripts.Version)
            {
                //the database needs to update
                var updateScripts = UpdateScripts.GetUpdateScripts(versionDB);

                if (connection.State != ConnectionState.Open)
                    connection.Open();
                var transaction = connection.BeginTransaction();
                //update the database schema and data
                if (updateScripts != null && updateScripts.Length > 0)
                {
                    foreach (var script in updateScripts.OrderBy(s => s.Version).ThenBy(s => s.Order))
                    {
                        connection.Execute(sql: script.Sql, transaction: transaction);
                    }
                }

                //update the database version
                connection.Execute(
                    "INSERT INTO DatabaseVersion (UpgradeDate,Version,AppVersion) VALUES (@UpgradeDate,@Version,@AppVersion);",
                    new { UpdateScripts.Version, UpgradeDate = DateTime.Now, AppVersion = app_version },
                    transaction);

                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            LogException(ex, "InitDatabase");
        }
    }

    public string GetAppVersion()
    {
        return app_version;
    }

    public void LogException(Exception ex, string controller)
    {
        using var connection = InitConnection();
        string message = ex.Message;

        Exception innerEx = ex.InnerException;
        while (innerEx != null)
        {
            message += Environment.NewLine + innerEx;

            innerEx = innerEx.InnerException;
            connection.Execute("INSERT INTO Logs (Message,Controller) VALUES (@message, @controller);", new { message, controller });
        }
    }

    public void LogInfo(string message, string controller)
    {
        using var connection = InitConnection();
        connection.Execute("INSERT INTO Logs (Message,Controller) VALUES (@message, @controller);", new { message, controller });
    }

    public void ClearLogs()
    {
        using var connection = InitConnection();
        connection.Execute("DELETE FROM Logs;");
    }

    public IEnumerable<PemFile> GetPemFiles()
    {
        using var connection = InitConnection();
        return connection.Query<PemFile>("SELECT * FROM PemFiles ORDER BY Name;");
    }
    public void DeletePemFile(int id)
    {
        using var connection = InitConnection();
        connection.Execute("DELETE FROM PemFiles WHERE Id=@id;", new { id });
    }
    public void UpdatePemFiles(IEnumerable<PemFile> pemFiles)
    {
        if (pemFiles == null || pemFiles.Count() == 0)
            return;

        using var connection = InitConnection();
        var update = pemFiles.Where(p => p.Id > 0).ToArray();
        var insert = pemFiles.Where(p => p.Id == 0).ToArray();

        if (update.Length > 0)
            connection.Execute(
                "UPDATE PemFiles SET Name=@Name,PemText=@PemText,RsaSettings=@RsaSettings,PrivateKey=@PrivateKey " +
                "WHERE Id=@Id;", 
                update);

        if (insert.Length > 0)
            connection.Execute(
                "INSERT INTO PemFiles (Name,PemText,RsaSettings,PrivateKey) VALUES (@Name,@PemText,@RsaSettings,@PrivateKey);", 
                insert);
    }

    public async Task<string> GetLatestFileIdAsync(string filename = null)
    {
        using var connection = CreateConnection();
        if (string.IsNullOrWhiteSpace(filename))
        {
            return await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Id FROM AuditFiles ORDER BY SystemEntryDate DESC LIMIT 1;");
        }

        return await connection.QueryFirstOrDefaultAsync<string>(
            "SELECT Id FROM AuditFiles WHERE FileName = @filename ORDER BY SystemEntryDate DESC LIMIT 1;",
            new { filename });
    }

    private static T ParseEnum<T>(string value, T defaultValue = default) where T : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<T>(value, true, out var result))
            return result;
        return defaultValue;
    }

    public async Task<Header> GetHeaderAsync(string fileId)
    {
        using var connection = CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM Headers WHERE FileId = @fileId LIMIT 1;", new { fileId });

        if (row == null) return null;

        var header = new Header
        {
            Pk = row.FileId,
            AuditFileVersion = row.AuditFileVersion,
            CompanyID = row.CompanyID,
            TaxRegistrationNumber = row.TaxRegistrationNumber,
            TaxAccountingBasis = ParseEnum<TaxAccountingBasis>((string)row.TaxAccountingBasis, TaxAccountingBasis.INVALIDO),
            CompanyName = row.CompanyName,
            BusinessName = row.BusinessName,
            FiscalYear = row.FiscalYear,
            StartDate = row.StartDate != null ? DateTime.Parse(row.StartDate.ToString()) : DateTime.MinValue,
            EndDate = row.EndDate != null ? DateTime.Parse(row.EndDate.ToString()) : DateTime.MinValue,
            CurrencyCode = row.CurrencyCode,
            DateCreated = row.DateCreated != null ? DateTime.Parse(row.DateCreated.ToString()) : DateTime.MinValue,
            TaxEntity = row.TaxEntity,
            ProductCompanyTaxID = row.ProductCompanyTaxID,
            SoftwareCertificateNumber = row.SoftwareCertificateNumber,
            ProductID = row.ProductID,
            ProductVersion = row.ProductVersion,
            HeaderComment = row.HeaderComment,
            Telephone = row.Telephone,
            Fax = row.Fax,
            Email = row.Email,
            Website = row.Website
        };

        if (!string.IsNullOrWhiteSpace((string)row.CompanyAddressId))
        {
            var addrRow = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM AddressStructures WHERE Id = @Id;", new { Id = (string)row.CompanyAddressId });
            if (addrRow != null)
            {
                header.CompanyAddress = new AddressStructure
                {
                    BuildingNumber = addrRow.BuildingNumber,
                    StreetName = addrRow.StreetName,
                    AddressDetail = addrRow.AddressDetail,
                    City = addrRow.City,
                    PostalCode = addrRow.PostalCode,
                    Region = addrRow.Region,
                    Country = addrRow.Country
                };
            }
        }

        return header;
    }

    public async Task<PagedResult<SourceDocumentsSalesInvoicesInvoice>> GetInvoicesAsync(
        string fileId, int page, int pageSize, string filter = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = CreateConnection();
        var where = new System.Text.StringBuilder("WHERE FileId = @FileId");
        var parameters = new DynamicParameters();
        parameters.Add("FileId", fileId);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            where.Append(" AND (InvoiceNo LIKE @Filter OR CustomerID LIKE @Filter OR ATCUD LIKE @Filter OR Reason LIKE @Filter)");
            parameters.Add("Filter", $"%{filter}%");
        }
        if (startDate.HasValue)
        {
            where.Append(" AND InvoiceDate >= @StartDate");
            parameters.Add("StartDate", startDate.Value);
        }
        if (endDate.HasValue)
        {
            where.Append(" AND InvoiceDate <= @EndDate");
            parameters.Add("EndDate", endDate.Value);
        }

        int totalItems = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM Invoices {where}", parameters);

        int offset = Math.Max(0, (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        string query = $"SELECT * FROM Invoices {where} ORDER BY InvoiceDate DESC, InvoiceNo LIMIT @Limit OFFSET @Offset;";
        var rows = await connection.QueryAsync<dynamic>(query, parameters);

        var items = new List<SourceDocumentsSalesInvoicesInvoice>();
        foreach (var row in rows)
        {
            items.Add(new SourceDocumentsSalesInvoicesInvoice
            {
                Pk = row.Id,
                InvoiceNo = row.InvoiceNo,
                ATCUD = row.ATCUD,
                Hash = row.Hash,
                HashControl = row.HashControl,
                Period = row.Period,
                InvoiceDate = row.InvoiceDate != null ? DateTime.Parse(row.InvoiceDate.ToString()) : DateTime.MinValue,
                InvoiceType = ParseEnum<InvoiceType>((string)row.InvoiceType, InvoiceType.INVALIDO),
                SourceID = row.SourceID,
                EACCode = row.EACCode,
                SystemEntryDate = row.SystemEntryDate != null ? DateTime.Parse(row.SystemEntryDate.ToString()) : DateTime.MinValue,
                TransactionID = row.TransactionID,
                CustomerID = row.CustomerID,
                DocumentStatus = new SourceDocumentsSalesInvoicesInvoiceDocumentStatus
                {
                    InvoiceStatus = ParseEnum<InvoiceStatus>((string)row.InvoiceStatus, InvoiceStatus.INVALIDO),
                    InvoiceStatusDate = row.InvoiceStatusDate != null ? DateTime.Parse(row.InvoiceStatusDate.ToString()) : DateTime.MinValue,
                    Reason = row.Reason,
                    SourceID = row.StatusSourceID,
                    SourceBilling = ParseEnum<SAFTPTSourceBilling>((string)row.SourceBilling, SAFTPTSourceBilling.INVALIDO)
                },
                DocumentTotals = new SourceDocumentsSalesInvoicesInvoiceDocumentTotals
                {
                    TaxPayable = row.TaxPayable != null ? Convert.ToDecimal(row.TaxPayable) : 0m,
                    NetTotal = row.NetTotal != null ? Convert.ToDecimal(row.NetTotal) : 0m,
                    GrossTotal = row.GrossTotal != null ? Convert.ToDecimal(row.GrossTotal) : 0m
                }
            });
        }

        return new PagedResult<SourceDocumentsSalesInvoicesInvoice>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<IReadOnlyList<SourceDocumentsSalesInvoicesInvoiceLine>> GetInvoiceLinesAsync(string invoiceId)
    {
        using var connection = CreateConnection();
        var rows = await connection.QueryAsync<dynamic>(
            "SELECT * FROM InvoiceLines WHERE ParentId = @invoiceId ORDER BY CAST(LineNumber AS INTEGER);",
            new { invoiceId });

        var list = new List<SourceDocumentsSalesInvoicesInvoiceLine>();
        foreach (var row in rows)
        {
            decimal taxAmount = row.TaxAmount != null ? Convert.ToDecimal(row.TaxAmount) : 0m;
            decimal taxPercentage = row.TaxPercentage != null ? Convert.ToDecimal(row.TaxPercentage) : 0m;

            list.Add(new SourceDocumentsSalesInvoicesInvoiceLine
            {
                Pk = row.Id,
                LineNumber = row.LineNumber,
                ProductCode = row.ProductCode,
                ProductDescription = row.ProductDescription,
                Quantity = row.Quantity != null ? Convert.ToDecimal(row.Quantity) : 0m,
                UnitOfMeasure = row.UnitOfMeasure,
                UnitPrice = row.UnitPrice != null ? Convert.ToDecimal(row.UnitPrice) : 0m,
                TaxBase = row.TaxBase != null ? Convert.ToDecimal(row.TaxBase) : 0m,
                TaxPointDate = row.TaxPointDate != null ? DateTime.Parse(row.TaxPointDate.ToString()) : DateTime.MinValue,
                Description = row.Description,
                CreditAmount = row.CreditAmount != null ? Convert.ToDecimal(row.CreditAmount) : 0m,
                DebitAmount = row.DebitAmount != null ? Convert.ToDecimal(row.DebitAmount) : 0m,
                ItemElementName = ParseEnum<ItemChoiceType4>((string)row.ItemElementName, ItemChoiceType4.INVALIDO),
                TaxExemptionReason = row.TaxExemptionReason,
                TaxExemptionCode = row.TaxExemptionCode,
                SettlementAmount = row.SettlementAmount != null ? Convert.ToDecimal(row.SettlementAmount) : 0m,
                Tax = new Tax
                {
                    TaxType = ParseEnum<TaxType>((string)row.TaxType, TaxType.INVALIDO),
                    TaxCountryRegion = row.TaxCountryRegion,
                    TaxCode = row.TaxCode,
                    Item = taxPercentage > 0 ? taxPercentage : taxAmount,
                    ItemElementName = taxPercentage > 0 ? ItemChoiceType1.TaxPercentage : ItemChoiceType1.TaxAmount
                }
            });
        }

        return list;
    }

    public async Task<PagedResult<Customer>> GetCustomersAsync(string fileId, int page, int pageSize, string filter = null)
    {
        using var connection = CreateConnection();
        var where = new System.Text.StringBuilder("WHERE FileId = @FileId");
        var parameters = new DynamicParameters();
        parameters.Add("FileId", fileId);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            where.Append(" AND (CustomerID LIKE @Filter OR CompanyName LIKE @Filter OR CustomerTaxID LIKE @Filter)");
            parameters.Add("Filter", $"%{filter}%");
        }

        int totalItems = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM Customers {where}", parameters);

        int offset = Math.Max(0, (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var rows = await connection.QueryAsync<dynamic>($"SELECT * FROM Customers {where} ORDER BY CompanyName LIMIT @Limit OFFSET @Offset;", parameters);

        var items = new List<Customer>();
        foreach (var row in rows)
        {
            var customer = new Customer
            {
                Pk = row.Id,
                CustomerID = row.CustomerID,
                AccountID = row.AccountID,
                CustomerTaxID = row.CustomerTaxID,
                CompanyName = row.CompanyName,
                Contact = row.Contact,
                Telephone = row.Telephone,
                Fax = row.Fax,
                Email = row.Email,
                Website = row.Website,
                SelfBillingIndicator = row.SelfBillingIndicator
            };

            if (!string.IsNullOrWhiteSpace((string)row.BillingAddressId))
            {
                var addr = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM AddressStructures WHERE Id = @Id;", new { Id = (string)row.BillingAddressId });
                if (addr != null)
                {
                    customer.BillingAddress = new AddressStructure
                    {
                        BuildingNumber = addr.BuildingNumber,
                        StreetName = addr.StreetName,
                        AddressDetail = addr.AddressDetail,
                        City = addr.City,
                        PostalCode = addr.PostalCode,
                        Region = addr.Region,
                        Country = addr.Country
                    };
                }
            }

            items.Add(customer);
        }

        return new PagedResult<Customer>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<PagedResult<Product>> GetProductsAsync(string fileId, int page, int pageSize, string filter = null)
    {
        using var connection = CreateConnection();
        var where = new System.Text.StringBuilder("WHERE FileId = @FileId");
        var parameters = new DynamicParameters();
        parameters.Add("FileId", fileId);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            where.Append(" AND (ProductCode LIKE @Filter OR ProductDescription LIKE @Filter OR ProductGroup LIKE @Filter)");
            parameters.Add("Filter", $"%{filter}%");
        }

        int totalItems = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM Products {where}", parameters);

        int offset = Math.Max(0, (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var rows = await connection.QueryAsync<dynamic>($"SELECT * FROM Products {where} ORDER BY ProductDescription LIMIT @Limit OFFSET @Offset;", parameters);

        var items = new List<Product>();
        foreach (var row in rows)
        {
            items.Add(new Product
            {
                Pk = row.Id,
                ProductCode = row.ProductCode,
                ProductDescription = row.ProductDescription,
                ProductGroup = row.ProductGroup,
                ProductNumberCode = row.ProductNumberCode,
                ProductType = ParseEnum<ProductType>((string)row.ProductType, ProductType.P)
            });
        }

        return new PagedResult<Product>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<PagedResult<Supplier>> GetSuppliersAsync(string fileId, int page, int pageSize, string filter = null)
    {
        using var connection = CreateConnection();
        var where = new System.Text.StringBuilder("WHERE FileId = @FileId");
        var parameters = new DynamicParameters();
        parameters.Add("FileId", fileId);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            where.Append(" AND (SupplierID LIKE @Filter OR CompanyName LIKE @Filter OR SupplierTaxID LIKE @Filter)");
            parameters.Add("Filter", $"%{filter}%");
        }

        int totalItems = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM Suppliers {where}", parameters);

        int offset = Math.Max(0, (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var rows = await connection.QueryAsync<dynamic>($"SELECT * FROM Suppliers {where} ORDER BY CompanyName LIMIT @Limit OFFSET @Offset;", parameters);

        var items = new List<Supplier>();
        foreach (var row in rows)
        {
            items.Add(new Supplier
            {
                Pk = row.Id,
                SupplierID = row.SupplierID,
                AccountID = row.AccountID,
                SupplierTaxID = row.SupplierTaxID,
                CompanyName = row.CompanyName,
                Contact = row.Contact,
                Telephone = row.Telephone,
                Fax = row.Fax,
                Email = row.Email,
                Website = row.Website,
                SelfBillingIndicator = row.SelfBillingIndicator
            });
        }

        return new PagedResult<Supplier>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<PagedResult<SourceDocumentsMovementOfGoodsStockMovement>> GetStockMovementsAsync(string fileId, int page, int pageSize, string filter = null)
    {
        using var connection = CreateConnection();
        var where = new System.Text.StringBuilder("WHERE FileId = @FileId");
        var parameters = new DynamicParameters();
        parameters.Add("FileId", fileId);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            where.Append(" AND (DocumentNumber LIKE @Filter OR ATDocCodeID LIKE @Filter OR Reason LIKE @Filter)");
            parameters.Add("Filter", $"%{filter}%");
        }

        int totalItems = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM StockMovements {where}", parameters);

        int offset = Math.Max(0, (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var rows = await connection.QueryAsync<dynamic>($"SELECT * FROM StockMovements {where} ORDER BY MovementDate DESC, DocumentNumber LIMIT @Limit OFFSET @Offset;", parameters);

        var items = new List<SourceDocumentsMovementOfGoodsStockMovement>();
        foreach (var row in rows)
        {
            items.Add(new SourceDocumentsMovementOfGoodsStockMovement
            {
                Pk = row.Id,
                DocumentNumber = row.DocumentNumber,
                ATCUD = row.ATCUD,
                Hash = row.Hash,
                HashControl = row.HashControl,
                Period = row.Period,
                MovementDate = row.MovementDate != null ? DateTime.Parse(row.MovementDate.ToString()) : DateTime.MinValue,
                MovementType = ParseEnum<MovementType>((string)row.MovementType, MovementType.INVALIDO),
                SystemEntryDate = row.SystemEntryDate != null ? DateTime.Parse(row.SystemEntryDate.ToString()) : DateTime.MinValue,
                TransactionID = row.TransactionID,
                SourceID = row.SourceID,
                EACCode = row.EACCode,
                ATDocCodeID = row.ATDocCodeID,
                DocumentStatus = new SourceDocumentsMovementOfGoodsStockMovementDocumentStatus
                {
                    MovementStatus = ParseEnum<MovementStatus>((string)row.MovementStatus, MovementStatus.INVALIDO),
                    MovementStatusDate = row.MovementStatusDate != null ? DateTime.Parse(row.MovementStatusDate.ToString()) : DateTime.MinValue,
                    Reason = row.Reason,
                    SourceID = row.StatusSourceID,
                    SourceBilling = ParseEnum<SAFTPTSourceBilling>((string)row.SourceBilling, SAFTPTSourceBilling.INVALIDO)
                },
                DocumentTotals = new SourceDocumentsMovementOfGoodsStockMovementDocumentTotals
                {
                    TaxPayable = row.TaxPayable != null ? Convert.ToDecimal(row.TaxPayable) : 0m,
                    NetTotal = row.NetTotal != null ? Convert.ToDecimal(row.NetTotal) : 0m,
                    GrossTotal = row.GrossTotal != null ? Convert.ToDecimal(row.GrossTotal) : 0m
                }
            });
        }

        return new PagedResult<SourceDocumentsMovementOfGoodsStockMovement>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<IReadOnlyList<SourceDocumentsMovementOfGoodsStockMovementLine>> GetStockMovementLinesAsync(string documentId)
    {
        using var connection = CreateConnection();
        var rows = await connection.QueryAsync<dynamic>(
            "SELECT * FROM StockMovementLines WHERE ParentId = @documentId ORDER BY CAST(LineNumber AS INTEGER);",
            new { documentId });

        var list = new List<SourceDocumentsMovementOfGoodsStockMovementLine>();
        foreach (var row in rows)
        {
            list.Add(new SourceDocumentsMovementOfGoodsStockMovementLine
            {
                Pk = row.Id,
                LineNumber = row.LineNumber,
                ProductCode = row.ProductCode,
                ProductDescription = row.ProductDescription,
                Quantity = row.Quantity != null ? Convert.ToDecimal(row.Quantity) : 0m,
                UnitOfMeasure = row.UnitOfMeasure,
                UnitPrice = row.UnitPrice != null ? Convert.ToDecimal(row.UnitPrice) : 0m,
                Description = row.Description,
                CreditAmount = row.CreditAmount != null ? Convert.ToDecimal(row.CreditAmount) : 0m,
                DebitAmount = row.DebitAmount != null ? Convert.ToDecimal(row.DebitAmount) : 0m,
                ItemElementName = ParseEnum<ItemChoiceType6>((string)row.ItemElementName, ItemChoiceType6.INVALIDO),
                TaxExemptionReason = row.TaxExemptionReason,
                TaxExemptionCode = row.TaxExemptionCode,
                SettlementAmount = row.SettlementAmount != null ? Convert.ToDecimal(row.SettlementAmount) : 0m
            });
        }

        return list;
    }

    public async Task<PagedResult<SourceDocumentsWorkingDocumentsWorkDocument>> GetWorkDocumentsAsync(string fileId, int page, int pageSize, string filter = null)
    {
        using var connection = CreateConnection();
        var where = new System.Text.StringBuilder("WHERE FileId = @FileId");
        var parameters = new DynamicParameters();
        parameters.Add("FileId", fileId);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            where.Append(" AND (DocumentNumber LIKE @Filter OR CustomerID LIKE @Filter OR Reason LIKE @Filter)");
            parameters.Add("Filter", $"%{filter}%");
        }

        int totalItems = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM WorkDocuments {where}", parameters);

        int offset = Math.Max(0, (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var rows = await connection.QueryAsync<dynamic>($"SELECT * FROM WorkDocuments {where} ORDER BY WorkDate DESC, DocumentNumber LIMIT @Limit OFFSET @Offset;", parameters);

        var items = new List<SourceDocumentsWorkingDocumentsWorkDocument>();
        foreach (var row in rows)
        {
            items.Add(new SourceDocumentsWorkingDocumentsWorkDocument
            {
                Pk = row.Id,
                DocumentNumber = row.DocumentNumber,
                ATCUD = row.ATCUD,
                Hash = row.Hash,
                HashControl = row.HashControl,
                Period = row.Period,
                WorkDate = row.WorkDate != null ? DateTime.Parse(row.WorkDate.ToString()) : DateTime.MinValue,
                WorkType = ParseEnum<WorkType>((string)row.WorkType, WorkType.INVALIDO),
                SourceID = row.SourceID,
                EACCode = row.EACCode,
                SystemEntryDate = row.SystemEntryDate != null ? DateTime.Parse(row.SystemEntryDate.ToString()) : DateTime.MinValue,
                TransactionID = row.TransactionID,
                CustomerID = row.CustomerID,
                DocumentStatus = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus
                {
                    WorkStatus = ParseEnum<WorkStatus>((string)row.WorkStatus, WorkStatus.INVALIDO),
                    WorkStatusDate = row.WorkStatusDate != null ? DateTime.Parse(row.WorkStatusDate.ToString()) : DateTime.MinValue,
                    Reason = row.Reason,
                    SourceID = row.StatusSourceID,
                    SourceBilling = ParseEnum<SAFTPTSourceBilling>((string)row.SourceBilling, SAFTPTSourceBilling.INVALIDO)
                },
                DocumentTotals = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals
                {
                    TaxPayable = row.TaxPayable != null ? Convert.ToDecimal(row.TaxPayable) : 0m,
                    NetTotal = row.NetTotal != null ? Convert.ToDecimal(row.NetTotal) : 0m,
                    GrossTotal = row.GrossTotal != null ? Convert.ToDecimal(row.GrossTotal) : 0m
                }
            });
        }

        return new PagedResult<SourceDocumentsWorkingDocumentsWorkDocument>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<IReadOnlyList<SourceDocumentsWorkingDocumentsWorkDocumentLine>> GetWorkDocumentLinesAsync(string documentId)
    {
        using var connection = CreateConnection();
        var rows = await connection.QueryAsync<dynamic>(
            "SELECT * FROM WorkDocumentLines WHERE ParentId = @documentId ORDER BY CAST(LineNumber AS INTEGER);",
            new { documentId });

        var list = new List<SourceDocumentsWorkingDocumentsWorkDocumentLine>();
        foreach (var row in rows)
        {
            list.Add(new SourceDocumentsWorkingDocumentsWorkDocumentLine
            {
                Pk = row.Id,
                LineNumber = row.LineNumber,
                ProductCode = row.ProductCode,
                ProductDescription = row.ProductDescription,
                Quantity = row.Quantity != null ? Convert.ToDecimal(row.Quantity) : 0m,
                UnitOfMeasure = row.UnitOfMeasure,
                UnitPrice = row.UnitPrice != null ? Convert.ToDecimal(row.UnitPrice) : 0m,
                TaxBase = row.TaxBase != null ? Convert.ToDecimal(row.TaxBase) : 0m,
                TaxPointDate = row.TaxPointDate != null ? DateTime.Parse(row.TaxPointDate.ToString()) : DateTime.MinValue,
                Description = row.Description,
                CreditAmount = row.CreditAmount != null ? Convert.ToDecimal(row.CreditAmount) : 0m,
                DebitAmount = row.DebitAmount != null ? Convert.ToDecimal(row.DebitAmount) : 0m,
                ItemElementName = ParseEnum<ItemChoiceType7>((string)row.ItemElementName, ItemChoiceType7.INVALIDO),
                TaxExemptionReason = row.TaxExemptionReason,
                TaxExemptionCode = row.TaxExemptionCode,
                SettlementAmount = row.SettlementAmount != null ? Convert.ToDecimal(row.SettlementAmount) : 0m
            });
        }

        return list;
    }

    public async Task<PagedResult<SourceDocumentsPaymentsPayment>> GetPaymentsAsync(string fileId, int page, int pageSize, string filter = null)
    {
        using var connection = CreateConnection();
        var where = new System.Text.StringBuilder("WHERE FileId = @FileId");
        var parameters = new DynamicParameters();
        parameters.Add("FileId", fileId);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            where.Append(" AND (PaymentRefNo LIKE @Filter OR CustomerID LIKE @Filter OR Description LIKE @Filter)");
            parameters.Add("Filter", $"%{filter}%");
        }

        int totalItems = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM Payments {where}", parameters);

        int offset = Math.Max(0, (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", offset);

        var rows = await connection.QueryAsync<dynamic>($"SELECT * FROM Payments {where} ORDER BY TransactionDate DESC, PaymentRefNo LIMIT @Limit OFFSET @Offset;", parameters);

        var items = new List<SourceDocumentsPaymentsPayment>();
        foreach (var row in rows)
        {
            items.Add(new SourceDocumentsPaymentsPayment
            {
                Pk = row.Id,
                PaymentRefNo = row.PaymentRefNo,
                ATCUD = row.ATCUD,
                Period = row.Period,
                TransactionDate = row.TransactionDate != null ? DateTime.Parse(row.TransactionDate.ToString()) : DateTime.MinValue,
                PaymentType = ParseEnum<SAFTPTPaymentType>((string)row.PaymentType, SAFTPTPaymentType.INVALIDO),
                Description = row.Description,
                SystemID = row.SystemID,
                SourceID = row.SourceID,
                SystemEntryDate = row.SystemEntryDate != null ? DateTime.Parse(row.SystemEntryDate.ToString()) : DateTime.MinValue,
                CustomerID = row.CustomerID,
                DocumentStatus = new SourceDocumentsPaymentsPaymentDocumentStatus
                {
                    PaymentStatus = ParseEnum<PaymentStatus>((string)row.PaymentStatus, PaymentStatus.INVALIDO),
                    PaymentStatusDate = row.PaymentStatusDate != null ? DateTime.Parse(row.PaymentStatusDate.ToString()) : DateTime.MinValue,
                    Reason = row.Reason,
                    SourceID = row.StatusSourceID,
                    SourcePayment = ParseEnum<SAFTPTSourcePayment>((string)row.SourcePayment, SAFTPTSourcePayment.INVALIDO)
                },
                DocumentTotals = new SourceDocumentsPaymentsPaymentDocumentTotals
                {
                    TaxPayable = row.TaxPayable != null ? Convert.ToDecimal(row.TaxPayable) : 0m,
                    NetTotal = row.NetTotal != null ? Convert.ToDecimal(row.NetTotal) : 0m,
                    GrossTotal = row.GrossTotal != null ? Convert.ToDecimal(row.GrossTotal) : 0m
                }
            });
        }

        return new PagedResult<SourceDocumentsPaymentsPayment>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<IReadOnlyList<SourceDocumentsPaymentsPaymentLine>> GetPaymentLinesAsync(string paymentId)
    {
        using var connection = CreateConnection();
        var rows = await connection.QueryAsync<dynamic>(
            "SELECT * FROM PaymentLines WHERE ParentId = @paymentId ORDER BY CAST(LineNumber AS INTEGER);",
            new { paymentId });

        var list = new List<SourceDocumentsPaymentsPaymentLine>();
        foreach (var row in rows)
        {
            list.Add(new SourceDocumentsPaymentsPaymentLine
            {
                Pk = row.Id,
                LineNumber = row.LineNumber,
                SettlementAmount = row.SettlementAmount != null ? Convert.ToDecimal(row.SettlementAmount) : 0m,
                CreditAmount = row.CreditAmount != null ? Convert.ToDecimal(row.CreditAmount) : 0m,
                DebitAmount = row.DebitAmount != null ? Convert.ToDecimal(row.DebitAmount) : 0m,
                ItemElementName = ParseEnum<ItemChoiceType8>((string)row.ItemElementName, ItemChoiceType8.INVALIDO),
                TaxExemptionReason = row.TaxExemptionReason,
                TaxExemptionCode = row.TaxExemptionCode,
                OriginatingON = row.OriginatingON
            });
        }

        return list;
    }
}

