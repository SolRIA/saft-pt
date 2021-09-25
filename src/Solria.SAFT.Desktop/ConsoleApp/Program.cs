using Solria.SAFT.Desktop.Models.SaftV4;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleApp
{
    class Program
    {
        static AuditFile auditFile;
        static async Task Main()
        {
            //register the Windows-1252 encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            await ReadXml(@"E:\faturas\2021\01 Janeiro\SAFT_510958362_2021_janeiro.xml");
        }

        private static async Task ReadXml(string filename)
        {
            auditFile = new AuditFile
            {
                SourceDocuments = new SourceDocuments()
            };

            var settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using var reader = XmlReader.Create(filename, settings);

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name))
                    continue;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (StringEquals(reader.Name, "Header"))
                    {
                        auditFile.Header = await ReadHeader(reader.ReadSubtree());
                    }
                    if (StringEquals(reader.Name, "MasterFiles"))
                    {
                        auditFile.MasterFiles = await ReadMasterFiles(reader.ReadSubtree());
                    }
                    if (StringEquals(reader.Name, "SalesInvoices"))
                    {
                        auditFile.SourceDocuments.SalesInvoices = await ReadSalesInvoices(reader.ReadSubtree());
                    }
                    if (StringEquals(reader.Name, "Payments"))
                    {
                        auditFile.SourceDocuments.Payments = await ReadPayments(reader.ReadSubtree());
                    }
                }
            }

            Print();
        }

        private static void Print()
        {
            Console.WriteLine(auditFile.Header.AuditFileVersion);
            Console.WriteLine(auditFile.Header.CompanyID);
            Console.WriteLine(auditFile.Header.TaxRegistrationNumber);
            Console.WriteLine(auditFile.Header.TaxAccountingBasis);
            Console.WriteLine(auditFile.Header.CompanyName);
            foreach (var customer in auditFile.MasterFiles.Customer)
            {
                Console.WriteLine(customer.CompanyName);
                Console.WriteLine("\t{0}", customer.AccountID);
                Console.WriteLine("\t{0}", customer.CustomerID);
                Console.WriteLine("\t{0}", customer.CustomerTaxID);
                Console.WriteLine("\t{0}", customer.SelfBillingIndicator);
            }
            foreach (var product in auditFile.MasterFiles.Product)
            {
                Console.WriteLine(product.ProductDescription);
                Console.WriteLine("\t{0}", product.ProductCode);
                Console.WriteLine("\t{0}", product.ProductGroup);
                Console.WriteLine("\t{0}", product.ProductNumberCode);
                Console.WriteLine("\t{0}", product.ProductType);
            }
            foreach (var tableEntry in auditFile.MasterFiles.TaxTable)
            {
                Console.WriteLine(tableEntry.TaxType);
                Console.WriteLine("\t{0}", tableEntry.TaxCountryRegion);
                Console.WriteLine("\t{0}", tableEntry.TaxCode);
                Console.WriteLine("\t{0}", tableEntry.Description);
                Console.WriteLine("\t{0}", tableEntry.Item);
            }
            if (auditFile.SourceDocuments.SalesInvoices != null)
            {
                Console.WriteLine("SalesInvoices");
                Console.WriteLine("\tNumberOfEntries: {0}", auditFile.SourceDocuments.SalesInvoices.NumberOfEntries);
                Console.WriteLine("\tTotalDebit: {0}", auditFile.SourceDocuments.SalesInvoices.TotalDebit);
                Console.WriteLine("\tTotalCredit: {0}", auditFile.SourceDocuments.SalesInvoices.TotalCredit);
            }
            if (auditFile.SourceDocuments.Payments != null)
            {
                Console.WriteLine("Payments");
                Console.WriteLine("\tNumberOfEntries: {0}", auditFile.SourceDocuments.Payments.NumberOfEntries);
                Console.WriteLine("\tTotalDebit: {0}", auditFile.SourceDocuments.Payments.TotalDebit);
                Console.WriteLine("\tTotalCredit: {0}", auditFile.SourceDocuments.Payments.TotalCredit);
            }
        }

        private static async Task<Header> ReadHeader(XmlReader reader)
        {
            var header = new Header();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name))
                    continue;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (StringEquals(reader.Name, "AuditFileVersion"))
                        header.AuditFileVersion = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "CompanyID"))
                        header.CompanyID = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "TaxRegistrationNumber"))
                        header.TaxRegistrationNumber = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "CompanyName"))
                        header.CompanyName = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "TaxAccountingBasis"))
                    {
                        if (Enum.TryParse(reader.ReadElementContentAsString(), out TaxAccountingBasis taxAccountingBasis))
                            header.TaxAccountingBasis = taxAccountingBasis;
                    }
                }
            }

            return header;
        }

        private static async Task<AuditFileMasterFiles> ReadMasterFiles(XmlReader reader)
        {
            var masterFiles = new AuditFileMasterFiles();

            var customers = new List<Customer>();
            var products = new List<Product>();
            var taxes = new List<TaxTableEntry>();


            if (StringEquals(reader.Name, "MasterFiles"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (StringEquals(reader.Name, "Customer"))
                        customers.Add(await ReadCustomer(reader.ReadSubtree()));
                    if (StringEquals(reader.Name, "Product"))
                        products.Add(await ReadProduct(reader.ReadSubtree()));
                    if (StringEquals(reader.Name, "TaxTableEntry"))
                        taxes.Add(await ReadTaxTableEntry(reader.ReadSubtree()));
                }
            }

            masterFiles.Customer = customers.ToArray();
            masterFiles.Product = products.ToArray();
            masterFiles.TaxTable = taxes.ToArray();

            return masterFiles;
        }

        private static async Task<SourceDocumentsSalesInvoices> ReadSalesInvoices(XmlReader reader)
        {
            var files = new SourceDocumentsSalesInvoices();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name))
                    continue;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (StringEquals(reader.Name, "NumberOfEntries"))
                        files.NumberOfEntries = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "TotalDebit"))
                    {
                        if (decimal.TryParse(reader.ReadElementContentAsString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal debit))
                            files.TotalDebit = debit;
                    }
                    if (StringEquals(reader.Name, "TotalCredit"))
                    {
                        if (decimal.TryParse(reader.ReadElementContentAsString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal credit))
                            files.TotalCredit = credit;
                    }
                }
            }

            return files;
        }

        private static async Task<SourceDocumentsPayments> ReadPayments(XmlReader reader)
        {
            var files = new SourceDocumentsPayments();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name))
                    continue;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (StringEquals(reader.Name, "NumberOfEntries"))
                        files.NumberOfEntries = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "TotalDebit"))
                    {
                        if (decimal.TryParse(reader.ReadElementContentAsString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal debit))
                            files.TotalDebit = debit;
                    }
                    if (StringEquals(reader.Name, "TotalCredit"))
                    {
                        if (decimal.TryParse(reader.ReadElementContentAsString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal credit))
                            files.TotalCredit = credit;
                    }
                }
            }

            return files;
        }


        private static async Task<Customer> ReadCustomer(XmlReader reader)
        {
            var customer = new Customer();

            if (StringEquals(reader.Name, "Customer"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name))
                    continue;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (StringEquals(reader.Name, "CustomerID"))
                        customer.CustomerID = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "AccountID"))
                        customer.AccountID = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "CustomerTaxID"))
                        customer.CustomerTaxID = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "CompanyName"))
                        customer.CompanyName = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "SelfBillingIndicator"))
                        customer.SelfBillingIndicator = reader.ReadElementContentAsString();
                }
            }

            return customer;
        }

        private static async Task<Product> ReadProduct(XmlReader reader)
        {
            var product = new Product();

            if (StringEquals(reader.Name, "Product"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name))
                    continue;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (StringEquals(reader.Name, "ProductCode"))
                        product.ProductCode = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "ProductGroup"))
                        product.ProductGroup = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "ProductDescription"))
                        product.ProductDescription = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "ProductNumberCode"))
                        product.ProductNumberCode = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "ProductType"))
                    {
                        if (Enum.TryParse(reader.ReadElementContentAsString(), out ProductType productType))
                            product.ProductType = productType;
                    }
                }
            }

            return product;
        }

        private static async Task<TaxTableEntry> ReadTaxTableEntry(XmlReader reader)
        {
            var tax = new TaxTableEntry();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name))
                    continue;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (StringEquals(reader.Name, "TaxCountryRegion"))
                        tax.TaxCountryRegion = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "TaxCode"))
                        tax.TaxCode = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "Description"))
                        tax.Description = reader.ReadElementContentAsString();
                    if (StringEquals(reader.Name, "TaxPercentage"))
                    {
                        tax.ItemElementName = ItemChoiceType2.TaxPercentage;
                        if (decimal.TryParse(reader.ReadElementContentAsString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal taxPercentage))
                            tax.Item = taxPercentage;
                    }
                    if (StringEquals(reader.Name, "TaxAmount"))
                    {
                        tax.ItemElementName = ItemChoiceType2.TaxAmount;
                        if (decimal.TryParse(reader.ReadElementContentAsString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal taxAmount))
                            tax.Item = taxAmount;
                    }
                    if (StringEquals(reader.Name, "TaxType"))
                    {
                        if (Enum.TryParse(reader.ReadElementContentAsString(), out TaxType taxType))
                            tax.TaxType = taxType;
                    }
                }
            }

            return tax;
        }

        private static bool StringEquals(string str1, string str2)
        {
            if (str1 == null || str2 == null)
                return false;

            return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
