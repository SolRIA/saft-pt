using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser;
using SolRIA.SAFT.Parser.Models;
using SolRIA.SAFT.Parser.Services;
using System.Text;
using LegacyAuditFile = SolRIA.SAFT.Desktop.Models.SaftV4.AuditFile;

internal static class PaymentsValidation
{
    public static async Task VerifyAsync()
    {
        var filename = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(filename, """
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <SourceDocuments>
                    <Payments>
                      <NumberOfEntries>3</NumberOfEntries>
                      <TotalDebit>30.35</TotalDebit>
                      <TotalCredit>150.50</TotalCredit>
                      <Payment>
                        <PaymentRefNo>RG 2026/1</PaymentRefNo>
                        <ATCUD>ATCUD-1</ATCUD>
                        <TransactionDate>2026-01-10</TransactionDate>
                        <PaymentType>RG</PaymentType>
                        <DocumentStatus>
                          <PaymentStatus>N</PaymentStatus>
                          <PaymentStatusDate>2026-01-10T10:00:00</PaymentStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourcePayment>P</SourcePayment>
                        </DocumentStatus>
                        <SourceID>Admin</SourceID>
                        <SystemEntryDate>2026-01-10T10:00:00</SystemEntryDate>
                        <CustomerID>1</CustomerID>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <SourceDocumentID>
                            <OriginatingON>FT 2026/1</OriginatingON>
                            <InvoiceDate>2026-01-05</InvoiceDate>
                          </SourceDocumentID>
                          <CreditAmount>100.50</CreditAmount>
                        </Line>
                        <Line>
                          <LineNumber>2</LineNumber>
                          <SourceDocumentID>
                            <OriginatingON>NC 2026/1</OriginatingON>
                            <InvoiceDate>2026-01-06</InvoiceDate>
                          </SourceDocumentID>
                          <DebitAmount>20.25</DebitAmount>
                        </Line>
                      </Payment>
                      <Payment>
                        <PaymentRefNo>RG 2026/2</PaymentRefNo>
                        <ATCUD>ATCUD-2</ATCUD>
                        <TransactionDate>2026-01-11</TransactionDate>
                        <PaymentType>RG</PaymentType>
                        <DocumentStatus>
                          <PaymentStatus>N</PaymentStatus>
                          <PaymentStatusDate>2026-01-11T10:00:00</PaymentStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourcePayment>P</SourcePayment>
                        </DocumentStatus>
                        <SourceID>Admin</SourceID>
                        <SystemEntryDate>2026-01-11T10:00:00</SystemEntryDate>
                        <CustomerID>1</CustomerID>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <SourceDocumentID>
                            <OriginatingON>FT 2026/2</OriginatingON>
                            <InvoiceDate>2026-01-07</InvoiceDate>
                          </SourceDocumentID>
                          <CreditAmount>50.00</CreditAmount>
                        </Line>
                        <Line>
                          <LineNumber>2</LineNumber>
                          <SourceDocumentID>
                            <OriginatingON>NC 2026/2</OriginatingON>
                            <InvoiceDate>2026-01-08</InvoiceDate>
                          </SourceDocumentID>
                          <DebitAmount>10.10</DebitAmount>
                        </Line>
                      </Payment>
                      <Payment>
                        <PaymentRefNo>RG 2026/3</PaymentRefNo>
                        <ATCUD>ATCUD-3</ATCUD>
                        <TransactionDate>2026-01-12</TransactionDate>
                        <PaymentType>RG</PaymentType>
                        <DocumentStatus>
                          <PaymentStatus>A</PaymentStatus>
                          <PaymentStatusDate>2026-01-12T10:00:00</PaymentStatusDate>
                          <SourceID>Admin</SourceID>
                          <SourcePayment>P</SourcePayment>
                        </DocumentStatus>
                        <SourceID>Admin</SourceID>
                        <SystemEntryDate>2026-01-12T10:00:00</SystemEntryDate>
                        <CustomerID>1</CustomerID>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <SourceDocumentID>
                            <OriginatingON>FT 2026/3</OriginatingON>
                            <InvoiceDate>2026-01-09</InvoiceDate>
                          </SourceDocumentID>
                          <CreditAmount>999.99</CreditAmount>
                        </Line>
                        <Line>
                          <LineNumber>2</LineNumber>
                          <SourceDocumentID>
                            <OriginatingON>NC 2026/3</OriginatingON>
                            <InvoiceDate>2026-01-09</InvoiceDate>
                          </SourceDocumentID>
                          <DebitAmount>888.88</DebitAmount>
                        </Line>
                      </Payment>
                    </Payments>
                  </SourceDocuments>
                </AuditFile>
                """);

            var (streamed, _) = await SaftParser.ReadFile(filename);
            var legacy = SaftXmlParser.DeserializeXml<LegacyAuditFile>(filename, Encoding.UTF8);
            Check(legacy != null, "The in-memory parser must read the payments SAF-T fixture.");

            var mapped = new AuditFileMapper().Map(legacy);

            foreach (var auditFile in new[] { streamed, mapped })
            {
                var payments = auditFile.SourceDocuments.Payments;

                // 1. Valid totals (normal payments summed, cancelled payment excluded)
                var validErrors = Validate(payments);
                Check(validErrors.Count == 0,
                    $"Expected no payment validation errors when totals are valid, found {validErrors.Count}.");

                // 2. Mismatched TotalDebit only (0.01 discrepancy to ensure exact comparison)
                var originalDebit = payments.TotalDebit;
                var originalCredit = payments.TotalCredit;
                try
                {
                    payments.TotalDebit = 30.36m;
                    var debitErrors = Validate(payments);
                    var debitError = debitErrors.SingleOrDefault(e => e.Field == "Payments/TotalDebit");
                    Check(debitErrors.Count == 1 && debitError != null,
                        "Expected exactly one TotalDebit error when only TotalDebit differs by 0.01.");
                    Check(debitError.TypeofError == typeof(SourceDocumentsPayments) && debitError.DisplayName == "Recibos",
                        "TotalDebit error must have TypeofError SourceDocumentsPayments and DisplayName 'Recibos'.");
                    Check(debitError.Value == 30.36m.ToString(),
                        "TotalDebit error Value must reflect the reported header value.");
                    Check(!debitErrors.Any(e => e.Field == "Payments/TotalCredit"),
                        "TotalCredit must not report an error when only TotalDebit differs.");

                    // 3. Mismatched TotalCredit only (0.01 discrepancy)
                    payments.TotalDebit = originalDebit;
                    payments.TotalCredit = 150.49m;
                    var creditErrors = Validate(payments);
                    var creditError = creditErrors.SingleOrDefault(e => e.Field == "Payments/TotalCredit");
                    Check(creditErrors.Count == 1 && creditError != null,
                        "Expected exactly one TotalCredit error when only TotalCredit differs by 0.01.");
                    Check(creditError.TypeofError == typeof(SourceDocumentsPayments) && creditError.DisplayName == "Recibos",
                        "TotalCredit error must have TypeofError SourceDocumentsPayments and DisplayName 'Recibos'.");
                    Check(creditError.Value == 150.49m.ToString(),
                        "TotalCredit error Value must reflect the reported header value.");
                    Check(!creditErrors.Any(e => e.Field == "Payments/TotalDebit"),
                        "TotalDebit must not report an error when only TotalCredit differs.");

                    // 4. Cancelled payment (A) erroneously included in header totals
                    payments.TotalDebit = 919.23m;
                    payments.TotalCredit = 1150.49m;
                    var cancelledIncludedErrors = Validate(payments);
                    var cancelledDebitError = cancelledIncludedErrors.SingleOrDefault(e => e.Field == "Payments/TotalDebit");
                    var cancelledCreditError = cancelledIncludedErrors.SingleOrDefault(e => e.Field == "Payments/TotalCredit");
                    Check(cancelledIncludedErrors.Count == 2 && cancelledDebitError != null && cancelledCreditError != null,
                        "Expected TotalDebit and TotalCredit errors when cancelled payment amounts are included.");
                    Check(cancelledDebitError.Description.Contains(30.35m.ToString()),
                        "TotalDebit error description must include the expected total excluding cancelled payments.");
                    Check(cancelledCreditError.Description.Contains(150.50m.ToString()),
                        "TotalCredit error description must include the expected total excluding cancelled payments.");
                }
                finally
                {
                    payments.TotalDebit = originalDebit;
                    payments.TotalCredit = originalCredit;
                }
            }

            // 5. Empty / null Payment array and null payments guard
            Check(Validate(null).Count == 0, "Null payments section must not produce errors.");

            Check(Validate(new SourceDocumentsPayments
            {
                NumberOfEntries = "0",
                TotalDebit = 0m,
                TotalCredit = 0m,
                Payment = null
            }).Count == 0, "Null Payment array with zero totals must not produce errors.");

            Check(Validate(new SourceDocumentsPayments
            {
                NumberOfEntries = "0",
                TotalDebit = 0m,
                TotalCredit = 0m,
                Payment = []
            }).Count == 0, "Empty Payment array with zero totals must not produce errors.");

            var emptyNonZeroErrors = Validate(new SourceDocumentsPayments
            {
                NumberOfEntries = "0",
                TotalDebit = 1.25m,
                TotalCredit = 2.50m,
                Payment = null
            });
            Check(emptyNonZeroErrors.Count == 2
                && emptyNonZeroErrors.Any(e => e.Field == "Payments/TotalDebit" && e.DisplayName == "Recibos")
                && emptyNonZeroErrors.Any(e => e.Field == "Payments/TotalCredit" && e.DisplayName == "Recibos"),
                "Null Payment array with non-zero totals must report both TotalDebit and TotalCredit errors.");

            Console.WriteLine("PASS: payments totals validation");
        }
        finally
        {
            File.Delete(filename);
        }
    }

    private static IList<ValidationError> Validate(SourceDocumentsPayments payments)
    {
        var validator = new SaftValidator(new DatabaseService());
        validator.ValidatePayments(payments);
        return validator.GetErrors();
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
