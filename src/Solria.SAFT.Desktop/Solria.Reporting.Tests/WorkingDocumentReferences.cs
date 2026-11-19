using Microsoft.Extensions.DependencyInjection;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.ViewModels;
using SolRIA.SAFT.Parser;
using SolRIA.SAFT.Parser.Services;
using System.Text;
using LegacyAuditFile = SolRIA.SAFT.Desktop.Models.SaftV4.AuditFile;

internal static class WorkingDocumentReferences
{
    public static async Task VerifyAsync()
    {
        var filename = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(filename, $"""
                <AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
                  <SourceDocuments>
                    <SalesInvoices>
                      <Invoice>
                        <InvoiceNo>FT 2026/1</InvoiceNo>
                        <Line>
                          <LineNumber>1</LineNumber>
                          <References />
                          <References><Reference>pf 2026/2</Reference><Reason>Faturação</Reason></References>
                          <Description>Linha de fatura</Description>
                        </Line>
                      </Invoice>
                    </SalesInvoices>
                    <WorkingDocuments>
                      {WorkDocument(1, "2026-01-01", "")}
                      {WorkDocument(2, "2026-01-01", "")}
                      {WorkDocument(3, "2026-01-01", "<References><Reference>pf 2026/3</Reference></References>")}
                      {WorkDocument(4, "2026-01-02", """
                        <References><Reference> </Reference></References>
                        <References><Reference>PF 2026/1</Reference><Reason>Conferência</Reason></References>
                        <References><Reference>PF 2026/1</Reference></References>
                        """)}
                    </WorkingDocuments>
                  </SourceDocuments>
                </AuditFile>
                """);

            var (streamed, _) = await SaftParser.ReadFile(filename);
            var legacy = SaftXmlParser.DeserializeXml<LegacyAuditFile>(filename, Encoding.UTF8);
            Check(legacy != null, "The in-memory parser must read the SAF-T fixture.");

            var validator = new SaftValidator(new DatabaseService());
            var services = new ServiceCollection();
            services.AddSingleton<ISaftValidator>(validator);
            AppBootstrap.ConfigureServices(services);

            foreach (var auditFile in new[] { streamed, new AuditFileMapper().Map(legacy) })
            {
                validator.SaftFile = auditFile;
                var source = auditFile.SourceDocuments;
                Check(source.WorkingDocuments.WorkDocument[3].Line[0].References[1].Reason == "Conferência",
                    "The working document's second reference must preserve its reason.");
                var invoiceLine = source.SalesInvoices.Invoice[0].Line[0];
                Check(invoiceLine.References[1].Reason == "Faturação" && invoiceLine.Description == "Linha de fatura",
                    "Repeated invoice references and the following line field must survive import.");

                var vm = new SaftWorkingDocumentsPageViewModel();
                ExpectDocuments(vm, 1, 2, 3, 4);
                vm.FiltroDataFim = vm.FiltroDataInicio;
                vm.ShowOnlyUnreferencedDocuments = true;
                ExpectDocuments(vm, 3);
                vm.Filter = "2026/2";
                vm.SearchCommand.Execute(null);
                ExpectDocuments(vm);
                vm.ShowOnlyUnreferencedDocuments = false;
                ExpectDocuments(vm, 2);
                vm.ShowOnlyUnreferencedDocuments = true;
                vm.SearchClearCommand.Execute(null);
                Check(!vm.ShowOnlyUnreferencedDocuments && vm.FiltroDataFim == vm.FiltroDataInicio,
                    "Clearing filters must uncheck the option and retain the date range.");
                ExpectDocuments(vm, 1, 2, 3);

                source.SalesInvoices = null;
                vm.ShowOnlyUnreferencedDocuments = true;
                ExpectDocuments(vm, 2, 3);
                source.WorkingDocuments = null;
                vm.SearchCommand.Execute(null);
                ExpectDocuments(vm);
            }
            Console.WriteLine("PASS: working document reference filters with both SAF-T readers");
        }
        finally
        {
            File.Delete(filename);
        }
    }

    private static string WorkDocument(int number, string date, string references) => $"""
        <WorkDocument>
          <DocumentNumber>PF 2026/{number}</DocumentNumber>
          <DocumentStatus><WorkStatus>N</WorkStatus></DocumentStatus>
          <WorkDate>{date}</WorkDate>
          <Line><LineNumber>1</LineNumber>{references}<Description>Linha</Description></Line>
        </WorkDocument>
        """;

    private static void ExpectDocuments(SaftWorkingDocumentsPageViewModel vm, params int[] numbers)
    {
        var actual = vm.Documents.Select(document => document.DocumentNumber).ToArray();
        Check(actual.SequenceEqual(numbers.Select(number => $"PF 2026/{number}")),
            $"Unexpected working document list: {string.Join(", ", actual)}");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
