using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Solria.SAFT.Desktop.Services;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.Views
{
    public class DialogMessage : Window
    {
        private static IDialogManager dialogManager;
        public DialogMessage()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static IDisposable observableSubs;
        public static async Task<bool> Show(string title, string message, MessageDialogType messageDialogType)
        {
            dialogManager = Locator.Current.GetService<IDialogManager>();
            var vm = new MessageDialogViewModel(messageDialogType) { Title = title, Message = message };

            var dialog = new DialogMessage
            {
                DataContext = vm
            };

            observableSubs = Observable.Merge(vm.OkCommand, vm.CancelCommand)
                .Take(1)
                .Subscribe(r => OnClose(r));

            return await dialogManager.ShowChildDialogAsync<bool>(dialog);
        }

        private static void OnClose(bool result)
        {
            observableSubs?.Dispose();
            dialogManager.CloseDialog(result);
        }
    }

    public enum MessageDialogType
    {
        None,
        Information,
        Warning,
        Error,
        Question,
        Success
    }

    public class MessageDialogViewModel : ReactiveObject
    {
        public MessageDialogViewModel(MessageDialogType messageDialogType)
        {
            switch (messageDialogType)
            {
                case MessageDialogType.None:
                    break;
                case MessageDialogType.Information:
                    ShowIconInformation = true;
                    break;
                case MessageDialogType.Warning:
                    ShowIconWarning = true;
                    break;
                case MessageDialogType.Error:
                    ShowIconError = true;
                    break;
                case MessageDialogType.Question:
                    ShowIconQuestion = true;
                    break;
                case MessageDialogType.Success:
                    ShowIconSuccess = true;
                    break;
                default:
                    break;
            }

            OkCommand = ReactiveCommand.Create(() => OnOk());
            CancelCommand = ReactiveCommand.Create(() => OnCancel());
        }

        private bool showIconInformation;
        public bool ShowIconInformation
        {
            get => showIconInformation;
            set => this.RaiseAndSetIfChanged(ref showIconInformation, value);
        }

        private bool showIconWarning;
        public bool ShowIconWarning
        {
            get => showIconWarning;
            set => this.RaiseAndSetIfChanged(ref showIconWarning, value);
        }

        private bool showIconError;
        public bool ShowIconError
        {
            get => showIconError;
            set => this.RaiseAndSetIfChanged(ref showIconError, value);
        }

        private bool showIconQuestion;
        public bool ShowIconQuestion
        {
            get => showIconQuestion;
            set => this.RaiseAndSetIfChanged(ref showIconQuestion, value);
        }

        private bool showIconSuccess;
        public bool ShowIconSuccess
        {
            get => showIconSuccess;
            set => this.RaiseAndSetIfChanged(ref showIconSuccess, value);
        }

        private string message;
        public string Message
        {
            get => message;
            set => this.RaiseAndSetIfChanged(ref message, value);
        }

        private string title;
        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        public ReactiveCommand<Unit, bool> OkCommand { get; }
        public ReactiveCommand<Unit, bool> CancelCommand { get; }

        private bool OnOk()
        {
            return true;
        }
        private bool OnCancel()
        {
            return false;
        }
    }
}
