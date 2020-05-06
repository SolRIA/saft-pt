using System;
namespace Solria.SAFT.Desktop.Models.Saft
{
    public class AuditFile
    {
        public Header Header { get; set; }
        public AuditFileMasterFiles MasterFiles { get; set; }
        public GeneralLedgerEntries GeneralLedgerEntries { get; set; }
        public SourceDocuments SourceDocuments { get; set; }
    }
    public class Header
    {
        public string AuditFileVersion { get; set; }
        public string CompanyID { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string TaxAccountingBasis { get; set; }
        public string CompanyName { get; set; }
        public string BusinessName { get; set; }
        public AddressStructurePT CompanyAddress { get; set; }
        public string FiscalYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime DateCreated { get; set; }
        public string TaxEntity { get; set; }
        public string ProductCompanyTaxID { get; set; }
        public string SoftwareCertificateNumber { get; set; }
        public string ProductID { get; set; }
        public string ProductVersion { get; set; }
        public string HeaderComment { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
    }
    public class AddressStructurePT
    {
        public string BuildingNumber { get; set; }
        public string StreetName { get; set; }
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }
    public class WithholdingTax
    {
        public string WithholdingTaxType { get; set; }
        public string WithholdingTaxDescription { get; set; }
        public decimal WithholdingTaxAmount { get; set; }
    }
    public class Tax
    {
        public string TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
    }
    public class SupplierAddressStructure
    {
        public string BuildingNumber { get; set; }
        public string StreetName { get; set; }
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }
    public class SpecialRegimes
    {
        public string SelfBillingIndicator { get; set; }
        public string CashVATSchemeIndicator { get; set; }
        public string ThirdPartiesBillingIndicator { get; set; }
    }
    public class Settlement
    {
        public string SettlementDiscount { get; set; }
        public decimal? SettlementAmount { get; set; }
        public System.DateTime? SettlementDate { get; set; }
        public string PaymentTerms { get; set; }
    }
    public class References
    {
        public string Reference { get; set; }
        public string Reason { get; set; }
    }
    public class PaymentTax
    {
        public string TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
    }
    public class PaymentMethod
    {
        public string PaymentMechanism { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
    public class OrderReferences
    {
        public string OriginatingON { get; set; }
        public DateTime? OrderDate { get; set; }
    }
    public class MovementTax
    {
        public string TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public decimal TaxPercentage { get; set; }
    }
    public class CustomsInformation
    {
        public string[] ARCNo { get; set; }
        public decimal IECAmount { get; set; }
        public bool IECAmountSpecified { get; set; }
    }
    public class CustomsDetails
    {
        public string[] CNCode { get; set; }
        public string[] UNNumber { get; set; }
    }
    public class Currency
    {
        public string CurrencyCode { get; set; }
        public decimal CurrencyAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }
    public class AuditFileMasterFiles
    {
        public GeneralLedgerAccounts GeneralLedgerAccounts { get; set; }
        public Customer[] Customer { get; set; }
        public Supplier[] Supplier { get; set; }
        public Product[] Product { get; set; }
        public TaxTableEntry[] TaxTable { get; set; }
    }
    public class GeneralLedgerAccounts
    {
        public string TaxonomyReference { get; set; }
        public GeneralLedgerAccountsAccount[] Account { get; set; }
    }
    public class GeneralLedgerAccountsAccount
    {
        public string AccountID { get; set; }
        public string AccountDescription { get; set; }
        public decimal OpeningDebitBalance { get; set; }
        public decimal OpeningCreditBalance { get; set; }
        public decimal ClosingDebitBalance { get; set; }
        public decimal ClosingCreditBalance { get; set; }
        public string GroupingCategory { get; set; }
        public string GroupingCode { get; set; }
        public string TaxonomyCode { get; set; }
    }
    public class Customer
    {
        public string CustomerID { get; set; }
        public string AccountID { get; set; }
        public string CustomerTaxID { get; set; }
        public string CompanyName { get; set; }
        public string Contact { get; set; }
        public AddressStructure BillingAddress { get; set; }
        public AddressStructure[] ShipToAddress { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string SelfBillingIndicator { get; set; }
    }
    public class AddressStructure
    {
        public string BuildingNumber { get; set; }
        public string StreetName { get; set; }
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }
    public class Supplier
    {
        public string SupplierID { get; set; }
        public string AccountID { get; set; }
        public string SupplierTaxID { get; set; }
        public string CompanyName { get; set; }
        public string Contact { get; set; }
        public SupplierAddressStructure BillingAddress { get; set; }
        public SupplierAddressStructure[] ShipFromAddress { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string SelfBillingIndicator { get; set; }
    }
    public class Product
    {
        public string ProductType { get; set; }
        public string ProductCode { get; set; }
        public string ProductGroup { get; set; }
        public string ProductDescription { get; set; }
        public string ProductNumberCode { get; set; }
        public CustomsDetails CustomsDetails { get; set; }

        //calculated fileds
        public string Prices { get; set; }
        public string Taxes { get; set; }

    }
    public class TaxTableEntry
    {
        public string TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public string Description { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public string TaxExpirationDate { get; set; }
    }
    public class GeneralLedgerEntries
    {
        public string NumberOfEntries { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public GeneralLedgerEntriesJournal[] Journal { get; set; }
    }
    public class GeneralLedgerEntriesJournal
    {
        public string JournalID { get; set; }
        public string Description { get; set; }
        public GeneralLedgerEntriesJournalTransaction[] Transaction { get; set; }
    }
    public class GeneralLedgerEntriesJournalTransaction
    {
        public string TransactionID { get; set; }
        public string Period { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public string SourceID { get; set; }
        public string Description { get; set; }
        public string DocArchivalNumber { get; set; }
        public string TransactionType { get; set; }
        public System.DateTime GLPostingDate { get; set; }
        public string CustomerID { get; set; }
        public string SupplierID { get; set; }
        public GeneralLedgerEntriesJournalTransactionLines Lines { get; set; }
    }
    public class GeneralLedgerEntriesJournalTransactionLines
    {
        public GeneralLedgerEntriesJournalTransactionLinesDebitLine DebitLine { get; set; }
        public GeneralLedgerEntriesJournalTransactionLinesCreditLine CreditLine { get; set; }
    }
    public class GeneralLedgerEntriesJournalTransactionLinesDebitLine
    {
        public string RecordID { get; set; }
        public string AccountID { get; set; }
        public string SourceDocumentID { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string Description { get; set; }
        public decimal DebitAmount { get; set; }
    }
    public class GeneralLedgerEntriesJournalTransactionLinesCreditLine
    {
        public string RecordID { get; set; }
        public string AccountID { get; set; }
        public string SourceDocumentID { get; set; }
        public DateTime SystemEntryDate { get; set; }
        public string Description { get; set; }
        public decimal CreditAmount { get; set; }
    }
    public class SourceDocuments
    {
        public SourceDocumentsSalesInvoices SalesInvoices { get; set; }
        public SourceDocumentsMovementOfGoods MovementOfGoods { get; set; }
        public SourceDocumentsWorkingDocuments WorkingDocuments { get; set; }
        public SourceDocumentsPayments Payments { get; set; }
    }
    public class SourceDocumentsSalesInvoices
    {
        public string NumberOfEntries { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public SourceDocumentsSalesInvoicesInvoice[] Invoice { get; set; }
    }
    public class SourceDocumentsSalesInvoicesInvoice
    {
        public string InvoiceNo { get; set; }
        public string ATCUD { get; set; }
        public SourceDocumentsSalesInvoicesInvoiceDocumentStatus DocumentStatus { get; set; }
        public string Hash { get; set; }
        public string HashControl { get; set; }
        public string Period { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        public string InvoiceType { get; set; }
        public SpecialRegimes SpecialRegimes { get; set; }
        public string SourceID { get; set; }
        public string EACCode { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string TransactionID { get; set; }
        public string CustomerID { get; set; }
        public ShippingPointStructure ShipTo { get; set; }
        public ShippingPointStructure ShipFrom { get; set; }
        public System.DateTime? MovementEndTime { get; set; }
        public System.DateTime? MovementStartTime { get; set; }
        public SourceDocumentsSalesInvoicesInvoiceLine[] Line { get; set; }
        public SourceDocumentsSalesInvoicesInvoiceDocumentTotals DocumentTotals { get; set; }
        public WithholdingTax[] WithholdingTax { get; set; }
    }
    public class SourceDocumentsSalesInvoicesInvoiceDocumentStatus
    {
        public string InvoiceStatus { get; set; }
        public System.DateTime InvoiceStatusDate { get; set; }
        public string Reason { get; set; }
        public string SourceID { get; set; }
        public string SourceBilling { get; set; }
    }
    public class ShippingPointStructure
    {
        public string[] DeliveryID { get; set; }
        public System.DateTime? DeliveryDate { get; set; }
        public string[] WarehouseID { get; set; }
        public string[] LocationID { get; set; }
        public AddressStructure Address { get; set; }
    }
    public class SourceDocumentsSalesInvoicesInvoiceLine
    {
        public string LineNumber { get; set; }
        public OrderReferences[] OrderReferences { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? TaxBase { get; set; }
        public System.DateTime TaxPointDate { get; set; }
        public References[] References { get; set; }
        public string Description { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("SerialNumber", IsNullable = false)] public string[] ProductSerialNumber { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public Tax Tax { get; set; }
        public string TaxExemptionReason { get; set; }
        public string TaxExemptionCode { get; set; }
        public decimal SettlementAmount { get; set; }
        public bool SettlementAmountSpecified { get; set; }
        public CustomsInformation CustomsInformation { get; set; }
    }
    public class SourceDocumentsSalesInvoicesInvoiceDocumentTotals
    {
        public decimal TaxPayable { get; set; }
        public decimal NetTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public Currency Currency { get; set; }
        public Settlement[] Settlement { get; set; }
        public PaymentMethod[] Payment { get; set; }
    }
    public class SourceDocumentsMovementOfGoods
    {
        public string NumberOfMovementLines { get; set; }
        public decimal TotalQuantityIssued { get; set; }
        public SourceDocumentsMovementOfGoodsStockMovement[] StockMovement { get; set; }
    }
    public class SourceDocumentsMovementOfGoodsStockMovement
    {
        public string DocumentNumber { get; set; }
        public string ATCUD { get; set; }
        public SourceDocumentsMovementOfGoodsStockMovementDocumentStatus DocumentStatus { get; set; }
        public string Hash { get; set; }
        public string HashControl { get; set; }
        public string Period { get; set; }
        public System.DateTime MovementDate { get; set; }
        public string MovementType { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string TransactionID { get; set; }
        public string CustomerID { get; set; }
        public string SupplierID { get; set; }
        public string SourceID { get; set; }
        public string EACCode { get; set; }
        public string MovementComments { get; set; }
        public ShippingPointStructure ShipTo { get; set; }
        public ShippingPointStructure ShipFrom { get; set; }
        public System.DateTime? MovementEndTime { get; set; }
        public System.DateTime MovementStartTime { get; set; }
        public string ATDocCodeID { get; set; }
        public SourceDocumentsMovementOfGoodsStockMovementLine[] Line { get; set; }
        public SourceDocumentsMovementOfGoodsStockMovementDocumentTotals DocumentTotals { get; set; }
    }
    public class SourceDocumentsMovementOfGoodsStockMovementDocumentStatus
    {
        public string MovementStatus { get; set; }
        public System.DateTime MovementStatusDate { get; set; }
        public string Reason { get; set; }
        public string SourceID { get; set; }
        public string SourceBilling { get; set; }
    }
    public class SourceDocumentsMovementOfGoodsStockMovementLine
    {
        public string LineNumber { get; set; }
        public OrderReferences[] OrderReferences { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        public string Description { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("SerialNumber", IsNullable = false)] public string[] ProductSerialNumber { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public MovementTax Tax { get; set; }
        public string TaxExemptionReason { get; set; }
        public string TaxExemptionCode { get; set; }
        public decimal SettlementAmount { get; set; }
        public bool SettlementAmountSpecified { get; set; }
        public CustomsInformation CustomsInformation { get; set; }
    }
    public class SourceDocumentsMovementOfGoodsStockMovementDocumentTotals
    {
        public decimal TaxPayable { get; set; }
        public decimal NetTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public Currency Currency { get; set; }
    }
    public class SourceDocumentsWorkingDocuments
    {
        public string NumberOfEntries { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public SourceDocumentsWorkingDocumentsWorkDocument[] WorkDocument { get; set; }
    }
    public class SourceDocumentsWorkingDocumentsWorkDocument
    {
        public string DocumentNumber { get; set; }
        public string ATCUD { get; set; }
        public SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus DocumentStatus { get; set; }
        public string Hash { get; set; }
        public string HashControl { get; set; }
        public string Period { get; set; }
        public System.DateTime WorkDate { get; set; }
        public string WorkType { get; set; }
        public string SourceID { get; set; }
        public string EACCode { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string TransactionID { get; set; }
        public string CustomerID { get; set; }
        public SourceDocumentsWorkingDocumentsWorkDocumentLine[] Line { get; set; }
        public SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals DocumentTotals { get; set; }
    }
    public class SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus
    {
        public string WorkStatus { get; set; }
        public System.DateTime WorkStatusDate { get; set; }
        public string Reason { get; set; }
        public string SourceID { get; set; }
        public string SourceBilling { get; set; }
    }
    public class SourceDocumentsWorkingDocumentsWorkDocumentLine
    {
        public string LineNumber { get; set; }
        public OrderReferences[] OrderReferences { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxBase { get; set; }
        public bool TaxBaseSpecified { get; set; }
        public System.DateTime TaxPointDate { get; set; }
        public References[] References { get; set; }
        public string Description { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("SerialNumber", IsNullable = false)] public string[] ProductSerialNumber { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public Tax Tax { get; set; }
        public string TaxExemptionReason { get; set; }
        public string TaxExemptionCode { get; set; }
        public decimal SettlementAmount { get; set; }
        public bool SettlementAmountSpecified { get; set; }
        public CustomsInformation CustomsInformation { get; set; }
    }
    public class SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals
    {
        public decimal TaxPayable { get; set; }
        public decimal NetTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public Currency Currency { get; set; }
    }
    public class SourceDocumentsPayments
    {
        public string NumberOfEntries { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public SourceDocumentsPaymentsPayment[] Payment { get; set; }
    }
    public class SourceDocumentsPaymentsPayment
    {
        public string PaymentRefNo { get; set; }
        public string ATCUD { get; set; }
        public string Period { get; set; }
        public string TransactionID { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public string PaymentType { get; set; }
        public string Description { get; set; }
        public string SystemID { get; set; }
        public SourceDocumentsPaymentsPaymentDocumentStatus DocumentStatus { get; set; }
        public PaymentMethod[] PaymentMethod { get; set; }
        public string SourceID { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string CustomerID { get; set; }
        public SourceDocumentsPaymentsPaymentLine[] Line { get; set; }
        public SourceDocumentsPaymentsPaymentDocumentTotals DocumentTotals { get; set; }
        public WithholdingTax[] WithholdingTax { get; set; }
    }
    public class SourceDocumentsPaymentsPaymentDocumentStatus
    {
        public string PaymentStatus { get; set; }
        public System.DateTime PaymentStatusDate { get; set; }
        public string Reason { get; set; }
        public string SourceID { get; set; }
        public string SourcePayment { get; set; }
    }
    public class SourceDocumentsPaymentsPaymentLine
    {
        public string LineNumber { get; set; }
        public SourceDocumentsPaymentsPaymentLineSourceDocumentID[] SourceDocumentID { get; set; }
        public decimal SettlementAmount { get; set; }
        public bool SettlementAmountSpecified { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public PaymentTax Tax { get; set; }
        public string TaxExemptionReason { get; set; }
        public string TaxExemptionCode { get; set; }
    }
    public class SourceDocumentsPaymentsPaymentLineSourceDocumentID
    {
        public string OriginatingON { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        public string Description { get; set; }
    }
    public class SourceDocumentsPaymentsPaymentDocumentTotals
    {
        public decimal TaxPayable { get; set; }
        public decimal NetTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public SourceDocumentsPaymentsPaymentDocumentTotalsSettlement Settlement { get; set; }
        public Currency Currency { get; set; }
    }
    public class SourceDocumentsPaymentsPaymentDocumentTotalsSettlement
    {
        public decimal SettlementAmount { get; set; }
    }
    public class TaxTable
    {
        public TaxTableEntry[] TaxTableEntry { get; set; }
    }
}