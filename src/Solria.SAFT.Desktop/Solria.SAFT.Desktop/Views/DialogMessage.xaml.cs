using Avalonia.Controls;
using SolRIA.SAFT.Desktop.Services;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class DialogMessage : Window
    {
        public DialogMessage()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif
        }

        private static void OnClose(bool result)
        {
            var dialogManager = AppBootstrap.Resolve<IDialogManager>();

            dialogManager.CloseDialog(result);
        }
    }
}
