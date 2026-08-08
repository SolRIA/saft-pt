using CommunityToolkit.Mvvm.ComponentModel;

namespace SolRIA.SAFT.Desktop.ViewModels;

public sealed partial class DialogSaftDocumentDetailViewModel : ViewModelBase
{
    public DialogSaftDocumentDetailViewModel(Models.Reporting.Document doc)
    {
        Document = doc;

        Title = doc.Number;
    }

    [ObservableProperty]
    private Models.Reporting.Document _document;

    [ObservableProperty]
    private string _title;
}
