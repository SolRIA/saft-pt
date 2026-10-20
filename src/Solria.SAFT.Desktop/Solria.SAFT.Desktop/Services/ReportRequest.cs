using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SolRIA.SAFT.Desktop.Services;

/// <summary>A detached snapshot: a viewer never reads mutable page collections.</summary>
public sealed class ReportRequest
{
    public ReportTemplate Template { get; }
    public string Title { get; }
    public string FileName { get; }
    public bool AllowCsv { get; }
    public string Company { get; internal set; } = "";
    internal byte[] Excel { get; }
    internal IReadOnlyList<ReportTable> Tables { get; }
    internal bool HasData => Tables.Any(table => table.Data.Rows.Count > 0);
    internal static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("pt-PT");

    private ReportRequest(ReportTemplate template, string title, string fileName, bool allowCsv, byte[] excel, List<ReportTable> tables)
    {
        Template = template;
        Title = title;
        FileName = Path.GetFileNameWithoutExtension(fileName);
        AllowCsv = allowCsv;
        Excel = excel;
        Tables = tables;
    }

    public static ReportRequest FromWorkbook(ReportTemplate template, XLWorkbook workbook, string fileName, bool allowCsv = false)
    {
        var columns = ReportTemplates.ColumnCounts(template);
        if (workbook.Worksheets.Count != columns.Length)
            throw new ArgumentException($"O modelo {template} requer {columns.Length} tabelas.", nameof(workbook));
        workbook.RecalculateAllFormulas();
        var tables = workbook.Worksheets
            .Select((sheet, index) => ReportTable.Capture(sheet, columns[index]))
            .ToList();

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return new ReportRequest(template, Path.GetFileNameWithoutExtension(fileName).Replace('_', ' '), fileName, allowCsv, output.ToArray(), tables);
    }

    public static ReportRequest FromRows(ReportTemplate template, string title, string[] headers, IEnumerable<object[]> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Dados");
        sheet.Row(1).Style.Font.Bold = true;
        for (var c = 0; c < headers.Length; c++) sheet.Cell(1, c + 1).Value = headers[c];
        var r = 2;
        foreach (var row in rows)
        {
            for (var c = 0; c < headers.Length; c++)
            {
                var cell = sheet.Cell(r, c + 1);
                cell.Value = XLCellValue.FromObject(c < row.Length ? row[c] : null, Culture);
                if (cell.DataType == XLDataType.DateTime) cell.Style.DateFormat.Format = "dd/MM/yyyy";
            }
            r++;
        }
        sheet.Columns().AdjustToContents();
        return FromWorkbook(template, workbook, title + ".xlsx", true);
    }

    internal byte[] Csv()
    {
        var result = new StringBuilder();
        var table = Tables[0]; // CSV is enabled only for the single-table master lists.
        result.AppendLine(string.Join(';', table.Headers.Select(Escape)));
        foreach (var row in table.Values)
            result.AppendLine(string.Join(';', row.Select(value => Escape(ReportTable.Format(value)))));
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(result.ToString())).ToArray();
    }

    private static string Escape(string value) => value.IndexOfAny([';', '"', '\r', '\n']) >= 0
        ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
}

internal sealed class ReportTable
{
    public string Name { get; private init; }
    public string[] Headers { get; private init; }
    public List<object[]> Values { get; } = [];
    public DataTable Data { get; private init; }

    public static ReportTable Capture(IXLWorksheet sheet, int columnCount)
    {
        if ((sheet.LastColumnUsed()?.ColumnNumber() ?? 0) > columnCount)
            throw new ArgumentException($"A tabela {sheet.Name} contém mais de {columnCount} colunas.", nameof(sheet));
        var lastColumn = columnCount;
        var data = new DataTable();
        data.Columns.Add("Block", typeof(int));
        for (var c = 0; c < lastColumn; c++)
        {
            data.Columns.Add("C" + c, typeof(string));
            data.Columns.Add("H" + c, typeof(string));
        }
        var initialHeaders = Enumerable.Range(1, lastColumn).Select(c => sheet.Cell(1, c).GetString()).ToArray();
        var table = new ReportTable { Name = sheet.Name, Headers = initialHeaders, Data = data };
        var headers = initialHeaders;
        var block = 0;
        var afterGap = false;
        foreach (var row in sheet.RowsUsed())
        {
            // Blank worksheet rows delimit a document and its detail table.
            if (row.RowNumber() > 1 && sheet.Row(row.RowNumber() - 1).IsEmpty()) afterGap = true;
            if (row.RowNumber() == 1 || row.Style.Font.Bold)
            {
                headers = Enumerable.Range(1, lastColumn).Select(c => row.Cell(c).GetString()).ToArray();
                block++;
                afterGap = false;
                continue;
            }
            if (afterGap) { headers = initialHeaders; block++; afterGap = false; }
            var values = Enumerable.Range(1, lastColumn).Select(c => Value(row.Cell(c))).ToArray();
            table.Values.Add(values);
            var dataRow = data.NewRow();
            dataRow["Block"] = block;
            for (var c = 0; c < lastColumn; c++)
            {
                dataRow["C" + c] = Format(values[c]);
                dataRow["H" + c] = headers[c];
            }
            data.Rows.Add(dataRow);
        }
        return table;
    }

    private static object Value(IXLCell cell) => cell.DataType switch
    {
        XLDataType.Blank => null,
        XLDataType.Boolean => cell.GetBoolean(),
        XLDataType.Number => cell.GetDouble(),
        XLDataType.DateTime => cell.GetDateTime(),
        XLDataType.TimeSpan => cell.GetTimeSpan(),
        _ => cell.GetString()
    };

    internal static string Format(object value) => value switch
    {
        null => "",
        DateTime date => date.ToString("dd/MM/yyyy", ReportRequest.Culture),
        double number => number.ToString("0.################", ReportRequest.Culture),
        bool boolean => boolean ? "Sim" : "Não",
        IFormattable formatted => formatted.ToString(null, ReportRequest.Culture),
        _ => value.ToString()
    };
}
