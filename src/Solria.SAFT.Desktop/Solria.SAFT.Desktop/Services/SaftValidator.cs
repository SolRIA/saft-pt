using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Parser;
using Solria.SAFT.Parser.Models;
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

namespace Solria.SAFT.Desktop.Services
{
    public class SaftValidator : ISaftValidator
    {
        readonly CultureInfo enCulture = new("en-US");

        public SaftValidator()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public string SaftFileName { get; set; }
        public string StockFileName { get; set; }

        public string PublicKeyFileName { get; set; }
        public string PrivateKeyFileName { get; set; }

        public AuditFile SaftFile { get; set; }

        public StockFile StockFile { get; set; }

        List<ValidationError> mensagensErro;
        public List<ValidationError> MensagensErro
        {
            get
            {
                if (mensagensErro == null)
                    mensagensErro = new List<ValidationError>();
                return mensagensErro;
            }
            set { mensagensErro = value; }
        }

        public int SaftHashValidationNumber { get; set; }
        public int SaftHashValidationErrorNumber { get; set; }

        /// <summary>
        /// Loads the SAFT file
        /// </summary>
        public async Task OpenSaftFile(string filename)
        {
            if (File.Exists(filename) == false)
                return;

            SaftFileName = filename;
            MensagensErro.Clear();

            //deserialize the saft file
            var (saftFile, errors) = await SaftParser.ReadFile(SaftFileName);

            SaftFile = saftFile;

            //load the existing validations errors
            MensagensErro.AddRange(errors);

            //init custom navigation properties validations
            if (SaftFile != null)
            {
                if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.SalesInvoices != null && SaftFile.SourceDocuments.SalesInvoices.Invoice != null)
                {
                    //add the link from the line to the correspondent invoice
                    foreach (var invoice in SaftFile.SourceDocuments.SalesInvoices.Invoice)
                    {
                        foreach (var line in invoice.Line)
                        {
                            line.InvoiceNo = invoice.InvoiceNo;
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

                //remove empty messages
                MensagensErro.RemoveAll(c => c == null || string.IsNullOrEmpty(c.Description));
            }
            else
            {
                //show error open file
                MensagensErro.Add(new ValidationError { Description = "Erro ao abrir o ficheiro" });
            }
        }

        public async Task OpenStockFile(string filename)
        {
            if (File.Exists(filename) == false)
                return;

            StockFileName = filename;

            MensagensErro.Clear();


            var (stockFile, errors) = await StockParser.ReadFile(StockFileName);

            StockFile = stockFile;

            //load the existing validations errors
            MensagensErro.AddRange(errors);
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
                MensagensErro.Add(new ValidationError { Description = string.Format("Erro dados: A assinatura do documento {0} não existe.", documentNo) });

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
                MensagensErro.Add(new ValidationError { Description = string.Format("Erro dados: A assinatura do documento {0} é inválida.", documentNo) });

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
                MensagensErro.Add(new ValidationError { Description = string.Format("Não foi possível ler o ficheiro com a chave pública. {0}", ex.Message), TypeofError = typeof(HashResults) });
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
                    MensagensErro.Add(new ValidationError { Value = invoice.InvoiceNo, TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Field = "Hash", Description = string.Format("A assinatura do documento {0} é inválida.{1}", invoice.InvoiceNo, Environment.NewLine) });
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
                MensagensErro.Add(new ValidationError { Description = string.Format("Não foi possível ler o ficheiro com a chave pública. {0}", ex.Message), TypeofError = typeof(HashResults) });
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
                    MensagensErro.Add(new ValidationError { Value = doc.DocumentNumber, TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Field = "Hash", Description = string.Format("A assinatura do documento {0} é inválida.{1}", doc.DocumentNumber, Environment.NewLine) });
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
                MensagensErro.Add(new ValidationError { Description = string.Format("Não foi possível ler o ficheiro com a chave pública. {0}", ex.Message), TypeofError = typeof(HashResults) });
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
                    MensagensErro.Add(new ValidationError { Value = doc.DocumentNumber, TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovement), Field = "Hash", Description = string.Format("A assinatura do documento {0} é inválida.{1}", doc.DocumentNumber, Environment.NewLine) });
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
                MensagensErro.Add(new ValidationError { Description = string.Format("Mensagem de erro: {0}", error.Message), TypeofError = typeof(SchemaResults) });
            }
        }

        void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            MensagensErro.Add(new ValidationError { Description = e.Message, TypeofError = typeof(SchemaResults) });
        }

        void ValidateHeader(Header header)
        {
            MensagensErro.Add(header.ValidateTaxRegistrationNumber());
            MensagensErro.Add(header.ValidateAuditFileVersion());
            MensagensErro.Add(header.ValidateBusinessName());
            MensagensErro.Add(header.ValidateEmail());
            MensagensErro.Add(header.ValidateAddressDetail());
            MensagensErro.Add(header.ValidateBuildingNumber());
            MensagensErro.Add(header.ValidateCity());
            MensagensErro.Add(header.ValidateCountry());
            MensagensErro.Add(header.ValidatePostalCode());
            MensagensErro.Add(header.ValidateRegion());
            MensagensErro.Add(header.ValidateStreetName());
            MensagensErro.Add(header.ValidateCompanyID());
            MensagensErro.Add(header.ValidateCompanyName());
            MensagensErro.Add(header.ValidateCurrencyCode());
            MensagensErro.Add(header.ValidateDateCreated());
            MensagensErro.Add(header.ValidateEndDate());
            MensagensErro.Add(header.ValidateFax());
            MensagensErro.Add(header.ValidateFiscalYear());
            MensagensErro.Add(header.ValidateHeaderComment());
            MensagensErro.Add(header.ValidateProductCompanyTaxID());
            MensagensErro.Add(header.ValidateProductID());
            MensagensErro.Add(header.ValidateProductVersion());
            MensagensErro.Add(header.ValidateSoftwareCertificateNumber());
            MensagensErro.Add(header.ValidateTaxAccountingBasis());
            MensagensErro.Add(header.ValidateTaxEntity());
            MensagensErro.Add(header.ValidateTelephone());
            MensagensErro.Add(header.ValidateWebsite());
        }

        void ValidateCustomers(Customer[] customers)
        {
            if (customers != null && customers.Length > 0)
            {
                var duplicated = from p in customers
                                 group p by p.CustomerID into ci
                                 where ci.Count() > 1
                                 select new { codigo = ci.Key, quantidade = ci.Count() };

                foreach (var d in duplicated)
                {
                    string pk = (from p in customers where p.CustomerID.Contains(d.codigo, StringComparison.OrdinalIgnoreCase) select p.Pk).FirstOrDefault();
                    MensagensErro.Add(new ValidationError { Value = d.codigo, Field = "CustomerID", Description = string.Format("O código {0} está repetido {1} vezes.", d.codigo, d.quantidade), TypeofError = typeof(Customer), UID = pk });
                }

                foreach (var customer in customers)
                {
                    MensagensErro.Add(customer.ValidateAccountID());
                    MensagensErro.Add(customer.ValidateCompanyName());
                    MensagensErro.Add(customer.ValidateContact());
                    MensagensErro.Add(customer.ValidateCustomerID());
                    MensagensErro.Add(customer.ValidateCustomerTaxID());
                    MensagensErro.Add(customer.ValidateEmail());
                    MensagensErro.Add(customer.ValidateFax());
                    MensagensErro.Add(customer.ValidateSelfBillingIndicator());
                    MensagensErro.Add(customer.ValidateTelephone());
                    MensagensErro.Add(customer.ValidateWebsite());
                    MensagensErro.AddRange(customer.ValidateBillingAddress());
                    MensagensErro.AddRange(customer.ValidateShipToAddress());
                }
            }
        }

        void ValidateProducts(Product[] products)
        {
            if (products != null && products.Length > 0)
            {
                var duplicated = from p in products
                                 group p by p.ProductCode into pc
                                 where pc.Count() > 1
                                 select new { codigo = pc.Key, quantidade = pc.Count() };

                foreach (var d in duplicated)
                {
                    string pk = (from p in products where string.IsNullOrWhiteSpace(p.ProductCode) == false && p.ProductCode.Contains(d.codigo, StringComparison.OrdinalIgnoreCase) select p.Pk).FirstOrDefault();

                    MensagensErro.Add(new ValidationError { Value = d.codigo, Field = "ProductCode", Description = string.Format("O código {0} está repetido {1} vezes.", d.codigo, d.quantidade), TypeofError = typeof(Product), UID = pk });
                }

                foreach (var product in products)
                {
                    MensagensErro.Add(product.ValidateProductCode());
                    MensagensErro.Add(product.ValidateProductDescription());
                    MensagensErro.Add(product.ValidateProductGroup());
                    MensagensErro.Add(product.ValidateProductNumberCode());
                }
            }
        }

        void ValidateTax(TaxTableEntry[] taxs)
        {
            if (taxs != null && taxs.Length > 0)
            {
                foreach (var tax in taxs)
                {
                    MensagensErro.Add(tax.ValidateTaxCode());
                    MensagensErro.Add(tax.ValidateTaxCountryRegion());
                }
            }
        }

        void ValidateSupplier(Supplier[] suppliers)
        {
            if (suppliers != null && suppliers.Length > 0)
            {
                var duplicated = from p in suppliers
                                 group p by p.SupplierID into ci
                                 where ci.Count() > 1
                                 select new { codigo = ci.Key, quantidade = ci.Count() };

                foreach (var d in duplicated)
                {
                    string pk = (from p in suppliers where p.SupplierID.Contains(d.codigo, StringComparison.OrdinalIgnoreCase) select p.Pk).FirstOrDefault();
                    MensagensErro.Add(new ValidationError { Value = d.codigo, Field = "SupplierID", Description = string.Format("O código {0} está repetido {1} vezes.", d.codigo, d.quantidade), TypeofError = typeof(Supplier), UID = pk });
                }

                foreach (var supplier in suppliers)
                {
                    MensagensErro.Add(supplier.ValidateAccountID());
                    MensagensErro.Add(supplier.ValidateCompanyName());
                    MensagensErro.Add(supplier.ValidateContact());
                    MensagensErro.Add(supplier.ValidateCustomerID());
                    MensagensErro.Add(supplier.ValidateSupplierTaxID());
                    MensagensErro.Add(supplier.ValidateEmail());
                    MensagensErro.Add(supplier.ValidateFax());
                    MensagensErro.Add(supplier.ValidateSelfBillingIndicator());
                    MensagensErro.Add(supplier.ValidateTelephone());
                    MensagensErro.Add(supplier.ValidateWebsite());
                    MensagensErro.AddRange(supplier.ValidateBillingAddress());
                    MensagensErro.AddRange(supplier.ValidateShipFromAddress());
                }
            }
        }

        void ValidateWorkDocument(SourceDocumentsWorkingDocuments workDocuments)
        {
            int numberOfLines = workDocuments.WorkDocument.Length;
            if (Convert.ToInt32(workDocuments.NumberOfEntries) != numberOfLines)
                MensagensErro.Add(new ValidationError { Value = workDocuments.NumberOfEntries, Field = "WorkingDocuments", TypeofError = typeof(SourceDocumentsWorkingDocuments), Description = string.Format("Nº de registos incorrecto. Documento: {0}, esperado: {1}", workDocuments.NumberOfEntries, numberOfLines) });

            foreach (var workDocument in workDocuments.WorkDocument)
            {
                MensagensErro.Add(workDocument.ValidateDocumentNumber());
                MensagensErro.Add(workDocument.ValidateHash());
                MensagensErro.Add(workDocument.ValidateHashControl());
                MensagensErro.Add(workDocument.ValidatePeriod());
                MensagensErro.Add(workDocument.ValidateSystemEntryDate());
                MensagensErro.Add(workDocument.ValidateWorkDate());
                MensagensErro.AddRange(workDocument.ValidateDocumentStatus());
                MensagensErro.AddRange(workDocument.ValidateDocumentTotals());

                //verificar os totais do documento
                decimal netTotal, grossTotal, taxPayable;
                netTotal = workDocument.Line.Sum(l => l.CreditAmount ?? l.DebitAmount ?? 0);
                grossTotal = workDocument.Line.Where(l => l.Tax != null && l.Tax.ItemElementName == ItemChoiceType1.TaxPercentage)
                                        .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) * (1 + l.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m));
                taxPayable = workDocument.Line.Where(l => l.Tax != null && l.Tax.ItemElementName == ItemChoiceType1.TaxPercentage)
                                        .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) * l.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m);
                grossTotal += workDocument.Line.Where(l => l.Tax != null && l.Tax.ItemElementName == ItemChoiceType1.TaxAmount)
                                        .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) + l.Tax.TaxPercentage.GetValueOrDefault(0));
                taxPayable += workDocument.Line.Where(l => l.Tax != null && l.Tax.ItemElementName == ItemChoiceType1.TaxAmount)
                                        .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) + l.Tax.TaxPercentage.GetValueOrDefault(0));
                grossTotal += workDocument.Line.Where(l => l.Tax == null)
                                        .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0));
                taxPayable += workDocument.Line.Where(l => l.Tax == null)
                                        .Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0));

                if (Math.Abs(netTotal - workDocument.DocumentTotals.NetTotal) > 0.01m)
                    MensagensErro.Add(new ValidationError { Value = workDocument.DocumentTotals.NetTotal.ToString(), Field = "NetTotal", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = string.Format("Total de incidência incorrecto. Documento: {0}, esperado: {1}", workDocument.DocumentTotals.NetTotal, netTotal) });
                if (Math.Abs(grossTotal - workDocument.DocumentTotals.GrossTotal) > 0.01m)
                    MensagensErro.Add(new ValidationError { Value = workDocument.DocumentTotals.GrossTotal.ToString(), Field = "GrossTotal", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = string.Format("Total incorrecto. Documento: {0}, esperado: {1}", workDocument.DocumentTotals.GrossTotal, grossTotal) });
                if (Math.Abs(taxPayable - workDocument.DocumentTotals.TaxPayable) > 0.01m)
                    MensagensErro.Add(new ValidationError { Value = workDocument.DocumentTotals.TaxPayable.ToString(), Field = "TaxPayable", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = string.Format("Total de imposto incorrecto. Documento: {0}, esperado: {1}", workDocument.DocumentTotals.TaxPayable, taxPayable) });

                foreach (var line in workDocument.Line)
                {
                    ValidateWorkDocumentLine(line, workDocument);
                }
            }
        }

        void ValidateWorkDocumentLine(SourceDocumentsWorkingDocumentsWorkDocumentLine line, SourceDocumentsWorkingDocumentsWorkDocument workDocument)
        {
            MensagensErro.Add(line.ValidateLineNumber(workDocument.Pk));
            MensagensErro.Add(line.ValidateProductCode(workDocument.Pk));
            MensagensErro.Add(line.ValidateProductDescription(workDocument.Pk));
            MensagensErro.Add(line.ValidateQuantity(workDocument.Pk));
            MensagensErro.Add(line.ValidateTaxPointDate(workDocument.Pk));
            MensagensErro.Add(line.ValidateUnitOfMeasure(workDocument.Pk));
            MensagensErro.Add(line.ValidateUnitPrice(workDocument.Pk));
            MensagensErro.AddRange(line.ValidateOrderReferences(workDocument.Pk));
            MensagensErro.AddRange(line.ValidateTax(workDocument.Pk));

            int numCasasDecimais = 6;// Workspace.Instance.Config.NumCasasDecimaisValidacoes;

            if (line.UnitPrice * line.Quantity != line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) && Math.Round(line.UnitPrice * line.Quantity, numCasasDecimais, MidpointRounding.AwayFromZero) != Math.Round(line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0), numCasasDecimais, MidpointRounding.AwayFromZero))
                MensagensErro.Add(new ValidationError { Value = line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0).ToString(), Field = "Item", TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovementLine), Description = string.Format("Valor da linha incorrecto. Valor: {0}, esperado: {1}", line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0), line.UnitPrice * line.Quantity), UID = line.Pk, SupUID = workDocument.Pk });
        }

        void ValidateMovementOfGoods(SourceDocumentsMovementOfGoods movements)
        {
            int numberOfLines = movements.StockMovement.Sum(m => m.Line.Length);
            if (Convert.ToInt32(movements.NumberOfMovementLines) != numberOfLines)
                MensagensErro.Add(new ValidationError { Value = movements.NumberOfMovementLines, Field = "MovementOfGoods", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Nº de registos incorrecto. Documento: {0}, esperado: {1}", movements.NumberOfMovementLines, numberOfLines) });

            decimal quantity = 0;
            foreach (var movement in movements.StockMovement)
            {
                MensagensErro.Add(movement.ValidateDocumentNumber());
                MensagensErro.Add(movement.ValidateHash());
                MensagensErro.Add(movement.ValidateHashControl());
                MensagensErro.Add(movement.ValidatePeriod());
                MensagensErro.Add(movement.ValidateSystemEntryDate());
                MensagensErro.Add(movement.ValidateMovementDate());
                MensagensErro.Add(movement.ValidateMovementEndTime());
                MensagensErro.Add(movement.ValidateMovementStartTime());
                MensagensErro.Add(movement.ValidateTransactionID());
                MensagensErro.AddRange(movement.ValidateDocumentStatus());
                MensagensErro.AddRange(movement.ValidateDocumentTotals());
                MensagensErro.AddRange(movement.ValidateShipFrom());
                MensagensErro.AddRange(movement.ValidateShipTo());

                //verificar a quantidade
                quantity += movement.Line.Sum(l => l.Quantity);

                //verificar os totais do documento
                if (movement.DocumentTotals.GrossTotal > 0)
                {
                    decimal netTotal = movement.Line.Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0));
                    decimal grossTotal = movement.Line.Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) * (1 + (l.Tax != null ? l.Tax.TaxPercentage : 0) * 0.01m));
                    decimal taxPayable = movement.Line.Sum(l => l.CreditAmount.GetValueOrDefault(l.DebitAmount ?? 0) * (l.Tax != null ? l.Tax.TaxPercentage : 0) * 0.01m);

                    if (Math.Abs(netTotal - movement.DocumentTotals.NetTotal) > 0.01m)
                        MensagensErro.Add(new ValidationError { Value = movement.DocumentTotals.NetTotal.ToString(), Field = "NetTotal", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Total de incidência incorrecto. Documento: {0}, esperado: {1}", movement.DocumentTotals.NetTotal, netTotal) });
                    if (Math.Abs(grossTotal - movement.DocumentTotals.GrossTotal) > 0.01m)
                        MensagensErro.Add(new ValidationError { Value = movement.DocumentTotals.GrossTotal.ToString(), Field = "GrossTotal", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Total incorrecto. Documento: {0}, esperado: {1}", movement.DocumentTotals.GrossTotal, grossTotal) });
                    if (Math.Abs(taxPayable - movement.DocumentTotals.TaxPayable) > 0.01m)
                        MensagensErro.Add(new ValidationError { Value = movement.DocumentTotals.TaxPayable.ToString(), Field = "TaxPayable", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Total de imposto incorrecto. Documento: {0}, esperado: {1}", movement.DocumentTotals.TaxPayable, taxPayable) });
                }

                //verificar as linhas
                foreach (var line in movement.Line)
                {
                    ValidateMovementOfGoodsStockMovementLine(line, movement);
                }
            }

            if (quantity != movements.TotalQuantityIssued)
                MensagensErro.Add(new ValidationError { Value = movements.TotalQuantityIssued.ToString(), Field = "MovementOfGoods", TypeofError = typeof(SourceDocumentsMovementOfGoods), Description = string.Format("Total da quantidade incorrecto. Documento: {0}, esperado: {1}", movements.TotalQuantityIssued, quantity) });
        }

        void ValidateMovementOfGoodsStockMovementLine(SourceDocumentsMovementOfGoodsStockMovementLine line, SourceDocumentsMovementOfGoodsStockMovement movement)
        {
            MensagensErro.Add(line.ValidateLineNumber(movement.Pk));
            MensagensErro.Add(line.ValidateProductCode(movement.Pk));
            MensagensErro.Add(line.ValidateProductDescription(movement.Pk));
            MensagensErro.Add(line.ValidateQuantity(movement.Pk));
            MensagensErro.Add(line.ValidateUnitOfMeasure(movement.Pk));
            MensagensErro.Add(line.ValidateUnitPrice(movement.Pk));
            MensagensErro.AddRange(line.ValidateOrderReferences(movement.Pk));
            MensagensErro.AddRange(line.ValidateTax(movement.Pk));

            int numCasasDecimais = 6;// Workspace.Instance.Config.NumCasasDecimaisValidacoes;

            if (line.UnitPrice * line.Quantity != line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) && Math.Round(line.UnitPrice * line.Quantity, numCasasDecimais, MidpointRounding.AwayFromZero) != Math.Round(line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0), numCasasDecimais, MidpointRounding.AwayFromZero))
                MensagensErro.Add(new ValidationError { Value = line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0).ToString(), Field = "Item", TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovementLine), Description = string.Format("Valor da linha incorrecto. Valor: {0}, esperado: {1}", line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0), line.UnitPrice * line.Quantity), UID = line.Pk, SupUID = movement.Pk });
        }

        void ValidateInvoices(SourceDocumentsSalesInvoices invoices)
        {
            if (Convert.ToInt32(invoices.NumberOfEntries) != invoices.Invoice.Length)
                MensagensErro.Add(new ValidationError { Value = invoices.NumberOfEntries, Field = "SalesInvoices", TypeofError = typeof(SourceDocumentsSalesInvoices), Description = string.Format("Nº de registos de documentos comerciais incorrecto. Documento: {0}, esperado: {1}", invoices.NumberOfEntries, invoices.Invoice.Length) });

            foreach (var invoice in invoices.Invoice)
            {
                MensagensErro.Add(invoice.ValidateInvoiceNo());
                MensagensErro.Add(invoice.ValidateATCUD());
                MensagensErro.Add(invoice.ValidateHash());
                MensagensErro.Add(invoice.ValidateHashControl());
                MensagensErro.Add(invoice.ValidatePeriod());
                MensagensErro.Add(invoice.ValidateInvoiceDate());
                MensagensErro.Add(invoice.ValidateSystemEntryDate());
                MensagensErro.Add(invoice.ValidateTransactionID());
                MensagensErro.Add(invoice.ValidateCustomerID());
                MensagensErro.Add(invoice.ValidateSourceID());
                MensagensErro.Add(invoice.ValidateMovementEndTime());
                MensagensErro.Add(invoice.ValidateMovementStartTime());
                MensagensErro.AddRange(invoice.ValidateSpecialRegimes());
                MensagensErro.AddRange(invoice.ValidateDocumentStatus());
                MensagensErro.AddRange(invoice.ValidateShipTo());
                MensagensErro.AddRange(invoice.ValidateShipFrom());
                MensagensErro.AddRange(invoice.ValidateDocumentTotals());

                decimal total = 0, incidencia = 0, iva = 0;
                int numLinha = 1, num = -1;
                foreach (var line in invoice.Line)
                {
                    if (!string.IsNullOrEmpty(line.LineNumber))
                        int.TryParse(line.LineNumber, out num);

                    if (numLinha != num)
                        mensagensErro.Add(new ValidationError { Value = line.LineNumber, Field = "LineNumber", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine), Description = string.Format("Número de linha incorrecto, Documento: {0}, esperado: {1}, valor: {2}", invoice.InvoiceNo, numLinha, line.LineNumber), UID = line.Pk, SupUID = invoice.Pk });
                    numLinha++;

                    ValidateInvoiceLine(line, invoice.Pk);

                    if (string.IsNullOrEmpty(line.UnitOfMeasure))
                        MensagensErro.Add(new ValidationError { Value = invoice.InvoiceNo, Field = "UnitOfMeasure", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine), Description = string.Format("Unidade de medida não preenchida. Documento: {0}, linha nº: {1}", invoice.InvoiceNo, line.LineNumber), UID = invoice.Pk });
                    if (line.Tax.TaxPercentage == 0 && string.IsNullOrEmpty(line.TaxExemptionReason))
                        MensagensErro.Add(new ValidationError { Value = invoice.InvoiceNo, Field = "TaxExemptionReason", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine), Description = string.Format("Motivo de isenção do imposto não preenchido. Documento: {0}, linha nº: {1}", invoice.InvoiceNo, line.LineNumber), UID = invoice.Pk });

                    total += line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) * (1 + line.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m) * Operation(invoice, line);
                    incidencia += line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) * Operation(invoice, line);
                    iva += line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0) * line.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m * Operation(invoice, line);
                }
                //arredondar o valor a 2 casas decimais
                incidencia = Math.Round(incidencia, 2, MidpointRounding.AwayFromZero);
                iva = Math.Round(iva, 2, MidpointRounding.AwayFromZero);
                total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

                if (Math.Abs(total - invoice.DocumentTotals.GrossTotal) > 0.01m)
                    MensagensErro.Add(new ValidationError { Value = invoice.InvoiceNo, Field = "GrossTotal", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Total errado. Documento: {0}, total: {1} esperado: {2}", invoice.InvoiceNo, invoice.DocumentTotals.GrossTotal, total), UID = invoice.Pk });
                if (Math.Abs(incidencia - invoice.DocumentTotals.NetTotal) > 0.01m)
                    MensagensErro.Add(new ValidationError { Value = invoice.InvoiceNo, Field = "NetTotal", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Incidencia errada. Documento {0}, incidencia:{1} esperado:{2}", invoice.InvoiceNo, invoice.DocumentTotals.NetTotal, incidencia), UID = invoice.Pk });
                if (Math.Abs(iva - invoice.DocumentTotals.TaxPayable) > 0.01m)
                    MensagensErro.Add(new ValidationError { Value = invoice.InvoiceNo, Field = "TaxPayable", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Iva errado. Documento: {0}, iva: {1} esperado: {2}", invoice.InvoiceNo, invoice.DocumentTotals.TaxPayable, iva), UID = invoice.Pk });
                if (Math.Abs(invoice.DocumentTotals.TaxPayable - (invoice.DocumentTotals.GrossTotal - invoice.DocumentTotals.NetTotal)) > 0.01m)
                    MensagensErro.Add(new ValidationError { Value = invoice.InvoiceNo, Field = "TaxPayable", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Iva errado. Documento: {0}, iva: {1} esperado: {2}", invoice.InvoiceNo, invoice.DocumentTotals.TaxPayable, invoice.DocumentTotals.GrossTotal - invoice.DocumentTotals.NetTotal), UID = invoice.Pk });
                if (Math.Abs(invoice.DocumentTotals.GrossTotal - (invoice.DocumentTotals.NetTotal + invoice.DocumentTotals.TaxPayable)) > 0.01m)
                    MensagensErro.Add(new ValidationError { Value = invoice.InvoiceNo, Field = "DocumentTotals", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = string.Format("Total errado. Documento: {0}, NetTotal + TaxPayable = {1} != GrossTotal {2}", invoice.InvoiceNo, invoice.DocumentTotals.NetTotal + invoice.DocumentTotals.TaxPayable, invoice.DocumentTotals.GrossTotal), UID = invoice.Pk });
            }
        }

        void ValidateInvoiceLine(SourceDocumentsSalesInvoicesInvoiceLine line, string supPk)
        {
            MensagensErro.Add(line.ValidateLineNumber(supPk));
            MensagensErro.Add(line.ValidateProductCode(supPk));
            MensagensErro.Add(line.ValidateProductDescription(supPk));
            MensagensErro.Add(line.ValidateQuantity(supPk));
            MensagensErro.Add(line.ValidateTaxPointDate(supPk));
            MensagensErro.Add(line.ValidateUnitOfMeasure(supPk));
            MensagensErro.Add(line.ValidateUnitPrice(supPk));
            MensagensErro.AddRange(line.ValidateOrderReferences(supPk));
            MensagensErro.AddRange(line.ValidateReferences(supPk));
            MensagensErro.AddRange(line.ValidateTax(supPk));

            int numCasasDecimais = 6;// Workspace.Instance.Config.NumCasasDecimaisValidacoes;
            bool incidendia = Math.Abs(Math.Round(line.UnitPrice * line.Quantity, numCasasDecimais, MidpointRounding.AwayFromZero) - Math.Round(line.CreditAmount.GetValueOrDefault(line.DebitAmount ?? 0), numCasasDecimais, MidpointRounding.AwayFromZero)) > 0.01m;
            if (incidendia == true)
            {
                MensagensErro.Add(new ValidationError
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
                MensagensErro.Add(new ValidationError { Value = payments.NumberOfEntries, Field = "Payments/NumberOfEntries", TypeofError = typeof(SourceDocumentsPayments), Description = string.Format("Nº de registos dos recibos incorrecto. Documento: {0}, esperado: {1}", payments.NumberOfEntries, payments.Payment?.Length ?? 0) });

            if (payments.Payment != null)
            {
                foreach (var payment in payments.Payment)
                {
                    MensagensErro.Add(payment.ValidateCustomerID());
                    MensagensErro.Add(payment.ValidateDescription());
                    MensagensErro.Add(payment.ValidateDocumentStatusSourceID());
                    MensagensErro.Add(payment.ValidatePaymentRefNo());
                    MensagensErro.Add(payment.ValidatePaymentStatusDate());
                    MensagensErro.Add(payment.ValidatePeriod());
                    MensagensErro.Add(payment.ValidateReason());
                    MensagensErro.Add(payment.ValidateSourceID());
                    MensagensErro.Add(payment.ValidateSystemEntryDate());
                    MensagensErro.Add(payment.ValidateSystemID());
                    MensagensErro.Add(payment.ValidateTransactionDate());
                    MensagensErro.Add(payment.ValidateTransactionID());
                    MensagensErro.AddRange(payment.ValidatePaymentMethod());

                    int numLinha = 1, num = -1;
                    foreach (var line in payment.Line)
                    {
                        if (!string.IsNullOrEmpty(line.LineNumber))
                            int.TryParse(line.LineNumber, out num);

                        if (numLinha != num)
                            mensagensErro.Add(new ValidationError { Value = line.LineNumber, Field = "Payments/Line/LineNumber", TypeofError = typeof(SourceDocumentsPaymentsPaymentLine), Description = string.Format("Número de linha incorrecto, Documento: {0}, esperado: {1}, valor: {2}", payment.PaymentRefNo, numLinha, line.LineNumber), UID = line.Pk, SupUID = payment.Pk });
                        numLinha++;

                        ValidatePaymentLine(line, payment.Pk);
                    }
                }
            }
        }

        private void ValidatePaymentLine(SourceDocumentsPaymentsPaymentLine line, string supPk)
        {
            MensagensErro.Add(line.ValidateItem(supPk));
            MensagensErro.Add(line.ValidateLineNumber(supPk));
            MensagensErro.Add(line.ValidateSettlementAmount(supPk));
            MensagensErro.Add(line.ValidateTaxExemptionReason(supPk));
            MensagensErro.AddRange(line.ValidateSourceDocumentID(supPk));
            MensagensErro.AddRange(line.ValidateTax(supPk));
        }

        public static int Operation(SourceDocumentsSalesInvoicesInvoice i, SourceDocumentsSalesInvoicesInvoiceLine l)
        {
            if (i.InvoiceType == InvoiceType.FT || i.InvoiceType == InvoiceType.VD || i.InvoiceType == InvoiceType.ND || i.InvoiceType == InvoiceType.FR || i.InvoiceType == InvoiceType.FS || i.InvoiceType == InvoiceType.TV || i.InvoiceType == InvoiceType.AA)
                return l.ItemElementName == ItemChoiceType4.CreditAmount ? 1 : -1;
            else if (i.InvoiceType == InvoiceType.NC || i.InvoiceType == InvoiceType.TD || i.InvoiceType == InvoiceType.DA || i.InvoiceType == InvoiceType.RE)
                return l.ItemElementName == ItemChoiceType4.DebitAmount ? 1 : -1;

            return 1;
        }

        #endregion
    }
}
