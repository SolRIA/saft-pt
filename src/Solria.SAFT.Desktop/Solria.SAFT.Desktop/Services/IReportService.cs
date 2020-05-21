using FastReport;

namespace Solria.SAFT.Desktop.Services
{
    public interface IReportService
    {
        void Design(Report report);
        void Design(string fileName);
        void View(Report report);
        void View(string fileName);
    }
}
