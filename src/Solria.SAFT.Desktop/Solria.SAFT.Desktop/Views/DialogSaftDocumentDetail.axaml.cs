using Avalonia.Controls;
using SolRIA.SAFT.Desktop.ViewModels;

namespace SolRIA.SAFT.Desktop;

public static class DesignDataDialogSaftDocumentDetail
{
    public static DialogSaftDocumentDetailViewModel ExampleViewModel { get; } = 
        new DialogSaftDocumentDetailViewModel(
            new Models.Reporting.Document
            {
                Total = "999,99 €",
                NetTotal = "99,99 €",
                VatTotal = "9,99 €",
                Date = "2024/01/01",
                SystemDate = "2024/01/01 10:23:59",
                Number = "FT A24/1234",
                ATCUD = "AAAAAAAA-1234"
            });
}

public partial class DialogSaftDocumentDetail : Window
{
    public DialogSaftDocumentDetail()
    {
        InitializeComponent();
    }
}