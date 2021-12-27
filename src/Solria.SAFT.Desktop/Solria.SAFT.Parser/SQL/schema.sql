CREATE TABLE IF NOT EXISTS `PemFiles` (
	`Id`			INTEGER PRIMARY KEY AUTOINCREMENT,
	`Name`			VARCHAR,
	`PrivateKey`	BIT,
	`PemText`		VARCHAR,
	`RsaSettings`	VARCHAR
);

CREATE TABLE IF NOT EXISTS `RecentFiles` (
	`Id`        INTEGER PRIMARY KEY AUTOINCREMENT,
	`FileName`  VARCHAR,
	`LastUsed`  TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS `DatabaseVersion` (
	`Id`          INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	`AppVersion`  VARCHAR,
	`UpgradeDate` TIMESTAMP,
	`Version`     INTEGER
);

CREATE TABLE IF NOT EXISTS `Logs` (
	`Id` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	`Message` VARCHAR NOT NULL,
	`Controller` VARCHAR NOT NULL,
	`SystemEntryDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS AuditFiles (
    Id       TEXT PRIMARY KEY,
    FileName TEXT NOT NULL,
	`SystemEntryDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Headers (
    FileId                    TEXT PRIMARY KEY,
    AuditFileVersion          TEXT,
    CompanyID                 TEXT,
    TaxRegistrationNumber     TEXT,
    TaxAccountingBasis        TEXT,
    CompanyName               TIME,
    BusinessName              TIME,
    FiscalYear                TEXT,
    StartDate                 DATETIME,
    EndDate                   DATETIME,
    CurrencyCode              DATETIME,
    DateCreated               DATETIME,
    TaxEntity                 TEXT,
    ProductCompanyTaxID       TEXT,
    SoftwareCertificateNumber TEXT,
    ProductID                 TEXT,
    ProductVersion            TEXT,
    HeaderComment             TEXT,
    Telephone                 TEXT,
    Fax                       TEXT,
    Email                     TEXT,
    Website                   TEXT,
    CompanyAddressId          TEXT
);

CREATE TABLE IF NOT EXISTS AddressStructures (
    Id             TEXT PRIMARY KEY NOT NULL,
    FileId         TEXT NOT NULL,
    BuildingNumber TEXT,
    StreetName     TEXT,
    AddressDetail  TEXT,
    City           TEXT,
    PostalCode     TEXT,
    Region         TEXT,
    Country        TEXT
);

CREATE TABLE IF NOT EXISTS WithholdingTaxs (
    Id                        TEXT,
    FileId                    TEXT,
    WithholdingTaxType        TEXT,
    WithholdingTaxDescription TEXT,
    WithholdingTaxAmount      DECIMAL
);

CREATE TABLE IF NOT EXISTS Taxes (
    Id               TEXT,
    FileId           TEXT,
    TaxType          TEXT,
    TaxCountryRegion TEXT,
    TaxCode          TEXT,
    TaxAmount        DECIMAL,
    TaxPercentage    DECIMAL,
    ItemElementName  TEXT
);

CREATE TABLE IF NOT EXISTS Customers (
    Id                      TEXT,
    FileId                  TEXT,
    CustomerID              TEXT,
    AccountID               TEXT,
    CustomerTaxID           TEXT,
    CompanyName             TEXT,
    Contact                 TEXT,
    Telephone               TEXT,
    Fax                     TEXT,
    Email                   TEXT,
    Website                 TEXT,
    SelfBillingIndicator    TEXT,
    BillingAddressId        TEXT,
    ShipToAddress           TEXT
);

CREATE TABLE IF NOT EXISTS Products (
    Id                  TEXT,
    FileId              TEXT,
    ProductType         TEXT,
    ProductCode         TEXT,
    ProductGroup        TEXT,
    ProductDescription  TEXT,
    ProductNumberCode   TEXT,
    CustomsDetails      TEXT
);

CREATE TABLE IF NOT EXISTS InfoInvoices (
    Id              TEXT,
    FileId          TEXT,
    NumberOfEntries TEXT,
    TotalDebit      FLOAT,
    TotalCredit     FLOAT
);

CREATE TABLE IF NOT EXISTS Invoices (
    Id                  TEXT,
    FileId              TEXT,
    -- DocumentStatus
    InvoiceStatus       TEXT,
    InvoiceStatusDate   TIMESTAMP,
    Reason              TEXT,
    StatusSourceID      TEXT,
    SourceBilling       TEXT,
    -- DocumentTotals
    TaxPayable          FLOAT,
    NetTotal            FLOAT,
    GrossTotal          FLOAT,
    Currency            TEXT,
    Settlement          TEXT,
    Payment             TEXT,
    -- WithholdingTax JSON
    WithholdingTax      TEXT,
    -- SpecialRegimes JSON
    SpecialRegimes      TEXT,
    InvoiceNo           TEXT,
    ATCUD               TEXT,
    Hash                TEXT,
    HashControl         TEXT,
    Period              TEXT,
    InvoiceDate         TIMESTAMP,
    InvoiceType         TEXT,
    SourceID            TEXT,
    EACCode             TEXT,
    SystemEntryDate     TIMESTAMP,
    TransactionID       TEXT,
    CustomerID          TEXT,
    -- ShipTo JSON
    ShipTo              TEXT,
    -- ShipFrom JSON
    ShipFrom            TEXT,
    MovementEndTime     TIMESTAMP,
    MovementStartTime   TIMESTAMP
);

CREATE TABLE IF NOT EXISTS InvoiceLines (
    Id                  TEXT,
    ParentId            TEXT,
    LineNumber          TEXT,
    ProductCode         TEXT,
    ProductDescription  TEXT,
    Quantity            FLOAT,
    UnitOfMeasure       TEXT,
    UnitPrice           FLOAT,
    TaxBase             FLOAT,
    TaxPointDate        TIMESTAMP,
    Description         TEXT,
    CreditAmount        FLOAT,
    DebitAmount         FLOAT,
    ItemElementName     TEXT,
    TaxExemptionReason  TEXT,
    TaxExemptionCode    TEXT,
    SettlementAmount    FLOAT,
    -- Tax 
    TaxType             TEXT,
    TaxCountryRegion    TEXT,
    TaxCode             TEXT,
    TaxAmount           FLOAT,
    TaxPercentage       FLOAT,
    TaxItemElementName  TEXT,
    -- ProductSerialNumber JSON
    ProductSerialNumber TEXT,
    -- References JSON
    `References`        TEXT,
    -- OrderReferences JSON
    OrderReferences     TEXT,
    -- CustomsInformation JSON
    CustomsInformation  TEXT
);

CREATE TABLE IF NOT EXISTS InfoStockMovements (
    Id                      TEXT,
    FileId                  TEXT,
    NumberOfMovementLines   TEXT,
    TotalQuantityIssued     FLOAT
);

CREATE TABLE IF NOT EXISTS StockMovements (
    Id                  TEXT,
    FileId              TEXT,
    DocumentNumber      TEXT,
    ATCUD               TEXT,
    Hash                TEXT,
    HashControl         TEXT,
    Period              TEXT,
    MovementDate        TIMESTAMP,
    MovementType        TEXT,
    SystemEntryDate     TIMESTAMP,
    TransactionID       TEXT,
    Item                TEXT,
    ItemElementName     TEXT,
    SourceID            TEXT,
    EACCode             TEXT,
    MovementComments    TEXT,
    MovementEndTime     TIMESTAMP,
    MovementStartTime   TIMESTAMP,
    ATDocCodeID         TEXT,
    -- DocumentStatus
    MovementStatus      TEXT,
    MovementStatusDate  TIMESTAMP,
    Reason              TEXT,
    StatusSourceID      TEXT,
    SourceBilling       TEXT,
    -- DocumentTotals
    TaxPayable          FLOAT,
    NetTotal            FLOAT,
    GrossTotal          FLOAT,
    Currency            TEXT,
    -- ShipTo JSON
    ShipTo              TEXT,
    -- ShipFrom JSON
    ShipFrom            TEXT
);

CREATE TABLE IF NOT EXISTS StockMovementLines (
    Id                  TEXT,
    ParentId            TEXT,
    LineNumber          TEXT,
    ProductCode         TEXT,
    ProductDescription  TEXT,
    Quantity            FLOAT,
    UnitOfMeasure       TEXT,
    UnitPrice           FLOAT,
    Description         TEXT,
    CreditAmount        FLOAT,
    DebitAmount         FLOAT,
    ItemElementName     TEXT,
    TaxExemptionReason  TEXT,
    TaxExemptionCode    TEXT,
    SettlementAmount    FLOAT,
    -- Tax JSON
    Tax                 TEXT,
    -- ProductSerialNumber JSON
    ProductSerialNumber TEXT,
    -- OrderReferences JSON
    OrderReferences     TEXT,
    -- CustomsInformation JSON
    CustomsInformation  TEXT
);

CREATE TABLE IF NOT EXISTS InfoWorkDocuments (
    Id                  TEXT,
    FileId              TEXT,
    NumberOfEntries     TEXT,
    TotalDebit          FLOAT,
    TotalCredit         FLOAT
);

CREATE TABLE IF NOT EXISTS WorkDocuments (
    Id              TEXT,
    FileId          TEXT,
    DocumentNumber  TEXT,
    ATCUD           TEXT,
    Hash            TEXT,
    HashControl     TEXT,
    Period          TEXT,
    WorkDate        TIMESTAMP,
    WorkType        TEXT,
    SourceID        TEXT,
    EACCode         TEXT,
    SystemEntryDate TIMESTAMP,
    TransactionID   TEXT,
    CustomerID      TEXT,
    -- DocumentStatus
    WorkStatus      TEXT,
    WorkStatusDate  TIMESTAMP,
    Reason          TEXT,
    StatusSourceID  TEXT,
    SourceBilling   TEXT,
    -- DocumentTotals
    TaxPayable      FLOAT,
    NetTotal        FLOAT,
    GrossTotal      FLOAT,
    Currency        TEXT
);

CREATE TABLE IF NOT EXISTS WorkDocumentLines (
    Id                  TEXT,
    ParentId            TEXT,
    LineNumber          TEXT,
    ProductCode         TEXT,
    ProductDescription  TEXT,
    Quantity            FLOAT,
    UnitOfMeasure       TEXT,
    UnitPrice           FLOAT,
    TaxBase             FLOAT,
    TaxPointDate        TIMESTAMP,
    Description         TEXT,
    CreditAmount        FLOAT,
    DebitAmount         FLOAT,
    ItemElementName     TEXT,
    TaxExemptionReason  TEXT,
    TaxExemptionCode    TEXT,
    SettlementAmount    FLOAT,
    -- Tax JSON
    Tax                 TEXT,
    -- References JSON
    `References`        TEXT,
    -- ProductSerialNumber JSON
    ProductSerialNumber TEXT,
    -- OrderReferences JSON
    OrderReferences     TEXT,
    -- CustomsInformation JSON
    CustomsInformation  TEXT
);

CREATE TABLE IF NOT EXISTS InfoPayments (
    Id                  TEXT,
    FileId              TEXT,
    NumberOfEntries     TEXT,
    TotalDebit          FLOAT,
    TotalCredit         FLOAT
);

CREATE TABLE IF NOT EXISTS Payments (
    Id                  TEXT,
    FileId              TEXT,
    PaymentRefNo        TEXT,
    ATCUD               TEXT,
    Period              TEXT,
    TransactionID       TEXT,
    TransactionDate     TIMESTAMP,
    PaymentType         TEXT,
    Description         TEXT,
    SystemID            TEXT,
    SourceID            TEXT,
    SystemEntryDate     TIMESTAMP,
    CustomerID          TEXT,
    -- DocumentStatus
    PaymentStatus       TEXT,
    PaymentStatusDate   TIMESTAMP,
    Reason              TEXT,
    StatusSourceID      TEXT,
    SourcePayment       TEXT,
    -- DocumentTotals
    TaxPayable          FLOAT,
    NetTotal            FLOAT,
    GrossTotal          FLOAT,
    -- Settlement JSON
    Settlement          TEXT,
    -- Currency
    Currency            TEXT,
    -- PaymentMethod
    PaymentMethod       TEXT,
    -- WithholdingTax
    WithholdingTax      TEXT
);

CREATE TABLE IF NOT EXISTS PaymentLines (
    Id                  TEXT,
    ParentId            TEXT,
    LineNumber          TEXT,
    SettlementAmount    FLOAT,
    CreditAmount        FLOAT,
    DebitAmount         FLOAT,
    ItemElementName     TEXT,
    TaxExemptionReason  TEXT,
    TaxExemptionCode    TEXT,
    -- SourceDocumentID
    OriginatingON       TEXT,
    InvoiceDate         TIMESTAMP,
    Description         TEXT,
    -- Tax JSON
    Tax                 TEXT
);