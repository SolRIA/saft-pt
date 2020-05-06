using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Splat;
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

            SearchCommand = ReactiveCommand.Create(OnSearch);
            DoPrintCommand = ReactiveCommand.Create(OnDoPrint);
        }

        protected override void HandleActivation()
        {
            CollectionView = new DataGridCollectionView(saftValidator.MensagensErro)
            {
                Filter = o =>
                {
                    if (string.IsNullOrWhiteSpace(Filter))
                        return true;

                    if (o is Error error)
                    {
                        if (error.Description != null && error.Description.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (error.DisplayName != null && error.DisplayName.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (error.Field != null && error.Field.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (error.Value != null && error.Value.Contains(Filter, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    return false;
                }
            };
            //CollectionView.GroupDescriptions.Add(new DataGridPathGroupDescription("Code"));

            if (saftValidator.MensagensErro.Count() > 0)
                NumErros = $"Foram encontrados {saftValidator.MensagensErro.Count()} erro(s)";
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

        private Error selectedError;
        public Error SelectedError
        {
            get => selectedError;
            set => this.RaiseAndSetIfChanged(ref selectedError, value);
        }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }

        private void OnSearch()
        {
            CollectionView.Refresh();
        }
        private void OnDoPrint()
        {

        }
    }
}
