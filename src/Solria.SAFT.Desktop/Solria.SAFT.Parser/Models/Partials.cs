using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SolRIA.SAFT.Parser.Models;

public partial class SourceDocumentsPaymentsPayment : BaseData
{
    public ValidationError ValidatePaymentRefNo()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(PaymentRefNo) || PaymentRefNo.Length > 60)
        {
            erro = new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "Payment/PaymentRefNo", FileID = PaymentRefNo, Value = PaymentRefNo, UID = Pk };
        }
        else if (!PaymentReferenceNo().IsMatch(PaymentRefNo))
        {
            erro = new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "Payment/PaymentRefNo", FileID = PaymentRefNo, Value = PaymentRefNo, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidatePeriod()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(Period) == false)
        {
            int.TryParse(Period, out int periodo);

            if (periodo < 1 || periodo > 12)
            {
                erro = new ValidationError { Description = string.Format("Mês do período de tributação do documento {0} incorrecto.", PaymentRefNo), Field = "Payment/Period", FileID = PaymentRefNo, Value = Period, UID = Pk };
            }
        }

        return erro;
    }
    public ValidationError ValidateTransactionID()
    {
        ValidationError erro = null;

        if (!string.IsNullOrEmpty(TransactionID) && TransactionID.Length > 70)
        {
            erro = new ValidationError { Description = string.Format("Identificador da transacção do documento {0} incorrecto.", PaymentRefNo), Field = "Payment/TransactionID", FileID = PaymentRefNo, Value = TransactionID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateTransactionDate()
    {
        ValidationError erro = null;

        if (TransactionDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data de emissão do documento {0} incorrecta.", PaymentRefNo), Field = "Payment/TransactionDate", FileID = PaymentRefNo, Value = TransactionDate.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateDescription()
    {
        ValidationError erro = null;

        if (!string.IsNullOrEmpty(Description) && Description.Length > 200)
        {
            erro = new ValidationError { Description = string.Format("Tamanho da descrição do recibo {0} incorrecto.", PaymentRefNo), Field = "Payment/Description", FileID = PaymentRefNo, Value = Description, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidatePaymentStatusDate()
    {
        ValidationError erro = null;

        if (DocumentStatus.PaymentStatusDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data e hora do estado atual do recibo {0} incorrecta.", PaymentRefNo), Field = "Payment/PaymentStatusDate", FileID = PaymentRefNo, Value = DocumentStatus.PaymentStatusDate.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateReason()
    {
        ValidationError erro = null;

        if (!string.IsNullOrEmpty(DocumentStatus.Reason) && DocumentStatus.Reason.Length > 50)
        {
            erro = new ValidationError { Description = string.Format("Tamanho do motivo da alteração de estado do recibo {0} incorrecto.", PaymentRefNo), Field = "Payment/Reason", FileID = PaymentRefNo, Value = DocumentStatus.Reason, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateDocumentStatusSourceID()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
        {
            erro = new ValidationError { Description = string.Format("Utilizador responsável pelo estado atual do recibo {0} incorrecto.", DocumentStatus.SourceID), Field = "Payment/SourceID", FileID = PaymentRefNo, Value = DocumentStatus.SourceID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSystemID()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(SystemID) == false && SystemID.Length > 35)
        {
            erro = new ValidationError { Description = string.Format("Tamanho do número único do recibo {0} incorrecto.", PaymentRefNo), Field = "Payment/SystemID", FileID = PaymentRefNo, Value = SystemID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSourceID()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(SourceID) || SourceID.Length > 30)
        {
            erro = new ValidationError { Description = string.Format("Utilizador que gerou o documento {0} incorrecto.", SourceID), Field = "Payment/SourceID", FileID = PaymentRefNo, Value = SourceID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSystemEntryDate()
    {
        ValidationError erro = null;

        if (SystemEntryDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data da gravação do documento {0} incorrecta.", PaymentRefNo), Field = "Payment/SystemEntryDate", FileID = PaymentRefNo, Value = SystemEntryDate.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateCustomerID()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
        {
            erro = new ValidationError { Description = string.Format("Chave única da tabela de clientes no documento {0} incorrecta.", PaymentRefNo), Field = "Payment/CustomerID", FileID = PaymentRefNo, Value = CustomerID, UID = Pk };
        }

        return erro;
    }

    public ValidationError[] ValidatePaymentMethod()
    {
        if (PaymentMethod == null || PaymentMethod.Length == 0) return [];

        var listErro = new List<ValidationError>();

        foreach (var pay in PaymentMethod)
        {
            if (pay.PaymentAmount < 0)
            {
                listErro.Add(new ValidationError { Description = string.Format("Utilizador que gerou o documento {0} incorrecto.", PaymentRefNo), Field = "Payment/PaymentAmount", FileID = PaymentRefNo, Value = pay.PaymentAmount.ToString(), UID = Pk });
            }

            if (pay.PaymentDate > DateTime.Now)
            {
                listErro.Add(new ValidationError { Description = string.Format("Data do pagamento do documento {0} incorrecta.", PaymentRefNo), Field = "Payment/PaymentDate", FileID = PaymentRefNo, Value = pay.PaymentDate.ToString(), UID = Pk });
            }
        }

        return [.. listErro];
    }

    [GeneratedRegex("[^ ]+ [^/^ ]+/[0-9]+")]
    private static partial Regex PaymentReferenceNo();
}

public partial class SourceDocumentsPaymentsPaymentLine : BaseData
{
    /// <summary>
    /// Link to the Payment
    /// </summary>
    public string DocNo { get; set; }

    public ValidationError ValidateLineNumber(string SupPk)
    {
        ValidationError erro = null;
        int num = -1;
        if (!string.IsNullOrEmpty(LineNumber))
            int.TryParse(LineNumber, out num);

        if (string.IsNullOrEmpty(LineNumber) || num == -1)
        {
            erro = new ValidationError { Description = string.Format("Número de linha incorrecto {0}.", LineNumber), Field = "Payment/Line/LineNumber", FileID = DocNo, Value = LineNumber, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateSettlementAmount(string SupPk)
    {
        ValidationError erro = null;
        if (SettlementAmount < 0)
        {
            erro = new ValidationError { Description = string.Format("Montante do desconto, linha nº {0}.", LineNumber), Field = "Payment/Line/SettlementAmount", FileID = DocNo, Value = SettlementAmount.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateItem(string SupPk)
    {
        ValidationError erro = null;
        if ((CreditAmount ?? DebitAmount ?? 0) < 0)
        {
            if (ItemElementName == ItemChoiceType8.CreditAmount)
                erro = new ValidationError { Description = string.Format("Valor a crédito incorrecta, linha nº {0}.", LineNumber), Field = "Payment/Line/CreditAmount", FileID = DocNo, Value = CreditAmount.ToString(), UID = Pk, SupUID = SupPk };
            if (ItemElementName == ItemChoiceType8.DebitAmount)
                erro = new ValidationError { Description = string.Format("Valor a débito incorrecto, linha nº {0}.", LineNumber), Field = "Payment/Line/DebitAmount", FileID = DocNo, Value = DebitAmount.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateTaxExemptionReason(string SupPk)
    {
        ValidationError erro = null;
        if ((Tax != null && Tax.TaxPercentage == 0 && string.IsNullOrEmpty(TaxExemptionReason)) || (string.IsNullOrEmpty(TaxExemptionReason) == false && TaxExemptionReason.Length > 60))
        {
            erro = new ValidationError { Description = string.Format("Motivo da isenção de imposto incorrecto, linha nº {0}.", LineNumber), Field = "Payment/Line/TaxExemptionReason", FileID = DocNo, Value = TaxExemptionReason, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }

    public ValidationError[] ValidateSourceDocumentID(string SupPk)
    {
        var listErro = new List<ValidationError>();

        if (SourceDocumentID == null || SourceDocumentID.Length == 0)
            listErro.Add(new ValidationError { Description = string.Format("Referência ao documento de origem inexistente, linha nº {0}.", LineNumber), Field = "Payment/Line/SourceDocumentID", FileID = DocNo, UID = Pk, SupUID = SupPk });

        if (SourceDocumentID != null && SourceDocumentID.Length > 0)
        {
            foreach (var doc in SourceDocumentID)
            {
                if (string.IsNullOrEmpty(doc.Description) == false && doc.Description.Length > 100)
                    listErro.Add(new ValidationError { Description = string.Format("Tamanho da descrição da linha incorrecto, linha nº {0}.", LineNumber), Field = "Payment/Line/SourceDocumentID/Description", FileID = DocNo, Value = doc.Description, UID = Pk, SupUID = SupPk });
                if (doc.InvoiceDate > DateTime.Now)
                    listErro.Add(new ValidationError { Description = string.Format("Data do documento de origem incorrecta, linha nº {0}.", LineNumber), Field = "Payment/Line/SourceDocumentID/InvoiceDate", FileID = DocNo, Value = doc.InvoiceDate.ToString(), UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(doc.OriginatingON) || doc.OriginatingON.Length > 60)
                    listErro.Add(new ValidationError { Description = string.Format("Número do documento de origem incorrecto, linha nº {0}.", LineNumber), Field = "Payment/Line/SourceDocumentID/OriginatingON", FileID = DocNo, Value = doc.OriginatingON, UID = Pk, SupUID = SupPk });
            }
        }
        return listErro.ToArray();
    }
    public ValidationError[] ValidateTax(string SupPk)
    {
        var listErro = new List<ValidationError>();
        if (Tax != null)
        {
            if (Tax.TaxAmount < 0 && Tax.ItemElementName == ItemChoiceType.TaxAmount)
                listErro.Add(new ValidationError { Description = string.Format("Montante do imposto incorrecto, linha nº {0}.", LineNumber), Field = "Payment/Line/Tax/TaxAmount", FileID = DocNo, Value = Tax.TaxAmount.ToString(), UID = Pk, SupUID = SupPk });
            if ((Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100) && Tax.ItemElementName == ItemChoiceType.TaxPercentage)
                listErro.Add(new ValidationError { Description = string.Format("Montante do imposto incorrecto, linha nº {0}.", LineNumber), Field = "Payment/Line/Tax/TaxPercentage", FileID = DocNo, Value = Tax.TaxPercentage.ToString(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                listErro.Add(new ValidationError { Description = string.Format("Código da taxa incorrecto, linha nº {0}.", LineNumber), Field = "Payment/Line/Tax/TaxCode", FileID = DocNo, Value = Tax.TaxCode, UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCode.Length > 5)
                listErro.Add(new ValidationError { Description = string.Format("País ou região do imposto incorrecto, linha nº {0}.", LineNumber), Field = "Payment/Tax/Line/TaxCountryRegion", FileID = DocNo, Value = Tax.TaxCountryRegion, UID = Pk, SupUID = SupPk });
        }
        return listErro.ToArray();
    }
}

public partial class SourceDocumentsMovementOfGoodsStockMovement : BaseData
{
    public string HashTest { get; set; }
    public string CustomerID { get; set; }
    public ValidationError ValidateDocumentNumber()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(DocumentNumber) || DocumentNumber.Length > 60)
        {
            erro = new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "StockMovement/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, UID = Pk };
        }
        else if (!Regex.IsMatch(DocumentNumber, "([a-zA-Z0-9./_-])+ ([a-zA-Z0-9]*/[0-9]+)"))
        {
            erro = new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "StockMovement/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateHash()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(Hash) || Hash.Length != 172)
        {
            erro = new ValidationError { Description = string.Format("Assinatura do documento {0} de tamanho incorrecto.", DocumentNumber), Field = "StockMovement/Hash", FileID = DocumentNumber, Value = Hash, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateHashControl()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 40)
        {
            erro = new ValidationError { Description = string.Format("Versão da chave privada utilizada na assinatura do documento {0} incorrecta.", DocumentNumber), Field = "StockMovement/HashControl", FileID = DocumentNumber, Value = HashControl, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidatePeriod()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(Period) == false)
        {
            int.TryParse(Period, out int periodo);

            if (periodo < 1 || periodo > 12)
            {
                erro = new ValidationError { Description = string.Format("Mês do período de tributação do documento {0} incorrecto.", DocumentNumber), Field = "StockMovement/Period", FileID = DocumentNumber, Value = Period, UID = Pk };
            }
        }

        return erro;
    }
    public ValidationError ValidateMovementDate()
    {
        ValidationError erro = null;

        if (MovementDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data de emissão do documento {0} incorrecta.", DocumentNumber), Field = "StockMovement/MovementDate", FileID = DocumentNumber, Value = MovementDate.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSystemEntryDate()
    {
        ValidationError erro = null;

        if (SystemEntryDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data da gravação do documento {0} incorrecta.", DocumentNumber), Field = "StockMovement/SystemEntryDate", FileID = DocumentNumber, Value = SystemEntryDate.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateTransactionID()
    {
        ValidationError erro = null;

        if (!string.IsNullOrEmpty(TransactionID) && TransactionID.Length > 70)
        {
            erro = new ValidationError { Description = string.Format("Identificador da transacção do documento {0} incorrecto.", DocumentNumber), Field = "StockMovement/TransactionID", FileID = DocumentNumber, Value = TransactionID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateMovementEndTime()
    {
        ValidationError erro = null;

        if (MovementEndTime == DateTime.MinValue || MovementEndTime > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data e hora de fim de transporte do documento {0} incorrecta.", DocumentNumber), Field = "StockMovement/MovementEndTime", FileID = DocumentNumber, Value = MovementEndTime.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateMovementStartTime()
    {
        ValidationError erro = null;

        if (MovementStartTime > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data e hora de início de transporte do documento {0} incorrecta.", DocumentNumber), Field = "StockMovement/MovementStartTime", FileID = DocumentNumber, Value = MovementStartTime.ToString(), UID = Pk };
        }

        return erro;
    }

    public ValidationError[] ValidateDocumentStatus()
    {
        List<ValidationError> listError = new List<ValidationError>();

        if (DocumentStatus == null)
            listError.Add(new ValidationError { Description = string.Format("Situação do documento {0} inexistente.", DocumentNumber), Field = "StockMovement/DocumentStatus", FileID = DocumentNumber, UID = Pk });

        if (DocumentStatus != null)
        {
            if (DocumentStatus.MovementStatusDate > DateTime.Now)
                listError.Add(new ValidationError { Description = string.Format("Data e hora do estado atual do documento {0} incorrecta.", DocumentNumber), Field = "StockMovement/DocumentStatus/InvoiceStatusDate", FileID = DocumentNumber, Value = DocumentStatus.MovementStatusDate.ToString(), UID = Pk });
            if (string.IsNullOrEmpty(DocumentStatus.Reason) == false && DocumentStatus.Reason.Length > 50)
                listError.Add(new ValidationError { Description = string.Format("Tamanho do motivo da alteração de estado {0} incorrecto.", DocumentNumber), Field = "StockMovement/DocumentStatus/Reason", FileID = DocumentNumber, Value = DocumentStatus.Reason, UID = Pk });
            if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
                listError.Add(new ValidationError { Description = string.Format("Utilizador responsável pelo estado atual do documento {0} incorrecto.", DocumentNumber), Field = "StockMovement/DocumentStatus/SourceID", FileID = DocumentNumber, Value = DocumentStatus.SourceID, UID = Pk });
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateShipTo()
    {
        return ValidateAddress(ShipTo?.Address, "StockMovement/ShipTo");
    }
    public ValidationError[] ValidateShipFrom()
    {
        return ValidateAddress(ShipFrom?.Address, "StockMovement/ShipFrom");
    }
    public ValidationError[] ValidateAddress(AddressStructure address, string path)
    {
        var listError = new List<ValidationError>();

        if (address != null)
        {
            if (string.IsNullOrEmpty(ShipFrom.Address.AddressDetail) || ShipFrom.Address.AddressDetail.Length > 100)
                listError.Add(new ValidationError { Description = string.Format("Tamanho da morada detalhada do documento {0} do local de carga incorrecto.", DocumentNumber), Field = $"{path}/AddressDetail", FileID = DocumentNumber, Value = ShipFrom.Address.AddressDetail, UID = Pk });
            if (string.IsNullOrEmpty(ShipFrom.Address.BuildingNumber) == false && ShipFrom.Address.BuildingNumber.Length > 10)
                listError.Add(new ValidationError { Description = string.Format("Tamanho do número de polícia do documento {0} do local de carga incorrecto.", DocumentNumber), Field = $"{path}/BuildingNumber", FileID = DocumentNumber, Value = ShipFrom.Address.BuildingNumber, UID = Pk });
            if (string.IsNullOrEmpty(ShipFrom.Address.City) || ShipFrom.Address.City.Length > 50)
                listError.Add(new ValidationError { Description = string.Format("Tamanho da localidade do documento {0} do local de carga incorrecto.", DocumentNumber), Field = $"{path}/City", FileID = DocumentNumber, Value = ShipFrom.Address.City, UID = Pk });
            if (string.IsNullOrEmpty(ShipFrom.Address.Country) || ShipFrom.Address.Country.Length != 2)
                listError.Add(new ValidationError { Description = string.Format("Tamanho do País do documento {0} do local de carga incorrecto.", DocumentNumber), Field = $"{path}/Country", FileID = DocumentNumber, Value = ShipFrom.Address.Country, UID = Pk });
            if (string.IsNullOrEmpty(ShipFrom.Address.PostalCode) || ShipFrom.Address.PostalCode.Length > 20)
                listError.Add(new ValidationError { Description = string.Format("Tamanho do código postal do documento {0} do local de carga incorrecto.", DocumentNumber), Field = $"{path}/PostalCode, FileID = DocumentNumber", Value = ShipFrom.Address.PostalCode, UID = Pk });
            if (string.IsNullOrEmpty(ShipFrom.Address.Region) == false && ShipFrom.Address.Region.Length > 50)
                listError.Add(new ValidationError { Description = string.Format("Tamanho do distrito do documento {0} do local de carga incorrecto.", DocumentNumber), Field = $"{path}/Region", FileID = DocumentNumber, Value = ShipFrom.Address.Region, UID = Pk });
            if (string.IsNullOrEmpty(ShipFrom.Address.StreetName) == false && ShipFrom.Address.StreetName.Length > 90)
                listError.Add(new ValidationError { Description = string.Format("Tamanho do nome da rua do documento {0} do local de carga incorrecto.", DocumentNumber), Field = $"{path}/StreetName", FileID = DocumentNumber, Value = ShipFrom.Address.StreetName, UID = Pk });
        }
        if (ShipFrom.DeliveryDate > DateTime.Now)
            listError.Add(new ValidationError { Description = string.Format("Data da receção do documento {0} incorrecto.", DocumentNumber), Field = $"{path}/DeliveryDate", FileID = DocumentNumber, Value = ShipFrom.DeliveryDate.ToString(), UID = Pk });

        return listError.ToArray();
    }
    public ValidationError[] ValidateDocumentTotals()
    {
        List<ValidationError> listError = new List<ValidationError>();

        if (DocumentTotals == null)
            listError.Add(new ValidationError { Description = string.Format("Totais do documento {0} inexistentes.", DocumentNumber), Field = "StockMovement/DocumentTotals", FileID = DocumentNumber, UID = Pk });

        if (DocumentTotals != null)
        {
            if (DocumentTotals.Currency != null)
            {
                if (DocumentTotals.Currency.CurrencyAmount < 0)
                    listError.Add(new ValidationError { Description = string.Format("Valor total em moeda estrangeira do documento {0} incorrecto.", DocumentNumber), Field = "StockMovement/DocumentTotals/CurrencyAmount", FileID = DocumentNumber, Value = DocumentTotals.Currency.CurrencyAmount.ToString(), UID = Pk });
                if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length > 3)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do código de moeda do documento {0} incorrecto.", DocumentNumber), Field = "StockMovement/DocumentTotals/CurrencyCode", FileID = DocumentNumber, Value = DocumentTotals.Currency.CurrencyCode, UID = Pk });
                if (DocumentTotals.Currency.ExchangeRate < 0)
                    listError.Add(new ValidationError { Description = string.Format("Taxa de câmbio do documento {0} incorrecta.", DocumentNumber), Field = "StockMovement/DocumentTotals/ExchangeRate", FileID = DocumentNumber, Value = DocumentTotals.Currency.ExchangeRate.ToString(), UID = Pk });
            }

            if (DocumentTotals.GrossTotal < 0)
                listError.Add(new ValidationError { Description = string.Format("Total do documento {0} incorrecto.", DocumentNumber), Field = "StockMovement/DocumentTotals/GrossTotal", FileID = DocumentNumber, Value = DocumentTotals.GrossTotal.ToString(), UID = Pk });
            if (DocumentTotals.NetTotal < 0)
                listError.Add(new ValidationError { Description = string.Format("Total do documento {0} sem impostos incorrecto.", DocumentNumber), Field = "StockMovement/DocumentTotals/NetTotal", FileID = DocumentNumber, Value = DocumentTotals.NetTotal.ToString(), UID = Pk });
            if (DocumentTotals.TaxPayable < 0)
                listError.Add(new ValidationError { Description = string.Format("Valor do imposto a pagar do documento {0} incorrecto.", DocumentNumber), Field = "StockMovement/DocumentTotals/TaxPayable", FileID = DocumentNumber, Value = DocumentTotals.TaxPayable.ToString(), UID = Pk });
        }

        return listError.ToArray();
    }
}

public partial class SourceDocumentsMovementOfGoodsStockMovementLine : BaseData
{
    /// <summary>
    /// Link to the doc
    /// </summary>
    public string DocNo { get; set; }

    public ValidationError ValidateLineNumber(string SupPk)
    {
        ValidationError erro = null;
        int num = -1;
        if (!string.IsNullOrEmpty(LineNumber))
            int.TryParse(LineNumber, out num);

        if (string.IsNullOrEmpty(LineNumber) || num == -1)
        {
            erro = new ValidationError { Description = string.Format("Número de linha incorrecto {0}.", LineNumber), Field = "StockMovement/Line/LineNumber", FileID = DocNo, Value = LineNumber, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateProductCode(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 30)
        {
            erro = new ValidationError { Description = string.Format("Identificador do produto ou serviço incorrecto linha nº {0}.", LineNumber), Field = "StockMovement/Line/ProductCode", FileID = DocNo, Value = ProductCode, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateProductDescription(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length > 200)
        {
            erro = new ValidationError { Description = string.Format("Descrição do produto ou serviço incorrecta, linha nº {0}.", LineNumber), Field = "StockMovement/Line/ProductDescription", FileID = DocNo, Value = ProductDescription, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateQuantity(string SupPk)
    {
        ValidationError erro = null;
        if (Quantity <= 0)
        {
            erro = new ValidationError { Description = string.Format("Quantidade incorrecta, linha nº {0}.", LineNumber), Field = "StockMovement/Line/Quantity", FileID = DocNo, Value = Quantity.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateUnitOfMeasure(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
        {
            erro = new ValidationError { Description = string.Format("Unidade de medida incorrecta, linha nº {0}.", LineNumber), Field = "StockMovement/Line/UnitOfMeasure", FileID = DocNo, Value = UnitOfMeasure, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateUnitPrice(string SupPk)
    {
        ValidationError erro = null;
        if (UnitPrice < 0)
        {
            erro = new ValidationError { Description = string.Format("Preço unitário incorrecto, linha nº {0}.", LineNumber), Field = "StockMovement/Line/UnitPrice", FileID = DocNo, Value = UnitPrice.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }

    public ValidationError[] ValidateOrderReferences(string SupPk)
    {
        var listError = new List<ValidationError>();

        if (OrderReferences != null && OrderReferences.Length > 0)
        {
            foreach (var referencia in OrderReferences)
            {
                if (string.IsNullOrEmpty(referencia.OriginatingON) == false && referencia.OriginatingON.Length > 60)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do número do documento de origem na linha nº {0} incorrecto.", LineNumber), Field = "StockMovement/Line/OrderReferences/OriginatingON", FileID = DocNo, Value = referencia.OriginatingON, UID = Pk, SupUID = SupPk });
                if (referencia.OrderDate == DateTime.MinValue || referencia.OrderDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data do documento de origem na linha nº {0} incorrecto.", LineNumber), Field = "StockMovement/Line/OrderReferences/OrderDate", FileID = DocNo, Value = referencia.OrderDate.ToString(), UID = Pk, SupUID = SupPk });
            }
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateTax(string SupPk)
    {
        var listError = new List<ValidationError>();

        if (Tax == null)
            listError.Add(new ValidationError { Description = string.Format("Taxa de imposto na linha nº {0} inexistente.", LineNumber), Field = "StockMovement/Line/Tax", FileID = DocNo, UID = Pk, SupUID = SupPk });

        if (Tax != null)
        {
            if (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100)
                listError.Add(new ValidationError { Description = string.Format("Percentagem da taxa do imposto na linha nº {0} incorrecto.", LineNumber), Field = "StockMovement/Line/Tax/TaxPercentage", FileID = DocNo, Value = Tax.TaxPercentage.ToString(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                listError.Add(new ValidationError { Description = string.Format("Código da taxa na linha nº {0} incorrecto.", LineNumber), Field = "StockMovement/Line/Tax/TaxCode", FileID = DocNo, Value = Tax.TaxCode, UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                listError.Add(new ValidationError { Description = string.Format("País ou região do imposto na linha nº {0} incorrecto.", LineNumber), Field = "StockMovement/Line/Tax/TaxCountryRegion", FileID = DocNo, Value = Tax.TaxCountryRegion, UID = Pk, SupUID = SupPk });
        }

        return listError.ToArray();
    }
}

public partial class SourceDocumentsWorkingDocumentsWorkDocument : BaseData
{
    public string HashTest { get; set; }

    public ValidationError ValidateDocumentNumber()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(DocumentNumber) || DocumentNumber.Length > 60)
        {
            erro = new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "WorkDocument/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, UID = Pk };
        }
        else if (!DocumentNumberRegex().IsMatch(DocumentNumber))
        {
            erro = new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "WorkDocument/DocumentNumber", FileID = DocumentNumber, Value = DocumentNumber, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateHash()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(Hash) || Hash.Length != 172)
        {
            erro = new ValidationError { Description = string.Format("Assinatura do documento {0} de tamanho incorrecto.", DocumentNumber), Field = "WorkDocument/Hash", FileID = DocumentNumber, Value = Hash, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateHashControl()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 40)
        {
            erro = new ValidationError { Description = string.Format("Versão da chave privada utilizada na assinatura do documento {0} incorrecta.", DocumentNumber), Field = "WorkDocument/HashControl", FileID = DocumentNumber, Value = HashControl, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidatePeriod()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(Period) == false)
        {
            int.TryParse(Period, out int periodo);

            if (periodo < 1 || periodo > 12)
            {
                erro = new ValidationError { Description = string.Format("Mês do período de tributação do documento {0} incorrecto.", DocumentNumber), Field = "WorkDocument/Period", FileID = DocumentNumber, Value = Period, UID = Pk };
            }
        }

        return erro;
    }
    public ValidationError ValidateWorkDate()
    {
        ValidationError erro = null;

        if (WorkDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data de emissão do documento {0} incorrecta.", DocumentNumber), Field = "WorkDocument/WorkDate", FileID = DocumentNumber, Value = WorkDate.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSystemEntryDate()
    {
        ValidationError erro = null;

        if (SystemEntryDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data da gravação do documento {0} incorrecta.", DocumentNumber), Field = "WorkDocument/SystemEntryDate", FileID = DocumentNumber, Value = SystemEntryDate.ToString(), UID = Pk };
        }

        return erro;
    }

    public ValidationError[] ValidateDocumentStatus()
    {
        var listError = new List<ValidationError>();

        if (DocumentStatus == null)
            listError.Add(new ValidationError { Description = string.Format("Situação do documento {0} inexistente.", DocumentNumber), Field = "WorkDocument/DocumentStatus", FileID = DocumentNumber, UID = Pk });

        if (DocumentStatus != null)
        {
            if (DocumentStatus.WorkStatusDate > DateTime.Now)
                listError.Add(new ValidationError { Description = string.Format("Data e hora do estado atual do documento {0} incorrecta.", DocumentNumber), Field = "WorkDocument/DocumentStatus/InvoiceStatusDate", FileID = DocumentNumber, Value = DocumentStatus.WorkStatusDate.ToString(), UID = Pk });
            if (string.IsNullOrEmpty(DocumentStatus.Reason) == false && DocumentStatus.Reason.Length > 50)
                listError.Add(new ValidationError { Description = string.Format("Tamanho do motivo da alteração de estado {0} incorrecto.", DocumentNumber), Field = "WorkDocument/DocumentStatus/Reason", FileID = DocumentNumber, Value = DocumentStatus.Reason, UID = Pk });
            if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
                listError.Add(new ValidationError { Description = string.Format("Utilizador responsável pelo estado atual do documento {0} incorrecto.", DocumentNumber), Field = "WorkDocument/DocumentStatus/SourceID", FileID = DocumentNumber, Value = DocumentStatus.SourceID, UID = Pk });
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateDocumentTotals()
    {
        var listError = new List<ValidationError>();

        if (DocumentTotals == null)
            listError.Add(new ValidationError { Description = string.Format("Totais do documento {0} inexistentes.", DocumentNumber), Field = "WorkDocument/DocumentTotals", FileID = DocumentNumber, UID = Pk });

        if (DocumentTotals != null)
        {
            if (DocumentTotals.Currency != null)
            {
                if (DocumentTotals.Currency.CurrencyAmount < 0)
                    listError.Add(new ValidationError { Description = string.Format("Valor total em moeda estrangeira do documento {0} incorrecto.", DocumentNumber), Field = "WorkDocument/DocumentTotals/CurrencyAmount", FileID = DocumentNumber, Value = DocumentTotals.Currency.CurrencyAmount.ToString(), UID = Pk });
                if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length > 3)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do código de moeda do documento {0} incorrecto.", DocumentNumber), Field = "WorkDocument/DocumentTotals/CurrencyCode", FileID = DocumentNumber, Value = DocumentTotals.Currency.CurrencyCode, UID = Pk });
                if (DocumentTotals.Currency.ExchangeRate < 0)
                    listError.Add(new ValidationError { Description = string.Format("Taxa de câmbio do documento {0} incorrecta.", DocumentNumber), Field = "WorkDocument/DocumentTotals/ExchangeRate", FileID = DocumentNumber, Value = DocumentTotals.Currency.ExchangeRate.ToString(), UID = Pk });
            }

            if (DocumentTotals.GrossTotal < 0)
                listError.Add(new ValidationError { Description = string.Format("Total do documento {0} incorrecto.", DocumentNumber), Field = "WorkDocument/DocumentTotals/GrossTotal", FileID = DocumentNumber, Value = DocumentTotals.GrossTotal.ToString(), UID = Pk });
            if (DocumentTotals.NetTotal < 0)
                listError.Add(new ValidationError { Description = string.Format("Total do documento {0} sem impostos incorrecto.", DocumentNumber), Field = "WorkDocument/DocumentTotals/NetTotal", FileID = DocumentNumber, Value = DocumentTotals.NetTotal.ToString(), UID = Pk });
            if (DocumentTotals.TaxPayable < 0)
                listError.Add(new ValidationError { Description = string.Format("Valor do imposto a pagar do documento {0} incorrecto.", DocumentNumber), Field = "WorkDocument/DocumentTotals/TaxPayable", FileID = DocumentNumber, Value = DocumentTotals.TaxPayable.ToString(), UID = Pk });
        }

        return listError.ToArray();
    }

    [GeneratedRegex("([a-zA-Z0-9./_-])+ ([a-zA-Z0-9]*/[0-9]+)")]
    private static partial Regex DocumentNumberRegex();
}

public partial class SourceDocumentsWorkingDocumentsWorkDocumentLine : BaseData
{
    /// <summary>
    /// Link to the doc
    /// </summary>
    public string DocNo { get; set; }

    public ValidationError ValidateLineNumber(string SupPk)
    {
        ValidationError erro = null;
        int num = -1;
        if (!string.IsNullOrEmpty(LineNumber))
            int.TryParse(LineNumber, out num);

        if (string.IsNullOrEmpty(LineNumber) || num == -1)
        {
            erro = new ValidationError { Description = string.Format("Número de linha incorrecto {0}.", LineNumber), Field = "WorkDocument/Line/LineNumber", FileID = DocNo, Value = LineNumber, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateProductCode(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 30)
        {
            erro = new ValidationError { Description = string.Format("Identificador do produto ou serviço incorrecto, linha nº {0}.", LineNumber), Field = "WorkDocument/Line/ProductCode", FileID = DocNo, Value = ProductCode, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateProductDescription(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length > 200)
        {
            erro = new ValidationError { Description = string.Format("Descrição do produto ou serviço incorrecta, linha nº {0}.", LineNumber), Field = "WorkDocument/Line/ProductDescription", FileID = DocNo, Value = ProductDescription, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateQuantity(string SupPk)
    {
        ValidationError erro = null;
        if (Quantity <= 0)
        {
            erro = new ValidationError { Description = string.Format("Quantidade incorrecta, linha nº {0}.", LineNumber), Field = "WorkDocument/Line/Quantity", FileID = DocNo, Value = Quantity.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateUnitOfMeasure(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
        {
            erro = new ValidationError { Description = string.Format("Unidade de medida incorrecta, linha nº {0}.", LineNumber), Field = "WorkDocument/Line/UnitOfMeasure", FileID = DocNo, Value = UnitOfMeasure, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateUnitPrice(string SupPk)
    {
        ValidationError erro = null;
        if (UnitPrice == 0)
        {
            erro = new ValidationError { Description = string.Format("Preço unitário incorrecto, linha nº {0}.", LineNumber), Field = "WorkDocument/Line/UnitPrice", FileID = DocNo, Value = UnitPrice.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateTaxPointDate(string SupPk)
    {
        ValidationError erro = null;
        if (TaxPointDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data de envio da mercadoria ou prestação do serviço incorrecta, linha nº {0}.", LineNumber), Field = "WorkDocument/Line/TaxPointDate", FileID = DocNo, Value = TaxPointDate.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }

    public ValidationError[] ValidateOrderReferences(string SupPk)
    {
        var listError = new List<ValidationError>();

        if (OrderReferences != null && OrderReferences.Length > 0)
        {
            foreach (var referencia in OrderReferences)
            {
                if (string.IsNullOrEmpty(referencia.OriginatingON) == false && referencia.OriginatingON.Length > 60)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do número do documento de origem na linha nº {0} incorrecto.", LineNumber), Field = "WorkDocument/Line/OrderReferences/OriginatingON", FileID = DocNo, Value = referencia.OriginatingON, UID = Pk, SupUID = SupPk });
                if (referencia.OrderDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data do documento de origem na linha nº {0} incorrecta.", LineNumber), Field = "WorkDocument/Line/OrderReferences/OrderDate", FileID = DocNo, Value = referencia.OrderDate.ToString(), UID = Pk, SupUID = SupPk });
            }
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateTax(string SupPk)
    {
        var listError = new List<ValidationError>();

        if (Tax == null)
            listError.Add(new ValidationError { Description = string.Format("Taxa de imposto na linha nº {0} inexistente.", LineNumber), Field = "WorkDocument/Line/Tax", FileID = DocNo, UID = Pk, SupUID = SupPk });

        if (Tax != null)
        {
            if (Tax.ItemElementName == ItemChoiceType1.TaxAmount && Tax.TaxAmount < 0)
                listError.Add(new ValidationError { Description = string.Format("Montante do imposto na linha nº {0} incorrecto.", LineNumber), Field = "WorkDocument/Line/Tax/TaxAmount", FileID = DocNo, Value = Tax.TaxAmount.ToString(), UID = Pk, SupUID = SupPk });
            if (Tax.ItemElementName == ItemChoiceType1.TaxPercentage && (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100))
                listError.Add(new ValidationError { Description = string.Format("Percentagem da taxa do imposto na linha nº {0} incorrecto.", LineNumber), Field = "WorkDocument/Line/Tax/TaxPercentage", FileID = DocNo, Value = Tax.TaxPercentage.ToString(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                listError.Add(new ValidationError { Description = string.Format("Código da taxa na linha nº {0} incorrecto.", LineNumber), Field = "WorkDocument/Line/Tax/TaxCode", FileID = DocNo, Value = Tax.TaxCode, UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                listError.Add(new ValidationError { Description = string.Format("País ou região do imposto na linha nº {0} incorrecto.", LineNumber), Field = "WorkDocument/Line/Tax/TaxCountryRegion", FileID = DocNo, Value = Tax.TaxCountryRegion, UID = Pk, SupUID = SupPk });
        }

        return listError.ToArray();
    }
}

public partial class SourceDocumentsSalesInvoicesInvoice : BaseData
{
    public string HashTest { get; set; }

    public Customer Customer { get; set; }

    public ValidationError ValidateATCUD()
    {
        ValidationError erro = null;
        if (string.IsNullOrWhiteSpace(ATCUD))
        {
            erro = new ValidationError { Description = "ATCUD não pode ser vazio.", Field = "Invoice/ATCUD", Value = ATCUD, UID = Pk };
        }
        else if (ATCUD != "0")
        {
            string[] parts = ATCUD.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts == null || parts.Length != 2)
            {
                erro = new ValidationError { Description = "ATCUD deveria ser formado pela concatenação dos campos «CodigodeValidação-NumeroSequencial».", Field = "Invoice/ATCUD", Value = ATCUD, UID = Pk };
            }
            else
            {
                if (parts[0].Length < 8)
                {
                    erro = new ValidationError { Description = "CodigodeValidação do ATCUD com tamanho incorrecto.", Field = "Invoice/ATCUD", Value = ATCUD, UID = Pk };
                }
            }
        }
        return erro;
    }
    public ValidationError ValidateInvoiceNo()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(InvoiceNo) || InvoiceNo.Length > 60)
        {
            erro = new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "Invoice/InvoiceNo", Value = InvoiceNo, UID = Pk };
        }
        else if (!Regex.IsMatch(InvoiceNo, "([a-zA-Z0-9./_-])+ ([a-zA-Z0-9]*/[0-9]+)"))
        {
            erro = new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "Invoice/InvoiceNo", Value = InvoiceNo, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateHash()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(Hash) || Hash.Length != 172)
        {
            erro = new ValidationError { Description = string.Format("Assinatura do documento {0} de tamanho incorrecto.", InvoiceNo), Field = "Invoice/Hash", Value = Hash, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateHashControl()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 40)
        {
            erro = new ValidationError { Description = string.Format("Versão da chave privada utilizada na assinatura do documento {0} incorrecta.", InvoiceNo), Field = "Invoice/HashControl", Value = HashControl, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidatePeriod()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(Period) == false)
        {
            int.TryParse(Period, out int periodo);

            if (periodo < 1 || periodo > 12)
            {
                erro = new ValidationError { Description = string.Format("Mês do período de tributação do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/Period", Value = Period, UID = Pk };
            }
        }

        return erro;
    }
    public ValidationError ValidateInvoiceDate()
    {
        ValidationError erro = null;

        if (InvoiceDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data de emissão do documento {0} incorrecta.", InvoiceNo), Field = "Invoice/InvoiceDate", Value = InvoiceDate.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSystemEntryDate()
    {
        ValidationError erro = null;

        if (SystemEntryDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data da gravação do documento {0} incorrecta.", InvoiceNo), Field = "Invoice/SystemEntryDate", Value = SystemEntryDate.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateTransactionID()
    {
        ValidationError erro = null;

        if (!string.IsNullOrEmpty(TransactionID) && TransactionID.Length > 70)
        {
            erro = new ValidationError { Description = string.Format("Identificador da transacção do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/TransactionID", Value = TransactionID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateCustomerID()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
        {
            erro = new ValidationError { Description = string.Format("Chave única da tabela de clientes no documento {0} incorrecta.", InvoiceNo), Field = "Invoice/CustomerID", Value = CustomerID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSourceID()
    {
        ValidationError erro = null;

        if (string.IsNullOrEmpty(SourceID) || SourceID.Length > 30)
        {
            erro = new ValidationError { Description = string.Format("Utilizador que gerou o documento {0} incorrecto.", InvoiceNo), Field = "Invoice/SourceID", Value = CustomerID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateMovementEndTime()
    {
        ValidationError erro = null;

        if (MovementEndTime == DateTime.MinValue || MovementEndTime > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data e hora de fim de transporte do documento {0} incorrecta.", InvoiceNo), Field = "Invoice/MovementEndTime", Value = MovementEndTime.ToString(), UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateMovementStartTime()
    {
        ValidationError erro = null;

        if (MovementStartTime == DateTime.MinValue || MovementStartTime > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data e hora de início de transporte do documento {0} incorrecta.", InvoiceNo), Field = "Invoice/MovementStartTime", Value = MovementStartTime.ToString(), UID = Pk };
        }

        return erro;
    }

    public ValidationError[] ValidateDocumentStatus()
    {
        var listError = new List<ValidationError>();

        if (DocumentStatus == null)
            listError.Add(new ValidationError { Description = string.Format("Situação do documento {0} inexistente.", InvoiceNo), Field = "Invoice/DocumentStatus", UID = Pk });

        if (DocumentStatus != null)
        {
            if (DocumentStatus.InvoiceStatusDate > DateTime.Now)
                listError.Add(new ValidationError { Description = string.Format("Data e hora do estado atual do documento {0} incorrecta.", InvoiceNo), Field = "Invoice/DocumentStatus/InvoiceStatusDate", Value = DocumentStatus.InvoiceStatusDate.ToString(), UID = Pk });
            if (string.IsNullOrEmpty(DocumentStatus.Reason) == false && DocumentStatus.Reason.Length > 50)
                listError.Add(new ValidationError { Description = string.Format("Tamanho do motivo da alteração de estado {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentStatus/Reason", Value = DocumentStatus.Reason, UID = Pk });
            if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
                listError.Add(new ValidationError { Description = string.Format("Utilizador responsável pelo estado atual do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentStatus/SourceID", Value = DocumentStatus.SourceID, UID = Pk });
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateSpecialRegimes()
    {
        var listError = new List<ValidationError>();

        if (SpecialRegimes != null)
        {
            int.TryParse(SpecialRegimes.SelfBillingIndicator, out int auto);
            if (string.IsNullOrEmpty(SpecialRegimes.SelfBillingIndicator) || (auto != 0 && auto != 1))
                listError.Add(new ValidationError { Description = string.Format("Indicador de autofaturação do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/SpecialRegimes/SelfBillingIndicator", Value = SpecialRegimes.SelfBillingIndicator, UID = Pk });

            if (string.IsNullOrEmpty(SpecialRegimes.CashVATSchemeIndicator) || (SpecialRegimes.CashVATSchemeIndicator != "0" && SpecialRegimes.CashVATSchemeIndicator != "1"))
                listError.Add(new ValidationError { Description = string.Format("Indicador da existência de adesão ao regime de IVA de Caixa do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/SpecialRegimes/CashVATSchemeIndicator", Value = SpecialRegimes.CashVATSchemeIndicator, UID = Pk });
            if (string.IsNullOrEmpty(SpecialRegimes.ThirdPartiesBillingIndicator) || (SpecialRegimes.ThirdPartiesBillingIndicator != "0" && SpecialRegimes.CashVATSchemeIndicator != "1"))
                listError.Add(new ValidationError { Description = string.Format("Indicador da existência de adesão ao regime de IVA de Caixa do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/SpecialRegimes/ThirdPartiesBillingIndicator", Value = SpecialRegimes.ThirdPartiesBillingIndicator, UID = Pk });
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateShipTo()
    {
        return ValidateShipping(ShipTo, "Invoice/ShipTo");
    }
    public ValidationError[] ValidateShipFrom()
    {
        return ValidateShipping(ShipFrom, "Invoice/ShipFrom");
    }
    public ValidationError[] ValidateShipping(ShippingPointStructure ship, string path)
    {
        var listError = new List<ValidationError>();

        if (ship != null)
        {
            if (ship.Address != null)
            {
                if (string.IsNullOrEmpty(ship.Address.AddressDetail) || ship.Address.AddressDetail.Length > 100)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho da morada detalhada do documento {0} do local de carga incorrecto.", InvoiceNo), Field = $"{path}/AddressDetail", FileID = InvoiceNo, Value = ship.Address.AddressDetail, UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.BuildingNumber) == false && ship.Address.BuildingNumber.Length > 10)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do número de polícia do documento {0} do local de carga incorrecto.", InvoiceNo), Field = $"{path}/BuildingNumber", FileID = InvoiceNo, Value = ship.Address.BuildingNumber, UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.City) || ship.Address.City.Length > 50)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho da localidade do documento {0} do local de carga incorrecto.", InvoiceNo), Field = $"{path}/City", FileID = InvoiceNo, Value = ship.Address.City, UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.Country) || ship.Address.Country.Length != 2)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do País do documento {0} do local de carga incorrecto.", InvoiceNo), Field = $"{path}/Country", FileID = InvoiceNo, Value = ship.Address.Country, UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.PostalCode) || ship.Address.PostalCode.Length > 20)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do código postal do documento {0} do local de carga incorrecto.", InvoiceNo), Field = $"{path}/PostalCode", FileID = InvoiceNo, Value = ship.Address.PostalCode, UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.Region) == false && ship.Address.Region.Length > 50)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do distrito do documento {0} do local de carga incorrecto.", InvoiceNo), Field = $"{path}/Region", FileID = InvoiceNo, Value = ship.Address.Region, UID = Pk });
                if (string.IsNullOrEmpty(ship.Address.StreetName) == false && ship.Address.StreetName.Length > 90)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do nome da rua do documento {0} do local de carga incorrecto.", InvoiceNo), Field = $"{path}/StreetName", FileID = InvoiceNo, Value = ship.Address.StreetName, UID = Pk });
            }
            if (ship.DeliveryDate > DateTime.Now)
                listError.Add(new ValidationError { Description = string.Format("Data da receção do documento {0} incorrecto.", InvoiceNo), Field = $"{path}/DeliveryDate", FileID = InvoiceNo, Value = ship.DeliveryDate.ToString(), UID = Pk });
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateDocumentTotals()
    {
        var listError = new List<ValidationError>();

        if (DocumentTotals == null)
            listError.Add(new ValidationError { Description = string.Format("Totais do documento {0} inexistentes.", InvoiceNo), Field = "Invoice/DocumentTotals", UID = Pk });

        if (DocumentTotals != null)
        {
            if (DocumentTotals.Currency != null)
            {
                if (DocumentTotals.Currency.CurrencyAmount < 0)
                    listError.Add(new ValidationError { Description = string.Format("Valor total em moeda estrangeira do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/Currency/CurrencyAmount", FileID = InvoiceNo, Value = DocumentTotals.Currency.CurrencyAmount.ToString(), UID = Pk });
                if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length > 3)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do código de moeda do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/Currency/CurrencyCode", FileID = InvoiceNo, Value = DocumentTotals.Currency.CurrencyCode, UID = Pk });
                if (DocumentTotals.Currency.ExchangeRate < 0)
                    listError.Add(new ValidationError { Description = string.Format("Taxa de câmbio do documento {0} incorrecta.", InvoiceNo), Field = "Invoice/DocumentTotals/Currency/ExchangeRate", FileID = InvoiceNo, Value = DocumentTotals.Currency.ExchangeRate.ToString(), UID = Pk });
            }

            if (DocumentTotals.Settlement != null && DocumentTotals.Settlement.Length > 0)
            {
                foreach (var acordo in DocumentTotals.Settlement)
                {
                    if (string.IsNullOrEmpty(acordo.PaymentTerms) == false && acordo.PaymentTerms.Length > 100)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do acordo de pagamento do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/Settlement/PaymentTerms", FileID = InvoiceNo, Value = acordo.PaymentTerms, UID = Pk });
                    if (acordo.SettlementAmount < 0)
                        listError.Add(new ValidationError { Description = string.Format("Acordos de descontos futuros do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/Settlement/SettlementAmount", FileID = InvoiceNo, Value = acordo.SettlementAmount.ToString(), UID = Pk });
                    if (acordo.SettlementDate == DateTime.MinValue)
                        listError.Add(new ValidationError { Description = string.Format("Data acordada para o desconto do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/Settlement/SettlementDate", FileID = InvoiceNo, Value = acordo.SettlementDate.ToString(), UID = Pk });
                    if (string.IsNullOrEmpty(acordo.SettlementDiscount) == false && acordo.SettlementDiscount.Length > 30)
                        listError.Add(new ValidationError { Description = string.Format("Montante do desconto do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/Settlement/SettlementAmount", FileID = InvoiceNo, Value = acordo.SettlementAmount.ToString(), UID = Pk });
                }
            }

            if (DocumentTotals.GrossTotal < 0)
                listError.Add(new ValidationError { Description = string.Format("Total do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/GrossTotal", FileID = InvoiceNo, Value = DocumentTotals.GrossTotal.ToString(), UID = Pk });
            if (DocumentTotals.NetTotal < 0)
                listError.Add(new ValidationError { Description = string.Format("Total do documento {0} sem impostos incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/NetTotal", FileID = InvoiceNo, Value = DocumentTotals.NetTotal.ToString(), UID = Pk });
            if (DocumentTotals.TaxPayable < 0)
                listError.Add(new ValidationError { Description = string.Format("Valor do imposto a pagar do documento {0} incorrecto.", InvoiceNo), Field = "Invoice/DocumentTotals/TaxPayable", FileID = InvoiceNo, Value = DocumentTotals.TaxPayable.ToString(), UID = Pk });
        }

        return listError.ToArray();
    }
}

public partial class SourceDocumentsSalesInvoicesInvoiceLine : BaseData
{
    /// <summary>
    /// Link to the invoice
    /// </summary>
    public string InvoiceNo { get; set; }

    public ValidationError ValidateLineNumber(string SupPk)
    {
        ValidationError erro = null;
        int.TryParse(LineNumber, out int num);

        if (string.IsNullOrEmpty(LineNumber) || num == -1)
        {
            erro = new ValidationError { Description = string.Format("Número de linha incorrecto {0}.", LineNumber), Field = "Invoice/Line/LineNumber", FileID = InvoiceNo, Value = LineNumber, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateProductCode(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 60)
        {
            erro = new ValidationError { Description = string.Format("Identificador do produto ou serviço incorrecto, linha nº {0}.", LineNumber), Field = "Invoice/Line/ProductCode", FileID = InvoiceNo, Value = ProductCode, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateProductDescription(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length > 200)
        {
            erro = new ValidationError { Description = string.Format("Descrição do produto ou serviço incorrecta, linha nº {0}.", LineNumber), Field = "Invoice/Line/ProductDescription", FileID = InvoiceNo, Value = ProductDescription, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateQuantity(string SupPk)
    {
        ValidationError erro = null;
        if (Quantity <= 0)
        {
            erro = new ValidationError { Description = string.Format("Quantidade incorrecta, linha nº {0}.", LineNumber), Field = "Invoice/Line/Quantity", FileID = InvoiceNo, Value = Quantity.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateUnitOfMeasure(string SupPk)
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
        {
            erro = new ValidationError { Description = string.Format("Unidade de medida incorrecta, linha nº {0}.", LineNumber), Field = "Invoice/Line/UnitOfMeasure", FileID = InvoiceNo, Value = UnitOfMeasure, UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateUnitPrice(string SupPk)
    {
        ValidationError erro = null;
        if (UnitPrice == 0)
        {
            erro = new ValidationError { Description = string.Format("Preço unitário incorrecto, linha nº {0}.", LineNumber), Field = "Invoice/Line/UnitPrice", FileID = InvoiceNo, Value = UnitPrice.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }
    public ValidationError ValidateTaxPointDate(string SupPk)
    {
        ValidationError erro = null;
        if (TaxPointDate > DateTime.Now)
        {
            erro = new ValidationError { Description = string.Format("Data de envio da mercadoria ou prestação do serviço incorrecta, linha nº {0}.", LineNumber), Field = "Invoice/Line/TaxPointDate", FileID = InvoiceNo, Value = TaxPointDate.ToString(), UID = Pk, SupUID = SupPk };
        }
        return erro;
    }

    public ValidationError[] ValidateOrderReferences(string SupPk)
    {
        var listError = new List<ValidationError>();

        if (OrderReferences != null && OrderReferences.Length > 0)
        {
            foreach (var referencia in OrderReferences)
            {
                if (string.IsNullOrEmpty(referencia.OriginatingON) == false && referencia.OriginatingON.Length > 60)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do número do documento de origem na linha nº {0} incorrecto.", LineNumber), Field = "Invoice/Line/OrderReferences/OriginatingON", FileID = InvoiceNo, Value = referencia.OriginatingON, UID = Pk, SupUID = SupPk });
                if (referencia.OrderDate == DateTime.MinValue || referencia.OrderDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data do documento de origem na linha nº {0} incorrecto.", LineNumber), Field = "Invoice/Line/OrderReferences/OrderDate", FileID = InvoiceNo, Value = referencia.OrderDate.ToString(), UID = Pk, SupUID = SupPk });
            }
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateReferences(string SupPk)
    {
        var listError = new List<ValidationError>();

        if (References != null && References.Length > 0)
        {
            foreach (var referencia in References)
            {
                if (string.IsNullOrEmpty(referencia.Reason) == false && referencia.Reason.Length > 50)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do motivo da emissão na linha nº {0} incorrecto.", LineNumber), Field = "Invoice/Line/References/Reason", FileID = InvoiceNo, Value = referencia.Reason, UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(referencia.Reference) == false && referencia.Reference.Length > 60)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho da referência à fatura ou fatura simplificada na linha nº {0} incorrecto.", LineNumber), Field = "Invoice/Line/References/Reference", FileID = InvoiceNo, Value = referencia.Reference, UID = Pk, SupUID = SupPk });
            }
        }

        return listError.ToArray();
    }
    public ValidationError[] ValidateTax(string SupPk)
    {
        var listError = new List<ValidationError>();

        if (Tax == null)
            listError.Add(new ValidationError { Description = string.Format("Taxa de imposto na linha nº {0} inexistente.", LineNumber), Field = "Invoice/Line/Tax", UID = Pk, SupUID = SupPk });

        if (Tax != null)
        {
            if (Tax.ItemElementName == ItemChoiceType1.TaxAmount && Tax.TaxAmount < 0)
                listError.Add(new ValidationError { Description = string.Format("Montante do imposto na linha nº {0} incorrecto.", LineNumber), Field = "Invoice/Line/Tax/TaxAmount", FileID = InvoiceNo, Value = Tax.TaxAmount.ToString(), UID = Pk, SupUID = SupPk });
            if (Tax.ItemElementName == ItemChoiceType1.TaxPercentage && (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100))
                listError.Add(new ValidationError { Description = string.Format("Percentagem da taxa do imposto na linha nº {0} incorrecto.", LineNumber), Field = "Invoice/Line/Tax/TaxPercentage", FileID = InvoiceNo, Value = Tax.TaxPercentage.ToString(), UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                listError.Add(new ValidationError { Description = string.Format("Código da taxa na linha nº {0} incorrecto.", LineNumber), Field = "Invoice/Line/Tax/TaxCode", FileID = InvoiceNo, Value = Tax.TaxCode, UID = Pk, SupUID = SupPk });
            if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                listError.Add(new ValidationError { Description = string.Format("País ou região do imposto na linha nº {0} incorrecto.", LineNumber), Field = "Invoice/Line/Tax/TaxCountryRegion", FileID = InvoiceNo, Value = Tax.TaxCountryRegion, UID = Pk, SupUID = SupPk });
        }

        return listError.ToArray();
    }
}

public partial class Header : BaseData
{
    public ValidationError ValidateTaxRegistrationNumber()
    {
        ValidationError erro = null;
        if (!Validations.CheckTaxRegistrationNumber(TaxRegistrationNumber))
        {
            erro = new ValidationError { Description = "NIF inválido", Field = "Header/TaxRegistrationNumber", Value = TaxRegistrationNumber, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateAuditFileVersion()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(AuditFileVersion) || AuditFileVersion.Length > 10)
        {
            erro = new ValidationError { Description = "Versão do ficheiro SAF-T PT incorrecta.", Field = "Header/AuditFileVersion", Value = AuditFileVersion, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateBusinessName()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(BusinessName) == false)
        {
            if (BusinessName.Length > 60)
            {
                erro = new ValidationError { Description = "Designação comercial incorrecta.", Field = "Header/BusinessName", Value = BusinessName, UID = Pk };
            }
        }
        return erro;
    }
    public ValidationError ValidateAddressDetail()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CompanyAddress?.AddressDetail) || CompanyAddress.AddressDetail.Length > 100)
        {
            erro = new ValidationError { Description = "Morada detalhada incorrecta.", Field = "Header/CompanyAddress/AddressDetail", Value = CompanyAddress?.AddressDetail, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateBuildingNumber()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(CompanyAddress?.BuildingNumber) && CompanyAddress.BuildingNumber.Length > 10)
        {
            erro = new ValidationError { Description = "Número polícia incorrecto.", Field = "Header/CompanyAddress/BuildingNumber", Value = CompanyAddress?.BuildingNumber, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateCity()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CompanyAddress?.City) || CompanyAddress.City.Length > 50)
        {
            erro = new ValidationError { Description = "Localidade incorrecta.", Field = "Header/CompanyAddress/City", Value = CompanyAddress?.City, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateCountry()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CompanyAddress?.Country) || CompanyAddress.Country != "PT")
        {
            erro = new ValidationError { Description = "Localidade incorrecta.", Field = "Header/CompanyAddress/Country", Value = CompanyAddress?.Country, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidatePostalCode()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CompanyAddress?.PostalCode) || CompanyAddress.PostalCode.Length > 50)
        {
            erro = new ValidationError { Description = "Código postal incorrecto.", Field = "Header/CompanyAddress/PostalCode", Value = CompanyAddress?.PostalCode, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateRegion()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(CompanyAddress?.Region) && CompanyAddress.Region.Length > 50)
        {
            erro = new ValidationError { Description = "Distrito incorrecto.", Field = "Header/CompanyAddress/Region", Value = CompanyAddress?.Region, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateStreetName()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(CompanyAddress?.StreetName) && CompanyAddress.StreetName.Length > 90)
        {
            erro = new ValidationError { Description = "Nome da rua incorrecto.", Field = "Header/CompanyAddress/StreetName", Value = CompanyAddress?.StreetName, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateCompanyID()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CompanyID) || CompanyID.Length > 50 || !Regex.IsMatch(CompanyID, "([0-9])+|([a-zA-Z0-9-/]+ [0-9]+)"))
        {
            if (!Validations.CheckTaxRegistrationNumber(CompanyID))
            {
                erro = new ValidationError { Description = "Registo comercial incorrecto.", Field = "Header/CompanyID", Value = CompanyID, UID = Pk };
            }
        }
        return erro;
    }
    public ValidationError ValidateCompanyName()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
        {
            erro = new ValidationError { Description = "Nome empresa incorrecto.", Field = "Header/CompanyName", Value = CompanyName, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateCurrencyCode()
    {
        ValidationError erro = null;
        if (CurrencyCode == null || CurrencyCode.ToString() != "EUR")
        {
            erro = new ValidationError { Description = "Código moeda incorrecto.", Field = "Header/CurrencyCode", Value = string.Format("{0}", CurrencyCode ?? "null"), UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateDateCreated()
    {
        ValidationError erro = null;
        if (DateCreated > DateTime.Now)
        {
            erro = new ValidationError { Description = "Data de criação do ficheiro incorrecta.", Field = "Header/DateCreated", Value = DateCreated.ToString(), UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateEmail()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Email) && (Email.Length > 60 || !Regex.IsMatch(Email, @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.IgnoreCase)))
        {
            erro = new ValidationError { Description = "Email incorrecto.", Field = "Header/Email", Value = Email, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateEndDate()
    {
        ValidationError erro = null;
        if (EndDate == DateTime.MinValue)
        {
            erro = new ValidationError { Description = "Data do fim do periodo incorrecta.", Field = "Header/EndDate", Value = EndDate.ToString(), UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateFax()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
        {
            erro = new ValidationError { Description = "Fax incorrecto.", Field = "Header/Fax", Value = Fax, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateFiscalYear()
    {
        ValidationError erro = null;
        int.TryParse(FiscalYear, out int ano);
        if (string.IsNullOrEmpty(FiscalYear) || FiscalYear.Length > 4 || ano == -1)
        {
            erro = new ValidationError { Description = "Ano fiscal incorrecto.", Field = "Header/FiscalYear", Value = FiscalYear, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateHeaderComment()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(HeaderComment) && HeaderComment.Length > 255)
        {
            erro = new ValidationError { Description = "Comentário demasiado longo.", Field = "Header/HeaderComment", Value = HeaderComment, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateProductCompanyTaxID()
    {
        ValidationError erro = null;
        if (!Validations.CheckTaxRegistrationNumber(ProductCompanyTaxID))
        {
            erro = new ValidationError { Description = "NIF da empresa produtora de saftware inválido.", Field = "Header/ProductCompanyTaxID", Value = ProductCompanyTaxID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateProductID()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductID) || ProductID.Length > 255 || !ProductID.Contains('/'))
        {
            erro = new ValidationError { Description = "Nome da aplicação incorrecto.", Field = "Header/ProductID", Value = ProductID, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateProductVersion()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductVersion) || ProductVersion.Length > 30)
        {
            erro = new ValidationError { Description = "Versão da aplicação incorrecta.", Field = "Header/ProductVersion", Value = ProductVersion, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateSoftwareCertificateNumber()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(SoftwareCertificateNumber) || SoftwareCertificateNumber.Length > 20)
        {
            erro = new ValidationError { Description = "Número de certificação incorrecto.", Field = "SoftwareCertificateNumber", Value = SoftwareCertificateNumber, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateTaxAccountingBasis()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(TaxAccountingBasis.ToString()) || TaxAccountingBasis.ToString().Length > 1)
        {
            erro = new ValidationError { Description = "Sistema contabilístico incorrecto.", Field = "Header/TaxAccountingBasis", Value = TaxAccountingBasis.ToString(), UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateTaxEntity()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(TaxEntity) || TaxEntity.Length > 20)
        {
            erro = new ValidationError { Description = "Identificação do estabelecimento incorrecta.", Field = "Header/TaxEntity", Value = TaxEntity, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateTelephone()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
        {
            erro = new ValidationError { Description = "Identificação do estabelecimento incorrecta.", Field = "Header/Telephone", Value = Telephone, UID = Pk };
        }
        return erro;
    }
    public ValidationError ValidateWebsite()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
        {
            erro = new ValidationError { Description = "Website incorrecto.", Field = "Header/Website", Value = Website, UID = Pk };
        }
        return erro;
    }
}

public partial class Product : BaseData
{
    public string Prices { get; set; }
    public string Taxes { get; set; }

    public ValidationError ValidateProductCode()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 60)
        {
            erro = new ValidationError { Description = "Código do produto inválido", Field = "Product/ProductCode", FileID = ProductCode, Value = ProductCode, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateProductGroup()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(ProductGroup) && ProductGroup.Length > 50)
        {
            erro = new ValidationError { Description = "Família do produto ou serviço inválida", Field = "Product/ProductGroup", FileID = ProductCode, Value = ProductGroup, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateProductDescription()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length > 200)
        {
            erro = new ValidationError { Description = "Descrição do produto ou serviço inválida", Field = "Product/ProductDescription", FileID = ProductCode, Value = ProductDescription, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateProductNumberCode()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(ProductNumberCode) || ProductNumberCode.Length > 50)
        {
            erro = new ValidationError { Description = "Família do produto ou serviço inválida", Field = "Product/ProductNumberCode", FileID = ProductCode, Value = ProductNumberCode, UID = Pk };
        }

        return erro;
    }
}

public partial class Customer : BaseData
{
    public ValidationError ValidateCustomerID()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
        {
            erro = new ValidationError { Description = "Identificador único do cliente inválido", Field = "Customer/CustomerID", FileID = CustomerID, Value = CustomerID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateAccountID()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(AccountID) || AccountID.Length > 30)
        {
            erro = new ValidationError { Description = "Código da conta inválido", Field = "Customer/AccountID", FileID = CustomerID, Value = AccountID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateCustomerTaxID()
    {
        ValidationError erro = null;
        if (BillingAddress?.Country == "PT" && !Validations.CheckTaxRegistrationNumber(CustomerTaxID))
        {
            erro = new ValidationError { Description = "Número de identificação fiscal inválido", Field = "Customer/CustomerTaxID", FileID = CustomerID, Value = CustomerTaxID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateCompanyName()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
        {
            erro = new ValidationError { Description = "Nome da empresa inválido", Field = "Customer/CompanyName", FileID = CustomerID, Value = CompanyName, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateContact()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Contact) && Contact.Length > 50)
        {
            erro = new ValidationError { Description = "Nome do contacto na empresa inválido.", Field = "Customer/Contact", FileID = CustomerID, Value = Contact, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateTelephone()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
        {
            erro = new ValidationError { Description = "Telefone inválido", Field = "Customer/Telephone", FileID = CustomerID, Value = Telephone, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateFax()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
        {
            erro = new ValidationError { Description = "Fax inválido", Field = "Customer/Fax", FileID = CustomerID, Value = Fax, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateEmail()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Email) && Email.Length > 60)
        {
            erro = new ValidationError { Description = "Email inválido", Field = "Customer/Email", FileID = CustomerID, Value = Email, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateWebsite()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
        {
            erro = new ValidationError { Description = "Website inválido", Field = "Customer/Website", FileID = CustomerID, Value = Website, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSelfBillingIndicator()
    {
        ValidationError erro = null;

        int selfBillingIndicator = -1;
        if (!string.IsNullOrEmpty(SelfBillingIndicator))
            int.TryParse(SelfBillingIndicator, out selfBillingIndicator);

        if (string.IsNullOrEmpty(SelfBillingIndicator) || selfBillingIndicator == -1)
        {
            erro = new ValidationError { Description = "Nº de conta inválido", Field = "Customer/SelfBillingIndicator", FileID = CustomerID, Value = SelfBillingIndicator, UID = Pk };
        }

        return erro;
    }

    public ValidationError[] ValidateBillingAddress()
    {
        var listErro = new List<ValidationError>();

        if (BillingAddress == null)
            listErro.Add(new ValidationError { Description = "Morada de faturação inexistente", Field = "Customer/BillingAddress", FileID = CustomerID, UID = Pk });

        if (BillingAddress != null)
        {
            ValidateAddress(BillingAddress, "Customer/BillingAddress", listErro);
        }

        return listErro.ToArray();
    }
    public ValidationError[] ValidateShipToAddress()
    {
        List<ValidationError> listErro = new List<ValidationError>();

        if (ShipToAddress != null && ShipToAddress.Length > 0)
        {
            foreach (var morada in ShipToAddress)
            {
                ValidateAddress(morada, "Customer/ShipToAddress", listErro);
            }
        }

        return listErro.ToArray();
    }
    public void ValidateAddress(AddressStructure address, string path, List<ValidationError> listErro)
    {
        if (string.IsNullOrEmpty(address.AddressDetail) || address.AddressDetail.Length > 100)
            listErro.Add(new ValidationError { Description = "Tamanho da morada detalhada.", Field = $"{path}/AddressDetail", FileID = CustomerID, Value = address.AddressDetail, UID = Pk });
        if (string.IsNullOrEmpty(address.BuildingNumber) == false && address.BuildingNumber.Length > 10)
            listErro.Add(new ValidationError { Description = "Tamanho do número de polícia incorrecto.", Field = $"{path}/BuildingNumber", FileID = CustomerID, Value = address.BuildingNumber, UID = Pk });
        if (string.IsNullOrEmpty(address.City) || address.City.Length > 50)
            listErro.Add(new ValidationError { Description = "Tamanho da localidade incorrecto.", Field = $"{path}/City", FileID = CustomerID, Value = address.City, UID = Pk });
        if (string.IsNullOrEmpty(address.PostalCode) || address.PostalCode.Length > 20)
            listErro.Add(new ValidationError { Description = "Tamanho do código postal incorrecto.", Field = $"{path}/PostalCode", FileID = CustomerID, Value = address.PostalCode, UID = Pk });
        if (string.IsNullOrEmpty(address.Region) == false && address.Region.Length > 50)
            listErro.Add(new ValidationError { Description = "Tamanho do distrito incorrecto.", Field = $"{path}/Region", FileID = CustomerID, Value = address.Region, UID = Pk });
        if (string.IsNullOrEmpty(address.StreetName) == false && address.StreetName.Length > 90)
            listErro.Add(new ValidationError { Description = "Tamanho do nome da rua incorrecto.", Field = $"{path}/StreetName", FileID = CustomerID, Value = address.StreetName, UID = Pk });
        if (string.IsNullOrEmpty(address.Country) || (address.Country.Length != 2 && address.Country != "Desconhecido"))
            listErro.Add(new ValidationError { Description = "Tamanho do País incorrecto.", Field = $"{path}/Country", FileID = CustomerID, Value = address.Country, UID = Pk });
    }
}

public partial class TaxTableEntry : BaseData
{
    public ValidationError ValidateTaxCountryRegion()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(TaxCountryRegion) || TaxCountryRegion.Length > 5)
        {
            erro = new ValidationError { Description = "País ou região do imposto inválido", Field = "TaxTableEntry/TaxCountryRegion", Value = TaxCountryRegion, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateTaxCode()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(TaxCode) || TaxCode.Length > 10)
        {
            erro = new ValidationError { Description = "Código do imposto inválido", Field = "TaxTableEntry/TaxCode", Value = TaxCode, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateDescription()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(Description) || Description.Length > 255)
        {
            erro = new ValidationError { Description = "Descrição inválida", Field = "TaxTableEntry/Description", Value = Description, UID = Pk };
        }

        return erro;
    }
    //public Error ValidateTaxExpirationDate()
    //{
    //    Error erro = null;
    //    if (string.IsNullOrEmpty(TaxExpirationDate) || TaxExpirationDate.Length > 30)
    //    {
    //        erro = new Error { Description = "Data de fim de vigência inválida", Field = "TaxExpirationDate", ID = TaxExpirationDate, UID = Pk };
    //        if (appendError)
    //    }

    //    return erro;
    //}
    //public Error ValidateTaxPercentage()
    //{
    //    Error erro = null;
    //    if (string.IsNullOrEmpty(TaxPercentage) || TaxPercentage.Length > 30)
    //    {
    //        erro = new Error { Description = "Percentagem da taxa do imposto inválida.", Field = "TaxPercentage", ID = TaxPercentage, UID = Pk };
    //        if (appendError)
    //    }

    //    return erro;
    //}
    //public Error ValidateTaxAmount()
    //{
    //    Error erro = null;
    //    if (string.IsNullOrEmpty(TaxAmount) || TaxAmount.Length > 30)
    //    {
    //        erro = new Error { Description = "Identificador único do cliente inválido", Field = "TaxAmount", ID = TaxAmount, UID = Pk };
    //        if (appendError)
    //    }

    //    return erro;
    //}
}

public partial class Supplier : BaseData
{
    public ValidationError ValidateCustomerID()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(SupplierID) || SupplierID.Length > 30)
        {
            erro = new ValidationError { Description = "Identificador único do fornecedor inválido", Field = "Supplier/SupplierID", FileID = SupplierID, Value = SupplierID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateAccountID()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(AccountID) || AccountID.Length > 30)
        {
            erro = new ValidationError { Description = "Código da conta inválido", Field = "Supplier/AccountID", FileID = SupplierID, Value = AccountID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSupplierTaxID()
    {
        ValidationError erro = null;
        if (BillingAddress?.Country == "PT" && !Validations.CheckTaxRegistrationNumber(SupplierTaxID))
        {
            erro = new ValidationError { Description = "Número de identificação fiscal inválido", Field = "Supplier/SupplierTaxID", FileID = SupplierID, Value = SupplierTaxID, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateCompanyName()
    {
        ValidationError erro = null;
        if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
        {
            erro = new ValidationError { Description = "Nome da empresa inválido", Field = "Supplier/CompanyName", FileID = SupplierID, Value = CompanyName, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateContact()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Contact) && Contact.Length > 50)
        {
            erro = new ValidationError { Description = "Nome do contacto na empresa inválido.", Field = "Supplier/Contact", FileID = SupplierID, Value = Contact, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateTelephone()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
        {
            erro = new ValidationError { Description = "Telefone inválido", Field = "Supplier/Telephone", FileID = SupplierID, Value = Telephone, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateFax()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
        {
            erro = new ValidationError { Description = "Fax inválido", Field = "Supplier/Fax", FileID = SupplierID, Value = Fax, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateEmail()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Email) && Email.Length > 60)
        {
            erro = new ValidationError { Description = "Email inválido", Field = "Supplier/Email", FileID = SupplierID, Value = Email, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateWebsite()
    {
        ValidationError erro = null;
        if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
        {
            erro = new ValidationError { Description = "Website inválido", Field = "Supplier/Website", FileID = SupplierID, Value = Website, UID = Pk };
        }

        return erro;
    }
    public ValidationError ValidateSelfBillingIndicator()
    {
        ValidationError erro = null;

        int selfBillingIndicator = -1;
        if (!string.IsNullOrEmpty(SelfBillingIndicator))
            int.TryParse(SelfBillingIndicator, out selfBillingIndicator);

        if (string.IsNullOrEmpty(SelfBillingIndicator) || selfBillingIndicator == -1)
        {
            erro = new ValidationError { Description = "Nº de conta inválido", Field = "Supplier/SelfBillingIndicator", FileID = SupplierID, Value = SelfBillingIndicator, UID = Pk };
        }

        return erro;
    }

    public ValidationError[] ValidateBillingAddress()
    {
        var listErro = new List<ValidationError>();

        if (BillingAddress == null)
            listErro.Add(new ValidationError { Description = "Morada de faturação inexistente", Field = "Supplier/BillingAddress", FileID = SupplierID, UID = Pk });

        if (BillingAddress != null)
        {
            ValidateAddress(BillingAddress, "Supplier/BillingAddress", listErro);
        }

        return listErro.ToArray();
    }
    public ValidationError[] ValidateShipFromAddress()
    {
        var listErro = new List<ValidationError>();

        if (ShipFromAddress != null && ShipFromAddress.Length > 0)
        {
            foreach (var morada in ShipFromAddress)
            {
                ValidateAddress(morada, "Supplier/ShipFromAddress", listErro);
            }
        }

        return listErro.ToArray();
    }

    public void ValidateAddress(SupplierAddressStructure address, string path, List<ValidationError> listErro)
    {
        if (string.IsNullOrEmpty(address.AddressDetail) || address.AddressDetail.Length > 100)
            listErro.Add(new ValidationError { Description = "Tamanho da morada detalhada.", Field = $"{path}/AddressDetail", FileID = SupplierID, Value = address.AddressDetail, UID = Pk });
        if (string.IsNullOrEmpty(address.BuildingNumber) == false && address.BuildingNumber.Length > 10)
            listErro.Add(new ValidationError { Description = "Tamanho do número de polícia incorrecto.", Field = $"{path}/BuildingNumber", FileID = SupplierID, Value = address.BuildingNumber, UID = Pk });
        if (string.IsNullOrEmpty(address.City) || address.City.Length > 50)
            listErro.Add(new ValidationError { Description = "Tamanho da localidade incorrecto.", Field = $"{path}/City", FileID = SupplierID, Value = address.City, UID = Pk });
        if (string.IsNullOrEmpty(address.Country) || address.Country.Length != 2)
            listErro.Add(new ValidationError { Description = "Tamanho do País incorrecto.", Field = $"{path}/Country", FileID = SupplierID, Value = address.Country, UID = Pk });
        if (string.IsNullOrEmpty(address.PostalCode) || address.PostalCode.Length > 20)
            listErro.Add(new ValidationError { Description = "Tamanho do código postal incorrecto.", Field = $"{path}/PostalCode", FileID = SupplierID, Value = address.PostalCode, UID = Pk });
        if (string.IsNullOrEmpty(address.Region) == false && address.Region.Length > 50)
            listErro.Add(new ValidationError { Description = "Tamanho do distrito incorrecto.", Field = $"{path}/Region", FileID = SupplierID, Value = address.Region, UID = Pk });
        if (string.IsNullOrEmpty(address.StreetName) == false && address.StreetName.Length > 90)
            listErro.Add(new ValidationError { Description = "Tamanho do nome da rua incorrecto.", Field = $"{path}/StreetName", FileID = SupplierID, Value = address.StreetName, UID = Pk });

    }
}

public partial class GeneralLedger : BaseData
{

}

public partial class GeneralLedgerEntriesJournal : BaseData
{

}

public partial class GeneralLedgerEntriesJournalTransaction : BaseData
{
}

public partial class GeneralLedgerEntriesJournalTransactionLine : BaseData
{

}
