using Solria.SAFT.Parser.Models;
using System;
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
                    if (Parsers.StringEquals(reader.Name, "MovementOfGoods"))
                    {
                        auditFile.SourceDocuments.MovementOfGoods = await ReadMovementOfGoods(reader.ReadSubtree());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WorkingDocuments"))
                    {
                        auditFile.SourceDocuments.WorkingDocuments = await ReadWorkingDocuments(reader.ReadSubtree());
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
                        header.TaxAccountingBasis = Parsers.ParseEnum<TaxAccountingBasis>(reader.ReadElementContentAsString(), header.Pk, string.Empty, "Header/TaxAccountingBasis", typeof(Header));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "FiscalYear"))
                    {
                        header.FiscalYear = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "StartDate"))
                    {
                        header.StartDate = Parsers.ParseDate(reader.ReadElementContentAsString(), header.Pk, string.Empty, "Header/StartDate", typeof(Header));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "EndDate"))
                    {
                        header.EndDate = Parsers.ParseDate(reader.ReadElementContentAsString(), header.Pk, string.Empty, "Header/EndDate", typeof(Header));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DateCreated"))
                    {
                        header.DateCreated = Parsers.ParseDate(reader.ReadElementContentAsString(), header.Pk, string.Empty, "Header/DateCreated", typeof(Header));
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
                        files.TotalDebit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), string.Empty, string.Empty, "SalesInvoices/TotalDebit", typeof(SourceDocumentsSalesInvoices));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TotalCredit"))
                    {
                        files.TotalCredit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), string.Empty, string.Empty, "SalesInvoices/TotalCredit", typeof(SourceDocumentsSalesInvoices));
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
        private static async Task<SourceDocumentsMovementOfGoods> ReadMovementOfGoods(XmlReader reader)
        {
            var files = new SourceDocumentsMovementOfGoods();

            var documents = new List<SourceDocumentsMovementOfGoodsStockMovement>();
            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "NumberOfMovementLines"))
                    {
                        files.NumberOfMovementLines = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "TotalQuantityIssued"))
                    {
                        files.TotalQuantityIssued = Parsers.ParseDecimal(reader.ReadElementContentAsString(), string.Empty, string.Empty, "MovementOfGoods/TotalQuantityIssued", typeof(SourceDocumentsMovementOfGoods));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "StockMovement"))
                    {
                        documents.Add(await ReadStockMovement(reader.ReadSubtree()));
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            files.StockMovement = documents.ToArray();

            return files;
        }
        private static async Task<SourceDocumentsWorkingDocuments> ReadWorkingDocuments(XmlReader reader)
        {
            var files = new SourceDocumentsWorkingDocuments();

            var documents = new List<SourceDocumentsWorkingDocumentsWorkDocument>();
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
                        files.TotalDebit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), "", "", "WorkingDocuments/TotalDebit", typeof(SourceDocumentsWorkingDocuments));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TotalCredit"))
                    {
                        files.TotalCredit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), "", "", "WorkingDocuments/TotalCredit", typeof(SourceDocumentsWorkingDocuments));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WorkDocument"))
                    {
                        documents.Add(await ReadWorkDocument(reader.ReadSubtree()));
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            files.WorkDocument = documents.ToArray();

            return files;
        }
        private static async Task<SourceDocumentsPayments> ReadPayments(XmlReader reader)
        {
            var files = new SourceDocumentsPayments();
            var documents = new List<SourceDocumentsPaymentsPayment>();

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
                        files.TotalDebit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), string.Empty, string.Empty, "Payments/TotalDebit", typeof(SourceDocumentsPayments));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TotalCredit"))
                    {
                        files.TotalCredit = Parsers.ParseDecimal(reader.ReadElementContentAsString(), string.Empty, string.Empty, "Payments/TotalCredit", typeof(SourceDocumentsPayments));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Payment"))
                    {
                        documents.Add(await ReadPayment(reader.ReadSubtree()));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            files.Payment = documents.ToArray();

            return files;
        }


        private static async Task<SourceDocumentsSalesInvoicesInvoice> ReadInvoices(XmlReader reader)
        {
            var invoice = new SourceDocumentsSalesInvoicesInvoice();
            var lines = new List<SourceDocumentsSalesInvoicesInvoiceLine>();
            var withholdingTax = new List<WithholdingTax>();

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
                        invoice.InvoiceDate = Parsers.ParseDate(reader.ReadElementContentAsString(), invoice.Pk, invoice.InvoiceNo, "Invoice/InvoiceDate", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "InvoiceType"))
                    {
                        invoice.InvoiceType = Parsers.ParseEnum<InvoiceType>(reader.ReadElementContentAsString(), invoice.Pk, invoice.InvoiceNo, "Invoice/InvoiceType", typeof(SourceDocumentsSalesInvoicesInvoice));
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
                        invoice.SystemEntryDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), invoice.Pk, invoice.InvoiceNo, "Invoice/SystemEntryDate", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "MovementEndTime"))
                    {
                        invoice.MovementEndTime = Parsers.ParseDateTime(reader.ReadElementContentAsString(), invoice.Pk, invoice.InvoiceNo, "Invoice/MovementEndTime", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "MovementStartTime"))
                    {
                        invoice.MovementStartTime = Parsers.ParseDateTime(reader.ReadElementContentAsString(), invoice.Pk, invoice.InvoiceNo, "Invoice/MovementStartTime", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TransactionID"))
                    {
                        invoice.TransactionID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CustomerID"))
                    {
                        invoice.CustomerID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Line"))
                    {
                        lines.Add(await ReadInvoiceLines(reader.ReadSubtree(), invoice.Pk, invoice.InvoiceNo));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentStatus"))
                    {
                        invoice.DocumentStatus = await ReadInvoiceStatus(reader.ReadSubtree(), invoice.Pk, invoice.InvoiceNo);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SpecialRegimes"))
                    {
                        invoice.SpecialRegimes = await ReadInvoiceSpecialRegimes(reader.ReadSubtree());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentTotals"))
                    {
                        invoice.DocumentTotals = await ReadInvoiceDocumentTotals(reader.ReadSubtree(), invoice.Pk, invoice.InvoiceNo);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ShipTo"))
                    {
                        invoice.ShipTo = await ReadShippingPointStructure(reader.ReadSubtree(), invoice.Pk, invoice.InvoiceNo);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ShipFrom"))
                    {
                        invoice.ShipFrom = await ReadShippingPointStructure(reader.ReadSubtree(), invoice.Pk, invoice.InvoiceNo);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WithholdingTax"))
                    {
                        withholdingTax.Add(await ReadWithholdingTax(reader.ReadSubtree(), invoice.Pk, invoice.InvoiceNo, "Invoice", typeof(SourceDocumentsSalesInvoicesInvoice)));
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            invoice.Line = lines.ToArray();
            invoice.WithholdingTax = withholdingTax.ToArray();

            return invoice;
        }
        private static async Task<SourceDocumentsMovementOfGoodsStockMovement> ReadStockMovement(XmlReader reader)
        {
            var document = new SourceDocumentsMovementOfGoodsStockMovement();
            var lines = new List<SourceDocumentsMovementOfGoodsStockMovementLine>();

            if (Parsers.StringEquals(reader.Name, "StockMovement"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "DocumentNumber"))
                    {
                        document.DocumentNumber = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "ATCUD"))
                    {
                        document.ATCUD = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Hash"))
                    {
                        document.Hash = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "HashControl"))
                    {
                        document.HashControl = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Period"))
                    {
                        document.Period = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "MovementDate"))
                    {
                        document.MovementDate = Parsers.ParseDate(reader.ReadElementContentAsString(), document.Pk, document.DocumentNumber, "StockMovement/MovementDate", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "MovementType"))
                    {
                        document.MovementType = Parsers.ParseEnum<MovementType>(reader.ReadElementContentAsString(), document.Pk, document.DocumentNumber, "StockMovement/MovementType", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        document.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "EACCode"))
                    {
                        document.EACCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SystemEntryDate"))
                    {
                        document.SystemEntryDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), document.Pk, document.DocumentNumber, "StockMovement/SystemEntryDate", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "MovementEndTime"))
                    {
                        document.MovementEndTime = Parsers.ParseDateTime(reader.ReadElementContentAsString(), document.Pk, document.DocumentNumber, "StockMovement/MovementEndTime", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "MovementStartTime"))
                    {
                        document.MovementStartTime = Parsers.ParseDateTime(reader.ReadElementContentAsString(), document.Pk, document.DocumentNumber, "StockMovement/MovementStartTime", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ATDocCodeID"))
                    {
                        document.ATDocCodeID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TransactionID"))
                    {
                        document.TransactionID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CustomerID"))
                    {
                        document.CustomerID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Line"))
                    {
                        lines.Add(await ReadStockMovementLines(reader.ReadSubtree(), document.Pk, document.DocumentNumber));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentStatus"))
                    {
                        document.DocumentStatus = await ReadStockMovementStatus(reader.ReadSubtree(), document.Pk, document.DocumentNumber);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentTotals"))
                    {
                        document.DocumentTotals = await ReadStockMovementsDocumentTotals(reader.ReadSubtree(), document.Pk, document.DocumentNumber);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ShipTo"))
                    {
                        document.ShipTo = await ReadShippingPointStructure(reader.ReadSubtree(), document.Pk, document.DocumentNumber);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ShipFrom"))
                    {
                        document.ShipFrom = await ReadShippingPointStructure(reader.ReadSubtree(), document.Pk, document.DocumentNumber);
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            document.Line = lines.ToArray();

            return document;
        }
        private static async Task<SourceDocumentsWorkingDocumentsWorkDocument> ReadWorkDocument(XmlReader reader)
        {
            var document = new SourceDocumentsWorkingDocumentsWorkDocument();
            var lines = new List<SourceDocumentsWorkingDocumentsWorkDocumentLine>();

            if (Parsers.StringEquals(reader.Name, "WorkDocument"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "DocumentNumber"))
                    {
                        document.DocumentNumber = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "ATCUD"))
                    {
                        document.ATCUD = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Hash"))
                    {
                        document.Hash = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "HashControl"))
                    {
                        document.HashControl = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Period"))
                    {
                        document.Period = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WorkDate"))
                    {
                        document.WorkDate = Parsers.ParseDate(reader.ReadElementContentAsString(), document.Pk,document.DocumentNumber, "WorkDocument/WorkDate", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WorkType"))
                    {
                        document.WorkType = Parsers.ParseEnum<WorkType>(reader.ReadElementContentAsString(), document.Pk, document.DocumentNumber, "WorkDocument/WorkType", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        document.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "EACCode"))
                    {
                        document.EACCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SystemEntryDate"))
                    {
                        document.SystemEntryDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), document.Pk, document.DocumentNumber, "WorkDocument/SystemEntryDate", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TransactionID"))
                    {
                        document.TransactionID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CustomerID"))
                    {
                        document.CustomerID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Line"))
                    {
                        lines.Add(await ReadWorkDocumentLines(reader.ReadSubtree(), document.Pk, document.DocumentNumber));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentStatus"))
                    {
                        document.DocumentStatus = await ReadWorkDocumentStatus(reader.ReadSubtree(), document.Pk, document.DocumentNumber);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentTotals"))
                    {
                        document.DocumentTotals = await ReadWorkDocumentDocumentTotals(reader.ReadSubtree(), document.Pk, document.DocumentNumber);
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            document.Line = lines.ToArray();

            return document;
        }
        private static async Task<SourceDocumentsPaymentsPayment> ReadPayment(XmlReader reader)
        {
            var document = new SourceDocumentsPaymentsPayment();
            var lines = new List<SourceDocumentsPaymentsPaymentLine>();
            var withholdingTaxes = new List<WithholdingTax>();
            var paymentMethods = new List<PaymentMethod>();

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

                    if (Parsers.StringEquals(reader.Name, "PaymentRefNo"))
                    {
                        document.PaymentRefNo = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "ATCUD"))
                    {
                        document.ATCUD = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Period"))
                    {
                        document.Period = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TransactionDate"))
                    {
                        document.TransactionDate = Parsers.ParseDate(reader.ReadElementContentAsString(), document.Pk, document.PaymentRefNo, "Payment/TransactionDate", typeof(SourceDocumentsPaymentsPayment));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "PaymentType"))
                    {
                        document.PaymentType = Parsers.ParseEnum<SAFTPTPaymentType>(reader.ReadElementContentAsString(), document.Pk, document.PaymentRefNo, "Payment/PaymentType", typeof(SourceDocumentsPaymentsPayment));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Description"))
                    {
                        document.Description = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        document.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SystemEntryDate"))
                    {
                        document.SystemEntryDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), document.Pk, document.PaymentRefNo, "Payment/SystemEntryDate", typeof(SourceDocumentsPaymentsPayment));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TransactionID"))
                    {
                        document.TransactionID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CustomerID"))
                    {
                        document.CustomerID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Line"))
                    {
                        lines.Add(await ReadPaymentLines(reader.ReadSubtree(), document.Pk, document.PaymentRefNo));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentStatus"))
                    {
                        document.DocumentStatus = await ReadPaymentStatus(reader.ReadSubtree(), document.Pk, document.PaymentRefNo);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DocumentTotals"))
                    {
                        document.DocumentTotals = await ReadPaymentDocumentTotals(reader.ReadSubtree(), document.Pk, document.PaymentRefNo);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WithholdingTax"))
                    {
                        withholdingTaxes.Add(await ReadWithholdingTax(reader.ReadSubtree(), document.Pk, document.PaymentRefNo, "Payment", typeof(SourceDocumentsPaymentsPayment)));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "PaymentMethod"))
                    {
                        paymentMethods.Add(await ReadPaymentMethod(reader.ReadSubtree(), document.Pk, document.PaymentRefNo, "Payment/PaymentMethod", typeof(SourceDocumentsPaymentsPayment)));
                        continue;
                    }

                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            document.Line = lines.ToArray();
            document.WithholdingTax = withholdingTaxes.ToArray();
            document.PaymentMethod = paymentMethods.ToArray();

            return document;
        }


        private static async Task<SourceDocumentsSalesInvoicesInvoiceDocumentStatus> ReadInvoiceStatus(XmlReader reader, string pk, string id)
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
                        status.InvoiceStatus = Parsers.ParseEnum<InvoiceStatus>(reader.ReadElementContentAsString(), pk, id, "Invoice/DocumentStatus/InvoiceStatus", typeof(SourceDocumentsSalesInvoicesInvoice));
                    }
                    if (Parsers.StringEquals(reader.Name, "InvoiceStatusDate"))
                    {
                        status.InvoiceStatusDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), pk, id, "Invoice/DocumentStatus/InvoiceStatus", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Reason"))
                    {
                        status.Reason = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        status.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceBilling"))
                    {
                        status.SourceBilling = Parsers.ParseEnum<SAFTPTSourceBilling>(reader.ReadElementContentAsString(), pk, id, "Invoice/DocumentStatus/SourceBilling", typeof(SourceDocumentsSalesInvoicesInvoice));
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
        private static async Task<SourceDocumentsMovementOfGoodsStockMovementDocumentStatus> ReadStockMovementStatus(XmlReader reader, string pk, string id)
        {
            var status = new SourceDocumentsMovementOfGoodsStockMovementDocumentStatus();

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

                    if (Parsers.StringEquals(reader.Name, "MovementStatus"))
                    {
                        status.MovementStatus = Parsers.ParseEnum<MovementStatus>(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentStatus/MovementStatus", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                    }
                    if (Parsers.StringEquals(reader.Name, "MovementStatusDate"))
                    {
                        status.MovementStatusDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentStatus/MovementStatusDate", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Reason"))
                    {
                        status.Reason = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        status.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceBilling"))
                    {
                        status.SourceBilling = Parsers.ParseEnum<SAFTPTSourceBilling>(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentStatus/SourceBilling", typeof(SourceDocumentsMovementOfGoodsStockMovement));
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
        private static async Task<SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus> ReadWorkDocumentStatus(XmlReader reader, string pk, string id)
        {
            var status = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentStatus();

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

                    if (Parsers.StringEquals(reader.Name, "WorkStatus"))
                    {
                        status.WorkStatus = Parsers.ParseEnum<WorkStatus>(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentStatus/WorkStatus", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                    }
                    if (Parsers.StringEquals(reader.Name, "WorkStatusDate"))
                    {
                        status.WorkStatusDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentStatus/WorkStatusDate", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Reason"))
                    {
                        status.Reason = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        status.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceBilling"))
                    {
                        status.SourceBilling = Parsers.ParseEnum<SAFTPTSourceBilling>(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentStatus/SourceBilling", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
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
        private static async Task<SourceDocumentsPaymentsPaymentDocumentStatus> ReadPaymentStatus(XmlReader reader, string pk, string id)
        {
            var status = new SourceDocumentsPaymentsPaymentDocumentStatus();

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

                    if (Parsers.StringEquals(reader.Name, "PaymentStatus"))
                    {
                        status.PaymentStatus = Parsers.ParseEnum<PaymentStatus>(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentStatus/PaymentStatus", typeof(SourceDocumentsPaymentsPayment));
                    }
                    if (Parsers.StringEquals(reader.Name, "PaymentStatusDate"))
                    {
                        status.PaymentStatusDate = Parsers.ParseDateTime(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentStatus/PaymentStatusDate", typeof(SourceDocumentsPaymentsPayment));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Reason"))
                    {
                        status.Reason = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourceID"))
                    {
                        status.SourceID = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SourcePayment"))
                    {
                        status.SourcePayment = Parsers.ParseEnum<SAFTPTSourcePayment>(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentStatus/SourcePayment", typeof(SourceDocumentsPaymentsPayment));
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

        private static async Task<WithholdingTax> ReadWithholdingTax(XmlReader reader, string pk, string id, string basePath, Type type)
        {
            var ship = new WithholdingTax();

            if (Parsers.StringEquals(reader.Name, "WithholdingTax"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "WithholdingTaxType"))
                    {
                        ship.WithholdingTaxType = Parsers.ParseEnum<WithholdingTaxType>(reader.ReadElementContentAsString(), pk, id, $"{basePath}/WithholdingTax/WithholdingTaxType", type);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WithholdingTaxDescription"))
                    {
                        ship.WithholdingTaxDescription = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WithholdingTaxAmount"))
                    {
                        ship.WithholdingTaxAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, $"{basePath}/WithholdingTax/WithholdingTaxAmount", type);
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return ship;
        }

        private static async Task<ShippingPointStructure> ReadShippingPointStructure(XmlReader reader, string pk, string id)
        {
            var ship = new ShippingPointStructure();
            var deliveryID = new List<string>();
            var warehouseID = new List<string>();
            var locationID = new List<string>();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "DeliveryID"))
                    {
                        deliveryID.Add(reader.ReadElementContentAsString());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DeliveryDate"))
                    {
                        ship.DeliveryDate = Parsers.ParseDate(reader.ReadElementContentAsString(), pk, id, "DeliveryDate", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "WarehouseID"))
                    {
                        warehouseID.Add(reader.ReadElementContentAsString());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "LocationID"))
                    {
                        locationID.Add(reader.ReadElementContentAsString());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Address"))
                    {
                        ship.Address = await ReadAddressStructure(reader.ReadSubtree());
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            ship.DeliveryID = deliveryID.ToArray();
            ship.WarehouseID = warehouseID.ToArray();
            ship.LocationID = locationID.ToArray();

            return ship;
        }

        private static async Task<SourceDocumentsSalesInvoicesInvoiceDocumentTotals> ReadInvoiceDocumentTotals(XmlReader reader, string pk, string id)
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
                        totals.TaxPayable = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Invoice/DocumentTotals/TaxPayable", typeof(SourceDocumentsSalesInvoicesInvoice));
                    }
                    if (Parsers.StringEquals(reader.Name, "NetTotal"))
                    {
                        totals.NetTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Invoice/DocumentTotals/NetTotal", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        totals.GrossTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Invoice/DocumentTotals/GrossTotal", typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Payment"))
                    {
                        payments.Add(await ReadPaymentMethod(reader.ReadSubtree(), pk, id, "Invoice/DocumentTotals/Payment", typeof(SourceDocumentsSalesInvoicesInvoice)));
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
        private static async Task<SourceDocumentsMovementOfGoodsStockMovementDocumentTotals> ReadStockMovementsDocumentTotals(XmlReader reader, string pk, string id)
        {
            var totals = new SourceDocumentsMovementOfGoodsStockMovementDocumentTotals();

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
                        totals.TaxPayable = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentTotals/TaxPayable", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                    }
                    if (Parsers.StringEquals(reader.Name, "NetTotal"))
                    {
                        totals.NetTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentTotals/NetTotal", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        totals.GrossTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentTotals/GrossTotal", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        totals.GrossTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentTotals/GrossTotal", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CurrencyCode"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.CurrencyCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CurrencyAmount"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.CurrencyAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentTotals/Currency/CurrencyAmount", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ExchangeRate"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.ExchangeRate = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "StockMovement/DocumentTotals/Currency/ExchangeRate", typeof(SourceDocumentsMovementOfGoodsStockMovement));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return totals;
        }
        private static async Task<SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals> ReadWorkDocumentDocumentTotals(XmlReader reader, string pk, string id)
        {
            var totals = new SourceDocumentsWorkingDocumentsWorkDocumentDocumentTotals();

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
                        totals.TaxPayable = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentTotals/TaxPayable", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                    }
                    if (Parsers.StringEquals(reader.Name, "NetTotal"))
                    {
                        totals.NetTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentTotals/NetTotal", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        totals.GrossTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentTotals/GrossTotal", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        totals.GrossTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentTotals/GrossTotal", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CurrencyCode"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.CurrencyCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CurrencyAmount"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.CurrencyAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentTotals/Currency/CurrencyAmount", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ExchangeRate"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.ExchangeRate = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "WorkDocument/DocumentTotals/Currency/ExchangeRate", typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return totals;
        }
        private static async Task<SourceDocumentsPaymentsPaymentDocumentTotals> ReadPaymentDocumentTotals(XmlReader reader, string pk, string id)
        {
            var totals = new SourceDocumentsPaymentsPaymentDocumentTotals();

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
                        totals.TaxPayable = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentTotals/TaxPayable", typeof(SourceDocumentsPaymentsPaymentDocumentTotals));
                    }
                    if (Parsers.StringEquals(reader.Name, "NetTotal"))
                    {
                        totals.NetTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentTotals/NetTotal", typeof(SourceDocumentsPaymentsPaymentDocumentTotals));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        totals.GrossTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentTotals/GrossTotal", typeof(SourceDocumentsPaymentsPaymentDocumentTotals));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "GrossTotal"))
                    {
                        totals.GrossTotal = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentTotals/GrossTotal", typeof(SourceDocumentsPaymentsPaymentDocumentTotals));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Settlement"))
                    {
                        totals.Settlement = new SourceDocumentsPaymentsPaymentDocumentTotalsSettlement
                        {
                            SettlementAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentTotals/Settlement/SettlementAmount", typeof(SourceDocumentsPaymentsPaymentDocumentTotals))
                        };
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CurrencyCode"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.CurrencyCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CurrencyAmount"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.CurrencyAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentTotals/Currency/CurrencyAmount", typeof(SourceDocumentsPaymentsPaymentDocumentTotals));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ExchangeRate"))
                    {
                        if (totals.Currency == null)
                            totals.Currency = new Currency();

                        totals.Currency.ExchangeRate = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, "Payment/DocumentTotals/Currency/ExchangeRate", typeof(SourceDocumentsPaymentsPaymentDocumentTotals));
                        continue;
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return totals;
        }

        private static async Task<PaymentMethod> ReadPaymentMethod(XmlReader reader, string pk, string id, string basePath, Type type)
        {
            var payment = new PaymentMethod();

            if (Parsers.StringEquals(reader.Name, "Payment") || Parsers.StringEquals(reader.Name, "PaymentMethod"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "PaymentMechanism"))
                    {
                        payment.PaymentMechanism = Parsers.ParseEnum<PaymentMechanism>(reader.ReadElementContentAsString(), pk, id, $"{basePath}/PaymentMechanism", type);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "PaymentAmount"))
                    {
                        payment.PaymentAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), pk, id, $"{basePath}/PaymentAmount", type);
                    }
                    if (Parsers.StringEquals(reader.Name, "PaymentDate"))
                    {
                        payment.PaymentDate = Parsers.ParseDate(reader.ReadElementContentAsString(), pk, id, $"{basePath}/PaymentDate", type);
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

        private static async Task<SourceDocumentsSalesInvoicesInvoiceLine> ReadInvoiceLines(XmlReader reader, string supPk, string id)
        {
            var line = new SourceDocumentsSalesInvoicesInvoiceLine();

            if (Parsers.StringEquals(reader.Name, "Line"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
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
                        line.Quantity = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk,id, "Invoice/Line/Quantity", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "UnitOfMeasure"))
                    {
                        line.UnitOfMeasure = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "UnitPrice"))
                    {
                        line.UnitPrice = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "Invoice/Line/UnitPrice", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxPointDate"))
                    {
                        line.TaxPointDate = Parsers.ParseDate(reader.ReadElementContentAsString(), line.Pk, id, "Invoice/Line/TaxPointDate", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: supPk);
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
                        line.CreditAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "Invoice/Line/CreditAmount", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DebitAmount"))
                    {
                        line.ItemElementName = ItemChoiceType4.DebitAmount;
                        line.DebitAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "Invoice/Line/DebitAmount", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Tax"))
                    {
                        line.Tax = await ReadTax(reader.ReadSubtree(), line.Pk, supPk, id, typeof(SourceDocumentsSalesInvoicesInvoice));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxExemptionReason"))
                    {
                        line.TaxExemptionReason = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxExemptionCode"))
                    {
                        line.TaxExemptionCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SettlementAmount"))
                    {
                        line.SettlementAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "Invoice/Line/DebitAmount", typeof(SourceDocumentsSalesInvoicesInvoice), supPk: supPk);
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
        private static async Task<SourceDocumentsMovementOfGoodsStockMovementLine> ReadStockMovementLines(XmlReader reader, string supPk, string id)
        {
            var line = new SourceDocumentsMovementOfGoodsStockMovementLine();
            var productSerialNumbers = new List<string>();

            if (Parsers.StringEquals(reader.Name, "Line"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
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
                        line.Quantity = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "StockMovement/Line/Quantity", typeof(SourceDocumentsMovementOfGoodsStockMovement), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "UnitOfMeasure"))
                    {
                        line.UnitOfMeasure = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "UnitPrice"))
                    {
                        line.UnitPrice = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "StockMovement/Line/UnitPrice", typeof(SourceDocumentsMovementOfGoodsStockMovement), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Description"))
                    {
                        line.Description = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductSerialNumber"))
                    {
                        productSerialNumbers.Add(reader.ReadElementContentAsString());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CreditAmount"))
                    {
                        line.ItemElementName = ItemChoiceType6.CreditAmount;
                        line.CreditAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "StockMovement/Line/CreditAmount", typeof(SourceDocumentsMovementOfGoodsStockMovement), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DebitAmount"))
                    {
                        line.ItemElementName = ItemChoiceType6.DebitAmount;
                        line.DebitAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "StockMovement/Line/DebitAmount", typeof(SourceDocumentsMovementOfGoodsStockMovement), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxExemptionReason"))
                    {
                        line.TaxExemptionReason = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxExemptionCode"))
                    {
                        line.TaxExemptionCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SettlementAmount"))
                    {
                        line.SettlementAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "StockMovement/Line/SettlementAmount", typeof(SourceDocumentsMovementOfGoodsStockMovement), supPk: supPk);
                        continue;
                    }


                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            line.ProductSerialNumber = productSerialNumbers.ToArray();

            return line;
        }
        private static async Task<SourceDocumentsWorkingDocumentsWorkDocumentLine> ReadWorkDocumentLines(XmlReader reader, string supPk, string id)
        {
            var line = new SourceDocumentsWorkingDocumentsWorkDocumentLine();
            var productSerialNumbers = new List<string>();
            var references = new List<References>();

            if (Parsers.StringEquals(reader.Name, "Line"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
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
                        line.Quantity = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "WorkDocument/Line/Quantity", typeof(SourceDocumentsWorkingDocumentsWorkDocument), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "UnitOfMeasure"))
                    {
                        line.UnitOfMeasure = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "UnitPrice"))
                    {
                        line.UnitPrice = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "WorkDocument/Line/UnitPrice", typeof(SourceDocumentsWorkingDocumentsWorkDocument), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxBase"))
                    {
                        line.TaxBase = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "WorkDocument/Line/TaxBase", typeof(SourceDocumentsWorkingDocumentsWorkDocument), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxPointDate"))
                    {
                        line.TaxPointDate = Parsers.ParseDate(reader.ReadElementContentAsString(), line.Pk, id, "WorkDocument/Line/TaxPointDate", typeof(SourceDocumentsWorkingDocumentsWorkDocument), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Description"))
                    {
                        line.Description = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductSerialNumber"))
                    {
                        productSerialNumbers.Add(reader.ReadElementContentAsString());
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CreditAmount"))
                    {
                        line.ItemElementName = ItemChoiceType7.CreditAmount;
                        line.CreditAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "WorkDocument/Line/CreditAmount", typeof(SourceDocumentsWorkingDocumentsWorkDocument), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DebitAmount"))
                    {
                        line.ItemElementName = ItemChoiceType7.DebitAmount;
                        line.DebitAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "WorkDocument/Line/DebitAmount", typeof(SourceDocumentsWorkingDocumentsWorkDocument), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Tax"))
                    {
                        line.Tax = await ReadTax(reader.ReadSubtree(), line.Pk, supPk, id, typeof(SourceDocumentsWorkingDocumentsWorkDocument));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxExemptionReason"))
                    {
                        line.TaxExemptionReason = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxExemptionCode"))
                    {
                        line.TaxExemptionCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "SettlementAmount"))
                    {
                        line.SettlementAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "StocWorkDocumentkMovement/Line/SettlementAmount", typeof(SourceDocumentsWorkingDocumentsWorkDocument), supPk: supPk);
                        continue;
                    }


                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            line.ProductSerialNumber = productSerialNumbers.ToArray();
            line.References = references.ToArray();

            return line;
        }
        private static async Task<SourceDocumentsPaymentsPaymentLine> ReadPaymentLines(XmlReader reader, string supPk, string id)
        {
            var line = new SourceDocumentsPaymentsPaymentLine();
            var sourceDocuments = new List<SourceDocumentsPaymentsPaymentLineSourceDocumentID>();

            if (Parsers.StringEquals(reader.Name, "Line"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "LineNumber"))
                    {
                        line.LineNumber = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "SettlementAmount"))
                    {
                        line.SettlementAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "Payment/Line/SettlementAmount", typeof(SourceDocumentsPaymentsPayment), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "CreditAmount"))
                    {
                        line.ItemElementName = ItemChoiceType8.CreditAmount;
                        line.CreditAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "Payment/Line/CreditAmount", typeof(SourceDocumentsPaymentsPayment), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "DebitAmount"))
                    {
                        line.ItemElementName = ItemChoiceType8.DebitAmount;
                        line.DebitAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), line.Pk, id, "Payment/Line/DebitAmount", typeof(SourceDocumentsPaymentsPayment), supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxExemptionReason"))
                    {
                        line.TaxExemptionReason = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxExemptionCode"))
                    {
                        line.TaxExemptionCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Tax"))
                    {
                        line.Tax = await ReadPaymentTax(reader.ReadSubtree(), line.Pk, supPk, id, typeof(SourceDocumentsPaymentsPayment));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "ProductSerialNumber"))
                    {
                        sourceDocuments.Add(await ReadSourceDocumentID(reader.ReadSubtree(), line.Pk, supPk, id, typeof(SourceDocumentsPaymentsPayment)));
                        continue;
                    }


                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            line.SourceDocumentID = sourceDocuments.ToArray();

            return line;
        }

        private static async Task<Tax> ReadTax(XmlReader reader, string linePk, string supPk, string id, Type type)
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
                        tax.TaxType = Parsers.ParseEnum<TaxType>(reader.ReadElementContentAsString(), linePk, id, "TaxType", type, supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxPercentage"))
                    {
                        tax.ItemElementName = ItemChoiceType1.TaxPercentage;
                        tax.TaxPercentage = Parsers.ParseDecimal(reader.ReadElementContentAsString(), linePk, id, "TaxPercentage", type, supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxAmount"))
                    {
                        tax.ItemElementName = ItemChoiceType1.TaxAmount;
                        tax.TaxAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), linePk, id, "TaxAmount", type, supPk: supPk);
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
        private static async Task<PaymentTax> ReadPaymentTax(XmlReader reader, string linePk, string supPk, string id, Type type)
        {
            var tax = new PaymentTax();

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

                    if (Parsers.StringEquals(reader.Name, "TaxType"))
                    {
                        tax.TaxType = Parsers.ParseEnum<TaxType>(reader.ReadElementContentAsString(), linePk, id, "Payment/Line/Tax/TaxType", type, supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxCountryRegion"))
                    {
                        tax.TaxCountryRegion = reader.ReadElementContentAsString();
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxCode"))
                    {
                        tax.TaxCode = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxAmount"))
                    {
                        tax.ItemElementName = ItemChoiceType.TaxAmount;
                        tax.TaxAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), linePk, id, "Payment/Line/Tax/TaxAmount", type, supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxPercentage"))
                    {
                        tax.ItemElementName = ItemChoiceType.TaxPercentage;
                        tax.TaxPercentage = Parsers.ParseDecimal(reader.ReadElementContentAsString(), linePk, id, "Payment/Line/Tax/TaxPercentage", type, supPk: supPk);
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

        private static async Task<SourceDocumentsPaymentsPaymentLineSourceDocumentID> ReadSourceDocumentID(XmlReader reader, string linePk, string supPk, string id, Type type)
        {
            var sourceDocument = new SourceDocumentsPaymentsPaymentLineSourceDocumentID();

            if (Parsers.StringEquals(reader.Name, "SourceDocumentID"))
                await reader.ReadAsync();

            while (await reader.ReadAsync())
            {
                if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                    continue;

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(reader.Name) || reader.NodeType != XmlNodeType.Element)
                        break;

                    if (Parsers.StringEquals(reader.Name, "OriginatingON"))
                    {
                        sourceDocument.OriginatingON = reader.ReadElementContentAsString();
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "InvoiceDate"))
                    {
                        sourceDocument.InvoiceDate = Parsers.ParseDate(reader.ReadElementContentAsString(), linePk, id, "Payment/Line/SourceDocumentID/InvoiceDate", type, supPk: supPk);
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "Description"))
                    {
                        sourceDocument.Description = reader.ReadElementContentAsString();
                    }

                    //found elements that we don't want, move to the next
                    await reader.ReadAsync();
                    if (string.IsNullOrWhiteSpace(reader.Name))
                        await reader.ReadAsync();
                }
            }

            return sourceDocument;
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

        private static async Task<AddressStructure> ReadAddressStructure(XmlReader reader)
        {
            var address = new AddressStructure();

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
                        product.ProductType = Parsers.ParseEnum<ProductType>(reader.ReadElementContentAsString(), product.Pk, string.Empty, "Product/ProductType", typeof(Product));
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
                        tax.TaxPercentage = Parsers.ParseDecimal(reader.ReadElementContentAsString(), tax.Pk, string.Empty, "TaxPercentage", typeof(TaxTableEntry));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxAmount"))
                    {
                        tax.ItemElementName = ItemChoiceType2.TaxAmount;
                        tax.TaxAmount = Parsers.ParseDecimal(reader.ReadElementContentAsString(), tax.Pk, string.Empty, "TaxAmount", typeof(TaxTableEntry));
                        continue;
                    }
                    if (Parsers.StringEquals(reader.Name, "TaxType"))
                    {
                        tax.TaxType = Parsers.ParseEnum<TaxType>(reader.ReadElementContentAsString(), tax.Pk, string.Empty, "TaxType", typeof(TaxTableEntry));
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
