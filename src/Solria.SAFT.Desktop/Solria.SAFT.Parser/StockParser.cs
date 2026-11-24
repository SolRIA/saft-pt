using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SolRIA.SAFT.Parser;

public static class StockParser
{
    public static async Task<(StockFile stockFile, ValidationError[] validations)> ReadFile(string filename)
    {
        Parsers.ResetValidations();

        if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
            return (null, []);

        if (filename.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            return await ReadXml(filename);
        }
        else if (filename.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return ReadCsv(filename);
        }

        return (null, []);
    }

    private static async Task<(StockFile stockFile, ValidationError[] validations)> ReadXml(string filename)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var stockFile = new StockFile
        {
            StockHeader = new StockHeader()
        };
        var stocks = new List<Stock>();

        var settings = new XmlReaderSettings
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024, useAsync: true);
        using var reader = XmlReader.Create(fileStream, settings);

        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element)
                continue;

            if (Parsers.StringEquals(reader.LocalName, "StockHeader"))
            {
                stockFile.StockHeader = await ReadStockHeader(reader.ReadSubtree());
            }
            else if (Parsers.StringEquals(reader.LocalName, "Stock"))
            {
                stocks.Add(await ReadStockItem(reader.ReadSubtree()));
            }
        }

        stockFile.Stock = [.. stocks];
        return (stockFile, [.. Parsers.Validations]);
    }

    private static async Task<StockHeader> ReadStockHeader(XmlReader reader)
    {
        var header = new StockHeader();
        if (Parsers.StringEquals(reader.LocalName, "StockHeader") || string.IsNullOrWhiteSpace(reader.LocalName))
            await reader.ReadAsync();

        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element)
                continue;

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (Parsers.StringEquals(reader.LocalName, "FileVersion"))
                {
                    header.FileVersion = reader.ReadElementContentAsString();
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "TaxRegistrationNumber"))
                {
                    header.TaxRegistrationNumber = reader.ReadElementContentAsString();
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "FiscalYear"))
                {
                    header.FiscalYear = reader.ReadElementContentAsString();
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "EndDate"))
                {
                    header.EndDate = Parsers.ParseDate(reader.ReadElementContentAsString(), string.Empty, string.Empty, "StockHeader/EndDate", typeof(StockHeader));
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "NoStock"))
                {
                    var raw = reader.ReadElementContentAsString()?.Trim();
                    header.NoStock = string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) || raw == "1";
                    continue;
                }

                await reader.ReadAsync();
            }
        }

        return header;
    }

    private static async Task<Stock> ReadStockItem(XmlReader reader)
    {
        var stock = new Stock();
        if (Parsers.StringEquals(reader.LocalName, "Stock") || string.IsNullOrWhiteSpace(reader.LocalName))
            await reader.ReadAsync();

        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element)
                continue;

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (Parsers.StringEquals(reader.LocalName, "ProductCategory"))
                {
                    stock.ProductCategory = Parsers.ParseEnum<ProductCategory>(reader.ReadElementContentAsString(), string.Empty, stock.ProductCode, "Stock/ProductCategory", typeof(Stock));
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "ProductCode"))
                {
                    stock.ProductCode = reader.ReadElementContentAsString();
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "ProductDescription"))
                {
                    stock.ProductDescription = reader.ReadElementContentAsString();
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "ProductNumberCode"))
                {
                    stock.ProductNumberCode = reader.ReadElementContentAsString();
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "ClosingStockQuantity"))
                {
                    stock.ClosingStockQuantity = Parsers.ParseDecimal(reader.ReadElementContentAsString(), string.Empty, stock.ProductCode, "Stock/ClosingStockQuantity", typeof(Stock));
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "UnitOfMeasure"))
                {
                    stock.UnitOfMeasure = reader.ReadElementContentAsString();
                    continue;
                }
                if (Parsers.StringEquals(reader.LocalName, "ClosingStockValue"))
                {
                    stock.ClosingStockValue = Parsers.ParseDecimal(reader.ReadElementContentAsString(), string.Empty, stock.ProductCode, "Stock/ClosingStockValue", typeof(Stock));
                    continue;
                }

                await reader.ReadAsync();
            }
        }

        return stock;
    }

    private static (StockFile stockFile, ValidationError[] validations) ReadCsv(string filename)
    {
        var stockFile = new StockFile
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
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                var columns = lines[i].Split(splitChar, StringSplitOptions.None);
                if (columns == null || columns.Length < 6)
                {
                    Parsers.Validations.Add(new ValidationError
                    {
                        Description = $"Linha {i + 1}: número de colunas insuficiente ({columns?.Length ?? 0}, esperado pelo menos 6).",
                        TypeofError = typeof(Stock),
                        UID = (i + 1).ToString(CultureInfo.InvariantCulture),
                        Field = "Stock",
                        Value = lines[i]
                    });
                    continue;
                }

                if (!Enum.TryParse(columns[0]?.Trim(), out ProductCategory productCategory))
                {
                    Parsers.Validations.Add(new ValidationError
                    {
                        Description = $"Linha {i + 1}, coluna 1 (ProductCategory): categoria de produto inválida '{columns[0]}'.",
                        TypeofError = typeof(Stock),
                        UID = (i + 1).ToString(CultureInfo.InvariantCulture),
                        Field = "Stock/ProductCategory",
                        Value = columns[0]
                    });
                }

                string productCode = ReadQuotedField(columns[1], i + 1, 2, nameof(Stock.ProductCode));
                string productDescription = ReadQuotedField(columns[2], i + 1, 3, nameof(Stock.ProductDescription));
                string productNumberCode = ReadQuotedField(columns[3], i + 1, 4, nameof(Stock.ProductNumberCode));

                if (!TryParseCsvDecimal(columns[4], out decimal quantity))
                {
                    Parsers.Validations.Add(new ValidationError
                    {
                        Description = $"Linha {i + 1}, coluna 5 (ClosingStockQuantity): quantidade inválida '{columns[4]}'.",
                        TypeofError = typeof(Stock),
                        UID = (i + 1).ToString(CultureInfo.InvariantCulture),
                        FileID = productCode,
                        Field = "Stock/ClosingStockQuantity",
                        Value = columns[4]
                    });
                }

                decimal closingStockValue = 0;
                if (columns.Length > 6 && !string.IsNullOrWhiteSpace(columns[6]) && !TryParseCsvDecimal(columns[6], out closingStockValue))
                {
                    Parsers.Validations.Add(new ValidationError
                    {
                        Description = $"Linha {i + 1}, coluna 7 (ClosingStockValue): valor inválido '{columns[6]}'.",
                        TypeofError = typeof(Stock),
                        UID = (i + 1).ToString(CultureInfo.InvariantCulture),
                        FileID = productCode,
                        Field = "Stock/ClosingStockValue",
                        Value = columns[6]
                    });
                }

                stocks.Add(new Stock
                {
                    ProductCategory = productCategory,
                    ProductCode = productCode,
                    ProductDescription = productDescription,
                    ProductNumberCode = productNumberCode,
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

        return (stockFile, [.. Parsers.Validations]);
    }

    private static bool TryParseCsvDecimal(string raw, out decimal value)
    {
        if (decimal.TryParse(raw?.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
            return true;

        return decimal.TryParse(raw?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value);
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
