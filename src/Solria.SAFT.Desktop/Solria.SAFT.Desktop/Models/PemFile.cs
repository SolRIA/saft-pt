using ReactiveUI;

namespace Solria.SAFT.Desktop.Models
{
    public class PemFile : ReactiveObject
    {
        private bool privateKey;
        private string name, pemText, rsaSettings;

        public int Id { get; set; }
        public bool PrivateKey { get => privateKey; set => this.RaiseAndSetIfChanged(ref privateKey, value); }
        public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }
        public string PemText { get => pemText; set => this.RaiseAndSetIfChanged(ref pemText, value); }
        public string RsaSettings { get => rsaSettings; set => this.RaiseAndSetIfChanged(ref rsaSettings, value); }
    }
}
