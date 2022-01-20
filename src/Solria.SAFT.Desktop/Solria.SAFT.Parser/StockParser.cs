using Solria.SAFT.Parser.Models;
using Solria.SAFT.Parser.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Solria.SAFT.Parser
{
    public static class StockParser
    {
        private static StockFile stockFile;

        public static async Task<(StockFile stockFile, List<ValidationError> validations)> ReadFile(string filename)
        {
            if (filename.EndsWith("xml"))
            {
                return await ReadXml(filename);
            }
            else if (filename.EndsWith("csv"))
            {
                return ReadCsv(filename);
            }

            return (null, Parsers.Validations);
        }

        private static async Task<(StockFile stockFile, List<ValidationError> validations)> ReadXml(string filename)
        {
            //TODO: replace with Xmlreader
            stockFile = await Task.Run(() => XmlParserService.DeserializeXml<StockFile>(filename, Encoding.UTF8));

            //stockFile = new StockFile
            //{
            //    StockHeader = new StockHeader()
            //};

            ////register the Windows-1252 encoding
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            //var settings = new XmlReaderSettings
            //{
            //    Async = true,
            //    IgnoreComments = true,
            //    IgnoreWhitespace = true
            //};

            //using var reader = XmlReader.Create(filename, settings);

            //while (await reader.ReadAsync())
            //{
            //    if (string.IsNullOrWhiteSpace(reader.Name))
            //        continue;

            //    if (reader.NodeType == XmlNodeType.Element)
            //    {
            //        if (Parsers.StringEquals(reader.Name, "Header"))
            //        {

            //        }
            //    }
            //}

            return (stockFile, Parsers.Validations);
        }

        private static (StockFile stockFile, List<ValidationError> validations) ReadCsv(string filename)
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
                    var columns = lines[i].Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if (columns == null || columns.Length < 6)
                        continue;

                    Enum.TryParse(columns[0], out ProductCategory productCategory);
                    decimal.TryParse(columns[4], out decimal quantity);

                    stocks.Add(new Stock
                    {
                        ProductCategory = productCategory,
                        ProductCode = columns[1],
                        ProductDescription = columns[2],
                        ProductNumberCode = columns[3],
                        ClosingStockQuantity = quantity,
                        UnitOfMeasure = columns[5]
                    });
                }
                stockFile = new StockFile
                {
                    StockHeader = new StockHeader
                    {
                        FileVersion = "csv",
                        TaxRegistrationNumber = "Sem Informação"
                    },
                    Stock = stocks.ToArray()
                };
            }

            return (stockFile, Parsers.Validations);
        }
    }
}
