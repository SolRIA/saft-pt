using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;

namespace SolRIA.SAFT.Parser.Services
{
    public interface IDatabaseService
    {
        void InitDatabase();
        string GetAppVersion();
        System.Data.Common.DbConnection CreateConnection();
        void LogException(Exception ex, string controller);
        void LogInfo(string message, string controller);
        void ClearLogs();
        IEnumerable<PemFile> GetPemFiles();
        void DeletePemFile(int id);
        void UpdatePemFiles(IEnumerable<PemFile> pemFiles);

        // SAF-T queries and pagination
        System.Threading.Tasks.Task<string> GetLatestFileIdAsync(string filename = null);
        System.Threading.Tasks.Task<Header> GetHeaderAsync(string fileId);
        System.Threading.Tasks.Task<PagedResult<SourceDocumentsSalesInvoicesInvoice>> GetInvoicesAsync(string fileId, int page, int pageSize, string filter = null, DateTime? startDate = null, DateTime? endDate = null);
        System.Threading.Tasks.Task<IReadOnlyList<SourceDocumentsSalesInvoicesInvoiceLine>> GetInvoiceLinesAsync(string invoiceId);
        System.Threading.Tasks.Task<PagedResult<Customer>> GetCustomersAsync(string fileId, int page, int pageSize, string filter = null);
        System.Threading.Tasks.Task<PagedResult<Product>> GetProductsAsync(string fileId, int page, int pageSize, string filter = null);
        System.Threading.Tasks.Task<PagedResult<Supplier>> GetSuppliersAsync(string fileId, int page, int pageSize, string filter = null);
        System.Threading.Tasks.Task<PagedResult<SourceDocumentsMovementOfGoodsStockMovement>> GetStockMovementsAsync(string fileId, int page, int pageSize, string filter = null);
        System.Threading.Tasks.Task<IReadOnlyList<SourceDocumentsMovementOfGoodsStockMovementLine>> GetStockMovementLinesAsync(string documentId);
        System.Threading.Tasks.Task<PagedResult<SourceDocumentsWorkingDocumentsWorkDocument>> GetWorkDocumentsAsync(string fileId, int page, int pageSize, string filter = null);
        System.Threading.Tasks.Task<IReadOnlyList<SourceDocumentsWorkingDocumentsWorkDocumentLine>> GetWorkDocumentLinesAsync(string documentId);
        System.Threading.Tasks.Task<PagedResult<SourceDocumentsPaymentsPayment>> GetPaymentsAsync(string fileId, int page, int pageSize, string filter = null);
        System.Threading.Tasks.Task<IReadOnlyList<SourceDocumentsPaymentsPaymentLine>> GetPaymentLinesAsync(string paymentId);
    }
}

