using Microsoft.Data.Sqlite;
using SolRIA.SAFT.Parser;
using SolRIA.SAFT.Parser.Models;
using SolRIA.SAFT.Parser.Services;

internal static class OrderReferencesImport
{
    public static async Task VerifyAsync()
    {
        const string lines = """
            <Line>
              <LineNumber>1</LineNumber>
              <OrderReferences />
              <OrderReferences><OriginatingON>NE 2026/1</OriginatingON><OrderDate>2026-01-02</OrderDate></OrderReferences>
              <OrderReferences><OriginatingON>NE 2026/1</OriginatingON><OrderDate>2026-01-02</OrderDate></OrderReferences>
              <OrderReferences><OriginatingON>NE 2026/2</OriginatingON><OrderDate>invalid</OrderDate></OrderReferences>
              <ProductCode>TEST</ProductCode>
            </Line>
            <Line><LineNumber>2</LineNumber><ProductCode>NO-REF</ProductCode></Line>
            """;

        var directory = Directory.CreateTempSubdirectory();
        var databasePath = Path.Combine(directory.FullName, "saft.sqlite");
        var database = new DatabaseService(databasePath);
        try
        {
            var filename = Path.Combine(directory.FullName, "saft.xml");
            await File.WriteAllTextAsync(filename, $"""
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <SourceDocuments>
                    <SalesInvoices><Invoice><InvoiceNo>FT 2026/1</InvoiceNo>{lines}</Invoice></SalesInvoices>
                    <MovementOfGoods><StockMovement><DocumentNumber>GT 2026/1</DocumentNumber>{lines}</StockMovement></MovementOfGoods>
                    <WorkingDocuments><WorkDocument><DocumentNumber>PF 2026/1</DocumentNumber>{lines}</WorkDocument></WorkingDocuments>
                  </SourceDocuments>
                </AuditFile>
                """);

            using (File.Create(databasePath)) { }
            database.InitDatabase();
            var (auditFile, errors) = await SaftParser.ReadFile(filename, database, keepInMemory: true);
            var invoice = auditFile.SourceDocuments.SalesInvoices.Invoice.Single();
            var movement = auditFile.SourceDocuments.MovementOfGoods.StockMovement.Single();
            var workDocument = auditFile.SourceDocuments.WorkingDocuments.WorkDocument.Single();

            var savedInvoice = await database.GetInvoiceLinesAsync(invoice.Pk);
            var savedMovement = await database.GetStockMovementLinesAsync(movement.Pk);
            var savedWorkDocument = await database.GetWorkDocumentLinesAsync(workDocument.Pk);

            VerifyLines(invoice.Line.Select(l => (l.OrderReferences, l.ProductCode)), "XML invoice");
            VerifyLines(movement.Line.Select(l => (l.OrderReferences, l.ProductCode)), "XML stock movement");
            VerifyLines(workDocument.Line.Select(l => (l.OrderReferences, l.ProductCode)), "XML work document");
            VerifyLines(savedInvoice.Select(l => (l.OrderReferences, l.ProductCode)), "SQLite invoice");
            VerifyLines(savedMovement.Select(l => (l.OrderReferences, l.ProductCode)), "SQLite stock movement");
            VerifyLines(savedWorkDocument.Select(l => (l.OrderReferences, l.ProductCode)), "SQLite work document");

            foreach (var document in new[] { "Invoice", "StockMovement", "WorkDocument" })
            {
                var field = $"{document}/Line/OrderReferences/OrderDate";
                Check(errors.Count(e => e.Field == field && e.Value == "invalid") == 1,
                    $"Expected one date validation error for {field}.");
            }
        }
        finally
        {
            using var connection = (SqliteConnection)database.CreateConnection();
            SqliteConnection.ClearPool(connection);
            directory.Delete(recursive: true);
        }
    }

    private static void VerifyLines(IEnumerable<(OrderReferences[] References, string ProductCode)> lines, string source)
    {
        var actual = lines.ToArray();
        Check(actual.Length == 2 && actual[0].ProductCode == "TEST" && actual[1].ProductCode == "NO-REF",
            $"{source} must preserve the field after the references and the next line.");
        Check(actual[1].References == null, $"{source} must preserve the absence of references.");
        (string, DateTime)[] expected =
        [
            (null, DateTime.MinValue),
            ("NE 2026/1", new DateTime(2026, 1, 2)),
            ("NE 2026/1", new DateTime(2026, 1, 2)),
            ("NE 2026/2", DateTime.MinValue)
        ];
        Check(actual[0].References?.Select(r => (r.OriginatingON, r.OrderDate)).SequenceEqual(expected) == true,
            $"{source} must preserve every reference in XML order.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
