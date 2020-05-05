using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class ErrorPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;

        public ErrorPageViewModel(IScreen screen) : base(screen, MenuIds.ERRORS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();

            DoPrintCommand = ReactiveCommand.Create(OnDoPrint);
        }

        protected override void HandleActivation()
        {
            ErrorMessages = saftValidator.MensagensErro;

            if (ErrorMessages.Count() > 0)
                NumErros = $"Foram encontrados {ErrorMessages.Count()} erro(s)";
            else
                NumErros = "Não foram encontrados erros";
        }

        protected override void HandleDeactivation()
        {

        }

        private string numErros;
        public string NumErros
        {
            get => numErros;
            set => this.RaiseAndSetIfChanged(ref numErros, value);
        }

        private IEnumerable<Error> errors;
        public IEnumerable<Error> ErrorMessages
        {
            get => errors;
            set => this.RaiseAndSetIfChanged(ref errors, value);
        }

        private Error selectedError;
        public Error SelectedError
        {
            get => selectedError;
            set => this.RaiseAndSetIfChanged(ref selectedError, value);
        }

        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }

        private void OnDoPrint()
        {

        }
    }
}
