using System;

namespace SolRIA.SAFT.Parser.Models
{
    public sealed class SchemaResults { }
    public sealed class HashResults { }

    public class ValidationError
    {
        public string Description { get; set; }

        public Type TypeofError { get; set; }

        public string Field { get; set; }

        public string Value { get; set; }

        public ValidationErrorType ValidationType { get; set; } = ValidationErrorType.LEGAL;

        /// <summary>
        /// Unique ID, to identify the register with error
        /// </summary>
        public string FileID { get; set; }
        public string UID { get; set; }

        public string SupUID { get; set; }

        public string GranSupUID { get; set; }

        string displayName;
        public string DisplayName
        {
            get
            {
                if (TypeofError == null)
                    displayName = "??";
                else
                {
                    displayName = TypeofError.Name switch
                    {
                        "SourceDocumentsSalesInvoicesInvoice" => "Documentos facturação",
                        "SourceDocumentsSalesInvoices" => "Documentos facturação",
                        "SourceDocumentsWorkingDocuments" => "Documentos de conferência",
                        "SourceDocumentsWorkingDocumentsWorkDocument" => "Documentos de conferência",
                        "SourceDocumentsWorkingDocumentsWorkDocumentLine" => "Linhas documentos de conferência",
                        "SourceDocumentsMovementOfGoods" => "Mercadorias",
                        "SourceDocumentsMovementOfGoodsStockMovement" => "Mercadorias",
                        "SourceDocumentsMovementOfGoodsStockMovementLine" => "Linhas mercadorias",
                        "SourceDocumentsPayments" => "Recibos",
                        "SourceDocumentsPaymentsPayment" => "Recibos",
                        "SourceDocumentsPaymentsPaymentLine" => "Linhas recibos",
                        "SourceDocumentsPaymentsPaymentDocumentTotals" => "Recibos",
                        "SourceDocumentsSalesInvoicesInvoiceLine" => "Linhas documentos de facturação",
                        "Product" => "Produtos",
                        "Stock" => "Existências",
                        "StockHeader" => "Existências",
                        "Customer" => "Clientes",
                        "Header" => "Cabeçalho",
                        "Tax" => "Impostos",
                        "TaxTableEntry" => "Impostos",
                        "Supplier" => "Fornecedores",
                        "GeneralLedgerAccounts" => "Plano de contas",
                        "GeneralLedgerAccountsAccount" => "Plano de contas",
                        "GeneralLedgerEntries" => "Movimentos contabilísticos",
                        "GeneralLedgerEntriesJournal" => "Movimentos contabilísticos",
                        "GeneralLedgerEntriesJournalTransaction" => "Movimentos contabilísticos",
                        "GeneralLedgerEntriesJournalTransactionLinesDebitLine" => "Movimentos contabilísticos",
                        "GeneralLedgerEntriesJournalTransactionLinesCreditLine" => "Movimentos contabilísticos",
                        "SchemaResults" => "Schema",
                        "HashResults" => "Assinaturas documentos",
                        _ => "??",
                    };
                }

                return displayName;
            }

            private set { displayName = value; }
        }
    }
}
