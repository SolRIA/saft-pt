using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using SolRIA.SAFT.Desktop.ViewModels;
using System.Collections.Generic;

namespace SolRIA.SAFT.Desktop;

public partial class DialogDocumentReferences : Window
{
    public DialogDocumentReferences()
    {
        InitializeComponent();
    }

    //private void InitDataSource()
    //{
    //    if (DataContext is not DialogDocumentReferencesViewModel vm) return;

    //    PreviousDocuments.Source = new HierarchicalTreeDataGridSource<Models.Reporting.Document>(vm.PreviousDocuments)
    //    {
    //        Columns =
    //        {
    //            new TextColumn<Models.Reporting.Document, string>("Nº", x => x.Number),
    //            new HierarchicalExpanderColumn<Models.Reporting.DocumentLine>(
    //                new TextColumn<Models.Reporting.DocumentLine, decimal>("Nº", x => x.Quantity), x => x.),
    //            new TextColumn<Models.Reporting.Document, string>("Data", x => x.Date),
    //            new TextColumn<Models.Reporting.Document, string>("Total", x => x.Total),
    //        },
    //    };
    //}
}