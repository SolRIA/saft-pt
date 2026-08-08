using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Infrastructure;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class SaftErrorPageViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;
    private readonly IList<ValidationError> _errors;

    public SaftErrorPageViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();

        _errors = saftValidator.GetErrors();
        Init();
    }

    private void Init()
    {
        Errors = [.. _errors];

        if (_errors.Any())
            NumErros = $"Foram encontrados {_errors.Count} erro(s)";
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

        var (_, stream) = await dialogManager.SaveFileDialog(
            "Guardar erros",
            directory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            initialFileName: "Erros.csv",
            ".csv");

        StringBuilder stringBuilder = new StringBuilder();
        foreach (var c in Errors)
        {
            stringBuilder.AppendLine($"{c.Field};{c.Value};{c.Description}");
        }

        if (stream == null) return;
        await stream.Save(stringBuilder.ToString()).ConfigureAwait(false);
    }

    [RelayCommand]
    private void OnSearch()
    {
        if (string.IsNullOrWhiteSpace(Filter))
        {
            Errors = [.. _errors];

            return;
        }

        Errors = [.. _errors.Where(d => FilterEntries(d, Filter))];
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
