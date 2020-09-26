using FastReport;
using System;
using System.IO;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string report_file = Path.Combine(Environment.CurrentDirectory, "BillingDocument_A4.frx");

            // create report instance
            Report report = new Report();

            // load the existing report
            report.Load(report_file);

            // register the array
            var lines = new DocumentLine[]
            {
                new DocumentLine
                {
                    LineNumber = "1",
                    ProductCode = "ProductCode",
                    ProductDescription = "ProductDescription",
                    TaxCode = "NOR",
                    Quantity = 1,
                    UnitPrice = 2.35m,
                    TaxBase = 1.1m,
                    Description = "Description"
                }
            };
            report.RegisterData(lines, "Lines");

            report.SetParameterValue("DocNo", "FT FT20/1234");
            report.SetParameterValue("ATCUD", "123456");
            report.SetParameterValue("Status", "N");
            report.SetParameterValue("Date", DateTime.Now);
            report.SetParameterValue("CustomerTaxID", "999999990");
            report.SetParameterValue("CustomerName", "Consumidor final");
            report.SetParameterValue("GrossTotal", 10.95m);
            report.SetParameterValue("NetTotal", 7.5m);
            report.SetParameterValue("TaxPayable", 3.05m);
            report.SetParameterValue("Hash", "asdf");

            // prepare the report
            report.Prepare();

            //save prepared report
            string preparedReport = Path.Combine(Path.GetTempPath(), "Prepared_Report.fpx");
            string pdf_file_name = Path.Combine(Environment.CurrentDirectory, "pdf.pdf");
            report.SavePrepared(preparedReport);

            var pdfexport = new FastReport.Export.PdfSimple.PDFSimpleExport();
            report.Export(pdfexport, pdf_file_name);

            System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{pdf_file_name}\"");
        }
    }

    public class DocumentLine
    {
        public string LineNumber { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public string TaxCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? TaxBase { get; set; }
        public string Description { get; set; }
    }
}
