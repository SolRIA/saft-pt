using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class DialogConvertPemKeyViewModel : ViewModelBase
{
    private readonly IDialogManager dialogManager;
    private readonly IDatabaseService databaseService;

    public DialogConvertPemKeyViewModel()
    {
        dialogManager = AppBootstrap.Resolve<IDialogManager>();
        databaseService = AppBootstrap.Resolve<IDatabaseService>();
    }

    public void Init()
    {
        //load the saved pem keys
        PemFiles = [];
        var pem_files = databaseService.GetPemFiles();

        if (pem_files != null && pem_files.Any())
        {
            foreach (var p in pem_files)
            {
                PemFiles.Add(new PemFile
                {
                    Name = p.Name,
                    Id = p.Id,
                    PemText = p.PemText,
                    PrivateKey = p.PrivateKey,
                    RsaSettings = p.RsaSettings
                });
            }
            PemFile = PemFiles.First();
        }
    }

    public string Title { get; set; } = "Ficheiros Pem";

    [ObservableProperty]
    private ObservableCollection<PemFile> pemFiles;

    [ObservableProperty]
    private PemFile pemFile;

    [ObservableProperty]
    private string pemFileName;

    [RelayCommand]
    private async Task OnOpenPemFile()
    {
        var filters = new FilePickerFileType[]
        {
            new("PEM files")
            {
                Patterns = new[] { "*.pem" },
                MimeTypes = new[] { "application/pdf" }
            }
        };

        var files = await dialogManager.OpenFileDialog("Pem Files", filters: filters);

        if (files != null && files.Length == 1)
        {
            PemFileName = files.First();
            PemFile = new PemFile();
            PemFiles.Add(PemFile);

            ConvertPemFile();
        }
    }

    [RelayCommand]
    private void OnSaveClose()
    {
        databaseService.UpdatePemFiles(PemFiles.Select(p => new Parser.Models.PemFile
        {
            Id = p.Id,
            Name = p.Name,
            PemText = p.PemText,
            PrivateKey = p.PrivateKey,
            RsaSettings = p.RsaSettings
        }));
        dialogManager.CloseDialog();
    }

    [RelayCommand]
    private void OnClose()
    {
        dialogManager.CloseDialog();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteKey))]
    private async Task OnDeleteKey()
    {
        var result = await dialogManager.ShowMessageDialogAsync("Apagar", "Quer apagar o registo atual?", MessageDialogType.None);

        if (result == false) return;

        databaseService.DeletePemFile(PemFile.Id);
    }
    private bool CanDeleteKey()
    {
        return PemFile != null && PemFile.Id > 0;
    }

    private void ConvertPemFile()
    {
        if (File.Exists(PemFileName) == false) return;

        PemFile.PemText = File.ReadAllText(PemFileName);

        RSAKeys rsa = new RSAKeys();
        rsa.DecodePEMKey(PemFile.PemText);

        string key = string.IsNullOrWhiteSpace(rsa.PublicKey) ? rsa.PrivateKey : rsa.PublicKey;

        //use XDocument to format the xml
        XDocument xmlDoc = XDocument.Parse(key);

        PemFile.RsaSettings = xmlDoc.ToString();
        PemFile.PrivateKey = string.IsNullOrWhiteSpace(rsa.PrivateKey) == false;
    }
}
