using SolRIA.SAFT.Parser.Models;
using SolRIA.SAFT.Parser.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SolRIA.SAFT.Parser;

public static class StockParser
{
    private static StockFile stockFile;

    public static async Task<(StockFile stockFile, ValidationError[] validations)> ReadFile(string filename)
    {
        Parsers.ResetValidations();

        if (filename.EndsWith("xml"))
        {
            return await ReadXml(filename);
        }
        else if (filename.EndsWith("csv"))
        {
            return ReadCsv(filename);
        }

        return (null, []);
    }

    private static async Task<(StockFile stockFile, ValidationError[] validations)> ReadXml(string filename)
    {
        //TODO: replace with Xmlreader
        stockFile = await Task.Run(() => XmlParserService.DeserializeXml<StockFile>(filename, Encoding.UTF8));

        return (stockFile, Parsers.Validations.ToArray());
    }

    private static (StockFile stockFile, ValidationError[] validations) ReadCsv(string filename)
    {
        stockFile = new StockFile
        {
            StockHeader = new StockHeader()
        };

        //register the Windows-1252 encoding
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string[] lines = File.ReadAllLines(filename, CodePagesEncodingProvider.Instance.GetEncoding(1252));
        if (lines != null && lines.Length > 1)
        {
            var stocks = new List<Stock>();
            var splitChar = new char[] { ';' };
            for (int i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(splitChar, StringSplitOptions.None);
                if (columns == null || columns.Length < 6)
                    continue;

                decimal closingStockValue = 0;

                Enum.TryParse(columns[0], out ProductCategory productCategory);
                decimal.TryParse(columns[4], CultureInfo.CurrentCulture, out decimal quantity);
                if (columns.Length > 6)
                    decimal.TryParse(columns[6], CultureInfo.CurrentCulture, out closingStockValue);

                stocks.Add(new Stock
                {
                    ProductCategory = productCategory,
                    ProductCode = ReadQuotedField(columns[1], i + 1, 2, nameof(Stock.ProductCode)),
                    ProductDescription = ReadQuotedField(columns[2], i + 1, 3, nameof(Stock.ProductDescription)),
                    ProductNumberCode = ReadQuotedField(columns[3], i + 1, 4, nameof(Stock.ProductNumberCode)),
                    ClosingStockQuantity = quantity,
                    UnitOfMeasure = columns[5],
                    ClosingStockValue = closingStockValue
                });
            }
            stockFile = new StockFile
            {
                StockHeader = new StockHeader
                {
                    FileVersion = "csv",
                    TaxRegistrationNumber = "Sem Informação"
                },
                Stock = [.. stocks]
            };
        }

        return (stockFile, Parsers.Validations.ToArray());
    }

    private static string ReadQuotedField(string value, int line, int column, string field)
    {
        bool startsWithQuote = value.StartsWith('"');
        bool endsWithQuote = value.EndsWith('"');

        if (value.Length < 2 || !startsWithQuote || !endsWithQuote)
        {
            Parsers.Validations.Add(new ValidationError
            {
                Description = $"Linha {line}, coluna {column} ({field}): o valor deve começar e terminar com aspas duplas (\").",
                TypeofError = typeof(Stock),
                UID = line.ToString(CultureInfo.InvariantCulture),
                Field = field,
                Value = value
            });
        }

        int start = startsWithQuote ? 1 : 0;
        int end = endsWithQuote ? value.Length - 1 : value.Length;
        return value.Substring(start, Math.Max(0, end - start));
    }
}
