using System.Threading.Tasks;

namespace SolRIA.SAFT.Desktop.Services
{
    public interface IReportService
    {
        Task ShowAsync(ReportRequest request);
    }
}
