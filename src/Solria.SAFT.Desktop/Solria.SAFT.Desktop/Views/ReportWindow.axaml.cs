using Avalonia.Controls;
using SolRIA.SAFT.Desktop.ViewModels;
using System;

namespace SolRIA.SAFT.Desktop.Views;

public partial class ReportWindow : Window
{
    private bool closed;
    public ReportWindow()
    {
        InitializeComponent();
        Viewer.UseEmbeddedMode();
        Opened += async (_, _) =>
        {
            if (DataContext is ReportViewModel vm)
                await vm.InitializeAsync(async report =>
                {
                    await Viewer.SetReportAsync(report);
                    if (closed) { Viewer.ReleaseReport(); return; }
                    if (Viewer.CurrentPages == null || Viewer.CurrentPages.PageCount == 0)
                        throw new InvalidOperationException("O relatório não produziu páginas.");
                });
        };
        Closed += (_, _) =>
        {
            closed = true;
            Viewer.ReleaseReport();
            (DataContext as ReportViewModel)?.Dispose();
        };
    }
}
