using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Parser.Models.AT;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SolRIA.SAFT.Desktop.ViewModels;

public sealed partial class DialogReadInvoicesATViewModel : ViewModelBase
{
    public void Init(string filename)
    {
        if (File.Exists(filename) == false) return;

        var rootFile = JsonSerializer.Deserialize<Root>(File.ReadAllText(filename));

        Linhas = rootFile.Linhas;

        var total = 0d;
        var numDocs = Linhas.Count();
        foreach (var linha in Linhas)
        {
            linha.Total = linha.ValorTotal / 100.0;
            total += linha.Total;
        }

        Resume = $"{numDocs} documentos | total: {total:N2}";
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string resume;

    [ObservableProperty]
    private IEnumerable<Linha> linhas;

    [RelayCommand]
    private void OnSearch()
    {

    }

    [RelayCommand]
    private void OnSearchClear()
    {

    }

    [RelayCommand]
    private void OnPrint()
    {

    }
}
