using Majorsilence.Reporting.Rdl;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Desktop.ViewModels;
using System.Xml.Linq;

internal static class TemplateFiles
{
    public static async Task VerifyAsync(ReportRequest request)
    {
        var path = ReportRenderer.TemplatePath(request.Template);
        var original = await File.ReadAllBytesAsync(path);
        using var existingReport = await ReportRenderer.CreateAsync(request);
        try
        {
            File.Delete(path); // Only the test output copy; source templates are never modified.
            await ExpectFailure("missing file");
            await File.WriteAllTextAsync(path, "<Report>");
            await ExpectFailure("invalid XML");
            await File.WriteAllBytesAsync(path, original);
            var document = XDocument.Load(path);
            var ns = document.Root!.Name.Namespace;
            document.Descendants(ns + "Textbox").Single(t => (string)t.Attribute("Name") == "Title")
                .Element(ns + "Value")!.Value = "=\"MODELO ALTERADO: \" & Parameters!Titulo.Value";
            document.Descendants(ns + "Textbox").Single(t => (string)t.Attribute("Name") == "Title")
                .Element(ns + "Style")!.Element(ns + "FontSize")!.Value = "18pt";
            document.Save(path);
            using var changed = await ReportRenderer.CreateAsync(request);
            var changedPages = await changed.BuildPages();
            if (!Texts(changedPages).Contains("MODELO ALTERADO: " + request.Title))
                throw new Exception("An edited external template must be reloaded without rebuilding.");
            if (Texts(await existingReport.BuildPages()).Any(text => text.StartsWith("MODELO ALTERADO")))
                throw new Exception("An open report must retain its loaded definition for PDF export.");
            await File.WriteAllBytesAsync(path, original);
            document = XDocument.Load(path);
            document.Descendants(ns + "Value").First(v => v.Value == "=Fields!C0.Value").Value = "=Fields!MissingField.Value";
            document.Save(path);
            await ExpectFailure("invalid RDL expression");
            await File.WriteAllBytesAsync(path, original);
            document = XDocument.Load(path);
            document.Descendants(ns + "Field").First().Remove();
            document.Save(path);
            await ExpectFailure("missing dataset field");
        }
        finally { await File.WriteAllBytesAsync(path, original); }
        Console.WriteLine("PASS: external template reload, missing/invalid files, preserved open reports and Portuguese errors");

        async Task ExpectFailure(string scenario)
        {
            using var vm = new ReportViewModel(request, new TestDialogs());
            await vm.InitializeAsync(_ => throw new Exception("Invalid templates must not reach the viewer."));
            if (vm.IsReady || vm.IsBusy || !vm.Status.Contains(path) || !vm.Status.Contains("Não foi possível"))
                throw new Exception("Missing Portuguese error and template path for " + scenario + ": " + vm.Status);
        }
    }

    internal static string[] Texts(Pages pages) => pages.Cast<Page>()
        .SelectMany(page => page.OfType<PageText>()).Select(text => text.Text).ToArray();
}
