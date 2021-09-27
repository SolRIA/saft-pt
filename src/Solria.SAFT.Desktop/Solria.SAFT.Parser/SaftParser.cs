using Solria.SAFT.Parser.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Solria.SAFT.Parser
{
    public static class SaftParser
    {
        private static AuditFile auditFile;
        public static async Task<(AuditFile saftFile, List<ValidationError> errors)> ReadFile(string filename)
        {
            auditFile = new AuditFile
            {
                SourceDocuments = new SourceDocuments()
            };

            //register the Windows-1252 encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using var reader = XmlReader.Create(filename, settings);

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "Header"))
                    {
                        auditFile.Header = await ReadHeader(reader.ReadSubtree());
                    }
                    if (Parsers.StringEquals(reader.Name, "MasterFiles"))
                    {
                        auditFile.MasterFiles = await ReadMasterFiles(reader.ReadSubtree());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SalesInvoices"))
                    {
                        auditFile.SourceDocuments.SalesInvoices = await ReadSalesInvoices(reader.ReadSubtree());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Payments"))
                    {
                        auditFile.SourceDocuments.Payments = await ReadPayments(reader.ReadSubtree());
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return (auditFile, Parsers.Validations);
        }

        private static async Task<Header> ReadHeader(XmlReader reader)
        {
            var header = new Header();

            if (Parsers.StringEquals(reader.Name, "Header") || string.IsNullOrWhiteSpace(reader.Name))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "AuditFileVersion"))
                    {
                        header.AuditFileVersion = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "CompanyName"))
                    {
                        header.CompanyName = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "BusinessName"))
                    {
                        header.BusinessName = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CompanyID"))
                    {
                        header.CompanyID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxRegistrationNumber"))
                    {
                        header.TaxRegistrationNumber = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxAccountingBasis"))
                    {
                        header.TaxAccountingBasis = Parsers.ParseEnum<TaxAccountingBasis>(reader.ReadElementContentAsString(), header.Pk, "Header/TaxAccountingBasis", typeof(Header));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "FiscalYear"))
                    {
                        header.FiscalYear = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "StartDate"))
                    {
                        header.StartDate = Parsers.ParseDate(reader.ReadElementContentAsString(), header.Pk, "Header/StartDate", typeof(Header));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "EndDate"))
                    {
                        header.EndDate = Parsers.ParseDate(reader.ReadElementContentAsString(), header.Pk, "Header/EndDate", typeof(Header));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DateCreated"))
                    {
                        header.DateCreated = Parsers.ParseDate(reader.ReadElementContentAsString(), header.Pk, "Header/DateCreated", typeof(Header));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CurrencyCode"))
                    {
                        header.CurrencyCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxEntity"))
                    {
                        header.TaxEntity = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductCompanyTaxID"))
                    {
                        header.ProductCompanyTaxID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SoftwareCertificateNumber"))
                    {
                        header.SoftwareCertificateNumber = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductID"))
                    {
                        header.ProductID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductVersion"))
                    {
                        header.ProductVersion = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "HeaderComment"))
                    {
                        header.HeaderComment = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Telephone"))
                    {
                        header.Telephone = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Fax"))
                    {
                        header.Fax = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Email"))
                    {
                        header.Email = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Website"))
                    {
                        header.Website = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CompanyAddress"))
                    {
                        header.CompanyAddress = await ReadAddressStructurePT(reader.ReadSubtree());
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
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


            if (Parsers.StringEquals(reader.Name, "MasterFiles"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "Customer"))
                    {
                        customers.Add(await ReadCustomer(reader.ReadSubtree()));
                    }
                    if (Parsers.StringEquals(reader.Name, "Product"))
                    {
                        products.Add(await ReadProduct(reader.ReadSubtree()));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxTableEntry"))
                    {
                        taxes.Add(await ReadTaxTableEntry(reader.ReadSubtree()));
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
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

            var invoices = new List<SourceDocumentsSalesInvoicesInvoice>();
            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "NumberOfEntries"))
                    {
                        files.NumberOfEntries = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "TotalDebit"))
                    {
                        files.TotalDebit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), "", "TotalDebit", typeof(SourceDocumentsSalesInvoices));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TotalCredit"))
                    {
                        files.TotalCredit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), "", "TotalCredit", typeof(SourceDocumentsSalesInvoices));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Invoice"))
                    {
                        invoices.Add(await ReadInvoices(reader.ReadSubtree()));
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            files.Invoice = invoices.ToArray();

            return files;
        }

        private static async Task<SourceDocumentsSalesInvoicesInvoice> ReadInvoices(XmlReader reader)
        {
            var invoice = new SourceDocumentsSalesInvoicesInvoice();
            var lines = new List<SourceDocumentsSalesInvoicesInvoiceLine>();

            if (Parsers.StringEquals(reader.Name, "Invoice"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "InvoiceNo"))
                    {
                        invoice.InvoiceNo = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "ATCUD"))
                    {
                        invoice.ATCUD = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Hash"))
                    {
                        invoice.Hash = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "HashControl"))
                    {
                        invoice.HashControl = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Period"))
                    {
                        invoice.Period = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "InvoiceDate"))
                    {
                        invoice.InvoiceDate = Parsers.ParseDate(reader.ReadElementContentAsString(), invoice.Pk, "InvoiceDate", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "InvoiceType"))
                    {
                        invoice.InvoiceType = Parsers.ParseEnum<InvoiceType>(reader.ReadElementContentAsString(), invoice.Pk, "InvoiceType", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        invoice.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "EACCode"))
                    {
                        invoice.EACCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SystemEntryDate"))
                    {
                        invoice.SystemEntryDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), invoice.Pk, "SystemEntryDate", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CustomerID"))
                    {
                        invoice.CustomerID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Line"))
                    {
                        lines.Add(await ReadInvoiceLines(reader.ReadSubtree(), invoice.Pk));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentStatus"))
                    {
                        invoice.DocumentStatus = await ReadInvoiceStatus(reader.ReadSubtree(), invoice.Pk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SpecialRegimes"))
                    {
                        invoice.SpecialRegimes = await ReadInvoiceSpecialRegimes(reader.ReadSubtree());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentTotals"))
                    {
                        invoice.DocumentTotals = await ReadInvoiceDocumentTotals(reader.ReadSubtree(), invoice.Pk);
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            invoice.Line = lines.ToArray();

            return invoice;
        }

        private static async Task<SourceDocumentsSalesInvoicesInvoiceDocumentStatus> ReadInvoiceStatus(XmlReader reader, string pk)
        {
            var status = new SourceDocumentsSalesInvoicesInvoiceDocumentStatus();

            if (Parsers.StringEquals(reader.Name, "DocumentStatus"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "InvoiceStatus"))
                    {
                        status.InvoiceStatus = Parsers.ParseEnum<InvoiceStatus>(reader.ReadElementContentAsString(), pk, "Invoice/DocumentStatus/InvoiceStatus", typeof(SourceDocumentsSalesInvoicesInvoice));
                    }
                    if (Parsers.StringEquals(reader.Name, "InvoiceStatusDate"))
                    {
                        status.InvoiceStatusDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), pk, "Invoice/DocumentStatus/InvoiceStatus", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        status.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceBilling"))
                    {
                        status.SourceBilling = Parsers.ParseEnum<SAFTPTSourceBilling>(reader.ReadElementContentAsString(), pk, "Invoice/DocumentStatus/SourceBilling", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return status;
        }

        private static async Task<SpecialRegimes> ReadInvoiceSpecialRegimes(XmlReader reader)
        {
            var regimes = new SpecialRegimes();

            if (Parsers.StringEquals(reader.Name, "SpecialRegimes"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "SelfBillingIndicator"))
                    {
                        regimes.SelfBillingIndicator = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "CashVATSchemeIndicator"))
                    {
                        regimes.CashVATSchemeIndicator = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ThirdPartiesBillingIndicator"))
                    {
                        regimes.ThirdPartiesBillingIndicator = reader.ReadElementContentAsString();
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return regimes;
        }

        private static async Task<SourceDocumentsSalesInvoicesInvoiceDocumentTotals> ReadInvoiceDocumentTotals(XmlReader reader, string pk)
        {
            var totals = new SourceDocumentsSalesInvoicesInvoiceDocumentTotals();
            var payments = new List<PaymentMethod>();

            if (Parsers.StringEquals(reader.Name, "DocumentTotals"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "TaxPayable"))
                    {
                        totals.TaxPayable = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, "Invoice/DocumentTotals/TaxPayable", typeof(SourceDocumentsSalesInvoicesInvoice));
                    }
                    if (Parsers.StringEquals(reader.Name, "NetTotal"))
                    {
                        totals.NetTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, "Invoice/DocumentTotals/NetTotal", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        totals.GrossTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, "Invoice/DocumentTotals/GrossTotal", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        payments.Add(await ReadInvoiceDocumentTotalsPayment(reader.ReadSubtree(), pk));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            totals.Payment = payments.ToArray();

            return totals;
        }

        private static async Task<PaymentMethod> ReadInvoiceDocumentTotalsPayment(XmlReader reader, string pk)
        {
            var payment = new PaymentMethod();

            if (Parsers.StringEquals(reader.Name, "Payment"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "PaymentAmount"))
                    {
                        payment.PaymentAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, "Invoice/DocumentTotals/Payment/PaymentAmount", typeof(SourceDocumentsSalesInvoicesInvoice));
                    }
                    if (Parsers.StringEquals(reader.Name, "PaymentMechanism"))
                    {
                        payment.PaymentMechanism = Parsers.ParseEnum<PaymentMechanism>(reader.ReadElementContentAsString(), pk, "Invoice/DocumentTotals/Payment/PaymentMechanism", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "PaymentDate"))
                    {
                        payment.PaymentDate = Parsers.ParseDate(reader.ReadElementContentAsString(), pk, "Invoice/DocumentTotals/Payment/PaymentDate", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return payment;
        }

        private static async Task<SourceDocumentsSalesInvoicesInvoiceLine> ReadInvoiceLines(XmlReader reader, string invoicePk)
        {
            var line = new SourceDocumentsSalesInvoicesInvoiceLine();

            if (Parsers.StringEquals(reader.Name, "Line"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name))
                    continue;
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "LineNumber"))
                    {
                        line.LineNumber = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductCode"))
                    {
                        line.ProductCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductDescription"))
                    {
                        line.ProductDescription = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Quantity"))
                    {
                        line.Quantity = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, "Invoice/Line/Quantity", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: invoicePk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "UnitOfMeasure"))
                    {
                        line.UnitOfMeasure = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "UnitPrice"))
                    {
                        line.UnitPrice = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, "Invoice/Line/UnitPrice", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: invoicePk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxPointDate"))
                    {
                        line.TaxPointDate = Parsers.ParseDate(reader.ReadElementContentAsString(), line.Pk, "Invoice/Line/TaxPointDate", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: invoicePk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Description"))
                    {
                        line.Description = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CreditAmount"))
                    {
                        line.ItemElementName = ItemChoiceType4.CreditAmount;
                        line.CreditAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, "Invoice/Line/CreditAmount", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: invoicePk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DebitAmount"))
                    {
                        line.ItemElementName = ItemChoiceType4.DebitAmount;
                        line.DebitAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, "Invoice/Line/DebitAmount", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: invoicePk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Tax"))
                    {
                        line.Tax = await ReadInvoiceLineTax(reader.ReadSubtree(), line.Pk, invoicePk);
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return line;
        }

        private static async Task<Tax> ReadInvoiceLineTax(XmlReader reader, string linePk, string invoicePk)
        {
            var tax = new Tax();

            if (Parsers.StringEquals(reader.Name, "Tax"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "TaxCountryRegion"))
                    {
                        tax.TaxCountryRegion = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxCode"))
                    {
                        tax.TaxCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxType"))
                    {
                        tax.TaxType = Parsers.ParseEnum<TaxType>(reader.ReadElementContentAsString(), linePk, "TaxType", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: invoicePk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxPercentage"))
                    {
                        tax.ItemElementName = ItemChoiceType1.TaxPercentage;
                        tax.TaxPercentage = Parsers.ParseDecimal(reader.ReadElementContentAsString(), linePk, "TaxPercentage", typeof(SourceDocumentsPayments), supPk: invoicePk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxAmount"))
                    {
                        tax.ItemElementName = ItemChoiceType1.TaxAmount;
                        tax.TaxAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), linePk, "TaxAmount", typeof(SourceDocumentsPayments), supPk: invoicePk);
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return tax;
        }

        private static async Task<SourceDocumentsPayments> ReadPayments(XmlReader reader)
        {
            var files = new SourceDocumentsPayments();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "NumberOfEntries"))
                    {
                        files.NumberOfEntries = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "TotalDebit"))
                    {
                        files.TotalDebit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), "SalesInvoices", "TotalDebit", typeof(SourceDocumentsPayments));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TotalCredit"))
                    {
                        files.TotalCredit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), "SalesInvoices", "TotalCredit", typeof(SourceDocumentsPayments));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return files;
        }

        private static async Task<Customer> ReadCustomer(XmlReader reader)
        {
            var customer = new Customer();

            if (Parsers.StringEquals(reader.Name, "Customer"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "CustomerID"))
                    {
                        customer.CustomerID = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "AccountID"))
                    {
                        customer.AccountID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CustomerTaxID"))
                    {
                        customer.CustomerTaxID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CompanyName"))
                    {
                        customer.CompanyName = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SelfBillingIndicator"))
                    {
                        customer.SelfBillingIndicator = reader.ReadElementContentAsString();
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return customer;
        }

        private static async Task<AddressStructurePT> ReadAddressStructurePT(XmlReader reader)
        {
            var address = new AddressStructurePT();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "BuildingNumber"))
                    {
                        address.BuildingNumber = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "StreetName"))
                    {
                        address.StreetName = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "AddressDetail"))
                    {
                        address.AddressDetail = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "City"))
                    {
                        address.City = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "PostalCode"))
                    {
                        address.PostalCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Region"))
                    {
                        address.Region = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Country"))
                    {
                        address.Country = reader.ReadElementContentAsString();
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return address;
        }

        private static async Task<Product> ReadProduct(XmlReader reader)
        {
            var product = new Product();

            if (Parsers.StringEquals(reader.Name, "Product"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "ProductCode"))
                    {
                        product.ProductCode = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductGroup"))
                    {
                        product.ProductGroup = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductDescription"))
                    {
                        product.ProductDescription = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductNumberCode"))
                    {
                        product.ProductNumberCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductType"))
                    {
                        product.ProductType = Parsers.ParseEnum<ProductType>(reader.ReadElementContentAsString(), product.Pk, "ProductType", typeof(Product));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return product;
        }

        private static async Task<TaxTableEntry> ReadTaxTableEntry(XmlReader reader)
        {
            var tax = new TaxTableEntry();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "TaxCountryRegion"))
                    {
                        tax.TaxCountryRegion = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxCode"))
                    {
                        tax.TaxCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Description"))
                    {
                        tax.Description = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxPercentage"))
                    {
                        tax.ItemElementName = ItemChoiceType2.TaxPercentage;
                        tax.TaxPercentage = Parsers.ParseDecimal(reader.ReadElementContentAsString(), tax.Pk, "TaxPercentage", typeof(TaxTableEntry));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxAmount"))
                    {
                        tax.ItemElementName = ItemChoiceType2.TaxAmount;
                        tax.TaxAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), tax.Pk, "TaxAmount", typeof(TaxTableEntry));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxType"))
                    {
                        tax.TaxType = Parsers.ParseEnum<TaxType>(reader.ReadElementContentAsString(), tax.Pk, "TaxType", typeof(TaxTableEntry));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return tax;
        }
    }
}
