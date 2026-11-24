using Riok.Mapperly.Abstractions;
using SolRIA.SAFT.Desktop.Models;
using SolRIA.SAFT.Parser;
using SolRIA.SAFT.Parser.Models;
using SolRIA.SAFT.Parser.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace SolRIA.SAFT.Desktop.Services;

[Mapper]
public partial class AuditFileMapper
{
    public partial AuditFile Map(Models.SaftV4.AuditFile file);

    private static string MapXmlObjectToString(object value)
    {
        if (value == null) return null;
        if (value is string s) return s;
        if (value is XmlNode[] nodes)
            return string.Concat(nodes.Select(n => n.InnerText ?? n.Value));
        if (value is XmlNode node)
            return node.InnerText ?? node.Value;
        return value.ToString();
    }
}

public class SaftValidator : ISaftValidator
{
    private const string SolRiaPublicKeyXml = "<RSAKeyValue><Modulus>vU9AbSDUgXjuKEHl8UaYWfboVz0YRnMMspGHxZ59aJSEYwMYPNsmakNBS9is3+BGXd3AVlthaPmNW5BUuzICNyCQ+DldQ9dP8jr7xcyv1+E5xrMobF8+4xD2ST+DuAUw41ZTCZA3/r47BXonF/DWx+PpVhS68zorDWiJZUJGVg8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

    readonly CultureInfo enCulture = new("en-US");
    private readonly List<ValidationError> _mensagensErro;
    private readonly IDatabaseService _databaseService;

    private bool _useNewParser = true;
    public bool UseNewParser
    {
        get => _useNewParser;
        set
        {
            _useNewParser = value;
            var prefs = Preferences.Load();
            if (prefs.UseNewParser != value)
            {
                prefs.UseNewParser = value;
                Preferences.Save(prefs);
            }
        }
    }

    public SaftValidator(IDatabaseService databaseService = null)
    {
        _databaseService = databaseService ?? AppBootstrap.Resolve<IDatabaseService>();
        _mensagensErro = [];
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var prefs = Preferences.Load();
        _useNewParser = prefs.UseNewParser;
    }

    public string SaftFileName { get; set; }
    public string StockFileName { get; set; }

    public string PublicKeyFileName { get; set; }
    public string PrivateKeyFileName { get; set; }

    public AuditFile SaftFile { get; set; }

    public StockFile StockFile { get; set; }

    public int SaftHashValidationNumber { get; set; }
    public int SaftHashValidationErrorNumber { get; set; }

    private void AddError(params ValidationError[] error)
    {
        if (error is null) return;

        foreach (ValidationError e in error)
        {
            if (e is null) continue;

            if (string.IsNullOrWhiteSpace(e.Description)) continue;

            _mensagensErro.Add(e);
        }
    }

    public IList<ValidationError> GetErrors()
    {
        return _mensagensErro;
    }

    /// <summary>
    /// Loads the SAFT file
    /// </summary>
    public async Task OpenSaftFile(string filename, IProgress<SaftProgress> progress = null)
    {
        if (File.Exists(filename) == false)
            return;

        SaftFileName = filename;
        _mensagensErro.Clear();

        if (UseNewParser)
        {
            progress?.Report(new SaftProgress(0, "A abrir ficheiro SAF-T...", "A inicializar leitura em fluxo contínuo..."));

            var dbService = _databaseService ?? AppBootstrap.Resolve<IDatabaseService>();

            // Map 0..100% of streaming read to 0..70% of total process
            var streamProgress = progress == null ? null : new Progress<SaftProgress>(p =>
            {
                progress.Report(new SaftProgress
                {
                    ProgressPercentage = p.ProgressPercentage * 0.70,
                    CurrentStep = p.CurrentStep,
                    Detail = p.Detail
                });
            });

            var (saftFile, errors) = await SaftParser.ReadFile(
                SaftFileName,
                databaseService: dbService,
                keepInMemory: true,
                progress: streamProgress);

            SaftFile = saftFile;

            if (errors != null && errors.Count > 0)
                AddError([.. errors]);
        }
        else
        {
            progress?.Report(new SaftProgress(20, "A ler Ficheiro SAF-T...", "A carregar XML para a memória..."));
            var saftFileV4 = await Task.Run(() => SaftXmlParser.DeserializeXml<Models.SaftV4.AuditFile>(SaftFileName, Encoding.GetEncoding(1252)));
            progress?.Report(new SaftProgress(60, "A converter modelos...", "A mapear dados em memória..."));
            SaftFile = new AuditFileMapper().Map(saftFileV4);
        }

        //init custom navigation properties validations
        if (SaftFile == null)
        {
            //show error open file
            AddError(new ValidationError { Description = "Erro ao abrir o ficheiro" });
            return;
        }

        progress?.Report(new SaftProgress(72, "A preparar dados...", "A associar referências entre documentos e linhas..."));
        LinkDocumentLineProperties();

        //Do validations on fields
        progress?.Report(new SaftProgress(76, "A validar Cabeçalho...", "Verificação de NIF, datas e versão fiscal"));
        if (SaftFile.Header != null)
            ValidateHeader(SaftFile.Header);
        else
            AddError(new ValidationError { Description = "Cabeçalho (Header) inexistente.", Field = "Header", TypeofError = typeof(Header) });

        progress?.Report(new SaftProgress(80, "A validar Estrutura XSD...", "Conformidade com o esquema oficial da AT"));
        var schemaVersion = SaftFile.Header?.AuditFileVersion ?? SaftFileVersion.V10401;
        await Task.Run(() => ValidaEstruturaXSD(schemaVersion));

        if (SaftFile.MasterFiles != null)
        {
            progress?.Report(new SaftProgress(84, "A validar Tabelas Mestras...", "Clientes, Fornecedores, Produtos, Impostos e Plano de Contas"));
            ValidateCustomers(SaftFile.MasterFiles.Customer);
            ValidateProducts(SaftFile.MasterFiles.Product);
            ValidateSupplier(SaftFile.MasterFiles.Supplier);
            ValidateTax(SaftFile.MasterFiles.TaxTable);
            ValidateGeneralLedgerAccounts(SaftFile.MasterFiles.GeneralLedgerAccounts);
        }

        if (SaftFile.GeneralLedgerEntries != null)
        {
            progress?.Report(new SaftProgress(87, "A validar Movimentos Contabilísticos...", "Validação de diários, lançamentos e saldos"));
            ValidateGeneralLedgerEntries(SaftFile.GeneralLedgerEntries, SaftFile.MasterFiles?.GeneralLedgerAccounts);
        }

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.SalesInvoices != null)
        {
            progress?.Report(new SaftProgress(89, "A validar Faturação...", "Validação de faturas, linhas e totais"));
            await Task.Run(() => ValidateInvoices(SaftFile.SourceDocuments.SalesInvoices, SaftFile.Header, SaftFile.MasterFiles));
        }

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.Payments != null)
        {
            progress?.Report(new SaftProgress(93, "A validar Pagamentos...", "Validação de recibos e meios de pagamento"));
            ValidatePayments(SaftFile.SourceDocuments.Payments, SaftFile.Header, SaftFile.MasterFiles, includeExtendedChecks: true);
        }

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.MovementOfGoods != null)
        {
            progress?.Report(new SaftProgress(95, "A validar Mercadorias...", "Validação de guias de transporte e movimentação"));
            ValidateMovementOfGoods(SaftFile.SourceDocuments.MovementOfGoods, SaftFile.Header, SaftFile.MasterFiles);
        }

        if (SaftFile.SourceDocuments != null && SaftFile.SourceDocuments.WorkingDocuments != null)
        {
            progress?.Report(new SaftProgress(97, "A validar Documentos de Conferência...", "Validação de documentos de conferência"));
            ValidateWorkDocument(SaftFile.SourceDocuments.WorkingDocuments, SaftFile.Header, SaftFile.MasterFiles);
        }

        //check for solria saft, if true validate the hash
        SaftHashValidationNumber = 0;
        SaftHashValidationErrorNumber = 0;
        if (SaftFile.Header?.SoftwareCertificateNumber == "2340")
        {
            progress?.Report(new SaftProgress(98, "A validar Assinaturas Digitais (Hash)...", "Verificação criptográfica SHA1/RSA de faturas"));
            await Task.Run(() => SolRiaValidateSaftHash(SaftFile));
        }

        progress?.Report(new SaftProgress(100, "Concluído!", "Leitura e validação terminadas com sucesso."));
    }

    private void LinkDocumentLineProperties()
    {
        if (SaftFile.SourceDocuments?.SalesInvoices?.Invoice != null)
        {
            foreach (var invoice in SaftFile.SourceDocuments.SalesInvoices.Invoice)
            {
                foreach (var line in invoice.Line ?? [])
                {
                    line.InvoiceNo = invoice.InvoiceNo;

                    if (line.ItemElementName == ItemChoiceType4.CreditAmount)
                    {
                        if (line.CreditAmount == null || line.CreditAmount == 0)
                            line.CreditAmount = line.Item;
                        else if (line.Item == 0 && line.CreditAmount != null)
                            line.Item = line.CreditAmount.Value;
                    }
                    if (line.ItemElementName == ItemChoiceType4.DebitAmount)
                    {
                        if (line.DebitAmount == null || line.DebitAmount == 0)
                            line.DebitAmount = line.Item;
                        else if (line.Item == 0 && line.DebitAmount != null)
                            line.Item = line.DebitAmount.Value;
                    }
                }
            }
        }

        if (SaftFile.SourceDocuments?.MovementOfGoods?.StockMovement != null)
        {
            foreach (var doc in SaftFile.SourceDocuments.MovementOfGoods.StockMovement)
            {
                if (string.IsNullOrEmpty(doc.CustomerID) && doc.ItemElementName == ItemChoiceType5.CustomerID)
                    doc.CustomerID = doc.Item;

                foreach (var line in doc.Line ?? [])
                {
                    line.DocNo = doc.DocumentNumber;

                    if (line.ItemElementName == ItemChoiceType6.CreditAmount)
                    {
                        if (line.CreditAmount == null || line.CreditAmount == 0)
                            line.CreditAmount = line.Item;
                        else if (line.Item == 0 && line.CreditAmount != null)
                            line.Item = line.CreditAmount.Value;
                    }
                    if (line.ItemElementName == ItemChoiceType6.DebitAmount)
                    {
                        if (line.DebitAmount == null || line.DebitAmount == 0)
                            line.DebitAmount = line.Item;
                        else if (line.Item == 0 && line.DebitAmount != null)
                            line.Item = line.DebitAmount.Value;
                    }
                }
            }
        }

        if (SaftFile.SourceDocuments?.Payments?.Payment != null)
        {
            foreach (var payment in SaftFile.SourceDocuments.Payments.Payment)
            {
                foreach (var line in payment.Line ?? [])
                {
                    line.DocNo = payment.PaymentRefNo;

                    if (line.ItemElementName == ItemChoiceType8.CreditAmount)
                    {
                        if (line.CreditAmount == null || line.CreditAmount == 0)
                            line.CreditAmount = line.Item;
                        else if (line.Item == 0 && line.CreditAmount != null)
                            line.Item = line.CreditAmount.Value;
                    }
                    if (line.ItemElementName == ItemChoiceType8.DebitAmount)
                    {
                        if (line.DebitAmount == null || line.DebitAmount == 0)
                            line.DebitAmount = line.Item;
                        else if (line.Item == 0 && line.DebitAmount != null)
                            line.Item = line.DebitAmount.Value;
                    }
                }
            }
        }

        if (SaftFile.SourceDocuments?.WorkingDocuments?.WorkDocument != null)
        {
            foreach (var doc in SaftFile.SourceDocuments.WorkingDocuments.WorkDocument)
            {
                foreach (var line in doc.Line ?? [])
                {
                    line.DocNo = doc.DocumentNumber;

                    if (line.ItemElementName == ItemChoiceType7.CreditAmount)
                    {
                        if (line.CreditAmount == null || line.CreditAmount == 0)
                            line.CreditAmount = line.Item;
                        else if (line.Item == 0 && line.CreditAmount != null)
                            line.Item = line.CreditAmount.Value;
                    }
                    if (line.ItemElementName == ItemChoiceType7.DebitAmount)
                    {
                        if (line.DebitAmount == null || line.DebitAmount == 0)
                            line.DebitAmount = line.Item;
                        else if (line.Item == 0 && line.DebitAmount != null)
                            line.Item = line.DebitAmount.Value;
                    }
                }
            }
        }
    }

    public async Task OpenStockFile(string filename, IProgress<SaftProgress> progress = null)
    {
        if (File.Exists(filename) == false)
            return;

        StockFileName = filename;
        _mensagensErro.Clear();

        progress?.Report(new SaftProgress(10, "A abrir ficheiro de existências...", Path.GetFileName(filename)));

        progress?.Report(new SaftProgress(40, "A ler ficheiro...", "A processar registos de existências"));
        var (stockFile, errors) = await StockParser.ReadFile(StockFileName);

        StockFile = stockFile;

        progress?.Report(new SaftProgress(75, "A verificar dados...", "A validar estrutura e existências"));
        AddError(errors);

        if (StockFile != null)
        {
            if (StockFileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                await Task.Run(() => ValidateStockSchema(StockFileName, StockFile.StockHeader?.FileVersion));

            ValidateStockFile(StockFile);
        }

        progress?.Report(new SaftProgress(100, "Concluído!", "Ficheiro de existências lido com sucesso."));
    }

    internal void ValidateStockFile(StockFile stockFile)
    {
        if (stockFile == null) return;

        var header = stockFile.StockHeader;
        bool isCsv = string.Equals(header?.FileVersion, "csv", StringComparison.OrdinalIgnoreCase);

        if (header == null)
        {
            AddError(new ValidationError
            {
                Description = "Cabeçalho de existências (StockHeader) inexistente.",
                Field = "StockHeader",
                TypeofError = typeof(StockHeader)
            });
        }
        else if (!isCsv)
        {
            if (!Validations.CheckTaxRegistrationNumber(header.TaxRegistrationNumber))
            {
                AddError(new ValidationError
                {
                    Description = "NIF inválido no cabeçalho do ficheiro de existências.",
                    Field = "StockHeader/TaxRegistrationNumber",
                    Value = header.TaxRegistrationNumber,
                    TypeofError = typeof(StockHeader)
                });
            }

            if (string.IsNullOrEmpty(header.FiscalYear) || !int.TryParse(header.FiscalYear, out int fiscalYear) || fiscalYear < 2000 || fiscalYear > 9999)
            {
                AddError(new ValidationError
                {
                    Description = "Ano fiscal inválido no cabeçalho do ficheiro de existências.",
                    Field = "StockHeader/FiscalYear",
                    Value = header.FiscalYear,
                    TypeofError = typeof(StockHeader)
                });
            }

            if (header.EndDate == DateTime.MinValue)
            {
                AddError(new ValidationError
                {
                    Description = "Data de fim do período inválida no ficheiro de existências.",
                    Field = "StockHeader/EndDate",
                    Value = header.EndDate.ToString(),
                    TypeofError = typeof(StockHeader)
                });
            }

            if (header.NoStock && (stockFile.Stock?.Length ?? 0) > 0)
            {
                AddError(new ValidationError
                {
                    Description = "O cabeçalho indica ausência de existências (NoStock=true), mas existem registos de stock.",
                    Field = "StockHeader/NoStock",
                    Value = header.NoStock.ToString(),
                    TypeofError = typeof(StockHeader)
                });
            }
        }

        var items = stockFile.Stock ?? [];
        var seenProductCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (item == null) continue;

            string uid = (i + 1).ToString(CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(item.ProductCode) || item.ProductCode.Length > 60)
            {
                AddError(new ValidationError
                {
                    Description = $"Código do produto inválido na linha {i + 1}.",
                    Field = "Stock/ProductCode",
                    FileID = item.ProductCode,
                    Value = item.ProductCode,
                    TypeofError = typeof(Stock),
                    UID = uid
                });
            }
            else if (!seenProductCodes.Add(item.ProductCode))
            {
                AddError(new ValidationError
                {
                    Description = $"Código de produto duplicado no inventário: {item.ProductCode}.",
                    Field = "Stock/ProductCode",
                    FileID = item.ProductCode,
                    Value = item.ProductCode,
                    TypeofError = typeof(Stock),
                    UID = uid
                });
            }

            if (string.IsNullOrEmpty(item.ProductDescription) || item.ProductDescription.Length > 200)
            {
                AddError(new ValidationError
                {
                    Description = $"Descrição do produto inválida na linha {i + 1}.",
                    Field = "Stock/ProductDescription",
                    FileID = item.ProductCode,
                    Value = item.ProductDescription,
                    TypeofError = typeof(Stock),
                    UID = uid
                });
            }

            if (string.IsNullOrEmpty(item.ProductNumberCode) || item.ProductNumberCode.Length > 60)
            {
                AddError(new ValidationError
                {
                    Description = $"Código de barras / número do produto inválido na linha {i + 1}.",
                    Field = "Stock/ProductNumberCode",
                    FileID = item.ProductCode,
                    Value = item.ProductNumberCode,
                    TypeofError = typeof(Stock),
                    UID = uid
                });
            }

            if (string.IsNullOrEmpty(item.UnitOfMeasure) || item.UnitOfMeasure.Length > 20)
            {
                AddError(new ValidationError
                {
                    Description = $"Unidade de medida inválida na linha {i + 1}.",
                    Field = "Stock/UnitOfMeasure",
                    FileID = item.ProductCode,
                    Value = item.UnitOfMeasure,
                    TypeofError = typeof(Stock),
                    UID = uid
                });
            }

            if (item.ClosingStockQuantity < 0)
            {
                AddError(new ValidationError
                {
                    Description = $"Quantidade de existência negativa na linha {i + 1}.",
                    Field = "Stock/ClosingStockQuantity",
                    FileID = item.ProductCode,
                    Value = item.ClosingStockQuantity.ToString(CultureInfo.InvariantCulture),
                    TypeofError = typeof(Stock),
                    UID = uid
                });
            }

            if (item.ClosingStockValue < 0)
            {
                AddError(new ValidationError
                {
                    Description = $"Valor de existência negativo na linha {i + 1}.",
                    Field = "Stock/ClosingStockValue",
                    FileID = item.ProductCode,
                    Value = item.ClosingStockValue.ToString(CultureInfo.InvariantCulture),
                    TypeofError = typeof(Stock),
                    UID = uid
                });
            }
        }
    }

    private void ValidateStockSchema(string stockFilename, string fileVersion)
    {
        try
        {
            string schemaName = fileVersion switch
            {
                "2_01" => "Inventario_2_01.xsd",
                "1_02" => "Stock_1_2.xsd",
                _ => DetectStockSchemaFromXml(stockFilename)
            };

            if (string.IsNullOrEmpty(schemaName))
                return;

            string schemaFilename = ResolveSchemaPath("Models", schemaName) ?? ResolveSchemaPath("Schemas", schemaName);
            if (string.IsNullOrEmpty(schemaFilename))
            {
                AddError(new ValidationError
                {
                    Description = $"Esquema XSD de inventário não encontrado: {schemaName}",
                    Field = "Schema",
                    TypeofError = typeof(SchemaResults),
                    ValidationType = ValidationErrorType.SCHEMA
                });
                return;
            }

            ValidateXmlAgainstSchema(stockFilename, schemaFilename);
        }
        catch (Exception error)
        {
            AddError(new ValidationError
            {
                Description = $"Erro na validação XSD do ficheiro de existências: {error.Message}",
                Field = "Schema",
                TypeofError = typeof(SchemaResults),
                ValidationType = ValidationErrorType.SCHEMA
            });
        }
    }

    private static string DetectStockSchemaFromXml(string stockFilename)
    {
        try
        {
            using var reader = XmlReader.Create(stockFilename);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.NamespaceURI.Contains("PT_2_01", StringComparison.OrdinalIgnoreCase))
                        return "Inventario_2_01.xsd";
                    if (reader.NamespaceURI.Contains("PT_1_02", StringComparison.OrdinalIgnoreCase))
                        return "Stock_1_2.xsd";
                    break;
                }
            }
        }
        catch
        {
            // Ignorado; validação seguinte reporta erro XML se malformado.
        }
        return "Inventario_2_01.xsd";
    }

    private void SolRiaValidateSaftHash(AuditFile auditFile)
    {
        using var rsaCryptokey = RSA.Create();
        rsaCryptokey.FromXmlString(SolRiaPublicKeyXml);
        ValidateAllDocumentHashes(auditFile, rsaCryptokey, countStats: true);
    }

    private void ValidateAllDocumentHashes(AuditFile auditFile, RSA rsaCryptokey, bool countStats)
    {
        var toHash = new StringBuilder();

        if (auditFile.SourceDocuments?.SalesInvoices?.Invoice != null)
        {
            ValidateSeriesHashes(
                auditFile.SourceDocuments.SalesInvoices.Invoice,
                i => i.InvoiceNo,
                i => i.InvoiceType.ToString(),
                i => i.DocumentStatus?.SourceBilling ?? SAFTPTSourceBilling.P,
                i => i.Hash,
                i => i.InvoiceDate,
                i => i.SystemEntryDate,
                i => i.DocumentTotals?.GrossTotal ?? 0m,
                i => i.Pk,
                typeof(SourceDocumentsSalesInvoicesInvoice),
                rsaCryptokey,
                toHash,
                countStats);
        }

        if (auditFile.SourceDocuments?.MovementOfGoods?.StockMovement != null)
        {
            ValidateSeriesHashes(
                auditFile.SourceDocuments.MovementOfGoods.StockMovement,
                m => m.DocumentNumber,
                m => m.MovementType.ToString(),
                m => m.DocumentStatus?.SourceBilling ?? SAFTPTSourceBilling.P,
                m => m.Hash,
                m => m.MovementDate,
                m => m.SystemEntryDate,
                m => m.DocumentTotals?.GrossTotal ?? 0m,
                m => m.Pk,
                typeof(SourceDocumentsMovementOfGoodsStockMovement),
                rsaCryptokey,
                toHash,
                countStats);
        }

        if (auditFile.SourceDocuments?.WorkingDocuments?.WorkDocument != null)
        {
            ValidateSeriesHashes(
                auditFile.SourceDocuments.WorkingDocuments.WorkDocument,
                d => d.DocumentNumber,
                d => d.WorkType.ToString(),
                d => d.DocumentStatus?.SourceBilling ?? SAFTPTSourceBilling.P,
                d => d.Hash,
                d => d.WorkDate,
                d => d.SystemEntryDate,
                d => d.DocumentTotals?.GrossTotal ?? 0m,
                d => d.Pk,
                typeof(SourceDocumentsWorkingDocumentsWorkDocument),
                rsaCryptokey,
                toHash,
                countStats);
        }
    }

    private void ValidateSeriesHashes<TDoc>(
        TDoc[] docs,
        Func<TDoc, string> getDocNo,
        Func<TDoc, string> getDocType,
        Func<TDoc, SAFTPTSourceBilling> getSourceBilling,
        Func<TDoc, string> getHash,
        Func<TDoc, DateTime> getDocDate,
        Func<TDoc, DateTime> getSystemEntryDate,
        Func<TDoc, decimal> getGrossTotal,
        Func<TDoc, string> getPk,
        Type docErrorType,
        RSA rsaCryptokey,
        StringBuilder toHash,
        bool countStats)
    {
        var parsedGroups = docs
            .Select(d =>
            {
                string docNo = getDocNo(d);
                bool ok = Validations.TryParseDocumentNumber(docNo, out string parsedType, out string series, out int number);
                return (Doc: d, DocNo: docNo, Type: ok ? parsedType : getDocType(d), Series: series ?? string.Empty, Number: number, Parsed: ok);
            })
            .Where(x => x.Parsed)
            .GroupBy(x => (x.Type, x.Series));

        foreach (var group in parsedGroups)
        {
            var ordered = group.OrderBy(x => x.Number).ToArray();
            for (int i = 0; i < ordered.Length; i++)
            {
                var current = ordered[i];
                if (getSourceBilling(current.Doc) != SAFTPTSourceBilling.P)
                    continue;

                string hashAnterior;
                if (current.Number == 1)
                {
                    hashAnterior = string.Empty;
                }
                else if (i > 0 && ordered[i - 1].Number == current.Number - 1)
                {
                    hashAnterior = getHash(ordered[i - 1].Doc) ?? string.Empty;
                }
                else
                {
                    // Documento anterior em falta (ex.: SAF-T parcial) -> não é possível validar a cadeia a partir daqui.
                    continue;
                }

                string docHash = getHash(current.Doc);
                if (string.IsNullOrEmpty(docHash))
                {
                    AddError(new ValidationError
                    {
                        Description = $"A assinatura do documento {current.DocNo} não existe.",
                        Field = "Hash",
                        FileID = current.DocNo,
                        Value = current.DocNo,
                        TypeofError = countStats ? typeof(HashResults) : docErrorType,
                        UID = getPk(current.Doc)
                    });
                    if (countStats) SaftHashValidationErrorNumber++;
                    continue;
                }

                SolRiaBuildHash(toHash, getDocDate(current.Doc), getSystemEntryDate(current.Doc), current.DocNo, getGrossTotal(current.Doc), hashAnterior);

                byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
                byte[] hashBuffer;
                try
                {
                    hashBuffer = Convert.FromBase64String(docHash);
                }
                catch (FormatException)
                {
                    AddError(new ValidationError
                    {
                        Description = $"A assinatura do documento {current.DocNo} não está em formato Base64 válido.",
                        Field = "Hash",
                        FileID = current.DocNo,
                        Value = current.DocNo,
                        TypeofError = countStats ? typeof(HashResults) : docErrorType,
                        UID = getPk(current.Doc)
                    });
                    if (countStats) SaftHashValidationErrorNumber++;
                    continue;
                }

                bool isHashCorrect = rsaCryptokey.VerifyData(stringToHashBuffer, hashBuffer, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                if (!isHashCorrect)
                {
                    AddError(new ValidationError
                    {
                        Description = $"A assinatura do documento {current.DocNo} é inválida.",
                        Field = "Hash",
                        FileID = current.DocNo,
                        Value = current.DocNo,
                        TypeofError = countStats ? typeof(HashResults) : docErrorType,
                        UID = getPk(current.Doc)
                    });
                    if (countStats) SaftHashValidationErrorNumber++;
                }
                else if (countStats)
                {
                    SaftHashValidationNumber++;
                }
            }
        }
    }

    private static void SolRiaBuildHash(StringBuilder toHash, DateTime documentDate, DateTime systemEntryDate, string documentNo, decimal documentTotal, string hashAnterior)
    {
        toHash.Clear();

        toHash.AppendFormat(CultureInfo.InvariantCulture,
            "{0:yyyy-MM-dd};{1:yyyy-MM-ddTHH:mm:ss};{2};{3:0.00};{4}"
            , documentDate
            , systemEntryDate
            , documentNo
            , documentTotal
            , hashAnterior);
    }

    /// <summary>
    /// Validate the hashes in the file for the sales invoices
    /// </summary>
    public void ValidateHashV4()
    {
        if (SaftFile != null)
            ValidateSaftHash(SaftFile);
    }

    /// <summary>
    /// Validate the hashes in the file for the working documents
    /// </summary>
    public void ValidateHashWD()
    {
        if (SaftFile != null)
            ValidateSaftHashWD(SaftFile);
    }

    /// <summary>
    /// Validate the hashes in the file for the movement of goods
    /// </summary>
    public void ValidateHashMG()
    {
        if (SaftFile != null)
            ValidateSaftHashMG(SaftFile);
    }

    /// <summary>
    /// Validate the saft file against the schema file
    /// </summary>
    public void ValidateSchemaV4()
    {
        if (SaftFile != null && SaftFile.Header != null)
            ValidateSchema(SaftFile.Header.AuditFileVersion);
    }

    /// <summary>
    /// Validate the saft file against a specific schema file
    /// </summary>
    public void ValidateSchema(string fileversion)
    {
        ValidaEstruturaXSD(fileversion);
    }

    /// <summary>
    /// Generate the hash with the provided private key for the sales invoices
    /// </summary>
    public void GenerateInvoiceHash()
    {
        if (SaftFile != null)
            GenerateHash(SaftFile);
    }

    /// <summary>
    /// Generate the hash with the provided private key for the working documents
    /// </summary>
    public void GenerateHashWD()
    {
        if (SaftFile != null)
            GenerateHashWD(SaftFile);
    }

    /// <summary>
    /// Generate the hash with the provided private key for the movement of goods
    /// </summary>
    public void GenerateHashMGV4()
    {
        if (SaftFile != null)
            GenerateHashMG(SaftFile);
    }

    #region metodos privados

    void GenerateHash(AuditFile saftfile)
    {
        if (saftfile?.SourceDocuments?.SalesInvoices?.Invoice == null)
            return;

        using var rsaCryptokey = RSA.Create();
        rsaCryptokey.FromXmlString(GetRSAPrivateKey());
        var toHash = new StringBuilder();

        var groups = saftfile.SourceDocuments.SalesInvoices.Invoice
            .Select(i => (Doc: i, Ok: Validations.TryParseDocumentNumber(i.InvoiceNo, out string t, out string s, out int n), Type: t, Series: s, Number: n))
            .Where(x => x.Ok)
            .GroupBy(x => (x.Type, x.Series));

        foreach (var group in groups)
        {
            var ordered = group.OrderBy(x => x.Number).ToArray();
            for (int i = 0; i < ordered.Length; i++)
            {
                var current = ordered[i];
                bool usaHashAnterior = i > 0 && ordered[i - 1].Number == current.Number - 1;
                FormatStringToHash(ref toHash, current.Doc, usaHashAnterior ? ordered[i - 1].Doc.Hash : string.Empty);

                byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
                byte[] r = rsaCryptokey.SignData(stringToHashBuffer, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                current.Doc.HashTest = Convert.ToBase64String(r);
            }
        }
    }

    void GenerateHashWD(AuditFile saftfile)
    {
        if (saftfile?.SourceDocuments?.WorkingDocuments?.WorkDocument == null)
            return;

        using var rsaCryptokey = RSA.Create();
        rsaCryptokey.FromXmlString(GetRSAPrivateKey());
        var toHash = new StringBuilder();

        var groups = saftfile.SourceDocuments.WorkingDocuments.WorkDocument
            .Select(i => (Doc: i, Ok: Validations.TryParseDocumentNumber(i.DocumentNumber, out string t, out string s, out int n), Type: t, Series: s, Number: n))
            .Where(x => x.Ok)
            .GroupBy(x => (x.Type, x.Series));

        foreach (var group in groups)
        {
            var ordered = group.OrderBy(x => x.Number).ToArray();
            for (int i = 0; i < ordered.Length; i++)
            {
                var current = ordered[i];
                bool usaHashAnterior = i > 0 && ordered[i - 1].Number == current.Number - 1;
                FormatStringToHash(ref toHash, current.Doc, usaHashAnterior ? ordered[i - 1].Doc.Hash : string.Empty);

                byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
                byte[] r = rsaCryptokey.SignData(stringToHashBuffer, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                current.Doc.HashTest = Convert.ToBase64String(r);
            }
        }
    }

    void GenerateHashMG(AuditFile saftfile)
    {
        if (saftfile?.SourceDocuments?.MovementOfGoods?.StockMovement == null)
            return;

        using var rsaCryptokey = RSA.Create();
        rsaCryptokey.FromXmlString(GetRSAPrivateKey());
        var toHash = new StringBuilder();

        var groups = saftfile.SourceDocuments.MovementOfGoods.StockMovement
            .Select(i => (Doc: i, Ok: Validations.TryParseDocumentNumber(i.DocumentNumber, out string t, out string s, out int n), Type: t, Series: s, Number: n))
            .Where(x => x.Ok)
            .GroupBy(x => (x.Type, x.Series));

        foreach (var group in groups)
        {
            var ordered = group.OrderBy(x => x.Number).ToArray();
            for (int i = 0; i < ordered.Length; i++)
            {
                var current = ordered[i];
                bool usaHashAnterior = i > 0 && ordered[i - 1].Number == current.Number - 1;
                FormatStringToHash(ref toHash, current.Doc, usaHashAnterior ? ordered[i - 1].Doc.Hash : string.Empty);

                byte[] stringToHashBuffer = Encoding.UTF8.GetBytes(toHash.ToString());
                byte[] r = rsaCryptokey.SignData(stringToHashBuffer, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                current.Doc.HashTest = Convert.ToBase64String(r);
            }
        }
    }

    void FormatStringToHash(ref StringBuilder toHash, SourceDocumentsSalesInvoicesInvoice invoice, string hashAnterior)
    {
        toHash.Clear();
        toHash.AppendFormat("{0};{1};{2};{3};{4}"
                    , invoice.InvoiceDate.ToString("yyyy-MM-dd")
                    , invoice.SystemEntryDate.ToString("yyyy-MM-ddTHH:mm:ss")
                    , invoice.InvoiceNo
                    , invoice.DocumentTotals.GrossTotal.ToString("0.00", enCulture)
                    , hashAnterior);
    }
    void FormatStringToHash(ref StringBuilder toHash, SourceDocumentsWorkingDocumentsWorkDocument doc, string hashAnterior)
    {
        toHash.Clear();
        toHash.AppendFormat("{0};{1};{2};{3};{4}"
                    , doc.WorkDate.ToString("yyyy-MM-dd")
                    , doc.SystemEntryDate.ToString("yyyy-MM-ddTHH:mm:ss")
                    , doc.DocumentNumber
                    , doc.DocumentTotals.GrossTotal.ToString("0.00", enCulture)
                    , hashAnterior);
    }
    void FormatStringToHash(ref StringBuilder toHash, SourceDocumentsMovementOfGoodsStockMovement doc, string hashAnterior)
    {
        toHash.Clear();
        toHash.AppendFormat("{0};{1};{2};{3};{4}"
                    , doc.MovementDate.ToString("yyyy-MM-dd")
                    , doc.SystemEntryDate.ToString("yyyy-MM-ddTHH:mm:ss")
                    , doc.DocumentNumber
                    , doc.DocumentTotals.GrossTotal.ToString("0.00", enCulture)
                    , hashAnterior);
    }

    /// <summary>
    /// Reads the public key file and returns the RSA public key
    /// </summary>
    public string GetRSAPublicKey()
    {
        if (File.Exists(PublicKeyFileName))
        {
            string publickey = File.ReadAllText(PublicKeyFileName).Trim();

            if (publickey.StartsWith(RSAKeys.PEM_PUB_HEADER) && publickey.EndsWith(RSAKeys.PEM_PUB_FOOTER))
            {
                var rsa = new RSAKeys();
                rsa.DecodePEMKey(publickey);
                return rsa.PublicKey;
            }
            return publickey;
        }

        return string.Empty;
    }

    /// <summary>
    /// Reads the private key file and returns the RSA private key
    /// </summary>
    public string GetRSAPrivateKey()
    {
        if (File.Exists(PrivateKeyFileName))
        {
            string privatekey = File.ReadAllText(PrivateKeyFileName).Trim();

            if (privatekey.StartsWith(RSAKeys.PEM_PRIV_HEADER) && privatekey.EndsWith(RSAKeys.PEM_PRIV_FOOTER))
            {
                var rsa = new RSAKeys();
                rsa.DecodePEMKey(privatekey);
                return rsa.PrivateKey;
            }
            return privatekey;
        }

        return string.Empty;
    }

    void ValidateSaftHash(AuditFile auditFile)
    {
        if (auditFile?.SourceDocuments?.SalesInvoices?.Invoice == null)
            return;

        using var rsaCryptokey = RSA.Create();
        try
        {
            rsaCryptokey.FromXmlString(GetRSAPublicKey());
        }
        catch (Exception ex)
        {
            AddError(new ValidationError { Description = $"Não foi possível ler o ficheiro com a chave pública. {ex.Message}", Field = "Hash", TypeofError = typeof(HashResults) });
            return;
        }

        ValidateSeriesHashes(
            auditFile.SourceDocuments.SalesInvoices.Invoice,
            i => i.InvoiceNo,
            i => i.InvoiceType.ToString(),
            i => i.DocumentStatus?.SourceBilling ?? SAFTPTSourceBilling.P,
            i => i.Hash,
            i => i.InvoiceDate,
            i => i.SystemEntryDate,
            i => i.DocumentTotals?.GrossTotal ?? 0m,
            i => i.Pk,
            typeof(SourceDocumentsSalesInvoicesInvoice),
            rsaCryptokey,
            new StringBuilder(),
            countStats: false);
    }

    void ValidateSaftHashWD(AuditFile auditFile)
    {
        if (auditFile?.SourceDocuments?.WorkingDocuments?.WorkDocument == null)
            return;

        using var rsaCryptokey = RSA.Create();
        try
        {
            rsaCryptokey.FromXmlString(GetRSAPublicKey());
        }
        catch (Exception ex)
        {
            AddError(new ValidationError { Description = $"Não foi possível ler o ficheiro com a chave pública. {ex.Message}", Field = "Hash", TypeofError = typeof(HashResults) });
            return;
        }

        ValidateSeriesHashes(
            auditFile.SourceDocuments.WorkingDocuments.WorkDocument,
            d => d.DocumentNumber,
            d => d.WorkType.ToString(),
            d => d.DocumentStatus?.SourceBilling ?? SAFTPTSourceBilling.P,
            d => d.Hash,
            d => d.WorkDate,
            d => d.SystemEntryDate,
            d => d.DocumentTotals?.GrossTotal ?? 0m,
            d => d.Pk,
            typeof(SourceDocumentsWorkingDocumentsWorkDocument),
            rsaCryptokey,
            new StringBuilder(),
            countStats: false);
    }

    void ValidateSaftHashMG(AuditFile auditFile)
    {
        if (auditFile?.SourceDocuments?.MovementOfGoods?.StockMovement == null)
            return;

        using var rsaCryptokey = RSA.Create();
        try
        {
            rsaCryptokey.FromXmlString(GetRSAPublicKey());
        }
        catch (Exception ex)
        {
            AddError(new ValidationError { Description = $"Não foi possível ler o ficheiro com a chave pública. {ex.Message}", Field = "Hash", TypeofError = typeof(HashResults) });
            return;
        }

        ValidateSeriesHashes(
            auditFile.SourceDocuments.MovementOfGoods.StockMovement,
            m => m.DocumentNumber,
            m => m.MovementType.ToString(),
            m => m.DocumentStatus?.SourceBilling ?? SAFTPTSourceBilling.P,
            m => m.Hash,
            m => m.MovementDate,
            m => m.SystemEntryDate,
            m => m.DocumentTotals?.GrossTotal ?? 0m,
            m => m.Pk,
            typeof(SourceDocumentsMovementOfGoodsStockMovement),
            rsaCryptokey,
            new StringBuilder(),
            countStats: false);
    }

    internal void ValidaEstruturaXSD(string fileversion)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileversion))
                fileversion = SaftFileVersion.V10401;

            string schemaName = fileversion switch
            {
                SaftFileVersion.V10401 => "SAFTPT1.04_01.xsd",
                SaftFileVersion.V10301 => "SAFTPT1.03_01.xsd",
                SaftFileVersion.V10201 => "SAFTPT1.02_01.xsd",
                SaftFileVersion.V10101 => "SAFTPT1.01_01.xsd",
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(schemaName))
            {
                AddError(new ValidationError
                {
                    Description = $"Versão de esquema XSD desconhecida: {fileversion}",
                    Field = "Schema",
                    Value = fileversion,
                    TypeofError = typeof(SchemaResults),
                    ValidationType = ValidationErrorType.SCHEMA
                });
                return;
            }

            string schemaFilename = ResolveSchemaPath("Schemas", schemaName);
            if (string.IsNullOrEmpty(schemaFilename))
            {
                AddError(new ValidationError
                {
                    Description = $"Esquema XSD não encontrado: {schemaName}",
                    Field = "Schema",
                    TypeofError = typeof(SchemaResults),
                    ValidationType = ValidationErrorType.SCHEMA
                });
                return;
            }

            ValidateXmlAgainstSchema(SaftFileName, schemaFilename);
        }
        catch (Exception error)
        {
            AddError(new ValidationError
            {
                Description = $"Mensagem de erro: {error.Message}",
                Field = "Schema",
                TypeofError = typeof(SchemaResults),
                ValidationType = ValidationErrorType.SCHEMA
            });
        }
    }

    private static string ResolveSchemaPath(string folder, string schemaName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, folder, schemaName);
        if (File.Exists(path)) return path;

        path = Path.Combine(Environment.CurrentDirectory, folder, schemaName);
        if (File.Exists(path)) return path;

        return null;
    }

    private void ValidateXmlAgainstSchema(string xmlFilename, string schemaFilename)
    {
        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings
        };
        settings.Schemas.Add(null, schemaFilename);
        settings.ValidationEventHandler += ValidationEventHandler;

        using var reader = XmlReader.Create(xmlFilename, settings);
        while (reader.Read()) { }
    }

    void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        string pos = e.Exception != null ? $" (Linha {e.Exception.LineNumber}, Coluna {e.Exception.LinePosition})" : string.Empty;
        AddError(new ValidationError
        {
            Description = $"{e.Message}{pos}",
            Field = "Schema",
            TypeofError = typeof(SchemaResults),
            ValidationType = e.Severity == XmlSeverityType.Warning ? ValidationErrorType.WARNING : ValidationErrorType.SCHEMA
        });
    }

    internal void ValidateHeader(Header header)
    {
        if (header == null) return;

        AddError(header.ValidateTaxRegistrationNumber());
        AddError(header.ValidateAuditFileVersion());
        AddError(header.ValidateBusinessName());
        AddError(header.ValidateEmail());
        AddError(header.ValidateAddressDetail());
        AddError(header.ValidateBuildingNumber());
        AddError(header.ValidateCity());
        AddError(header.ValidateCountry());
        AddError(header.ValidatePostalCode());
        AddError(header.ValidateRegion());
        AddError(header.ValidateStreetName());
        AddError(header.ValidateCompanyID());
        AddError(header.ValidateCompanyName());
        AddError(header.ValidateCurrencyCode());
        AddError(header.ValidateDateCreated());
        AddError(header.ValidateStartDate());
        AddError(header.ValidateEndDate());
        AddError(header.ValidateFax());
        AddError(header.ValidateFiscalYear());
        AddError(header.ValidateHeaderComment());
        AddError(header.ValidateProductCompanyTaxID());
        AddError(header.ValidateProductID());
        AddError(header.ValidateProductVersion());
        AddError(header.ValidateSoftwareCertificateNumber());
        AddError(header.ValidateTaxAccountingBasis());
        AddError(header.ValidateTaxEntity());
        AddError(header.ValidateTelephone());
        AddError(header.ValidateWebsite());
    }

    internal void ValidateCustomers(Customer[] customers)
    {
        if (customers is null || customers.Length == 0) return;

        var duplicated = from p in customers
                         where !string.IsNullOrEmpty(p.CustomerID)
                         group p by p.CustomerID into ci
                         where ci.Count() > 1
                         select new { codigo = ci.Key, quantidade = ci.Count() };

        foreach (var d in duplicated)
        {
            string pk = customers.FirstOrDefault(p => string.Equals(p.CustomerID, d.codigo, StringComparison.OrdinalIgnoreCase))?.Pk;
            AddError(new ValidationError
            {
                Value = d.codigo,
                FileID = d.codigo,
                Field = "Customer/CustomerID",
                Description = $"O código {d.codigo} está repetido {d.quantidade} vezes.",
                TypeofError = typeof(Customer),
                UID = pk
            });
        }

        foreach (var customer in customers)
        {
            AddError(customer.ValidateAccountID());
            AddError(customer.ValidateCompanyName());
            AddError(customer.ValidateContact());
            AddError(customer.ValidateCustomerID());
            AddError(customer.ValidateCustomerTaxID());
            AddError(customer.ValidateEmail());
            AddError(customer.ValidateFax());
            AddError(customer.ValidateSelfBillingIndicator());
            AddError(customer.ValidateTelephone());
            AddError(customer.ValidateWebsite());
            AddError(customer.ValidateBillingAddress());
            AddError(customer.ValidateShipToAddress());

            if (customer.CustomerTaxID == "999999990" && !string.Equals(customer.CompanyName, "Consumidor final", StringComparison.OrdinalIgnoreCase))
            {
                AddError(new ValidationError
                {
                    Description = "Cliente com NIF genérico 999999990 deve ter CompanyName igual a «Consumidor final».",
                    Field = "Customer/CompanyName",
                    FileID = customer.CustomerID,
                    Value = customer.CompanyName,
                    TypeofError = typeof(Customer),
                    ValidationType = ValidationErrorType.WARNING,
                    UID = customer.Pk
                });
            }
        }
    }

    internal void ValidateProducts(Product[] products)
    {
        if (products is null || products.Length == 0) return;

        var duplicated = from p in products
                         where !string.IsNullOrEmpty(p.ProductCode)
                         group p by p.ProductCode into pc
                         where pc.Count() > 1
                         select new { codigo = pc.Key, quantidade = pc.Count() };

        foreach (var d in duplicated)
        {
            string pk = products.FirstOrDefault(p => string.Equals(p.ProductCode, d.codigo, StringComparison.OrdinalIgnoreCase))?.Pk;

            AddError(new ValidationError
            {
                Value = d.codigo,
                FileID = d.codigo,
                Field = "Product/ProductCode",
                Description = $"O código {d.codigo} está repetido {d.quantidade} vezes.",
                TypeofError = typeof(Product),
                UID = pk
            });
        }

        foreach (var product in products)
        {
            AddError(product.ValidateProductCode());
            AddError(product.ValidateProductDescription());
            AddError(product.ValidateProductGroup());
            AddError(product.ValidateProductNumberCode());
        }
    }

    internal void ValidateTax(TaxTableEntry[] taxs)
    {
        if (taxs is null || taxs.Length == 0) return;

        var seen = new HashSet<(TaxType, string, string, decimal?, decimal?)>();

        foreach (var tax in taxs)
        {
            AddError(tax.ValidateTaxType());
            AddError(tax.ValidateTaxCode());
            AddError(tax.ValidateTaxCountryRegion());
            AddError(tax.ValidateDescription());
            AddError(tax.ValidateTaxValue());

            var key = (tax.TaxType, tax.TaxCountryRegion ?? string.Empty, tax.TaxCode ?? string.Empty, tax.TaxPercentage, tax.TaxAmount);
            if (!seen.Add(key))
            {
                AddError(new ValidationError
                {
                    Description = $"Entrada duplicada na tabela de impostos ({tax.TaxType}/{tax.TaxCountryRegion}/{tax.TaxCode}).",
                    Field = "TaxTableEntry",
                    FileID = tax.TaxCode,
                    Value = tax.TaxCode,
                    TypeofError = typeof(TaxTableEntry),
                    ValidationType = ValidationErrorType.WARNING,
                    UID = tax.Pk
                });
            }
        }
    }

    internal void ValidateSupplier(Supplier[] suppliers)
    {
        if (suppliers is null || suppliers.Length == 0) return;

        var duplicated = from p in suppliers
                         where !string.IsNullOrEmpty(p.SupplierID)
                         group p by p.SupplierID into ci
                         where ci.Count() > 1
                         select new { codigo = ci.Key, quantidade = ci.Count() };

        foreach (var d in duplicated)
        {
            string pk = suppliers.FirstOrDefault(p => string.Equals(p.SupplierID, d.codigo, StringComparison.OrdinalIgnoreCase))?.Pk;
            AddError(new ValidationError
            {
                Value = d.codigo,
                FileID = d.codigo,
                Field = "Supplier/SupplierID",
                Description = $"O código {d.codigo} está repetido {d.quantidade} vezes.",
                TypeofError = typeof(Supplier),
                UID = pk
            });
        }

        foreach (var supplier in suppliers)
        {
            AddError(supplier.ValidateAccountID());
            AddError(supplier.ValidateCompanyName());
            AddError(supplier.ValidateContact());
            AddError(supplier.ValidateCustomerID());
            AddError(supplier.ValidateSupplierTaxID());
            AddError(supplier.ValidateEmail());
            AddError(supplier.ValidateFax());
            AddError(supplier.ValidateSelfBillingIndicator());
            AddError(supplier.ValidateTelephone());
            AddError(supplier.ValidateWebsite());
            AddError(supplier.ValidateBillingAddress());
            AddError(supplier.ValidateShipFromAddress());
        }
    }

    internal void ValidateGeneralLedgerAccounts(GeneralLedgerAccounts glAccounts)
    {
        if (glAccounts == null) return;

        if (glAccounts.TaxonomyReference == TaxonomyReference.INVALIDO)
        {
            AddError(new ValidationError
            {
                Description = "Referência da taxonomia inválida no plano de contas.",
                Field = "GeneralLedgerAccounts/TaxonomyReference",
                Value = glAccounts.TaxonomyReference.ToString(),
                TypeofError = typeof(GeneralLedgerAccounts),
                UID = glAccounts.Pk
            });
        }

        var accounts = glAccounts.Account ?? [];
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var acc in accounts)
        {
            if (string.IsNullOrEmpty(acc.AccountID) || acc.AccountID.Length < 2 || acc.AccountID.Length > 30)
            {
                AddError(new ValidationError
                {
                    Description = $"Código de conta contabilística inválido ({acc.AccountID}).",
                    Field = "GeneralLedgerAccounts/Account/AccountID",
                    FileID = acc.AccountID,
                    Value = acc.AccountID,
                    TypeofError = typeof(GeneralLedgerAccountsAccount),
                    UID = acc.Pk
                });
            }
            else if (!seenIds.Add(acc.AccountID))
            {
                AddError(new ValidationError
                {
                    Description = $"Conta contabilística duplicada: {acc.AccountID}.",
                    Field = "GeneralLedgerAccounts/Account/AccountID",
                    FileID = acc.AccountID,
                    Value = acc.AccountID,
                    TypeofError = typeof(GeneralLedgerAccountsAccount),
                    UID = acc.Pk
                });
            }

            if (string.IsNullOrEmpty(acc.AccountDescription) || acc.AccountDescription.Length > 100)
            {
                AddError(new ValidationError
                {
                    Description = $"Descrição da conta {acc.AccountID} inválida.",
                    Field = "GeneralLedgerAccounts/Account/AccountDescription",
                    FileID = acc.AccountID,
                    Value = acc.AccountDescription,
                    TypeofError = typeof(GeneralLedgerAccountsAccount),
                    UID = acc.Pk
                });
            }

            if (acc.GroupingCategory == GroupingCategory.INVALIDO)
            {
                AddError(new ValidationError
                {
                    Description = $"Categoria de agrupamento inválida na conta {acc.AccountID}.",
                    Field = "GeneralLedgerAccounts/Account/GroupingCategory",
                    FileID = acc.AccountID,
                    Value = acc.GroupingCategory.ToString(),
                    TypeofError = typeof(GeneralLedgerAccountsAccount),
                    UID = acc.Pk
                });
            }

            if (acc.OpeningDebitBalance < 0 || acc.OpeningCreditBalance < 0 || acc.ClosingDebitBalance < 0 || acc.ClosingCreditBalance < 0)
            {
                AddError(new ValidationError
                {
                    Description = $"Saldos negativos não permitidos na conta {acc.AccountID}.",
                    Field = "GeneralLedgerAccounts/Account",
                    FileID = acc.AccountID,
                    TypeofError = typeof(GeneralLedgerAccountsAccount),
                    UID = acc.Pk
                });
            }

            if (!string.IsNullOrEmpty(acc.TaxonomyCode) && (!int.TryParse(acc.TaxonomyCode, out int taxCode) || taxCode < 0 || taxCode > 999))
            {
                AddError(new ValidationError
                {
                    Description = $"Código de taxonomia inválido na conta {acc.AccountID}: {acc.TaxonomyCode}.",
                    Field = "GeneralLedgerAccounts/Account/TaxonomyCode",
                    FileID = acc.AccountID,
                    Value = acc.TaxonomyCode,
                    TypeofError = typeof(GeneralLedgerAccountsAccount),
                    UID = acc.Pk
                });
            }
        }

        // Equilíbrio de saldos nas contas de movimento (GM), ou de 1.º grau (GR) se não houver GM
        var balanceAccounts = accounts.Where(a => a.GroupingCategory == GroupingCategory.GM).ToArray();
        if (balanceAccounts.Length == 0)
            balanceAccounts = accounts.Where(a => a.GroupingCategory == GroupingCategory.GR).ToArray();

        if (balanceAccounts.Length > 0)
        {
            decimal openDebit = balanceAccounts.Sum(a => a.OpeningDebitBalance);
            decimal openCredit = balanceAccounts.Sum(a => a.OpeningCreditBalance);
            if (Math.Abs(openDebit - openCredit) > 0.01m)
            {
                AddError(new ValidationError
                {
                    Description = $"Desequilíbrio nos saldos de abertura do plano de contas: Débito={openDebit:N2} <> Crédito={openCredit:N2}.",
                    Field = "GeneralLedgerAccounts/OpeningBalance",
                    Value = openDebit.ToString(CultureInfo.InvariantCulture),
                    TypeofError = typeof(GeneralLedgerAccounts),
                    UID = glAccounts.Pk
                });
            }

            decimal closeDebit = balanceAccounts.Sum(a => a.ClosingDebitBalance);
            decimal closeCredit = balanceAccounts.Sum(a => a.ClosingCreditBalance);
            if (Math.Abs(closeDebit - closeCredit) > 0.01m)
            {
                AddError(new ValidationError
                {
                    Description = $"Desequilíbrio nos saldos de fecho do plano de contas: Débito={closeDebit:N2} <> Crédito={closeCredit:N2}.",
                    Field = "GeneralLedgerAccounts/ClosingBalance",
                    Value = closeDebit.ToString(CultureInfo.InvariantCulture),
                    TypeofError = typeof(GeneralLedgerAccounts),
                    UID = glAccounts.Pk
                });
            }
        }
    }

    internal void ValidateGeneralLedgerEntries(GeneralLedgerEntries gl, GeneralLedgerAccounts glAccounts = null)
    {
        if (gl == null) return;

        var journals = gl.Journal ?? [];
        int totalTransactions = journals.Sum(j => j.Transaction?.Length ?? 0);
        if (!int.TryParse(gl.NumberOfEntries, out int declaredEntries) || declaredEntries != totalTransactions)
        {
            AddError(new ValidationError
            {
                Value = gl.NumberOfEntries,
                Field = "GeneralLedgerEntries/NumberOfEntries",
                TypeofError = typeof(GeneralLedgerEntries),
                Description = $"Nº de registos contabilísticos incorrecto. valor: {gl.NumberOfEntries}, esperado: {totalTransactions}",
                UID = gl.Pk
            });
        }

        var validAccountIds = glAccounts?.Account != null
            ? new HashSet<string>(glAccounts.Account.Where(a => !string.IsNullOrEmpty(a.AccountID)).Select(a => a.AccountID), StringComparer.OrdinalIgnoreCase)
            : null;

        var seenJournalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenTransactionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        decimal calcTotalDebit = 0m;
        decimal calcTotalCredit = 0m;

        foreach (var journal in journals)
        {
            if (string.IsNullOrEmpty(journal.JournalID) || journal.JournalID.Length > 30 || journal.JournalID.Contains(' '))
            {
                AddError(new ValidationError
                {
                    Description = $"Identificador de diário inválido ({journal.JournalID}).",
                    Field = "GeneralLedgerEntries/Journal/JournalID",
                    FileID = journal.JournalID,
                    Value = journal.JournalID,
                    TypeofError = typeof(GeneralLedgerEntriesJournal),
                    UID = journal.Pk
                });
            }
            else if (!seenJournalIds.Add(journal.JournalID))
            {
                AddError(new ValidationError
                {
                    Description = $"Diário duplicado: {journal.JournalID}.",
                    Field = "GeneralLedgerEntries/Journal/JournalID",
                    FileID = journal.JournalID,
                    Value = journal.JournalID,
                    TypeofError = typeof(GeneralLedgerEntriesJournal),
                    UID = journal.Pk
                });
            }

            if (string.IsNullOrEmpty(journal.Description) || journal.Description.Length > 200)
            {
                AddError(new ValidationError
                {
                    Description = $"Descrição do diário {journal.JournalID} inválida.",
                    Field = "GeneralLedgerEntries/Journal/Description",
                    FileID = journal.JournalID,
                    Value = journal.Description,
                    TypeofError = typeof(GeneralLedgerEntriesJournal),
                    UID = journal.Pk
                });
            }

            foreach (var tx in journal.Transaction ?? [])
            {
                if (string.IsNullOrEmpty(tx.TransactionID) || tx.TransactionID.Length > 70)
                {
                    AddError(new ValidationError
                    {
                        Description = $"TransactionID inválido no diário {journal.JournalID}.",
                        Field = "GeneralLedgerEntries/Journal/Transaction/TransactionID",
                        FileID = tx.TransactionID,
                        Value = tx.TransactionID,
                        TypeofError = typeof(GeneralLedgerEntriesJournalTransaction),
                        UID = tx.Pk
                    });
                }
                else if (!seenTransactionIds.Add(tx.TransactionID))
                {
                    AddError(new ValidationError
                    {
                        Description = $"TransactionID duplicado: {tx.TransactionID}.",
                        Field = "GeneralLedgerEntries/Journal/Transaction/TransactionID",
                        FileID = tx.TransactionID,
                        Value = tx.TransactionID,
                        TypeofError = typeof(GeneralLedgerEntriesJournalTransaction),
                        UID = tx.Pk
                    });
                }

                if (string.IsNullOrEmpty(tx.Period) || !int.TryParse(tx.Period, out int period) || period < 1 || period > 16)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Período contabilístico inválido na transacção {tx.TransactionID}.",
                        Field = "GeneralLedgerEntries/Journal/Transaction/Period",
                        FileID = tx.TransactionID,
                        Value = tx.Period,
                        TypeofError = typeof(GeneralLedgerEntriesJournalTransaction),
                        UID = tx.Pk
                    });
                }

                if (tx.TransactionDate == DateTime.MinValue)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Data de transacção inválida em {tx.TransactionID}.",
                        Field = "GeneralLedgerEntries/Journal/Transaction/TransactionDate",
                        FileID = tx.TransactionID,
                        Value = tx.TransactionDate.ToString(),
                        TypeofError = typeof(GeneralLedgerEntriesJournalTransaction),
                        UID = tx.Pk
                    });
                }

                if (tx.GLPostingDate == DateTime.MinValue)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Data de movimento contabilístico (GLPostingDate) inválida em {tx.TransactionID}.",
                        Field = "GeneralLedgerEntries/Journal/Transaction/GLPostingDate",
                        FileID = tx.TransactionID,
                        Value = tx.GLPostingDate.ToString(),
                        TypeofError = typeof(GeneralLedgerEntriesJournalTransaction),
                        UID = tx.Pk
                    });
                }

                if (tx.TransactionType == TransactionType.INVALIDO)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Tipo de movimento contabilístico inválido em {tx.TransactionID}.",
                        Field = "GeneralLedgerEntries/Journal/Transaction/TransactionType",
                        FileID = tx.TransactionID,
                        Value = tx.TransactionType.ToString(),
                        TypeofError = typeof(GeneralLedgerEntriesJournalTransaction),
                        UID = tx.Pk
                    });
                }

                var debitLines = tx.Lines?.DebitLines is { Length: > 0 } dl
                    ? dl
                    : (tx.Lines?.DebitLine != null ? [tx.Lines.DebitLine] : []);
                var creditLines = tx.Lines?.CreditLines is { Length: > 0 } cl
                    ? cl
                    : (tx.Lines?.CreditLine != null ? [tx.Lines.CreditLine] : []);

                decimal txDebit = 0m;
                foreach (var d in debitLines)
                {
                    if (d.DebitAmount < 0)
                    {
                        AddError(new ValidationError
                        {
                            Description = $"Montante a débito negativo na transacção {tx.TransactionID}.",
                            Field = "GeneralLedgerEntries/DebitLine/DebitAmount",
                            FileID = tx.TransactionID,
                            Value = d.DebitAmount.ToString(CultureInfo.InvariantCulture),
                            TypeofError = typeof(GeneralLedgerEntriesJournalTransactionLinesDebitLine),
                            UID = d.Pk,
                            SupUID = tx.Pk
                        });
                    }
                    if (validAccountIds != null && (string.IsNullOrEmpty(d.AccountID) || !validAccountIds.Contains(d.AccountID)))
                    {
                        AddError(new ValidationError
                        {
                            Description = $"Conta {d.AccountID} na linha de débito da transacção {tx.TransactionID} não existe no Plano de Contas.",
                            Field = "GeneralLedgerEntries/DebitLine/AccountID",
                            FileID = tx.TransactionID,
                            Value = d.AccountID,
                            TypeofError = typeof(GeneralLedgerEntriesJournalTransactionLinesDebitLine),
                            UID = d.Pk,
                            SupUID = tx.Pk
                        });
                    }
                    txDebit += d.DebitAmount;
                }

                decimal txCredit = 0m;
                foreach (var c in creditLines)
                {
                    if (c.CreditAmount < 0)
                    {
                        AddError(new ValidationError
                        {
                            Description = $"Montante a crédito negativo na transacção {tx.TransactionID}.",
                            Field = "GeneralLedgerEntries/CreditLine/CreditAmount",
                            FileID = tx.TransactionID,
                            Value = c.CreditAmount.ToString(CultureInfo.InvariantCulture),
                            TypeofError = typeof(GeneralLedgerEntriesJournalTransactionLinesCreditLine),
                            UID = c.Pk,
                            SupUID = tx.Pk
                        });
                    }
                    if (validAccountIds != null && (string.IsNullOrEmpty(c.AccountID) || !validAccountIds.Contains(c.AccountID)))
                    {
                        AddError(new ValidationError
                        {
                            Description = $"Conta {c.AccountID} na linha de crédito da transacção {tx.TransactionID} não existe no Plano de Contas.",
                            Field = "GeneralLedgerEntries/CreditLine/AccountID",
                            FileID = tx.TransactionID,
                            Value = c.AccountID,
                            TypeofError = typeof(GeneralLedgerEntriesJournalTransactionLinesCreditLine),
                            UID = c.Pk,
                            SupUID = tx.Pk
                        });
                    }
                    txCredit += c.CreditAmount;
                }

                if (Math.Abs(txDebit - txCredit) > 0.01m)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Transacção contabilística {tx.TransactionID} desequilibrada: Débito={txDebit:N2} <> Crédito={txCredit:N2}.",
                        Field = "GeneralLedgerEntries/Journal/Transaction",
                        FileID = tx.TransactionID,
                        Value = txDebit.ToString(CultureInfo.InvariantCulture),
                        TypeofError = typeof(GeneralLedgerEntriesJournalTransaction),
                        UID = tx.Pk
                    });
                }

                calcTotalDebit += txDebit;
                calcTotalCredit += txCredit;
            }
        }

        if (gl.TotalDebit != calcTotalDebit)
        {
            AddError(new ValidationError
            {
                Description = $"Total de débitos contabilísticos incorrecto. valor: {gl.TotalDebit}, esperado: {calcTotalDebit}",
                Field = "GeneralLedgerEntries/TotalDebit",
                Value = gl.TotalDebit.ToString(CultureInfo.InvariantCulture),
                TypeofError = typeof(GeneralLedgerEntries),
                UID = gl.Pk
            });
        }

        if (gl.TotalCredit != calcTotalCredit)
        {
            AddError(new ValidationError
            {
                Description = $"Total de créditos contabilísticos incorrecto. valor: {gl.TotalCredit}, esperado: {calcTotalCredit}",
                Field = "GeneralLedgerEntries/TotalCredit",
                Value = gl.TotalCredit.ToString(CultureInfo.InvariantCulture),
                TypeofError = typeof(GeneralLedgerEntries),
                UID = gl.Pk
            });
        }

        if (Math.Abs(calcTotalDebit - calcTotalCredit) > 0.01m)
        {
            AddError(new ValidationError
            {
                Description = $"Desequilíbrio global nos movimentos contabilísticos: TotalDebit={calcTotalDebit:N2} <> TotalCredit={calcTotalCredit:N2}.",
                Field = "GeneralLedgerEntries",
                Value = calcTotalDebit.ToString(CultureInfo.InvariantCulture),
                TypeofError = typeof(GeneralLedgerEntries),
                UID = gl.Pk
            });
        }
    }

    internal void ValidateWorkDocument(SourceDocumentsWorkingDocuments workDocuments, Header header = null, AuditFileMasterFiles masterFiles = null)
    {
        if (workDocuments == null) return;

        var docs = workDocuments.WorkDocument ?? [];
        int numberOfLines = docs.Length;
        if (!int.TryParse(workDocuments.NumberOfEntries, out int declaredEntries) || declaredEntries != numberOfLines)
        {
            AddError(new ValidationError
            {
                Value = workDocuments.NumberOfEntries,
                Field = "WorkingDocuments/NumberOfEntries",
                TypeofError = typeof(SourceDocumentsWorkingDocuments),
                Description = $"Nº de registos incorrecto. Documento: {workDocuments.NumberOfEntries}, esperado: {numberOfLines}"
            });
        }

        var (customerIds, _, productCodes, taxKeys) = BuildMasterLookups(masterFiles);

        decimal headerTotalDebit = 0m;
        decimal headerTotalCredit = 0m;

        ValidateDocumentSequenceAndDates(
            docs,
            d => d.DocumentNumber,
            d => d.WorkDate,
            d => d.SystemEntryDate,
            d => d.Pk,
            "WorkDocument/DocumentNumber",
            typeof(SourceDocumentsWorkingDocumentsWorkDocument));

        foreach (var workDocument in docs)
        {
            AddError(workDocument.ValidateDocumentNumber());
            AddError(workDocument.ValidateATCUD());
            AddError(workDocument.ValidateHash());
            AddError(workDocument.ValidateHashControl());
            AddError(workDocument.ValidatePeriod());
            AddError(workDocument.ValidateSystemEntryDate());
            AddError(workDocument.ValidateWorkDate());
            AddError(workDocument.ValidateCustomerID());
            AddError(workDocument.ValidateSourceID());
            AddError(workDocument.ValidateTransactionID());
            AddError(workDocument.ValidateEACCode());
            AddError(workDocument.ValidateDocumentStatus());
            AddError(workDocument.ValidateDocumentTotals());

            ValidateDocumentTemporalConsistency(
                header,
                workDocument.DocumentNumber,
                workDocument.WorkDate,
                workDocument.SystemEntryDate,
                workDocument.DocumentStatus?.WorkStatusDate ?? DateTime.MinValue,
                workDocument.Period,
                "WorkDocument",
                typeof(SourceDocumentsWorkingDocumentsWorkDocument),
                workDocument.Pk);

            if (customerIds != null && !string.IsNullOrEmpty(workDocument.CustomerID) && !customerIds.Contains(workDocument.CustomerID))
            {
                AddError(new ValidationError
                {
                    Description = $"Cliente {workDocument.CustomerID} referenciado no documento {workDocument.DocumentNumber} não existe na tabela de clientes.",
                    Field = "WorkDocument/CustomerID",
                    FileID = workDocument.DocumentNumber,
                    Value = workDocument.CustomerID,
                    TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument),
                    UID = workDocument.Pk
                });
            }

            var lines = workDocument.Line ?? [];
            bool isExcludedFromTotals = workDocument.DocumentStatus?.WorkStatus is WorkStatus.A or WorkStatus.F;

            decimal netTotal = 0m;
            decimal grossTotal = 0m;
            decimal taxPayable = 0m;
            int numLinha = 1;

            foreach (var line in lines)
            {
                int num = -1;
                if (!string.IsNullOrEmpty(line.LineNumber))
                    int.TryParse(line.LineNumber, out num);

                if (numLinha != num)
                {
                    AddError(new ValidationError
                    {
                        Value = line.LineNumber,
                        FileID = workDocument.DocumentNumber,
                        Field = "WorkDocument/Line/LineNumber",
                        TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocumentLine),
                        Description = $"Número de linha incorrecto, Documento: {workDocument.DocumentNumber}, esperado: {numLinha}, valor: {line.LineNumber}",
                        UID = line.Pk,
                        SupUID = workDocument.Pk
                    });
                }
                numLinha++;

                ValidateWorkDocumentLine(line, workDocument, productCodes, taxKeys);

                decimal creditAmt = GetWorkLineCreditAmount(line);
                decimal debitAmt = GetWorkLineDebitAmount(line);
                if (!isExcludedFromTotals)
                {
                    headerTotalCredit += creditAmt;
                    headerTotalDebit += debitAmt;
                }

                decimal lineAmount = creditAmt > 0 ? creditAmt : debitAmt;
                int op = Operation(workDocument, line);

                decimal lineTaxAmount = 0m;
                if (line.Tax != null)
                {
                    if (line.Tax.ItemElementName == ItemChoiceType1.TaxPercentage)
                        lineTaxAmount = lineAmount * line.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m;
                    else if (line.Tax.ItemElementName == ItemChoiceType1.TaxAmount)
                        lineTaxAmount = line.Tax.TaxAmount.GetValueOrDefault(0);
                }

                netTotal += lineAmount * op;
                taxPayable += lineTaxAmount * op;
                grossTotal += (lineAmount + lineTaxAmount) * op;
            }

            netTotal = Math.Round(netTotal, 2, MidpointRounding.AwayFromZero);
            taxPayable = Math.Round(taxPayable, 2, MidpointRounding.AwayFromZero);
            grossTotal = Math.Round(grossTotal, 2, MidpointRounding.AwayFromZero);

            if (workDocument.DocumentTotals != null)
            {
                if (Math.Abs(netTotal - workDocument.DocumentTotals.NetTotal) > 0.01m)
                    AddError(new ValidationError { Value = workDocument.DocumentTotals.NetTotal.ToString(), FileID = workDocument.DocumentNumber, Field = "WorkDocument/DocumentTotals/NetTotal", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = $"Documento: {workDocument.DocumentNumber}, NetTotal: {workDocument.DocumentTotals.NetTotal} esperado: {netTotal}", UID = workDocument.Pk });
                if (Math.Abs(grossTotal - workDocument.DocumentTotals.GrossTotal) > 0.01m)
                    AddError(new ValidationError { Value = workDocument.DocumentTotals.GrossTotal.ToString(), FileID = workDocument.DocumentNumber, Field = "WorkDocument/DocumentTotals/GrossTotal", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = $"Documento: {workDocument.DocumentNumber}, GrossTotal: {workDocument.DocumentTotals.GrossTotal} esperado: {grossTotal}", UID = workDocument.Pk });
                if (Math.Abs(taxPayable - workDocument.DocumentTotals.TaxPayable) > 0.01m)
                    AddError(new ValidationError { Value = workDocument.DocumentTotals.TaxPayable.ToString(), FileID = workDocument.DocumentNumber, Field = "WorkDocument/DocumentTotals/TaxPayable", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = $"Documento: {workDocument.DocumentNumber}, TaxPayable: {workDocument.DocumentTotals.TaxPayable} esperado: {taxPayable}", UID = workDocument.Pk });
                if (Math.Abs(workDocument.DocumentTotals.GrossTotal - (workDocument.DocumentTotals.NetTotal + workDocument.DocumentTotals.TaxPayable)) > 0.01m)
                    AddError(new ValidationError { Value = workDocument.DocumentTotals.GrossTotal.ToString(), FileID = workDocument.DocumentNumber, Field = "WorkDocument/DocumentTotals", TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocument), Description = $"Documento: {workDocument.DocumentNumber}, NetTotal + TaxPayable = {workDocument.DocumentTotals.NetTotal + workDocument.DocumentTotals.TaxPayable} != GrossTotal {workDocument.DocumentTotals.GrossTotal}", UID = workDocument.Pk });
            }
        }

        if (workDocuments.TotalDebit != headerTotalDebit)
        {
            AddError(new ValidationError
            {
                Value = workDocuments.TotalDebit.ToString(),
                Field = "WorkingDocuments/TotalDebit",
                TypeofError = typeof(SourceDocumentsWorkingDocuments),
                Description = $"Total de débitos dos documentos de conferência incorrecto. valor: {workDocuments.TotalDebit}, esperado: {headerTotalDebit}"
            });
        }

        if (workDocuments.TotalCredit != headerTotalCredit)
        {
            AddError(new ValidationError
            {
                Value = workDocuments.TotalCredit.ToString(),
                Field = "WorkingDocuments/TotalCredit",
                TypeofError = typeof(SourceDocumentsWorkingDocuments),
                Description = $"Total de créditos dos documentos de conferência incorrecto. valor: {workDocuments.TotalCredit}, esperado: {headerTotalCredit}"
            });
        }
    }

    void ValidateWorkDocumentLine(
        SourceDocumentsWorkingDocumentsWorkDocumentLine line,
        SourceDocumentsWorkingDocumentsWorkDocument workDocument,
        HashSet<string> productCodes,
        HashSet<(TaxType, string, string)> taxKeys)
    {
        AddError(line.ValidateLineNumber(workDocument.Pk));
        AddError(line.ValidateProductCode(workDocument.Pk));
        AddError(line.ValidateProductDescription(workDocument.Pk));
        AddError(line.ValidateQuantity(workDocument.Pk));
        AddError(line.ValidateTaxPointDate(workDocument.Pk));
        AddError(line.ValidateUnitOfMeasure(workDocument.Pk));
        AddError(line.ValidateUnitPrice(workDocument.Pk));
        AddError(line.ValidateDescription(workDocument.Pk));
        AddError(line.ValidateSettlementAmount(workDocument.Pk));
        AddError(line.ValidateTaxBase(workDocument.Pk));
        AddError(line.ValidateOrderReferences(workDocument.Pk));
        AddError(line.ValidateTax(workDocument.Pk));
        AddError(line.ValidateTaxExemption(workDocument.Pk, workDocument.WorkDate));

        if (productCodes != null && !string.IsNullOrEmpty(line.ProductCode) && !productCodes.Contains(line.ProductCode))
        {
            AddError(new ValidationError
            {
                Description = $"Produto {line.ProductCode} na linha nº {line.LineNumber} do documento {workDocument.DocumentNumber} não existe na tabela de produtos.",
                Field = "WorkDocument/Line/ProductCode",
                FileID = workDocument.DocumentNumber,
                Value = line.ProductCode,
                TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocumentLine),
                UID = line.Pk,
                SupUID = workDocument.Pk
            });
        }

        if (taxKeys != null && line.Tax != null && !taxKeys.Contains((line.Tax.TaxType, line.Tax.TaxCountryRegion ?? string.Empty, line.Tax.TaxCode ?? string.Empty)))
        {
            AddError(new ValidationError
            {
                Description = $"Taxa ({line.Tax.TaxType}/{line.Tax.TaxCountryRegion}/{line.Tax.TaxCode}) na linha nº {line.LineNumber} do documento {workDocument.DocumentNumber} não existe na tabela de impostos.",
                Field = "WorkDocument/Line/Tax",
                FileID = workDocument.DocumentNumber,
                Value = line.Tax.TaxCode,
                TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocumentLine),
                UID = line.Pk,
                SupUID = workDocument.Pk
            });
        }

        decimal lineAmount = line.CreditAmount ?? line.DebitAmount ?? line.Item;
        int numCasasDecimais = 6;
        bool incidencia = Math.Abs(Math.Round(line.UnitPrice * line.Quantity, numCasasDecimais, MidpointRounding.AwayFromZero) - Math.Round(lineAmount, numCasasDecimais, MidpointRounding.AwayFromZero)) > 0.01m;

        if (incidencia)
        {
            AddError(new ValidationError
            {
                Value = lineAmount.ToString(),
                FileID = workDocument.DocumentNumber,
                Field = $"WorkDocument/Line/{line.ItemElementName}",
                TypeofError = typeof(SourceDocumentsWorkingDocumentsWorkDocumentLine),
                Description = $"Total de linha diferente do valor unitário * quantidade ({line.UnitPrice} * {line.Quantity} <> {lineAmount}). Documento: {line.DocNo}",
                UID = line.Pk,
                SupUID = workDocument.Pk
            });
        }
    }

    internal void ValidateMovementOfGoods(SourceDocumentsMovementOfGoods movements, Header header = null, AuditFileMasterFiles masterFiles = null)
    {
        if (movements == null) return;

        var docs = movements.StockMovement ?? [];
        int numberOfLines = docs.Sum(m => m.Line?.Length ?? 0);
        if (!int.TryParse(movements.NumberOfMovementLines, out int declaredLines) || declaredLines != numberOfLines)
        {
            AddError(new ValidationError
            {
                Value = movements.NumberOfMovementLines,
                Field = "MovementOfGoods/NumberOfMovementLines",
                TypeofError = typeof(SourceDocumentsMovementOfGoods),
                Description = $"Nº de registos incorrecto. valor: {movements.NumberOfMovementLines} esperado: {numberOfLines}"
            });
        }

        var (customerIds, supplierIds, productCodes, taxKeys) = BuildMasterLookups(masterFiles);

        ValidateDocumentSequenceAndDates(
            docs,
            d => d.DocumentNumber,
            d => d.MovementDate,
            d => d.SystemEntryDate,
            d => d.Pk,
            "StockMovement/DocumentNumber",
            typeof(SourceDocumentsMovementOfGoodsStockMovement));

        decimal quantity = 0m;
        foreach (var movement in docs)
        {
            AddError(movement.ValidateDocumentNumber());
            AddError(movement.ValidateATCUD());
            AddError(movement.ValidateHash());
            AddError(movement.ValidateHashControl());
            AddError(movement.ValidatePeriod());
            AddError(movement.ValidateSystemEntryDate());
            AddError(movement.ValidateMovementDate());
            AddError(movement.ValidateMovementEndTime());
            AddError(movement.ValidateMovementStartTime());
            AddError(movement.ValidateMovementTimeRange());
            AddError(movement.ValidateSourceID());
            AddError(movement.ValidateEACCode());
            AddError(movement.ValidateTransactionID());
            AddError(movement.ValidateDocumentStatus());
            AddError(movement.ValidateDocumentTotals());
            AddError(movement.ValidateShipFrom());
            AddError(movement.ValidateShipTo());

            ValidateDocumentTemporalConsistency(
                header,
                movement.DocumentNumber,
                movement.MovementDate,
                movement.SystemEntryDate,
                movement.DocumentStatus?.MovementStatusDate ?? DateTime.MinValue,
                movement.Period,
                "StockMovement",
                typeof(SourceDocumentsMovementOfGoodsStockMovement),
                movement.Pk);

            if (movement.ItemElementName == ItemChoiceType5.SupplierID)
            {
                if (supplierIds != null && !string.IsNullOrEmpty(movement.Item) && !supplierIds.Contains(movement.Item))
                {
                    AddError(new ValidationError
                    {
                        Description = $"Fornecedor {movement.Item} referenciado no documento {movement.DocumentNumber} não existe na tabela de fornecedores.",
                        Field = "StockMovement/SupplierID",
                        FileID = movement.DocumentNumber,
                        Value = movement.Item,
                        TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovement),
                        UID = movement.Pk
                    });
                }
            }
            else
            {
                string custId = !string.IsNullOrEmpty(movement.CustomerID) ? movement.CustomerID : movement.Item;
                if (customerIds != null && !string.IsNullOrEmpty(custId) && !customerIds.Contains(custId))
                {
                    AddError(new ValidationError
                    {
                        Description = $"Cliente {custId} referenciado no documento {movement.DocumentNumber} não existe na tabela de clientes.",
                        Field = "StockMovement/CustomerID",
                        FileID = movement.DocumentNumber,
                        Value = custId,
                        TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovement),
                        UID = movement.Pk
                    });
                }
            }

            var lines = movement.Line ?? [];
            bool isExcludedFromTotals = movement.DocumentStatus?.MovementStatus is MovementStatus.A or MovementStatus.F;

            if (!isExcludedFromTotals)
                quantity += lines.Sum(l => l.Quantity);

            if (movement.DocumentTotals != null && movement.DocumentTotals.GrossTotal > 0)
            {
                decimal netTotal = Math.Round(lines.Sum(l => l.CreditAmount ?? l.DebitAmount ?? l.Item), 2, MidpointRounding.AwayFromZero);
                decimal taxPayable = Math.Round(lines.Sum(l => (l.CreditAmount ?? l.DebitAmount ?? l.Item) * (l.Tax != null ? l.Tax.TaxPercentage : 0m) * 0.01m), 2, MidpointRounding.AwayFromZero);
                decimal grossTotal = Math.Round(netTotal + taxPayable, 2, MidpointRounding.AwayFromZero);

                if (Math.Abs(netTotal - movement.DocumentTotals.NetTotal) > 0.01m)
                    AddError(new ValidationError { Value = movement.DocumentTotals.NetTotal.ToString(), FileID = movement.DocumentNumber, Field = "StockMovement/DocumentTotals/NetTotal", TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovement), Description = $"Documento: {movement.DocumentNumber}, NetTotal: {movement.DocumentTotals.NetTotal} esperado: {netTotal}", UID = movement.Pk });
                if (Math.Abs(grossTotal - movement.DocumentTotals.GrossTotal) > 0.01m)
                    AddError(new ValidationError { Value = movement.DocumentTotals.GrossTotal.ToString(), FileID = movement.DocumentNumber, Field = "StockMovement/DocumentTotals/GrossTotal", TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovement), Description = $"Documento: {movement.DocumentNumber}, GrossTotal: {movement.DocumentTotals.GrossTotal} esperado: {grossTotal}", UID = movement.Pk });
                if (Math.Abs(taxPayable - movement.DocumentTotals.TaxPayable) > 0.01m)
                    AddError(new ValidationError { Value = movement.DocumentTotals.TaxPayable.ToString(), FileID = movement.DocumentNumber, Field = "StockMovement/DocumentTotals/TaxPayable", TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovement), Description = $"Documento: {movement.DocumentNumber}, TaxPayable: {movement.DocumentTotals.TaxPayable} esperado: {taxPayable}", UID = movement.Pk });
                if (Math.Abs(movement.DocumentTotals.GrossTotal - (movement.DocumentTotals.NetTotal + movement.DocumentTotals.TaxPayable)) > 0.01m)
                    AddError(new ValidationError { Value = movement.DocumentTotals.GrossTotal.ToString(), FileID = movement.DocumentNumber, Field = "StockMovement/DocumentTotals", TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovement), Description = $"Documento: {movement.DocumentNumber}, NetTotal + TaxPayable = {movement.DocumentTotals.NetTotal + movement.DocumentTotals.TaxPayable} != GrossTotal {movement.DocumentTotals.GrossTotal}", UID = movement.Pk });
            }

            int numLinha = 1;
            foreach (var line in lines)
            {
                int num = -1;
                if (!string.IsNullOrEmpty(line.LineNumber))
                    int.TryParse(line.LineNumber, out num);

                if (numLinha != num)
                {
                    AddError(new ValidationError
                    {
                        Value = line.LineNumber,
                        FileID = movement.DocumentNumber,
                        Field = "StockMovement/Line/LineNumber",
                        TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovementLine),
                        Description = $"Número de linha incorrecto, Documento: {movement.DocumentNumber}, esperado: {numLinha}, valor: {line.LineNumber}",
                        UID = line.Pk,
                        SupUID = movement.Pk
                    });
                }
                numLinha++;

                ValidateMovementOfGoodsStockMovementLine(line, movement, productCodes, taxKeys);
            }
        }

        if (quantity != movements.TotalQuantityIssued)
        {
            AddError(new ValidationError
            {
                Value = movements.TotalQuantityIssued.ToString(),
                Field = "MovementOfGoods/TotalQuantityIssued",
                TypeofError = typeof(SourceDocumentsMovementOfGoods),
                Description = $"Total da quantidade incorrecto. valor: {movements.TotalQuantityIssued}, esperado: {quantity}"
            });
        }
    }

    void ValidateMovementOfGoodsStockMovementLine(
        SourceDocumentsMovementOfGoodsStockMovementLine line,
        SourceDocumentsMovementOfGoodsStockMovement movement,
        HashSet<string> productCodes,
        HashSet<(TaxType, string, string)> taxKeys)
    {
        AddError(line.ValidateLineNumber(movement.Pk));
        AddError(line.ValidateProductCode(movement.Pk));
        AddError(line.ValidateProductDescription(movement.Pk));
        AddError(line.ValidateQuantity(movement.Pk));
        AddError(line.ValidateUnitOfMeasure(movement.Pk));
        AddError(line.ValidateUnitPrice(movement.Pk));
        AddError(line.ValidateDescription(movement.Pk));
        AddError(line.ValidateSettlementAmount(movement.Pk));
        AddError(line.ValidateOrderReferences(movement.Pk));
        AddError(line.ValidateTax(movement.Pk));
        AddError(line.ValidateTaxExemption(movement.Pk, movement.MovementDate));

        if (productCodes != null && !string.IsNullOrEmpty(line.ProductCode) && !productCodes.Contains(line.ProductCode))
        {
            AddError(new ValidationError
            {
                Description = $"Produto {line.ProductCode} na linha nº {line.LineNumber} do documento {movement.DocumentNumber} não existe na tabela de produtos.",
                Field = "StockMovement/Line/ProductCode",
                FileID = movement.DocumentNumber,
                Value = line.ProductCode,
                TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovementLine),
                UID = line.Pk,
                SupUID = movement.Pk
            });
        }

        if (taxKeys != null && line.Tax != null)
        {
            var mappedTaxType = line.Tax.TaxType switch
            {
                SAFTPTMovementTaxType.IVA => TaxType.IVA,
                SAFTPTMovementTaxType.NS => TaxType.NS,
                _ => TaxType.INVALIDO
            };
            if (!taxKeys.Contains((mappedTaxType, line.Tax.TaxCountryRegion ?? string.Empty, line.Tax.TaxCode ?? string.Empty)))
            {
                AddError(new ValidationError
                {
                    Description = $"Taxa ({line.Tax.TaxType}/{line.Tax.TaxCountryRegion}/{line.Tax.TaxCode}) na linha nº {line.LineNumber} do documento {movement.DocumentNumber} não existe na tabela de impostos.",
                    Field = "StockMovement/Line/Tax",
                    FileID = movement.DocumentNumber,
                    Value = line.Tax.TaxCode,
                    TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovementLine),
                    UID = line.Pk,
                    SupUID = movement.Pk
                });
            }
        }

        var value = line.UnitPrice * line.Quantity;
        var amount = line.CreditAmount ?? line.DebitAmount ?? line.Item;
        var difference = Math.Abs(value - amount);

        if (value != amount && difference > 0.01m)
        {
            AddError(new ValidationError
            {
                Value = amount.ToString(),
                FileID = movement.DocumentNumber,
                Field = $"StockMovement/Line/{line.ItemElementName}",
                TypeofError = typeof(SourceDocumentsMovementOfGoodsStockMovementLine),
                Description = $"Valor da linha incorrecto. Valor: {amount}, esperado: {value}, diferença: {difference}",
                UID = line.Pk,
                SupUID = movement.Pk
            });
        }
    }

    internal void ValidateInvoices(SourceDocumentsSalesInvoices invoices, Header header = null, AuditFileMasterFiles masterFiles = null)
    {
        if (invoices == null) return;

        var invoiceList = invoices.Invoice ?? [];
        if (!int.TryParse(invoices.NumberOfEntries, out int declaredEntries) || declaredEntries != invoiceList.Length)
        {
            AddError(new ValidationError
            {
                Value = invoices.NumberOfEntries,
                Field = "SalesInvoices/NumberOfEntries",
                TypeofError = typeof(SourceDocumentsSalesInvoices),
                Description = $"Nº de registos de documentos comerciais incorrecto. valor: {invoices.NumberOfEntries}, esperado: {invoiceList.Length}"
            });
        }

        var (customerIds, _, productCodes, taxKeys) = BuildMasterLookups(masterFiles);

        ValidateDocumentSequenceAndDates(
            invoiceList,
            i => i.InvoiceNo,
            i => i.InvoiceDate,
            i => i.SystemEntryDate,
            i => i.Pk,
            "Invoice/InvoiceNo",
            typeof(SourceDocumentsSalesInvoicesInvoice));

        decimal headerTotalDebit = 0m;
        decimal headerTotalCredit = 0m;

        foreach (var invoice in invoiceList)
        {
            AddError(invoice.ValidateInvoiceNo());
            AddError(invoice.ValidateATCUD());
            AddError(invoice.ValidateHash());
            AddError(invoice.ValidateHashControl());
            AddError(invoice.ValidatePeriod());
            AddError(invoice.ValidateInvoiceDate());
            AddError(invoice.ValidateSystemEntryDate());
            AddError(invoice.ValidateTransactionID());
            AddError(invoice.ValidateCustomerID());
            AddError(invoice.ValidateSourceID());
            AddError(invoice.ValidateEACCode());
            AddError(invoice.ValidateMovementEndTime());
            AddError(invoice.ValidateMovementStartTime());
            AddError(invoice.ValidateMovementTimeRange());
            AddError(invoice.ValidateSpecialRegimes());
            AddError(invoice.ValidateDocumentStatus());
            AddError(invoice.ValidateShipTo());
            AddError(invoice.ValidateShipFrom());
            AddError(invoice.ValidateDocumentTotals());
            AddError(invoice.ValidateWithholdingTax());

            ValidateDocumentTemporalConsistency(
                header,
                invoice.InvoiceNo,
                invoice.InvoiceDate,
                invoice.SystemEntryDate,
                invoice.DocumentStatus?.InvoiceStatusDate ?? DateTime.MinValue,
                invoice.Period,
                "Invoice",
                typeof(SourceDocumentsSalesInvoicesInvoice),
                invoice.Pk);

            if (customerIds != null && !string.IsNullOrEmpty(invoice.CustomerID) && !customerIds.Contains(invoice.CustomerID))
            {
                AddError(new ValidationError
                {
                    Description = $"Cliente {invoice.CustomerID} referenciado no documento {invoice.InvoiceNo} não existe na tabela de clientes.",
                    Field = "Invoice/CustomerID",
                    FileID = invoice.InvoiceNo,
                    Value = invoice.CustomerID,
                    TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice),
                    UID = invoice.Pk
                });
            }

            var lines = invoice.Line ?? [];
            bool isExcludedFromTotals = invoice.DocumentStatus?.InvoiceStatus is InvoiceStatus.A or InvoiceStatus.F;

            if (invoice.InvoiceType == InvoiceType.NC &&
                !lines.Any(l => l.References != null && l.References.Any(r => !string.IsNullOrWhiteSpace(r.Reference) && !string.IsNullOrWhiteSpace(r.Reason))))
            {
                AddError(new ValidationError
                {
                    Description = $"Nota de crédito {invoice.InvoiceNo} deve referenciar o documento de origem e respetivo motivo (References/Reference e Reason).",
                    Field = "Invoice/Line/References",
                    FileID = invoice.InvoiceNo,
                    Value = invoice.InvoiceNo,
                    TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice),
                    UID = invoice.Pk
                });
            }

            decimal total = 0m, incidencia = 0m, iva = 0m;
            int numLinha = 1;
            foreach (var line in lines)
            {
                int num = -1;
                if (!string.IsNullOrEmpty(line.LineNumber))
                    int.TryParse(line.LineNumber, out num);

                if (numLinha != num)
                {
                    AddError(new ValidationError
                    {
                        Value = line.LineNumber,
                        FileID = invoice.InvoiceNo,
                        Field = "Invoice/Line/LineNumber",
                        TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine),
                        Description = $"Número de linha incorrecto, Documento: {invoice.InvoiceNo}, esperado: {numLinha}, valor: {line.LineNumber}",
                        UID = line.Pk,
                        SupUID = invoice.Pk
                    });
                }
                numLinha++;

                ValidateInvoiceLine(line, invoice, productCodes, taxKeys);

                decimal creditAmt = GetInvoiceLineCreditAmount(line);
                decimal debitAmt = GetInvoiceLineDebitAmount(line);
                if (!isExcludedFromTotals)
                {
                    headerTotalCredit += creditAmt;
                    headerTotalDebit += debitAmt;
                }

                decimal lineAmount = creditAmt > 0 ? creditAmt : debitAmt;
                int op = Operation(invoice, line);

                decimal lineTaxAmount = 0m;
                if (line.Tax != null)
                {
                    if (line.Tax.ItemElementName == ItemChoiceType1.TaxPercentage)
                        lineTaxAmount = lineAmount * line.Tax.TaxPercentage.GetValueOrDefault(0) * 0.01m;
                    else if (line.Tax.ItemElementName == ItemChoiceType1.TaxAmount)
                        lineTaxAmount = line.Tax.TaxAmount.GetValueOrDefault(0);
                }

                incidencia += lineAmount * op;
                iva += lineTaxAmount * op;
                total += (lineAmount + lineTaxAmount) * op;
            }

            incidencia = Math.Round(incidencia, 2, MidpointRounding.AwayFromZero);
            iva = Math.Round(iva, 2, MidpointRounding.AwayFromZero);
            total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

            if (invoice.DocumentTotals != null)
            {
                if (Math.Abs(total - invoice.DocumentTotals.GrossTotal) > 0.01m)
                    AddError(new ValidationError { Value = invoice.DocumentTotals.GrossTotal.ToString(), FileID = invoice.InvoiceNo, Field = "Invoice/DocumentTotals/GrossTotal", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = $"Documento: {invoice.InvoiceNo}, total: {invoice.DocumentTotals.GrossTotal} esperado: {total}", UID = invoice.Pk });
                if (Math.Abs(incidencia - invoice.DocumentTotals.NetTotal) > 0.01m)
                    AddError(new ValidationError { Value = invoice.DocumentTotals.NetTotal.ToString(), FileID = invoice.InvoiceNo, Field = "Invoice/DocumentTotals/NetTotal", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = $"Documento {invoice.InvoiceNo}, incidencia:{invoice.DocumentTotals.NetTotal} esperado:{incidencia}", UID = invoice.Pk });
                if (Math.Abs(iva - invoice.DocumentTotals.TaxPayable) > 0.01m)
                    AddError(new ValidationError { Value = invoice.DocumentTotals.TaxPayable.ToString(), FileID = invoice.InvoiceNo, Field = "Invoice/DocumentTotals/TaxPayable", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = $"Documento: {invoice.InvoiceNo}, iva: {invoice.DocumentTotals.TaxPayable} esperado: {iva}", UID = invoice.Pk });
                if (Math.Abs(invoice.DocumentTotals.GrossTotal - (invoice.DocumentTotals.NetTotal + invoice.DocumentTotals.TaxPayable)) > 0.01m)
                    AddError(new ValidationError { Value = invoice.DocumentTotals.GrossTotal.ToString(), FileID = invoice.InvoiceNo, Field = "Invoice/DocumentTotals", TypeofError = typeof(SourceDocumentsSalesInvoicesInvoice), Description = $"Documento: {invoice.InvoiceNo}, NetTotal + TaxPayable = {invoice.DocumentTotals.NetTotal + invoice.DocumentTotals.TaxPayable} != GrossTotal {invoice.DocumentTotals.GrossTotal}", UID = invoice.Pk });
            }
        }

        if (invoices.TotalDebit != headerTotalDebit)
        {
            AddError(new ValidationError
            {
                Value = invoices.TotalDebit.ToString(),
                Field = "SalesInvoices/TotalDebit",
                TypeofError = typeof(SourceDocumentsSalesInvoices),
                Description = $"Total de débitos de faturação incorrecto. valor: {invoices.TotalDebit}, esperado: {headerTotalDebit}"
            });
        }

        if (invoices.TotalCredit != headerTotalCredit)
        {
            AddError(new ValidationError
            {
                Value = invoices.TotalCredit.ToString(),
                Field = "SalesInvoices/TotalCredit",
                TypeofError = typeof(SourceDocumentsSalesInvoices),
                Description = $"Total de créditos de faturação incorrecto. valor: {invoices.TotalCredit}, esperado: {headerTotalCredit}"
            });
        }
    }

    void ValidateInvoiceLine(
        SourceDocumentsSalesInvoicesInvoiceLine line,
        SourceDocumentsSalesInvoicesInvoice invoice,
        HashSet<string> productCodes,
        HashSet<(TaxType, string, string)> taxKeys)
    {
        AddError(line.ValidateLineNumber(invoice.Pk));
        AddError(line.ValidateProductCode(invoice.Pk));
        AddError(line.ValidateProductDescription(invoice.Pk));
        AddError(line.ValidateQuantity(invoice.Pk));
        AddError(line.ValidateTaxPointDate(invoice.Pk));
        AddError(line.ValidateUnitOfMeasure(invoice.Pk));
        AddError(line.ValidateUnitPrice(invoice.Pk));
        AddError(line.ValidateDescription(invoice.Pk));
        AddError(line.ValidateSettlementAmount(invoice.Pk));
        AddError(line.ValidateTaxBase(invoice.Pk));
        AddError(line.ValidateOrderReferences(invoice.Pk));
        AddError(line.ValidateReferences(invoice.Pk));
        AddError(line.ValidateTax(invoice.Pk));
        AddError(line.ValidateTaxExemption(invoice.Pk, invoice.InvoiceDate));

        if (productCodes != null && !string.IsNullOrEmpty(line.ProductCode) && !productCodes.Contains(line.ProductCode))
        {
            AddError(new ValidationError
            {
                Description = $"Produto {line.ProductCode} na linha nº {line.LineNumber} do documento {invoice.InvoiceNo} não existe na tabela de produtos.",
                Field = "Invoice/Line/ProductCode",
                FileID = invoice.InvoiceNo,
                Value = line.ProductCode,
                TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine),
                UID = line.Pk,
                SupUID = invoice.Pk
            });
        }

        if (taxKeys != null && line.Tax != null && !taxKeys.Contains((line.Tax.TaxType, line.Tax.TaxCountryRegion ?? string.Empty, line.Tax.TaxCode ?? string.Empty)))
        {
            AddError(new ValidationError
            {
                Description = $"Taxa ({line.Tax.TaxType}/{line.Tax.TaxCountryRegion}/{line.Tax.TaxCode}) na linha nº {line.LineNumber} do documento {invoice.InvoiceNo} não existe na tabela de impostos.",
                Field = "Invoice/Line/Tax",
                FileID = invoice.InvoiceNo,
                Value = line.Tax.TaxCode,
                TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine),
                UID = line.Pk,
                SupUID = invoice.Pk
            });
        }

        decimal lineAmount = line.CreditAmount ?? line.DebitAmount ?? line.Item;
        int numCasasDecimais = 6;
        bool incidencia = Math.Abs(Math.Round(line.UnitPrice * line.Quantity, numCasasDecimais, MidpointRounding.AwayFromZero) - Math.Round(lineAmount, numCasasDecimais, MidpointRounding.AwayFromZero)) > 0.01m;
        if (incidencia)
        {
            AddError(new ValidationError
            {
                Value = lineAmount.ToString(),
                FileID = invoice.InvoiceNo,
                Field = $"Invoice/Line/{line.ItemElementName}",
                TypeofError = typeof(SourceDocumentsSalesInvoicesInvoiceLine),
                Description = $"Total de linha diferente do valor unitário * quantidade ({line.UnitPrice} * {line.Quantity} <> {lineAmount}). Documento: {line.InvoiceNo}",
                UID = line.Pk,
                SupUID = invoice.Pk
            });
        }
    }

    internal void ValidatePayments(SourceDocumentsPayments payments)
    {
        ValidatePayments(payments, header: null, masterFiles: null, includeExtendedChecks: false);
    }

    internal void ValidatePayments(SourceDocumentsPayments payments, Header header, AuditFileMasterFiles masterFiles, bool includeExtendedChecks = true)
    {
        if (payments == null) return;

        var paymentList = payments.Payment ?? [];
        if (!int.TryParse(payments.NumberOfEntries, out int declaredEntries) || declaredEntries != paymentList.Length)
        {
            AddError(new ValidationError
            {
                Value = payments.NumberOfEntries,
                Field = "Payments/NumberOfEntries",
                TypeofError = typeof(SourceDocumentsPayments),
                Description = $"Nº de registos dos recibos incorrecto. valor: {payments.NumberOfEntries}, esperado: {paymentList.Length}"
            });
        }

        decimal totalDebit = 0m;
        decimal totalCredit = 0m;

        foreach (var payment in paymentList)
        {
            // revoked documents don't add the debit or credit
            if (payment.DocumentStatus?.PaymentStatus == PaymentStatus.A || payment.Line == null)
                continue;

            foreach (var line in payment.Line)
            {
                totalDebit += GetPaymentLineDebitAmount(line);
                totalCredit += GetPaymentLineCreditAmount(line);
            }
        }

        if (payments.TotalDebit != totalDebit)
            AddError(new ValidationError { Value = payments.TotalDebit.ToString(), Field = "Payments/TotalDebit", TypeofError = typeof(SourceDocumentsPayments), Description = $"Total de débitos dos recibos incorrecto. valor: {payments.TotalDebit}, esperado: {totalDebit}" });

        if (payments.TotalCredit != totalCredit)
            AddError(new ValidationError { Value = payments.TotalCredit.ToString(), Field = "Payments/TotalCredit", TypeofError = typeof(SourceDocumentsPayments), Description = $"Total de créditos dos recibos incorrecto. valor: {payments.TotalCredit}, esperado: {totalCredit}" });

        var (customerIds, _, _, taxKeys) = includeExtendedChecks ? BuildMasterLookups(masterFiles) : (null, null, null, null);

        if (includeExtendedChecks)
        {
            ValidateDocumentSequenceAndDates(
                paymentList,
                p => p.PaymentRefNo,
                p => p.TransactionDate,
                p => p.SystemEntryDate,
                p => p.Pk,
                "Payment/PaymentRefNo",
                typeof(SourceDocumentsPaymentsPayment));
        }

        foreach (var payment in paymentList)
        {
            AddError(payment.ValidateCustomerID());
            AddError(payment.ValidateDescription());
            AddError(payment.ValidatePaymentRefNo());
            AddError(payment.ValidatePeriod());
            AddError(payment.ValidateSourceID());
            AddError(payment.ValidateSystemEntryDate());
            AddError(payment.ValidateSystemID());
            AddError(payment.ValidateTransactionDate());
            AddError(payment.ValidateTransactionID());
            AddError(payment.ValidatePaymentMethod());

            if (includeExtendedChecks)
            {
                AddError(payment.ValidateATCUD());
                AddError(payment.ValidateDocumentStatus());
                AddError(payment.ValidateDocumentTotals());
                AddError(payment.ValidateWithholdingTax());

                ValidateDocumentTemporalConsistency(
                    header,
                    payment.PaymentRefNo,
                    payment.TransactionDate,
                    payment.SystemEntryDate,
                    payment.DocumentStatus?.PaymentStatusDate ?? DateTime.MinValue,
                    payment.Period,
                    "Payment",
                    typeof(SourceDocumentsPaymentsPayment),
                    payment.Pk);

                if (customerIds != null && !string.IsNullOrEmpty(payment.CustomerID) && !customerIds.Contains(payment.CustomerID))
                {
                    AddError(new ValidationError
                    {
                        Description = $"Cliente {payment.CustomerID} referenciado no recibo {payment.PaymentRefNo} não existe na tabela de clientes.",
                        Field = "Payment/CustomerID",
                        FileID = payment.PaymentRefNo,
                        Value = payment.CustomerID,
                        TypeofError = typeof(SourceDocumentsPaymentsPayment),
                        UID = payment.Pk
                    });
                }
            }
            else
            {
                AddError(payment.ValidateDocumentStatusSourceID());
                AddError(payment.ValidatePaymentStatusDate());
                AddError(payment.ValidateReason());
            }

            decimal sumNetOrGrossLines = 0m;
            decimal sumSettlementLines = 0m;
            int numLinha = 1;
            foreach (var line in payment.Line ?? [])
            {
                int num = -1;
                if (!string.IsNullOrEmpty(line.LineNumber))
                    int.TryParse(line.LineNumber, out num);

                if (numLinha != num)
                    AddError(new ValidationError { Value = line.LineNumber, FileID = payment.PaymentRefNo, Field = "Payments/Line/LineNumber", TypeofError = typeof(SourceDocumentsPaymentsPaymentLine), Description = $"Número de linha incorrecto, Documento: {payment.PaymentRefNo}, esperado: {numLinha}, valor: {line.LineNumber}", UID = line.Pk, SupUID = payment.Pk });
                numLinha++;

                ValidatePaymentLine(line, payment, taxKeys, includeExtendedChecks);

                decimal creditAmt = GetPaymentLineCreditAmount(line);
                decimal debitAmt = GetPaymentLineDebitAmount(line);
                sumNetOrGrossLines += creditAmt - debitAmt;
                sumSettlementLines += line.SettlementAmount;
            }

            if (includeExtendedChecks && payment.DocumentTotals != null)
            {
                var totals = payment.DocumentTotals;
                if (Math.Abs(totals.GrossTotal - (totals.NetTotal + totals.TaxPayable)) > 0.01m)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Recibo {payment.PaymentRefNo}: NetTotal + TaxPayable ({totals.NetTotal + totals.TaxPayable}) != GrossTotal ({totals.GrossTotal}).",
                        Field = "Payment/DocumentTotals",
                        FileID = payment.PaymentRefNo,
                        Value = totals.GrossTotal.ToString(),
                        TypeofError = typeof(SourceDocumentsPaymentsPaymentDocumentTotals),
                        UID = payment.Pk
                    });
                }

                decimal expectedLineSumTarget = (payment.PaymentType == SAFTPTPaymentType.RC || totals.TaxPayable > 0)
                    ? totals.NetTotal
                    : totals.GrossTotal;
                decimal absSumLines = Math.Abs(Math.Round(sumNetOrGrossLines, 2, MidpointRounding.AwayFromZero));

                if (Math.Abs(absSumLines - expectedLineSumTarget) > 0.01m)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Recibo {payment.PaymentRefNo}: soma das linhas ({absSumLines}) diferente do total esperado ({expectedLineSumTarget}).",
                        Field = "Payment/DocumentTotals",
                        FileID = payment.PaymentRefNo,
                        Value = expectedLineSumTarget.ToString(),
                        TypeofError = typeof(SourceDocumentsPaymentsPaymentDocumentTotals),
                        UID = payment.Pk
                    });
                }

                if (totals.Settlement != null && Math.Abs(sumSettlementLines - totals.Settlement.SettlementAmount) > 0.01m)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Recibo {payment.PaymentRefNo}: SettlementAmount ({totals.Settlement.SettlementAmount}) diferente da soma dos descontos das linhas ({sumSettlementLines}).",
                        Field = "Payment/DocumentTotals/Settlement/SettlementAmount",
                        FileID = payment.PaymentRefNo,
                        Value = totals.Settlement.SettlementAmount.ToString(),
                        TypeofError = typeof(SourceDocumentsPaymentsPaymentDocumentTotals),
                        UID = payment.Pk
                    });
                }

                if (payment.PaymentMethod is { Length: > 0 })
                {
                    decimal paidAmount = payment.PaymentMethod.Sum(m => m.PaymentAmount);
                    decimal withholding = payment.WithholdingTax?.Sum(w => w.WithholdingTaxAmount) ?? 0m;
                    decimal expectedPaid = totals.GrossTotal - withholding;
                    if (Math.Abs(paidAmount - expectedPaid) > 0.01m)
                    {
                        AddError(new ValidationError
                        {
                            Description = $"Recibo {payment.PaymentRefNo}: soma dos meios de pagamento ({paidAmount}) diferente do total líquido de retenção ({expectedPaid}).",
                            Field = "Payment/PaymentMethod/PaymentAmount",
                            FileID = payment.PaymentRefNo,
                            Value = paidAmount.ToString(),
                            TypeofError = typeof(SourceDocumentsPaymentsPayment),
                            UID = payment.Pk
                        });
                    }
                }
            }
        }
    }

    private void ValidatePaymentLine(
        SourceDocumentsPaymentsPaymentLine line,
        SourceDocumentsPaymentsPayment payment,
        HashSet<(TaxType, string, string)> taxKeys,
        bool includeExtendedChecks)
    {
        AddError(line.ValidateItem(payment.Pk));
        AddError(line.ValidateLineNumber(payment.Pk));
        AddError(line.ValidateSettlementAmount(payment.Pk));
        AddError(line.ValidateSourceDocumentID(payment.Pk));
        AddError(line.ValidateTax(payment.Pk));

        if (includeExtendedChecks)
        {
            AddError(line.ValidateTaxExemption(payment.Pk, payment.TransactionDate));

            if (taxKeys != null && line.Tax != null && !taxKeys.Contains((line.Tax.TaxType, line.Tax.TaxCountryRegion ?? string.Empty, line.Tax.TaxCode ?? string.Empty)))
            {
                AddError(new ValidationError
                {
                    Description = $"Taxa ({line.Tax.TaxType}/{line.Tax.TaxCountryRegion}/{line.Tax.TaxCode}) na linha nº {line.LineNumber} do recibo {payment.PaymentRefNo} não existe na tabela de impostos.",
                    Field = "Payment/Line/Tax",
                    FileID = payment.PaymentRefNo,
                    Value = line.Tax.TaxCode,
                    TypeofError = typeof(SourceDocumentsPaymentsPaymentLine),
                    UID = line.Pk,
                    SupUID = payment.Pk
                });
            }
        }
        else
        {
            AddError(line.ValidateTaxExemptionReason(payment.Pk));
        }
    }

    private static (HashSet<string> CustomerIds, HashSet<string> SupplierIds, HashSet<string> ProductCodes, HashSet<(TaxType, string, string)> TaxKeys) BuildMasterLookups(AuditFileMasterFiles masterFiles)
    {
        if (masterFiles == null)
            return (null, null, null, null);

        var customerIds = masterFiles.Customer != null
            ? new HashSet<string>(masterFiles.Customer.Where(c => !string.IsNullOrEmpty(c.CustomerID)).Select(c => c.CustomerID), StringComparer.OrdinalIgnoreCase)
            : null;
        var supplierIds = masterFiles.Supplier != null
            ? new HashSet<string>(masterFiles.Supplier.Where(s => !string.IsNullOrEmpty(s.SupplierID)).Select(s => s.SupplierID), StringComparer.OrdinalIgnoreCase)
            : null;
        var productCodes = masterFiles.Product != null
            ? new HashSet<string>(masterFiles.Product.Where(p => !string.IsNullOrEmpty(p.ProductCode)).Select(p => p.ProductCode), StringComparer.OrdinalIgnoreCase)
            : null;
        var taxKeys = masterFiles.TaxTable != null
            ? new HashSet<(TaxType, string, string)>(masterFiles.TaxTable.Select(t => (t.TaxType, t.TaxCountryRegion ?? string.Empty, t.TaxCode ?? string.Empty)))
            : null;

        return (customerIds, supplierIds, productCodes, taxKeys);
    }

    private void ValidateDocumentSequenceAndDates<TDoc>(
        TDoc[] docs,
        Func<TDoc, string> getDocNo,
        Func<TDoc, DateTime> getDocDate,
        Func<TDoc, DateTime> getSystemEntryDate,
        Func<TDoc, string> getPk,
        string field,
        Type typeofError)
    {
        var seenNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parsedDocs = new List<(string DocNo, string Type, string Series, int Number, DateTime DocDate, DateTime SysDate, string Pk)>();

        foreach (var doc in docs)
        {
            string docNo = getDocNo(doc);
            if (string.IsNullOrWhiteSpace(docNo)) continue;

            if (!seenNumbers.Add(docNo))
            {
                AddError(new ValidationError
                {
                    Description = $"Número de documento duplicado: {docNo}.",
                    Field = field,
                    FileID = docNo,
                    Value = docNo,
                    TypeofError = typeofError,
                    UID = getPk(doc)
                });
            }

            if (Validations.TryParseDocumentNumber(docNo, out string docType, out string series, out int number))
            {
                parsedDocs.Add((docNo, docType, series, number, getDocDate(doc), getSystemEntryDate(doc), getPk(doc)));
            }
        }

        foreach (var group in parsedDocs.GroupBy(d => (d.Type, d.Series)))
        {
            var ordered = group.OrderBy(d => d.Number).ToArray();
            for (int i = 1; i < ordered.Length; i++)
            {
                var prev = ordered[i - 1];
                var curr = ordered[i];

                if (prev.DocDate != DateTime.MinValue && curr.DocDate != DateTime.MinValue && curr.DocDate.Date < prev.DocDate.Date)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Ordem cronológica inválida: data de emissão de {curr.DocNo} ({curr.DocDate:yyyy-MM-dd}) anterior à de {prev.DocNo} ({prev.DocDate:yyyy-MM-dd}).",
                        Field = field,
                        FileID = curr.DocNo,
                        Value = curr.DocDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        TypeofError = typeofError,
                        UID = curr.Pk
                    });
                }

                if (prev.SysDate != DateTime.MinValue && curr.SysDate != DateTime.MinValue && curr.SysDate < prev.SysDate)
                {
                    AddError(new ValidationError
                    {
                        Description = $"Ordem cronológica inválida: SystemEntryDate de {curr.DocNo} ({curr.SysDate:yyyy-MM-ddTHH:mm:ss}) anterior ao de {prev.DocNo} ({prev.SysDate:yyyy-MM-ddTHH:mm:ss}).",
                        Field = field,
                        FileID = curr.DocNo,
                        Value = curr.SysDate.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                        TypeofError = typeofError,
                        UID = curr.Pk
                    });
                }
            }
        }
    }

    private void ValidateDocumentTemporalConsistency(
        Header header,
        string docNo,
        DateTime docDate,
        DateTime systemEntryDate,
        DateTime statusDate,
        string period,
        string basePath,
        Type typeofError,
        string uid)
    {
        if (header != null && docDate != DateTime.MinValue && header.StartDate != DateTime.MinValue && header.EndDate != DateTime.MinValue)
        {
            if (docDate.Date < header.StartDate.Date || docDate.Date > header.EndDate.Date)
            {
                AddError(new ValidationError
                {
                    Description = $"Data do documento {docNo} ({docDate:yyyy-MM-dd}) fora do período do cabeçalho ({header.StartDate:yyyy-MM-dd} a {header.EndDate:yyyy-MM-dd}).",
                    Field = $"{basePath}/Date",
                    FileID = docNo,
                    Value = docDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TypeofError = typeofError,
                    UID = uid
                });
            }
        }

        if (docDate != DateTime.MinValue && systemEntryDate != DateTime.MinValue && docDate.Date > systemEntryDate.Date)
        {
            AddError(new ValidationError
            {
                Description = $"Data de emissão do documento {docNo} ({docDate:yyyy-MM-dd}) posterior à data de gravação ({systemEntryDate:yyyy-MM-dd}).",
                Field = $"{basePath}/SystemEntryDate",
                FileID = docNo,
                Value = systemEntryDate.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                TypeofError = typeofError,
                UID = uid
            });
        }

        if (statusDate != DateTime.MinValue && systemEntryDate != DateTime.MinValue && statusDate < systemEntryDate)
        {
            AddError(new ValidationError
            {
                Description = $"Data do estado atual do documento {docNo} ({statusDate:yyyy-MM-ddTHH:mm:ss}) anterior à data de gravação ({systemEntryDate:yyyy-MM-ddTHH:mm:ss}).",
                Field = $"{basePath}/DocumentStatus",
                FileID = docNo,
                Value = statusDate.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                TypeofError = typeofError,
                UID = uid
            });
        }

        if (!string.IsNullOrEmpty(period) && int.TryParse(period, out int p) && p is >= 1 and <= 12 && docDate != DateTime.MinValue && p != docDate.Month)
        {
            AddError(new ValidationError
            {
                Description = $"Período ({p}) do documento {docNo} não coincide com o mês da data do documento ({docDate.Month}).",
                Field = $"{basePath}/Period",
                FileID = docNo,
                Value = period,
                TypeofError = typeofError,
                ValidationType = ValidationErrorType.WARNING,
                UID = uid
            });
        }
    }

    private static decimal GetInvoiceLineDebitAmount(SourceDocumentsSalesInvoicesInvoiceLine line) =>
        line.DebitAmount ?? (line.CreditAmount == null && line.ItemElementName == ItemChoiceType4.DebitAmount ? line.Item : 0m);

    private static decimal GetInvoiceLineCreditAmount(SourceDocumentsSalesInvoicesInvoiceLine line) =>
        line.CreditAmount ?? (line.DebitAmount == null && line.ItemElementName == ItemChoiceType4.CreditAmount ? line.Item : 0m);

    private static decimal GetWorkLineDebitAmount(SourceDocumentsWorkingDocumentsWorkDocumentLine line) =>
        line.DebitAmount ?? (line.CreditAmount == null && line.ItemElementName == ItemChoiceType7.DebitAmount ? line.Item : 0m);

    private static decimal GetWorkLineCreditAmount(SourceDocumentsWorkingDocumentsWorkDocumentLine line) =>
        line.CreditAmount ?? (line.DebitAmount == null && line.ItemElementName == ItemChoiceType7.CreditAmount ? line.Item : 0m);

    private static decimal GetPaymentLineDebitAmount(SourceDocumentsPaymentsPaymentLine line) =>
        line.DebitAmount ?? (line.CreditAmount == null && line.ItemElementName == ItemChoiceType8.DebitAmount ? line.Item : 0m);

    private static decimal GetPaymentLineCreditAmount(SourceDocumentsPaymentsPaymentLine line) =>
        line.CreditAmount ?? (line.DebitAmount == null && line.ItemElementName == ItemChoiceType8.CreditAmount ? line.Item : 0m);

    public static int Operation(SourceDocumentsSalesInvoicesInvoice i, SourceDocumentsSalesInvoicesInvoiceLine l)
    {
        if (i.InvoiceType is InvoiceType.FT or InvoiceType.VD or InvoiceType.ND or InvoiceType.FR or InvoiceType.FS or InvoiceType.TV or InvoiceType.AA or InvoiceType.RP or InvoiceType.CS or InvoiceType.LD or InvoiceType.RA)
            return l.ItemElementName == ItemChoiceType4.CreditAmount ? 1 : -1;

        if (i.InvoiceType is InvoiceType.NC or InvoiceType.TD or InvoiceType.DA or InvoiceType.RE)
            return l.ItemElementName == ItemChoiceType4.DebitAmount ? 1 : -1;

        return 1;
    }

    public static int Operation(SourceDocumentsWorkingDocumentsWorkDocument d, SourceDocumentsWorkingDocumentsWorkDocumentLine l)
    {
        if (d.WorkType == WorkType.RE)
            return l.ItemElementName == ItemChoiceType7.DebitAmount ? 1 : -1;

        return l.ItemElementName == ItemChoiceType7.CreditAmount ? 1 : -1;
    }

    #endregion
}
