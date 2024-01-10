using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using SolRIA.SAFT.Parser.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class DialogHashTestViewModel : ViewModelBase
{
    private readonly IDialogManager dialogManager;
    private readonly IDatabaseService databaseService;

    public DialogHashTestViewModel()
    {
        databaseService = AppBootstrap.Resolve<IDatabaseService>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();
    }

    public void Init()
    {
        //load the keys
        var pemFiles = databaseService.GetPemFiles();

        if (pemFiles != null && pemFiles.Any())
        {
            PemFiles = pemFiles.Select(p => new Models.PemFile
            {
                Id = p.Id,
                Name = p.Name,
                PemText = p.PemText,
                PrivateKey = p.PrivateKey,
                RsaSettings = p.RsaSettings
            });
            PemFile = PemFiles.First();
        }

        DocumentTypes = ["FT", "FS", "FR", "ND", "NC", "VD", "TV", "TD", "AA", "DA", "RP", "RE", "CS", "LD", "RA"];
    }

    public void InitFromInvoice(SourceDocumentsSalesInvoicesInvoice invoice, string hashDocumentoAnterior)
    {
        if (invoice == null) return;

        DocumentTypes = ["FT", "FS", "FR", "ND", "NC", "VD", "TV", "TD", "AA", "DA", "RP", "RE", "CS", "LD", "RA"];
        DocumenType = invoice.InvoiceType.ToString();
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

    public void InitFromMovement(SourceDocumentsMovementOfGoodsStockMovement movement, string hashDocumentoAnterior)
    {
        if (movement == null) return;

        DocumentTypes = ["GR", "GT", "GA", "GC", "GD"];
        DocumenType = movement.MovementType.ToString();
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

    public void InitFromWorking(SourceDocumentsWorkingDocumentsWorkDocument doc, string hashDocumentoAnterior)
    {
        if (doc == null) return;

        DocumentTypes = ["CM", "CC", "FC", "FO", "NE", "OU", "OR", "PF", "DC", "RP", "RE", "CS", "LD", "RA"];
        DocumenType = doc.WorkType.ToString();
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

    public string Title { get; set; } = "Testar assinaturas";

    [ObservableProperty]
    private Models.PemFile pemFile;
    
    [ObservableProperty]
    private IEnumerable<Models.PemFile> pemFiles;
    
    [ObservableProperty]
    private bool valuesFromDocument;
    
    [ObservableProperty]
    private string message;
    
    [ObservableProperty]
    private string[] documentTypes;
    
    [ObservableProperty]
    private string documenType;
    
    [ObservableProperty]
    private string billingNumber;
    
    [ObservableProperty]
    private int documentNumber;
    
    [ObservableProperty]
    private decimal documentTotal;
    
    [ObservableProperty]
    private DateTime documentDate;
    
    [ObservableProperty]
    private DateTime systemDate;
    
    [ObservableProperty]
    private string previousHash;
    
    [ObservableProperty]
    private string documentHash;
    
    [ObservableProperty]
    private string currentDocumentHash;
    
    [RelayCommand]
    private void OnClose()
    {
        dialogManager.CloseDialog();
    }

    [RelayCommand]
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

    [RelayCommand(CanExecute = nameof(CanTestHash))]
    private void OnTestHash()
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
        {
            Message = "A assinatura do documento é válida.";
            dialogManager.ShowNotification("Hash", Message, NotificationType.Success);
        }
    }
    private bool CanTestHash()
    {
        return string.IsNullOrWhiteSpace(DocumenType) == false && string.IsNullOrEmpty(BillingNumber) == false && DocumentNumber > 0 && PemFile != null && PemFile.PrivateKey == false;
    }

    [RelayCommand(CanExecute = nameof(CanCreateHash))]
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
        else
            Message = "Ok";
    }
    private bool CanCreateHash()
    {
        return string.IsNullOrWhiteSpace(DocumenType) == false && string.IsNullOrEmpty(BillingNumber) == false && DocumentNumber > 0 && PemFile != null && PemFile.PrivateKey;
    }

    [RelayCommand(CanExecute = nameof(CanCreateHashDates))]
    private void OnCreateHashDates()
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

        decimal total = 0;
        while (total < 20)
        {
            byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(FormatStringToHash(total));
            byte[] r = rsaCryptokey.SignData(stringToHashBuffer, hasher);

            DocumentHash = Convert.ToBase64String(r);

            if (string.IsNullOrEmpty(CurrentDocumentHash) == false && DocumentHash.IndexOf(CurrentDocumentHash, StringComparison.OrdinalIgnoreCase) < 0)
            {

            }
            else
            {
                Message = $"Ok {total}";
                break;
            }

            total += 0.01m;
        }

        dialogManager.ShowNotification("Hash", "Verificação terminou", NotificationType.Success);
    }
    private bool CanCreateHashDates()
    {
        return string.IsNullOrWhiteSpace(DocumenType) == false && string.IsNullOrWhiteSpace(BillingNumber) == false && DocumentNumber > 0 && PemFile != null && PemFile.PrivateKey;
    }

    string FormatStringToHash()
    {
        return string.Format(CultureInfo.InvariantCulture,
            "{0:yyyy-MM-dd};{1:yyyy-MM-ddTHH:mm:ss};{2} {3}/{4};{5:0.00};{6}"
            , DocumentDate
            , SystemDate
            , DocumenType
            , BillingNumber
            , DocumentNumber
            , DocumentTotal
            , PreviousHash);
    }

    string FormatStringToHash(decimal total)
    {
        return string.Format(CultureInfo.InvariantCulture,
            "{0:yyyy-MM-dd};{1:yyyy-MM-ddTHH:mm:ss};{2} {3}/{4};{5:0.00};{6}"
            , DocumentDate
            , SystemDate
            , DocumenType
            , BillingNumber
            , DocumentNumber
            , total
            , PreviousHash);
    }
}
