using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>, IDialogManager
    {
        private readonly StyleInclude _lightTheme;
        private readonly StyleInclude _darkTheme;
        readonly TextBlock txtMessage;
        private readonly WindowNotificationManager notificationManager;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            notificationManager = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };

            var _themeSelector = this.Find<CheckBox>("themeSelector");
            _themeSelector.Checked += (sender, e) =>
            {
                Styles[0] = _darkTheme;
            };
            _themeSelector.Unchecked += (sender, e) =>
            {
                Styles[0] = _lightTheme;
            };

            _lightTheme = new StyleInclude(new Uri("resm:Styles?assembly=Solria.SAFT.Desktop"))
            {
                Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default")
            };
            _darkTheme = new StyleInclude(new Uri("resm:Styles?assembly=Solria.SAFT.Desktop"))
            {
                Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default")
            };
            Styles.Add(_darkTheme);

            txtMessage = this.Find<TextBlock>("messages");

            this.WhenActivated(disposables =>
            {
                Disposable.Create(() => { })
                .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void CloseApp()
        {
            Close();
        }

        public void UpdateVersionInfo(string version)
        {
            var txtVersion = this.Find<TextBlock>("version");
            txtVersion.Text = version;
        }
        public void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title) == false)
                Title = "SolRIA SAFT - Validador | " + title;
        }
        public void SetFileName(string file)
        {
            var txtFile = this.Find<TextBlock>("filename");
            txtFile.Text = file;
        }
        public void AddMessage(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                txtMessage.Text = message;
            });
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

        public void ShowChildDialog(Window window)
        {
            window.Owner = GetTopWindow();
            window.Show();
        }

        private readonly List<Window> dialogs = new List<Window>();
        public async Task ShowChildDialogAsync(Window window)
        {
            var top_window = GetTopWindow();
            dialogs.Add(window);
            await window.ShowDialog(top_window);
        }

        public async Task<T> ShowChildDialogAsync<T>(Window window)
        {
            var top_window = GetTopWindow();
            dialogs.Add(window);
            return await window.ShowDialog<T>(top_window);
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
        private Window GetTopWindow()
        {
            Window parent = this;
            if (dialogs.Count > 0)
                parent = dialogs.Last();

            return parent;
        }

        public async Task<string[]> OpenFileDialog(string title, string directory = "", string initialFileName = "", bool allowMultiple = false, List<FileDialogFilter> filters = null)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = title,
                AllowMultiple = allowMultiple,
                Directory = directory,
                InitialFileName = initialFileName,
                Filters = filters
            };

            return await openFileDialog.ShowAsync(this);
        }

        public async Task<string> SaveFileDialog(string title, string directory = "", string initialFileName = "", string defaultExtension = "", List<FileDialogFilter> filters = null)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = title,
                Directory = directory,
                InitialFileName = initialFileName,
                Filters = filters,
                DefaultExtension = defaultExtension
            };

            return await saveFileDialog.ShowAsync(this);
        }

        public async Task<string> OpenFolderDialog(string title, string directory = "")
        {
            var openFolderDialog = new OpenFolderDialog
            {
                Title = title,
                Directory = directory
            };

            return await openFolderDialog.ShowAsync(this);
        }
    }
}
