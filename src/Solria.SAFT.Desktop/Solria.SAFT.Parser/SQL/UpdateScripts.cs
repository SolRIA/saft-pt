using System.Linq;

namespace SolRIA.SAFT.Parser.SQL
{
    /// <summary>
    /// This class manages the db update scripts for the diferent app versions
    /// </summary>
    public class UpdateScripts
    {
        /// <summary>
        /// The current app version for the database
        /// </summary>
        public static int Version = 2;

        public static UpdateScript[] GetUpdateScripts(int version)
        {
            return BuildUpdateScripts().Where(s => s.Version > version).OrderBy(s => s.Order).ToArray();
        }

        private static UpdateScript[] BuildUpdateScripts()
        {
            return new UpdateScript[]
            {
                new UpdateScript { Version = 2, Order = 1, Sql = "CREATE TABLE IF NOT EXISTS Suppliers (Id TEXT, FileId TEXT, SupplierID TEXT, AccountID TEXT, SupplierTaxID TEXT, CompanyName TEXT, Contact TEXT, Telephone TEXT, Fax TEXT, Email TEXT, Website TEXT, SelfBillingIndicator TEXT, BillingAddressId TEXT, ShipFromAddress TEXT);" },
                new UpdateScript { Version = 2, Order = 2, Sql = "CREATE TABLE IF NOT EXISTS GeneralLedgerAccounts (Id TEXT, FileId TEXT, TaxonomyReference TEXT, AccountID TEXT, AccountDescription TEXT, OpeningDebitBalance DECIMAL, OpeningCreditBalance DECIMAL, ClosingDebitBalance DECIMAL, ClosingCreditBalance DECIMAL, GroupingCategory TEXT, GroupingCode TEXT, TaxonomyCode TEXT);" },
                new UpdateScript { Version = 2, Order = 3, Sql = "CREATE TABLE IF NOT EXISTS GeneralLedgerEntries (Id TEXT, FileId TEXT, NumberOfEntries TEXT, TotalDebit FLOAT, TotalCredit FLOAT);" },
                new UpdateScript { Version = 2, Order = 4, Sql = "CREATE TABLE IF NOT EXISTS GeneralLedgerJournals (Id TEXT, FileId TEXT, JournalID TEXT, Description TEXT);" },
                new UpdateScript { Version = 2, Order = 5, Sql = "CREATE TABLE IF NOT EXISTS GeneralLedgerTransactions (Id TEXT, JournalId TEXT, FileId TEXT, TransactionID TEXT, Period TEXT, TransactionDate TIMESTAMP, SourceID TEXT, Description TEXT, DocArchivalNumber TEXT, TransactionType TEXT, GLPostingDate TIMESTAMP, CustomerID TEXT, SupplierID TEXT);" },
                new UpdateScript { Version = 2, Order = 6, Sql = "CREATE TABLE IF NOT EXISTS GeneralLedgerTransactionLines (Id TEXT, TransactionId TEXT, RecordID TEXT, AccountID TEXT, SourceDocumentID TEXT, SystemEntryDate TIMESTAMP, Description TEXT, DebitAmount DECIMAL, CreditAmount DECIMAL);" },
                new UpdateScript { Version = 2, Order = 7, Sql = "CREATE INDEX IF NOT EXISTS IX_Invoices_FileId ON Invoices(FileId);" },
                new UpdateScript { Version = 2, Order = 8, Sql = "CREATE INDEX IF NOT EXISTS IX_Invoices_FileId_Date ON Invoices(FileId, InvoiceDate DESC);" },
                new UpdateScript { Version = 2, Order = 9, Sql = "CREATE INDEX IF NOT EXISTS IX_Invoices_InvoiceNo ON Invoices(InvoiceNo);" },
                new UpdateScript { Version = 2, Order = 10, Sql = "CREATE INDEX IF NOT EXISTS IX_InvoiceLines_ParentId ON InvoiceLines(ParentId);" },
                new UpdateScript { Version = 2, Order = 11, Sql = "CREATE INDEX IF NOT EXISTS IX_Customers_FileId ON Customers(FileId);" },
                new UpdateScript { Version = 2, Order = 12, Sql = "CREATE INDEX IF NOT EXISTS IX_Products_FileId ON Products(FileId);" },
                new UpdateScript { Version = 2, Order = 13, Sql = "CREATE INDEX IF NOT EXISTS IX_Suppliers_FileId ON Suppliers(FileId);" },
                new UpdateScript { Version = 2, Order = 14, Sql = "CREATE INDEX IF NOT EXISTS IX_StockMovements_FileId ON StockMovements(FileId);" },
                new UpdateScript { Version = 2, Order = 15, Sql = "CREATE INDEX IF NOT EXISTS IX_StockMovementLines_ParentId ON StockMovementLines(ParentId);" },
                new UpdateScript { Version = 2, Order = 16, Sql = "CREATE INDEX IF NOT EXISTS IX_WorkDocuments_FileId ON WorkDocuments(FileId);" },
                new UpdateScript { Version = 2, Order = 17, Sql = "CREATE INDEX IF NOT EXISTS IX_WorkDocumentLines_ParentId ON WorkDocumentLines(ParentId);" },
                new UpdateScript { Version = 2, Order = 18, Sql = "CREATE INDEX IF NOT EXISTS IX_Payments_FileId ON Payments(FileId);" },
                new UpdateScript { Version = 2, Order = 19, Sql = "CREATE INDEX IF NOT EXISTS IX_PaymentLines_ParentId ON PaymentLines(ParentId);" }
            };
        }

    }
}
