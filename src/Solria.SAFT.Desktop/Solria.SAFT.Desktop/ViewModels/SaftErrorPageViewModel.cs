﻿using Avalonia.Collections;
using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class SaftErrorPageViewModel : ViewModelBase
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;

        public SaftErrorPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_ERRORS_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            OpenErrorCommand = ReactiveCommand.Create(OnOpenError);
            DoPrintCommand = ReactiveCommand.CreateFromTask(OnDoPrint);
            SearchCommand = ReactiveCommand.Create<string>(OnSearch);
            SearchClearCommand = ReactiveCommand.Create(OnSearchClear);
        }

        protected override void HandleActivation(CompositeDisposable disposables)
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

            this.WhenAnyValue(x => x.Filter)
                .Throttle(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand)
                .DisposeWith(disposables);
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

        public ReactiveCommand<Unit, Unit> OpenErrorCommand { get; }
        public ReactiveCommand<Unit, Unit> DoPrintCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchClearCommand { get; }

        private void OnOpenError()
        {

        }
        private async Task OnDoPrint()
        {
            if (CollectionView != null && CollectionView.SourceCollection != null && CollectionView.TotalItemCount > 0 && CollectionView.SourceCollection is IEnumerable<Error> errors)
            {
                var file = await dialogManager.SaveFileDialog(
                    "Guardar erros",
                    directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    initialFileName: "Erros.csv",
                    ".csv");

                if (string.IsNullOrWhiteSpace(file) == false)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var c in errors)
                    {
                        stringBuilder.AppendLine($"{c.Field};{c.Value};{c.Description}");
                    }

                    File.WriteAllText(file, stringBuilder.ToString());
                }
            }
        }
        private void OnSearch(string _)
        {
            CollectionView.Refresh();
        }
        private void OnSearchClear()
        {
            Filter = null;
        }
    }
}
