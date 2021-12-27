using DynamicData;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Desktop.Views;
using Solria.SAFT.Parser.Services;
using Splat;
using System;
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
            SaveCloseCommand = ReactiveCommand.Create(OnSaveClose);
            CloseCommand = ReactiveCommand.Create(OnClose);
            DeleteKeyCommand = ReactiveCommand.CreateFromTask(OnDeleteKey, CanDeleteKey());
        }

        public void Init()
        {
            //load the saved pem keys
            PemFiles = new ObservableCollection<PemFile>();
            var pem_files = databaseService.GetPemFiles();

            if (pem_files != null && pem_files.Any())
            {
                PemFiles.AddRange(pem_files.Select(p => new PemFile
                {
                    Name = p.Name,
                    Id = p.Id,
                    PemText = p.PemText,
                    PrivateKey = p.PrivateKey,
                    RsaSettings = p.RsaSettings
                }));
                PemFile = PemFiles.First();
            }
        }

        public string Title { get; set; } = "Ficheiros Pem";

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
            set => this.RaiseAndSetIfChanged(ref pemFile, value);
        }

        string pemFileName;
        public string PemFileName
        {
            get => pemFileName;
            set => this.RaiseAndSetIfChanged(ref pemFileName, value);
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
                pemFile = new PemFile();
                PemFiles.Add(pemFile);

                ConvertPemFile();
            }
        }

        public ReactiveCommand<Unit, Unit> SaveCloseCommand { get; }
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

        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        private void OnClose()
        {
            dialogManager.CloseDialog();
        }

        public ReactiveCommand<Unit, Unit> DeleteKeyCommand { get; }
        private IObservable<bool> CanDeleteKey()
        {
            return this.WhenAnyValue(x => x.PemFile,
                (pemFile) => pemFile != null && pemFile.Id > 0);
        }
        private async Task OnDeleteKey()
        {
            var result = await DialogMessage.Show("Apagar", "Quer apagar o registo atual?", MessageDialogType.None);

            if (result)
                databaseService.DeletePemFile(PemFile.Id);
        }

        private void ConvertPemFile()
        {
            if (File.Exists(PemFileName))
            {
                pemFile.PemText = File.ReadAllText(PemFileName);

                RSAKeys rsa = new RSAKeys();
                rsa.DecodePEMKey(pemFile.PemText);

                string key = string.IsNullOrWhiteSpace(rsa.PublicKey) ? rsa.PrivateKey : rsa.PublicKey;

                //use XDocument to format the xml
                XDocument xmlDoc = XDocument.Parse(key);

                pemFile.RsaSettings = xmlDoc.ToString();
                pemFile.PrivateKey = string.IsNullOrWhiteSpace(rsa.PrivateKey) == false;
            }
        }
    }
}
