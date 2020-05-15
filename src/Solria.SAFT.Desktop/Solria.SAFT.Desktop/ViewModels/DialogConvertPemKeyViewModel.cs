using DynamicData;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class DialogConvertPemKeyViewModel : ReactiveObject
    {
        private readonly IDialogManager dialogManager;
        private readonly IDatabaseService databaseService;

        public DialogConvertPemKeyViewModel()
        {
            dialogManager = Locator.Current.GetService<IDialogManager>();
            databaseService = Locator.Current.GetService<IDatabaseService>();

            OpenPemFileCommand = ReactiveCommand.CreateFromTask(OnOpenPemFile);
            CloseDialogCommand = ReactiveCommand.Create(OnCloseDialog);
        }

        public void Init()
        {
            //load the saved pem keys
            PemFiles = new ObservableCollection<PemFile>();
            var pem_files = databaseService.GetPemFiles();

            if (pem_files != null && pem_files.Count() > 0)
            {
                PemFiles.AddRange(pem_files);
                PemFile = PemFiles.First();
            }
        }

        private ObservableCollection<PemFile> pemFiles;
        public ObservableCollection<PemFile> PemFiles
        {
            get => pemFiles;
            set => this.RaiseAndSetIfChanged(ref pemFiles, value);
        }
        private PemFile pemFile;
        public PemFile PemFile
        {
            get => pemFile;
            set
            {
                this.RaiseAndSetIfChanged(ref pemFile, value);

                PemName = pemFile?.Name;
                PemText = pemFile?.PemText;
                RsaSettings = pemFile?.RsaSettings;
            }
        }

        string pemFileName;
        public string PemFileName
        {
            get => pemFileName;
            set => this.RaiseAndSetIfChanged(ref pemFileName, value);
        }

        string pemName;
        public string PemName
        {
            get => pemName;
            set => this.RaiseAndSetIfChanged(ref pemName, value);
        }

        string pemText;
        public string PemText
        {
            get => pemText;
            set => this.RaiseAndSetIfChanged(ref pemText, value);
        }

        string rsaSettings;
        public string RsaSettings
        {
            get => rsaSettings;
            set => this.RaiseAndSetIfChanged(ref rsaSettings, value);
        }

        public ReactiveCommand<Unit, Unit> OpenPemFileCommand { get; }
        private async Task OnOpenPemFile()
        {
            var filters = new List<Avalonia.Controls.FileDialogFilter>
            {
                new Avalonia.Controls.FileDialogFilter
                {
                    Extensions = new List<string> { "pem" },
                    Name = "Pem Files"
                }
            };

            var files = await dialogManager.OpenFileDialog("Pem Files", filters: filters);

            if (files != null && files.Length == 1)
            {
                PemFileName = files.First();

                ConvertPemFile();

                PemFiles.Add(new PemFile { PemText = PemText, RsaSettings = RsaSettings });
            }
        }

        public ReactiveCommand<Unit, Unit> CloseDialogCommand { get; }
        private void OnCloseDialog()
        {
            databaseService.UpdatePemFiles(PemFiles);
            dialogManager.CloseDialog();
        }

        private void ConvertPemFile()
        {
            if (File.Exists(PemFileName))
            {
                PemText = File.ReadAllText(PemFileName);

                RSAKeys rsa = new RSAKeys();
                rsa.DecodePEMKey(PemText);

                string key = string.IsNullOrEmpty(rsa.PublicKey) ? rsa.PrivateKey : rsa.PublicKey;

                //use XDocument to format the xml
                XDocument xmlDoc = XDocument.Parse(key);

                RsaSettings = xmlDoc.ToString();
            }
        }
    }
}
