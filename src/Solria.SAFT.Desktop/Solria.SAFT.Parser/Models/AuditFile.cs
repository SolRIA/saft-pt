namespace SolRIA.SAFT.Parser.Models
{
    public partial class AuditFile
    {
        public Header Header { get; set; }
        public AuditFileMasterFiles MasterFiles { get; set; }
        public GeneralLedgerEntries GeneralLedgerEntries { get; set; }
        public SourceDocuments SourceDocuments { get; set; }
    }

    public partial class Header
    {
        public string AuditFileVersion { get; set; }
        public string CompanyID { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public TaxAccountingBasis TaxAccountingBasis { get; set; }
        public string CompanyName { get; set; }
        public string BusinessName { get; set; }
        public AddressStructure CompanyAddress { get; set; }
        public string FiscalYear { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public string CurrencyCode { get; set; }
        public System.DateTime DateCreated { get; set; }
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

    public enum TaxAccountingBasis { C, E, F, I, P, R, S, T, INVALIDO }

    public partial class WithholdingTax
    {
        public WithholdingTaxType WithholdingTaxType { get; set; }
        public string WithholdingTaxDescription { get; set; }
        public decimal WithholdingTaxAmount { get; set; }
    }

    public enum WithholdingTaxType { IRS, IRC, IS, INVALIDO }

    public partial class Tax
    {
        public TaxType TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public ItemChoiceType1 ItemElementName { get; set; }
    }

    public enum TaxType { IVA, IS, NS, INVALIDO }

    public enum ItemChoiceType1 { TaxAmount, TaxPercentage, INVALIDO }

    public partial class SupplierAddressStructure
    {
        public string BuildingNumber { get; set; }
        public string StreetName { get; set; }
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }

    public partial class SpecialRegimes
    {
        public string SelfBillingIndicator { get; set; }
        public string CashVATSchemeIndicator { get; set; }
        public string ThirdPartiesBillingIndicator { get; set; }
    }

    public partial class Settlement
    {
        public string SettlementDiscount { get; set; }
        public decimal SettlementAmount { get; set; }
        public System.DateTime SettlementDate { get; set; }
        public string PaymentTerms { get; set; }
    }

    public partial class References
    {
        public string Reference { get; set; }
        public string Reason { get; set; }
    }

    public partial class PaymentTax
    {
        public TaxType TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public ItemChoiceType ItemElementName { get; set; }
    }

    public enum ItemChoiceType { TaxAmount, TaxPercentage, INVALIDO }

    public partial class PaymentMethod
    {
        public PaymentMechanism PaymentMechanism { get; set; }
        public decimal PaymentAmount { get; set; }
        public System.DateTime PaymentDate { get; set; }
    }

    public enum PaymentMechanism { CC, CD, CH, CI, CO, CS, DE, LC, MB, NU, OU, PR, TB, TR, INVALIDO }

    public partial class OrderReferences
    {
        public string OriginatingON { get; set; }
        public System.DateTime OrderDate { get; set; }
    }

    public partial class MovementTax
    {
        public SAFTPTMovementTaxType TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public decimal TaxPercentage { get; set; }
    }

    public enum SAFTPTMovementTaxType { IVA, NS, INVALIDO }

    public partial class CustomsInformation
    {
        public string[] ARCNo { get; set; }
        public decimal IECAmount { get; set; }
    }

    public partial class CustomsDetails
    {
        public string[] CNCode { get; set; }
        public string[] UNNumber { get; set; }
    }

    public partial class Currency
    {
        public string CurrencyCode { get; set; }
        public decimal CurrencyAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }

    public partial class AuditFileMasterFiles
    {
        public GeneralLedgerAccounts GeneralLedgerAccounts { get; set; }
        public Customer[] Customer { get; set; }
        public Supplier[] Supplier { get; set; }
        public Product[] Product { get; set; }
        public TaxTableEntry[] TaxTable { get; set; }
    }

    public partial class GeneralLedgerAccounts
    {
        public TaxonomyReference TaxonomyReference { get; set; }
        public GeneralLedgerAccountsAccount[] Account { get; set; }
    }

    public enum TaxonomyReference { S, M, N, O, INVALIDO }

    public partial class GeneralLedgerAccountsAccount
    {
        public string AccountID { get; set; }
        public string AccountDescription { get; set; }
        public decimal OpeningDebitBalance { get; set; }
        public decimal OpeningCreditBalance { get; set; }
        public decimal ClosingDebitBalance { get; set; }
        public decimal ClosingCreditBalance { get; set; }
        public GroupingCategory GroupingCategory { get; set; }
        public string GroupingCode { get; set; }
        public string TaxonomyCode { get; set; }
    }

    public enum GroupingCategory { GR, GA, GM, AR, AA, AM, INVALIDO }

    public partial class Customer
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

    public partial class AddressStructure
    {
        public string BuildingNumber { get; set; }
        public string StreetName { get; set; }
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }

    public partial class Supplier
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

    public partial class Product
    {
        public ProductType ProductType { get; set; }
        public string ProductCode { get; set; }
        public string ProductGroup { get; set; }
        public string ProductDescription { get; set; }
        public string ProductNumberCode { get; set; }
        public CustomsDetails CustomsDetails { get; set; }
    }

    public enum ProductType { P, S, O, E, I, INVALIDO }

    public partial class TaxTableEntry
    {
        public TaxType TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public string Description { get; set; }
        public System.DateTime TaxExpirationDate { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public ItemChoiceType2 ItemElementName { get; set; }
    }

    public enum ItemChoiceType2 { TaxAmount, TaxPercentage, INVALIDO }

    public partial class GeneralLedgerEntries
    {
        public string NumberOfEntries { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public GeneralLedgerEntriesJournal[] Journal { get; set; }
    }

    public partial class GeneralLedgerEntriesJournal
    {
        public string JournalID { get; set; }
        public string Description { get; set; }
        public GeneralLedgerEntriesJournalTransaction[] Transaction { get; set; }
    }

    public partial class GeneralLedgerEntriesJournalTransaction
    {
        public string TransactionID { get; set; }
        public string Period { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public string SourceID { get; set; }
        public string Description { get; set; }
        public string DocArchivalNumber { get; set; }
        public TransactionType TransactionType { get; set; }
        public System.DateTime GLPostingDate { get; set; }
        public string Item { get; set; }
        public ItemChoiceType3 ItemElementName { get; set; }
        public GeneralLedgerEntriesJournalTransactionLines Lines { get; set; }
    }

    public enum TransactionType { N, R, A, J, INVALIDO }

    public enum ItemChoiceType3 { CustomerID, SupplierID, INVALIDO }

    public partial class GeneralLedgerEntriesJournalTransactionLines
    {
        public GeneralLedgerEntriesJournalTransactionLinesDebitLine DebitLine { get; set; }
        public GeneralLedgerEntriesJournalTransactionLinesCreditLine CreditLine { get; set; }
    }

    public partial class GeneralLedgerEntriesJournalTransactionLinesDebitLine
    {
        public string RecordID { get; set; }
        public string AccountID { get; set; }
        public string SourceDocumentID { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string Description { get; set; }
        public decimal DebitAmount { get; set; }
    }

    public partial class GeneralLedgerEntriesJournalTransactionLinesCreditLine
    {
        public string RecordID { get; set; }
        public string AccountID { get; set; }
        public string SourceDocumentID { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string Description { get; set; }
        public decimal CreditAmount { get; set; }
    }

    public partial class SourceDocuments
    {
        public SourceDocumentsSalesInvoices SalesInvoices { get; set; }
        public SourceDocumentsMovementOfGoods MovementOfGoods { get; set; }
        public SourceDocumentsWorkingDocuments WorkingDocuments { get; set; }
        public SourceDocumentsPayments Payments { get; set; }
    }

    public partial class SourceDocumentsSalesInvoices
    {
        public string NumberOfEntries { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public SourceDocumentsSalesInvoicesInvoice[] Invoice { get; set; }
    }

    public partial class SourceDocumentsSalesInvoicesInvoice
    {
        public string InvoiceNo { get; set; }
        public string ATCUD { get; set; }
        public SourceDocumentsSalesInvoicesInvoiceDocumentStatus DocumentStatus { get; set; }
        public string Hash { get; set; }
        public string HashControl { get; set; }
        public string Period { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        public InvoiceType InvoiceType { get; set; }
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

    public partial class SourceDocumentsSalesInvoicesInvoiceDocumentStatus
    {
        public InvoiceStatus InvoiceStatus { get; set; }
        public System.DateTime InvoiceStatusDate { get; set; }
        public string Reason { get; set; }
        public string SourceID { get; set; }
        public SAFTPTSourceBilling SourceBilling { get; set; }
    }

    public enum InvoiceStatus { N, S, A, R, F, INVALIDO }

    public enum SAFTPTSourceBilling { P, I, M, INVALIDO }

    public enum InvoiceType { FT, FS, FR, ND, NC, VD, TV, TD, AA, DA, RP, RE, CS, LD, RA, INVALIDO }

    public partial class ShippingPointStructure
    {
        public string[] DeliveryID { get; set; }
        public System.DateTime DeliveryDate { get; set; }
        public string[] WarehouseID { get; set; }
        public string[] LocationID { get; set; }
        public AddressStructure Address { get; set; }
    }

    public partial class SourceDocumentsSalesInvoicesInvoiceLine
    {
        public string LineNumber { get; set; }
        public OrderReferences[] OrderReferences { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxBase { get; set; }
        public System.DateTime TaxPointDate { get; set; }
        public References[] References { get; set; }
        public string Description { get; set; }
        public string[] ProductSerialNumber { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public ItemChoiceType4 ItemElementName { get; set; }
        public Tax Tax { get; set; }
        public string TaxExemptionReason { get; set; }
        public string TaxExemptionCode { get; set; }
        public decimal SettlementAmount { get; set; }
        public CustomsInformation CustomsInformation { get; set; }
    }

    public enum ItemChoiceType4 { CreditAmount, DebitAmount, INVALIDO }

    public partial class SourceDocumentsSalesInvoicesInvoiceDocumentTotals
    {
        public decimal TaxPayable { get; set; }
        public decimal NetTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public Currency Currency { get; set; }
        public Settlement[] Settlement { get; set; }
        public PaymentMethod[] Payment { get; set; }
    }

    public partial class SourceDocumentsMovementOfGoods
    {
        public string NumberOfMovementLines { get; set; }
        public decimal TotalQuantityIssued { get; set; }
        public SourceDocumentsMovementOfGoodsStockMovement[] StockMovement { get; set; }
    }

    public partial class SourceDocumentsMovementOfGoodsStockMovement
    {
        public string DocumentNumber { get; set; }
        public string ATCUD { get; set; }
        public SourceDocumentsMovementOfGoodsStockMovementDocumentStatus DocumentStatus { get; set; }
        public string Hash { get; set; }
        public string HashControl { get; set; }
        public string Period { get; set; }
        public System.DateTime MovementDate { get; set; }
        public MovementType MovementType { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string TransactionID { get; set; }
        public string Item { get; set; }
        public ItemChoiceType5 ItemElementName { get; set; }
        public string SourceID { get; set; }
        public string EACCode { get; set; }
        public string MovementComments { get; set; }
        public ShippingPointStructure ShipTo { get; set; }
        public ShippingPointStructure ShipFrom { get; set; }
        public System.DateTime MovementEndTime { get; set; }
        public System.DateTime MovementStartTime { get; set; }
        public string ATDocCodeID { get; set; }
        public SourceDocumentsMovementOfGoodsStockMovementLine[] Line { get; set; }
        public SourceDocumentsMovementOfGoodsStockMovementDocumentTotals DocumentTotals { get; set; }
    }

    public partial class SourceDocumentsMovementOfGoodsStockMovementDocumentStatus
    {
        public MovementStatus MovementStatus { get; set; }
        public System.DateTime MovementStatusDate { get; set; }
        public string Reason { get; set; }
        public string SourceID { get; set; }
        public SAFTPTSourceBilling SourceBilling { get; set; }
    }

    public enum MovementStatus { N, T, A, F, R, INVALIDO }

    public enum MovementType { GR, GT, GA, GC, GD, INVALIDO }

    public enum ItemChoiceType5 { CustomerID, SupplierID, INVALIDO }

    public partial class SourceDocumentsMovementOfGoodsStockMovementLine
    {
        public string LineNumber { get; set; }
        public OrderReferences[] OrderReferences { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        public string Description { get; set; }
        public string[] ProductSerialNumber { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public ItemChoiceType6 ItemElementName { get; set; }
        public MovementTax Tax { get; set; }
        public string TaxExemptionReason { get; set; }
        public string TaxExemptionCode { get; set; }
        public decimal SettlementAmount { get; set; }
        public CustomsInformation CustomsInformation { get; set; }
    }

    public enum ItemChoiceType6 { CreditAmount, DebitAmount, INVALIDO }

    public partial class SourceDocumentsMovementOfGoodsStockMovementDocumentTotals
    {
        public decimal TaxPayable { get; set; }
        public decimal NetTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public Currency Currency { get; set; }
    }

    public partial class SourceDocumentsWorkingDocuments
    {
        public string NumberOfEntries { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public SourceDocumentsWorkingDocumentsWorkDocument[] WorkDocument { get; set; }
    }

    public partial class SourceDocumentsWorkingDocumentsWorkDocument
    {
        public string DocumentNumber { get; set; }
        public string ATCUD { get; set; }
        public SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus DocumentStatus { get; set; }
        public string Hash { get; set; }
        public string HashControl { get; set; }
        public string Period { get; set; }
        public System.DateTime WorkDate { get; set; }
        public WorkType WorkType { get; set; }
        public string SourceID { get; set; }
        public string EACCode { get; set; }
        public System.DateTime SystemEntryDate { get; set; }
        public string TransactionID { get; set; }
        public string CustomerID { get; set; }
        public SourceDocumentsWorkingDocumentsWorkDocumentLine[] Line { get; set; }
        public SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals DocumentTotals { get; set; }
    }

    public partial class SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus
    {
        public WorkStatus WorkStatus { get; set; }
        public System.DateTime WorkStatusDate { get; set; }
        public string Reason { get; set; }
        public string SourceID { get; set; }
        public SAFTPTSourceBilling SourceBilling { get; set; }
    }

    public enum WorkStatus { N, A, F, INVALIDO }

    public enum WorkType { CM, CC, FC, FO, NE, OU, OR, PF, DC, RP, RE, CS, LD, RA, INVALIDO }

    public partial class SourceDocumentsWorkingDocumentsWorkDocumentLine
    {
        public string LineNumber { get; set; }
        public OrderReferences[] OrderReferences { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxBase { get; set; }
        public System.DateTime TaxPointDate { get; set; }
        public References[] References { get; set; }
        public string Description { get; set; }
        public string[] ProductSerialNumber { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public ItemChoiceType7 ItemElementName { get; set; }
        public Tax Tax { get; set; }
        public string TaxExemptionReason { get; set; }
        public string TaxExemptionCode { get; set; }
        public decimal SettlementAmount { get; set; }
        public CustomsInformation CustomsInformation { get; set; }
    }

    public enum ItemChoiceType7 { CreditAmount, DebitAmount, INVALIDO }

    public partial class SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals
    {
        public decimal TaxPayable { get; set; }
        public decimal NetTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public Currency Currency { get; set; }
    }

    public partial class SourceDocumentsPayments
    {
        public string NumberOfEntries { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public SourceDocumentsPaymentsPayment[] Payment { get; set; }
    }

    public partial class SourceDocumentsPaymentsPayment
    {
        public string PaymentRefNo { get; set; }
        public string ATCUD { get; set; }
        public string Period { get; set; }
        public string TransactionID { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public SAFTPTPaymentType PaymentType { get; set; }
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

    public enum SAFTPTPaymentType { RC, RG, INVALIDO }

    public partial class SourceDocumentsPaymentsPaymentDocumentStatus
    {
        public PaymentStatus PaymentStatus { get; set; }
        public System.DateTime PaymentStatusDate { get; set; }
        public string Reason { get; set; }
        public string SourceID { get; set; }
        public SAFTPTSourcePayment SourcePayment { get; set; }
    }

    public enum PaymentStatus { N, A, INVALIDO }

    public enum SAFTPTSourcePayment { P, I, M, INVALIDO }

    public partial class SourceDocumentsPaymentsPaymentLine
    {
        public string LineNumber { get; set; }
        public SourceDocumentsPaymentsPaymentLineSourceDocumentID[] SourceDocumentID { get; set; }
        public decimal SettlementAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public ItemChoiceType8 ItemElementName { get; set; }
        public PaymentTax Tax { get; set; }
        public string TaxExemptionReason { get; set; }
        public string TaxExemptionCode { get; set; }
    }

    public partial class SourceDocumentsPaymentsPaymentLineSourceDocumentID
    {
        public string OriginatingON { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        public string Description { get; set; }
    }

    public enum ItemChoiceType8 { CreditAmount, DebitAmount, INVALIDO }

    public partial class SourceDocumentsPaymentsPaymentDocumentTotals
    {
        public decimal TaxPayable { get; set; }
        public decimal NetTotal { get; set; }
        public decimal GrossTotal { get; set; }
        public SourceDocumentsPaymentsPaymentDocumentTotalsSettlement Settlement { get; set; }
        public Currency Currency { get; set; }
    }

    public partial class SourceDocumentsPaymentsPaymentDocumentTotalsSettlement
    {
        public decimal SettlementAmount { get; set; }
    }

    public partial class TaxTable
    {
        public TaxTableEntry[] TaxTableEntry { get; set; }
    }
}
