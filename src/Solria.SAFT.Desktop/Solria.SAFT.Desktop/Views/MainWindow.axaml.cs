using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.Views
{
    public partial class MainWindow : Window, IDialogManager, IReportService
    {
        readonly TextBlock txtMessage;
        private readonly WindowNotificationManager notificationManager;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            //this.AttachDevTools();
#endif

            notificationManager = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };


            //txtMessage = this.Find<TextBlock>("messages");
        }

        private void ToggleButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
        {
            var app = Application.Current;
            if (app is not null)
            {
                var theme = app.ActualThemeVariant;
                app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
            }
        }

        public void NavigateToFirstPage()
        {
            var vm = new MainWindowViewModel();
            DataContext = vm;
            var view = new MainMenuPageView { DataContext = vm };

            var navService = AppBootstrap.Resolve<INavigationService>();
            navService.InitNavigationcontrol(mainContentGrid);

            navService.NavigateTo(view);
        }

        public void CloseApp()
        {
            Close();
        }

        public void UpdateVersionInfo(string version)
        {

        }
        public void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title) == false)
                Title = "SolRIA SAFT - Validador | " + title;
        }
        public void SetFileName(string file)
        {
            this.Title = file;
        }
        public void AddMessage(string message)
        {
            //Dispatcher.UIThread.Post(() =>
            //{
            //    txtMessage.Text = message;
            //});
        }

        public void ShowNotification(string title, string message, NotificationType type = NotificationType.Information, TimeSpan? expiration = null, Action onClick = null, Action onClose = null)
        {
            //if (dialogs.Count > 0)
            //{
            //    var last_dialog = dialogs.Last();

            //    var notificationManager = new WindowNotificationManager(last_dialog)
            //    {
            //        Position = NotificationPosition.TopRight,
            //        MaxItems = 3
            //    };

            //    notificationManager.Show(new Notification(title, message, type, expiration, onClick, onClose));
            //}
            //else
            notificationManager.Show(new Notification(title, message, type, expiration, onClick, onClose));
        }

        public void ShowChildDialog<V>(V vm) where V : ViewModelBase
        {
            var dialog = GetDialogView(vm);

            if (dialog == null) return;

            dialog.Show(GetTopWindow());
        }

        private readonly List<Window> dialogs = [];
        public async Task ShowChildDialogAsync<V>(V vm) where V : ViewModelBase
        {
            var top_window = GetTopWindow();
            var dialog = GetDialogView(vm);

            if (dialog == null) return;

            dialogs.Add(dialog);
            await dialog.ShowDialog(top_window);
        }

        public Task<bool> ShowMessageDialogAsync(string title, string message, MessageDialogType messageDialogType)
        {
            var top_window = GetTopWindow();

            var vm = new DialogMessageViewModel(messageDialogType) { Title = title, Message = message };
            var dialog = new DialogMessage { DataContext = vm };

            dialogs.Add(dialog);
            return dialog.ShowDialog<bool>(top_window);
        }

        public void CloseDialog(bool result = false)
        {
            if (dialogs.Count > 0)
            {
                var last = dialogs.Last();
                last.Close(result);
                dialogs.Remove(last);
            }
        }
        private static Window GetDialogView<T>(T vm) where T : ViewModelBase
        {
            var vmType = typeof(T);

            if (vmType == typeof(DialogConvertPemKeyViewModel))
                return new DialogConvertPemKey { DataContext = vm };
            if (vmType == typeof(DialogDocumentReferencesViewModel))
                return new DialogDocumentReferences { DataContext = vm };
            if (vmType == typeof(DialogHashTestViewModel))
                return new DialogHashTest { DataContext = vm };
            if (vmType == typeof(DialogSaftDocumentDetailViewModel))
                return new DialogSaftDocumentDetail { DataContext = vm };
            if (vmType == typeof(DialogSaftResumeViewModel))
                return new DialogSaftResume { DataContext = vm };
            if (vmType == typeof(DialogReadInvoicesATViewModel))
                return new DialogReadInvoicesAT { DataContext = vm };

            return null;
        }
        private Window GetTopWindow()
        {
            Window parent = this;
            if (dialogs.Count > 0)
                parent = dialogs.Last();

            return parent;
        }

        public async Task<string[]> OpenFileDialog(string title, string initialFileName = "", bool allowMultiple = false, FilePickerFileType[] filters = null)
        {
            var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple,
                FileTypeFilter = filters,
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(initialFileName)
            });

            return result.Select(f => f.Path.LocalPath).ToArray();
        }

        public async Task<string> SaveFileDialog(string title, string directory = "", string initialFileName = "", string defaultExtension = "", FilePickerFileType[] filters = null)
        {
            var result = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                DefaultExtension = defaultExtension,
                ShowOverwritePrompt = true,
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(directory),
                SuggestedFileName = initialFileName,
                FileTypeChoices = filters
            });

            return result?.Name;
        }

        public async Task<string> OpenFolderDialog(string title, string directory = "")
        {
            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(directory)
            });

            return result.Select(result => result.Name).FirstOrDefault();
        }
    }
}
