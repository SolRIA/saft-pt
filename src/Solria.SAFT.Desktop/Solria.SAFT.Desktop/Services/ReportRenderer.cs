using Majorsilence.Reporting.Rdl;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SolRIA.SAFT.Desktop.Services;

internal static class ReportRenderer
{
    internal static string TemplatePath(ReportTemplate template) =>
        Path.Combine(AppContext.BaseDirectory, "Reports", ReportTemplates.FileName(template));

    internal static async Task<Report> CreateAsync(ReportRequest request)
    {
        var path = TemplatePath(request.Template);
        Report report = null;
        try
        {
            // Read on every opening. The live report keeps this definition for subsequent PDF exports.
            var source = await File.ReadAllTextAsync(path);
            var document = XDocument.Parse(source);
            ValidateDataSets(document, request);
            BindParameterDefaults(document, request);
            report = await new RDLParser(document.ToString()) { Folder = Path.GetDirectoryName(path) }.Parse();
            if (report == null || report.ErrorMaxSeverity > 4)
                throw new InvalidOperationException(report == null ? "Não foi possível interpretar o RDL." : string.Join(Environment.NewLine, report.ErrorItems.Cast<object>()));
            for (var i = 0; i < request.Tables.Count; i++)
                await report.DataSets["Data" + i].SetData(request.Tables[i].Data);
            await report.RunGetData(new Hashtable { ["Titulo"] = request.Title, ["Empresa"] = request.Company });
            if (report.ErrorMaxSeverity > 4)
                throw new InvalidOperationException(string.Join(Environment.NewLine, report.ErrorItems.Cast<object>()));
            return report;
        }
        catch (Exception ex)
        {
            report?.Dispose();
            var reason = ex is FileNotFoundException or DirectoryNotFoundException
                ? "O ficheiro do modelo não foi encontrado."
                : ex.Message;
            throw new InvalidOperationException($"Não foi possível carregar o modelo RDL '{path}': {reason}", ex);
        }
    }

    private static void BindParameterDefaults(XDocument document, ReportRequest request)
    {
        // Reporting 26.0.8 resets its parameter cache after BuildPages/RunRender.
        // Per-instance defaults preserve the same parameters across preview and repeated PDF exports.
        // Only parameter values are bound here; all layout remains in the external RDL.
        var ns = document.Root.Name.Namespace;
        foreach (var parameter in document.Root.Element(ns + "ReportParameters").Elements(ns + "ReportParameter"))
        {
            var name = (string)parameter.Attribute("Name");
            if (name != "Titulo" && name != "Empresa") continue;
            var value = name == "Titulo" ? request.Title : request.Company;
            var literal = "=\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
            var defaultValue = new XElement(ns + "DefaultValue", new XElement(ns + "Values", new XElement(ns + "Value", literal)));
            if (parameter.Element(ns + "DefaultValue") is { } existing) existing.ReplaceWith(defaultValue);
            else parameter.Add(defaultValue);
        }
    }

    private static void ValidateDataSets(XDocument document, ReportRequest request)
    {
        var root = document.Root ?? throw new InvalidOperationException("O ficheiro RDL está vazio.");
        var ns = root.Name.Namespace;
        var dataSets = root.Element(ns + "DataSets")?.Elements(ns + "DataSet").ToArray() ?? [];
        if (dataSets.Length != request.Tables.Count)
            throw new InvalidOperationException($"O modelo requer {request.Tables.Count} datasets (Data0, Data1, …).");
        for (var i = 0; i < request.Tables.Count; i++)
        {
            var name = "Data" + i;
            var dataSet = dataSets.SingleOrDefault(d => (string)d.Attribute("Name") == name)
                ?? throw new InvalidOperationException($"O dataset {name} está em falta.");
            var fields = dataSet.Element(ns + "Fields")?.Elements(ns + "Field")
                .Select(f => (string)f.Attribute("Name")).ToHashSet() ?? [];
            foreach (System.Data.DataColumn column in request.Tables[i].Data.Columns)
                if (!fields.Contains(column.ColumnName))
                    throw new InvalidOperationException($"O campo {name}.{column.ColumnName} está em falta.");
        }
        var parameters = root.Element(ns + "ReportParameters")?.Elements(ns + "ReportParameter")
            .Select(p => (string)p.Attribute("Name")).ToHashSet() ?? [];
        if (!parameters.Contains("Titulo") || !parameters.Contains("Empresa"))
            throw new InvalidOperationException("Os parâmetros Titulo e Empresa são obrigatórios.");
    }
}
