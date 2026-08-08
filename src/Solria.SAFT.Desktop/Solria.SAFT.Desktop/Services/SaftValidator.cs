using Riok.Mapperly.Abstractions;
using SolRIA.SAFT.Parser;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace SolRIA.SAFT.Desktop.Services;

[Mapper]
public partial class AuditFileMapper
{
    public partial AuditFile Map(Models.SaftV4.AuditFile file);
}

public class SaftValidator : ISaftValidator
{
    readonly CultureInfo enCulture = new("en-US");
    private readonly List<ValidationError> _mensagensErro;

    public SaftValidator()
    {
        _mensagensErro = [];
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public string SaftFileName { get; set; }
    public string StockFileName { get; set; }

    public string PublicKeyFileName { get; set; }
    public string PrivateKeyFileName { get; set; }

    public AuditFile SaftFile { get; set; }

    public StockFile StockFile { get; set; }

    public int SaftHashValidationNumber { get; set; }
    public int SaftHashValidationErrorNumber { get; set; }

    private void AddError(params ValidationError[] error)
    {
        if (error is null) return;

        foreach (ValidationError e in error)
        {
            if (e is null) continue;

            if (string.IsNullOrWhiteSpace(e.Description)) continue;

            _mensagensErro.Add(e);
        }
    }

    public IList<ValidationError> GetErrors()
    {
        return _mensagensErro;
    }

    /// <summary>
    /// Loads the SAFT file
    /// </summary>
    public async Task OpenSaftFile(string filename)
    {
        if (File.Exists(filename) == false)
            return;

        SaftFileName = filename;
        _mensagensErro.Clear();

        //deserialize the saft file
        //var (saftFile, errors) = await SaftParser.ReadFile(SaftFileName);

        //SaftFile = saftFile;

        //load the existing validations errors
        //AddErro(errors);

        var saftFileV4 = await Task.Run(() => SaftXmlParser.DeserializeXml<Models.SaftV4.AuditFile>(SaftFileName, Encoding.GetEncoding(1252)));

        SaftFile = new AuditFileMapper().Map(saftFileV4);

        //init custom navigation properties validations
        if (SaftFile == null)
        {
            //show error open file
            AddError(new ValidationError { Description = "Erro ao abrir o ficheiro" });
            return;
        }
        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.SalesInvoices != null && SaftFile.SourceDocuments.SalesInvoices.Invoice != null)
        {
            //add the link from the line to the correspondent invoice
            foreach (var invoice in SaftFile.SourceDocuments.SalesInvoices.Invoice)
            {
                foreach (var line in invoice.Line)
                {
                    line.InvoiceNo = invoice.InvoiceNo;

                    if (line.ItemElementName == ItemChoiceType4.CreditAmount)
                        line.CreditAmount = line.Item;
                    if (line.ItemElementName == ItemChoiceType4.DebitAmount)
                        line.DebitAmount = line.Item;
                }
            }
        }

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.MovementOfGoods != null && SaftFile.SourceDocuments.MovementOfGoods.StockMovement != null)
        {
            //add the link from the line to the correspondent movement
            foreach (var doc in SaftFile.SourceDocuments.MovementOfGoods.StockMovement)
            {
                foreach (var line in doc.Line)
                {
                    line.DocNo = doc.DocumentNumber;

                    if (line.ItemElementName == ItemChoiceType6.CreditAmount)
                        line.CreditAmount = line.Item;
                    if (line.ItemElementName == ItemChoiceType6.DebitAmount)
                        line.DebitAmount = line.Item;
                }
            }
        }

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.Payments != null && SaftFile.SourceDocuments.Payments.Payment != null)
        {
            //add the link from the line to the correspondent payment
            foreach (var payment in SaftFile.SourceDocuments.Payments.Payment)
            {
                foreach (var line in payment.Line)
                {
                    line.DocNo = payment.PaymentRefNo;

                    if (line.ItemElementName == ItemChoiceType8.CreditAmount)
                        line.CreditAmount = line.Item;
                    if (line.ItemElementName == ItemChoiceType8.DebitAmount)
                        line.DebitAmount = line.Item;
                }
            }
        }

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.WorkingDocuments != null && SaftFile.SourceDocuments.WorkingDocuments.WorkDocument != null)
        {
            //add the link from the line to the correspondent invoice
            foreach (var doc in SaftFile.SourceDocuments.WorkingDocuments.WorkDocument)
            {
                foreach (var line in doc.Line)
                {
                    line.DocNo = doc.DocumentNumber;

                    if (line.ItemElementName == ItemChoiceType7.CreditAmount)
                        line.CreditAmount = line.Item;
                    if (line.ItemElementName == ItemChoiceType7.DebitAmount)
                        line.DebitAmount = line.Item;
                }
            }
        }

        //Do validations on fields
        ValidateHeader(SaftFile.Header);
        ValidaEstruturaXSD(SaftFileVersion.V10401);
        if (SaftFile.MasterFiles != null)
        {
            ValidateCustomers(SaftFile.MasterFiles.Customer);
            ValidateProducts(SaftFile.MasterFiles.Product);
            ValidateSupplier(SaftFile.MasterFiles.Supplier);
            ValidateTax(SaftFile.MasterFiles.TaxTable);
        }
        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.SalesInvoices != null)
            ValidateInvoices(SaftFile.SourceDocuments.SalesInvoices);

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.Payments != null)
            ValidatePayments(SaftFile.SourceDocuments.Payments);

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.MovementOfGoods != null)
            ValidateMovementOfGoods(SaftFile.SourceDocuments.MovementOfGoods);

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.WorkingDocuments != null)
            ValidateWorkDocument(SaftFile.SourceDocuments.WorkingDocuments);

        //check for solria saft, if true validate the hash
        SaftHashValidationNumber = 0;
        SaftHashValidationErrorNumber = 0;
        if (SaftFile.Header.SoftwareCertificateNumber == "2340")
            SolRiaValidateSaftHash(SaftFile);
    }

    public async Task OpenStockFile(string filename)
    {
        if (File.Exists(filename) == false)
            return;

        StockFileName = filename;

        _mensagensErro.Clear();


        var (stockFile, errors) = await StockParser.ReadFile(StockFileName);

        StockFile = stockFile;

        //load the existing validations errors
        AddError(errors);
    }

    private void SolRiaValidateSaftHash(AuditFile auditFile)
    {
        object hasher = SHA1.Create();

        using var rsaCryptokey = new RSACryptoServiceProvider(1024);
        rsaCryptokey.FromXmlString("<RSAKeyValue><Modulus>vU9AbSDUgXjuKEHl8UaYWfboVz0YRnMMspGHxZ59aJSEYwMYPNsmakNBS9is3+BGXd3AVlthaPmNW5BUuzICNyCQ+DldQ9dP8jr7xcyv1+E5xrMobF8+4xD2ST+DuAUw41ZTCZA3/r47BXonF/DWx+PpVhS68zorDWiJZUJGVg8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");

        StringBuilder toHash = new StringBuilder();

        //invoices
        if (auditFile.SourceDocuments.SalesInvoices != null)
        {
            for (int i = 0; i < auditFile.SourceDocuments.SalesInvoices.Invoice.Length; i++)
            {
                var invoice = auditFile.SourceDocuments.SalesInvoices.Invoice[i];

                bool usaHashAnterior = true;
                if (i == 0 || invoice.InvoiceType != auditFile.SourceDocuments.SalesInvoices.Invoice[i - 1].InvoiceType || Convert.ToInt32(invoice.InvoiceNo.Split('/')[1]) != Convert.ToInt32(auditFile.SourceDocuments.SalesInvoices.Invoice[i - 1].InvoiceNo.Split('/')[1]) + 1)
                    usaHashAnterior = false;

                //ignore the first documents on each billing numbers, there is no way to verify these without the hash of the previous documents
                if (usaHashAnterior == false)
                    continue;

                //check for invoice hash
                if (SolRiaHashExists(invoice.Hash, invoice.InvoiceNo) == false)
                    continue;

                //build the hash
                SolRiaBuildHash(toHash, invoice.InvoiceDate, invoice.SystemEntryDate, invoice.InvoiceNo, invoice.DocumentTotals.GrossTotal, usaHashAnterior ? auditFile.SourceDocuments.SalesInvoices.Invoice[i - 1].Hash : "");

                SolRiaIsHashValid(toHash.ToString(), invoice.Hash, rsaCryptokey, hasher, invoice.InvoiceNo);
            }
        }

        //movement of goods
        if (auditFile.SourceDocuments.MovementOfGoods != null)
        {
            for (int i = 0; i < auditFile.SourceDocuments.MovementOfGoods.StockMovement.Length; i++)
            {
                var movement = auditFile.SourceDocuments.MovementOfGoods.StockMovement[i];

                bool usaHashAnterior = true;
                if (i == 0 || movement.MovementType != auditFile.SourceDocuments.MovementOfGoods.StockMovement[i - 1].MovementType || Convert.ToInt32(movement.DocumentNumber.Split('/')[1]) != Convert.ToInt32(auditFile.SourceDocuments.MovementOfGoods.StockMovement[i - 1].DocumentNumber.Split('/')[1]) + 1)
                    usaHashAnterior = false;

                //ignore the first documents on each billing numbers, there is no way to verify these without the hash of the previous documents
                if (usaHashAnterior == false)
                    continue;

                //check for invoice hash
                if (SolRiaHashExists(movement.Hash, movement.DocumentNumber) == false)
                    continue;

                //build the hash
                SolRiaBuildHash(toHash, movement.MovementDate, movement.SystemEntryDate, movement.DocumentNumber, movement.DocumentTotals.GrossTotal, usaHashAnterior ? auditFile.SourceDocuments.MovementOfGoods.StockMovement[i - 1].Hash : "");

                SolRiaIsHashValid(toHash.ToString(), movement.Hash, rsaCryptokey, hasher, movement.DocumentNumber);
            }
        }

        //working documents
        if (auditFile.SourceDocuments.WorkingDocuments != null)
        {
            for (int i = 0; i < auditFile.SourceDocuments.WorkingDocuments.WorkDocument.Length; i++)
            {
                var document = auditFile.SourceDocuments.WorkingDocuments.WorkDocument[i];

                bool usaHashAnterior = true;
                if (i == 0 || document.WorkType != auditFile.SourceDocuments.WorkingDocuments.WorkDocument[i - 1].WorkType || Convert.ToInt32(document.DocumentNumber.Split('/')[1]) != Convert.ToInt32(auditFile.SourceDocuments.WorkingDocuments.WorkDocument[i - 1].DocumentNumber.Split('/')[1]) + 1)
                    usaHashAnterior = false;

                //ignore the first documents on each billing numbers, there is no way to verify these without the hash of the previous documents
                if (usaHashAnterior == false)
                    continue;

                //check for invoice hash
                if (SolRiaHashExists(document.Hash, document.DocumentNumber) == false)
                    continue;

                //build the hash
                SolRiaBuildHash(toHash, document.WorkDate, document.SystemEntryDate, document.DocumentNumber, document.DocumentTotals.GrossTotal, usaHashAnterior ? auditFile.SourceDocuments.WorkingDocuments.WorkDocument[i - 1].Hash : "");

                SolRiaIsHashValid(toHash.ToString(), document.Hash, rsaCryptokey, hasher, document.DocumentNumber);
            }
        }
    }

    private bool SolRiaHashExists(string hash, string documentNo)
    {
        if (string.IsNullOrEmpty(hash))
        {
            AddError(new ValidationError { Description = string.Format("Erro dados: A assinatura do documento {0} não existe.", documentNo) });

            return false;
        }

        return true;
    }

    private static void SolRiaBuildHash(StringBuilder toHash, DateTime documentDate, DateTime systemEntryDate, string documentNo, decimal documentTotal, string hashAnterior)
    {
        toHash.Clear();

        toHash.AppendFormat(CultureInfo.InvariantCulture,
            "{0:yyyy-MM-dd};{1:yyyy-MM-ddTHH:mm:ss};{2};{3:0.00};{4}"
            , documentDate
            , systemEntryDate
            , documentNo
            , documentTotal
            , hashAnterior);
    }

    private bool SolRiaIsHashValid(string generatedHash, string fileHash, RSACryptoServiceProvider rsaCryptokey, object hasher, string documentNo)
    {
        byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(generatedHash);
        byte[] hashBuffer = Convert.FromBase64String(fileHash);

        //verifi the hash with the public key
        bool isHashCorrect = rsaCryptokey.VerifyData(stringToHashBuffer, hasher, hashBuffer);

        //add error message if the document hash is incorrect
        if (isHashCorrect == false)
        {
            AddError(new ValidationError { Description = string.Format("Erro dados: A assinatura do documento {0} é inválida.", documentNo) });

            SaftHashValidationErrorNumber++;
            return false;
        }

        SaftHashValidationNumber++;
        return true;
    }


    /// <summary>
    /// Validate the hashes in the file for the sales invoices
    /// </summary>
    public void ValidateHashV4()
    {
        if (SaftFile != null)
            ValidateSaftHash(SaftFile);
    }

    /// <summary>
    /// Validate the hashes in the file for the working documents
    /// </summary>
    public void ValidateHashWD()
    {
        if (SaftFile != null)
            ValidateSaftHashWD(SaftFile);
    }

    /// <summary>
    /// Validate the hashes in the file for the movement of goods
    /// </summary>
    public void ValidateHashMG()
    {
        if (SaftFile != null)
            ValidateSaftHashMG(SaftFile);
    }

    /// <summary>
    /// Validate the saft file against the schema file
    /// </summary>
    public void ValidateSchemaV4()
    {
        if (SaftFile != null && SaftFile.Header != null)
            ValidateSchema(SaftFile.Header.AuditFileVersion);
    }

    /// <summary>
    /// Validate the saft file against a specific schema file
    /// </summary>
    public void ValidateSchema(string fileversion)
    {
        ValidaEstruturaXSD(fileversion);
    }

    /// <summary>
    /// Generate the hash with the provided private key for the sales invoices
    /// </summary>
    public void GenerateInvoiceHash()
    {
        if (SaftFile != null)
            GenerateHash(SaftFile);
    }

    /// <summary>
    /// Generate the hash with the provided private key for the working documents
    /// </summary>
    public void GenerateHashWD()
    {
        if (SaftFile != null)
            GenerateHashWD(SaftFile);
    }

    /// <summary>
    /// Generate the hash with the provided private key for the movement of goods
    /// </summary>
    public void GenerateHashMGV4()
    {
        if (SaftFile != null)
            GenerateHashMG(SaftFile);
    }

    #region metodos privados

    /// <summary>
    /// Generate the hash filed for the sales invoices, base in a AuditFile object, the hash will be stored in HashTest field.
    /// </summary>
    void GenerateHash(AuditFile saftfile)
    {
        if (saftfile == null || saftfile.SourceDocuments == null || saftfile.SourceDocuments.SalesInvoices == null || saftfile.SourceDocuments.SalesInvoices.Invoice == null)
            return;

        object hasher = SHA1.Create();

        using var rsaCryptokey = new RSACryptoServiceProvider(1024);
        rsaCryptokey.FromXmlString(GetRSAPrivateKey());

        StringBuilder toHash = new StringBuilder();

        var invoices =
            (from i in saftfile.SourceDocuments.SalesInvoices.Invoice
             orderby i.InvoiceNo
             select i).ToArray();

        for (int i = 0; i < invoices.Length; i++)
        {
            var invoice = invoices[i];

            bool usaHashAnterior = true;
            if (i == 0 || invoice.InvoiceType != invoices[i - 1].InvoiceType || Convert.ToInt32(invoice.InvoiceNo.Split('/')[1]) != Convert.ToInt32(invoices[i - 1].InvoiceNo.Split('/')[1]) + 1)
                usaHashAnterior = false;

            FormatStringToHash(ref toHash, invoice, usaHashAnterior ? invoices[i - 1].Hash : "");

            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
            byte[] r = rsaCryptokey.SignData(stringToHashBuffer, hasher);

            invoice.HashTest = Convert.ToBase64String(r);
        }
    }

    /// <summary>
    /// Generate the hash filed for the working documents, base in a AuditFile object, the hash will be stored in HashTest field.
    /// </summary>
    void GenerateHashWD(AuditFile saftfile)
    {
        if (saftfile == null || saftfile.SourceDocuments == null || saftfile.SourceDocuments.WorkingDocuments == null || saftfile.SourceDocuments.WorkingDocuments.WorkDocument == null)
            return;

        object hasher = SHA1.Create();

        using var rsaCryptokey = new RSACryptoServiceProvider(1024);
        rsaCryptokey.FromXmlString(GetRSAPrivateKey());

        StringBuilder toHash = new StringBuilder();

        var workingDocs =
           (from i in saftfile.SourceDocuments.WorkingDocuments.WorkDocument
            orderby i.DocumentNumber
            select i).ToArray();

        for (int i = 0; i < workingDocs.Length; i++)
        {
            var doc = workingDocs[i];

            bool usaHashAnterior = true;
            if (i == 0 || doc.WorkType != workingDocs[i - 1].WorkType || Convert.ToInt32(doc.DocumentNumber.Split('/')[1]) != Convert.ToInt32(workingDocs[i - 1].DocumentNumber.Split('/')[1]) + 1)
                usaHashAnterior = false;

            FormatStringToHash(ref toHash, doc, usaHashAnterior ? workingDocs[i - 1].Hash : "");

            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
            byte[] r = rsaCryptokey.SignData(stringToHashBuffer, hasher);

            doc.HashTest = Convert.ToBase64String(r);
        }
    }

    /// <summary>
    /// Generate the hash filed for the Movement of goods, base in a AuditFile object, the hash will be stored in HashTest field.
    /// </summary>
    void GenerateHashMG(AuditFile saftfile)
    {
        if (saftfile == null || saftfile.SourceDocuments == null || saftfile.SourceDocuments.MovementOfGoods == null || saftfile.SourceDocuments.MovementOfGoods.StockMovement == null)
            return;

        object hasher = SHA1.Create();

        using var rsaCryptokey = new RSACryptoServiceProvider(1024);
        rsaCryptokey.FromXmlString(GetRSAPrivateKey());

        StringBuilder toHash = new StringBuilder();

        var movements =
            (from i in saftfile.SourceDocuments.MovementOfGoods.StockMovement
             orderby i.DocumentNumber
             select i).ToArray();

        for (int i = 0; i < movements.Length; i++)
        {
            var doc = movements[i];

            bool usaHashAnterior = true;
            if (i == 0 || doc.MovementType != movements[i - 1].MovementType || Convert.ToInt32(doc.DocumentNumber.Split('/')[1]) != Convert.ToInt32(movements[i - 1].DocumentNumber.Split('/')[1]) + 1)
                usaHashAnterior = false;

            FormatStringToHash(ref toHash, doc, usaHashAnterior ? movements[i - 1].Hash : "");

            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
            byte[] r = rsaCryptokey.SignData(stringToHashBuffer, hasher);

            doc.HashTest = Convert.ToBase64String(r);
        }
    }

    /// <summary>
    /// Format the correct invoice fields to the spesification of the hash field.
    /// </summary>
    /// <param name="toHash">StringBuilder that will contain the formated string.</param>
    /// <param name="invoice">The invoice.</param>
    /// <param name="hashAnterior">The string hash generated of the last invoice.</param>
    void FormatStringToHash(ref StringBuilder toHash, SourceDocumentsSalesInvoicesInvoice invoice, string hashAnterior)
    {
        toHash.Clear();
        toHash.AppendFormat("{0};{1};{2};{3};{4}"
                    , invoice.InvoiceDate.ToString("yyyy-MM-dd")
                    , invoice.SystemEntryDate.ToString("yyyy-MM-ddTHH:mm:ss")
                    , invoice.InvoiceNo
                    , invoice.DocumentTotals.GrossTotal.ToString("0.00", enCulture)
                    , hashAnterior);
    }
    void FormatStringToHash(ref StringBuilder toHash, SourceDocumentsWorkingDocumentsWorkDocument doc, string hashAnterior)
    {
        toHash.Clear();
        toHash.AppendFormat("{0};{1};{2};{3};{4}"
                    , doc.WorkDate.ToString("yyyy-MM-dd")
                    , doc.SystemEntryDate.ToString("yyyy-MM-ddTHH:mm:ss")
                    , doc.DocumentNumber
                    , doc.DocumentTotals.GrossTotal.ToString("0.00", enCulture)
                    , hashAnterior);
    }
    void FormatStringToHash(ref StringBuilder toHash, SourceDocumentsMovementOfGoodsStockMovement doc, string hashAnterior)
    {
        toHash.Clear();
        toHash.AppendFormat("{0};{1};{2};{3};{4}"
                    , doc.MovementDate.ToString("yyyy-MM-dd")
                    , doc.SystemEntryDate.ToString("yyyy-MM-ddTHH:mm:ss")
                    , doc.DocumentNumber
                    , doc.DocumentTotals.GrossTotal.ToString("0.00", enCulture)
                    , hashAnterior);
    }

    /// <summary>
    /// Reads the public key file and returns the RSA public key
    /// </summary>
    /// <returns></returns>
    public string GetRSAPublicKey()
    {
        if (File.Exists(PublicKeyFileName))
        {
            string publickey = File.ReadAllText(PublicKeyFileName).Trim();

            if (publickey.StartsWith(RSAKeys.PEM_PUB_HEADER) && publickey.EndsWith(RSAKeys.PEM_PUB_FOOTER))
            {
                //this is a pem file
                var rsa = new RSAKeys();
                rsa.DecodePEMKey(publickey);

                return rsa.PublicKey;
            }
            else
            {
                return publickey;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Reads the public key file and returns the RSA private key
    /// </summary>
    /// <returns></returns>
    public string GetRSAPrivateKey()
    {
        if (File.Exists(PrivateKeyFileName))
        {
            string privatekey = File.ReadAllText(PrivateKeyFileName).Trim();

            if (privatekey.StartsWith(RSAKeys.PEM_PRIV_HEADER) && privatekey.EndsWith(RSAKeys.PEM_PRIV_FOOTER))
            {
                //this is a pem file
                var rsa = new RSAKeys();
                rsa.DecodePEMKey(privatekey);

                return rsa.PrivateKey;
            }
            else
            {
                return privatekey;
            }
        }

        return string.Empty;
    }

    void ValidateSaftHash(AuditFile auditFile)
    {
        if (!File.Exists(SaftFileName) || auditFile == null || auditFile.SourceDocuments == null || auditFile.SourceDocuments.SalesInvoices == null || auditFile.SourceDocuments.SalesInvoices.Invoice == null)
            return;

        object hasher = SHA1.Create();

        using var rsaCryptokey = new RSACryptoServiceProvider(1024);
        StringBuilder toHash = new StringBuilder();

        try
        {
            rsaCryptokey.FromXmlString(GetRSAPublicKey());
        }
        catch (Exception ex)
        {
            AddError(new ValidationError { Description = string.Format("Não foi possível ler o ficheiro com a chave pública. {0}", ex.Message) });
            return;
        }

        var invoices =
            (from i in auditFile.SourceDocuments.SalesInvoices.Invoice
             orderby i.InvoiceNo
             select i).ToArray();

        for (int i = 0; i < invoices.Length; i++)
        {
            var invoice = invoices[i];

            bool usaHashAnterior = true;
            if (i == 0 || invoice.InvoiceType != invoices[i - 1].InvoiceType || Convert.ToInt32(invoice.InvoiceNo.Split('/')[1]) != Convert.ToInt32(invoices[i - 1].InvoiceNo.Split('/')[1]) + 1)
                usaHashAnterior = false;

            FormatStringToHash(ref toHash, invoice, usaHashAnterior ? auditFile.SourceDocuments.SalesInvoices.Invoice[i - 1].Hash : "");

            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
            byte[] hashBuffer = Convert.FromBase64String(invoice.Hash);

            if (rsaCryptokey.VerifyData(stringToHashBuffer, hasher, hashBuffer) == false)
                AddError(new ValidationError { Value = invoice.InvoiceNo, TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Field = "Hash", Description = string.Format("A assinatura do documento {0} é inválida.{1}", invoice.InvoiceNo, Environment.NewLine) });
        }
    }
    void ValidateSaftHashWD(AuditFile auditFile)
    {
        if (!File.Exists(SaftFileName) || auditFile == null || auditFile.SourceDocuments == null || auditFile.SourceDocuments.WorkingDocuments == null || auditFile.SourceDocuments.WorkingDocuments.WorkDocument == null)
            return;

        object hasher = SHA1.Create();

        using var rsaCryptokey = new RSACryptoServiceProvider(1024);
        StringBuilder toHash = new StringBuilder();

        try
        {
            rsaCryptokey.FromXmlString(GetRSAPublicKey());
        }
        catch (Exception ex)
        {
            AddError(new ValidationError { Description = string.Format("Não foi possível ler o ficheiro com a chave pública. {0}", ex.Message) });
            return;
        }

        var workingDocs =
            (from i in auditFile.SourceDocuments.WorkingDocuments.WorkDocument
             orderby i.DocumentNumber
             select i).ToArray();

        for (int i = 0; i < workingDocs.Length; i++)
        {
            var doc = workingDocs[i];

            bool usaHashAnterior = true;
            if (i == 0 || doc.WorkType != workingDocs[i - 1].WorkType || Convert.ToInt32(doc.DocumentNumber.Split('/')[1]) != Convert.ToInt32(workingDocs[i - 1].DocumentNumber.Split('/')[1]) + 1)
                usaHashAnterior = false;

            FormatStringToHash(ref toHash, doc, usaHashAnterior ? workingDocs[i - 1].Hash : "");

            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
            byte[] hashBuffer = Convert.FromBase64String(doc.Hash);

            if (rsaCryptokey.VerifyData(stringToHashBuffer, hasher, hashBuffer) == false)
                AddError(new ValidationError { Value = doc.DocumentNumber, TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Field = "Hash", Description = string.Format("A assinatura do documento {0} é inválida.{1}", doc.DocumentNumber, Environment.NewLine) });
        }
    }
    void ValidateSaftHashMG(AuditFile auditFile)
    {
        if (!File.Exists(SaftFileName) || auditFile == null || auditFile.SourceDocuments == null || auditFile.SourceDocuments.MovementOfGoods == null || auditFile.SourceDocuments.MovementOfGoods.StockMovement == null)
            return;

        object hasher = SHA1.Create();

        using var rsaCryptokey = new RSACryptoServiceProvider(1024);
        StringBuilder toHash = new StringBuilder();

        try
        {
            rsaCryptokey.FromXmlString(GetRSAPublicKey());
        }
        catch (Exception ex)
        {
            AddError(new ValidationError { Description = string.Format("Não foi possível ler o ficheiro com a chave pública. {0}", ex.Message) });
            return;
        }

        var movements =
            (from i in auditFile.SourceDocuments.MovementOfGoods.StockMovement
             orderby i.DocumentNumber
             select i).ToArray();

        for (int i = 0; i < movements.Length; i++)
        {
            var doc = movements[i];

            bool usaHashAnterior = true;
            if (i == 0 || doc.MovementType != movements[i - 1].MovementType || Convert.ToInt32(doc.DocumentNumber.Split('/')[1]) != Convert.ToInt32(movements[i - 1].DocumentNumber.Split('/')[1]) + 1)
                usaHashAnterior = false;

            FormatStringToHash(ref toHash, doc, usaHashAnterior ? movements[i - 1].Hash : "");

            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
            byte[] hashBuffer = Convert.FromBase64String(doc.Hash);

            if (rsaCryptokey.VerifyData(stringToHashBuffer, hasher, hashBuffer) == false)
                AddError(new ValidationError { Value = doc.DocumentNumber, TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovement), Field = "Hash", Description = string.Format("A assinatura do documento {0} é inválida.{1}", doc.DocumentNumber, Environment.NewLine) });
        }
    }

    void ValidaEstruturaXSD(string fileversion)
    {
        try
        {
            string schema_filename = string.Empty;
            if (fileversion == SaftFileVersion.V10401)
                schema_filename = Path.Combine(Environment.CurrentDirectory, "Schemas", "SAFTPT1.04_01.xsd");
            else if (fileversion == SaftFileVersion.V10301)
                schema_filename = Path.Combine(Environment.CurrentDirectory, "Schemas", "SAFTPT1.03_01.xsd");
            else if (fileversion == SaftFileVersion.V10201)
                schema_filename = Path.Combine(Environment.CurrentDirectory, "Schemas", "SAFTPT1.02_01.xsd");
            else if (fileversion == SaftFileVersion.V10101)
                schema_filename = Path.Combine(Environment.CurrentDirectory, "Schemas", "SAFTPT1.01_01.xsd");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(null, schema_filename);
            settings.ValidationType = ValidationType.Schema;

            ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

            XmlReader reader = XmlReader.Create(SaftFileName, settings);
            XmlDocument document = new XmlDocument();
            document.Load(reader);
            // the following call to Validate succeeds.
            document.Validate(eventHandler);
        }
        catch (Exception error)
        {
            // XML Validation failed
            AddError(new ValidationError { Description = string.Format("Mensagem de erro: {0}", error.Message) });
        }
    }

    void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        AddError(new ValidationError { Description = e.Message, ValidationType = ValidationErrorType.SCHEMA });
    }

    void ValidateHeader(Header header)
    {
        AddError(header.ValidateTaxRegistrationNumber());
        AddError(header.ValidateAuditFileVersion());
        AddError(header.ValidateBusinessName());
        AddError(header.ValidateEmail());
        AddError(header.ValidateAddressDetail());
        AddError(header.ValidateBuildingNumber());
        AddError(header.ValidateCity());
        AddError(header.ValidateCountry());
        AddError(header.ValidatePostalCode());
        AddError(header.ValidateRegion());
        AddError(header.ValidateStreetName());
        AddError(header.ValidateCompanyID());
        AddError(header.ValidateCompanyName());
        AddError(header.ValidateCurrencyCode());
        AddError(header.ValidateDateCreated());
        AddError(header.ValidateEndDate());
        AddError(header.ValidateFax());
        AddError(header.ValidateFiscalYear());
        AddError(header.ValidateHeaderComment());
        AddError(header.ValidateProductCompanyTaxID());
        AddError(header.ValidateProductID());
        AddError(header.ValidateProductVersion());
        AddError(header.ValidateSoftwareCertificateNumber());
        AddError(header.ValidateTaxAccountingBasis());
        AddError(header.ValidateTaxEntity());
        AddError(header.ValidateTelephone());
        AddError(header.ValidateWebsite());
    }

    void ValidateCustomers(Customer[] customers)
    {
        if (customers is null || customers.Length == 0) return;

        var duplicated = from p in customers
                         group p by p.CustomerID into ci
                         where ci.Count() > 1
                         select new { codigo = ci.Key, quantidade = ci.Count() };

        foreach (var d in duplicated)
        {
            string pk = (from p in customers where p.CustomerID.Contains(d.codigo, StringComparison.OrdinalIgnoreCase) select p.Pk).FirstOrDefault();
            AddError(new ValidationError { Value = d.codigo, Field = "CustomerID", Description = string.Format("O código {0} está repetido {1} vezes.", d.codigo, d.quantidade), TypeofError = typeof(Customer), UID = pk });
        }

        foreach (var customer in customers)
        {
            AddError(customer.ValidateAccountID());
            AddError(customer.ValidateCompanyName());
            AddError(customer.ValidateContact());
            AddError(customer.ValidateCustomerID());
            AddError(customer.ValidateCustomerTaxID());
            AddError(customer.ValidateEmail());
            AddError(customer.ValidateFax());
            AddError(customer.ValidateSelfBillingIndicator());
            AddError(customer.ValidateTelephone());
            AddError(customer.ValidateWebsite());
            AddError(customer.ValidateBillingAddress());
            AddError(customer.ValidateShipToAddress());
        }
    }

    void ValidateProducts(Product[] products)
    {
        if (products is null || products.Length == 0) return;

        var duplicated = from p in products
                         group p by p.ProductCode into pc
                         where pc.Count() > 1
                         select new { codigo = pc.Key, quantidade = pc.Count() };

        foreach (var d in duplicated)
        {
            string pk = (from p in products where string.IsNullOrWhiteSpace(p.ProductCode) == false && p.ProductCode.Contains(d.codigo, StringComparison.OrdinalIgnoreCase) select p.Pk).FirstOrDefault();

            AddError(new ValidationError { Value = d.codigo, Field = "ProductCode", Description = string.Format("O código {0} está repetido {1} vezes.", d.codigo, d.quantidade), TypeofError = typeof(Product), UID = pk });
        }

        foreach (var product in products)
        {
            AddError(product.ValidateProductCode());
            AddError(product.ValidateProductDescription());
            AddError(product.ValidateProductGroup());
            AddError(product.ValidateProductNumberCode());
        }
    }

    void ValidateTax(TaxTableEntry[] taxs)
    {
        if (taxs is null || taxs.Length == 0) return;

        foreach (var tax in taxs)
        {
            AddError(tax.ValidateTaxCode());
            AddError(tax.ValidateTaxCountryRegion());
        }
    }

    void ValidateSupplier(Supplier[] suppliers)
    {
        if (suppliers is null || suppliers.Length == 0) return;

        var duplicated = from p in suppliers
                         group p by p.SupplierID into ci
                         where ci.Count() > 1
                         select new { codigo = ci.Key, quantidade = ci.Count() };

        foreach (var d in duplicated)
        {
            string pk = (from p in suppliers where p.SupplierID.Contains(d.codigo, StringComparison.OrdinalIgnoreCase) select p.Pk).FirstOrDefault();
            AddError(new ValidationError { Value = d.codigo, Field = "SupplierID", Description = string.Format("O código {0} está repetido {1} vezes.", d.codigo, d.quantidade), TypeofError = typeof(Supplier), UID = pk });
        }

        foreach (var supplier in suppliers)
        {
            AddError(supplier.ValidateAccountID());
            AddError(supplier.ValidateCompanyName());
            AddError(supplier.ValidateContact());
            AddError(supplier.ValidateCustomerID());
            AddError(supplier.ValidateSupplierTaxID());
            AddError(supplier.ValidateEmail());
            AddError(supplier.ValidateFax());
            AddError(supplier.ValidateSelfBillingIndicator());
            AddError(supplier.ValidateTelephone());
            AddError(supplier.ValidateWebsite());
            AddError(supplier.ValidateBillingAddress());
            AddError(supplier.ValidateShipFromAddress());
        }
    }

    void ValidateWorkDocument(SourceDocumentsWorkingDocuments workDocuments)
    {
        int numberOfLines = workDocuments.WorkDocument.Length;
        if (Convert.ToInt32(workDocuments.NumberOfEntries) != numberOfLines)
            AddError(new ValidationError { Value = workDocuments.NumberOfEntries, Field = "WorkingDocuments", TypeofError = typeof(SourceDocumentsWorkingDocuments), Description = string.Format("Nº de registos incorrecto. Documento: {0}, esperado: {1}", workDocuments.NumberOfEntries, numberOfLines) });

        foreach (var workDocument in workDocuments.WorkDocument)
        {
            AddError(workDocument.ValidateDocumentNumber());
            AddError(workDocument.ValidateHash());
            AddError(workDocument.ValidateHashControl());
            AddError(workDocument.ValidatePeriod());
            AddError(workDocument.ValidateSystemEntryDate());
            AddError(workDocument.ValidateWorkDate());
            AddError(workDocument.ValidateDocumentStatus());
            AddError(workDocument.ValidateDocumentTotals());

            //verificar os totais do documento
            decimal netTotal, grossTotal, taxPayable;
            netTotal = workDocument.Line.Sum(l => l.CreditAmount ?? l.DebitAmount ?? 0);
            grossTotal = workDocument.Line
                .Where(l => l.Tax != null && l.Tax.ItemElementName == ItemChoiceType1.TaxPercentage)
                .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) * (1 + l.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m));
            taxPayable = workDocument.Line
                .Where(l => l.Tax != null && l.Tax.ItemElementName == ItemChoiceType1.TaxPercentage)
                .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) * l.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m);
            grossTotal += workDocument.Line
                .Where(l => l.Tax != null && l.Tax.ItemElementName == ItemChoiceType1.TaxAmount)
                .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) + l.Tax.TaxPercentage.GetValueOrDefault(0));
            taxPayable += workDocument.Line
                .Where(l => l.Tax != null && l.Tax.ItemElementName == ItemChoiceType1.TaxAmount)
                .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) + l.Tax.TaxPercentage.GetValueOrDefault(0));
            grossTotal += workDocument.Line
                .Where(l => l.Tax == null)
                .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0));
            taxPayable += workDocument.Line
                .Where(l => l.Tax == null)
                .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0));

            if (Math.Abs(netTotal - workDocument.DocumentTotals.NetTotal) > 0.01m)
                AddError(new ValidationError { Value = workDocument.DocumentTotals.NetTotal.ToString(), Field = "NetTotal", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = string.Format("Total de incidência incorrecto. Documento: {0}, esperado: {1}", workDocument.DocumentTotals.NetTotal, netTotal) });
            if (Math.Abs(grossTotal - workDocument.DocumentTotals.GrossTotal) > 0.01m)
                AddError(new ValidationError { Value = workDocument.DocumentTotals.GrossTotal.ToString(), Field = "GrossTotal", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = string.Format("Total incorrecto. Documento: {0}, esperado: {1}", workDocument.DocumentTotals.GrossTotal, grossTotal) });
            if (Math.Abs(taxPayable - workDocument.DocumentTotals.TaxPayable) > 0.01m)
                AddError(new ValidationError { Value = workDocument.DocumentTotals.TaxPayable.ToString(), Field = "TaxPayable", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = string.Format("Total de imposto incorrecto. Documento: {0}, esperado: {1}", workDocument.DocumentTotals.TaxPayable, taxPayable) });

            foreach (var line in workDocument.Line)
            {
                ValidateWorkDocumentLine(line, workDocument);
            }
        }
    }

    void ValidateWorkDocumentLine(SourceDocumentsWorkingDocumentsWorkDocumentLine line, SourceDocumentsWorkingDocumentsWorkDocument workDocument)
    {
        AddError(line.ValidateLineNumber(workDocument.Pk));
        AddError(line.ValidateProductCode(workDocument.Pk));
        AddError(line.ValidateProductDescription(workDocument.Pk));
        AddError(line.ValidateQuantity(workDocument.Pk));
        AddError(line.ValidateTaxPointDate(workDocument.Pk));
        AddError(line.ValidateUnitOfMeasure(workDocument.Pk));
        AddError(line.ValidateUnitPrice(workDocument.Pk));
        AddError(line.ValidateOrderReferences(workDocument.Pk));
        AddError(line.ValidateTax(workDocument.Pk));

        int numCasasDecimais = 6;// Workspace.Instance.Config.NumCasasDecimaisValidacoes;

        if (line.UnitPrice * line.Quantity != line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) && Math.Round(line.UnitPrice * line.Quantity, numCasasDecimais, MidpointRounding.AwayFromZero) != Math.Round(line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0), numCasasDecimais, MidpointRounding.AwayFromZero))
            AddError(new ValidationError { Value = line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0).ToString(), Field = "Item", TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovementLine), Description = string.Format("Valor da linha incorrecto. Valor: {0}, esperado: {1}", line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0), line.UnitPrice * line.Quantity), UID = line.Pk, SupUID = workDocument.Pk });
    }

    void ValidateMovementOfGoods(SourceDocumentsMovementOfGoods movements)
    {
        int numberOfLines = movements.StockMovement.Sum(m => m.Line.Length);
        if (Convert.ToInt32(movements.NumberOfMovementLines) != numberOfLines)
            AddError(new ValidationError { Value = movements.NumberOfMovementLines, Field = "MovementOfGoods", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Nº de registos incorrecto. Documento: {0}, esperado: {1}", movements.NumberOfMovementLines, numberOfLines) });

        decimal quantity = 0;
        foreach (var movement in movements.StockMovement)
        {
            AddError(movement.ValidateDocumentNumber());
            AddError(movement.ValidateHash());
            AddError(movement.ValidateHashControl());
            AddError(movement.ValidatePeriod());
            AddError(movement.ValidateSystemEntryDate());
            AddError(movement.ValidateMovementDate());
            AddError(movement.ValidateMovementEndTime());
            AddError(movement.ValidateMovementStartTime());
            AddError(movement.ValidateTransactionID());
            AddError(movement.ValidateDocumentStatus());
            AddError(movement.ValidateDocumentTotals());
            AddError(movement.ValidateShipFrom());
            AddError(movement.ValidateShipTo());

            //verificar a quantidade
            quantity += movement.Line.Sum(l => l.Quantity);

            //verificar os totais do documento
            if (movement.DocumentTotals.GrossTotal > 0)
            {
                decimal netTotal = movement.Line.Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0));
                decimal grossTotal = movement.Line.Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) * (1 + (l.Tax != null ? l.Tax.TaxPercentage : 0) * 0.01m));
                decimal taxPayable = movement.Line.Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) * (l.Tax != null ? l.Tax.TaxPercentage : 0) * 0.01m);

                if (Math.Abs(netTotal - movement.DocumentTotals.NetTotal) > 0.01m)
                    AddError(new ValidationError { Value = movement.DocumentTotals.NetTotal.ToString(), Field = "NetTotal", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Total de incidência incorrecto. Documento: {0}, esperado: {1}", movement.DocumentTotals.NetTotal, netTotal) });
                if (Math.Abs(grossTotal - movement.DocumentTotals.GrossTotal) > 0.01m)
                    AddError(new ValidationError { Value = movement.DocumentTotals.GrossTotal.ToString(), Field = "GrossTotal", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Total incorrecto. Documento: {0}, esperado: {1}", movement.DocumentTotals.GrossTotal, grossTotal) });
                if (Math.Abs(taxPayable - movement.DocumentTotals.TaxPayable) > 0.01m)
                    AddError(new ValidationError { Value = movement.DocumentTotals.TaxPayable.ToString(), Field = "TaxPayable", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Total de imposto incorrecto. Documento: {0}, esperado: {1}", movement.DocumentTotals.TaxPayable, taxPayable) });
            }

            //verificar as linhas
            foreach (var line in movement.Line)
            {
                ValidateMovementOfGoodsStockMovementLine(line, movement);
            }
        }

        if (quantity != movements.TotalQuantityIssued)
            AddError(new ValidationError { Value = movements.TotalQuantityIssued.ToString(), Field = "MovementOfGoods", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Total da quantidade incorrecto. Documento: {0}, esperado: {1}", movements.TotalQuantityIssued, quantity) });
    }

    void ValidateMovementOfGoodsStockMovementLine(SourceDocumentsMovementOfGoodsStockMovementLine line, SourceDocumentsMovementOfGoodsStockMovement movement)
    {
        AddError(line.ValidateLineNumber(movement.Pk));
        AddError(line.ValidateProductCode(movement.Pk));
        AddError(line.ValidateProductDescription(movement.Pk));
        AddError(line.ValidateQuantity(movement.Pk));
        AddError(line.ValidateUnitOfMeasure(movement.Pk));
        AddError(line.ValidateUnitPrice(movement.Pk));
        AddError(line.ValidateOrderReferences(movement.Pk));
        AddError(line.ValidateTax(movement.Pk));

        var value = line.UnitPrice * line.Quantity;
        var amount = line.CreditAmount ?? line.DebitAmount ?? 0;
        var difference = Math.Abs(value - amount);

        if (value != amount && difference > 0.01m)
            AddError(new ValidationError { Value = amount.ToString(), Field = "Item", TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovementLine), Description = string.Format("Valor da linha incorrecto. Valor: {0}, esperado: {1}, diferença: {2}", amount, value, difference), UID = line.Pk, SupUID = movement.Pk });
    }

    void ValidateInvoices(SourceDocumentsSalesInvoices invoices)
    {
        if (Convert.ToInt32(invoices.NumberOfEntries) != invoices.Invoice.Length)
            AddError(new ValidationError { Value = invoices.NumberOfEntries, Field = "SalesInvoices", TypeofError = typeof(SourceDocumentsSalesInvoices), Description = string.Format("Nº de registos de documentos comerciais incorrecto. Documento: {0}, esperado: {1}", invoices.NumberOfEntries, invoices.Invoice.Length) });

        foreach (var invoice in invoices.Invoice)
        {
            AddError(invoice.ValidateInvoiceNo());
            AddError(invoice.ValidateATCUD());
            AddError(invoice.ValidateHash());
            AddError(invoice.ValidateHashControl());
            AddError(invoice.ValidatePeriod());
            AddError(invoice.ValidateInvoiceDate());
            AddError(invoice.ValidateSystemEntryDate());
            AddError(invoice.ValidateTransactionID());
            AddError(invoice.ValidateCustomerID());
            AddError(invoice.ValidateSourceID());
            AddError(invoice.ValidateMovementEndTime());
            AddError(invoice.ValidateMovementStartTime());
            AddError(invoice.ValidateSpecialRegimes());
            AddError(invoice.ValidateDocumentStatus());
            AddError(invoice.ValidateShipTo());
            AddError(invoice.ValidateShipFrom());
            AddError(invoice.ValidateDocumentTotals());

            decimal total = 0, incidencia = 0, iva = 0;
            int numLinha = 1, num = -1;
            foreach (var line in invoice.Line)
            {
                if (!string.IsNullOrEmpty(line.LineNumber))
                    int.TryParse(line.LineNumber, out num);

                if (numLinha != num)
                    AddError(new ValidationError { Value = line.LineNumber, Field = "LineNumber", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine), Description = string.Format("Número de linha incorrecto, Documento: {0}, esperado: {1}, valor: {2}", invoice.InvoiceNo, numLinha, line.LineNumber), UID = line.Pk, SupUID = invoice.Pk });
                numLinha++;

                ValidateInvoiceLine(line, invoice.Pk);

                if (string.IsNullOrEmpty(line.UnitOfMeasure))
                    AddError(new ValidationError { Value = invoice.InvoiceNo, Field = "UnitOfMeasure", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine), Description = string.Format("Unidade de medida não preenchida. Documento: {0}, linha nº: {1}", invoice.InvoiceNo, line.LineNumber), UID = invoice.Pk });
                if (line.Tax.TaxPercentage == 0 && string.IsNullOrEmpty(line.TaxExemptionReason))
                    AddError(new ValidationError { Value = invoice.InvoiceNo, Field = "TaxExemptionReason", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine), Description = string.Format("Motivo de isenção do imposto não preenchido. Documento: {0}, linha nº: {1}", invoice.InvoiceNo, line.LineNumber), UID = invoice.Pk });

                total += line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) * (1 + line.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m) * Operation(invoice, line);
                incidencia += line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) * Operation(invoice, line);
                iva += line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) * line.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m * Operation(invoice, line);
            }
            //arredondar o valor a 2 casas decimais
            incidencia = Math.Round(incidencia, 2, MidpointRounding.AwayFromZero);
            iva = Math.Round(iva, 2, MidpointRounding.AwayFromZero);
            total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

            if (Math.Abs(total - invoice.DocumentTotals.GrossTotal) > 0.01m)
                AddError(new ValidationError { Value = invoice.InvoiceNo, Field = "GrossTotal", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Total errado. Documento: {0}, total: {1} esperado: {2}", invoice.InvoiceNo, invoice.DocumentTotals.GrossTotal, total), UID = invoice.Pk });
            if (Math.Abs(incidencia - invoice.DocumentTotals.NetTotal) > 0.01m)
                AddError(new ValidationError { Value = invoice.InvoiceNo, Field = "NetTotal", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Incidencia errada. Documento {0}, incidencia:{1} esperado:{2}", invoice.InvoiceNo, invoice.DocumentTotals.NetTotal, incidencia), UID = invoice.Pk });
            if (Math.Abs(iva - invoice.DocumentTotals.TaxPayable) > 0.01m)
                AddError(new ValidationError { Value = invoice.InvoiceNo, Field = "TaxPayable", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Iva errado. Documento: {0}, iva: {1} esperado: {2}", invoice.InvoiceNo, invoice.DocumentTotals.TaxPayable, iva), UID = invoice.Pk });
            if (Math.Abs(invoice.DocumentTotals.TaxPayable - (invoice.DocumentTotals.GrossTotal - invoice.DocumentTotals.NetTotal)) > 0.01m)
                AddError(new ValidationError { Value = invoice.InvoiceNo, Field = "TaxPayable", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Iva errado. Documento: {0}, iva: {1} esperado: {2}", invoice.InvoiceNo, invoice.DocumentTotals.TaxPayable, invoice.DocumentTotals.GrossTotal - invoice.DocumentTotals.NetTotal), UID = invoice.Pk });
            if (Math.Abs(invoice.DocumentTotals.GrossTotal - (invoice.DocumentTotals.NetTotal + invoice.DocumentTotals.TaxPayable)) > 0.01m)
                AddError(new ValidationError { Value = invoice.InvoiceNo, Field = "DocumentTotals", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Total errado. Documento: {0}, NetTotal + TaxPayable = {1} != GrossTotal {2}", invoice.InvoiceNo, invoice.DocumentTotals.NetTotal + invoice.DocumentTotals.TaxPayable, invoice.DocumentTotals.GrossTotal), UID = invoice.Pk });
        }
    }

    void ValidateInvoiceLine(SourceDocumentsSalesInvoicesInvoiceLine line, string supPk)
    {
        AddError(line.ValidateLineNumber(supPk));
        AddError(line.ValidateProductCode(supPk));
        AddError(line.ValidateProductDescription(supPk));
        AddError(line.ValidateQuantity(supPk));
        AddError(line.ValidateTaxPointDate(supPk));
        AddError(line.ValidateUnitOfMeasure(supPk));
        AddError(line.ValidateUnitPrice(supPk));
        AddError(line.ValidateOrderReferences(supPk));
        AddError(line.ValidateReferences(supPk));
        AddError(line.ValidateTax(supPk));

        int numCasasDecimais = 6;// Workspace.Instance.Config.NumCasasDecimaisValidacoes;
        bool incidendia = Math.Abs(Math.Round(line.UnitPrice * line.Quantity, numCasasDecimais, MidpointRounding.AwayFromZero) - Math.Round(line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0), numCasasDecimais, MidpointRounding.AwayFromZero)) > 0.01m;
        if (incidendia == true)
        {
            AddError(new ValidationError
            {
                Value = line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0).ToString(),
                Field = line.ItemElementName.ToString(),
                TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice),
                Description = $"Total de linha diferente do valor unitário * quantidade ({line.UnitPrice} * {line.Quantity} <> {line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0)}). Documento: {line.InvoiceNo}",
                UID = line.Pk,
                SupUID = supPk
            });
        }
    }

    private void ValidatePayments(SourceDocumentsPayments payments)
    {
        if (Convert.ToInt32(payments.NumberOfEntries) != (payments.Payment?.Length ?? 0))
            AddError(new ValidationError { Value = payments.NumberOfEntries, Field = "Payments/NumberOfEntries", TypeofError = typeof(SourceDocumentsPayments), Description = string.Format("Nº de registos dos recibos incorrecto. Documento: {0}, esperado: {1}", payments.NumberOfEntries, payments.Payment?.Length ?? 0) });

        if (payments.Payment != null)
        {
            foreach (var payment in payments.Payment)
            {
                AddError(payment.ValidateCustomerID());
                AddError(payment.ValidateDescription());
                AddError(payment.ValidateDocumentStatusSourceID());
                AddError(payment.ValidatePaymentRefNo());
                AddError(payment.ValidatePaymentStatusDate());
                AddError(payment.ValidatePeriod());
                AddError(payment.ValidateReason());
                AddError(payment.ValidateSourceID());
                AddError(payment.ValidateSystemEntryDate());
                AddError(payment.ValidateSystemID());
                AddError(payment.ValidateTransactionDate());
                AddError(payment.ValidateTransactionID());
                AddError(payment.ValidatePaymentMethod());

                int numLinha = 1, num = -1;
                foreach (var line in payment.Line)
                {
                    if (!string.IsNullOrEmpty(line.LineNumber))
                        int.TryParse(line.LineNumber, out num);

                    if (numLinha != num)
                        AddError(new ValidationError { Value = line.LineNumber, Field = "Payments/Line/LineNumber", TypeofError = typeof(SourceDocumentsPaymentsPaymentLine), Description = string.Format("Número de linha incorrecto, Documento: {0}, esperado: {1}, valor: {2}", payment.PaymentRefNo, numLinha, line.LineNumber), UID = line.Pk, SupUID = payment.Pk });
                    numLinha++;

                    ValidatePaymentLine(line, payment.Pk);
                }
            }
        }
    }

    private void ValidatePaymentLine(SourceDocumentsPaymentsPaymentLine line, string supPk)
    {
        AddError(line.ValidateItem(supPk));
        AddError(line.ValidateLineNumber(supPk));
        AddError(line.ValidateSettlementAmount(supPk));
        AddError(line.ValidateTaxExemptionReason(supPk));
        AddError(line.ValidateSourceDocumentID(supPk));
        AddError(line.ValidateTax(supPk));
    }

    public static int Operation(SourceDocumentsSalesInvoicesInvoice i, SourceDocumentsSalesInvoicesInvoiceLine l)
    {
        if (i.InvoiceType == InvoiceType.FT || i.InvoiceType == InvoiceType.VD || i.InvoiceType == InvoiceType.ND || i.InvoiceType == InvoiceType.FR || i.InvoiceType == InvoiceType.FS || i.InvoiceType == InvoiceType.TV || i.InvoiceType == InvoiceType.AA)
            return l.ItemElementName == ItemChoiceType4.CreditAmount ? 1 : -1;
        
        if (i.InvoiceType == InvoiceType.NC || i.InvoiceType == InvoiceType.TD || i.InvoiceType == InvoiceType.DA || i.InvoiceType == InvoiceType.RE)
            return l.ItemElementName == ItemChoiceType4.DebitAmount ? 1 : -1;

        return 1;
    }

    #endregion
}
