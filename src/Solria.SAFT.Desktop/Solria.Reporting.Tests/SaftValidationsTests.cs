using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser;
using SolRIA.SAFT.Parser.Models;
using SolRIA.SAFT.Parser.Services;
using System.Security.Cryptography;
using System.Text;
using LegacyAuditFile = SolRIA.SAFT.Desktop.Models.SaftV4.AuditFile;

internal static class SaftValidationsTests
{
    public static async Task VerifyAsync()
    {
        VerifyNifValidation();
        await VerifyCoreFixesAndBothParsersAsync();
        await VerifyWorkingDocumentsAndMovementsAsync();
        await VerifyPaymentsExtendedRulesAsync();
        await VerifyReferentialAndTaxExemptionRulesAsync();
        await VerifyTemporalAndMovementFutureDatesAsync();
        await VerifyGeneralLedgerValidationsAsync();
        await VerifyHashChainAndSchemaStreamingAsync();
        await VerifyStockFileValidationsAsync();
        Console.WriteLine("PASS: comprehensive SAFT & Stock validations");
    }

    private static void VerifyNifValidation()
    {
        Check(Validations.CheckTaxRegistrationNumber("123456789"), "123456789 must be valid.");
        Check(Validations.CheckTaxRegistrationNumber("999999990"), "999999990 (Consumidor final) must be valid.");
        Check(Validations.CheckTaxRegistrationNumber("501234560"), "501234560 must be valid.");
        Check(Validations.CheckTaxRegistrationNumber("450000001"), "450000001 must be valid.");

        Check(!Validations.CheckTaxRegistrationNumber("000000000"), "000000000 must be invalid.");
        Check(!Validations.CheckTaxRegistrationNumber("400000008"), "400000008 (invalid prefix) must be invalid.");
        Check(!Validations.CheckTaxRegistrationNumber("501234567"), "501234567 (bad check digit) must be invalid.");
        Check(!Validations.CheckTaxRegistrationNumber("12345678"), "8-digit NIF must be invalid.");
        Check(!Validations.CheckTaxRegistrationNumber("12345678A"), "Non-numeric NIF must be invalid.");
        Check(!Validations.CheckTaxRegistrationNumber(""), "Empty NIF must be invalid.");
    }

    private static async Task VerifyCoreFixesAndBothParsersAsync()
    {
        var filename = Path.GetTempFileName();
        try
        {
            var longAddress = new string('A', 150); // >100 (1.03 limit) and <=210 (1.04_01 limit)
            await File.WriteAllTextAsync(filename, $"""
                <?xml version="1.0" encoding="utf-8"?>
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <Header>
                    <AuditFileVersion>1.04_01</AuditFileVersion>
                    <CompanyID>501234560</CompanyID>
                    <TaxRegistrationNumber>501234560</TaxRegistrationNumber>
                    <TaxAccountingBasis>F</TaxAccountingBasis>
                    <CompanyName>Empresa Teste Lda</CompanyName>
                    <CompanyAddress>
                      <AddressDetail>{longAddress}</AddressDetail>
                      <City>Lisboa</City>
                      <PostalCode>1000-001</PostalCode>
                      <Country>PT</Country>
                    </CompanyAddress>
                    <FiscalYear>2026</FiscalYear>
                    <StartDate>2026-01-01</StartDate>
                    <EndDate>2026-01-31</EndDate>
                    <CurrencyCode>EUR</CurrencyCode>
                    <DateCreated>2026-02-01</DateCreated>
                    <TaxEntity>Sede</TaxEntity>
                    <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
                    <SoftwareCertificateNumber>9999</SoftwareCertificateNumber>
                    <ProductID>AppTeste/SolRIA</ProductID>
                    <ProductVersion>1.0</ProductVersion>
                    <Email>contacto@empresa.cloud</Email>
                  </Header>
                  <MasterFiles>
                    <Customer>
                      <CustomerID>C1</CustomerID>
                      <AccountID>2111</AccountID>
                      <CustomerTaxID>999999990</CustomerTaxID>
                      <CompanyName>Consumidor final</CompanyName>
                      <BillingAddress>
                        <AddressDetail>Desconhecido</AddressDetail>
                        <City>Desconhecido</City>
                        <PostalCode>Desconhecido</PostalCode>
                        <Country>Desconhecido</Country>
                      </BillingAddress>
                      <SelfBillingIndicator>0</SelfBillingIndicator>
                    </Customer>
                    <Product>
                      <ProductType>P</ProductType>
                      <ProductCode>P1</ProductCode>
                      <ProductDescription>Produto 1</ProductDescription>
                      <ProductNumberCode>P1</ProductNumberCode>
                    </Product>
                    <TaxTable>
                      <TaxTableEntry>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>NOR</TaxCode>
                        <Description>IVA Normal</Description>
                        <TaxPercentage>23.00</TaxPercentage>
                      </TaxTableEntry>
                      <TaxTableEntry>
                        <TaxType>IS</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>OUT</TaxCode>
                        <Description>Imposto Selo</Description>
                        <TaxAmount>5.00</TaxAmount>
                      </TaxTableEntry>
                    </TaxTable>
                  </MasterFiles>
                  <SourceDocuments>
                    <SalesInvoices>
                      <NumberOfEntries>2</NumberOfEntries>
                      <TotalDebit>0.00</TotalDebit>
                      <TotalCredit>100.00</TotalCredit>
                      <Invoice>
                        <InvoiceNo>FT 2026-A/1</InvoiceNo>
                        <ATCUD>ABCD1234-1</ATCUD>
                        <DocumentStatus>
                          <InvoiceStatus>N</InvoiceStatus>
                          <InvoiceStatusDate>2026-01-10T10:00:00</InvoiceStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourceBilling>M</SourceBilling>
                        </DocumentStatus>
                        <Hash>0</Hash>
                        <HashControl>1</HashControl>
                        <Period>1</Period>
                        <InvoiceDate>2026-01-10</InvoiceDate>
                        <InvoiceType>FT</InvoiceType>
                        <SpecialRegimes>
                          <SelfBillingIndicator>0</SelfBillingIndicator>
                          <CashVATSchemeIndicator>0</CashVATSchemeIndicator>
                          <ThirdPartiesBillingIndicator>1</ThirdPartiesBillingIndicator>
                        </SpecialRegimes>
                        <SourceID>Admin</SourceID>
                        <SystemEntryDate>2026-01-10T10:00:00</SystemEntryDate>
                        <CustomerID>C1</CustomerID>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <ProductCode>P1</ProductCode>
                          <ProductDescription>Produto 1</ProductDescription>
                          <Quantity>1</Quantity>
                          <UnitOfMeasure>UN</UnitOfMeasure>
                          <UnitPrice>100.00</UnitPrice>
                          <TaxPointDate>2026-01-10</TaxPointDate>
                          <Description>Produto 1</Description>
                          <CreditAmount>100.00</CreditAmount>
                          <Tax>
                            <TaxType>IS</TaxType>
                            <TaxCountryRegion>PT</TaxCountryRegion>
                            <TaxCode>OUT</TaxCode>
                            <TaxAmount>5.00</TaxAmount>
                          </Tax>
                        </Line>
                        <DocumentTotals>
                          <TaxPayable>5.00</TaxPayable>
                          <NetTotal>100.00</NetTotal>
                          <GrossTotal>105.00</GrossTotal>
                        </DocumentTotals>
                      </Invoice>
                      <Invoice>
                        <InvoiceNo>FT 2026-A/2</InvoiceNo>
                        <ATCUD>ABCD1234-2</ATCUD>
                        <DocumentStatus>
                          <InvoiceStatus>A</InvoiceStatus>
                          <InvoiceStatusDate>2026-01-11T10:00:00</InvoiceStatusDate>
                          <Reason>Erro de emissão</Reason>
                          <SourceID>Admin</SourceID>
                          <SourceBilling>M</SourceBilling>
                        </DocumentStatus>
                        <Hash>0</Hash>
                        <HashControl>1</HashControl>
                        <Period>1</Period>
                        <InvoiceDate>2026-01-11</InvoiceDate>
                        <InvoiceType>FT</InvoiceType>
                        <SpecialRegimes>
                          <SelfBillingIndicator>0</SelfBillingIndicator>
                          <CashVATSchemeIndicator>0</CashVATSchemeIndicator>
                          <ThirdPartiesBillingIndicator>0</ThirdPartiesBillingIndicator>
                        </SpecialRegimes>
                        <SourceID>Admin</SourceID>
                        <SystemEntryDate>2026-01-11T10:00:00</SystemEntryDate>
                        <CustomerID>C1</CustomerID>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <ProductCode>P1</ProductCode>
                          <ProductDescription>Produto 1</ProductDescription>
                          <Quantity>1</Quantity>
                          <UnitOfMeasure>UN</UnitOfMeasure>
                          <UnitPrice>50.00</UnitPrice>
                          <TaxPointDate>2026-01-11</TaxPointDate>
                          <Description>Produto 1</Description>
                          <CreditAmount>50.00</CreditAmount>
                          <Tax>
                            <TaxType>IVA</TaxType>
                            <TaxCountryRegion>PT</TaxCountryRegion>
                            <TaxCode>NOR</TaxCode>
                            <TaxPercentage>23.00</TaxPercentage>
                          </Tax>
                        </Line>
                        <DocumentTotals>
                          <TaxPayable>11.50</TaxPayable>
                          <NetTotal>50.00</NetTotal>
                          <GrossTotal>61.50</GrossTotal>
                        </DocumentTotals>
                      </Invoice>
                    </SalesInvoices>
                  </SourceDocuments>
                </AuditFile>
                """);

            foreach (var useNewParser in new[] { true, false })
            {
                var validator = new SaftValidator(new DatabaseService()) { UseNewParser = useNewParser };
                await validator.OpenSaftFile(filename);
                var errors = validator.GetErrors();
                Check(errors.Count == 0,
                    $"Expected 0 errors on valid SAF-T 1.04_01 (UseNewParser={useNewParser}), got {errors.Count}: {string.Join(" | ", errors.Select(e => $"{e.Field}: {e.Description}"))}");
            }

            // Now test invalid SpecialRegimes.ThirdPartiesBillingIndicator, empty LineNumber after 1, and TypeofError/DisplayName
            var (saft, _) = await SaftParser.ReadFile(filename);
            var inv = saft.SourceDocuments.SalesInvoices.Invoice[0];
            inv.SpecialRegimes.ThirdPartiesBillingIndicator = "9";
            inv.Line[0].LineNumber = "";
            var v2 = new SaftValidator(new DatabaseService());
            v2.ValidateInvoices(saft.SourceDocuments.SalesInvoices, saft.Header, saft.MasterFiles);
            var errs2 = v2.GetErrors();
            Check(errs2.Any(e => e.Field == "Invoice/SpecialRegimes/ThirdPartiesBillingIndicator" && e.TypeofError == typeof(SourceDocumentsSalesInvoicesInvoice)),
                "Invalid ThirdPartiesBillingIndicator must report exact field and TypeofError.");
            Check(errs2.Any(e => e.Field == "Invoice/Line/LineNumber" && e.DisplayName == "Linhas documentos de facturação"),
                "Empty LineNumber must be detected and have proper DisplayName.");
        }
        finally
        {
            File.Delete(filename);
        }
    }

    private static async Task VerifyWorkingDocumentsAndMovementsAsync()
    {
        var filename = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(filename, """
                <?xml version="1.0" encoding="utf-8"?>
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <Header>
                    <AuditFileVersion>1.04_01</AuditFileVersion>
                    <CompanyID>501234560</CompanyID>
                    <TaxRegistrationNumber>501234560</TaxRegistrationNumber>
                    <TaxAccountingBasis>F</TaxAccountingBasis>
                    <CompanyName>Empresa Teste</CompanyName>
                    <CompanyAddress>
                      <AddressDetail>Rua A 1</AddressDetail>
                      <City>Porto</City>
                      <PostalCode>4000-001</PostalCode>
                      <Country>PT</Country>
                    </CompanyAddress>
                    <FiscalYear>2026</FiscalYear>
                    <StartDate>2026-01-01</StartDate>
                    <EndDate>2026-01-31</EndDate>
                    <CurrencyCode>EUR</CurrencyCode>
                    <DateCreated>2026-02-01</DateCreated>
                    <TaxEntity>Sede</TaxEntity>
                    <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
                    <SoftwareCertificateNumber>0</SoftwareCertificateNumber>
                    <ProductID>App/SolRIA</ProductID>
                    <ProductVersion>1.0</ProductVersion>
                  </Header>
                  <MasterFiles>
                    <Customer>
                      <CustomerID>C1</CustomerID>
                      <AccountID>2111</AccountID>
                      <CustomerTaxID>999999990</CustomerTaxID>
                      <CompanyName>Consumidor final</CompanyName>
                      <BillingAddress>
                        <AddressDetail>Desconhecido</AddressDetail>
                        <City>Desconhecido</City>
                        <PostalCode>Desconhecido</PostalCode>
                        <Country>Desconhecido</Country>
                      </BillingAddress>
                      <SelfBillingIndicator>0</SelfBillingIndicator>
                    </Customer>
                    <Product>
                      <ProductType>P</ProductType>
                      <ProductCode>P1</ProductCode>
                      <ProductDescription>Produto 1</ProductDescription>
                      <ProductNumberCode>P1</ProductNumberCode>
                    </Product>
                    <TaxTable>
                      <TaxTableEntry>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>NOR</TaxCode>
                        <Description>Normal</Description>
                        <TaxPercentage>23.00</TaxPercentage>
                      </TaxTableEntry>
                      <TaxTableEntry>
                        <TaxType>IS</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>OUT</TaxCode>
                        <Description>IS</Description>
                        <TaxAmount>2.00</TaxAmount>
                      </TaxTableEntry>
                    </TaxTable>
                  </MasterFiles>
                  <SourceDocuments>
                    <MovementOfGoods>
                      <NumberOfMovementLines>2</NumberOfMovementLines>
                      <TotalQuantityIssued>5.00</TotalQuantityIssued>
                      <StockMovement>
                        <DocumentNumber>GT 2026/1</DocumentNumber>
                        <ATCUD>0</ATCUD>
                        <DocumentStatus>
                          <MovementStatus>N</MovementStatus>
                          <MovementStatusDate>2026-01-10T09:00:00</MovementStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourceBilling>M</SourceBilling>
                        </DocumentStatus>
                        <Hash>0</Hash>
                        <HashControl>1</HashControl>
                        <MovementDate>2026-01-10</MovementDate>
                        <MovementType>GT</MovementType>
                        <SystemEntryDate>2026-01-10T09:00:00</SystemEntryDate>
                        <CustomerID>C1</CustomerID>
                        <SourceID>Admin</SourceID>
                        <ShipTo>
                          <Address>
                            <AddressDetail>Rua Destino 10</AddressDetail>
                            <City></City>
                            <PostalCode>1000-001</PostalCode>
                            <Country>PT</Country>
                          </Address>
                        </ShipTo>
                        <MovementStartTime>2026-01-10T10:00:00</MovementStartTime>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <ProductCode>P1</ProductCode>
                          <ProductDescription>Produto 1</ProductDescription>
                          <Quantity>5</Quantity>
                          <UnitOfMeasure>UN</UnitOfMeasure>
                          <UnitPrice>10.00</UnitPrice>
                          <Description>Produto 1</Description>
                          <CreditAmount>50.00</CreditAmount>
                          <Tax>
                            <TaxType>IVA</TaxType>
                            <TaxCountryRegion>PT</TaxCountryRegion>
                            <TaxCode>NOR</TaxCode>
                            <TaxPercentage>23.00</TaxPercentage>
                          </Tax>
                        </Line>
                        <DocumentTotals>
                          <TaxPayable>11.50</TaxPayable>
                          <NetTotal>50.00</NetTotal>
                          <GrossTotal>61.50</GrossTotal>
                        </DocumentTotals>
                      </StockMovement>
                      <StockMovement>
                        <DocumentNumber>GT 2026/2</DocumentNumber>
                        <ATCUD>0</ATCUD>
                        <DocumentStatus>
                          <MovementStatus>A</MovementStatus>
                          <MovementStatusDate>2026-01-11T09:00:00</MovementStatusDate>
                          <Reason>Anulada</Reason>
                          <SourceID>Admin</SourceID>
                          <SourceBilling>M</SourceBilling>
                        </DocumentStatus>
                        <Hash>0</Hash>
                        <HashControl>1</HashControl>
                        <MovementDate>2026-01-11</MovementDate>
                        <MovementType>GT</MovementType>
                        <SystemEntryDate>2026-01-11T09:00:00</SystemEntryDate>
                        <CustomerID>C1</CustomerID>
                        <SourceID>Admin</SourceID>
                        <MovementStartTime>2026-01-11T10:00:00</MovementStartTime>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <ProductCode>P1</ProductCode>
                          <ProductDescription>Produto 1</ProductDescription>
                          <Quantity>99</Quantity>
                          <UnitOfMeasure>UN</UnitOfMeasure>
                          <UnitPrice>10.00</UnitPrice>
                          <Description>Produto 1</Description>
                          <CreditAmount>990.00</CreditAmount>
                        </Line>
                        <DocumentTotals>
                          <TaxPayable>0.00</TaxPayable>
                          <NetTotal>0.00</NetTotal>
                          <GrossTotal>0.00</GrossTotal>
                        </DocumentTotals>
                      </StockMovement>
                    </MovementOfGoods>
                    <WorkingDocuments>
                      <NumberOfEntries>1</NumberOfEntries>
                      <TotalDebit>0.00</TotalDebit>
                      <TotalCredit>10.00</TotalCredit>
                      <WorkDocument>
                        <DocumentNumber>PF 2026/1</DocumentNumber>
                        <ATCUD>0</ATCUD>
                        <DocumentStatus>
                          <WorkStatus>N</WorkStatus>
                          <WorkStatusDate>2026-01-10T09:00:00</WorkStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourceBilling>M</SourceBilling>
                        </DocumentStatus>
                        <Hash>0</Hash>
                        <HashControl>1</HashControl>
                        <WorkDate>2026-01-10</WorkDate>
                        <WorkType>PF</WorkType>
                        <SourceID>Admin</SourceID>
                        <SystemEntryDate>2026-01-10T09:00:00</SystemEntryDate>
                        <CustomerID>C1</CustomerID>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <ProductCode>P1</ProductCode>
                          <ProductDescription>Produto 1</ProductDescription>
                          <Quantity>1</Quantity>
                          <UnitOfMeasure>UN</UnitOfMeasure>
                          <UnitPrice>0.00</UnitPrice>
                          <Description>Oferta</Description>
                          <CreditAmount>0.00</CreditAmount>
                        </Line>
                        <Line>
                          <LineNumber>2</LineNumber>
                          <ProductCode>P1</ProductCode>
                          <ProductDescription>Produto 1</ProductDescription>
                          <Quantity>1</Quantity>
                          <UnitOfMeasure>UN</UnitOfMeasure>
                          <UnitPrice>10.00</UnitPrice>
                          <Description>Taxa fixa</Description>
                          <CreditAmount>10.00</CreditAmount>
                          <Tax>
                            <TaxType>IS</TaxType>
                            <TaxCountryRegion>PT</TaxCountryRegion>
                            <TaxCode>OUT</TaxCode>
                            <TaxAmount>2.00</TaxAmount>
                          </Tax>
                        </Line>
                        <DocumentTotals>
                          <TaxPayable>2.00</TaxPayable>
                          <NetTotal>10.00</NetTotal>
                          <GrossTotal>12.00</GrossTotal>
                        </DocumentTotals>
                      </WorkDocument>
                    </WorkingDocuments>
                  </SourceDocuments>
                </AuditFile>
                """);

            foreach (var useNewParser in new[] { true, false })
            {
                var validator = new SaftValidator(new DatabaseService()) { UseNewParser = useNewParser };
                await validator.OpenSaftFile(filename);
                var errors = validator.GetErrors();

                // Only ShipTo/City should fail (ShipFrom is null -> must not throw, and ShipTo must actually be validated!).
                Check(errors.Any(e => e.Field == "StockMovement/ShipTo/City" && e.TypeofError == typeof(SourceDocumentsMovementOfGoodsStockMovement)),
                    $"Expected ShipTo/City error (UseNewParser={useNewParser}). Errors: {string.Join(" | ", errors.Select(e => $"{e.Field}: {e.Description}"))}");
                Check(!errors.Any(e => e.Field.StartsWith("MovementOfGoods", StringComparison.Ordinal)),
                    $"TotalQuantityIssued must exclude cancelled StockMovement and streaming parser must read StockMovement/Line/Tax (UseNewParser={useNewParser}).");
                Check(!errors.Any(e => e.Field.StartsWith("WorkDocument", StringComparison.Ordinal) || e.Field.StartsWith("WorkingDocuments", StringComparison.Ordinal)),
                    $"WorkDocument with UnitPrice=0, Tax==null, and TaxAmount=2.00 must have 0 errors (UseNewParser={useNewParser}). Errors: {string.Join(" | ", errors.Where(e => e.Field.StartsWith("Work", StringComparison.Ordinal)).Select(e => $"{e.Field}: {e.Description}"))}");
            }
        }
        finally
        {
            File.Delete(filename);
        }
    }

    private static async Task VerifyPaymentsExtendedRulesAsync()
    {
        var filename = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(filename, """
                <?xml version="1.0" encoding="utf-8"?>
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <Header>
                    <AuditFileVersion>1.04_01</AuditFileVersion>
                    <CompanyID>501234560</CompanyID>
                    <TaxRegistrationNumber>501234560</TaxRegistrationNumber>
                    <TaxAccountingBasis>F</TaxAccountingBasis>
                    <CompanyName>Empresa</CompanyName>
                    <CompanyAddress>
                      <AddressDetail>Rua A</AddressDetail>
                      <City>Lisboa</City>
                      <PostalCode>1000-001</PostalCode>
                      <Country>PT</Country>
                    </CompanyAddress>
                    <FiscalYear>2026</FiscalYear>
                    <StartDate>2026-01-01</StartDate>
                    <EndDate>2026-01-31</EndDate>
                    <CurrencyCode>EUR</CurrencyCode>
                    <DateCreated>2026-02-01</DateCreated>
                    <TaxEntity>Sede</TaxEntity>
                    <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
                    <SoftwareCertificateNumber>0</SoftwareCertificateNumber>
                    <ProductID>App/SolRIA</ProductID>
                    <ProductVersion>1.0</ProductVersion>
                  </Header>
                  <MasterFiles>
                    <Customer>
                      <CustomerID>C1</CustomerID>
                      <AccountID>2111</AccountID>
                      <CustomerTaxID>999999990</CustomerTaxID>
                      <CompanyName>Consumidor final</CompanyName>
                      <BillingAddress>
                        <AddressDetail>Desconhecido</AddressDetail>
                        <City>Desconhecido</City>
                        <PostalCode>Desconhecido</PostalCode>
                        <Country>Desconhecido</Country>
                      </BillingAddress>
                      <SelfBillingIndicator>0</SelfBillingIndicator>
                    </Customer>
                    <TaxTable>
                      <TaxTableEntry>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>NOR</TaxCode>
                        <Description>IVA Normal</Description>
                        <TaxPercentage>23.00</TaxPercentage>
                      </TaxTableEntry>
                    </TaxTable>
                  </MasterFiles>
                  <SourceDocuments>
                    <Payments>
                      <NumberOfEntries>1</NumberOfEntries>
                      <TotalDebit>0.00</TotalDebit>
                      <TotalCredit>100.00</TotalCredit>
                      <Payment>
                        <PaymentRefNo>RC 2026/1</PaymentRefNo>
                        <ATCUD>ABCD1234-1</ATCUD>
                        <Period>1</Period>
                        <TransactionDate>2026-01-15</TransactionDate>
                        <PaymentType>RC</PaymentType>
                        <DocumentStatus>
                          <PaymentStatus>N</PaymentStatus>
                          <PaymentStatusDate>2026-01-15T10:00:00</PaymentStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourcePayment>P</SourcePayment>
                        </DocumentStatus>
                        <PaymentMethod>
                          <PaymentMechanism>MB</PaymentMechanism>
                          <PaymentAmount>123.00</PaymentAmount>
                          <PaymentDate>2026-01-15</PaymentDate>
                        </PaymentMethod>
                        <SourceID>Admin</SourceID>
                        <SystemEntryDate>2026-01-15T10:00:00</SystemEntryDate>
                        <CustomerID>C1</CustomerID>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <SourceDocumentID>
                            <OriginatingON>FT 2026/1</OriginatingON>
                            <InvoiceDate>2026-01-10</InvoiceDate>
                          </SourceDocumentID>
                          <SettlementAmount>5.00</SettlementAmount>
                          <CreditAmount>100.00</CreditAmount>
                          <Tax>
                            <TaxType>IVA</TaxType>
                            <TaxCountryRegion>PT</TaxCountryRegion>
                            <TaxCode>NOR</TaxCode>
                            <TaxPercentage>23.00</TaxPercentage>
                          </Tax>
                        </Line>
                        <DocumentTotals>
                          <TaxPayable>23.00</TaxPayable>
                          <NetTotal>100.00</NetTotal>
                          <GrossTotal>123.00</GrossTotal>
                          <Settlement>
                            <SettlementAmount>5.00</SettlementAmount>
                          </Settlement>
                        </DocumentTotals>
                      </Payment>
                    </Payments>
                  </SourceDocuments>
                </AuditFile>
                """);

            foreach (var useNewParser in new[] { true, false })
            {
                var validator = new SaftValidator(new DatabaseService()) { UseNewParser = useNewParser };
                await validator.OpenSaftFile(filename);
                Check(validator.GetErrors().Count == 0,
                    $"Valid RC Payment must have 0 errors (UseNewParser={useNewParser}), got: {string.Join(" | ", validator.GetErrors().Select(e => $"{e.Field}: {e.Description}"))}");
            }

            var (saft, _) = await SaftParser.ReadFile(filename);
            var p = saft.SourceDocuments.Payments.Payment[0];
            p.PaymentMethod[0].PaymentAmount = 50.00m;
            p.DocumentTotals.Settlement.SettlementAmount = 9.00m;
            var v2 = new SaftValidator(new DatabaseService());
            v2.ValidatePayments(saft.SourceDocuments.Payments, saft.Header, saft.MasterFiles, includeExtendedChecks: true);
            Check(v2.GetErrors().Any(e => e.Field == "Payment/PaymentMethod/PaymentAmount"),
                "Mismatched PaymentMethod sum must be detected.");
            Check(v2.GetErrors().Any(e => e.Field == "Payment/DocumentTotals/Settlement/SettlementAmount"),
                "Mismatched Payment SettlementAmount must be detected.");
        }
        finally
        {
            File.Delete(filename);
        }
    }

    private static async Task VerifyReferentialAndTaxExemptionRulesAsync()
    {
        var filename = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(filename, """
                <?xml version="1.0" encoding="utf-8"?>
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <Header>
                    <AuditFileVersion>1.04_01</AuditFileVersion>
                    <CompanyID>501234560</CompanyID>
                    <TaxRegistrationNumber>501234560</TaxRegistrationNumber>
                    <TaxAccountingBasis>F</TaxAccountingBasis>
                    <CompanyName>Empresa</CompanyName>
                    <CompanyAddress>
                      <AddressDetail>Rua A</AddressDetail>
                      <City>Lisboa</City>
                      <PostalCode>1000-001</PostalCode>
                      <Country>PT</Country>
                    </CompanyAddress>
                    <FiscalYear>2026</FiscalYear>
                    <StartDate>2026-01-01</StartDate>
                    <EndDate>2026-01-31</EndDate>
                    <CurrencyCode>EUR</CurrencyCode>
                    <DateCreated>2026-02-01</DateCreated>
                    <TaxEntity>Sede</TaxEntity>
                    <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
                    <SoftwareCertificateNumber>0</SoftwareCertificateNumber>
                    <ProductID>App/SolRIA</ProductID>
                    <ProductVersion>1.0</ProductVersion>
                  </Header>
                  <MasterFiles>
                    <Customer>
                      <CustomerID>C_GEN</CustomerID>
                      <AccountID>2111</AccountID>
                      <CustomerTaxID>999999990</CustomerTaxID>
                      <CompanyName>Cliente Errado</CompanyName>
                      <BillingAddress>
                        <AddressDetail>Desconhecido</AddressDetail>
                        <City>Desconhecido</City>
                        <PostalCode>Desconhecido</PostalCode>
                        <Country>Desconhecido</Country>
                      </BillingAddress>
                      <SelfBillingIndicator>0</SelfBillingIndicator>
                    </Customer>
                    <Product>
                      <ProductType>P</ProductType>
                      <ProductCode>P1</ProductCode>
                      <ProductDescription>Produto 1</ProductDescription>
                      <ProductNumberCode>P1</ProductNumberCode>
                    </Product>
                    <TaxTable>
                      <TaxTableEntry>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>ISE</TaxCode>
                        <Description>Isento</Description>
                        <TaxPercentage>0.00</TaxPercentage>
                      </TaxTableEntry>
                    </TaxTable>
                  </MasterFiles>
                  <SourceDocuments>
                    <SalesInvoices>
                      <NumberOfEntries>1</NumberOfEntries>
                      <TotalDebit>99.00</TotalDebit>
                      <TotalCredit>0.00</TotalCredit>
                      <Invoice>
                        <InvoiceNo>NC 2026/1</InvoiceNo>
                        <ATCUD>ABCD1234-99</ATCUD>
                        <DocumentStatus>
                          <InvoiceStatus>N</InvoiceStatus>
                          <InvoiceStatusDate>2026-01-10T10:00:00</InvoiceStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourceBilling>M</SourceBilling>
                        </DocumentStatus>
                        <Hash>0</Hash>
                        <HashControl>1</HashControl>
                        <InvoiceDate>2026-01-10</InvoiceDate>
                        <InvoiceType>NC</InvoiceType>
                        <SpecialRegimes>
                          <SelfBillingIndicator>0</SelfBillingIndicator>
                          <CashVATSchemeIndicator>0</CashVATSchemeIndicator>
                          <ThirdPartiesBillingIndicator>0</ThirdPartiesBillingIndicator>
                        </SpecialRegimes>
                        <SourceID>Admin</SourceID>
                        <SystemEntryDate>2026-01-10T10:00:00</SystemEntryDate>
                        <CustomerID>C_INEXISTENTE</CustomerID>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <ProductCode>P_INEXISTENTE</ProductCode>
                          <ProductDescription>Produto</ProductDescription>
                          <Quantity>1</Quantity>
                          <UnitOfMeasure>UN</UnitOfMeasure>
                          <UnitPrice>10.00</UnitPrice>
                          <TaxPointDate>2026-01-10</TaxPointDate>
                          <Description>Produto</Description>
                          <DebitAmount>10.00</DebitAmount>
                          <Tax>
                            <TaxType>IVA</TaxType>
                            <TaxCountryRegion>PT</TaxCountryRegion>
                            <TaxCode>ISE</TaxCode>
                            <TaxPercentage>0.00</TaxPercentage>
                          </Tax>
                          <TaxExemptionReason>IVA autoliquidação</TaxExemptionReason>
                          <TaxExemptionCode>M08</TaxExemptionCode>
                        </Line>
                        <DocumentTotals>
                          <TaxPayable>0.00</TaxPayable>
                          <NetTotal>10.00</NetTotal>
                          <GrossTotal>10.00</GrossTotal>
                        </DocumentTotals>
                      </Invoice>
                    </SalesInvoices>
                  </SourceDocuments>
                </AuditFile>
                """);

            var validator = new SaftValidator(new DatabaseService()) { UseNewParser = true };
            await validator.OpenSaftFile(filename);
            var errors = validator.GetErrors();

            Check(errors.Any(e => e.Field == "Customer/CompanyName" && e.ValidationType == ValidationErrorType.WARNING),
                "999999990 with non-'Consumidor final' CompanyName must emit WARNING.");
            Check(errors.Any(e => e.Field == "SalesInvoices/TotalDebit"),
                "Mismatched SalesInvoices/TotalDebit must be detected.");
            Check(errors.Any(e => e.Field == "Invoice/ATCUD"),
                "Mismatched ATCUD sequence (99 vs 1) must be detected.");
            Check(errors.Any(e => e.Field == "Invoice/CustomerID"),
                "Missing CustomerID in MasterFiles must be detected.");
            Check(errors.Any(e => e.Field == "Invoice/Line/ProductCode"),
                "Missing ProductCode in MasterFiles must be detected.");
            Check(errors.Any(e => e.Field == "Invoice/Line/References"),
                "NC without References must be detected.");
            Check(errors.Any(e => e.Field == "Invoice/Line/TaxExemptionCode" && e.ValidationType == ValidationErrorType.WARNING),
                "M08 in 2026 must emit WARNING.");
        }
        finally
        {
            File.Delete(filename);
        }
    }

    private static async Task VerifyTemporalAndMovementFutureDatesAsync()
    {
        var filename = Path.GetTempFileName();
        try
        {
            // Future MovementStartTime / MovementEndTime outside Header.StartDate..EndDate MUST be allowed!
            var futureStart = DateTime.Now.AddDays(3).ToString("yyyy-MM-ddTHH:mm:ss");
            var futureEnd = DateTime.Now.AddDays(4).ToString("yyyy-MM-ddTHH:mm:ss");

            await File.WriteAllTextAsync(filename, $"""
                <?xml version="1.0" encoding="utf-8"?>
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <Header>
                    <AuditFileVersion>1.04_01</AuditFileVersion>
                    <CompanyID>501234560</CompanyID>
                    <TaxRegistrationNumber>501234560</TaxRegistrationNumber>
                    <TaxAccountingBasis>F</TaxAccountingBasis>
                    <CompanyName>Empresa</CompanyName>
                    <CompanyAddress>
                      <AddressDetail>Rua A</AddressDetail>
                      <City>Lisboa</City>
                      <PostalCode>1000-001</PostalCode>
                      <Country>PT</Country>
                    </CompanyAddress>
                    <FiscalYear>2026</FiscalYear>
                    <StartDate>2026-01-01</StartDate>
                    <EndDate>2026-01-31</EndDate>
                    <CurrencyCode>EUR</CurrencyCode>
                    <DateCreated>2026-02-01</DateCreated>
                    <TaxEntity>Sede</TaxEntity>
                    <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
                    <SoftwareCertificateNumber>0</SoftwareCertificateNumber>
                    <ProductID>App/SolRIA</ProductID>
                    <ProductVersion>1.0</ProductVersion>
                  </Header>
                  <MasterFiles>
                    <Customer>
                      <CustomerID>C1</CustomerID>
                      <AccountID>2111</AccountID>
                      <CustomerTaxID>999999990</CustomerTaxID>
                      <CompanyName>Consumidor final</CompanyName>
                      <BillingAddress>
                        <AddressDetail>Desconhecido</AddressDetail>
                        <City>Desconhecido</City>
                        <PostalCode>Desconhecido</PostalCode>
                        <Country>Desconhecido</Country>
                      </BillingAddress>
                      <SelfBillingIndicator>0</SelfBillingIndicator>
                    </Customer>
                    <Product>
                      <ProductType>P</ProductType>
                      <ProductCode>P1</ProductCode>
                      <ProductDescription>Produto 1</ProductDescription>
                      <ProductNumberCode>P1</ProductNumberCode>
                    </Product>
                  </MasterFiles>
                  <SourceDocuments>
                    <MovementOfGoods>
                      <NumberOfMovementLines>1</NumberOfMovementLines>
                      <TotalQuantityIssued>1.00</TotalQuantityIssued>
                      <StockMovement>
                        <DocumentNumber>GT 2026/1</DocumentNumber>
                        <ATCUD>0</ATCUD>
                        <DocumentStatus>
                          <MovementStatus>N</MovementStatus>
                          <MovementStatusDate>2026-01-31T23:00:00</MovementStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourceBilling>M</SourceBilling>
                        </DocumentStatus>
                        <Hash>0</Hash>
                        <HashControl>1</HashControl>
                        <MovementDate>2026-01-31</MovementDate>
                        <MovementType>GT</MovementType>
                        <SystemEntryDate>2026-01-31T23:00:00</SystemEntryDate>
                        <CustomerID>C1</CustomerID>
                        <SourceID>Admin</SourceID>
                        <MovementEndTime>{futureEnd}</MovementEndTime>
                        <MovementStartTime>{futureStart}</MovementStartTime>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <ProductCode>P1</ProductCode>
                          <ProductDescription>Produto 1</ProductDescription>
                          <Quantity>1</Quantity>
                          <UnitOfMeasure>UN</UnitOfMeasure>
                          <UnitPrice>10.00</UnitPrice>
                          <Description>Produto 1</Description>
                          <CreditAmount>10.00</CreditAmount>
                        </Line>
                        <DocumentTotals>
                          <TaxPayable>0.00</TaxPayable>
                          <NetTotal>10.00</NetTotal>
                          <GrossTotal>10.00</GrossTotal>
                        </DocumentTotals>
                      </StockMovement>
                    </MovementOfGoods>
                  </SourceDocuments>
                </AuditFile>
                """);

            var validator = new SaftValidator(new DatabaseService()) { UseNewParser = true };
            await validator.OpenSaftFile(filename);
            var errors = validator.GetErrors();
            Check(errors.Count == 0,
                $"Future MovementStartTime/EndTime outside Header range must be allowed, got: {string.Join(" | ", errors.Select(e => $"{e.Field}: {e.Description}"))}");

            // Now test inverted MovementStartTime > MovementEndTime
            var mov = validator.SaftFile.SourceDocuments.MovementOfGoods.StockMovement[0];
            (mov.MovementStartTime, mov.MovementEndTime) = (mov.MovementEndTime, mov.MovementStartTime);
            var v2 = new SaftValidator(new DatabaseService());
            v2.ValidateMovementOfGoods(validator.SaftFile.SourceDocuments.MovementOfGoods, validator.SaftFile.Header, validator.SaftFile.MasterFiles);
            Check(v2.GetErrors().Any(e => e.Field == "StockMovement/MovementEndTime"),
                "MovementStartTime > MovementEndTime must report an error.");
        }
        finally
        {
            File.Delete(filename);
        }
    }

    private static async Task VerifyGeneralLedgerValidationsAsync()
    {
        var filename = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(filename, """
                <?xml version="1.0" encoding="utf-8"?>
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <Header>
                    <AuditFileVersion>1.04_01</AuditFileVersion>
                    <CompanyID>501234560</CompanyID>
                    <TaxRegistrationNumber>501234560</TaxRegistrationNumber>
                    <TaxAccountingBasis>C</TaxAccountingBasis>
                    <CompanyName>Empresa</CompanyName>
                    <CompanyAddress>
                      <AddressDetail>Rua A</AddressDetail>
                      <City>Lisboa</City>
                      <PostalCode>1000-001</PostalCode>
                      <Country>PT</Country>
                    </CompanyAddress>
                    <FiscalYear>2026</FiscalYear>
                    <StartDate>2026-01-01</StartDate>
                    <EndDate>2026-01-31</EndDate>
                    <CurrencyCode>EUR</CurrencyCode>
                    <DateCreated>2026-02-01</DateCreated>
                    <TaxEntity>Sede</TaxEntity>
                    <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
                    <SoftwareCertificateNumber>0</SoftwareCertificateNumber>
                    <ProductID>App/SolRIA</ProductID>
                    <ProductVersion>1.0</ProductVersion>
                  </Header>
                  <MasterFiles>
                    <GeneralLedgerAccounts>
                      <TaxonomyReference>S</TaxonomyReference>
                      <Account>
                        <AccountID>111</AccountID>
                        <AccountDescription>Caixa</AccountDescription>
                        <OpeningDebitBalance>100.00</OpeningDebitBalance>
                        <OpeningCreditBalance>0.00</OpeningCreditBalance>
                        <ClosingDebitBalance>223.00</ClosingDebitBalance>
                        <ClosingCreditBalance>0.00</ClosingCreditBalance>
                        <GroupingCategory>GM</GroupingCategory>
                        <TaxonomyCode>1</TaxonomyCode>
                      </Account>
                      <Account>
                        <AccountID>2433</AccountID>
                        <AccountDescription>IVA Liquidado</AccountDescription>
                        <OpeningDebitBalance>0.00</OpeningDebitBalance>
                        <OpeningCreditBalance>0.00</OpeningCreditBalance>
                        <ClosingDebitBalance>0.00</ClosingDebitBalance>
                        <ClosingCreditBalance>23.00</ClosingCreditBalance>
                        <GroupingCategory>GM</GroupingCategory>
                        <TaxonomyCode>2</TaxonomyCode>
                      </Account>
                      <Account>
                        <AccountID>711</AccountID>
                        <AccountDescription>Vendas</AccountDescription>
                        <OpeningDebitBalance>0.00</OpeningDebitBalance>
                        <OpeningCreditBalance>100.00</OpeningCreditBalance>
                        <ClosingDebitBalance>0.00</ClosingDebitBalance>
                        <ClosingCreditBalance>200.00</ClosingCreditBalance>
                        <GroupingCategory>GM</GroupingCategory>
                        <TaxonomyCode>3</TaxonomyCode>
                      </Account>
                    </GeneralLedgerAccounts>
                  </MasterFiles>
                  <GeneralLedgerEntries>
                    <NumberOfEntries>1</NumberOfEntries>
                    <TotalDebit>123.00</TotalDebit>
                    <TotalCredit>123.00</TotalCredit>
                    <Journal>
                      <JournalID>VENDAS</JournalID>
                      <Description>Diário de Vendas</Description>
                      <Transaction>
                        <TransactionID>2026-01-10 VENDAS 1</TransactionID>
                        <Period>1</Period>
                        <TransactionDate>2026-01-10</TransactionDate>
                        <SourceID>Admin</SourceID>
                        <Description>Venda</Description>
                        <DocArchivalNumber>1</DocArchivalNumber>
                        <TransactionType>N</TransactionType>
                        <GLPostingDate>2026-01-10</GLPostingDate>
                        <Lines>
                          <DebitLine>
                            <RecordID>1</RecordID>
                            <AccountID>111</AccountID>
                            <SystemEntryDate>2026-01-10T10:00:00</SystemEntryDate>
                            <Description>Caixa</Description>
                            <DebitAmount>123.00</DebitAmount>
                          </DebitLine>
                          <CreditLine>
                            <RecordID>2</RecordID>
                            <AccountID>711</AccountID>
                            <SystemEntryDate>2026-01-10T10:00:00</SystemEntryDate>
                            <Description>Venda</Description>
                            <CreditAmount>123.00</CreditAmount>
                          </CreditLine>
                        </Lines>
                      </Transaction>
                    </Journal>
                  </GeneralLedgerEntries>
                </AuditFile>
                """);

            var validator = new SaftValidator(new DatabaseService()) { UseNewParser = true };
            await validator.OpenSaftFile(filename);
            var errors = validator.GetErrors();
            Check(errors.Count == 0,
                $"Balanced GL entries must pass with 0 errors, got: {string.Join(" | ", errors.Select(e => $"{e.Field}: {e.Description}"))}");

            // Also verify multi-line DebitLines/CreditLines and unbalanced detection directly on ValidateGeneralLedgerEntries
            var glValidator = new SaftValidator(new DatabaseService());
            var glAccounts = validator.SaftFile.MasterFiles.GeneralLedgerAccounts;
            var glMulti = new GeneralLedgerEntries
            {
                NumberOfEntries = "1",
                TotalDebit = 123m,
                TotalCredit = 123m,
                Journal =
                [
                    new GeneralLedgerEntriesJournal
                    {
                        JournalID = "VENDAS",
                        Description = "Vendas",
                        Transaction =
                        [
                            new GeneralLedgerEntriesJournalTransaction
                            {
                                TransactionID = "2026-01-10 VENDAS 1",
                                Period = "1",
                                TransactionDate = new DateTime(2026, 1, 10),
                                GLPostingDate = new DateTime(2026, 1, 10),
                                TransactionType = TransactionType.N,
                                Lines = new GeneralLedgerEntriesJournalTransactionLines
                                {
                                    DebitLines = [new GeneralLedgerEntriesJournalTransactionLinesDebitLine { AccountID = "111", DebitAmount = 123m }],
                                    CreditLines =
                                    [
                                        new GeneralLedgerEntriesJournalTransactionLinesCreditLine { AccountID = "711", CreditAmount = 100m },
                                        new GeneralLedgerEntriesJournalTransactionLinesCreditLine { AccountID = "2433", CreditAmount = 23m }
                                    ]
                                }
                            }
                        ]
                    }
                ]
            };
            glValidator.ValidateGeneralLedgerEntries(glMulti, glAccounts);
            Check(glValidator.GetErrors().Count == 0, "Balanced multi-line GL transaction must pass.");

            glMulti.Journal[0].Transaction[0].Lines.CreditLines[1].CreditAmount = 10m;
            glValidator.ValidateGeneralLedgerEntries(glMulti, glAccounts);
            Check(glValidator.GetErrors().Any(e => e.Field == "GeneralLedgerEntries/Journal/Transaction"),
                "Unbalanced GL transaction must be detected.");
        }
        finally
        {
            File.Delete(filename);
        }
    }

    private static async Task VerifyHashChainAndSchemaStreamingAsync()
    {
        // Generate an RSA keypair in temp files and test 12 invoices out-of-order (1..12) + 2 series
        var privFile = Path.GetTempFileName();
        var pubFile = Path.GetTempFileName();
        var badXmlFile = Path.GetTempFileName();
        try
        {
            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                await File.WriteAllTextAsync(privFile, rsa.ToXmlString(true));
                await File.WriteAllTextAsync(pubFile, rsa.ToXmlString(false));
            }

            var invoices = new List<SourceDocumentsSalesInvoicesInvoice>();
            // Insert 1..12 in reverse order so lexicographical/array order fails if not numerically sorted!
            for (int n = 12; n >= 1; n--)
            {
                invoices.Add(new SourceDocumentsSalesInvoicesInvoice
                {
                    InvoiceNo = $"FT A/{n}",
                    InvoiceType = InvoiceType.FT,
                    InvoiceDate = new DateTime(2026, 1, n),
                    SystemEntryDate = new DateTime(2026, 1, n, 10, 0, 0),
                    DocumentStatus = new SourceDocumentsSalesInvoicesInvoiceDocumentStatus { SourceBilling = SAFTPTSourceBilling.P },
                    DocumentTotals = new SourceDocumentsSalesInvoicesInvoiceDocumentTotals { GrossTotal = 100m + n }
                });
            }
            invoices.Add(new SourceDocumentsSalesInvoicesInvoice
            {
                InvoiceNo = "FT B/1",
                InvoiceType = InvoiceType.FT,
                InvoiceDate = new DateTime(2026, 1, 1),
                SystemEntryDate = new DateTime(2026, 1, 1, 10, 0, 0),
                DocumentStatus = new SourceDocumentsSalesInvoicesInvoiceDocumentStatus { SourceBilling = SAFTPTSourceBilling.P },
                DocumentTotals = new SourceDocumentsSalesInvoicesInvoiceDocumentTotals { GrossTotal = 50m }
            });

            var validator = new SaftValidator(new DatabaseService())
            {
                PrivateKeyFileName = privFile,
                PublicKeyFileName = pubFile,
                SaftFile = new AuditFile
                {
                    SourceDocuments = new SourceDocuments
                    {
                        SalesInvoices = new SourceDocumentsSalesInvoices { Invoice = [.. invoices] }
                    }
                }
            };

            // Generate hashes sequentially in numeric order per series
            var orderedA = invoices.Where(i => i.InvoiceNo.StartsWith("FT A/", StringComparison.Ordinal)).OrderBy(i => int.Parse(i.InvoiceNo.Split('/')[1])).ToArray();
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(await File.ReadAllTextAsync(privFile));
                string prevHash = "";
                foreach (var inv in orderedA)
                {
                    var payload = $"{inv.InvoiceDate:yyyy-MM-dd};{inv.SystemEntryDate:yyyy-MM-ddTHH:mm:ss};{inv.InvoiceNo};{inv.DocumentTotals.GrossTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)};{prevHash}";
                    inv.Hash = Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(payload), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));
                    prevHash = inv.Hash;
                }
                var invB1 = invoices.Single(i => i.InvoiceNo == "FT B/1");
                var payloadB = $"{invB1.InvoiceDate:yyyy-MM-dd};{invB1.SystemEntryDate:yyyy-MM-ddTHH:mm:ss};{invB1.InvoiceNo};{invB1.DocumentTotals.GrossTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)};";
                invB1.Hash = Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(payloadB), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));
            }

            validator.ValidateHashV4();
            Check(validator.GetErrors().Count == 0,
                $"Expected 0 hash errors across 1..12 + second series, got {validator.GetErrors().Count}");

            // Tamper with FT A/1 -> doc #1 must be validated too!
            orderedA[0].DocumentTotals.GrossTotal = 999m;
            validator.ValidateHashV4();
            Check(validator.GetErrors().Any(e => e.Value == "FT A/1"),
                "First document in series (num == 1) must be validated when tampered.");

            // Streaming XSD must report multiple schema errors in one pass
            await File.WriteAllTextAsync(badXmlFile, """
                <?xml version="1.0" encoding="utf-8"?>
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <Header>
                    <AuditFileVersion>9.99</AuditFileVersion>
                    <CompanyID>501234560</CompanyID>
                    <TaxRegistrationNumber>10</TaxRegistrationNumber>
                  </Header>
                </AuditFile>
                """);
            var schemaValidator = new SaftValidator(new DatabaseService()) { SaftFileName = badXmlFile };
            schemaValidator.ValidaEstruturaXSD("1.04_01");
            var schemaErrors = schemaValidator.GetErrors();
            Check(schemaErrors.Count >= 2 && schemaErrors.All(e => e.ValidationType == ValidationErrorType.SCHEMA && e.TypeofError == typeof(SchemaResults)),
                $"Streaming XSD validation must report multiple SCHEMA errors, got {schemaErrors.Count}.");
        }
        finally
        {
            File.Delete(privFile);
            File.Delete(pubFile);
            File.Delete(badXmlFile);
        }
    }

    private static async Task VerifyStockFileValidationsAsync()
    {
        var xmlFile = Path.GetTempFileName() + ".xml";
        var csvFile = Path.GetTempFileName() + ".csv";
        try
        {
            await File.WriteAllTextAsync(xmlFile, """
                <?xml version="1.0" encoding="utf-8"?>
                <StockFile xmlns="urn:StockFile:PT_2_01">
                  <StockHeader>
                    <FileVersion>2_01</FileVersion>
                    <TaxRegistrationNumber>501234560</TaxRegistrationNumber>
                    <FiscalYear>2026</FiscalYear>
                    <EndDate>2026-12-31</EndDate>
                    <NoStock>false</NoStock>
                  </StockHeader>
                  <Stock>
                    <ProductCategory>M</ProductCategory>
                    <ProductCode>P1</ProductCode>
                    <ProductDescription>Produto 1</ProductDescription>
                    <ProductNumberCode>5601234567890</ProductNumberCode>
                    <ClosingStockQuantity>10.00</ClosingStockQuantity>
                    <UnitOfMeasure>UN</UnitOfMeasure>
                    <ClosingStockValue>150.00</ClosingStockValue>
                  </Stock>
                </StockFile>
                """);

            var v1 = new SaftValidator(new DatabaseService());
            await v1.OpenStockFile(xmlFile);
            Check(v1.GetErrors().Count == 0,
                $"Valid Stock 2_01 XML must have 0 errors, got: {string.Join(" | ", v1.GetErrors().Select(e => e.Description))}");
            Check(v1.StockFile.Stock.Length == 1 && v1.StockFile.Stock[0].ClosingStockValue == 150.00m,
                "Stock 2_01 ClosingStockValue must be read correctly.");

            await File.WriteAllTextAsync(csvFile, """
                ProductCategory;ProductCode;ProductDescription;ProductNumberCode;ClosingStockQuantity;UnitOfMeasure;ClosingStockValue
                M;"P1";"Produto 1";"123";10.00;UN;50.00
                M;"P1";"Produto Duplicado";"124";-2.00;UN;10.00
                BAD_ROW
                """);

            var v2 = new SaftValidator(new DatabaseService());
            await v2.OpenStockFile(csvFile);
            var csvErrors = v2.GetErrors();
            Check(csvErrors.Any(e => e.Field == "Stock/ProductCode" && e.Description.Contains("duplicado", StringComparison.OrdinalIgnoreCase)),
                "Duplicate ProductCode in CSV stock file must be detected.");
            Check(csvErrors.Any(e => e.Field == "Stock/ClosingStockQuantity"),
                "Negative ClosingStockQuantity in CSV stock file must be detected.");
            Check(csvErrors.Any(e => e.Value == "BAD_ROW"),
                "Malformed CSV row with fewer than 6 columns must report an error.");
        }
        finally
        {
            if (File.Exists(xmlFile)) File.Delete(xmlFile);
            if (File.Exists(csvFile)) File.Delete(csvFile);
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
