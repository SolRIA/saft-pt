using Solria.SAFT.Parser;
using Solria.SAFT.Parser.Models;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main()
        {
            AnsiConsole.Render(new FigletText("SolRIA").Centered().Color(Color.Red));
            var rule = new Rule("[bold yellow on blue]SAFT-PT/STOCKS Parser![/]");
            AnsiConsole.Render(rule);

            var (saftFile, errors) = await SaftParser.ReadFile(@"E:\faturas\2021\01 Janeiro\SAFT_510958362_2021_janeiro_utf8.xml");

            Print(saftFile, errors);
        }

        private static void Print(AuditFile auditFile, IEnumerable<ValidationError> errors)
        {
            AnsiConsole.MarkupLine($"[bold]Versão ficheiro:[/] {auditFile.Header.AuditFileVersion}");
            AnsiConsole.MarkupLine($"[bold]NIF:[/] {auditFile.Header.TaxRegistrationNumber}");
            AnsiConsole.MarkupLine($"[bold]Empresa:[/] {auditFile.Header.CompanyName}");
            AnsiConsole.MarkupLine($"[bold]Criado:[/] {auditFile.Header.DateCreated}");
            AnsiConsole.MarkupLine($"[bold]Datas:[/] {auditFile.Header.StartDate:yyyy-MM-dd} a {auditFile.Header.EndDate:yyyy-MM-dd}");
            AnsiConsole.MarkupLine($"[bold]Certificado AT:[/] {auditFile.Header.SoftwareCertificateNumber}");

            var tableCustomers = new Table
            {
                Title = new TableTitle("Clientes")
            };
            tableCustomers.AddColumn("CompanyName");
            tableCustomers.AddColumn("AccountID");
            tableCustomers.AddColumn("CustomerID");
            tableCustomers.AddColumn("CustomerTaxID");
            tableCustomers.AddColumn("SelfBillingIndicator");
            foreach (var customer in auditFile.MasterFiles.Customer)
            {
                tableCustomers.AddRow(customer.CompanyName, customer.AccountID, customer.CustomerID, customer.CustomerTaxID, customer.SelfBillingIndicator);
            }
            tableCustomers.Expand();
            AnsiConsole.Render(tableCustomers);

            var tableProducts = new Table
            {
                Title = new TableTitle("Produtos")
            };
            tableProducts.AddColumn("ProductDescription");
            tableProducts.AddColumn("ProductCode");
            tableProducts.AddColumn("ProductGroup");
            tableProducts.AddColumn("ProductNumberCode");
            tableProducts.AddColumn("ProductType");
            foreach (var product in auditFile.MasterFiles.Product)
            {
                tableProducts.AddRow(product.ProductDescription, product.ProductCode ?? string.Empty, product.ProductGroup ?? string.Empty, product.ProductNumberCode, product.ProductType.ToString());
            }
            tableProducts.Expand();
            AnsiConsole.Render(tableProducts);

            var tableTaxes = new Table
            {
                Title = new TableTitle("Impostos")
            };
            tableTaxes.AddColumn("TaxType");
            tableTaxes.AddColumn("TaxCountryRegion");
            tableTaxes.AddColumn("TaxCode");
            tableTaxes.AddColumn("Description");
            tableTaxes.AddColumn("TaxPercentage");
            foreach (var tableEntry in auditFile.MasterFiles.TaxTable)
            {
                tableTaxes.AddRow(tableEntry.TaxType.ToString(), tableEntry.TaxCountryRegion ?? string.Empty, tableEntry.TaxCode ?? string.Empty, tableEntry.Description, tableEntry.TaxPercentage.GetValueOrDefault(0).ToString());
            }

            if (auditFile.SourceDocuments.SalesInvoices != null)
            {
                var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn(new GridColumn().RightAligned())
                .AddRow("[b]NumberOfEntries[/]", $"{auditFile.SourceDocuments.SalesInvoices.NumberOfEntries}")
                .AddRow("[b]TotalDebit[/]", $"{auditFile.SourceDocuments.SalesInvoices.TotalDebit:N3}")
                .AddRow("[b]TotalCredit[/]", $"{auditFile.SourceDocuments.SalesInvoices.TotalCredit:N3}");

                AnsiConsole.Render(new Panel(grid).Header("Documentos faturação"));

                var tableInvoices = new Table();
                tableInvoices.AddColumn("InvoiceNo");
                tableInvoices.AddColumn("InvoiceDate");
                tableInvoices.AddColumn("InvoiceType");
                tableInvoices.AddColumn("CustomerID");
                tableInvoices.AddColumn("TaxPayable", t => t.RightAligned());
                tableInvoices.AddColumn("NetTotal", t => t.RightAligned());
                tableInvoices.AddColumn("GrossTotal", t => t.RightAligned());
                foreach (var invoice in auditFile.SourceDocuments.SalesInvoices.Invoice)
                {
                    tableInvoices.AddRow(
                        invoice.InvoiceNo, 
                        $"{invoice.InvoiceDate:yyyy-MM-dd}", 
                        $"{invoice.InvoiceType}", 
                        $"{invoice.CustomerID}",
                        $"{invoice.DocumentTotals?.TaxPayable:N2}",
                        $"{invoice.DocumentTotals?.NetTotal:N2}",
                        $"{invoice.DocumentTotals?.GrossTotal:N2}");
                }
                tableInvoices.Expand();
                AnsiConsole.Render(tableInvoices);
            }
            if (auditFile.SourceDocuments.Payments != null)
            {
                var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn(new GridColumn().RightAligned())
                .AddRow("[b]NumberOfEntries[/]", $"{auditFile.SourceDocuments.Payments.NumberOfEntries}")
                .AddRow("[b]TotalDebit[/]", $"{auditFile.SourceDocuments.Payments.TotalDebit:N3}")
                .AddRow("[b]TotalCredit[/]", $"{auditFile.SourceDocuments.Payments.TotalCredit:N3}");

                AnsiConsole.Render(new Panel(grid).Header("Recibos"));
            }

            if (errors != null && errors.Any())
            {
                var tableErrors = new Table
                {
                    Title = new TableTitle("Erros")
                };
                tableErrors.AddColumn("Description");
                tableErrors.AddColumn("Field");
                foreach (var error in errors)
                {
                    tableErrors.AddRow(error.Description, error.Field);
                }
                tableErrors.Expand();
                AnsiConsole.Render(tableErrors);
            }
            else
            {
                AnsiConsole.MarkupLine("[bold]Não foram encontrados erros[/]");
            }
        }
    }
}
