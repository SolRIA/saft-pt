using SolRIA.SAFT.Desktop.ViewModels;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.Services;

public sealed class ReportService(IDialogManager dialogs, ISaftValidator validator) : IReportService
{
    public Task ShowAsync(ReportRequest request)
    {
        if (!request.HasData) return Task.CompletedTask;
        var header = validator.SaftFile?.Header;
        request.Company = request.Template == ReportTemplate.Inventario
            ? "NIF: " + validator.StockFile?.StockHeader?.TaxRegistrationNumber
            : header == null ? "" : $"{header.CompanyName} | NIF: {header.TaxRegistrationNumber}";
        return dialogs.ShowChildDialogAsync(new ReportViewModel(request, dialogs));
    }
}
