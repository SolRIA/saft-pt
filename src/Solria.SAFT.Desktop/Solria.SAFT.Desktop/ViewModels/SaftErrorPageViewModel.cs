using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftErrorPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;

    public SaftErrorPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        Init();
    }

    private void Init()
    {
        Errors = new List<ValidationError>(saftValidator.MensagensErro);

        if (saftValidator.MensagensErro.Count > 0)
            NumErros = $"Foram encontrados {saftValidator.MensagensErro.Count} erro(s)";
        else
            NumErros = "Não foram encontrados erros";
    }

    [ObservableProperty]
    private string numErros;

    [ObservableProperty]
    private IList<ValidationError> errors;

    [ObservableProperty]
    private ValidationError selectedError;

    [RelayCommand]
    private void OnOpenError()
    {

    }

    [RelayCommand]
    private async Task OnDoPrint()
    {
        if (Errors == null || Errors.Count == 0) return;

        var file = await dialogManager.SaveFileDialog(
            "Guardar erros",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Erros.csv",
            ".csv");

        if (string.IsNullOrWhiteSpace(file) == false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var c in Errors)
            {
                stringBuilder.AppendLine($"{c.Field};{c.Value};{c.Description}");
            }

            File.WriteAllText(file, stringBuilder.ToString());
        }
    }

    [RelayCommand]
    private void OnSearch()
    {
        if (string.IsNullOrWhiteSpace(Filter))
        {
            Errors = new List<ValidationError>(saftValidator.MensagensErro);

            return;
        }

        Errors = saftValidator.MensagensErro
            .Where(d => FilterEntries(d, Filter))
            .ToArray();
    }
    private static bool FilterEntries(ValidationError entry, string filter)
    {
        if (string.IsNullOrWhiteSpace(entry.Description) == false && entry.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.DisplayName) == false && entry.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Field) == false && entry.Field.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.Value) == false && entry.Value.Contains(filter, StringComparison.OrdinalIgnoreCase) 
            || string.IsNullOrWhiteSpace(entry.FileID) == false && entry.FileID.Contains(filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    [RelayCommand]
    private void OnSearchClear()
    {
        Filter = null;
        OnSearch();
    }
}
