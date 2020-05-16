using Avalonia.Controls.Notifications;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Models.Saft;
using Solria.SAFT.Desktop.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Security.Cryptography;
using System.Text;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class DialogHashTestViewModel : ReactiveObject
    {
        private readonly IDialogManager dialogManager;
        private readonly IDatabaseService databaseService;

        public DialogHashTestViewModel()
        {
            databaseService = Locator.Current.GetService<IDatabaseService>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            CloseCommand = ReactiveCommand.Create(OnClose);
            CleanFieldsCommand = ReactiveCommand.Create(OnCleanFields);
            TestHashCommand = ReactiveCommand.Create(OnTestHash, CanTestHash());
            CreateHashCommand = ReactiveCommand.Create(OnCreateHash, CanCreateHash());
        }

        public void Init()
        {
            //load the keys
            PemFiles = databaseService.GetPemFiles();
            if (PemFiles != null && PemFiles.Count() > 0)
                PemFile = PemFiles.First();

            DocumentTypes = new string[] { "FT", "FS", "FR", "ND", "NC", "VD", "TV", "TD", "AA", "DA", "RP", "RE", "CS", "LD", "RA" };
        }

        public void InitFromInvoice(SourceDocumentsSalesInvoicesInvoice invoice, string hashDocumentoAnterior)
        {
            if (invoice != null)
            {
                DocumentTypes = new string[] { "FT", "FS", "FR", "ND", "NC", "VD", "TV", "TD", "AA", "DA", "RP", "RE", "CS", "LD", "RA" };
                DocumenType = invoice.InvoiceType;
                DocumentTotal = invoice.DocumentTotals.GrossTotal;
                DocumentDate = invoice.InvoiceDate;
                SystemDate = invoice.SystemEntryDate;
                PreviousHash = hashDocumentoAnterior;
                CurrentDocumentHash = invoice.Hash;
                string[] invoiceNo = invoice.InvoiceNo.Split('/');
                if (invoiceNo != null && invoiceNo.Length == 2)
                {
                    int.TryParse(invoiceNo[1], out int num);
                    DocumentNumber = num;

                    string[] tipoSerie = invoiceNo[0].Split(' ');
                    if (tipoSerie != null && tipoSerie.Length == 2)
                    {
                        BillingNumber = tipoSerie[1];
                    }
                }

                ValuesFromDocument = true;
            }
        }

        public void InitFromMovement(SourceDocumentsMovementOfGoodsStockMovement movement, string hashDocumentoAnterior)
        {
            if (movement != null)
            {
                DocumentTypes = new string[] { "GR", "GT", "GA", "GC", "GD" };
                DocumenType = movement.MovementType;
                DocumentTotal = movement.DocumentTotals.GrossTotal;
                DocumentDate = movement.MovementDate;
                SystemDate = movement.SystemEntryDate;
                PreviousHash = hashDocumentoAnterior;
                CurrentDocumentHash = movement.Hash;
                string[] movementNo = movement.DocumentNumber.Split('/');
                if (movementNo != null && movementNo.Length == 2)
                {
                    int.TryParse(movementNo[1], out int num);
                    DocumentNumber = num;

                    string[] tipoSerie = movementNo[0].Split(' ');
                    if (tipoSerie != null && tipoSerie.Length == 2)
                    {
                        BillingNumber = tipoSerie[1];
                    }
                }

                ValuesFromDocument = true;
            }
        }

        public void InitFromWorking(SourceDocumentsWorkingDocumentsWorkDocument doc, string hashDocumentoAnterior)
        {
            if (doc != null)
            {
                DocumentTypes = new string[] { "CM", "CC", "FC", "FO", "NE", "OU", "OR", "PF", "DC", "RP", "RE", "CS", "LD", "RA" };
                DocumenType = doc.WorkType;
                DocumentTotal = doc.DocumentTotals.GrossTotal;
                DocumentDate = doc.WorkDate;
                SystemDate = doc.SystemEntryDate;
                PreviousHash = hashDocumentoAnterior;
                CurrentDocumentHash = doc.Hash;
                string[] movementNo = doc.DocumentNumber.Split('/');
                if (movementNo != null && movementNo.Length == 2)
                {
                    int.TryParse(movementNo[1], out int num);
                    DocumentNumber = num;

                    string[] tipoSerie = movementNo[0].Split(' ');
                    if (tipoSerie != null && tipoSerie.Length == 2)
                    {
                        BillingNumber = tipoSerie[1];
                    }
                }

                ValuesFromDocument = true;
            }
        }

        public string Title { get; set; } = "Testar assinaturas";

        private PemFile pemFile;
        public PemFile PemFile
        {
            get => pemFile;
            set => this.RaiseAndSetIfChanged(ref pemFile, value);
        }

        private IEnumerable<PemFile> pemFiles;
        public IEnumerable<PemFile> PemFiles
        {
            get => pemFiles;
            set => this.RaiseAndSetIfChanged(ref pemFiles, value);
        }


        bool valuesFromDocument;
        public bool ValuesFromDocument
        {
            get => valuesFromDocument;
            set => this.RaiseAndSetIfChanged(ref valuesFromDocument, value);
        }

        string message;
        public string Message
        {
            get => message;
            set => this.RaiseAndSetIfChanged(ref message, value);
        }

        string[] documentTypes;
        public string[] DocumentTypes
        {
            get => documentTypes;
            set => this.RaiseAndSetIfChanged(ref documentTypes, value);
        }

        string documenType;
        public string DocumenType
        {
            get => documenType;
            set => this.RaiseAndSetIfChanged(ref documenType, value);
        }

        string billingNumber;
        public string BillingNumber
        {
            get => billingNumber;
            set => this.RaiseAndSetIfChanged(ref billingNumber, value);
        }

        int documentNumber;
        public int DocumentNumber
        {
            get => documentNumber;
            set => this.RaiseAndSetIfChanged(ref documentNumber, value);
        }

        decimal documentTotal;
        public decimal DocumentTotal
        {
            get => documentTotal;
            set => this.RaiseAndSetIfChanged(ref documentTotal, value);
        }

        DateTime documentDate;
        public DateTime DocumentDate
        {
            get => documentDate;
            set => this.RaiseAndSetIfChanged(ref documentDate, value);
        }

        DateTime systemDate;
        public DateTime SystemDate
        {
            get => systemDate;
            set => this.RaiseAndSetIfChanged(ref systemDate, value);
        }

        string previousHash;
        public string PreviousHash
        {
            get => previousHash;
            set => this.RaiseAndSetIfChanged(ref previousHash, value);
        }

        string documentHash;
        public string DocumentHash
        {
            get => documentHash;
            set => this.RaiseAndSetIfChanged(ref documentHash, value);
        }

        string currentDocumentHash;
        public string CurrentDocumentHash
        {
            get => currentDocumentHash;
            set => this.RaiseAndSetIfChanged(ref currentDocumentHash, value);
        }

        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        private void OnClose()
        {
            dialogManager.CloseDialog();
        }

        public ReactiveCommand<Unit, Unit> CleanFieldsCommand { get; }
        private void OnCleanFields()
        {
            DocumenType = string.Empty;
            DocumentTotal = 0;
            DocumentDate = DateTime.Now;
            SystemDate = DateTime.Now;
            PreviousHash = string.Empty;
            CurrentDocumentHash = string.Empty;
            DocumentNumber = 0;
            BillingNumber = string.Empty;
            ValuesFromDocument = false;
        }

        public ReactiveCommand<Unit, Unit> TestHashCommand { get; }
        private void OnTestHash()
        {
            Message = "";

            object hasher = SHA1.Create();

            using var rsaCryptokey = new RSACryptoServiceProvider(1024);
            StringBuilder toHash = new StringBuilder();

            try
            {
                rsaCryptokey.FromXmlString(PemFile.RsaSettings);
            }
            catch (Exception)
            {
                Message = "Houve um erro ao ler a chave pública.";
                return;
            }

            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(FormatStringToHash());
            byte[] hashBuffer = Convert.FromBase64String(CurrentDocumentHash);

            if (rsaCryptokey.VerifyData(stringToHashBuffer, hasher, hashBuffer) == false)
            {
                Message = "A assinatura do documento é inválida.";
                dialogManager.ShowNotification("Hash", Message, NotificationType.Error);
            }
            else
                dialogManager.ShowNotification("Hash", "A Hash do documento é válida", NotificationType.Success);
        }
        private IObservable<bool> CanTestHash()
        {
            return this.WhenAnyValue(x => x.DocumenType, x => x.BillingNumber, x => x.DocumentNumber, x => x.PemFile,
                    (tipo, serie, numero, pemFile) => string.IsNullOrEmpty(tipo) == false && string.IsNullOrEmpty(serie) == false && numero > 0 && pemFile != null && pemFile.PrivateKey == false);
        }

        public ReactiveCommand<Unit, Unit> CreateHashCommand { get; }
        private void OnCreateHash()
        {
            Message = "";

            object hasher = SHA1.Create();

            using var rsaCryptokey = new RSACryptoServiceProvider(1024);
            try
            {
                rsaCryptokey.FromXmlString(PemFile.RsaSettings);
            }
            catch (Exception)
            {
                Message = "Houve um erro ao ler a chave privada.";
                return;
            }

            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(FormatStringToHash());
            byte[] r = rsaCryptokey.SignData(stringToHashBuffer, hasher);

            DocumentHash = Convert.ToBase64String(r);

            if (string.IsNullOrEmpty(CurrentDocumentHash) == false && DocumentHash.IndexOf(CurrentDocumentHash, StringComparison.OrdinalIgnoreCase) < 0)
                Message = "A assinatura do documento é diferente da calculada.";
        }
        private IObservable<bool> CanCreateHash()
        {
            return this.WhenAnyValue(x => x.DocumenType, x => x.BillingNumber, x => x.DocumentNumber, x => x.PemFile,
                (tipo, serie, numero, pemFile) => string.IsNullOrEmpty(tipo) == false && string.IsNullOrEmpty(serie) == false && numero > 0 && pemFile != null && pemFile.PrivateKey);
        }

        string FormatStringToHash()
        {
            System.Globalization.CultureInfo en = new System.Globalization.CultureInfo("en-US");

            return string.Format("{0};{1};{2} {3}/{4};{5};{6}"
                        , DocumentDate.ToString("yyyy-MM-dd")
                        , SystemDate.ToString("yyyy-MM-ddTHH:mm:ss")
                        , DocumenType
                        , BillingNumber
                        , DocumentNumber
                        , DocumentTotal.ToString("0.00", en)
                        , PreviousHash);
        }
    }
}
