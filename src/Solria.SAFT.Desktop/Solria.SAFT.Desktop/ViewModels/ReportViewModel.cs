using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Majorsilence.Reporting.Rdl;
using SolRIA.SAFT.Desktop.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.ViewModels;

public sealed partial class ReportViewModel(ReportRequest request, IDialogManager dialogs) : ViewModelBase, IDisposable
{
    private Report report;
    private bool closed;
    public string Title => request.Title;
    public bool AllowCsv => request.AllowCsv;
    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool IsReady { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "A preparar o relatório…";

    public async Task InitializeAsync(Func<Report, Task> display)
    {
        try
        {
            report = await Task.Run(() => ReportRenderer.CreateAsync(request));
            if (closed) return;
            await display(report);
            if (closed) return;
            IsReady = true;
            Status = "";
        }
        catch (Exception ex) { Status = "Não foi possível apresentar o relatório: " + ex.Message; }
        finally { IsBusy = false; if (closed) Release(); }
    }

    partial void OnIsBusyChanged(bool value)
    {
        ExportExcelCommand.NotifyCanExecuteChanged();
        ExportPdfCommand.NotifyCanExecuteChanged();
        ExportCsvCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsReadyChanged(bool value) => OnIsBusyChanged(IsBusy);
    private bool CanExport() => IsReady && !IsBusy && !closed;
    [RelayCommand(CanExecute = nameof(CanExport))]
    private Task ExportExcel() => ExportAsync("xlsx");
    [RelayCommand(CanExecute = nameof(CanExport))]
    private Task ExportPdf() => ExportAsync("pdf");
    [RelayCommand(CanExecute = nameof(CanExport))]
    private Task ExportCsv() => ExportAsync("csv");

    private async Task ExportAsync(string extension)
    {
        if (extension == "csv" && !AllowCsv) return;
        IsBusy = true;
        try
        {
            var (_, stream) = await dialogs.SaveFileDialog("Exportar " + Title,
                initialFileName: request.FileName + "." + extension, defaultExtension: extension,
                filters: [new FilePickerFileType(extension.ToUpperInvariant()) { Patterns = ["*." + extension] }]);
            if (stream == null) return;
            await using (stream)
            {
                Status = "A exportar…";
                var bytes = extension switch
                {
                    "xlsx" => request.Excel,
                    "csv" => request.Csv(),
                    _ => await Task.Run(async () =>
                    {
                        using var output = new MemoryStreamGen();
                        await report.RunRender(output, OutputPresentationType.PDF);
                        return ((MemoryStream)output.GetStream()).ToArray();
                    })
                };
                if (stream.CanSeek) { stream.SetLength(0); stream.Position = 0; }
                await stream.WriteAsync(bytes);
                await stream.FlushAsync();
            }
            Status = "Ficheiro exportado.";
        }
        catch (Exception ex) { Status = "Não foi possível exportar o relatório: " + ex.Message; }
        finally { IsBusy = false; if (closed) Release(); }
    }

    public void Dispose()
    {
        closed = true;
        if (!IsBusy) Release();
    }

    private void Release() { report?.Dispose(); report = null; }
}
