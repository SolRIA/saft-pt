using FastReport;
using System;
using System.Diagnostics;
using System.IO;

namespace Solria.SAFT.Desktop.Services
{
    public class ReportService : IReportService
    {
        readonly string designerPath = Path.Combine(Environment.CurrentDirectory, "Designer.exe");
        readonly string viewerPath = Path.Combine(Environment.CurrentDirectory, "Viewer", "Viewer.exe");
        //readonly string viewerPath = Path.Combine(@"C:\Users\frede\Desktop\FR reports\FastReport.Community-2020.3.0", "Viewer.exe");

        public void Design(Report report)
        {
            if (string.IsNullOrWhiteSpace(report?.FileName) || File.Exists(report.FileName) == false)
                return;

            ProcessStartInfo psi = new ProcessStartInfo(designerPath, report.FileName)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                //Verb = "runas"
            };
            Process.Start(psi);
        }
        public void Design(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || File.Exists(fileName) == false)
                return;

            ProcessStartInfo psi = new ProcessStartInfo(designerPath)
            {
                //CreateNoWindow = true,
                //UseShellExecute = false,
                //RedirectStandardError = true,
                //RedirectStandardOutput = true,
                //Verb = "runas"
            };
            Process.Start(psi);
        }

        public void View(Report report)
        {
            if (string.IsNullOrWhiteSpace(report?.FileName) || File.Exists(report.FileName) == false)
                return;

            ProcessStartInfo psi = new ProcessStartInfo(viewerPath, report.FileName)
            {
                //CreateNoWindow = true,
                //UseShellExecute = false,
                //RedirectStandardError = true,
                //RedirectStandardOutput = true,
                //Verb = "runas"
            };
            Process.Start(psi);
        }
        public void View(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || File.Exists(fileName) == false)
                return;

            ProcessStartInfo psi = new ProcessStartInfo(viewerPath, fileName)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                //Verb = "runas"
            };
            Process.Start(psi);
        }
    }
}
