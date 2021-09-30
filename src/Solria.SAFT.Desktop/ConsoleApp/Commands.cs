using Solria.SAFT.Parser;
using Solria.SAFT.Parser.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class OpenFileCommand : AsyncCommand<OpenFileCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [Description("Caminho completo do ficheiro a abrir.")]
            [CommandArgument(0, "[filePath]")]
            public string FileName { get; init; }

            [Description("Tipo de documento a abrir (saft|stock|transporte).")]
            [CommandOption("--tipo")]
            public string FileType { get; init; }

            [Description("Imprime os produtos")]
            [CommandOption("-p|--print-products")]
            [DefaultValue(false)]
            public bool PrintProducts { get; init; }

            [Description("Imprime os clientes")]
            [CommandOption("-c|--print-customers")]
            [DefaultValue(false)]
            public bool PrintCustomers { get; init; }

            [Description("Imprime as taxas de IVA")]
            [CommandOption("-t|--print-taxes")]
            [DefaultValue(false)]
            public bool PrintTaxes { get; init; }

            [Description("Imprime os documentos de faturação")]
            [CommandOption("-i|--print-invoices")]
            [DefaultValue(false)]
            public bool PrintInvoices { get; init; }

            [Description("Imprime os recibos")]
            [CommandOption("-r|--print-receipts")]
            [DefaultValue(false)]
            public bool PrintReceipts { get; init; }

            [Description("Imprime os erros")]
            [CommandOption("-e|--print-errors")]
            [DefaultValue(false)]
            public bool PrintErrors { get; init; }

            [Description("Imprime todas as tabelas")]
            [CommandOption("-a|--print-all")]
            [DefaultValue(false)]
            public bool PrintAll { get; init; }
        }

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.FileName) == false && File.Exists(settings.FileName) == false)
                return ValidationResult.Error("O ficheiro não existe");

            if (string.IsNullOrWhiteSpace(settings.FileType))
                return ValidationResult.Error("Não foi indicado o tipo de ficheiro --tipo (saft|stock|transporte)");

            if (settings.FileType.Equals("saft", StringComparison.OrdinalIgnoreCase) == false &&
                settings.FileType.Equals("stock", StringComparison.OrdinalIgnoreCase) == false &&
                settings.FileType.Equals("transporte", StringComparison.OrdinalIgnoreCase) == false)
                return ValidationResult.Error("Tipo de ficheiro incorreto --tipo (saft|stock|transporte)");

            return ValidationResult.Success();
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            string filename = settings.FileName;
#if DEBUG
            if (string.IsNullOrWhiteSpace(filename))
                filename = @"E:\faturas\2021\01 Janeiro\SAFT_510958362_2021_janeiro_utf8.xml";
#endif
            var (saftFile, errors) = await SaftParser.ReadFile(filename);

            Print(saftFile, errors, settings);

            return 0;
        }

        private static void Print(AuditFile auditFile, IEnumerable<ValidationError> errors, Settings settings)
        {
            AnsiConsole.MarkupLine($"[bold]Ficheiro:[/] {settings.FileName}");
            System.Console.WriteLine();

            AnsiConsole.MarkupLine($"[bold]Versão:[/] {auditFile.Header.AuditFileVersion}");
            AnsiConsole.MarkupLine($"[bold]NIF:[/] {auditFile.Header.TaxRegistrationNumber}");
            AnsiConsole.MarkupLine($"[bold]Empresa:[/] {auditFile.Header.CompanyName}");
            AnsiConsole.MarkupLine($"[bold]Criado:[/] {auditFile.Header.DateCreated}");
            AnsiConsole.MarkupLine($"[bold]Datas:[/] {auditFile.Header.StartDate:yyyy-MM-dd} a {auditFile.Header.EndDate:yyyy-MM-dd}");
            AnsiConsole.MarkupLine($"[bold]Certificado AT:[/] {auditFile.Header.SoftwareCertificateNumber}");

            if (settings.PrintAll || settings.PrintProducts)
            {
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
            }

            if (settings.PrintAll || settings.PrintProducts)
            {
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
            }

            if (settings.PrintAll || settings.PrintTaxes)
            {
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
            }

            if (settings.PrintAll || settings.PrintInvoices && auditFile.SourceDocuments.SalesInvoices != null)
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
                if (auditFile.SourceDocuments.SalesInvoices.Invoice != null && auditFile.SourceDocuments.SalesInvoices.Invoice.Any())
                {
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
                }
                tableInvoices.Expand();
                AnsiConsole.Render(tableInvoices);
            }

            if (settings.PrintAll || settings.PrintReceipts && auditFile.SourceDocuments.Payments != null)
            {
                var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn(new GridColumn().RightAligned())
                .AddRow("[b]NumberOfEntries[/]", $"{auditFile.SourceDocuments.Payments.NumberOfEntries}")
                .AddRow("[b]TotalDebit[/]", $"{auditFile.SourceDocuments.Payments.TotalDebit:N3}")
                .AddRow("[b]TotalCredit[/]", $"{auditFile.SourceDocuments.Payments.TotalCredit:N3}");

                AnsiConsole.Render(new Panel(grid).Header("Recibos"));

                var tableInvoices = new Table();
                tableInvoices.AddColumn("DocNo");
                tableInvoices.AddColumn("Date");
                tableInvoices.AddColumn("Type");
                tableInvoices.AddColumn("CustomerID");
                tableInvoices.AddColumn("TaxPayable", t => t.RightAligned());
                tableInvoices.AddColumn("NetTotal", t => t.RightAligned());
                tableInvoices.AddColumn("GrossTotal", t => t.RightAligned());
                if (auditFile.SourceDocuments.Payments.Payment != null && auditFile.SourceDocuments.Payments.Payment.Any())
                {
                    foreach (var payment in auditFile.SourceDocuments.Payments.Payment)
                    {
                        tableInvoices.AddRow(
                            payment.PaymentRefNo,
                            $"{payment.TransactionDate:yyyy-MM-dd}",
                            $"{payment.PaymentType}",
                            $"{payment.CustomerID}",
                            $"{payment.DocumentTotals?.TaxPayable:N2}",
                            $"{payment.DocumentTotals?.NetTotal:N2}",
                            $"{payment.DocumentTotals?.GrossTotal:N2}");
                    }
                }
                tableInvoices.Expand();
                AnsiConsole.Render(tableInvoices);
            }

            if (settings.PrintAll || settings.PrintErrors)
            {
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
}
