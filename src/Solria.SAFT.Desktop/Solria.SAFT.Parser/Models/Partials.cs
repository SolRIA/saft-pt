using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SolRIA.SAFT.Parser.Models;

internal static partial class PartialValidationPatterns
{
    [GeneratedRegex("^[^ ]+ [^/^ ]+/[0-9]+$")]
    internal static partial Regex DocumentNumberRegex();

    [GeneratedRegex("^[0-9]{5}$")]
    internal static partial Regex EacCodeRegex();

    [GeneratedRegex("^[0-9]{4}-[0-9]{3}$")]
    internal static partial Regex PostalCodePtRegex();

    [GeneratedRegex(@"^(([0-9]{9})|([^^]+ [0-9/]+))$")]
    internal static partial Regex CompanyIdRegex();

    [GeneratedRegex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$", RegexOptions.IgnoreCase)]
    internal static partial Regex EmailRegex();

    [GeneratedRegex("^[1-9][0-9]{3}-[01][0-9]-[0-3][0-9] [^ ]{1,30} [^ ]{1,20}$")]
    internal static partial Regex TransactionIdRegex();

    internal static void ValidateTaxExemptionFields(
        List<ValidationError> errors,
        Type typeofError,
        string docNo,
        string lineNumber,
        string pk,
        string supPk,
        string basePath,
        bool isIvaZeroOrExempt,
        bool isTaxedAboveZero,
        string taxExemptionReason,
        string taxExemptionCode,
        DateTime documentDate)
    {
        if (!string.IsNullOrEmpty(taxExemptionReason) && (taxExemptionReason.Length < 6 || taxExemptionReason.Length > 60))
        {
            errors.Add(new ValidationError
            {
                Description = $"Motivo da isenção de imposto com tamanho incorrecto (6 a 60 caracteres), linha nº {lineNumber}.",
                Field = $"{basePath}/TaxExemptionReason",
                FileID = docNo,
                Value = taxExemptionReason,
                TypeofError = typeofError,
                UID = pk,
                SupUID = supPk
            });
        }

        if (!string.IsNullOrEmpty(taxExemptionCode))
        {
            if (Validations.IsRevokedTaxExemptionCode(taxExemptionCode))
            {
                if (documentDate >= new DateTime(2023, 1, 1))
                {
                    errors.Add(new ValidationError
                    {
                        Description = $"O código de isenção M08 foi revogado a partir de 2023-01-01 (use M30-M34 ou M40-M43), linha nº {lineNumber}.",
                        Field = $"{basePath}/TaxExemptionCode",
                        FileID = docNo,
                        Value = taxExemptionCode,
                        TypeofError = typeofError,
                        ValidationType = ValidationErrorType.WARNING,
                        UID = pk,
                        SupUID = supPk
                    });
                }
            }
            else if (!Validations.IsValidTaxExemptionCode(taxExemptionCode))
            {
                errors.Add(new ValidationError
                {
                    Description = $"Código de motivo de isenção de imposto inválido ({taxExemptionCode}), linha nº {lineNumber}.",
                    Field = $"{basePath}/TaxExemptionCode",
                    FileID = docNo,
                    Value = taxExemptionCode,
                    TypeofError = typeofError,
                    UID = pk,
                    SupUID = supPk
                });
            }
        }

        if (isIvaZeroOrExempt)
        {
            if (string.IsNullOrEmpty(taxExemptionReason))
            {
                errors.Add(new ValidationError
                {
                    Description = $"Motivo de isenção do imposto não preenchido. Documento: {docNo}, linha nº: {lineNumber}",
                    Field = $"{basePath}/TaxExemptionReason",
                    FileID = docNo,
                    Value = taxExemptionReason,
                    TypeofError = typeofError,
                    UID = pk,
                    SupUID = supPk
                });
            }

            if (string.IsNullOrEmpty(taxExemptionCode))
            {
                errors.Add(new ValidationError
                {
                    Description = $"Código de motivo de isenção do imposto não preenchido. Documento: {docNo}, linha nº: {lineNumber}",
                    Field = $"{basePath}/TaxExemptionCode",
                    FileID = docNo,
                    Value = taxExemptionCode,
                    TypeofError = typeofError,
                    UID = pk,
                    SupUID = supPk
                });
            }
        }
        else if (isTaxedAboveZero)
        {
            if (!string.IsNullOrWhiteSpace(taxExemptionReason))
            {
                errors.Add(new ValidationError
                {
                    Description = $"Motivo de isenção preenchido numa linha sujeita a imposto (> 0%), linha nº {lineNumber}.",
                    Field = $"{basePath}/TaxExemptionReason",
                    FileID = docNo,
                    Value = taxExemptionReason,
                    TypeofError = typeofError,
                    UID = pk,
                    SupUID = supPk
                });
            }

            if (!string.IsNullOrWhiteSpace(taxExemptionCode))
            {
                errors.Add(new ValidationError
                {
                    Description = $"Código de isenção preenchido numa linha sujeita a imposto (> 0%), linha nº {lineNumber}.",
                    Field = $"{basePath}/TaxExemptionCode",
                    FileID = docNo,
                    Value = taxExemptionCode,
                    TypeofError = typeofError,
                    UID = pk,
                    SupUID = supPk
                });
            }
        }
    }
}

public partial class SourceDocumentsPaymentsPayment : BaseData
{
    public ValidationError ValidatePaymentRefNo()
    {
        if (string.IsNullOrEmpty(PaymentRefNo) || PaymentRefNo.Length > 60)
        {
            return new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "Payment/PaymentRefNo", FileID = PaymentRefNo, Value = PaymentRefNo, TypeofError = GetType(), UID = Pk };
        }
        if (!PartialValidationPatterns.DocumentNumberRegex().IsMatch(PaymentRefNo))
        {
            return new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "Payment/PaymentRefNo", FileID = PaymentRefNo, Value = PaymentRefNo, TypeofError = GetType(), UID = Pk };
        }
        if (PaymentType != SAFTPTPaymentType.INVALIDO && !PaymentRefNo.StartsWith($"{PaymentType} ", StringComparison.Ordinal))
        {
            return new ValidationError { Description = $"O número do recibo ({PaymentRefNo}) deve iniciar pelo tipo ({PaymentType}).", Field = "Payment/PaymentRefNo", FileID = PaymentRefNo, Value = PaymentRefNo, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateATCUD()
    {
        return Validations.ValidateAtcud(ATCUD, PaymentRefNo, "Payment/ATCUD", GetType(), Pk);
    }

    public ValidationError ValidatePeriod()
    {
        if (!string.IsNullOrEmpty(Period))
        {
            if (!int.TryParse(Period, out int periodo) || periodo < 1 || periodo > 12)
            {
                return new ValidationError { Description = $"Mês do período de tributação do documento {PaymentRefNo} incorrecto.", Field = "Payment/Period", FileID = PaymentRefNo, Value = Period, TypeofError = GetType(), UID = Pk };
            }
        }
        return null;
    }

    public ValidationError ValidateTransactionID()
    {
        if (!string.IsNullOrEmpty(TransactionID) && (TransactionID.Length > 70 || !PartialValidationPatterns.TransactionIdRegex().IsMatch(TransactionID)))
        {
            return new ValidationError { Description = $"Identificador da transacção do documento {PaymentRefNo} incorrecto.", Field = "Payment/TransactionID", FileID = PaymentRefNo, Value = TransactionID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTransactionDate()
    {
        if (TransactionDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data de emissão do documento {PaymentRefNo} incorrecta.", Field = "Payment/TransactionDate", FileID = PaymentRefNo, Value = TransactionDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateDescription()
    {
        if (!string.IsNullOrEmpty(Description) && Description.Length > 200)
        {
            return new ValidationError { Description = $"Tamanho da descrição do recibo {PaymentRefNo} incorrecto.", Field = "Payment/Description", FileID = PaymentRefNo, Value = Description, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidatePaymentStatusDate()
    {
        if (DocumentStatus != null && DocumentStatus.PaymentStatusDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data e hora do estado atual do recibo {PaymentRefNo} incorrecta.", Field = "Payment/DocumentStatus/PaymentStatusDate", FileID = PaymentRefNo, Value = DocumentStatus.PaymentStatusDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateReason()
    {
        if (DocumentStatus != null && !string.IsNullOrEmpty(DocumentStatus.Reason) && DocumentStatus.Reason.Length > 50)
        {
            return new ValidationError { Description = $"Tamanho do motivo da alteração de estado do recibo {PaymentRefNo} incorrecto.", Field = "Payment/DocumentStatus/Reason", FileID = PaymentRefNo, Value = DocumentStatus.Reason, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateDocumentStatusSourceID()
    {
        if (DocumentStatus != null && (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30))
        {
            return new ValidationError { Description = $"Utilizador responsável pelo estado atual do recibo {PaymentRefNo} incorrecto.", Field = "Payment/DocumentStatus/SourceID", FileID = PaymentRefNo, Value = DocumentStatus.SourceID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError[] ValidateDocumentStatus()
    {
        var listError = new List<ValidationError>();

        if (DocumentStatus == null)
        {
            listError.Add(new ValidationError { Description = $"Situação do recibo {PaymentRefNo} inexistente.", Field = "Payment/DocumentStatus", FileID = PaymentRefNo, TypeofError = GetType(), UID = Pk });
            return [.. listError];
        }

        if (DocumentStatus.PaymentStatus == PaymentStatus.INVALIDO)
            listError.Add(new ValidationError { Description = $"Estado do recibo {PaymentRefNo} inválido.", Field = "Payment/DocumentStatus/PaymentStatus", FileID = PaymentRefNo, Value = DocumentStatus.PaymentStatus.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.SourcePayment == SAFTPTSourcePayment.INVALIDO)
            listError.Add(new ValidationError { Description = $"Origem do recibo {PaymentRefNo} inválida.", Field = "Payment/DocumentStatus/SourcePayment", FileID = PaymentRefNo, Value = DocumentStatus.SourcePayment.ToString(), TypeofError = GetType(), UID = Pk });

        var dateErr = ValidatePaymentStatusDate();
        if (dateErr != null) listError.Add(dateErr);
        var reasonErr = ValidateReason();
        if (reasonErr != null) listError.Add(reasonErr);
        var sourceErr = ValidateDocumentStatusSourceID();
        if (sourceErr != null) listError.Add(sourceErr);

        if (DocumentStatus.PaymentStatus == PaymentStatus.A && string.IsNullOrWhiteSpace(DocumentStatus.Reason))
        {
            listError.Add(new ValidationError
            {
                Description = $"Recibo anulado {PaymentRefNo} sem motivo de alteração de estado preenchido.",
                Field = "Payment/DocumentStatus/Reason",
                FileID = PaymentRefNo,
                TypeofError = GetType(),
                ValidationType = ValidationErrorType.WARNING,
                UID = Pk
            });
        }

        return [.. listError];
    }

    public ValidationError ValidateSystemID()
    {
        if (!string.IsNullOrEmpty(SystemID) && SystemID.Length > 60)
        {
            return new ValidationError { Description = $"Tamanho do número único do recibo {PaymentRefNo} incorrecto.", Field = "Payment/SystemID", FileID = PaymentRefNo, Value = SystemID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSourceID()
    {
        if (string.IsNullOrEmpty(SourceID) || SourceID.Length > 30)
        {
            return new ValidationError { Description = $"Utilizador que gerou o documento {PaymentRefNo} incorrecto.", Field = "Payment/SourceID", FileID = PaymentRefNo, Value = SourceID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSystemEntryDate()
    {
        if (SystemEntryDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data da gravação do documento {PaymentRefNo} incorrecta.", Field = "Payment/SystemEntryDate", FileID = PaymentRefNo, Value = SystemEntryDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCustomerID()
    {
        if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
        {
            return new ValidationError { Description = $"Chave única da tabela de clientes no documento {PaymentRefNo} incorrecta.", Field = "Payment/CustomerID", FileID = PaymentRefNo, Value = CustomerID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError[] ValidatePaymentMethod()
    {
        if (PaymentMethod == null || PaymentMethod.Length == 0) return [];

        var listErro = new List<ValidationError>();
        foreach (var pay in PaymentMethod)
        {
            if (pay.PaymentMechanism == PaymentMechanism.INVALIDO)
            {
                listErro.Add(new ValidationError { Description = $"Meio de pagamento do documento {PaymentRefNo} inválido.", Field = "Payment/PaymentMethod/PaymentMechanism", FileID = PaymentRefNo, Value = pay.PaymentMechanism.ToString(), TypeofError = GetType(), UID = Pk });
            }
            if (pay.PaymentAmount < 0)
            {
                listErro.Add(new ValidationError { Description = $"Montante do pagamento do documento {PaymentRefNo} incorrecto.", Field = "Payment/PaymentMethod/PaymentAmount", FileID = PaymentRefNo, Value = pay.PaymentAmount.ToString(), TypeofError = GetType(), UID = Pk });
            }
            if (pay.PaymentDate == DateTime.MinValue)
            {
                listErro.Add(new ValidationError { Description = $"Data do pagamento do documento {PaymentRefNo} incorrecta.", Field = "Payment/PaymentMethod/PaymentDate", FileID = PaymentRefNo, Value = pay.PaymentDate.ToString(), TypeofError = GetType(), UID = Pk });
            }
        }
        return [.. listErro];
    }

    public ValidationError[] ValidateDocumentTotals()
    {
        var listError = new List<ValidationError>();

        if (DocumentTotals == null)
        {
            listError.Add(new ValidationError { Description = $"Totais do recibo {PaymentRefNo} inexistentes.", Field = "Payment/DocumentTotals", FileID = PaymentRefNo, TypeofError = GetType(), UID = Pk });
            return [.. listError];
        }

        if (DocumentTotals.Currency != null)
        {
            if (DocumentTotals.Currency.CurrencyAmount < 0)
                listError.Add(new ValidationError { Description = $"Valor total em moeda estrangeira do recibo {PaymentRefNo} incorrecto.", Field = "Payment/DocumentTotals/Currency/CurrencyAmount", FileID = PaymentRefNo, Value = DocumentTotals.Currency.CurrencyAmount.ToString(), TypeofError = GetType(), UID = Pk });
            if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length != 3)
                listError.Add(new ValidationError { Description = $"Código de moeda do recibo {PaymentRefNo} incorrecto.", Field = "Payment/DocumentTotals/Currency/CurrencyCode", FileID = PaymentRefNo, Value = DocumentTotals.Currency.CurrencyCode, TypeofError = GetType(), UID = Pk });
            if (DocumentTotals.Currency.ExchangeRate < 0)
                listError.Add(new ValidationError { Description = $"Taxa de câmbio do recibo {PaymentRefNo} incorrecta.", Field = "Payment/DocumentTotals/Currency/ExchangeRate", FileID = PaymentRefNo, Value = DocumentTotals.Currency.ExchangeRate.ToString(), TypeofError = GetType(), UID = Pk });
        }

        if (DocumentTotals.Settlement != null && DocumentTotals.Settlement.SettlementAmount < 0)
            listError.Add(new ValidationError { Description = $"Montante de desconto do recibo {PaymentRefNo} incorrecto.", Field = "Payment/DocumentTotals/Settlement/SettlementAmount", FileID = PaymentRefNo, Value = DocumentTotals.Settlement.SettlementAmount.ToString(), TypeofError = GetType(), UID = Pk });

        if (DocumentTotals.GrossTotal < 0)
            listError.Add(new ValidationError { Description = $"Total do recibo {PaymentRefNo} incorrecto.", Field = "Payment/DocumentTotals/GrossTotal", FileID = PaymentRefNo, Value = DocumentTotals.GrossTotal.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentTotals.NetTotal < 0)
            listError.Add(new ValidationError { Description = $"Total sem impostos do recibo {PaymentRefNo} incorrecto.", Field = "Payment/DocumentTotals/NetTotal", FileID = PaymentRefNo, Value = DocumentTotals.NetTotal.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentTotals.TaxPayable < 0)
            listError.Add(new ValidationError { Description = $"Valor do imposto do recibo {PaymentRefNo} incorrecto.", Field = "Payment/DocumentTotals/TaxPayable", FileID = PaymentRefNo, Value = DocumentTotals.TaxPayable.ToString(), TypeofError = GetType(), UID = Pk });

        return [.. listError];
    }

    public ValidationError[] ValidateWithholdingTax()
    {
        if (WithholdingTax == null || WithholdingTax.Length == 0) return [];

        var listError = new List<ValidationError>();
        foreach (var wt in WithholdingTax)
        {
            if (wt.WithholdingTaxType == WithholdingTaxType.INVALIDO)
                listError.Add(new ValidationError { Description = $"Tipo de retenção na fonte inválido no recibo {PaymentRefNo}.", Field = "Payment/WithholdingTax/WithholdingTaxType", FileID = PaymentRefNo, Value = wt.WithholdingTaxType.ToString(), TypeofError = GetType(), UID = Pk });
            if (!string.IsNullOrEmpty(wt.WithholdingTaxDescription) && wt.WithholdingTaxDescription.Length > 60)
                listError.Add(new ValidationError { Description = $"Descrição da retenção na fonte demasiado longa no recibo {PaymentRefNo}.", Field = "Payment/WithholdingTax/WithholdingTaxDescription", FileID = PaymentRefNo, Value = wt.WithholdingTaxDescription, TypeofError = GetType(), UID = Pk });
            if (wt.WithholdingTaxAmount < 0)
                listError.Add(new ValidationError { Description = $"Montante da retenção na fonte negativo no recibo {PaymentRefNo}.", Field = "Payment/WithholdingTax/WithholdingTaxAmount", FileID = PaymentRefNo, Value = wt.WithholdingTaxAmount.ToString(), TypeofError = GetType(), UID = Pk });
        }
        return [.. listError];
    }
}

public partial class SourceDocumentsPaymentsPaymentLine : BaseData
{
    public string DocNo { get; set; }
    public string OriginatingON { get; set; }

    public ValidationError ValidateLineNumber(string SupPk)
    {
        if (string.IsNullOrEmpty(LineNumber) || !int.TryParse(LineNumber, out int num) || num <= 0)
        {
            return new ValidationError { Description = $"Número de linha incorrecto {LineNumber}.", Field = "Payment/Line/LineNumber", FileID = DocNo, Value = LineNumber, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateSettlementAmount(string SupPk)
    {
        if (SettlementAmount < 0)
        {
            return new ValidationError { Description = $"Montante do desconto incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/SettlementAmount", FileID = DocNo, Value = SettlementAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateItem(string SupPk)
    {
        if ((CreditAmount ?? DebitAmount ?? 0) < 0)
        {
            if (ItemElementName == ItemChoiceType8.CreditAmount)
                return new ValidationError { Description = $"Valor a crédito incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/CreditAmount", FileID = DocNo, Value = CreditAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
            if (ItemElementName == ItemChoiceType8.DebitAmount)
                return new ValidationError { Description = $"Valor a débito incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/DebitAmount", FileID = DocNo, Value = DebitAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateTaxExemptionReason(string SupPk)
    {
        var percentage = Tax?.TaxPercentage;
        if ((Tax != null && Tax.ItemElementName == ItemChoiceType.TaxPercentage && percentage == 0 && string.IsNullOrEmpty(TaxExemptionReason))
            || (!string.IsNullOrEmpty(TaxExemptionReason) && (TaxExemptionReason.Length < 6 || TaxExemptionReason.Length > 60)))
        {
            return new ValidationError { Description = $"Motivo da isenção de imposto incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/TaxExemptionReason", FileID = DocNo, Value = TaxExemptionReason, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError[] ValidateTaxExemption(string SupPk, DateTime transactionDate)
    {
        var errors = new List<ValidationError>();
        bool isIvaZeroOrExempt = Tax != null && Tax.TaxType == TaxType.IVA &&
            ((Tax.ItemElementName == ItemChoiceType.TaxPercentage && (Tax.TaxPercentage ?? 0) == 0) ||
             string.Equals(Tax.TaxCode, "ISE", StringComparison.OrdinalIgnoreCase));
        bool isTaxedAboveZero = Tax != null && Tax.ItemElementName == ItemChoiceType.TaxPercentage && (Tax.TaxPercentage ?? 0) > 0;

        PartialValidationPatterns.ValidateTaxExemptionFields(
            errors, GetType(), DocNo, LineNumber, Pk, SupPk, "Payment/Line",
            isIvaZeroOrExempt, isTaxedAboveZero, TaxExemptionReason, TaxExemptionCode, transactionDate);

        return [.. errors];
    }

    public ValidationError[] ValidateSourceDocumentID(string SupPk)
    {
        var listErro = new List<ValidationError>();

        if (SourceDocumentID == null || SourceDocumentID.Length == 0)
            listErro.Add(new ValidationError { Description = $"Referência ao documento de origem inexistente, linha nº {LineNumber}.", Field = "Payment/Line/SourceDocumentID", FileID = DocNo, TypeofError = GetType(), UID = Pk, SupUID = SupPk });

        if (SourceDocumentID != null && SourceDocumentID.Length > 0)
        {
            foreach (var doc in SourceDocumentID)
            {
                if (!string.IsNullOrEmpty(doc.Description) && doc.Description.Length > 100)
                    listErro.Add(new ValidationError { Description = $"Tamanho da descrição da linha incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/SourceDocumentID/Description", FileID = DocNo, Value = doc.Description, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
                if (doc.InvoiceDate == DateTime.MinValue)
                    listErro.Add(new ValidationError { Description = $"Data do documento de origem incorrecta, linha nº {LineNumber}.", Field = "Payment/Line/SourceDocumentID/InvoiceDate", FileID = DocNo, Value = doc.InvoiceDate.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(doc.OriginatingON) || doc.OriginatingON.Length > 60)
                    listErro.Add(new ValidationError { Description = $"Número do documento de origem incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/SourceDocumentID/OriginatingON", FileID = DocNo, Value = doc.OriginatingON, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            }
        }
        return [.. listErro];
    }

    public ValidationError[] ValidateTax(string SupPk)
    {
        var listErro = new List<ValidationError>();
        if (Tax != null)
        {
            if (Tax.TaxType == TaxType.INVALIDO)
                listErro.Add(new ValidationError { Description = $"Tipo de imposto inválido, linha nº {LineNumber}.", Field = "Payment/Line/Tax/TaxType", FileID = DocNo, Value = Tax.TaxType.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });

            decimal taxAmount = Tax.TaxAmount ?? Tax.Item;
            decimal taxPercentage = Tax.TaxPercentage ?? Tax.Item;

            if (Tax.ItemElementName == ItemChoiceType.TaxAmount && taxAmount < 0)
                listErro.Add(new ValidationError { Description = $"Montante do imposto incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/Tax/TaxAmount", FileID = DocNo, Value = taxAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (Tax.ItemElementName == ItemChoiceType.TaxPercentage && (taxPercentage < 0 || taxPercentage > 100))
                listErro.Add(new ValidationError { Description = $"Percentagem do imposto incorrecta, linha nº {LineNumber}.", Field = "Payment/Line/Tax/TaxPercentage", FileID = DocNo, Value = taxPercentage.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                listErro.Add(new ValidationError { Description = $"Código da taxa incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/Tax/TaxCode", FileID = DocNo, Value = Tax.TaxCode, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                listErro.Add(new ValidationError { Description = $"País ou região do imposto incorrecto, linha nº {LineNumber}.", Field = "Payment/Line/Tax/TaxCountryRegion", FileID = DocNo, Value = Tax.TaxCountryRegion, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
        }
        return [.. listErro];
    }
}

public partial class SourceDocumentsMovementOfGoodsStockMovement : BaseData
{
    public string HashTest { get; set; }
    public string CustomerID { get; set; }

    public ValidationError ValidateDocumentNumber()
    {
        if (string.IsNullOrEmpty(DocumentNumber) || DocumentNumber.Length > 60)
        {
            return new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "StockMovement/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, TypeofError = GetType(), UID = Pk };
        }
        if (!PartialValidationPatterns.DocumentNumberRegex().IsMatch(DocumentNumber))
        {
            return new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "StockMovement/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, TypeofError = GetType(), UID = Pk };
        }
        if (MovementType != MovementType.INVALIDO && !DocumentNumber.StartsWith($"{MovementType} ", StringComparison.Ordinal))
        {
            return new ValidationError { Description = $"O número do documento ({DocumentNumber}) deve iniciar pelo tipo ({MovementType}).", Field = "StockMovement/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateATCUD()
    {
        return Validations.ValidateAtcud(ATCUD, DocumentNumber, "StockMovement/ATCUD", GetType(), Pk);
    }

    public ValidationError ValidateHash()
    {
        bool allowZeroHash = DocumentStatus?.SourceBilling is SAFTPTSourceBilling.M or SAFTPTSourceBilling.I;
        if (string.IsNullOrEmpty(Hash) || (Hash.Length != 172 && !(allowZeroHash && Hash == "0")))
        {
            return new ValidationError { Description = $"Assinatura do documento {DocumentNumber} de tamanho incorrecto.", Field = "StockMovement/Hash", FileID = DocumentNumber, Value = Hash, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateHashControl()
    {
        if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 70)
        {
            return new ValidationError { Description = $"Versão da chave privada utilizada na assinatura do documento {DocumentNumber} incorrecta.", Field = "StockMovement/HashControl", FileID = DocumentNumber, Value = HashControl, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidatePeriod()
    {
        if (!string.IsNullOrEmpty(Period))
        {
            if (!int.TryParse(Period, out int periodo) || periodo < 1 || periodo > 12)
            {
                return new ValidationError { Description = $"Mês do período de tributação do documento {DocumentNumber} incorrecto.", Field = "StockMovement/Period", FileID = DocumentNumber, Value = Period, TypeofError = GetType(), UID = Pk };
            }
        }
        return null;
    }

    public ValidationError ValidateMovementDate()
    {
        if (MovementDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data de emissão do documento {DocumentNumber} incorrecta.", Field = "StockMovement/MovementDate", FileID = DocumentNumber, Value = MovementDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSystemEntryDate()
    {
        if (SystemEntryDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data da gravação do documento {DocumentNumber} incorrecta.", Field = "StockMovement/SystemEntryDate", FileID = DocumentNumber, Value = SystemEntryDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSourceID()
    {
        if (string.IsNullOrEmpty(SourceID) || SourceID.Length > 30)
        {
            return new ValidationError { Description = $"Utilizador que gerou o documento {DocumentNumber} incorrecto.", Field = "StockMovement/SourceID", FileID = DocumentNumber, Value = SourceID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateEACCode()
    {
        if (!string.IsNullOrEmpty(EACCode) && !PartialValidationPatterns.EacCodeRegex().IsMatch(EACCode))
        {
            return new ValidationError { Description = $"Código CAE do documento {DocumentNumber} inválido (deve ter 5 dígitos).", Field = "StockMovement/EACCode", FileID = DocumentNumber, Value = EACCode, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTransactionID()
    {
        if (!string.IsNullOrEmpty(TransactionID) && (TransactionID.Length > 70 || !PartialValidationPatterns.TransactionIdRegex().IsMatch(TransactionID)))
        {
            return new ValidationError { Description = $"Identificador da transacção do documento {DocumentNumber} incorrecto.", Field = "StockMovement/TransactionID", FileID = DocumentNumber, Value = TransactionID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateMovementEndTime()
    {
        if (MovementEndTimeSpecified && MovementEndTime == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data e hora de fim de transporte do documento {DocumentNumber} incorrecta.", Field = "StockMovement/MovementEndTime", FileID = DocumentNumber, Value = MovementEndTime.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateMovementStartTime()
    {
        if (MovementStartTimeSpecified && MovementStartTime == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data e hora de início de transporte do documento {DocumentNumber} incorrecta.", Field = "StockMovement/MovementStartTime", FileID = DocumentNumber, Value = MovementStartTime.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateMovementTimeRange()
    {
        if (MovementStartTimeSpecified && MovementEndTimeSpecified &&
            MovementStartTime != DateTime.MinValue && MovementEndTime != DateTime.MinValue &&
            MovementStartTime > MovementEndTime)
        {
            return new ValidationError
            {
                Description = $"Data de início de transporte posterior à data de fim de transporte no documento {DocumentNumber}.",
                Field = "StockMovement/MovementEndTime",
                FileID = DocumentNumber,
                Value = $"{MovementStartTime:yyyy-MM-ddTHH:mm:ss} > {MovementEndTime:yyyy-MM-ddTHH:mm:ss}",
                TypeofError = GetType(),
                UID = Pk
            };
        }
        return null;
    }

    public ValidationError[] ValidateDocumentStatus()
    {
        var listError = new List<ValidationError>();

        if (DocumentStatus == null)
        {
            listError.Add(new ValidationError { Description = $"Situação do documento {DocumentNumber} inexistente.", Field = "StockMovement/DocumentStatus", FileID = DocumentNumber, TypeofError = GetType(), UID = Pk });
            return [.. listError];
        }

        if (DocumentStatus.MovementStatus == MovementStatus.INVALIDO)
            listError.Add(new ValidationError { Description = $"Estado do documento {DocumentNumber} inválido.", Field = "StockMovement/DocumentStatus/MovementStatus", FileID = DocumentNumber, Value = DocumentStatus.MovementStatus.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.SourceBilling == SAFTPTSourceBilling.INVALIDO)
            listError.Add(new ValidationError { Description = $"Origem do documento {DocumentNumber} inválida.", Field = "StockMovement/DocumentStatus/SourceBilling", FileID = DocumentNumber, Value = DocumentStatus.SourceBilling.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.MovementStatusDate == DateTime.MinValue)
            listError.Add(new ValidationError { Description = $"Data e hora do estado atual do documento {DocumentNumber} incorrecta.", Field = "StockMovement/DocumentStatus/MovementStatusDate", FileID = DocumentNumber, Value = DocumentStatus.MovementStatusDate.ToString(), TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(DocumentStatus.Reason) && DocumentStatus.Reason.Length > 50)
            listError.Add(new ValidationError { Description = $"Tamanho do motivo da alteração de estado {DocumentNumber} incorrecto.", Field = "StockMovement/DocumentStatus/Reason", FileID = DocumentNumber, Value = DocumentStatus.Reason, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
            listError.Add(new ValidationError { Description = $"Utilizador responsável pelo estado atual do documento {DocumentNumber} incorrecto.", Field = "StockMovement/DocumentStatus/SourceID", FileID = DocumentNumber, Value = DocumentStatus.SourceID, TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.MovementStatus == MovementStatus.A && string.IsNullOrWhiteSpace(DocumentStatus.Reason))
            listError.Add(new ValidationError { Description = $"Documento anulado {DocumentNumber} sem motivo de alteração de estado preenchido.", Field = "StockMovement/DocumentStatus/Reason", FileID = DocumentNumber, TypeofError = GetType(), ValidationType = ValidationErrorType.WARNING, UID = Pk });

        return [.. listError];
    }

    public ValidationError[] ValidateShipTo()
    {
        return ValidateShipping(ShipTo, "StockMovement/ShipTo", "local de descarga");
    }

    public ValidationError[] ValidateShipFrom()
    {
        return ValidateShipping(ShipFrom, "StockMovement/ShipFrom", "local de carga");
    }

    public ValidationError[] ValidateShipping(ShippingPointStructure ship, string path, string localLabel = "local de transporte")
    {
        var listError = new List<ValidationError>();
        if (ship == null) return [];

        var address = ship.Address;
        if (address != null)
        {
            if (string.IsNullOrEmpty(address.AddressDetail) || address.AddressDetail.Length > 210)
                listError.Add(new ValidationError { Description = $"Tamanho da morada detalhada do documento {DocumentNumber} do {localLabel} incorrecto.", Field = $"{path}/AddressDetail", FileID = DocumentNumber, Value = address.AddressDetail, TypeofError = GetType(), UID = Pk });
            if (!string.IsNullOrEmpty(address.BuildingNumber) && address.BuildingNumber.Length > 10)
                listError.Add(new ValidationError { Description = $"Tamanho do número de polícia do documento {DocumentNumber} do {localLabel} incorrecto.", Field = $"{path}/BuildingNumber", FileID = DocumentNumber, Value = address.BuildingNumber, TypeofError = GetType(), UID = Pk });
            if (string.IsNullOrEmpty(address.City) || address.City.Length > 50)
                listError.Add(new ValidationError { Description = $"Tamanho da localidade do documento {DocumentNumber} do {localLabel} incorrecto.", Field = $"{path}/City", FileID = DocumentNumber, Value = address.City, TypeofError = GetType(), UID = Pk });
            if (string.IsNullOrEmpty(address.Country) || (address.Country.Length != 2 && address.Country != "Desconhecido"))
                listError.Add(new ValidationError { Description = $"País do documento {DocumentNumber} do {localLabel} incorrecto.", Field = $"{path}/Country", FileID = DocumentNumber, Value = address.Country, TypeofError = GetType(), UID = Pk });
            if (string.IsNullOrEmpty(address.PostalCode) || address.PostalCode.Length > 20)
                listError.Add(new ValidationError { Description = $"Tamanho do código postal do documento {DocumentNumber} do {localLabel} incorrecto.", Field = $"{path}/PostalCode", FileID = DocumentNumber, Value = address.PostalCode, TypeofError = GetType(), UID = Pk });
            if (!string.IsNullOrEmpty(address.Region) && address.Region.Length > 50)
                listError.Add(new ValidationError { Description = $"Tamanho do distrito do documento {DocumentNumber} do {localLabel} incorrecto.", Field = $"{path}/Region", FileID = DocumentNumber, Value = address.Region, TypeofError = GetType(), UID = Pk });
            if (!string.IsNullOrEmpty(address.StreetName) && address.StreetName.Length > 200)
                listError.Add(new ValidationError { Description = $"Tamanho do nome da rua do documento {DocumentNumber} do {localLabel} incorrecto.", Field = $"{path}/StreetName", FileID = DocumentNumber, Value = address.StreetName, TypeofError = GetType(), UID = Pk });
        }

        if (ship.DeliveryDateSpecified && ship.DeliveryDate == DateTime.MinValue)
            listError.Add(new ValidationError { Description = $"Data de entrega/receção do documento {DocumentNumber} incorrecta.", Field = $"{path}/DeliveryDate", FileID = DocumentNumber, Value = ship.DeliveryDate.ToString(), TypeofError = GetType(), UID = Pk });

        return [.. listError];
    }

    public ValidationError[] ValidateDocumentTotals()
    {
        var listError = new List<ValidationError>();

        if (DocumentTotals == null)
        {
            listError.Add(new ValidationError { Description = $"Totais do documento {DocumentNumber} inexistentes.", Field = "StockMovement/DocumentTotals", FileID = DocumentNumber, TypeofError = GetType(), UID = Pk });
            return [.. listError];
        }

        if (DocumentTotals.Currency != null)
        {
            if (DocumentTotals.Currency.CurrencyAmount < 0)
                listError.Add(new ValidationError { Description = $"Valor total em moeda estrangeira do documento {DocumentNumber} incorrecto.", Field = "StockMovement/DocumentTotals/Currency/CurrencyAmount", FileID = DocumentNumber, Value = DocumentTotals.Currency.CurrencyAmount.ToString(), TypeofError = GetType(), UID = Pk });
            if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length != 3)
                listError.Add(new ValidationError { Description = $"Código de moeda do documento {DocumentNumber} incorrecto.", Field = "StockMovement/DocumentTotals/Currency/CurrencyCode", FileID = DocumentNumber, Value = DocumentTotals.Currency.CurrencyCode, TypeofError = GetType(), UID = Pk });
            if (DocumentTotals.Currency.ExchangeRate < 0)
                listError.Add(new ValidationError { Description = $"Taxa de câmbio do documento {DocumentNumber} incorrecta.", Field = "StockMovement/DocumentTotals/Currency/ExchangeRate", FileID = DocumentNumber, Value = DocumentTotals.Currency.ExchangeRate.ToString(), TypeofError = GetType(), UID = Pk });
        }

        if (DocumentTotals.GrossTotal < 0)
            listError.Add(new ValidationError { Description = $"Total do documento {DocumentNumber} incorrecto.", Field = "StockMovement/DocumentTotals/GrossTotal", FileID = DocumentNumber, Value = DocumentTotals.GrossTotal.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentTotals.NetTotal < 0)
            listError.Add(new ValidationError { Description = $"Total do documento {DocumentNumber} sem impostos incorrecto.", Field = "StockMovement/DocumentTotals/NetTotal", FileID = DocumentNumber, Value = DocumentTotals.NetTotal.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentTotals.TaxPayable < 0)
            listError.Add(new ValidationError { Description = $"Valor do imposto a pagar do documento {DocumentNumber} incorrecto.", Field = "StockMovement/DocumentTotals/TaxPayable", FileID = DocumentNumber, Value = DocumentTotals.TaxPayable.ToString(), TypeofError = GetType(), UID = Pk });

        return [.. listError];
    }
}

public partial class SourceDocumentsMovementOfGoodsStockMovementLine : BaseData
{
    public string DocNo { get; set; }

    public ValidationError ValidateLineNumber(string SupPk)
    {
        if (string.IsNullOrEmpty(LineNumber) || !int.TryParse(LineNumber, out int num) || num <= 0)
        {
            return new ValidationError { Description = $"Número de linha incorrecto {LineNumber}.", Field = "StockMovement/Line/LineNumber", FileID = DocNo, Value = LineNumber, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateProductCode(string SupPk)
    {
        if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 60)
        {
            return new ValidationError { Description = $"Identificador do produto ou serviço incorrecto, linha nº {LineNumber}.", Field = "StockMovement/Line/ProductCode", FileID = DocNo, Value = ProductCode, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateProductDescription(string SupPk)
    {
        if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length < 2 || ProductDescription.Length > 200)
        {
            return new ValidationError { Description = $"Descrição do produto ou serviço incorrecta, linha nº {LineNumber}.", Field = "StockMovement/Line/ProductDescription", FileID = DocNo, Value = ProductDescription, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateQuantity(string SupPk)
    {
        if (Quantity <= 0)
        {
            return new ValidationError { Description = $"Quantidade incorrecta, linha nº {LineNumber}.", Field = "StockMovement/Line/Quantity", FileID = DocNo, Value = Quantity.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateUnitOfMeasure(string SupPk)
    {
        if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
        {
            return new ValidationError { Description = $"Unidade de medida incorrecta, linha nº {LineNumber}.", Field = "StockMovement/Line/UnitOfMeasure", FileID = DocNo, Value = UnitOfMeasure, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateUnitPrice(string SupPk)
    {
        if (UnitPrice < 0)
        {
            return new ValidationError { Description = $"Preço unitário incorrecto, linha nº {LineNumber}.", Field = "StockMovement/Line/UnitPrice", FileID = DocNo, Value = UnitPrice.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateDescription(string SupPk)
    {
        if (string.IsNullOrEmpty(Description) || Description.Length > 200)
        {
            return new ValidationError { Description = $"Descrição da linha nº {LineNumber} incorrecta.", Field = "StockMovement/Line/Description", FileID = DocNo, Value = Description, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateSettlementAmount(string SupPk)
    {
        if (SettlementAmount < 0)
        {
            return new ValidationError { Description = $"Montante do desconto na linha nº {LineNumber} incorrecto.", Field = "StockMovement/Line/SettlementAmount", FileID = DocNo, Value = SettlementAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError[] ValidateOrderReferences(string SupPk)
    {
        var listError = new List<ValidationError>();
        if (OrderReferences != null && OrderReferences.Length > 0)
        {
            foreach (var referencia in OrderReferences)
            {
                if (!string.IsNullOrEmpty(referencia.OriginatingON) && referencia.OriginatingON.Length > 60)
                    listError.Add(new ValidationError { Description = $"Tamanho do número do documento de origem na linha nº {LineNumber} incorrecto.", Field = "StockMovement/Line/OrderReferences/OriginatingON", FileID = DocNo, Value = referencia.OriginatingON, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
                if (referencia.OrderDateSpecified && referencia.OrderDate == DateTime.MinValue)
                    listError.Add(new ValidationError { Description = $"Data do documento de origem na linha nº {LineNumber} incorrecta.", Field = "StockMovement/Line/OrderReferences/OrderDate", FileID = DocNo, Value = referencia.OrderDate.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            }
        }
        return [.. listError];
    }

    public ValidationError[] ValidateTax(string SupPk)
    {
        var listError = new List<ValidationError>();
        if (Tax != null)
        {
            if (Tax.TaxType == SAFTPTMovementTaxType.INVALIDO)
                listError.Add(new ValidationError { Description = $"Tipo de imposto na linha nº {LineNumber} inválido.", Field = "StockMovement/Line/Tax/TaxType", FileID = DocNo, Value = Tax.TaxType.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100)
                listError.Add(new ValidationError { Description = $"Percentagem da taxa do imposto na linha nº {LineNumber} incorrecta.", Field = "StockMovement/Line/Tax/TaxPercentage", FileID = DocNo, Value = Tax.TaxPercentage.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                listError.Add(new ValidationError { Description = $"Código da taxa na linha nº {LineNumber} incorrecto.", Field = "StockMovement/Line/Tax/TaxCode", FileID = DocNo, Value = Tax.TaxCode, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                listError.Add(new ValidationError { Description = $"País ou região do imposto na linha nº {LineNumber} incorrecto.", Field = "StockMovement/Line/Tax/TaxCountryRegion", FileID = DocNo, Value = Tax.TaxCountryRegion, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
        }
        return [.. listError];
    }

    public ValidationError[] ValidateTaxExemption(string SupPk, DateTime docDate)
    {
        var errors = new List<ValidationError>();
        bool isIvaZeroOrExempt = Tax != null && Tax.TaxType == SAFTPTMovementTaxType.IVA &&
            (Tax.TaxPercentage == 0 || string.Equals(Tax.TaxCode, "ISE", StringComparison.OrdinalIgnoreCase));
        bool isTaxedAboveZero = Tax != null && Tax.TaxPercentage > 0;

        PartialValidationPatterns.ValidateTaxExemptionFields(
            errors, GetType(), DocNo, LineNumber, Pk, SupPk, "StockMovement/Line",
            isIvaZeroOrExempt, isTaxedAboveZero, TaxExemptionReason, TaxExemptionCode, docDate);

        return [.. errors];
    }
}

public partial class SourceDocumentsWorkingDocumentsWorkDocument : BaseData
{
    public string HashTest { get; set; }

    public ValidationError ValidateDocumentNumber()
    {
        if (string.IsNullOrEmpty(DocumentNumber) || DocumentNumber.Length > 60)
        {
            return new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "WorkDocument/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, TypeofError = GetType(), UID = Pk };
        }
        if (!PartialValidationPatterns.DocumentNumberRegex().IsMatch(DocumentNumber))
        {
            return new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "WorkDocument/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, TypeofError = GetType(), UID = Pk };
        }
        if (WorkType != WorkType.INVALIDO && !DocumentNumber.StartsWith($"{WorkType} ", StringComparison.Ordinal))
        {
            return new ValidationError { Description = $"O número do documento ({DocumentNumber}) deve iniciar pelo tipo ({WorkType}).", Field = "WorkDocument/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateATCUD()
    {
        return Validations.ValidateAtcud(ATCUD, DocumentNumber, "WorkDocument/ATCUD", GetType(), Pk);
    }

    public ValidationError ValidateHash()
    {
        bool allowZeroHash = DocumentStatus?.SourceBilling is SAFTPTSourceBilling.M or SAFTPTSourceBilling.I;
        if (string.IsNullOrEmpty(Hash) || (Hash.Length != 172 && !(allowZeroHash && Hash == "0")))
        {
            return new ValidationError { Description = $"Assinatura do documento {DocumentNumber} de tamanho incorrecto.", Field = "WorkDocument/Hash", FileID = DocumentNumber, Value = Hash, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateHashControl()
    {
        if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 70)
        {
            return new ValidationError { Description = $"Versão da chave privada utilizada na assinatura do documento {DocumentNumber} incorrecta.", Field = "WorkDocument/HashControl", FileID = DocumentNumber, Value = HashControl, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidatePeriod()
    {
        if (!string.IsNullOrEmpty(Period))
        {
            if (!int.TryParse(Period, out int periodo) || periodo < 1 || periodo > 12)
            {
                return new ValidationError { Description = $"Mês do período de tributação do documento {DocumentNumber} incorrecto.", Field = "WorkDocument/Period", FileID = DocumentNumber, Value = Period, TypeofError = GetType(), UID = Pk };
            }
        }
        return null;
    }

    public ValidationError ValidateWorkDate()
    {
        if (WorkDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data de emissão do documento {DocumentNumber} incorrecta.", Field = "WorkDocument/WorkDate", FileID = DocumentNumber, Value = WorkDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSystemEntryDate()
    {
        if (SystemEntryDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data da gravação do documento {DocumentNumber} incorrecta.", Field = "WorkDocument/SystemEntryDate", FileID = DocumentNumber, Value = SystemEntryDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCustomerID()
    {
        if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
        {
            return new ValidationError { Description = $"Chave única da tabela de clientes no documento {DocumentNumber} incorrecta.", Field = "WorkDocument/CustomerID", FileID = DocumentNumber, Value = CustomerID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSourceID()
    {
        if (string.IsNullOrEmpty(SourceID) || SourceID.Length > 30)
        {
            return new ValidationError { Description = $"Utilizador que gerou o documento {DocumentNumber} incorrecto.", Field = "WorkDocument/SourceID", FileID = DocumentNumber, Value = SourceID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTransactionID()
    {
        if (!string.IsNullOrEmpty(TransactionID) && (TransactionID.Length > 70 || !PartialValidationPatterns.TransactionIdRegex().IsMatch(TransactionID)))
        {
            return new ValidationError { Description = $"Identificador da transacção do documento {DocumentNumber} incorrecto.", Field = "WorkDocument/TransactionID", FileID = DocumentNumber, Value = TransactionID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateEACCode()
    {
        if (!string.IsNullOrEmpty(EACCode) && !PartialValidationPatterns.EacCodeRegex().IsMatch(EACCode))
        {
            return new ValidationError { Description = $"Código CAE do documento {DocumentNumber} inválido (deve ter 5 dígitos).", Field = "WorkDocument/EACCode", FileID = DocumentNumber, Value = EACCode, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError[] ValidateDocumentStatus()
    {
        var listError = new List<ValidationError>();

        if (DocumentStatus == null)
        {
            listError.Add(new ValidationError { Description = $"Situação do documento {DocumentNumber} inexistente.", Field = "WorkDocument/DocumentStatus", FileID = DocumentNumber, TypeofError = GetType(), UID = Pk });
            return [.. listError];
        }

        if (DocumentStatus.WorkStatus == WorkStatus.INVALIDO)
            listError.Add(new ValidationError { Description = $"Estado do documento {DocumentNumber} inválido.", Field = "WorkDocument/DocumentStatus/WorkStatus", FileID = DocumentNumber, Value = DocumentStatus.WorkStatus.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.SourceBilling == SAFTPTSourceBilling.INVALIDO)
            listError.Add(new ValidationError { Description = $"Origem do documento {DocumentNumber} inválida.", Field = "WorkDocument/DocumentStatus/SourceBilling", FileID = DocumentNumber, Value = DocumentStatus.SourceBilling.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.WorkStatusDate == DateTime.MinValue)
            listError.Add(new ValidationError { Description = $"Data e hora do estado atual do documento {DocumentNumber} incorrecta.", Field = "WorkDocument/DocumentStatus/WorkStatusDate", FileID = DocumentNumber, Value = DocumentStatus.WorkStatusDate.ToString(), TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(DocumentStatus.Reason) && DocumentStatus.Reason.Length > 50)
            listError.Add(new ValidationError { Description = $"Tamanho do motivo da alteração de estado {DocumentNumber} incorrecto.", Field = "WorkDocument/DocumentStatus/Reason", FileID = DocumentNumber, Value = DocumentStatus.Reason, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
            listError.Add(new ValidationError { Description = $"Utilizador responsável pelo estado atual do documento {DocumentNumber} incorrecto.", Field = "WorkDocument/DocumentStatus/SourceID", FileID = DocumentNumber, Value = DocumentStatus.SourceID, TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.WorkStatus == WorkStatus.A && string.IsNullOrWhiteSpace(DocumentStatus.Reason))
            listError.Add(new ValidationError { Description = $"Documento anulado {DocumentNumber} sem motivo de alteração de estado preenchido.", Field = "WorkDocument/DocumentStatus/Reason", FileID = DocumentNumber, TypeofError = GetType(), ValidationType = ValidationErrorType.WARNING, UID = Pk });

        return [.. listError];
    }

    public ValidationError[] ValidateDocumentTotals()
    {
        var listError = new List<ValidationError>();

        if (DocumentTotals == null)
        {
            listError.Add(new ValidationError { Description = $"Totais do documento {DocumentNumber} inexistentes.", Field = "WorkDocument/DocumentTotals", FileID = DocumentNumber, TypeofError = GetType(), UID = Pk });
            return [.. listError];
        }

        if (DocumentTotals.Currency != null)
        {
            if (DocumentTotals.Currency.CurrencyAmount < 0)
                listError.Add(new ValidationError { Description = $"Valor total em moeda estrangeira do documento {DocumentNumber} incorrecto.", Field = "WorkDocument/DocumentTotals/Currency/CurrencyAmount", FileID = DocumentNumber, Value = DocumentTotals.Currency.CurrencyAmount.ToString(), TypeofError = GetType(), UID = Pk });
            if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length != 3)
                listError.Add(new ValidationError { Description = $"Código de moeda do documento {DocumentNumber} incorrecto.", Field = "WorkDocument/DocumentTotals/Currency/CurrencyCode", FileID = DocumentNumber, Value = DocumentTotals.Currency.CurrencyCode, TypeofError = GetType(), UID = Pk });
            if (DocumentTotals.Currency.ExchangeRate < 0)
                listError.Add(new ValidationError { Description = $"Taxa de câmbio do documento {DocumentNumber} incorrecta.", Field = "WorkDocument/DocumentTotals/Currency/ExchangeRate", FileID = DocumentNumber, Value = DocumentTotals.Currency.ExchangeRate.ToString(), TypeofError = GetType(), UID = Pk });
        }

        if (DocumentTotals.GrossTotal < 0)
            listError.Add(new ValidationError { Description = $"Total do documento {DocumentNumber} incorrecto.", Field = "WorkDocument/DocumentTotals/GrossTotal", FileID = DocumentNumber, Value = DocumentTotals.GrossTotal.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentTotals.NetTotal < 0)
            listError.Add(new ValidationError { Description = $"Total do documento {DocumentNumber} sem impostos incorrecto.", Field = "WorkDocument/DocumentTotals/NetTotal", FileID = DocumentNumber, Value = DocumentTotals.NetTotal.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentTotals.TaxPayable < 0)
            listError.Add(new ValidationError { Description = $"Valor do imposto a pagar do documento {DocumentNumber} incorrecto.", Field = "WorkDocument/DocumentTotals/TaxPayable", FileID = DocumentNumber, Value = DocumentTotals.TaxPayable.ToString(), TypeofError = GetType(), UID = Pk });

        return [.. listError];
    }
}

public partial class SourceDocumentsWorkingDocumentsWorkDocumentLine : BaseData
{
    public string DocNo { get; set; }

    public ValidationError ValidateLineNumber(string SupPk)
    {
        if (string.IsNullOrEmpty(LineNumber) || !int.TryParse(LineNumber, out int num) || num <= 0)
        {
            return new ValidationError { Description = $"Número de linha incorrecto {LineNumber}.", Field = "WorkDocument/Line/LineNumber", FileID = DocNo, Value = LineNumber, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateProductCode(string SupPk)
    {
        if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 60)
        {
            return new ValidationError { Description = $"Identificador do produto ou serviço incorrecto, linha nº {LineNumber}.", Field = "WorkDocument/Line/ProductCode", FileID = DocNo, Value = ProductCode, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateProductDescription(string SupPk)
    {
        if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length < 2 || ProductDescription.Length > 200)
        {
            return new ValidationError { Description = $"Descrição do produto ou serviço incorrecta, linha nº {LineNumber}.", Field = "WorkDocument/Line/ProductDescription", FileID = DocNo, Value = ProductDescription, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateQuantity(string SupPk)
    {
        if (Quantity <= 0)
        {
            return new ValidationError { Description = $"Quantidade incorrecta, linha nº {LineNumber}.", Field = "WorkDocument/Line/Quantity", FileID = DocNo, Value = Quantity.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateUnitOfMeasure(string SupPk)
    {
        if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
        {
            return new ValidationError { Description = $"Unidade de medida incorrecta, linha nº {LineNumber}.", Field = "WorkDocument/Line/UnitOfMeasure", FileID = DocNo, Value = UnitOfMeasure, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateUnitPrice(string SupPk)
    {
        if (UnitPrice < 0)
        {
            return new ValidationError { Description = $"Preço unitário incorrecto, linha nº {LineNumber}.", Field = "WorkDocument/Line/UnitPrice", FileID = DocNo, Value = UnitPrice.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateDescription(string SupPk)
    {
        if (string.IsNullOrEmpty(Description) || Description.Length > 200)
        {
            return new ValidationError { Description = $"Descrição da linha nº {LineNumber} incorrecta.", Field = "WorkDocument/Line/Description", FileID = DocNo, Value = Description, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateSettlementAmount(string SupPk)
    {
        if (SettlementAmount < 0)
        {
            return new ValidationError { Description = $"Montante do desconto na linha nº {LineNumber} incorrecto.", Field = "WorkDocument/Line/SettlementAmount", FileID = DocNo, Value = SettlementAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateTaxBase(string SupPk)
    {
        if (TaxBase < 0)
        {
            return new ValidationError { Description = $"Base tributável na linha nº {LineNumber} incorrecta.", Field = "WorkDocument/Line/TaxBase", FileID = DocNo, Value = TaxBase.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateTaxPointDate(string SupPk)
    {
        // Opcional no XSD de WorkDocument/Line (minOccurs="0"); erros de formato são reportados no parser.
        return null;
    }

    public ValidationError[] ValidateOrderReferences(string SupPk)
    {
        var listError = new List<ValidationError>();
        if (OrderReferences != null && OrderReferences.Length > 0)
        {
            foreach (var referencia in OrderReferences)
            {
                if (!string.IsNullOrEmpty(referencia.OriginatingON) && referencia.OriginatingON.Length > 60)
                    listError.Add(new ValidationError { Description = $"Tamanho do número do documento de origem na linha nº {LineNumber} incorrecto.", Field = "WorkDocument/Line/OrderReferences/OriginatingON", FileID = DocNo, Value = referencia.OriginatingON, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
                if (referencia.OrderDateSpecified && referencia.OrderDate == DateTime.MinValue)
                    listError.Add(new ValidationError { Description = $"Data do documento de origem na linha nº {LineNumber} incorrecta.", Field = "WorkDocument/Line/OrderReferences/OrderDate", FileID = DocNo, Value = referencia.OrderDate.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            }
        }
        return [.. listError];
    }

    public ValidationError[] ValidateTax(string SupPk)
    {
        var listError = new List<ValidationError>();
        if (Tax != null)
        {
            if (Tax.TaxType == TaxType.INVALIDO)
                listError.Add(new ValidationError { Description = $"Tipo de imposto na linha nº {LineNumber} inválido.", Field = "WorkDocument/Line/Tax/TaxType", FileID = DocNo, Value = Tax.TaxType.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (Tax.ItemElementName == ItemChoiceType1.TaxAmount && Tax.TaxAmount < 0)
                listError.Add(new ValidationError { Description = $"Montante do imposto na linha nº {LineNumber} incorrecto.", Field = "WorkDocument/Line/Tax/TaxAmount", FileID = DocNo, Value = Tax.TaxAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (Tax.ItemElementName == ItemChoiceType1.TaxPercentage && (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100))
                listError.Add(new ValidationError { Description = $"Percentagem da taxa do imposto na linha nº {LineNumber} incorrecta.", Field = "WorkDocument/Line/Tax/TaxPercentage", FileID = DocNo, Value = Tax.TaxPercentage.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                listError.Add(new ValidationError { Description = $"Código da taxa na linha nº {LineNumber} incorrecto.", Field = "WorkDocument/Line/Tax/TaxCode", FileID = DocNo, Value = Tax.TaxCode, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                listError.Add(new ValidationError { Description = $"País ou região do imposto na linha nº {LineNumber} incorrecto.", Field = "WorkDocument/Line/Tax/TaxCountryRegion", FileID = DocNo, Value = Tax.TaxCountryRegion, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
        }
        return [.. listError];
    }

    public ValidationError[] ValidateTaxExemption(string SupPk, DateTime docDate)
    {
        var errors = new List<ValidationError>();
        bool isIvaZeroOrExempt = Tax != null && Tax.TaxType == TaxType.IVA &&
            ((Tax.ItemElementName == ItemChoiceType1.TaxPercentage && Tax.TaxPercentage.GetValueOrDefault(0) == 0) ||
             string.Equals(Tax.TaxCode, "ISE", StringComparison.OrdinalIgnoreCase));
        bool isTaxedAboveZero = Tax != null && Tax.ItemElementName == ItemChoiceType1.TaxPercentage && Tax.TaxPercentage.GetValueOrDefault(0) > 0;

        PartialValidationPatterns.ValidateTaxExemptionFields(
            errors, GetType(), DocNo, LineNumber, Pk, SupPk, "WorkDocument/Line",
            isIvaZeroOrExempt, isTaxedAboveZero, TaxExemptionReason, TaxExemptionCode, docDate);

        return [.. errors];
    }
}

public partial class SourceDocumentsSalesInvoicesInvoice : BaseData
{
    public string HashTest { get; set; }
    public Customer Customer { get; set; }

    public ValidationError ValidateATCUD()
    {
        return Validations.ValidateAtcud(ATCUD, InvoiceNo, "Invoice/ATCUD", GetType(), Pk);
    }

    public ValidationError ValidateInvoiceNo()
    {
        if (string.IsNullOrEmpty(InvoiceNo) || InvoiceNo.Length > 60)
        {
            return new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "Invoice/InvoiceNo", FileID = InvoiceNo, Value = InvoiceNo, TypeofError = GetType(), UID = Pk };
        }
        if (!PartialValidationPatterns.DocumentNumberRegex().IsMatch(InvoiceNo))
        {
            return new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "Invoice/InvoiceNo", FileID = InvoiceNo, Value = InvoiceNo, TypeofError = GetType(), UID = Pk };
        }
        if (InvoiceType != InvoiceType.INVALIDO && !InvoiceNo.StartsWith($"{InvoiceType} ", StringComparison.Ordinal))
        {
            return new ValidationError { Description = $"O número da fatura ({InvoiceNo}) deve iniciar pelo tipo ({InvoiceType}).", Field = "Invoice/InvoiceNo", FileID = InvoiceNo, Value = InvoiceNo, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateHash()
    {
        bool allowZeroHash = DocumentStatus?.SourceBilling is SAFTPTSourceBilling.M or SAFTPTSourceBilling.I;
        if (string.IsNullOrEmpty(Hash) || (Hash.Length != 172 && !(allowZeroHash && Hash == "0")))
        {
            return new ValidationError { Description = $"Assinatura do documento {InvoiceNo} de tamanho incorrecto.", Field = "Invoice/Hash", FileID = InvoiceNo, Value = Hash, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateHashControl()
    {
        if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 70)
        {
            return new ValidationError { Description = $"Versão da chave privada utilizada na assinatura do documento {InvoiceNo} incorrecta.", Field = "Invoice/HashControl", FileID = InvoiceNo, Value = HashControl, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidatePeriod()
    {
        if (!string.IsNullOrEmpty(Period))
        {
            if (!int.TryParse(Period, out int periodo) || periodo < 1 || periodo > 12)
            {
                return new ValidationError { Description = $"Mês do período de tributação do documento {InvoiceNo} incorrecto.", Field = "Invoice/Period", FileID = InvoiceNo, Value = Period, TypeofError = GetType(), UID = Pk };
            }
        }
        return null;
    }

    public ValidationError ValidateInvoiceDate()
    {
        if (InvoiceDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data de emissão do documento {InvoiceNo} incorrecta.", Field = "Invoice/InvoiceDate", FileID = InvoiceNo, Value = InvoiceDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSystemEntryDate()
    {
        if (SystemEntryDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data da gravação do documento {InvoiceNo} incorrecta.", Field = "Invoice/SystemEntryDate", FileID = InvoiceNo, Value = SystemEntryDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTransactionID()
    {
        if (!string.IsNullOrEmpty(TransactionID) && (TransactionID.Length > 70 || !PartialValidationPatterns.TransactionIdRegex().IsMatch(TransactionID)))
        {
            return new ValidationError { Description = $"Identificador da transacção do documento {InvoiceNo} incorrecto.", Field = "Invoice/TransactionID", FileID = InvoiceNo, Value = TransactionID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCustomerID()
    {
        if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
        {
            return new ValidationError { Description = $"Chave única da tabela de clientes no documento {InvoiceNo} incorrecta.", Field = "Invoice/CustomerID", FileID = InvoiceNo, Value = CustomerID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSourceID()
    {
        if (string.IsNullOrEmpty(SourceID) || SourceID.Length > 30)
        {
            return new ValidationError { Description = $"Utilizador que gerou o documento {InvoiceNo} incorrecto.", Field = "Invoice/SourceID", FileID = InvoiceNo, Value = SourceID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateEACCode()
    {
        if (!string.IsNullOrEmpty(EACCode) && !PartialValidationPatterns.EacCodeRegex().IsMatch(EACCode))
        {
            return new ValidationError { Description = $"Código CAE do documento {InvoiceNo} inválido (deve ter 5 dígitos).", Field = "Invoice/EACCode", FileID = InvoiceNo, Value = EACCode, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateMovementEndTime()
    {
        if (MovementEndTimeSpecified && MovementEndTime == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data e hora de fim de transporte do documento {InvoiceNo} incorrecta.", Field = "Invoice/MovementEndTime", FileID = InvoiceNo, Value = MovementEndTime.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateMovementStartTime()
    {
        if (MovementStartTimeSpecified && MovementStartTime == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data e hora de início de transporte do documento {InvoiceNo} incorrecta.", Field = "Invoice/MovementStartTime", FileID = InvoiceNo, Value = MovementStartTime.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateMovementTimeRange()
    {
        if (MovementStartTimeSpecified && MovementEndTimeSpecified &&
            MovementStartTime != DateTime.MinValue && MovementEndTime != DateTime.MinValue &&
            MovementStartTime > MovementEndTime)
        {
            return new ValidationError
            {
                Description = $"Data de início de transporte posterior à data de fim de transporte no documento {InvoiceNo}.",
                Field = "Invoice/MovementEndTime",
                FileID = InvoiceNo,
                Value = $"{MovementStartTime:yyyy-MM-ddTHH:mm:ss} > {MovementEndTime:yyyy-MM-ddTHH:mm:ss}",
                TypeofError = GetType(),
                UID = Pk
            };
        }
        return null;
    }

    public ValidationError[] ValidateDocumentStatus()
    {
        var listError = new List<ValidationError>();

        if (DocumentStatus == null)
        {
            listError.Add(new ValidationError { Description = $"Situação do documento {InvoiceNo} inexistente.", Field = "Invoice/DocumentStatus", FileID = InvoiceNo, TypeofError = GetType(), UID = Pk });
            return [.. listError];
        }

        if (DocumentStatus.InvoiceStatus == InvoiceStatus.INVALIDO)
            listError.Add(new ValidationError { Description = $"Estado do documento {InvoiceNo} inválido.", Field = "Invoice/DocumentStatus/InvoiceStatus", FileID = InvoiceNo, Value = DocumentStatus.InvoiceStatus.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.SourceBilling == SAFTPTSourceBilling.INVALIDO)
            listError.Add(new ValidationError { Description = $"Origem do documento {InvoiceNo} inválida.", Field = "Invoice/DocumentStatus/SourceBilling", FileID = InvoiceNo, Value = DocumentStatus.SourceBilling.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.InvoiceStatusDate == DateTime.MinValue)
            listError.Add(new ValidationError { Description = $"Data e hora do estado atual do documento {InvoiceNo} incorrecta.", Field = "Invoice/DocumentStatus/InvoiceStatusDate", FileID = InvoiceNo, Value = DocumentStatus.InvoiceStatusDate.ToString(), TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(DocumentStatus.Reason) && DocumentStatus.Reason.Length > 50)
            listError.Add(new ValidationError { Description = $"Tamanho do motivo da alteração de estado {InvoiceNo} incorrecto.", Field = "Invoice/DocumentStatus/Reason", FileID = InvoiceNo, Value = DocumentStatus.Reason, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
            listError.Add(new ValidationError { Description = $"Utilizador responsável pelo estado atual do documento {InvoiceNo} incorrecto.", Field = "Invoice/DocumentStatus/SourceID", FileID = InvoiceNo, Value = DocumentStatus.SourceID, TypeofError = GetType(), UID = Pk });
        if (DocumentStatus.InvoiceStatus == InvoiceStatus.A && string.IsNullOrWhiteSpace(DocumentStatus.Reason))
            listError.Add(new ValidationError { Description = $"Documento anulado {InvoiceNo} sem motivo de alteração de estado preenchido.", Field = "Invoice/DocumentStatus/Reason", FileID = InvoiceNo, TypeofError = GetType(), ValidationType = ValidationErrorType.WARNING, UID = Pk });

        return [.. listError];
    }

    public ValidationError[] ValidateSpecialRegimes()
    {
        var listError = new List<ValidationError>();

        if (SpecialRegimes != null)
        {
            if (string.IsNullOrEmpty(SpecialRegimes.SelfBillingIndicator) || (SpecialRegimes.SelfBillingIndicator != "0" && SpecialRegimes.SelfBillingIndicator != "1"))
                listError.Add(new ValidationError { Description = $"Indicador de autofaturação do documento {InvoiceNo} incorrecto.", Field = "Invoice/SpecialRegimes/SelfBillingIndicator", FileID = InvoiceNo, Value = SpecialRegimes.SelfBillingIndicator, TypeofError = GetType(), UID = Pk });

            if (string.IsNullOrEmpty(SpecialRegimes.CashVATSchemeIndicator) || (SpecialRegimes.CashVATSchemeIndicator != "0" && SpecialRegimes.CashVATSchemeIndicator != "1"))
                listError.Add(new ValidationError { Description = $"Indicador da existência de adesão ao regime de IVA de Caixa do documento {InvoiceNo} incorrecto.", Field = "Invoice/SpecialRegimes/CashVATSchemeIndicator", FileID = InvoiceNo, Value = SpecialRegimes.CashVATSchemeIndicator, TypeofError = GetType(), UID = Pk });

            if (string.IsNullOrEmpty(SpecialRegimes.ThirdPartiesBillingIndicator) || (SpecialRegimes.ThirdPartiesBillingIndicator != "0" && SpecialRegimes.ThirdPartiesBillingIndicator != "1"))
                listError.Add(new ValidationError { Description = $"Indicador de faturação emitida em nome e por conta de terceiros do documento {InvoiceNo} incorrecto.", Field = "Invoice/SpecialRegimes/ThirdPartiesBillingIndicator", FileID = InvoiceNo, Value = SpecialRegimes.ThirdPartiesBillingIndicator, TypeofError = GetType(), UID = Pk });
        }

        return [.. listError];
    }

    public ValidationError[] ValidateShipTo() => ValidateShipping(ShipTo, "Invoice/ShipTo");
    public ValidationError[] ValidateShipFrom() => ValidateShipping(ShipFrom, "Invoice/ShipFrom");

    public ValidationError[] ValidateShipping(ShippingPointStructure ship, string path)
    {
        var listError = new List<ValidationError>();

        if (ship != null)
        {
            if (ship.Address != null)
            {
                if (string.IsNullOrEmpty(ship.Address.AddressDetail) || ship.Address.AddressDetail.Length > 210)
                    listError.Add(new ValidationError { Description = $"Tamanho da morada detalhada do documento {InvoiceNo} do local de transporte incorrecto.", Field = $"{path}/AddressDetail", FileID = InvoiceNo, Value = ship.Address.AddressDetail, TypeofError = GetType(), UID = Pk });
                if (!string.IsNullOrEmpty(ship.Address.BuildingNumber) && ship.Address.BuildingNumber.Length > 10)
                    listError.Add(new ValidationError { Description = $"Tamanho do número de polícia do documento {InvoiceNo} do local de transporte incorrecto.", Field = $"{path}/BuildingNumber", FileID = InvoiceNo, Value = ship.Address.BuildingNumber, TypeofError = GetType(), UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.City) || ship.Address.City.Length > 50)
                    listError.Add(new ValidationError { Description = $"Tamanho da localidade do documento {InvoiceNo} do local de transporte incorrecto.", Field = $"{path}/City", FileID = InvoiceNo, Value = ship.Address.City, TypeofError = GetType(), UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.Country) || (ship.Address.Country.Length != 2 && ship.Address.Country != "Desconhecido"))
                    listError.Add(new ValidationError { Description = $"País do documento {InvoiceNo} do local de transporte incorrecto.", Field = $"{path}/Country", FileID = InvoiceNo, Value = ship.Address.Country, TypeofError = GetType(), UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.PostalCode) || ship.Address.PostalCode.Length > 20)
                    listError.Add(new ValidationError { Description = $"Tamanho do código postal do documento {InvoiceNo} do local de transporte incorrecto.", Field = $"{path}/PostalCode", FileID = InvoiceNo, Value = ship.Address.PostalCode, TypeofError = GetType(), UID = Pk });
                if (!string.IsNullOrEmpty(ship.Address.Region) && ship.Address.Region.Length > 50)
                    listError.Add(new ValidationError { Description = $"Tamanho do distrito do documento {InvoiceNo} do local de transporte incorrecto.", Field = $"{path}/Region", FileID = InvoiceNo, Value = ship.Address.Region, TypeofError = GetType(), UID = Pk });
                if (!string.IsNullOrEmpty(ship.Address.StreetName) && ship.Address.StreetName.Length > 200)
                    listError.Add(new ValidationError { Description = $"Tamanho do nome da rua do documento {InvoiceNo} do local de transporte incorrecto.", Field = $"{path}/StreetName", FileID = InvoiceNo, Value = ship.Address.StreetName, TypeofError = GetType(), UID = Pk });
            }
            if (ship.DeliveryDateSpecified && ship.DeliveryDate == DateTime.MinValue)
                listError.Add(new ValidationError { Description = $"Data de entrega/receção do documento {InvoiceNo} incorrecta.", Field = $"{path}/DeliveryDate", FileID = InvoiceNo, Value = ship.DeliveryDate.ToString(), TypeofError = GetType(), UID = Pk });
        }

        return [.. listError];
    }

    public ValidationError[] ValidateDocumentTotals()
    {
        var listError = new List<ValidationError>();

        if (DocumentTotals == null)
        {
            listError.Add(new ValidationError { Description = $"Totais do documento {InvoiceNo} inexistentes.", Field = "Invoice/DocumentTotals", FileID = InvoiceNo, TypeofError = GetType(), UID = Pk });
            return [.. listError];
        }

        if (DocumentTotals.Currency != null)
        {
            if (DocumentTotals.Currency.CurrencyAmount < 0)
                listError.Add(new ValidationError { Description = $"Valor total em moeda estrangeira do documento {InvoiceNo} incorrecto.", Field = "Invoice/DocumentTotals/Currency/CurrencyAmount", FileID = InvoiceNo, Value = DocumentTotals.Currency.CurrencyAmount.ToString(), TypeofError = GetType(), UID = Pk });
            if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length != 3)
                listError.Add(new ValidationError { Description = $"Código de moeda do documento {InvoiceNo} incorrecto.", Field = "Invoice/DocumentTotals/Currency/CurrencyCode", FileID = InvoiceNo, Value = DocumentTotals.Currency.CurrencyCode, TypeofError = GetType(), UID = Pk });
            if (DocumentTotals.Currency.ExchangeRate < 0)
                listError.Add(new ValidationError { Description = $"Taxa de câmbio do documento {InvoiceNo} incorrecta.", Field = "Invoice/DocumentTotals/Currency/ExchangeRate", FileID = InvoiceNo, Value = DocumentTotals.Currency.ExchangeRate.ToString(), TypeofError = GetType(), UID = Pk });
        }

        if (DocumentTotals.Settlement != null && DocumentTotals.Settlement.Length > 0)
        {
            foreach (var acordo in DocumentTotals.Settlement)
            {
                if (!string.IsNullOrEmpty(acordo.PaymentTerms) && acordo.PaymentTerms.Length > 100)
                    listError.Add(new ValidationError { Description = $"Tamanho do acordo de pagamento do documento {InvoiceNo} incorrecto.", Field = "Invoice/DocumentTotals/Settlement/PaymentTerms", FileID = InvoiceNo, Value = acordo.PaymentTerms, TypeofError = GetType(), UID = Pk });
                if (acordo.SettlementAmount < 0)
                    listError.Add(new ValidationError { Description = $"Acordos de descontos futuros do documento {InvoiceNo} incorrecto.", Field = "Invoice/DocumentTotals/Settlement/SettlementAmount", FileID = InvoiceNo, Value = acordo.SettlementAmount.ToString(), TypeofError = GetType(), UID = Pk });
                if (acordo.SettlementDateSpecified && acordo.SettlementDate == DateTime.MinValue)
                    listError.Add(new ValidationError { Description = $"Data acordada para o desconto do documento {InvoiceNo} incorrecta.", Field = "Invoice/DocumentTotals/Settlement/SettlementDate", FileID = InvoiceNo, Value = acordo.SettlementDate.ToString(), TypeofError = GetType(), UID = Pk });
                if (!string.IsNullOrEmpty(acordo.SettlementDiscount) && acordo.SettlementDiscount.Length > 30)
                    listError.Add(new ValidationError { Description = $"Tamanho da descrição do desconto do documento {InvoiceNo} incorrecto.", Field = "Invoice/DocumentTotals/Settlement/SettlementDiscount", FileID = InvoiceNo, Value = acordo.SettlementDiscount, TypeofError = GetType(), UID = Pk });
            }
        }

        if (DocumentTotals.GrossTotal < 0)
            listError.Add(new ValidationError { Description = $"Total do documento {InvoiceNo} incorrecto.", Field = "Invoice/DocumentTotals/GrossTotal", FileID = InvoiceNo, Value = DocumentTotals.GrossTotal.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentTotals.NetTotal < 0)
            listError.Add(new ValidationError { Description = $"Total do documento {InvoiceNo} sem impostos incorrecto.", Field = "Invoice/DocumentTotals/NetTotal", FileID = InvoiceNo, Value = DocumentTotals.NetTotal.ToString(), TypeofError = GetType(), UID = Pk });
        if (DocumentTotals.TaxPayable < 0)
            listError.Add(new ValidationError { Description = $"Valor do imposto a pagar do documento {InvoiceNo} incorrecto.", Field = "Invoice/DocumentTotals/TaxPayable", FileID = InvoiceNo, Value = DocumentTotals.TaxPayable.ToString(), TypeofError = GetType(), UID = Pk });

        return [.. listError];
    }

    public ValidationError[] ValidateWithholdingTax()
    {
        if (WithholdingTax == null || WithholdingTax.Length == 0) return [];

        var listError = new List<ValidationError>();
        foreach (var wt in WithholdingTax)
        {
            if (wt.WithholdingTaxType == WithholdingTaxType.INVALIDO)
                listError.Add(new ValidationError { Description = $"Tipo de retenção na fonte inválido no documento {InvoiceNo}.", Field = "Invoice/WithholdingTax/WithholdingTaxType", FileID = InvoiceNo, Value = wt.WithholdingTaxType.ToString(), TypeofError = GetType(), UID = Pk });
            if (!string.IsNullOrEmpty(wt.WithholdingTaxDescription) && wt.WithholdingTaxDescription.Length > 60)
                listError.Add(new ValidationError { Description = $"Descrição da retenção na fonte demasiado longa no documento {InvoiceNo}.", Field = "Invoice/WithholdingTax/WithholdingTaxDescription", FileID = InvoiceNo, Value = wt.WithholdingTaxDescription, TypeofError = GetType(), UID = Pk });
            if (wt.WithholdingTaxAmount < 0)
                listError.Add(new ValidationError { Description = $"Montante da retenção na fonte negativo no documento {InvoiceNo}.", Field = "Invoice/WithholdingTax/WithholdingTaxAmount", FileID = InvoiceNo, Value = wt.WithholdingTaxAmount.ToString(), TypeofError = GetType(), UID = Pk });
        }
        return [.. listError];
    }
}

public partial class SourceDocumentsSalesInvoicesInvoiceLine : BaseData
{
    public string InvoiceNo { get; set; }

    public ValidationError ValidateLineNumber(string SupPk)
    {
        if (string.IsNullOrEmpty(LineNumber) || !int.TryParse(LineNumber, out int num) || num <= 0)
        {
            return new ValidationError { Description = $"Número de linha incorrecto {LineNumber}.", Field = "Invoice/Line/LineNumber", FileID = InvoiceNo, Value = LineNumber, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateProductCode(string SupPk)
    {
        if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 60)
        {
            return new ValidationError { Description = $"Identificador do produto ou serviço incorrecto, linha nº {LineNumber}.", Field = "Invoice/Line/ProductCode", FileID = InvoiceNo, Value = ProductCode, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateProductDescription(string SupPk)
    {
        if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length < 2 || ProductDescription.Length > 200)
        {
            return new ValidationError { Description = $"Descrição do produto ou serviço incorrecta, linha nº {LineNumber}.", Field = "Invoice/Line/ProductDescription", FileID = InvoiceNo, Value = ProductDescription, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateQuantity(string SupPk)
    {
        if (Quantity < 0)
        {
            return new ValidationError { Description = $"Quantidade incorrecta, linha nº {LineNumber}.", Field = "Invoice/Line/Quantity", FileID = InvoiceNo, Value = Quantity.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateUnitOfMeasure(string SupPk)
    {
        if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
        {
            return new ValidationError { Description = $"Unidade de medida incorrecta, linha nº {LineNumber}.", Field = "Invoice/Line/UnitOfMeasure", FileID = InvoiceNo, Value = UnitOfMeasure, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateUnitPrice(string SupPk)
    {
        if (UnitPrice < 0)
        {
            return new ValidationError { Description = $"Preço unitário incorrecto, linha nº {LineNumber}.", Field = "Invoice/Line/UnitPrice", FileID = InvoiceNo, Value = UnitPrice.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateDescription(string SupPk)
    {
        if (string.IsNullOrEmpty(Description) || Description.Length > 200)
        {
            return new ValidationError { Description = $"Descrição da linha nº {LineNumber} incorrecta.", Field = "Invoice/Line/Description", FileID = InvoiceNo, Value = Description, TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateSettlementAmount(string SupPk)
    {
        if (SettlementAmount < 0)
        {
            return new ValidationError { Description = $"Montante do desconto na linha nº {LineNumber} incorrecto.", Field = "Invoice/Line/SettlementAmount", FileID = InvoiceNo, Value = SettlementAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateTaxBase(string SupPk)
    {
        if (TaxBase < 0)
        {
            return new ValidationError { Description = $"Base tributável na linha nº {LineNumber} incorrecta.", Field = "Invoice/Line/TaxBase", FileID = InvoiceNo, Value = TaxBase.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError ValidateTaxPointDate(string SupPk)
    {
        if (TaxPointDate == DateTime.MinValue)
        {
            return new ValidationError { Description = $"Data de envio da mercadoria ou prestação do serviço incorrecta, linha nº {LineNumber}.", Field = "Invoice/Line/TaxPointDate", FileID = InvoiceNo, Value = TaxPointDate.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk };
        }
        return null;
    }

    public ValidationError[] ValidateOrderReferences(string SupPk)
    {
        var listError = new List<ValidationError>();
        if (OrderReferences != null && OrderReferences.Length > 0)
        {
            foreach (var referencia in OrderReferences)
            {
                if (!string.IsNullOrEmpty(referencia.OriginatingON) && referencia.OriginatingON.Length > 60)
                    listError.Add(new ValidationError { Description = $"Tamanho do número do documento de origem na linha nº {LineNumber} incorrecto.", Field = "Invoice/Line/OrderReferences/OriginatingON", FileID = InvoiceNo, Value = referencia.OriginatingON, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
                if (referencia.OrderDateSpecified && referencia.OrderDate == DateTime.MinValue)
                    listError.Add(new ValidationError { Description = $"Data do documento de origem na linha nº {LineNumber} incorrecta.", Field = "Invoice/Line/OrderReferences/OrderDate", FileID = InvoiceNo, Value = referencia.OrderDate.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            }
        }
        return [.. listError];
    }

    public ValidationError[] ValidateReferences(string SupPk)
    {
        var listError = new List<ValidationError>();
        if (References != null && References.Length > 0)
        {
            foreach (var referencia in References)
            {
                if (!string.IsNullOrEmpty(referencia.Reason) && referencia.Reason.Length > 50)
                    listError.Add(new ValidationError { Description = $"Tamanho do motivo da emissão na linha nº {LineNumber} incorrecto.", Field = "Invoice/Line/References/Reason", FileID = InvoiceNo, Value = referencia.Reason, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
                if (!string.IsNullOrEmpty(referencia.Reference) && referencia.Reference.Length > 60)
                    listError.Add(new ValidationError { Description = $"Tamanho da referência à fatura ou fatura simplificada na linha nº {LineNumber} incorrecto.", Field = "Invoice/Line/References/Reference", FileID = InvoiceNo, Value = referencia.Reference, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            }
        }
        return [.. listError];
    }

    public ValidationError[] ValidateTax(string SupPk)
    {
        var listError = new List<ValidationError>();

        if (Tax == null)
        {
            listError.Add(new ValidationError { Description = $"Taxa de imposto na linha nº {LineNumber} inexistente.", Field = "Invoice/Line/Tax", FileID = InvoiceNo, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
            return [.. listError];
        }

        if (Tax.TaxType == TaxType.INVALIDO)
            listError.Add(new ValidationError { Description = $"Tipo de imposto na linha nº {LineNumber} inválido.", Field = "Invoice/Line/Tax/TaxType", FileID = InvoiceNo, Value = Tax.TaxType.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
        if (Tax.ItemElementName == ItemChoiceType1.TaxAmount && Tax.TaxAmount < 0)
            listError.Add(new ValidationError { Description = $"Montante do imposto na linha nº {LineNumber} incorrecto.", Field = "Invoice/Line/Tax/TaxAmount", FileID = InvoiceNo, Value = Tax.TaxAmount.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
        if (Tax.ItemElementName == ItemChoiceType1.TaxPercentage && (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100))
            listError.Add(new ValidationError { Description = $"Percentagem da taxa do imposto na linha nº {LineNumber} incorrecta.", Field = "Invoice/Line/Tax/TaxPercentage", FileID = InvoiceNo, Value = Tax.TaxPercentage.ToString(), TypeofError = GetType(), UID = Pk, SupUID = SupPk });
        if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
            listError.Add(new ValidationError { Description = $"Código da taxa na linha nº {LineNumber} incorrecto.", Field = "Invoice/Line/Tax/TaxCode", FileID = InvoiceNo, Value = Tax.TaxCode, TypeofError = GetType(), UID = Pk, SupUID = SupPk });
        if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
            listError.Add(new ValidationError { Description = $"País ou região do imposto na linha nº {LineNumber} incorrecto.", Field = "Invoice/Line/Tax/TaxCountryRegion", FileID = InvoiceNo, Value = Tax.TaxCountryRegion, TypeofError = GetType(), UID = Pk, SupUID = SupPk });

        return [.. listError];
    }

    public ValidationError[] ValidateTaxExemption(string SupPk, DateTime invoiceDate)
    {
        var errors = new List<ValidationError>();
        bool isIvaZeroOrExempt = Tax != null && Tax.TaxType == TaxType.IVA &&
            ((Tax.ItemElementName == ItemChoiceType1.TaxPercentage && Tax.TaxPercentage.GetValueOrDefault(0) == 0) ||
             string.Equals(Tax.TaxCode, "ISE", StringComparison.OrdinalIgnoreCase));
        bool isTaxedAboveZero = Tax != null && Tax.ItemElementName == ItemChoiceType1.TaxPercentage && Tax.TaxPercentage.GetValueOrDefault(0) > 0;

        PartialValidationPatterns.ValidateTaxExemptionFields(
            errors, GetType(), InvoiceNo, LineNumber, Pk, SupPk, "Invoice/Line",
            isIvaZeroOrExempt, isTaxedAboveZero, TaxExemptionReason, TaxExemptionCode, invoiceDate);

        return [.. errors];
    }
}

public partial class Header : BaseData
{
    public ValidationError ValidateTaxRegistrationNumber()
    {
        if (!Validations.CheckTaxRegistrationNumber(TaxRegistrationNumber))
        {
            return new ValidationError { Description = "NIF inválido", Field = "Header/TaxRegistrationNumber", Value = TaxRegistrationNumber, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateAuditFileVersion()
    {
        if (string.IsNullOrEmpty(AuditFileVersion) || AuditFileVersion.Length > 10)
        {
            return new ValidationError { Description = "Versão do ficheiro SAF-T PT incorrecta.", Field = "Header/AuditFileVersion", Value = AuditFileVersion, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateBusinessName()
    {
        if (!string.IsNullOrEmpty(BusinessName) && BusinessName.Length > 60)
        {
            return new ValidationError { Description = "Designação comercial incorrecta.", Field = "Header/BusinessName", Value = BusinessName, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateAddressDetail()
    {
        if (string.IsNullOrEmpty(CompanyAddress?.AddressDetail) || CompanyAddress.AddressDetail.Length > 210)
        {
            return new ValidationError { Description = "Morada detalhada incorrecta.", Field = "Header/CompanyAddress/AddressDetail", Value = CompanyAddress?.AddressDetail, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateBuildingNumber()
    {
        if (!string.IsNullOrEmpty(CompanyAddress?.BuildingNumber) && CompanyAddress.BuildingNumber.Length > 10)
        {
            return new ValidationError { Description = "Número polícia incorrecto.", Field = "Header/CompanyAddress/BuildingNumber", Value = CompanyAddress?.BuildingNumber, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCity()
    {
        if (string.IsNullOrEmpty(CompanyAddress?.City) || CompanyAddress.City.Length > 50)
        {
            return new ValidationError { Description = "Localidade incorrecta.", Field = "Header/CompanyAddress/City", Value = CompanyAddress?.City, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCountry()
    {
        if (string.IsNullOrEmpty(CompanyAddress?.Country) || CompanyAddress.Country != "PT")
        {
            return new ValidationError { Description = "País incorrecto.", Field = "Header/CompanyAddress/Country", Value = CompanyAddress?.Country, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidatePostalCode()
    {
        if (string.IsNullOrEmpty(CompanyAddress?.PostalCode) || !PartialValidationPatterns.PostalCodePtRegex().IsMatch(CompanyAddress.PostalCode))
        {
            return new ValidationError { Description = "Código postal incorrecto (formato esperado: XXXX-XXX).", Field = "Header/CompanyAddress/PostalCode", Value = CompanyAddress?.PostalCode, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateRegion()
    {
        if (!string.IsNullOrEmpty(CompanyAddress?.Region) && CompanyAddress.Region.Length > 50)
        {
            return new ValidationError { Description = "Distrito incorrecto.", Field = "Header/CompanyAddress/Region", Value = CompanyAddress?.Region, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateStreetName()
    {
        if (!string.IsNullOrEmpty(CompanyAddress?.StreetName) && CompanyAddress.StreetName.Length > 200)
        {
            return new ValidationError { Description = "Nome da rua incorrecto.", Field = "Header/CompanyAddress/StreetName", Value = CompanyAddress?.StreetName, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCompanyID()
    {
        if (string.IsNullOrEmpty(CompanyID) || CompanyID.Length > 50 || !PartialValidationPatterns.CompanyIdRegex().IsMatch(CompanyID))
        {
            return new ValidationError { Description = "Registo comercial incorrecto.", Field = "Header/CompanyID", Value = CompanyID, TypeofError = GetType(), UID = Pk };
        }
        if (Validations.IsNumeric(CompanyID) && CompanyID.Length == 9 && !Validations.CheckTaxRegistrationNumber(CompanyID))
        {
            return new ValidationError { Description = "Registo comercial (NIF) incorrecto.", Field = "Header/CompanyID", Value = CompanyID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCompanyName()
    {
        if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
        {
            return new ValidationError { Description = "Nome empresa incorrecto.", Field = "Header/CompanyName", Value = CompanyName, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCurrencyCode()
    {
        if (CurrencyCode == null || CurrencyCode != "EUR")
        {
            return new ValidationError { Description = "Código moeda incorrecto.", Field = "Header/CurrencyCode", Value = CurrencyCode ?? "null", TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateStartDate()
    {
        if (StartDate == DateTime.MinValue)
        {
            return new ValidationError { Description = "Data do início do período incorrecta.", Field = "Header/StartDate", Value = StartDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateEndDate()
    {
        if (EndDate == DateTime.MinValue)
        {
            return new ValidationError { Description = "Data do fim do período incorrecta.", Field = "Header/EndDate", Value = EndDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        if (StartDate != DateTime.MinValue && EndDate < StartDate)
        {
            return new ValidationError { Description = "Data do fim do período anterior à data de início.", Field = "Header/EndDate", Value = EndDate.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateDateCreated()
    {
        if (DateCreated == DateTime.MinValue)
        {
            return new ValidationError { Description = "Data de criação do ficheiro incorrecta.", Field = "Header/DateCreated", Value = DateCreated.ToString(), TypeofError = GetType(), UID = Pk };
        }
        if (StartDate != DateTime.MinValue && DateCreated < StartDate)
        {
            return new ValidationError { Description = "Data de criação do ficheiro anterior à data de início do período.", Field = "Header/DateCreated", Value = DateCreated.ToString(), TypeofError = GetType(), ValidationType = ValidationErrorType.WARNING, UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateEmail()
    {
        if (!string.IsNullOrEmpty(Email) && (Email.Length > 254 || !PartialValidationPatterns.EmailRegex().IsMatch(Email)))
        {
            return new ValidationError { Description = "Email incorrecto.", Field = "Header/Email", Value = Email, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateFax()
    {
        if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
        {
            return new ValidationError { Description = "Fax incorrecto.", Field = "Header/Fax", Value = Fax, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateFiscalYear()
    {
        if (string.IsNullOrEmpty(FiscalYear) || FiscalYear.Length > 4 || !int.TryParse(FiscalYear, out int ano) || ano < 2000 || ano > 9999)
        {
            return new ValidationError { Description = "Ano fiscal incorrecto.", Field = "Header/FiscalYear", Value = FiscalYear, TypeofError = GetType(), UID = Pk };
        }
        if (StartDate != DateTime.MinValue && EndDate != DateTime.MinValue && StartDate.Year != ano && EndDate.Year != ano)
        {
            return new ValidationError { Description = $"Ano fiscal ({ano}) incoerente com o período ({StartDate:yyyy-MM-dd} a {EndDate:yyyy-MM-dd}).", Field = "Header/FiscalYear", Value = FiscalYear, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateHeaderComment()
    {
        if (!string.IsNullOrEmpty(HeaderComment) && HeaderComment.Length > 255)
        {
            return new ValidationError { Description = "Comentário demasiado longo.", Field = "Header/HeaderComment", Value = HeaderComment, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateProductCompanyTaxID()
    {
        if (string.IsNullOrEmpty(ProductCompanyTaxID) || ProductCompanyTaxID.Length > 20)
        {
            return new ValidationError { Description = "NIF da empresa produtora de software inválido.", Field = "Header/ProductCompanyTaxID", Value = ProductCompanyTaxID, TypeofError = GetType(), UID = Pk };
        }
        if (Validations.IsNumeric(ProductCompanyTaxID) && ProductCompanyTaxID.Length == 9 && !Validations.CheckTaxRegistrationNumber(ProductCompanyTaxID))
        {
            return new ValidationError { Description = "NIF da empresa produtora de software inválido.", Field = "Header/ProductCompanyTaxID", Value = ProductCompanyTaxID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateProductID()
    {
        if (string.IsNullOrEmpty(ProductID) || ProductID.Length < 3 || ProductID.Length > 255 || !ProductID.Contains('/'))
        {
            return new ValidationError { Description = "Nome da aplicação incorrecto.", Field = "Header/ProductID", Value = ProductID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateProductVersion()
    {
        if (string.IsNullOrEmpty(ProductVersion) || ProductVersion.Length > 30)
        {
            return new ValidationError { Description = "Versão da aplicação incorrecta.", Field = "Header/ProductVersion", Value = ProductVersion, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSoftwareCertificateNumber()
    {
        if (string.IsNullOrEmpty(SoftwareCertificateNumber) || SoftwareCertificateNumber.Length > 20 || !long.TryParse(SoftwareCertificateNumber, out long cert) || cert < 0)
        {
            return new ValidationError { Description = "Número de certificação incorrecto.", Field = "Header/SoftwareCertificateNumber", Value = SoftwareCertificateNumber, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTaxAccountingBasis()
    {
        if (TaxAccountingBasis == TaxAccountingBasis.INVALIDO || string.IsNullOrEmpty(TaxAccountingBasis.ToString()) || TaxAccountingBasis.ToString().Length > 1)
        {
            return new ValidationError { Description = "Sistema contabilístico incorrecto.", Field = "Header/TaxAccountingBasis", Value = TaxAccountingBasis.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTaxEntity()
    {
        if (string.IsNullOrEmpty(TaxEntity) || TaxEntity.Length > 20)
        {
            return new ValidationError { Description = "Identificação do estabelecimento incorrecta.", Field = "Header/TaxEntity", Value = TaxEntity, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTelephone()
    {
        if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
        {
            return new ValidationError { Description = "Telefone incorrecto.", Field = "Header/Telephone", Value = Telephone, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateWebsite()
    {
        if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
        {
            return new ValidationError { Description = "Website incorrecto.", Field = "Header/Website", Value = Website, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }
}

public partial class Product : BaseData
{
    public string Prices { get; set; }
    public string PricesWithVat { get; set; }
    public string Taxes { get; set; }

    public ValidationError ValidateProductCode()
    {
        if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 60)
        {
            return new ValidationError { Description = "Código do produto inválido.", Field = "Product/ProductCode", FileID = ProductCode, Value = ProductCode, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateProductGroup()
    {
        if (!string.IsNullOrEmpty(ProductGroup) && ProductGroup.Length > 50)
        {
            return new ValidationError { Description = "Família do produto ou serviço inválida.", Field = "Product/ProductGroup", FileID = ProductCode, Value = ProductGroup, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateProductDescription()
    {
        if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length < 2 || ProductDescription.Length > 200)
        {
            return new ValidationError { Description = "Descrição do produto ou serviço inválida.", Field = "Product/ProductDescription", FileID = ProductCode, Value = ProductDescription, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateProductNumberCode()
    {
        if (string.IsNullOrEmpty(ProductNumberCode) || ProductNumberCode.Length > 60)
        {
            return new ValidationError { Description = "Código de barras / número do produto inválido.", Field = "Product/ProductNumberCode", FileID = ProductCode, Value = ProductNumberCode, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }
}

public partial class Customer : BaseData
{
    public ValidationError ValidateCustomerID()
    {
        if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
        {
            return new ValidationError { Description = "Identificador único do cliente inválido", Field = "Customer/CustomerID", FileID = CustomerID, Value = CustomerID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateAccountID()
    {
        if (string.IsNullOrEmpty(AccountID) || AccountID.Length > 30)
        {
            return new ValidationError { Description = "Código da conta inválido", Field = "Customer/AccountID", FileID = CustomerID, Value = AccountID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCustomerTaxID()
    {
        if (string.IsNullOrEmpty(CustomerTaxID) || CustomerTaxID.Length > 30 ||
            (BillingAddress?.Country == "PT" && !Validations.CheckTaxRegistrationNumber(CustomerTaxID)))
        {
            return new ValidationError { Description = "Número de identificação fiscal inválido", Field = "Customer/CustomerTaxID", FileID = CustomerID, Value = CustomerTaxID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCompanyName()
    {
        if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
        {
            return new ValidationError { Description = "Nome da empresa inválido", Field = "Customer/CompanyName", FileID = CustomerID, Value = CompanyName, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateContact()
    {
        if (!string.IsNullOrEmpty(Contact) && Contact.Length > 50)
        {
            return new ValidationError { Description = "Nome do contacto na empresa inválido.", Field = "Customer/Contact", FileID = CustomerID, Value = Contact, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTelephone()
    {
        if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
        {
            return new ValidationError { Description = "Telefone inválido", Field = "Customer/Telephone", FileID = CustomerID, Value = Telephone, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateFax()
    {
        if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
        {
            return new ValidationError { Description = "Fax inválido", Field = "Customer/Fax", FileID = CustomerID, Value = Fax, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateEmail()
    {
        if (!string.IsNullOrEmpty(Email) && Email.Length > 254)
        {
            return new ValidationError { Description = "Email inválido", Field = "Customer/Email", FileID = CustomerID, Value = Email, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateWebsite()
    {
        if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
        {
            return new ValidationError { Description = "Website inválido", Field = "Customer/Website", FileID = CustomerID, Value = Website, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSelfBillingIndicator()
    {
        if (string.IsNullOrEmpty(SelfBillingIndicator) || (SelfBillingIndicator != "0" && SelfBillingIndicator != "1"))
        {
            return new ValidationError { Description = "Indicador de autofaturação inválido", Field = "Customer/SelfBillingIndicator", FileID = CustomerID, Value = SelfBillingIndicator, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError[] ValidateBillingAddress()
    {
        var listErro = new List<ValidationError>();
        if (BillingAddress == null)
            listErro.Add(new ValidationError { Description = "Morada de faturação inexistente", Field = "Customer/BillingAddress", FileID = CustomerID, TypeofError = GetType(), UID = Pk });
        else
            ValidateAddress(BillingAddress, "Customer/BillingAddress", listErro);
        return [.. listErro];
    }

    public ValidationError[] ValidateShipToAddress()
    {
        var listErro = new List<ValidationError>();
        if (ShipToAddress != null && ShipToAddress.Length > 0)
        {
            foreach (var morada in ShipToAddress)
                ValidateAddress(morada, "Customer/ShipToAddress", listErro);
        }
        return [.. listErro];
    }

    public void ValidateAddress(AddressStructure address, string path, List<ValidationError> listErro)
    {
        if (string.IsNullOrEmpty(address.AddressDetail) || address.AddressDetail.Length > 210)
            listErro.Add(new ValidationError { Description = "Tamanho da morada detalhada incorrecto.", Field = $"{path}/AddressDetail", FileID = CustomerID, Value = address.AddressDetail, TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(address.BuildingNumber) && address.BuildingNumber.Length > 10)
            listErro.Add(new ValidationError { Description = "Tamanho do número de polícia incorrecto.", Field = $"{path}/BuildingNumber", FileID = CustomerID, Value = address.BuildingNumber, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(address.City) || address.City.Length > 50)
            listErro.Add(new ValidationError { Description = "Tamanho da localidade incorrecto.", Field = $"{path}/City", FileID = CustomerID, Value = address.City, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(address.PostalCode) || address.PostalCode.Length > 20)
            listErro.Add(new ValidationError { Description = "Tamanho do código postal incorrecto.", Field = $"{path}/PostalCode", FileID = CustomerID, Value = address.PostalCode, TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(address.Region) && address.Region.Length > 50)
            listErro.Add(new ValidationError { Description = "Tamanho do distrito incorrecto.", Field = $"{path}/Region", FileID = CustomerID, Value = address.Region, TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(address.StreetName) && address.StreetName.Length > 200)
            listErro.Add(new ValidationError { Description = "Tamanho do nome da rua incorrecto.", Field = $"{path}/StreetName", FileID = CustomerID, Value = address.StreetName, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(address.Country) || (address.Country.Length != 2 && address.Country != "Desconhecido"))
            listErro.Add(new ValidationError { Description = "País incorrecto.", Field = $"{path}/Country", FileID = CustomerID, Value = address.Country, TypeofError = GetType(), UID = Pk });
    }
}

public partial class TaxTableEntry : BaseData
{
    public ValidationError ValidateTaxType()
    {
        if (TaxType == TaxType.INVALIDO)
        {
            return new ValidationError { Description = "Tipo de imposto inválido", Field = "TaxTableEntry/TaxType", FileID = TaxCode, Value = TaxType.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTaxCountryRegion()
    {
        if (string.IsNullOrEmpty(TaxCountryRegion) || TaxCountryRegion.Length > 5)
        {
            return new ValidationError { Description = "País ou região do imposto inválido", Field = "TaxTableEntry/TaxCountryRegion", FileID = TaxCode, Value = TaxCountryRegion, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTaxCode()
    {
        if (string.IsNullOrEmpty(TaxCode) || TaxCode.Length > 10)
        {
            return new ValidationError { Description = "Código do imposto inválido", Field = "TaxTableEntry/TaxCode", FileID = TaxCode, Value = TaxCode, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateDescription()
    {
        if (string.IsNullOrEmpty(Description) || Description.Length > 255)
        {
            return new ValidationError { Description = "Descrição inválida", Field = "TaxTableEntry/Description", FileID = TaxCode, Value = Description, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTaxValue()
    {
        if (ItemElementName == ItemChoiceType2.TaxPercentage)
        {
            decimal pct = TaxPercentage ?? Item;
            if (pct < 0 || pct > 100)
                return new ValidationError { Description = "Percentagem da taxa do imposto inválida.", Field = "TaxTableEntry/TaxPercentage", FileID = TaxCode, Value = pct.ToString(), TypeofError = GetType(), UID = Pk };
        }
        else if (ItemElementName == ItemChoiceType2.TaxAmount)
        {
            decimal amt = TaxAmount ?? Item;
            if (amt < 0)
                return new ValidationError { Description = "Montante da taxa do imposto inválido.", Field = "TaxTableEntry/TaxAmount", FileID = TaxCode, Value = amt.ToString(), TypeofError = GetType(), UID = Pk };
        }
        return null;
    }
}

public partial class Supplier : BaseData
{
    public ValidationError ValidateCustomerID()
    {
        if (string.IsNullOrEmpty(SupplierID) || SupplierID.Length > 30)
        {
            return new ValidationError { Description = "Identificador único do fornecedor inválido", Field = "Supplier/SupplierID", FileID = SupplierID, Value = SupplierID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateAccountID()
    {
        if (string.IsNullOrEmpty(AccountID) || AccountID.Length > 30)
        {
            return new ValidationError { Description = "Código da conta inválido", Field = "Supplier/AccountID", FileID = SupplierID, Value = AccountID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSupplierTaxID()
    {
        if (string.IsNullOrEmpty(SupplierTaxID) || SupplierTaxID.Length > 30 ||
            (BillingAddress?.Country == "PT" && !Validations.CheckTaxRegistrationNumber(SupplierTaxID)))
        {
            return new ValidationError { Description = "Número de identificação fiscal inválido", Field = "Supplier/SupplierTaxID", FileID = SupplierID, Value = SupplierTaxID, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateCompanyName()
    {
        if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
        {
            return new ValidationError { Description = "Nome da empresa inválido", Field = "Supplier/CompanyName", FileID = SupplierID, Value = CompanyName, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateContact()
    {
        if (!string.IsNullOrEmpty(Contact) && Contact.Length > 50)
        {
            return new ValidationError { Description = "Nome do contacto na empresa inválido.", Field = "Supplier/Contact", FileID = SupplierID, Value = Contact, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateTelephone()
    {
        if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
        {
            return new ValidationError { Description = "Telefone inválido", Field = "Supplier/Telephone", FileID = SupplierID, Value = Telephone, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateFax()
    {
        if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
        {
            return new ValidationError { Description = "Fax inválido", Field = "Supplier/Fax", FileID = SupplierID, Value = Fax, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateEmail()
    {
        if (!string.IsNullOrEmpty(Email) && Email.Length > 254)
        {
            return new ValidationError { Description = "Email inválido", Field = "Supplier/Email", FileID = SupplierID, Value = Email, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateWebsite()
    {
        if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
        {
            return new ValidationError { Description = "Website inválido", Field = "Supplier/Website", FileID = SupplierID, Value = Website, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError ValidateSelfBillingIndicator()
    {
        if (string.IsNullOrEmpty(SelfBillingIndicator) || (SelfBillingIndicator != "0" && SelfBillingIndicator != "1"))
        {
            return new ValidationError { Description = "Indicador de autofaturação inválido", Field = "Supplier/SelfBillingIndicator", FileID = SupplierID, Value = SelfBillingIndicator, TypeofError = GetType(), UID = Pk };
        }
        return null;
    }

    public ValidationError[] ValidateBillingAddress()
    {
        var listErro = new List<ValidationError>();
        if (BillingAddress == null)
            listErro.Add(new ValidationError { Description = "Morada de faturação inexistente", Field = "Supplier/BillingAddress", FileID = SupplierID, TypeofError = GetType(), UID = Pk });
        else
            ValidateAddress(BillingAddress, "Supplier/BillingAddress", listErro);
        return [.. listErro];
    }

    public ValidationError[] ValidateShipFromAddress()
    {
        var listErro = new List<ValidationError>();
        if (ShipFromAddress != null && ShipFromAddress.Length > 0)
        {
            foreach (var morada in ShipFromAddress)
                ValidateAddress(morada, "Supplier/ShipFromAddress", listErro);
        }
        return [.. listErro];
    }

    public void ValidateAddress(SupplierAddressStructure address, string path, List<ValidationError> listErro)
    {
        if (string.IsNullOrEmpty(address.AddressDetail) || address.AddressDetail.Length > 210)
            listErro.Add(new ValidationError { Description = "Tamanho da morada detalhada incorrecto.", Field = $"{path}/AddressDetail", FileID = SupplierID, Value = address.AddressDetail, TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(address.BuildingNumber) && address.BuildingNumber.Length > 10)
            listErro.Add(new ValidationError { Description = "Tamanho do número de polícia incorrecto.", Field = $"{path}/BuildingNumber", FileID = SupplierID, Value = address.BuildingNumber, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(address.City) || address.City.Length > 50)
            listErro.Add(new ValidationError { Description = "Tamanho da localidade incorrecto.", Field = $"{path}/City", FileID = SupplierID, Value = address.City, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(address.Country) || address.Country.Length != 2)
            listErro.Add(new ValidationError { Description = "País incorrecto.", Field = $"{path}/Country", FileID = SupplierID, Value = address.Country, TypeofError = GetType(), UID = Pk });
        if (string.IsNullOrEmpty(address.PostalCode) || address.PostalCode.Length > 20)
            listErro.Add(new ValidationError { Description = "Tamanho do código postal incorrecto.", Field = $"{path}/PostalCode", FileID = SupplierID, Value = address.PostalCode, TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(address.Region) && address.Region.Length > 50)
            listErro.Add(new ValidationError { Description = "Tamanho do distrito incorrecto.", Field = $"{path}/Region", FileID = SupplierID, Value = address.Region, TypeofError = GetType(), UID = Pk });
        if (!string.IsNullOrEmpty(address.StreetName) && address.StreetName.Length > 200)
            listErro.Add(new ValidationError { Description = "Tamanho do nome da rua incorrecto.", Field = $"{path}/StreetName", FileID = SupplierID, Value = address.StreetName, TypeofError = GetType(), UID = Pk });
    }
}

public partial class GeneralLedger : BaseData { }
public partial class GeneralLedgerEntriesJournal : BaseData { }
public partial class GeneralLedgerEntriesJournalTransaction : BaseData { }
public partial class GeneralLedgerEntriesJournalTransactionLinesDebitLine : BaseData { }
public partial class GeneralLedgerEntriesJournalTransactionLinesCreditLine : BaseData { }
public partial class AuditFile : BaseData { }
public partial class GeneralLedgerAccountsAccount : BaseData { }
public partial class GeneralLedgerAccounts : BaseData { }
public partial class GeneralLedgerEntries : BaseData { }
