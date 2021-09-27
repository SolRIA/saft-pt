using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Solria.SAFT.Parser.Models
{
    public partial class SourceDocumentsPaymentsPayment : BaseData
    {
        public ValidationError ValidatePaymentRefNo()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(PaymentRefNo) || PaymentRefNo.Length > 60)
            {
                erro = new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "PaymentRefNo", TypeofError = GetType(), Value = PaymentRefNo, UID = Pk };
                //if (appendError)
                //    Tooltip.PaymentRefNo = Tooltip.PaymentRefNo.FormatTooltipWithError(erro.Description);
            }
            else if (!Regex.IsMatch(PaymentRefNo, "[^ ]+ [^/^ ]+/[0-9]+"))
            {
                erro = new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "PaymentRefNo", TypeofError = GetType(), Value = PaymentRefNo, UID = Pk };
                //if (appendError)
                //    Tooltip.PaymentRefNo = Tooltip.PaymentRefNo.FormatTooltipWithError(erro.Description);
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
                    erro = new ValidationError { Description = string.Format("Mês do período de tributação do documento {0} incorrecto.", PaymentRefNo), Field = "Period", TypeofError = GetType(), Value = Period, UID = Pk };
                    //if (appendError)
                    //    Tooltip.Period = Tooltip.Period.FormatTooltipWithError(erro.Description);
                }
            }

            return erro;
        }
        public ValidationError ValidateTransactionID()
        {
            ValidationError erro = null;

            if (!string.IsNullOrEmpty(TransactionID) && TransactionID.Length > 70)
            {
                erro = new ValidationError { Description = string.Format("Identificador da transacção do documento {0} incorrecto.", PaymentRefNo), Field = "TransactionID", TypeofError = GetType(), Value = TransactionID, UID = Pk };
                //if (appendError)
                //    Tooltip.TransactionID = Tooltip.TransactionID.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateTransactionDate()
        {
            ValidationError erro = null;

            if (TransactionDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data de emissão do documento {0} incorrecta.", PaymentRefNo), Field = "TransactionDate", TypeofError = GetType(), Value = TransactionDate.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.TransactionDate = Tooltip.TransactionDate.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateDescription()
        {
            ValidationError erro = null;

            if (!string.IsNullOrEmpty(Description) && Description.Length > 200)
            {
                erro = new ValidationError { Description = string.Format("Tamanho da descrição do recibo {0} incorrecto.", PaymentRefNo), Field = "Description", TypeofError = GetType(), Value = Description, UID = Pk };
                //if (appendError)
                //    Tooltip.Description = Tooltip.Description.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidatePaymentStatusDate()
        {
            ValidationError erro = null;

            if (DocumentStatus.PaymentStatusDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data e hora do estado atual do recibo {0} incorrecta.", PaymentRefNo), Field = "PaymentStatusDate", TypeofError = GetType(), Value = DocumentStatus.PaymentStatusDate.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.PaymentStatusDate = Tooltip.PaymentStatusDate.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateReason()
        {
            ValidationError erro = null;

            if (!string.IsNullOrEmpty(DocumentStatus.Reason) && DocumentStatus.Reason.Length > 50)
            {
                erro = new ValidationError { Description = string.Format("Tamanho do motivo da alteração de estado do recibo {0} incorrecto.", PaymentRefNo), Field = "Reason", TypeofError = GetType(), Value = DocumentStatus.Reason, UID = Pk };
                //if (appendError)
                //    Tooltip.Reason = Tooltip.Reason.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateDocumentStatusSourceID()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
            {
                erro = new ValidationError { Description = string.Format("Utilizador responsável pelo estado atual do recibo {0} incorrecto.", DocumentStatus.SourceID), Field = "SourceID", TypeofError = GetType(), Value = DocumentStatus.SourceID, UID = Pk };
                //if (appendError)
                //    Tooltip.ResponsableUserSourceID = Tooltip.ResponsableUserSourceID.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateSystemID()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(SystemID) == false && SystemID.Length > 35)
            {
                erro = new ValidationError { Description = string.Format("Tamanho do número único do recibo {0} incorrecto.", PaymentRefNo), Field = "SystemID", TypeofError = GetType(), Value = SystemID, UID = Pk };
                //if (appendError)
                //    Tooltip.SystemID = Tooltip.SystemID.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateSourceID()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(SourceID) || SourceID.Length > 30)
            {
                erro = new ValidationError { Description = string.Format("Utilizador que gerou o documento {0} incorrecto.", SourceID), Field = "SourceID", TypeofError = GetType(), Value = SourceID, UID = Pk };
                //if (appendError)
                //    Tooltip.GeneratedDocumentUserSourceID = Tooltip.GeneratedDocumentUserSourceID.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateSystemEntryDate()
        {
            ValidationError erro = null;

            if (SystemEntryDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data da gravação do documento {0} incorrecta.", PaymentRefNo), Field = "SystemEntryDate", TypeofError = GetType(), Value = SystemEntryDate.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.SystemEntryDate = Tooltip.SystemEntryDate.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateCustomerID()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
            {
                erro = new ValidationError { Description = string.Format("Chave única da tabela de clientes no documento {0} incorrecta.", PaymentRefNo), Field = "CustomerID", TypeofError = GetType(), Value = CustomerID, UID = Pk };
                //if (appendError)
                //    Tooltip.CustomerID = Tooltip.CustomerID.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }

        public ValidationError[] ValidatePaymentMethod()
        {
            List<ValidationError> listErro = new List<ValidationError>();

            if (PaymentMethod != null && PaymentMethod.Length > 0)
            {
                foreach (var pay in PaymentMethod)
                {
                    if (pay.PaymentAmount < 0)
                    {
                        ValidationError erro = new ValidationError { Description = string.Format("Utilizador que gerou o documento {0} incorrecto.", PaymentRefNo), Field = "PaymentAmount", TypeofError = GetType(), Value = pay.PaymentAmount.ToString(), UID = Pk };
                        listErro.Add(erro);
                        //if (appendError)
                        //    Tooltip.PaymentMechanism = Tooltip.PaymentMechanism.FormatTooltipWithError(erro.Description);
                    }

                    if (pay.PaymentDate > DateTime.Now)
                    {
                        ValidationError erro = new ValidationError { Description = string.Format("Data do pagamento do documento {0} incorrecta.", PaymentRefNo), Field = "PaymentDate", TypeofError = GetType(), Value = pay.PaymentDate.ToString(), UID = Pk };
                        listErro.Add(erro);
                        //if (appendError)
                        //    Tooltip.PaymentMechanism = Tooltip.PaymentMechanism.FormatTooltipWithError(erro.Description);
                    }
                }
            }
            return listErro.ToArray();
        }
    }

    public partial class SourceDocumentsPaymentsPaymentLine : BaseData
    {
        /// <summary>
        /// Link to the Payment
        /// </summary>
        public string DocNo { get; set; }

        public ValidationError ValidateLineNumber(string SupPk = "", string paymentNo = "")
        {
            ValidationError erro = null;
            int num = -1;
            if (!string.IsNullOrEmpty(LineNumber))
                Int32.TryParse(LineNumber, out num);

            if (string.IsNullOrEmpty(LineNumber) || num == -1)
            {
                if (!string.IsNullOrEmpty(paymentNo))
                    paymentNo = string.Format(", documento {0}", paymentNo);

                erro = new ValidationError { Description = string.Format("Número de linha incorrecto{0}.", paymentNo), Field = "LineNumber", TypeofError = GetType(), Value = LineNumber, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.LineNumber = Tooltip.LineNumber.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateSettlementAmount(string SupPk = "", string paymentNo = "")
        {
            ValidationError erro = null;
            if (SettlementAmount < 0)
            {
                if (!string.IsNullOrEmpty(paymentNo))
                    paymentNo = string.Format(" documento {0}", paymentNo);

                erro = new ValidationError { Description = string.Format("Montante do desconto, {0} linha {1}.", paymentNo, LineNumber), Field = "SettlementAmount", TypeofError = GetType(), Value = SettlementAmount.ToString(), UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.LineSettlementAmount = Tooltip.LineSettlementAmount.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateItem(string SupPk = "", string paymentNo = "")
        {
            ValidationError erro = null;
            if ((CreditAmount ?? DebitAmount ?? 0) < 0)
            {
                if (!string.IsNullOrEmpty(paymentNo))
                    paymentNo = string.Format(" documento {0}", paymentNo);

                if (ItemElementName == ItemChoiceType8.CreditAmount)
                    erro = new ValidationError { Description = string.Format("Valor a crédito incorrecta, {0} linha {1}.", paymentNo, LineNumber), Field = "CreditAmount", TypeofError = GetType(), Value = CreditAmount.ToString(), UID = Pk, SupUID = SupPk };
                if (ItemElementName == ItemChoiceType8.DebitAmount)
                    erro = new ValidationError { Description = string.Format("Valor a débito incorrecto, {0} linha {1}.", paymentNo, LineNumber), Field = "DebitAmount", TypeofError = GetType(), Value = DebitAmount.ToString(), UID = Pk, SupUID = SupPk };

                //if (appendError && ItemElementName == ItemChoiceType9.CreditAmount)
                //    Tooltip.CreditAmount = Tooltip.CreditAmount.FormatTooltipWithError(erro.Description);
                //if (appendError && ItemElementName == ItemChoiceType9.DebitAmount)
                //    Tooltip.DebitAmount = Tooltip.DebitAmount.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateTaxExemptionReason(string SupPk = "", string paymentNo = "")
        {
            ValidationError erro = null;
            if ((Tax != null && Tax.TaxPercentage == 0 && string.IsNullOrEmpty(TaxExemptionReason)) || (string.IsNullOrEmpty(TaxExemptionReason) == false && TaxExemptionReason.Length > 60))
            {
                if (!string.IsNullOrEmpty(paymentNo))
                    paymentNo = string.Format(" documento {0}", paymentNo);

                erro = new ValidationError { Description = string.Format("Motivo da isenção de imposto incorrecto, {0} linha {1}.", paymentNo, LineNumber), Field = "TaxExemptionReason", TypeofError = GetType(), Value = TaxExemptionReason, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.TaxExemptionReason = Tooltip.TaxExemptionReason.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }

        public ValidationError[] ValidateSourceDocumentID(string SupPk = "", string paymentNo = "")
        {
            List<ValidationError> listErro = new List<ValidationError>();

            if (!string.IsNullOrEmpty(paymentNo))
                paymentNo = string.Format(" documento {0}", paymentNo);

            if (SourceDocumentID == null || SourceDocumentID.Length == 0)
                listErro.Add(new ValidationError { Description = string.Format("Referência ao documento de origem inexistente,{0} linha {1}.", paymentNo, LineNumber), Field = "SourceDocumentID", TypeofError = GetType(), UID = Pk, SupUID = SupPk });

            if (SourceDocumentID != null && SourceDocumentID.Length > 0)
            {
                foreach (var doc in SourceDocumentID)
                {
                    if (string.IsNullOrEmpty(doc.Description) == false && doc.Description.Length > 100)
                        listErro.Add(new ValidationError { Description = string.Format("Tamanho da descrição da linha incorrecto,{0} linha {1}.", paymentNo, LineNumber), Field = "Description", TypeofError = GetType(), Value = doc.Description, UID = Pk, SupUID = SupPk });
                    if (doc.InvoiceDate > DateTime.Now)
                        listErro.Add(new ValidationError { Description = string.Format("Data do documento de origem incorrecta,{0} linha {1}.", paymentNo, LineNumber), Field = "InvoiceDate", TypeofError = GetType(), Value = doc.InvoiceDate.ToString(), UID = Pk, SupUID = SupPk });
                    if (string.IsNullOrEmpty(doc.OriginatingON) || doc.OriginatingON.Length > 60)
                        listErro.Add(new ValidationError { Description = string.Format("Número do documento de origem incorrecto,{0} linha {1}.", paymentNo, LineNumber), Field = "OriginatingON", TypeofError = GetType(), Value = doc.OriginatingON, UID = Pk, SupUID = SupPk });
                }
            }
            return listErro.ToArray();
        }
        public ValidationError[] ValidateTax(string SupPk = "", string paymentNo = "")
        {
            List<ValidationError> listErro = new List<ValidationError>();
            if (Tax != null)
            {
                if (!string.IsNullOrEmpty(paymentNo))
                    paymentNo = string.Format(" documento {0}", paymentNo);

                if (Tax.TaxAmount < 0 && Tax.ItemElementName == ItemChoiceType.TaxAmount)
                    listErro.Add(new ValidationError { Description = string.Format("Montante do imposto incorrecto, {0} linha {1}.", paymentNo, LineNumber), Field = "TaxAmount", TypeofError = GetType(), Value = Tax.TaxAmount.ToString(), UID = Pk, SupUID = SupPk });
                if ((Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100) && Tax.ItemElementName == ItemChoiceType.TaxPercentage)
                    listErro.Add(new ValidationError { Description = string.Format("Montante do imposto incorrecto, {0} linha {1}.", paymentNo, LineNumber), Field = "TaxPercentage", TypeofError = GetType(), Value = Tax.TaxPercentage.ToString(), UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                    listErro.Add(new ValidationError { Description = string.Format("Código da taxa incorrecto, {0} linha {1}.", paymentNo, LineNumber), Field = "TaxCode", TypeofError = GetType(), Value = Tax.TaxCode, UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCode.Length > 5)
                    listErro.Add(new ValidationError { Description = string.Format("País ou região do imposto incorrecto, {0} linha {1}.", paymentNo, LineNumber), Field = "TaxCountryRegion", TypeofError = GetType(), Value = Tax.TaxCountryRegion, UID = Pk, SupUID = SupPk });
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
                erro = new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "DocumentNumber", TypeofError = GetType(), Value = DocumentNumber, UID = Pk };
                //if (appendError)
                //    Tooltip.DocumentNumber = Tooltip.DocumentNumber.FormatTooltipWithError(erro.Description);
            }
            else if (!Regex.IsMatch(DocumentNumber, "([a-zA-Z0-9./_-])+ ([a-zA-Z0-9]*/[0-9]+)"))
            {
                erro = new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "DocumentNumber", TypeofError = GetType(), Value = DocumentNumber, UID = Pk };
                //if (appendError)
                //    Tooltip.DocumentNumber = Tooltip.DocumentNumber.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateHash()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(Hash) || Hash.Length != 172)
            {
                erro = new ValidationError { Description = string.Format("Assinatura do documento {0} de tamanho incorrecto.", DocumentNumber), Field = "Hash", TypeofError = GetType(), Value = Hash, UID = Pk };
                //if (appendError)
                //    Tooltip.Hash = Tooltip.Hash.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateHashControl()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 40)
            {
                erro = new ValidationError { Description = string.Format("Versão da chave privada utilizada na assinatura do documento {0} incorrecta.", DocumentNumber), Field = "HashControl", TypeofError = GetType(), Value = HashControl, UID = Pk };
                //if (appendError)
                //    Tooltip.HashControl = Tooltip.HashControl.FormatTooltipWithError(erro.Description);
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
                    erro = new ValidationError { Description = string.Format("Mês do período de tributação do documento {0} incorrecto.", DocumentNumber), Field = "Period", TypeofError = GetType(), Value = Period, UID = Pk };
                    //if (appendError)
                    //    Tooltip.Period = Tooltip.Period.FormatTooltipWithError(erro.Description);
                }
            }

            return erro;
        }
        public ValidationError ValidateMovementDate()
        {
            ValidationError erro = null;

            if (MovementDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data de emissão do documento {0} incorrecta.", DocumentNumber), Field = "InvoiceDate", TypeofError = GetType(), Value = MovementDate.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.MovementDate = Tooltip.MovementDate.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateSystemEntryDate()
        {
            ValidationError erro = null;

            if (SystemEntryDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data da gravação do documento {0} incorrecta.", DocumentNumber), Field = "SystemEntryDate", TypeofError = GetType(), Value = SystemEntryDate.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.SystemEntryDate = Tooltip.SystemEntryDate.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateTransactionID()
        {
            ValidationError erro = null;

            if (!string.IsNullOrEmpty(TransactionID) && TransactionID.Length > 70)
            {
                erro = new ValidationError { Description = string.Format("Identificador da transacção do documento {0} incorrecto.", DocumentNumber), Field = "TransactionID", TypeofError = GetType(), Value = TransactionID, UID = Pk };
                //if (appendError)
                //    Tooltip.TransactionID = Tooltip.TransactionID.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateMovementEndTime()
        {
            ValidationError erro = null;

            if (MovementEndTime == DateTime.MinValue || MovementEndTime > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data e hora de fim de transporte do documento {0} incorrecta.", DocumentNumber), Field = "MovementEndTime", TypeofError = GetType(), Value = MovementEndTime.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.MovementEndTime = Tooltip.MovementEndTime.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateMovementStartTime()
        {
            ValidationError erro = null;

            if (MovementStartTime > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data e hora de início de transporte do documento {0} incorrecta.", DocumentNumber), Field = "MovementStartTime", TypeofError = GetType(), Value = MovementStartTime.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.MovementStartTime = Tooltip.MovementStartTime.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }

        public ValidationError[] ValidateDocumentStatus()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (DocumentStatus == null)
                listError.Add(new ValidationError { Description = string.Format("Situação do documento {0} inexistente.", DocumentNumber), Field = "DocumentStatus", TypeofError = GetType(), UID = Pk });

            if (DocumentStatus != null)
            {
                if (DocumentStatus.MovementStatusDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data e hora do estado atual do documento {0} incorrecta.", DocumentNumber), Field = "InvoiceStatusDate", TypeofError = GetType(), Value = DocumentStatus.MovementStatusDate.ToString(), UID = Pk });
                if (string.IsNullOrEmpty(DocumentStatus.Reason) == false && DocumentStatus.Reason.Length > 50)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do motivo da alteração de estado {0} incorrecto.", DocumentNumber), Field = "Reason", TypeofError = GetType(), Value = DocumentStatus.Reason, UID = Pk });
                if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
                    listError.Add(new ValidationError { Description = string.Format("Utilizador responsável pelo estado atual do documento {0} incorrecto.", DocumentNumber), Field = "SourceID", TypeofError = GetType(), Value = DocumentStatus.SourceID, UID = Pk });
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateShipTo()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (ShipTo != null)
            {
                if (ShipTo.Address != null)
                {
                    if (string.IsNullOrEmpty(ShipTo.Address.AddressDetail) || ShipTo.Address.AddressDetail.Length > 100)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da morada detalhada do documento {0} do local de descarga incorrecto.", DocumentNumber), Field = "AddressDetail", TypeofError = GetType(), Value = ShipTo.Address.AddressDetail, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.BuildingNumber) == false && ShipTo.Address.BuildingNumber.Length > 10)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do número de polícia do documento {0} do local de descarga incorrecto.", DocumentNumber), Field = "BuildingNumber", TypeofError = GetType(), Value = ShipTo.Address.BuildingNumber, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.City) || ShipTo.Address.City.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da localidade do documento {0} do local de descarga incorrecto.", DocumentNumber), Field = "City", TypeofError = GetType(), Value = ShipTo.Address.City, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.Country) || ShipTo.Address.Country.Length != 2)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do País do documento {0} do local de descarga incorrecto.", DocumentNumber), Field = "Country", TypeofError = GetType(), Value = ShipTo.Address.Country, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.PostalCode) || ShipTo.Address.PostalCode.Length > 20)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do código postal do documento {0} do local de descarga incorrecto.", DocumentNumber), Field = "PostalCode", TypeofError = GetType(), Value = ShipTo.Address.PostalCode, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.Region) == false && ShipTo.Address.Region.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do distrito do documento {0} do local de descarga incorrecto.", DocumentNumber), Field = "Region", TypeofError = GetType(), Value = ShipTo.Address.Region, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.StreetName) == false && ShipTo.Address.StreetName.Length > 90)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do nome da rua do documento {0} do local de descarga incorrecto.", DocumentNumber), Field = "StreetName", TypeofError = GetType(), Value = ShipTo.Address.StreetName, UID = Pk });
                }
                if (ShipTo.DeliveryDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data da entrega do documento {0} incorrecto.", DocumentNumber), Field = "DeliveryDate", TypeofError = GetType(), Value = ShipTo.DeliveryDate.ToString(), UID = Pk });
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateShipFrom()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (ShipFrom != null)
            {
                if (ShipFrom.Address != null)
                {
                    if (string.IsNullOrEmpty(ShipFrom.Address.AddressDetail) || ShipFrom.Address.AddressDetail.Length > 100)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da morada detalhada do documento {0} do local de carga incorrecto.", DocumentNumber), Field = "AddressDetail", TypeofError = GetType(), Value = ShipFrom.Address.AddressDetail, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.BuildingNumber) == false && ShipFrom.Address.BuildingNumber.Length > 10)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do número de polícia do documento {0} do local de carga incorrecto.", DocumentNumber), Field = "BuildingNumber", TypeofError = GetType(), Value = ShipFrom.Address.BuildingNumber, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.City) || ShipFrom.Address.City.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da localidade do documento {0} do local de carga incorrecto.", DocumentNumber), Field = "City", TypeofError = GetType(), Value = ShipFrom.Address.City, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.Country) || ShipFrom.Address.Country.Length != 2)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do País do documento {0} do local de carga incorrecto.", DocumentNumber), Field = "Country", TypeofError = GetType(), Value = ShipFrom.Address.Country, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.PostalCode) || ShipFrom.Address.PostalCode.Length > 20)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do código postal do documento {0} do local de carga incorrecto.", DocumentNumber), Field = "PostalCode", TypeofError = GetType(), Value = ShipFrom.Address.PostalCode, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.Region) == false && ShipFrom.Address.Region.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do distrito do documento {0} do local de carga incorrecto.", DocumentNumber), Field = "Region", TypeofError = GetType(), Value = ShipFrom.Address.Region, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.StreetName) == false && ShipFrom.Address.StreetName.Length > 90)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do nome da rua do documento {0} do local de carga incorrecto.", DocumentNumber), Field = "StreetName", TypeofError = GetType(), Value = ShipFrom.Address.StreetName, UID = Pk });
                }
                if (ShipFrom.DeliveryDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data da receção do documento {0} incorrecto.", DocumentNumber), Field = "DeliveryDate", TypeofError = GetType(), Value = ShipFrom.DeliveryDate.ToString(), UID = Pk });
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateDocumentTotals()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (DocumentTotals == null)
                listError.Add(new ValidationError { Description = string.Format("Totais do documento {0} inexistentes.", DocumentNumber), Field = "DocumentTotals", TypeofError = GetType(), UID = Pk });

            if (DocumentTotals != null)
            {
                if (DocumentTotals.Currency != null)
                {
                    if (DocumentTotals.Currency.CurrencyAmount < 0)
                        listError.Add(new ValidationError { Description = string.Format("Valor total em moeda estrangeira do documento {0} incorrecto.", DocumentNumber), Field = "CurrencyAmount", TypeofError = GetType(), Value = DocumentTotals.Currency.CurrencyAmount.ToString(), UID = Pk });
                    if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length > 3)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do código de moeda do documento {0} incorrecto.", DocumentNumber), Field = "CurrencyCode", TypeofError = GetType(), Value = DocumentTotals.Currency.CurrencyCode, UID = Pk });
                    if (DocumentTotals.Currency.ExchangeRate < 0)
                        listError.Add(new ValidationError { Description = string.Format("Taxa de câmbio do documento {0} incorrecta.", DocumentNumber), Field = "ExchangeRate", TypeofError = GetType(), Value = DocumentTotals.Currency.ExchangeRate.ToString(), UID = Pk });
                }

                if (DocumentTotals.GrossTotal < 0)
                    listError.Add(new ValidationError { Description = string.Format("Total do documento {0} incorrecto.", DocumentNumber), Field = "GrossTotal", TypeofError = GetType(), Value = DocumentTotals.GrossTotal.ToString(), UID = Pk });
                if (DocumentTotals.NetTotal < 0)
                    listError.Add(new ValidationError { Description = string.Format("Total do documento {0} sem impostos incorrecto.", DocumentNumber), Field = "NetTotal", TypeofError = GetType(), Value = DocumentTotals.NetTotal.ToString(), UID = Pk });
                if (DocumentTotals.TaxPayable < 0)
                    listError.Add(new ValidationError { Description = string.Format("Valor do imposto a pagar do documento {0} incorrecto.", DocumentNumber), Field = "TaxPayable", TypeofError = GetType(), Value = DocumentTotals.TaxPayable.ToString(), UID = Pk });
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

        public ValidationError ValidateLineNumber(string SupPk = "", string movement = "")
        {
            ValidationError erro = null;
            int num = -1;
            if (!string.IsNullOrEmpty(LineNumber))
                Int32.TryParse(LineNumber, out num);

            if (string.IsNullOrEmpty(LineNumber) || num == -1)
            {
                if (!string.IsNullOrEmpty(movement))
                    movement = string.Format(", documento {0}", movement);

                erro = new ValidationError { Description = string.Format("Número de linha incorrecto {0}.", movement), Field = "LineNumber", TypeofError = GetType(), Value = LineNumber, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.LineNumber = Tooltip.LineNumber.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateProductCode(string SupPk = "", string movement = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 30)
            {
                if (!string.IsNullOrEmpty(movement))
                    movement = string.Format(" documento {0}", movement);

                erro = new ValidationError { Description = string.Format("Identificador do produto ou serviço incorrecto, {0} linha {1}.", movement, LineNumber), Field = "ProductCode", TypeofError = GetType(), Value = ProductCode, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.ProductCode = Tooltip.ProductCode.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateProductDescription(string SupPk = "", string movement = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length > 200)
            {
                if (!string.IsNullOrEmpty(movement))
                    movement = string.Format(" documento {0}", movement);

                erro = new ValidationError { Description = string.Format("Descrição do produto ou serviço incorrecta, {0} linha {1}.", movement, LineNumber), Field = "ProductDescription", TypeofError = GetType(), Value = ProductDescription, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.ProductDescription = Tooltip.ProductDescription.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateQuantity(string SupPk = "", string movement = "")
        {
            ValidationError erro = null;
            if (Quantity <= 0)
            {
                if (!string.IsNullOrEmpty(movement))
                    movement = string.Format(" documento {0}", movement);

                erro = new ValidationError { Description = string.Format("Quantidade incorrecta, {0} linha {1}.", movement, LineNumber), Field = "Quantity", TypeofError = GetType(), Value = Quantity.ToString(), UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.Quantity = Tooltip.Quantity.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateUnitOfMeasure(string SupPk = "", string movement = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
            {
                if (!string.IsNullOrEmpty(movement))
                    movement = string.Format(" documento {0}", movement);

                erro = new ValidationError { Description = string.Format("Unidade de medida incorrecta, {0} linha {1}.", movement, LineNumber), Field = "UnitOfMeasure", TypeofError = GetType(), Value = UnitOfMeasure, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.UnitOfMeasure = Tooltip.UnitOfMeasure.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateUnitPrice(string SupPk = "", string movement = "")
        {
            ValidationError erro = null;
            if (UnitPrice == 0)
            {
                if (!string.IsNullOrEmpty(movement))
                    movement = string.Format(" documento {0}", movement);

                erro = new ValidationError { Description = string.Format("Preço unitário incorrecto, {0} linha {1}.", movement, LineNumber), Field = "UnitPrice", TypeofError = GetType(), Value = UnitPrice.ToString(), UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.UnitPrice = Tooltip.UnitPrice.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }

        public ValidationError[] ValidateOrderReferences(string SupPk = "", string movement = "")
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (OrderReferences != null && OrderReferences.Length > 0)
            {
                foreach (var referencia in OrderReferences)
                {
                    if (string.IsNullOrEmpty(referencia.OriginatingON) == false && referencia.OriginatingON.Length > 60)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do número do documento de origem na linha {0} do documento {1} incorrecto.", LineNumber, movement), Field = "OriginatingON", TypeofError = GetType(), Value = referencia.OriginatingON, UID = Pk, SupUID = SupPk });
                    if (referencia.OrderDate == DateTime.MinValue || referencia.OrderDate > DateTime.Now)
                        listError.Add(new ValidationError { Description = string.Format("Data do documento de origem na linha {0} do documento {1} incorrecto.", LineNumber, movement), Field = "OrderDate", TypeofError = GetType(), Value = referencia.OrderDate.ToString(), UID = Pk, SupUID = SupPk });
                }
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateTax(string SupPk = "", string movement = "")
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (Tax == null)
                listError.Add(new ValidationError { Description = string.Format("Taxa de imposto na linha {0} do documento {1} inexistente.", LineNumber, movement), Field = "Tax", TypeofError = GetType(), UID = Pk, SupUID = SupPk });

            if (Tax != null)
            {
                if (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100)
                    listError.Add(new ValidationError { Description = string.Format("Percentagem da taxa do imposto na linha {0} do documento {1} incorrecto.", LineNumber, movement), Field = "TaxPercentage", TypeofError = GetType(), Value = Tax.TaxPercentage.ToString(), UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                    listError.Add(new ValidationError { Description = string.Format("Código da taxa na linha {0} do documento {1} incorrecto.", LineNumber, movement), Field = "TaxCode", TypeofError = GetType(), Value = Tax.TaxCode, UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                    listError.Add(new ValidationError { Description = string.Format("País ou região do imposto na linha {0} do documento {1} incorrecto.", LineNumber, movement), Field = "TaxCountryRegion", TypeofError = GetType(), Value = Tax.TaxCountryRegion, UID = Pk, SupUID = SupPk });
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
                erro = new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "DocumentNumber", TypeofError = GetType(), Value = DocumentNumber, UID = Pk };
                //if (appendError)
                //    Tooltip.DocumentNumber = Tooltip.DocumentNumber.FormatTooltipWithError(erro.Description);
            }
            else if (!Regex.IsMatch(DocumentNumber, "([a-zA-Z0-9./_-])+ ([a-zA-Z0-9]*/[0-9]+)"))
            {
                erro = new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "DocumentNumber", TypeofError = GetType(), Value = DocumentNumber, UID = Pk };
                //if (appendError)
                //    Tooltip.DocumentNumber = Tooltip.DocumentNumber.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateHash()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(Hash) || Hash.Length != 172)
            {
                erro = new ValidationError { Description = string.Format("Assinatura do documento {0} de tamanho incorrecto.", DocumentNumber), Field = "Hash", TypeofError = GetType(), Value = Hash, UID = Pk };
                //if (appendError)
                //    Tooltip.Hash = Tooltip.Hash.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateHashControl()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 40)
            {
                erro = new ValidationError { Description = string.Format("Versão da chave privada utilizada na assinatura do documento {0} incorrecta.", DocumentNumber), Field = "HashControl", TypeofError = GetType(), Value = HashControl, UID = Pk };
                //if (appendError)
                //    Tooltip.HashControl = Tooltip.HashControl.FormatTooltipWithError(erro.Description);
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
                    erro = new ValidationError { Description = string.Format("Mês do período de tributação do documento {0} incorrecto.", DocumentNumber), Field = "Period", TypeofError = GetType(), Value = Period, UID = Pk };
                    //if (appendError)
                    //    Tooltip.Period = Tooltip.Period.FormatTooltipWithError(erro.Description);
                }
            }

            return erro;
        }
        public ValidationError ValidateWorkDate()
        {
            ValidationError erro = null;

            if (WorkDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data de emissão do documento {0} incorrecta.", DocumentNumber), Field = "WorkDate", TypeofError = GetType(), Value = WorkDate.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.WorkDate = Tooltip.WorkDate.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }
        public ValidationError ValidateSystemEntryDate()
        {
            ValidationError erro = null;

            if (SystemEntryDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data da gravação do documento {0} incorrecta.", DocumentNumber), Field = "SystemEntryDate", TypeofError = GetType(), Value = SystemEntryDate.ToString(), UID = Pk };
                //if (appendError)
                //    Tooltip.SystemEntryDate = Tooltip.SystemEntryDate.FormatTooltipWithError(erro.Description);
            }

            return erro;
        }

        public ValidationError[] ValidateDocumentStatus()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (DocumentStatus == null)
                listError.Add(new ValidationError { Description = string.Format("Situação do documento {0} inexistente.", DocumentNumber), Field = "DocumentStatus", TypeofError = GetType(), UID = Pk });

            if (DocumentStatus != null)
            {
                if (DocumentStatus.WorkStatusDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data e hora do estado atual do documento {0} incorrecta.", DocumentNumber), Field = "InvoiceStatusDate", TypeofError = GetType(), Value = DocumentStatus.WorkStatusDate.ToString(), UID = Pk });
                if (string.IsNullOrEmpty(DocumentStatus.Reason) == false && DocumentStatus.Reason.Length > 50)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do motivo da alteração de estado {0} incorrecto.", DocumentNumber), Field = "Reason", TypeofError = GetType(), Value = DocumentStatus.Reason, UID = Pk });
                if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
                    listError.Add(new ValidationError { Description = string.Format("Utilizador responsável pelo estado atual do documento {0} incorrecto.", DocumentNumber), Field = "SourceID", TypeofError = GetType(), Value = DocumentStatus.SourceID, UID = Pk });
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateDocumentTotals()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (DocumentTotals == null)
                listError.Add(new ValidationError { Description = string.Format("Totais do documento {0} inexistentes.", DocumentNumber), Field = "DocumentTotals", TypeofError = GetType(), UID = Pk });

            if (DocumentTotals != null)
            {
                if (DocumentTotals.Currency != null)
                {
                    if (DocumentTotals.Currency.CurrencyAmount < 0)
                        listError.Add(new ValidationError { Description = string.Format("Valor total em moeda estrangeira do documento {0} incorrecto.", DocumentNumber), Field = "CurrencyAmount", TypeofError = GetType(), Value = DocumentTotals.Currency.CurrencyAmount.ToString(), UID = Pk });
                    if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length > 3)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do código de moeda do documento {0} incorrecto.", DocumentNumber), Field = "CurrencyCode", TypeofError = GetType(), Value = DocumentTotals.Currency.CurrencyCode, UID = Pk });
                    if (DocumentTotals.Currency.ExchangeRate < 0)
                        listError.Add(new ValidationError { Description = string.Format("Taxa de câmbio do documento {0} incorrecta.", DocumentNumber), Field = "ExchangeRate", TypeofError = GetType(), Value = DocumentTotals.Currency.ExchangeRate.ToString(), UID = Pk });
                }

                if (DocumentTotals.GrossTotal < 0)
                    listError.Add(new ValidationError { Description = string.Format("Total do documento {0} incorrecto.", DocumentNumber), Field = "GrossTotal", TypeofError = GetType(), Value = DocumentTotals.GrossTotal.ToString(), UID = Pk });
                if (DocumentTotals.NetTotal < 0)
                    listError.Add(new ValidationError { Description = string.Format("Total do documento {0} sem impostos incorrecto.", DocumentNumber), Field = "NetTotal", TypeofError = GetType(), Value = DocumentTotals.NetTotal.ToString(), UID = Pk });
                if (DocumentTotals.TaxPayable < 0)
                    listError.Add(new ValidationError { Description = string.Format("Valor do imposto a pagar do documento {0} incorrecto.", DocumentNumber), Field = "TaxPayable", TypeofError = GetType(), Value = DocumentTotals.TaxPayable.ToString(), UID = Pk });
            }

            return listError.ToArray();
        }
    }

    public partial class SourceDocumentsWorkingDocumentsWorkDocumentLine : BaseData
    {
        /// <summary>
        /// Link to the doc
        /// </summary>
        public string DocNo { get; set; }

        public ValidationError ValidateLineNumber(string SupPk = "", string workingDocument = "")
        {
            ValidationError erro = null;
            int num = -1;
            if (!string.IsNullOrEmpty(LineNumber))
                Int32.TryParse(LineNumber, out num);

            if (string.IsNullOrEmpty(LineNumber) || num == -1)
            {
                if (!string.IsNullOrEmpty(workingDocument))
                    workingDocument = string.Format(", documento {0}", workingDocument);

                erro = new ValidationError { Description = string.Format("Número de linha incorrecto {0}.", workingDocument), Field = "LineNumber", TypeofError = GetType(), Value = LineNumber, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.LineNumber = Tooltip.LineNumber.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateProductCode(string SupPk = "", string workingDocument = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 30)
            {
                if (!string.IsNullOrEmpty(workingDocument))
                    workingDocument = string.Format(" documento {0}", workingDocument);

                erro = new ValidationError { Description = string.Format("Identificador do produto ou serviço incorrecto, {0} linha {1}.", workingDocument, LineNumber), Field = "ProductCode", TypeofError = GetType(), Value = ProductCode, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.ProductCode = Tooltip.ProductCode.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateProductDescription(string SupPk = "", string workingDocument = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length > 200)
            {
                if (!string.IsNullOrEmpty(workingDocument))
                    workingDocument = string.Format(" documento {0}", workingDocument);

                erro = new ValidationError { Description = string.Format("Descrição do produto ou serviço incorrecta, {0} linha {1}.", workingDocument, LineNumber), Field = "ProductDescription", TypeofError = GetType(), Value = ProductDescription, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.ProductDescription = Tooltip.ProductDescription.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateQuantity(string SupPk = "", string workingDocument = "")
        {
            ValidationError erro = null;
            if (Quantity <= 0)
            {
                if (!string.IsNullOrEmpty(workingDocument))
                    workingDocument = string.Format(" documento {0}", workingDocument);

                erro = new ValidationError { Description = string.Format("Quantidade incorrecta, {0} linha {1}.", workingDocument, LineNumber), Field = "Quantity", TypeofError = GetType(), Value = Quantity.ToString(), UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.Quantity = Tooltip.Quantity.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateUnitOfMeasure(string SupPk = "", string workingDocument = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
            {
                if (!string.IsNullOrEmpty(workingDocument))
                    workingDocument = string.Format(" documento {0}", workingDocument);

                erro = new ValidationError { Description = string.Format("Unidade de medida incorrecta, {0} linha {1}.", workingDocument, LineNumber), Field = "UnitOfMeasure", TypeofError = GetType(), Value = UnitOfMeasure, UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.UnitOfMeasure = Tooltip.UnitOfMeasure.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateUnitPrice(string SupPk = "", string workingDocument = "")
        {
            ValidationError erro = null;
            if (UnitPrice == 0)
            {
                if (!string.IsNullOrEmpty(workingDocument))
                    workingDocument = string.Format(" documento {0}", workingDocument);

                erro = new ValidationError { Description = string.Format("Preço unitário incorrecto, {0} linha {1}.", workingDocument, LineNumber), Field = "UnitPrice", TypeofError = GetType(), Value = UnitPrice.ToString(), UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.UnitPrice = Tooltip.UnitPrice.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }
        public ValidationError ValidateTaxPointDate(string SupPk = "", string workingDocument = "")
        {
            ValidationError erro = null;
            if (TaxPointDate > DateTime.Now)
            {
                if (!string.IsNullOrEmpty(workingDocument))
                    workingDocument = string.Format(" documento {0}", workingDocument);

                erro = new ValidationError { Description = string.Format("Data de envio da mercadoria ou prestação do serviço incorrecta, {0} linha {1}.", workingDocument, LineNumber), Field = "TaxPointDate", TypeofError = GetType(), Value = TaxPointDate.ToString(), UID = Pk, SupUID = SupPk };
                //if (appendError)
                //    Tooltip.TaxPointDate = Tooltip.TaxPointDate.FormatTooltipWithError(erro.Description);
            }
            return erro;
        }

        public ValidationError[] ValidateOrderReferences(string SupPk = "", string workingDocument = "")
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (OrderReferences != null && OrderReferences.Length > 0)
            {
                foreach (var referencia in OrderReferences)
                {
                    if (string.IsNullOrEmpty(referencia.OriginatingON) == false && referencia.OriginatingON.Length > 60)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do número do documento de origem na linha {0} do documento {1} incorrecto.", LineNumber, workingDocument), Field = "OriginatingON", TypeofError = GetType(), Value = referencia.OriginatingON, UID = Pk, SupUID = SupPk });
                    if (referencia.OrderDate > DateTime.Now)
                        listError.Add(new ValidationError { Description = string.Format("Data do documento de origem na linha {0} do documento {1} incorrecto.", LineNumber, workingDocument), Field = "OrderDate", TypeofError = GetType(), Value = referencia.OrderDate.ToString(), UID = Pk, SupUID = SupPk });
                }
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateTax(string SupPk = "", string workingDocument = "")
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (Tax == null)
                listError.Add(new ValidationError { Description = string.Format("Taxa de imposto na linha {0} do documento {1} inexistente.", LineNumber, workingDocument), Field = "Tax", TypeofError = GetType(), UID = Pk, SupUID = SupPk });

            if (Tax != null)
            {
                if (Tax.ItemElementName == ItemChoiceType1.TaxAmount && Tax.TaxAmount < 0)
                    listError.Add(new ValidationError { Description = string.Format("Montante do imposto na linha {0} do documento {1} incorrecto.", LineNumber, workingDocument), Field = "TaxAmount", TypeofError = GetType(), Value = Tax.TaxAmount.ToString(), UID = Pk, SupUID = SupPk });
                if (Tax.ItemElementName == ItemChoiceType1.TaxPercentage && (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100))
                    listError.Add(new ValidationError { Description = string.Format("Percentagem da taxa do imposto na linha {0} do documento {1} incorrecto.", LineNumber, workingDocument), Field = "TaxPercentage", TypeofError = GetType(), Value = Tax.TaxPercentage.ToString(), UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                    listError.Add(new ValidationError { Description = string.Format("Código da taxa na linha {0} do documento {1} incorrecto.", LineNumber, workingDocument), Field = "TaxCode", TypeofError = GetType(), Value = Tax.TaxCode, UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                    listError.Add(new ValidationError { Description = string.Format("País ou região do imposto na linha {0} do documento {1} incorrecto.", LineNumber, workingDocument), Field = "TaxCountryRegion", TypeofError = GetType(), Value = Tax.TaxCountryRegion, UID = Pk, SupUID = SupPk });
            }

            return listError.ToArray();
        }
    }

    public partial class SourceDocumentsSalesInvoicesInvoice : BaseData
    {
        public string HashTest { get; set; }

        public Customer Customer { get; set; }

        public SourceDocumentsSalesInvoicesInvoice()
        {
            TooltipInvoiceNo = string.Format("4.1.4.1 * Texto 60{0}Identificação única do documento de venda.{0}Esta identificação é composta sequencialmente pelos seguintes elementos:{0}o código interno do documento atribuído pela aplicação, um espaço, o identificador da série do documento, uma barra (/) e o número sequencial desse documento dentro dessa série.{0}Não podem existir registos com a mesma identificação.{0}Não podem ser utilizados o mesmo código interno de documento em tipos de documentos (InvoiceType) diferentes.", Environment.NewLine);
            TooltipDocumentStatus = string.Format("4.1.4.2 * Situação do documento.", Environment.NewLine);
            TooltipInvoiceStatus = string.Format("4.1.4.2.1 * Texto 1{0}Estado atual do documento.{0}Deve ser preenchido com:{0}«N» - Normal;{0}«S» - Autofaturação;{0}«A» - Documento anulado;{0}«R» - Documento de resumo doutros documentos criados noutras aplicações e gerado nesta aplicação;{0}«F» - Documento faturado:{0}• Quando o documento é do tipo talão de venda ou talão de devolução e existe na tabela o correspondente do tipo fatura ou nota de crédito para dados até 2012-12-31;{0}• Quando o documento é do tipo fatura simplificada e existe na tabela o correspondente do tipo fatura - para dados após 2013-01-01.", Environment.NewLine);
            TooltipInvoiceStatusDate = string.Format("4.1.4.2.2 * Data e hora{0}Data e hora do estado atual do documento.{0}Data da última gravação do estado do documento ao segundo.{0}Tipo data e hora: «AAAA-MM-DDThh:mm:ss».", Environment.NewLine);
            TooltipReason = string.Format("4.1.4.2.3 Texto 50{0}Motivo da alteração de estado.{0}Deve ser indicada a razão que levou à alteração de estado do documento.", Environment.NewLine);
            TooltipResponsableUserSourceID = string.Format("4.1.4.2.4 * Texto 30{0}Código do utilizador.{0}Utilizador responsável pelo estado atual do documento.", Environment.NewLine);
            TooltipSourceBilling = string.Format("4.1.4.2.5 * Texto 1{0}Deve ser preenchido com: «P» - Documento produzido na aplicação;{0}«I» - Documento integrado e produzido noutra aplicação;{0}«M» - Documento proveniente de recuperação ou de emissão manual.", Environment.NewLine);
            TooltipHash = string.Format("4.1.4.3 * Texto 172{0}Chave do documento.{0}Assinatura nos termos da Portaria n.º 363/2010, de 23 de junho.{0}O campo deve ser preenchido com «0» (zero), caso não haja obrigatoriedade de certificação.", Environment.NewLine);
            TooltipHashControl = string.Format("4.1.4.4 Texto 40{0}Chave de controlo.{0}Versão da chave privada utilizada na criação da assinatura do campo 4.1.4.3.", Environment.NewLine);
            TooltipPeriod = string.Format("4.1.4.5 Inteiro{0}Período contabilístico.{0}Deve ser indicado o mês do período de tributação de «1» a «12», contando desde a data do seu início.", Environment.NewLine);
            TooltipInvoiceDate = string.Format("4.1.4.6 * Data{0}Data do documento de venda.{0}Data de emissão do documento de venda.", Environment.NewLine);
            TooltipInvoiceType = string.Format("4.1.4.7 * Texto 2{0}Tipo de documento.{0}Deve ser preenchido com:{0}«FT» - Fatura, emitida nos termos do artigo 36.º do Código do IVA;{0}«FS» - Fatura simplificada, emitida nos termos do artigo 40.º do Código do IVA;{0}«FR» - Fatura-recibo;{0}«ND» - Nota de débito;{0}«NC» - Nota de crédito;{0}«VD» - Venda a dinheiro e fatura/recibo (a);{0}«TV» - Talão de venda (a);{0}«TD» - Talão de devolução (a);{0}«AA» - Alienação de ativos;{0}«DA» - Devolução de ativos.{0}Para o setor Segurador, ainda pode ser preenchido com:{0}«RP» - Prémio ou recibo de prémio;{0}«RE» - Estorno ou recibo de estorno;{0}«CS» - Imputação a cosseguradoras; «LD» - Imputação a cosseguradora líder;{0}«RA» - Resseguro aceite.{0}(a) Para os dados até 2012-12-31.", Environment.NewLine);
            TooltipSelfBillingIndicator = string.Format("4.1.4.8.1 * Inteiro{0}Indicador de autofaturação.{0}Deverá ser preenchido com «1» se respeitar a autofaturação e com «0» (zero) no caso contrário.", Environment.NewLine);
            TooltipCashVATSchemeIndicator = string.Format("4.1.4.8.2 * Inteiro{0}Indicador de faturação emitida em nome e por conta de terceiros.{0}Deve ser preenchido com «1» se respeitar a faturação emitida em nome e por conta de terceiros e com «0» (zero) no caso contrário.", Environment.NewLine);
            TooltipThirdPartiesBillingIndicator = string.Format("4.1.4.8.3 * Inteiro{0}Indicador de autofaturação.{0}Deverá ser preenchido com «1» se respeitar a autofaturação e com «0» (zero) no caso contrário.", Environment.NewLine);
            TooltipGeneratedDocumentUserSourceID = string.Format("4.1.4.9 * Texto 30{0}Código do utilizador.{0}Utilizador que gerou o documento.", Environment.NewLine);
            TooltipSystemEntryDate = string.Format("4.1.4.10 * Data e hora{0}Data de gravação do documento.{0}Data da gravação do registo ao segundo, no momento da assinatura.{0}Tipo data e hora: «AAAA-MM-DDThh:mm:ss».", Environment.NewLine);
            TooltipTransactionID = string.Format("4.1.4.11 ** Texto 70{0}Identificador da transação (TransactionID).{0}O preenchimento é obrigatório, no caso de se tratar de um sistema integrado em que o campo 1.4 - Sistema contabilístico (TaxAccountingBasis) = «I».{0}Chave única da tabela de movimentos contabilísticos (GeneralLedgerEntries) da transação onde foi lançado este documento, respeitando a regra aí definida para o campo chave única do movimento contabilístico (TransactionID).", Environment.NewLine);
            TooltipCustomerID = string.Format("4.1.4.12 * Texto 30{0}Identificador do cliente.{0}Chave única da tabela de clientes (Customer) respeitando a regra aí definida para o campo identificador único do cliente (CustomerID).", Environment.NewLine);
            TooltipShipTo = string.Format("4.1.4.13 Envio para.{0}Informação do local e data de entrega onde os artigos vendidos são colocados à disposição do cliente.", Environment.NewLine);
            TooltipShipToDeliveryID = string.Format("4.1.4.13.1 Texto 30{0}Identificador da entrega.", Environment.NewLine);
            TooltipShipToDeliveryDate = string.Format("4.1.4.13.2 Data{0}Data da entrega.", Environment.NewLine);
            TooltipShipToWarehouseID = string.Format("4.1.4.13.3 Texto 50{0}Identificador do armazém de destino.", Environment.NewLine);
            TooltipShipToLocationID = string.Format("4.1.4.13.4 Texto 30{0}Localização dos bens no armazém de destino.", Environment.NewLine);
            TooltipShipToAddress = string.Format("4.1.4.13.5 Morada.", Environment.NewLine);
            TooltipShipToBuildingNumber = string.Format("4.1.4.13.5.1 Texto 10{0}Número de polícia.", Environment.NewLine);
            TooltipShipToStreetName = string.Format("4.1.4.13.5.2 Texto 90{0}Nome da rua.", Environment.NewLine);
            TooltipShipToAddressDetail = string.Format("4.1.4.13.5.3 * Texto 100{0}Morada detalhada.{0}Deve incluir o nome da rua, número de polícia e andar, se aplicável.", Environment.NewLine);
            TooltipShipToCity = string.Format("4.1.4.13.5.4 * Texto 50{0}Localidade.", Environment.NewLine);
            TooltipShipToPostalCode = string.Format("4.1.4.13.5.5 * Texto 20{0}Código postal.", Environment.NewLine);
            TooltipShipToRegion = string.Format("4.1.4.13.5.6 Texto 50{0}Distrito.", Environment.NewLine);
            TooltipShipToCountry = string.Format("4.1.4.13.5.7 * Texto 2{0}País.{0}Deve ser preenchido de acordo com a norma ISO 3166-1-alpha-2.", Environment.NewLine);
            TooltipShipFrom = string.Format("4.1.4.14 Envio de.{0}Informação do local e data de carga onde se inicia a expedição dos artigos vendidos para o cliente.", Environment.NewLine);
            TooltipShipFromDeliveryID = string.Format("4.1.4.14.1 Texto 30{0}Identificador da entrega", Environment.NewLine);
            TooltipShipFromDeliveryDate = string.Format("4.1.4.14.2 Data{0}Data de receção.", Environment.NewLine);
            TooltipShipFromWarehouseID = string.Format("4.1.4.14.3 Texto 50{0}Identificador do armazém de partida.", Environment.NewLine);
            TooltipShipFromLocationID = string.Format("4.1.4.14.4 Texto 30{0}Localização dos bens no armazém de partida.", Environment.NewLine);
            TooltipShipFromAddress = string.Format("4.1.4.14.5 Morada.", Environment.NewLine);
            TooltipShipFromBuildingNumber = string.Format("4.1.4.14.5.1 Texto 10{0}Número de polícia.", Environment.NewLine);
            TooltipShipFromStreetName = string.Format("4.1.4.14.5.2 Texto 90{0}Nome da rua.", Environment.NewLine);
            TooltipShipFromAddressDetail = string.Format("4.1.4.14.5.3 * Texto 100{0}Morada detalhada.{0}Deve incluir o nome da rua, número de polícia e andar, se aplicável.", Environment.NewLine);
            TooltipShipFromCity = string.Format("4.1.4.14.5.4 * Texto 50{0}Localidade.", Environment.NewLine);
            TooltipShipFromPostalCode = string.Format("4.1.4.14.5.5 * Texto 20{0}Código postal.", Environment.NewLine);
            TooltipShipFromRegion = string.Format("4.1.4.14.5.6 Texto 50{0}Distrito.", Environment.NewLine);
            TooltipShipFromCountry = string.Format("4.1.4.14.5.7 * Texto 2{0}País.{0}Deve ser preenchido de acordo com a norma ISO 3166-1-alpha-2.", Environment.NewLine);
            TooltipMovementEndTime = string.Format("4.1.4.15 Data e hora{0}Data e hora de fim de transporte.{0}Tipo de data e hora: «AAAA-MM-DDThh:mm:ss» em que o «ss» pode ser «00», na ausência de informação concreta.", Environment.NewLine);
            TooltipMovementStartTime = string.Format("4.1.4.16 Data e hora{0}Data e hora para o início de transporte.{0}Tipo de data e hora: «AAAA-MM-DDThh:mm:ss» em que o «ss» pode ser «00», na ausência de informação concreta.", Environment.NewLine);
            TooltipATDocCodeID = string.Format("4.1.4.17 Texto 200{0}Código de identificação do documento.{0}Código de identificação atribuído pela AT ao documento, nos termos do Decreto-Lei n.º 198/2012, de 24 de agosto.", Environment.NewLine);
            TooltipReferences = string.Format("4.1.4.18.9 Referências.{0}Referências a outros documentos.", Environment.NewLine);
            TooltipCreditNote = string.Format("4.1.4.18.9.1 Nota de crédito.{0}Referências da nota de crédito, caso seja aplicável.", Environment.NewLine);
            TooltipReference = string.Format("4.1.4.18.9.1 Texto 60{0}Referência.{0}Referência à fatura ou fatura simplificada, através de identificação única da mesma, nos sistemas em que exista.{0}Deve ser utilizada a estrutura de numeração do campo de origem.", Environment.NewLine);
            TooltipLineReason = string.Format("4.1.4.18.9.1 Texto 50{0}Motivo.{0}Deve ser preenchido com o motivo do crédito.", Environment.NewLine);
            TooltipDescription = string.Format("4.1.4.18.10 * Texto 60{0}Descrição.{0}Descrição da linha do documento.", Environment.NewLine);
            TooltipDebitAmount = string.Format("4.1.4.18.11 ** Monetário{0}Valor a débito.{0}Valor da linha dos documentos a lançar a débito na conta de vendas.{0}Este valor é sem imposto e deduzido dos descontos de linha e cabeçalho.", Environment.NewLine);
            TooltipCreditAmount = string.Format("4.1.4.18.12 ** Monetário{0}Valor a crédito.{0}Valor da linha dos documentos a lançar a crédito à conta de vendas.{0}Este valor é sem imposto e deduzido dos descontos de linha e cabeçalho.", Environment.NewLine);
            TooltipTax = string.Format("4.1.4.18.13 Taxa de imposto.", Environment.NewLine);
            TooltipTaxType = string.Format("4.1.4.18.13 * Texto 3{0}Identificador do regime de imposto.{0}Neste campo deve ser indicado o tipo de imposto.{0}Deve preenchido com: «IVA» - Imposto sobre o valor acrescentado; «IS» - Imposto do selo.", Environment.NewLine);
            TooltipTaxCountryRegion = string.Format("4.1.4.18.13 * Texto 5{0}País ou região do imposto.{0}Deve ser preenchido de acordo com a norma ISO 3166-1-alpha-2.{0}No caso das Regiões Autónomas da Madeira e Açores deve ser preenchido com: «PT-AC» - Espaço fiscal da Região Autónoma dos Açores; «PT-MA» - Espaço fiscal da Região Autónoma da Madeira.", Environment.NewLine);
            TooltipTaxCode = string.Format("4.1.4.18.13 * Texto 10{0}Código da taxa.{0}Código da taxa na tabela de impostos.{0}Tem que ser preenchido quando os campos percentagem da taxa de imposto (TaxPercentage) ou montante do imposto (TaxAmount) são diferentes de zero.{0}No caso do código do tipo de imposto (TaxType) = IVA, deve ser preenchido com: «RED» - Taxa reduzida; «INT» - Taxa intermédia; «NOR» - Taxa normal; «ISE» - Isenta; «OUT» - Outros, aplicável para os regimes especiais de IVA.{0}No caso do código do tipo de imposto (TaxType) = «IS», deve ser preenchido com o código da verba respetiva.", Environment.NewLine);
            TooltipTaxPercentage = string.Format("4.1.4.18.13 ** Decimal{0}Percentagem da taxa de imposto.{0}O preenchimento é obrigatório, no caso de se tratar de uma percentagem de imposto.{0}A percentagem da taxa é correspondente ao imposto aplicável ao campo 4.1.4.18.11 - Valor a débito (DebitAmount) ou ao campo 4.1.418.12 - Valor a crédito (CreditAmount).", Environment.NewLine);
            TooltipTaxAmount = string.Format("4.1.4.18.13 ** Monetário{0}Montante do imposto.{0}O preenchimento é obrigatório, no caso de se tratar de uma verba fixa de imposto do selo.", Environment.NewLine);
            TooltipTaxExemptionReason = string.Format("4.1.4.18.14 ** Texto 60{0}Motivo da isenção de imposto.{0}O preenchimento é obrigatório, quando os campos percentagem da taxa de imposto (TaxPercentage) ou montante do imposto (TaxAmount) são iguais a zero.{0}Deve ser referido o preceito legal aplicável.{0}Este campo deve ser igualmente preenchido nos casos de não sujeição aos impostos referidos na tabela 2.5 - Tabela de impostos (TaxTable).", Environment.NewLine);
            TooltipLineSettlementAmount = string.Format("4.1.4.18.15 Monetário{0}Montante do desconto da linha.{0}Deve refletir todos os descontos concedidos (globais e de linha) que afetam o valor do campo 4.1.4.19.3 - GrossTotal.", Environment.NewLine);
            TooltipDocumentTotals = string.Format("4.1.4.19 * Totais do documento.", Environment.NewLine);
            TooltipTaxPayable = string.Format("4.1.4.19.1 * Monetário{0}Valor do imposto a pagar.", Environment.NewLine);
            TooltipNetTotal = string.Format("4.1.4.19.2 * Monetário{0}Total do documento sem impostos.{0}Este campo não deve incluir as parcelas referentes aos impostos constantes da tabela de impostos (TaxTable).", Environment.NewLine);
            TooltipGrossTotal = string.Format("4.1.4.19.3 * Monetário{0}Total do documento com impostos.{0}Este campo não deve refletir eventuais retenções na fonte constantes no campo 4.1.4.20 - Retenção na fonte (WithholdingTax).", Environment.NewLine);
            TooltipCurrency = string.Format("4.1.4.19.4.1 * Texto 3{0}Código de moeda.{0}No caso de moeda estrangeira deve ser preenchido de acordo com a norma ISO 4217.", Environment.NewLine);
            TooltipCurrencyAmount = string.Format("4.1.4.19.4.2 * Monetário{0}Valor total em moeda estrangeira.{0}Valor do campo 4.1.4.19.3 - Total do documento com impostos (GrossTotal) na moeda original do documento.", Environment.NewLine);
            TooltipExchangeRate = string.Format("4.1.4.19.4.3 Decimal{0}Taxa de câmbio.{0}Deve ser indicada a taxa de câmbio utilizada na conversão para EUR.", Environment.NewLine);
            TooltipSettlement = string.Format("4.1.4.19.5 Acordos.{0}Acordos ou formas de pagamento.", Environment.NewLine);
            TooltipSettlementDiscount = string.Format("4.1.4.19.5.1 Texto 30{0}Acordos de descontos futuros.{0}Deve ser preenchido com os acordos de descontos a aplicar no futuro sobre o valor presente.", Environment.NewLine);
            TooltipSettlementAmount = string.Format("4.1.4.19.5.2 Monetário{0}Montante do desconto.{0}Representa o valor do desconto futuro sem afetar o valor presente do documento indicado no campo 4.1.4.19.3 - Total do documento com impostos (GrossTotal).", Environment.NewLine);
            TooltipSettlementDate = string.Format("4.1.4.19.5.3 Data{0}Data acordada para o desconto.{0}A informação a constar é a data acordada para o pagamento com desconto.", Environment.NewLine);
            TooltipPaymentTerms = string.Format("4.1.4.19.5.4 Texto 100{0}Acordos de pagamento.{0}A informação a constar são os acordos estabelecidos, a data limite de pagamento ou os prazos relativos a regimes especiais de exigibilidade de IVA.", Environment.NewLine);
            TooltipPaymentMechanism = string.Format("4.1.4.19.5.5 Texto 2{0}Forma prevista de pagamento.{0}Deve ser preenchido com:{0}«CC» - Cartão crédito;{0}«CD» - Cartão débito{0}«CH» - Cheque;{0}«CS» - Compensação de saldos em conta corrente;{0}«LC» - Letra comercial;{0}«MB» - Multibanco;{0}«NU» - Numerário;{0}«PR» - Permuta;{0}«TB» - Transferência bancária;{0}«TR» - Ticket restaurante.", Environment.NewLine);
            TooltipWithholdingTax = string.Format("4.1.4.20 Retenção na fonte.", Environment.NewLine);
            TooltipWithholdingTaxType = string.Format("4.1.4.20.1 Texto 3{0}Código do tipo de imposto retido.{0}Neste campo deve ser indicado o tipo de imposto retido, preenchendo-o com:{0}«IRS» - Imposto sobre o rendimento de pessoas singulares;{0}«IRC» - Imposto sobre o rendimento de pessoas coletivas;{0}«IS» - Imposto do selo.", Environment.NewLine);
            TooltipWithholdingTaxDescription = string.Format("4.1.4.20.2 Texto 60{0}Motivo da retenção na fonte.{0}Deve ser indicado o normativo legal aplicável.{0}No caso do código do tipo de imposto (TaxType) = IS, deve ser preenchido com o código da verba respetiva.", Environment.NewLine);
            TooltipWithholdingTaxAmount = string.Format("4.1.4.20.3 * Monetário{0}Montante da retenção na fonte.{0}Deve ser indicado o montante retido de imposto.", Environment.NewLine);
        }

        public string TooltipInvoiceNo { get; set; }
        public string TooltipDocumentStatus { get; set; }
        public string TooltipInvoiceStatus { get; set; }
        public string TooltipInvoiceStatusDate { get; set; }
        public string TooltipReason { get; set; }
        public string TooltipResponsableUserSourceID { get; set; }
        public string TooltipSourceBilling { get; set; }
        public string TooltipHash { get; set; }
        public string TooltipHashControl { get; set; }
        public string TooltipPeriod { get; set; }
        public string TooltipInvoiceDate { get; set; }
        public string TooltipInvoiceType { get; set; }
        public string TooltipSelfBillingIndicator { get; set; }
        public string TooltipCashVATSchemeIndicator { get; set; }
        public string TooltipThirdPartiesBillingIndicator { get; set; }
        public string TooltipGeneratedDocumentUserSourceID { get; set; }
        public string TooltipSystemEntryDate { get; set; }
        public string TooltipTransactionID { get; set; }
        public string TooltipCustomerID { get; set; }
        public string TooltipShipTo { get; set; }
        public string TooltipShipToDeliveryID { get; set; }
        public string TooltipShipToDeliveryDate { get; set; }
        public string TooltipShipToWarehouseID { get; set; }
        public string TooltipShipToLocationID { get; set; }
        public string TooltipShipToAddress { get; set; }
        public string TooltipShipToBuildingNumber { get; set; }
        public string TooltipShipToStreetName { get; set; }
        public string TooltipShipToAddressDetail { get; set; }
        public string TooltipShipToCity { get; set; }
        public string TooltipShipToPostalCode { get; set; }
        public string TooltipShipToRegion { get; set; }
        public string TooltipShipToCountry { get; set; }
        public string TooltipShipFrom { get; set; }
        public string TooltipShipFromDeliveryID { get; set; }
        public string TooltipShipFromDeliveryDate { get; set; }
        public string TooltipShipFromWarehouseID { get; set; }
        public string TooltipShipFromLocationID { get; set; }
        public string TooltipShipFromAddress { get; set; }
        public string TooltipShipFromBuildingNumber { get; set; }
        public string TooltipShipFromStreetName { get; set; }
        public string TooltipShipFromAddressDetail { get; set; }
        public string TooltipShipFromCity { get; set; }
        public string TooltipShipFromPostalCode { get; set; }
        public string TooltipShipFromRegion { get; set; }
        public string TooltipShipFromCountry { get; set; }
        public string TooltipMovementEndTime { get; set; }
        public string TooltipMovementStartTime { get; set; }
        public string TooltipATDocCodeID { get; set; }
        public string TooltipReferences { get; set; }
        public string TooltipCreditNote { get; set; }
        public string TooltipReference { get; set; }
        public string TooltipLineReason { get; set; }
        public string TooltipDescription { get; set; }
        public string TooltipDebitAmount { get; set; }
        public string TooltipCreditAmount { get; set; }
        public string TooltipTax { get; set; }
        public string TooltipTaxType { get; set; }
        public string TooltipTaxCountryRegion { get; set; }
        public string TooltipTaxCode { get; set; }
        public string TooltipTaxPercentage { get; set; }
        public string TooltipTaxAmount { get; set; }
        public string TooltipTaxExemptionReason { get; set; }
        public string TooltipLineSettlementAmount { get; set; }
        public string TooltipDocumentTotals { get; set; }
        public string TooltipTaxPayable { get; set; }
        public string TooltipNetTotal { get; set; }
        public string TooltipGrossTotal { get; set; }
        public string TooltipCurrency { get; set; }
        public string TooltipCurrencyAmount { get; set; }
        public string TooltipExchangeRate { get; set; }
        public string TooltipSettlement { get; set; }
        public string TooltipSettlementDiscount { get; set; }
        public string TooltipSettlementAmount { get; set; }
        public string TooltipSettlementDate { get; set; }
        public string TooltipPaymentTerms { get; set; }
        public string TooltipPaymentMechanism { get; set; }
        public string TooltipWithholdingTax { get; set; }
        public string TooltipWithholdingTaxType { get; set; }
        public string TooltipWithholdingTaxDescription { get; set; }
        public string TooltipWithholdingTaxAmount { get; set; }

        public ValidationError ValidateATCUD()
        {
            ValidationError erro = null;
            if (string.IsNullOrWhiteSpace(ATCUD))
            {
                erro = new ValidationError { Description = "ATCUD não pode ser vazio.", Field = "ATCUD", TypeofError = GetType(), Value = ATCUD, UID = Pk };
                TooltipInvoiceNo += Environment.NewLine + erro.Description;
            }
            else if (ATCUD != "0")
            {
                string[] parts = ATCUD.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts == null || parts.Length != 2)
                {
                    erro = new ValidationError { Description = "ATCUD deveria ser formado pela concatenação dos campos «CodigodeValidação-NumeroSequencial».", Field = "ATCUD", TypeofError = GetType(), Value = ATCUD, UID = Pk };
                    TooltipInvoiceNo += Environment.NewLine + erro.Description;
                }
                else
                {
                    if (parts[0].Length < 8)
                    {
                        erro = new ValidationError { Description = "CodigodeValidação do ATCUD com tamanho incorrecto.", Field = "ATCUD", TypeofError = GetType(), Value = ATCUD, UID = Pk };
                        TooltipInvoiceNo += Environment.NewLine + erro.Description;
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
                erro = new ValidationError { Description = "Identificação única com tamanho incorrecto.", Field = "InvoiceNo", TypeofError = GetType(), Value = InvoiceNo, UID = Pk };
                TooltipInvoiceNo += Environment.NewLine + erro.Description;
            }
            else if (!Regex.IsMatch(InvoiceNo, "([a-zA-Z0-9./_-])+ ([a-zA-Z0-9]*/[0-9]+)"))
            {
                erro = new ValidationError { Description = "Identificação única com caracteres não permitidos.", Field = "InvoiceNo", TypeofError = GetType(), Value = InvoiceNo, UID = Pk };
                TooltipInvoiceNo += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateHash()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(Hash) || Hash.Length != 172)
            {
                erro = new ValidationError { Description = string.Format("Assinatura do documento {0} de tamanho incorrecto.", InvoiceNo), Field = "Hash", TypeofError = GetType(), Value = Hash, UID = Pk };
                TooltipHash += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateHashControl()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(HashControl) || HashControl.Length > 40)
            {
                erro = new ValidationError { Description = string.Format("Versão da chave privada utilizada na assinatura do documento {0} incorrecta.", InvoiceNo), Field = "HashControl", TypeofError = GetType(), Value = HashControl, UID = Pk };
                TooltipHashControl += Environment.NewLine + erro.Description;
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
                    erro = new ValidationError { Description = string.Format("Mês do período de tributação do documento {0} incorrecto.", InvoiceNo), Field = "Period", TypeofError = GetType(), Value = Period, UID = Pk };
                    TooltipPeriod += Environment.NewLine + erro.Description;
                }
            }

            return erro;
        }
        public ValidationError ValidateInvoiceDate()
        {
            ValidationError erro = null;

            if (InvoiceDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data de emissão do documento {0} incorrecta.", InvoiceNo), Field = "InvoiceDate", TypeofError = GetType(), Value = InvoiceDate.ToString(), UID = Pk };
                TooltipInvoiceDate += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateSystemEntryDate()
        {
            ValidationError erro = null;

            if (SystemEntryDate > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data da gravação do documento {0} incorrecta.", InvoiceNo), Field = "SystemEntryDate", TypeofError = GetType(), Value = SystemEntryDate.ToString(), UID = Pk };
                TooltipSystemEntryDate += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateTransactionID()
        {
            ValidationError erro = null;

            if (!string.IsNullOrEmpty(TransactionID) && TransactionID.Length > 70)
            {
                erro = new ValidationError { Description = string.Format("Identificador da transacção do documento {0} incorrecto.", InvoiceNo), Field = "TransactionID", TypeofError = GetType(), Value = TransactionID, UID = Pk };
                TooltipTransactionID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateCustomerID()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
            {
                erro = new ValidationError { Description = string.Format("Chave única da tabela de clientes no documento {0} incorrecta.", InvoiceNo), Field = "CustomerID", TypeofError = GetType(), Value = CustomerID, UID = Pk };
                TooltipCustomerID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateSourceID()
        {
            ValidationError erro = null;

            if (string.IsNullOrEmpty(SourceID) || SourceID.Length > 30)
            {
                erro = new ValidationError { Description = string.Format("Utilizador que gerou o documento {0} incorrecto.", InvoiceNo), Field = "CustomerID", TypeofError = GetType(), Value = CustomerID, UID = Pk };
                TooltipGeneratedDocumentUserSourceID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateMovementEndTime()
        {
            ValidationError erro = null;

            if (MovementEndTime == DateTime.MinValue || MovementEndTime > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data e hora de fim de transporte do documento {0} incorrecta.", InvoiceNo), Field = "MovementEndTime", TypeofError = GetType(), Value = MovementEndTime.ToString(), UID = Pk };
                TooltipMovementEndTime += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateMovementStartTime()
        {
            ValidationError erro = null;

            if (MovementStartTime == DateTime.MinValue || MovementStartTime > DateTime.Now)
            {
                erro = new ValidationError { Description = string.Format("Data e hora de início de transporte do documento {0} incorrecta.", InvoiceNo), Field = "MovementStartTime", TypeofError = GetType(), Value = MovementStartTime.ToString(), UID = Pk };
                TooltipMovementStartTime += Environment.NewLine + erro.Description;
            }

            return erro;
        }

        public ValidationError[] ValidateDocumentStatus()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (DocumentStatus == null)
                listError.Add(new ValidationError { Description = string.Format("Situação do documento {0} inexistente.", InvoiceNo), Field = "DocumentStatus", TypeofError = GetType(), UID = Pk });

            if (DocumentStatus != null)
            {
                if (DocumentStatus.InvoiceStatusDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data e hora do estado atual do documento {0} incorrecta.", InvoiceNo), Field = "InvoiceStatusDate", TypeofError = GetType(), Value = DocumentStatus.InvoiceStatusDate.ToString(), UID = Pk });
                if (string.IsNullOrEmpty(DocumentStatus.Reason) == false && DocumentStatus.Reason.Length > 50)
                    listError.Add(new ValidationError { Description = string.Format("Tamanho do motivo da alteração de estado {0} incorrecto.", InvoiceNo), Field = "Reason", TypeofError = GetType(), Value = DocumentStatus.Reason, UID = Pk });
                if (string.IsNullOrEmpty(DocumentStatus.SourceID) || DocumentStatus.SourceID.Length > 30)
                    listError.Add(new ValidationError { Description = string.Format("Utilizador responsável pelo estado atual do documento {0} incorrecto.", InvoiceNo), Field = "SourceID", TypeofError = GetType(), Value = DocumentStatus.SourceID, UID = Pk });
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateSpecialRegimes()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (SpecialRegimes != null)
            {
                int.TryParse(SpecialRegimes.SelfBillingIndicator, out int auto);
                if (string.IsNullOrEmpty(SpecialRegimes.SelfBillingIndicator) || (auto != 0 && auto != 1))
                    listError.Add(new ValidationError { Description = string.Format("Indicador de autofaturação do documento {0} incorrecto.", InvoiceNo), Field = "SelfBillingIndicator", TypeofError = GetType(), Value = SpecialRegimes.SelfBillingIndicator, UID = Pk });

                if (string.IsNullOrEmpty(SpecialRegimes.CashVATSchemeIndicator) || (SpecialRegimes.CashVATSchemeIndicator != "0" && SpecialRegimes.CashVATSchemeIndicator != "1"))
                    listError.Add(new ValidationError { Description = string.Format("Indicador da existência de adesão ao regime de IVA de Caixa do documento {0} incorrecto.", InvoiceNo), Field = "CashVATSchemeIndicator", TypeofError = GetType(), Value = SpecialRegimes.CashVATSchemeIndicator, UID = Pk });
                if (string.IsNullOrEmpty(SpecialRegimes.ThirdPartiesBillingIndicator) || (SpecialRegimes.ThirdPartiesBillingIndicator != "0" && SpecialRegimes.CashVATSchemeIndicator != "1"))
                    listError.Add(new ValidationError { Description = string.Format("Indicador da existência de adesão ao regime de IVA de Caixa do documento {0} incorrecto.", InvoiceNo), Field = "ThirdPartiesBillingIndicator", TypeofError = GetType(), Value = SpecialRegimes.ThirdPartiesBillingIndicator, UID = Pk });
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateShipTo()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (ShipTo != null)
            {
                if (ShipTo.Address != null)
                {
                    if (string.IsNullOrEmpty(ShipTo.Address.AddressDetail) || ShipTo.Address.AddressDetail.Length > 100)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da morada detalhada do documento {0} do local de descarga incorrecto.", InvoiceNo), Field = "AddressDetail", TypeofError = GetType(), Value = ShipTo.Address.AddressDetail, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.BuildingNumber) == false && ShipTo.Address.BuildingNumber.Length > 10)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do número de polícia do documento {0} do local de descarga incorrecto.", InvoiceNo), Field = "BuildingNumber", TypeofError = GetType(), Value = ShipTo.Address.BuildingNumber, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.City) || ShipTo.Address.City.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da localidade do documento {0} do local de descarga incorrecto.", InvoiceNo), Field = "City", TypeofError = GetType(), Value = ShipTo.Address.City, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.Country) || ShipTo.Address.Country.Length != 2)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do País do documento {0} do local de descarga incorrecto.", InvoiceNo), Field = "Country", TypeofError = GetType(), Value = ShipTo.Address.Country, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.PostalCode) || ShipTo.Address.PostalCode.Length > 20)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do código postal do documento {0} do local de descarga incorrecto.", InvoiceNo), Field = "PostalCode", TypeofError = GetType(), Value = ShipTo.Address.PostalCode, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.Region) == false && ShipTo.Address.Region.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do distrito do documento {0} do local de descarga incorrecto.", InvoiceNo), Field = "Region", TypeofError = GetType(), Value = ShipTo.Address.Region, UID = Pk });
                    if (string.IsNullOrEmpty(ShipTo.Address.StreetName) == false && ShipTo.Address.StreetName.Length > 90)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do nome da rua do documento {0} do local de descarga incorrecto.", InvoiceNo), Field = "StreetName", TypeofError = GetType(), Value = ShipTo.Address.StreetName, UID = Pk });
                }
                if (ShipTo.DeliveryDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data da entrega do documento {0} incorrecto.", InvoiceNo), Field = "DeliveryDate", TypeofError = GetType(), Value = ShipTo.DeliveryDate.ToString(), UID = Pk });
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateShipFrom()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (ShipFrom != null)
            {
                if (ShipFrom.Address != null)
                {
                    if (string.IsNullOrEmpty(ShipFrom.Address.AddressDetail) || ShipFrom.Address.AddressDetail.Length > 100)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da morada detalhada do documento {0} do local de carga incorrecto.", InvoiceNo), Field = "AddressDetail", TypeofError = GetType(), Value = ShipFrom.Address.AddressDetail, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.BuildingNumber) == false && ShipFrom.Address.BuildingNumber.Length > 10)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do número de polícia do documento {0} do local de carga incorrecto.", InvoiceNo), Field = "BuildingNumber", TypeofError = GetType(), Value = ShipFrom.Address.BuildingNumber, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.City) || ShipFrom.Address.City.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da localidade do documento {0} do local de carga incorrecto.", InvoiceNo), Field = "City", TypeofError = GetType(), Value = ShipFrom.Address.City, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.Country) || ShipFrom.Address.Country.Length != 2)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do País do documento {0} do local de carga incorrecto.", InvoiceNo), Field = "Country", TypeofError = GetType(), Value = ShipFrom.Address.Country, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.PostalCode) || ShipFrom.Address.PostalCode.Length > 20)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do código postal do documento {0} do local de carga incorrecto.", InvoiceNo), Field = "PostalCode", TypeofError = GetType(), Value = ShipFrom.Address.PostalCode, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.Region) == false && ShipFrom.Address.Region.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do distrito do documento {0} do local de carga incorrecto.", InvoiceNo), Field = "Region", TypeofError = GetType(), Value = ShipFrom.Address.Region, UID = Pk });
                    if (string.IsNullOrEmpty(ShipFrom.Address.StreetName) == false && ShipFrom.Address.StreetName.Length > 90)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do nome da rua do documento {0} do local de carga incorrecto.", InvoiceNo), Field = "StreetName", TypeofError = GetType(), Value = ShipFrom.Address.StreetName, UID = Pk });
                }
                if (ShipFrom.DeliveryDate > DateTime.Now)
                    listError.Add(new ValidationError { Description = string.Format("Data da receção do documento {0} incorrecto.", InvoiceNo), Field = "DeliveryDate", TypeofError = GetType(), Value = ShipFrom.DeliveryDate.ToString(), UID = Pk });
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateDocumentTotals()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (DocumentTotals == null)
                listError.Add(new ValidationError { Description = string.Format("Totais do documento {0} inexistentes.", InvoiceNo), Field = "DocumentTotals", TypeofError = GetType(), UID = Pk });

            if (DocumentTotals != null)
            {
                if (DocumentTotals.Currency != null)
                {
                    if (DocumentTotals.Currency.CurrencyAmount < 0)
                        listError.Add(new ValidationError { Description = string.Format("Valor total em moeda estrangeira do documento {0} incorrecto.", InvoiceNo), Field = "CurrencyAmount", TypeofError = GetType(), Value = DocumentTotals.Currency.CurrencyAmount.ToString(), UID = Pk });
                    if (string.IsNullOrEmpty(DocumentTotals.Currency.CurrencyCode) || DocumentTotals.Currency.CurrencyCode.Length > 3)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do código de moeda do documento {0} incorrecto.", InvoiceNo), Field = "CurrencyCode", TypeofError = GetType(), Value = DocumentTotals.Currency.CurrencyCode, UID = Pk });
                    if (DocumentTotals.Currency.ExchangeRate < 0)
                        listError.Add(new ValidationError { Description = string.Format("Taxa de câmbio do documento {0} incorrecta.", InvoiceNo), Field = "ExchangeRate", TypeofError = GetType(), Value = DocumentTotals.Currency.ExchangeRate.ToString(), UID = Pk });
                }

                if (DocumentTotals.Settlement != null && DocumentTotals.Settlement.Length > 0)
                {
                    foreach (var acordo in DocumentTotals.Settlement)
                    {
                        if (string.IsNullOrEmpty(acordo.PaymentTerms) == false && acordo.PaymentTerms.Length > 100)
                            listError.Add(new ValidationError { Description = string.Format("Tamanho do acordo de pagamento do documento {0} incorrecto.", InvoiceNo), Field = "PaymentTerms", TypeofError = GetType(), Value = acordo.PaymentTerms, UID = Pk });
                        if (acordo.SettlementAmount < 0)
                            listError.Add(new ValidationError { Description = string.Format("Acordos de descontos futuros do documento {0} incorrecto.", InvoiceNo), Field = "SettlementAmount", TypeofError = GetType(), Value = acordo.SettlementAmount.ToString(), UID = Pk });
                        if (acordo.SettlementDate == DateTime.MinValue)
                            listError.Add(new ValidationError { Description = string.Format("Data acordada para o desconto do documento {0} incorrecto.", InvoiceNo), Field = "SettlementDate", TypeofError = GetType(), Value = acordo.SettlementDate.ToString(), UID = Pk });
                        if (string.IsNullOrEmpty(acordo.SettlementDiscount) == false && acordo.SettlementDiscount.Length > 30)
                            listError.Add(new ValidationError { Description = string.Format("Montante do desconto do documento {0} incorrecto.", InvoiceNo), Field = "SettlementAmount", TypeofError = GetType(), Value = acordo.SettlementAmount.ToString(), UID = Pk });
                    }
                }

                if (DocumentTotals.GrossTotal < 0)
                    listError.Add(new ValidationError { Description = string.Format("Total do documento {0} incorrecto.", InvoiceNo), Field = "GrossTotal", TypeofError = GetType(), Value = DocumentTotals.GrossTotal.ToString(), UID = Pk });
                if (DocumentTotals.NetTotal < 0)
                    listError.Add(new ValidationError { Description = string.Format("Total do documento {0} sem impostos incorrecto.", InvoiceNo), Field = "NetTotal", TypeofError = GetType(), Value = DocumentTotals.NetTotal.ToString(), UID = Pk });
                if (DocumentTotals.TaxPayable < 0)
                    listError.Add(new ValidationError { Description = string.Format("Valor do imposto a pagar do documento {0} incorrecto.", InvoiceNo), Field = "TaxPayable", TypeofError = GetType(), Value = DocumentTotals.TaxPayable.ToString(), UID = Pk });
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

        public SourceDocumentsSalesInvoicesInvoiceLine()
        {
            TooltipLineNumber = string.Format("4.1.4.18.1 * Inteiro{0}Número de linha.{0}As linhas devem ser exportadas pela mesma ordem em que se encontram no documento original.", Environment.NewLine);
            TooltipOrderReferences = string.Format("4.1.4.18.2 Referência ao documento de origem.", Environment.NewLine);
            TooltipOriginatingON = string.Format("4.1.4.18.2.1 Texto 255{0}Número do documento de origem.{0}Deve ser indicado o tipo, a série e o número do documento que despoletou a emissão.{0}Se o documento estiver contido no SAF-T(PT) deve ser utilizada a estrutura de numeração do campo de origem.{0}Caso sejam referenciados vários documentos, estes deverão ser separados por «;».", Environment.NewLine);
            TooltipOrderDate = string.Format("4.1.4.18.2.2 Data{0}Data do documento de origem.", Environment.NewLine);
            TooltipProductCode = string.Format("4.1.4.18.3 * Texto 30{0}Identificador do produto ou serviço.{0}Chave do registo na tabela de produtos/serviços.", Environment.NewLine);
            TooltipProductDescription = string.Format("4.1.4.18.4 * Texto 200{0}Descrição do produto ou serviço.{0}Descrição da linha da fatura, ligada à tabela de produtos/serviços.", Environment.NewLine);
            TooltipQuantity = string.Format("4.1.4.18.5 * Decimal{0}Quantidade.", Environment.NewLine);
            TooltipUnitOfMeasure = string.Format("4.1.4.18.6 * Texto 20{0}Unidade de medida.", Environment.NewLine);
            TooltipUnitPrice = string.Format("4.1.4.18.7 * Monetário{0}Preço unitário.{0}Preço unitário sem imposto e deduzido dos descontos de linha e cabeçalho.", Environment.NewLine);
            TooltipTaxPointDate = string.Format("4.1.4.18.8 * Data{0}Data de envio da mercadoria ou prestação do serviço.{0}Data de envio da mercadoria ou da prestação de serviço Deve ser preenchido com a data da guia de remessa asso- ciada, se existir.", Environment.NewLine);
        }

        public string TooltipLineNumber { get; set; }
        public string TooltipOrderReferences { get; set; }
        public string TooltipOriginatingON { get; set; }
        public string TooltipOrderDate { get; set; }
        public string TooltipProductCode { get; set; }
        public string TooltipProductDescription { get; set; }
        public string TooltipQuantity { get; set; }
        public string TooltipUnitOfMeasure { get; set; }
        public string TooltipUnitPrice { get; set; }
        public string TooltipTaxPointDate { get; set; }

        public ValidationError ValidateLineNumber(string SupPk = "", string invoiceNo = "")
        {
            ValidationError erro = null;
            int.TryParse(LineNumber, out int num);

            if (string.IsNullOrEmpty(LineNumber) || num == -1)
            {
                if (!string.IsNullOrEmpty(invoiceNo))
                    invoiceNo = string.Format(", documento {0}", invoiceNo);

                erro = new ValidationError { Description = string.Format("Número de linha incorrecto {0}.", invoiceNo), Field = "LineNumber", TypeofError = GetType(), Value = LineNumber, UID = Pk, SupUID = SupPk };
                TooltipLineNumber += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateProductCode(string SupPk = "", string invoiceNo = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 30)
            {
                if (!string.IsNullOrEmpty(invoiceNo))
                    invoiceNo = string.Format(" documento {0}", invoiceNo);

                erro = new ValidationError { Description = string.Format("Identificador do produto ou serviço incorrecto, {0} linha {1}.", invoiceNo, LineNumber), Field = "ProductCode", TypeofError = GetType(), Value = ProductCode, UID = Pk, SupUID = SupPk };
                TooltipProductCode += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateProductDescription(string SupPk = "", string invoiceNo = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length > 200)
            {
                if (!string.IsNullOrEmpty(invoiceNo))
                    invoiceNo = string.Format(" documento {0}", invoiceNo);

                erro = new ValidationError { Description = string.Format("Descrição do produto ou serviço incorrecta, {0} linha {1}.", invoiceNo, LineNumber), Field = "ProductDescription", TypeofError = GetType(), Value = ProductDescription, UID = Pk, SupUID = SupPk };
                TooltipProductDescription += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateQuantity(string SupPk = "", string invoiceNo = "")
        {
            ValidationError erro = null;
            if (Quantity <= 0)
            {
                if (!string.IsNullOrEmpty(invoiceNo))
                    invoiceNo = string.Format(" documento {0}", invoiceNo);

                erro = new ValidationError { Description = string.Format("Quantidade incorrecta, {0} linha {1}.", invoiceNo, LineNumber), Field = "Quantity", TypeofError = GetType(), Value = Quantity.ToString(), UID = Pk, SupUID = SupPk };
                TooltipQuantity += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateUnitOfMeasure(string SupPk = "", string invoiceNo = "")
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(UnitOfMeasure) || UnitOfMeasure.Length > 20)
            {
                if (!string.IsNullOrEmpty(invoiceNo))
                    invoiceNo = string.Format(" documento {0}", invoiceNo);

                erro = new ValidationError { Description = string.Format("Unidade de medida incorrecta, {0} linha {1}.", invoiceNo, LineNumber), Field = "UnitOfMeasure", TypeofError = GetType(), Value = UnitOfMeasure, UID = Pk, SupUID = SupPk };
                TooltipUnitOfMeasure += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateUnitPrice(string SupPk = "", string invoiceNo = "")
        {
            ValidationError erro = null;
            if (UnitPrice == 0)
            {
                if (!string.IsNullOrEmpty(invoiceNo))
                    invoiceNo = string.Format(" documento {0}", invoiceNo);

                erro = new ValidationError { Description = string.Format("Preço unitário incorrecto, {0} linha {1}.", invoiceNo, LineNumber), Field = "UnitPrice", TypeofError = GetType(), Value = UnitPrice.ToString(), UID = Pk, SupUID = SupPk };
                TooltipUnitPrice += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateTaxPointDate(string SupPk = "", string invoiceNo = "")
        {
            ValidationError erro = null;
            if (TaxPointDate > DateTime.Now)
            {
                if (!string.IsNullOrEmpty(invoiceNo))
                    invoiceNo = string.Format(" documento {0}", invoiceNo);

                erro = new ValidationError { Description = string.Format("Data de envio da mercadoria ou prestação do serviço incorrecta, {0} linha {1}.", invoiceNo, LineNumber), Field = "TaxPointDate", TypeofError = GetType(), Value = TaxPointDate.ToString(), UID = Pk, SupUID = SupPk };
                TooltipTaxPointDate += Environment.NewLine + erro.Description;
            }
            return erro;
        }

        public ValidationError[] ValidateOrderReferences(string SupPk = "", string invoiceNo = "")
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (OrderReferences != null && OrderReferences.Length > 0)
            {
                foreach (var referencia in OrderReferences)
                {
                    if (string.IsNullOrEmpty(referencia.OriginatingON) == false && referencia.OriginatingON.Length > 60)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do número do documento de origem na linha {0} do documento {1} incorrecto.", LineNumber, invoiceNo), Field = "OriginatingON", TypeofError = GetType(), Value = referencia.OriginatingON, UID = Pk, SupUID = SupPk });
                    if (referencia.OrderDate == DateTime.MinValue || referencia.OrderDate > DateTime.Now)
                        listError.Add(new ValidationError { Description = string.Format("Data do documento de origem na linha {0} do documento {1} incorrecto.", LineNumber, invoiceNo), Field = "OrderDate", TypeofError = GetType(), Value = referencia.OrderDate.ToString(), UID = Pk, SupUID = SupPk });
                }
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateReferences(string SupPk = "", string invoiceNo = "")
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (References != null && References.Length > 0)
            {
                foreach (var referencia in References)
                {
                    if (string.IsNullOrEmpty(referencia.Reason) == false && referencia.Reason.Length > 50)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho do motivo da emissão na linha {0} do documento {1} incorrecto.", LineNumber, invoiceNo), Field = "Reason", TypeofError = GetType(), Value = referencia.Reason, UID = Pk, SupUID = SupPk });
                    if (string.IsNullOrEmpty(referencia.Reference) == false && referencia.Reference.Length > 60)
                        listError.Add(new ValidationError { Description = string.Format("Tamanho da referência à fatura ou fatura simplificada na linha {0} do documento {1} incorrecto.", LineNumber, invoiceNo), Field = "Reference", TypeofError = GetType(), Value = referencia.Reference, UID = Pk, SupUID = SupPk });
                }
            }

            return listError.ToArray();
        }
        public ValidationError[] ValidateTax(string SupPk = "", string invoiceNo = "")
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (Tax == null)
                listError.Add(new ValidationError { Description = string.Format("Taxa de imposto na linha {0} do documento {1} inexistente.", LineNumber, invoiceNo), Field = "Tax", TypeofError = GetType(), UID = Pk, SupUID = SupPk });

            if (Tax != null)
            {
                if (Tax.ItemElementName == ItemChoiceType1.TaxAmount && Tax.TaxAmount < 0)
                    listError.Add(new ValidationError { Description = string.Format("Montante do imposto na linha {0} do documento {1} incorrecto.", LineNumber, invoiceNo), Field = "TaxAmount", TypeofError = GetType(), Value = Tax.TaxAmount.ToString(), UID = Pk, SupUID = SupPk });
                if (Tax.ItemElementName == ItemChoiceType1.TaxPercentage && (Tax.TaxPercentage < 0 || Tax.TaxPercentage > 100))
                    listError.Add(new ValidationError { Description = string.Format("Percentagem da taxa do imposto na linha {0} do documento {1} incorrecto.", LineNumber, invoiceNo), Field = "TaxPercentage", TypeofError = GetType(), Value = Tax.TaxPercentage.ToString(), UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(Tax.TaxCode) || Tax.TaxCode.Length > 10)
                    listError.Add(new ValidationError { Description = string.Format("Código da taxa na linha {0} do documento {1} incorrecto.", LineNumber, invoiceNo), Field = "TaxCode", TypeofError = GetType(), Value = Tax.TaxCode, UID = Pk, SupUID = SupPk });
                if (string.IsNullOrEmpty(Tax.TaxCountryRegion) || Tax.TaxCountryRegion.Length > 5)
                    listError.Add(new ValidationError { Description = string.Format("País ou região do imposto na linha {0} do documento {1} incorrecto.", LineNumber, invoiceNo), Field = "TaxCountryRegion", TypeofError = GetType(), Value = Tax.TaxCountryRegion, UID = Pk, SupUID = SupPk });
            }

            return listError.ToArray();
        }
    }

    public partial class Header : BaseData
    {
        public Header()
        {
            TooltipAuditFileVersion = string.Format("1.1 * Texto 10{0}Ficheiro de auditoria informática.{0}A versão a utilizar do esquema XML será a que se encontra disponível no endereço http://www.portaldasfinancas.gov.pt.", Environment.NewLine);
            TooltipCompanyID = string.Format("1.2 * Texto 50{0}Identificação do registo comercial da empresa.{0}Obtém-se pela concatenação da conservatória do registo comercial com o número do registo comercial, separados pelo carácter espaço.{0}Nos casos em que não existe o registo comercial, deve ser indicado o NIF.", Environment.NewLine);
            TooltipTaxRegistrationNumber = string.Format("1.3 * Inteiro 9{0}Número de identificação fiscal da empresa.{0}Preencher com o NIF português sem espaços e sem qualquer prefixo do país.", Environment.NewLine);
            TooltipTaxAccountingBasis = string.Format("1.4 * Texto 1{0}Sistema contabilístico.{0}Deve ser preenchido com:{0}C - Contabilidade;{0}F - Faturação incluindo os documentos de transporte e os de conferência;{0}I - Dados integrados de contabilidade e faturação, incluindo os documentos de transporte e os de conferência;{0}S - Autofaturação;{0}E - Faturação emitida por terceiros, incluindo documentos de transporte e os de conferência;{0}P - Dados parciais de faturação, incluindo os documentos de transporte e os de conferência.", Environment.NewLine);
            TooltipCompanyName = string.Format("1.5 * Texto 100{0}Nome da empresa.{0}Denominação social da empresa ou nome do sujeito passivo.", Environment.NewLine);
            TooltipBusinessName = string.Format("1.6 Texto 60{0}Designação Comercial.{0}Designação comercial do sujeito passivo.", Environment.NewLine);
            TooltipCompanyAddress = string.Format("1.7 *{0}Endereço da empresa.", Environment.NewLine);
            TooltipBuildingNumber = string.Format("1.7.1 Texto 10{0}Número de polícia.", Environment.NewLine);
            TooltipStreetName = string.Format("1.7.2 Texto 90{0}Nome da rua.", Environment.NewLine);
            TooltipAddressDetail = string.Format("1.7.3 * Texto 100{0}Morada detalhada.{0}Deve incluir o nome da rua, número de polícia e andar, se aplicável.", Environment.NewLine);
            TooltipCity = string.Format("1.7.4 * Texto 50{0}Localidade.", Environment.NewLine);
            TooltipPostalCode = string.Format("1.7.5 * Texto 8{0}Código postal.", Environment.NewLine);
            TooltipRegion = string.Format("1.7.6 Texto 50{0}Distrito.", Environment.NewLine);
            TooltipCountry = string.Format("1.7.7 * Texto 2{0}País.{0}Preencher com «PT».", Environment.NewLine);
            TooltipFiscalYear = string.Format("1.8 * Inteiro 4{0}Ano fiscal.{0}Utilizar as regras do Código do IRC, no caso de períodos contabilísticos não coincidentes com o ano civil.{0}(Exemplo: período de tributação de 1-10-2012 a 30-9-2013 corresponde a FiscalYear = 2012).", Environment.NewLine);
            TooltipStartDate = string.Format("1.9 * Data{0}Data do início do período do ficheiro.", Environment.NewLine);
            TooltipEndDate = string.Format("1.10 * Data{0}Data do fim do período do ficheiro.", Environment.NewLine);
            TooltipCurrencyCode = string.Format("1.11 * Texto 3{0}Código de moeda.{0}Preencher com «EUR».", Environment.NewLine);
            TooltipDateCreated = string.Format("1.12 * Data{0}Data da criação.{0}Data de criação do ficheiro XML do SAF-T (PT).", Environment.NewLine);
            TooltipTaxEntity = string.Format("1.13 * Texto 20{0}Identificação do estabelecimento.{0}No caso do ficheiro de faturação, deverá ser especificado a que estabelecimento diz respeito o ficheiro produzido, se aplicável.{0}Caso contrário, deverá ser preenchido com a especificação «Global».{0}No caso do ficheiro de contabilidade ou integrado, este campo deverá ser preenchido com a especificação «Sede».", Environment.NewLine);
            TooltipProductCompanyTaxID = string.Format("1.14 * Texto 20{0}Identificação fiscal da entidade produtora do software.{0}Preencher com o NIF da entidade produtora do software.", Environment.NewLine);
            TooltipSoftwareCertificateNumber = string.Format("1.15 * Texto 20{0}Número do certificado atribuído ao software.{0}Número do certificado atribuído à entidade produtora do software, de acordo com a Portaria n.º 363/2010, de 23 de junho.{0}Se não aplicável, deverá ser preenchido com «0» (zero).", Environment.NewLine);
            TooltipProductID = string.Format("1.16 * Texto 255{0}Nome da aplicação que gera o SAF-T (PT).{0}Deve ser indicado o nome comercial do software e o da empresa produtora no formato «Nome da aplicação/Nome da empresa produtora do software».", Environment.NewLine);
            TooltipProductVersion = string.Format("1.17 * Texto 30{0}Versão da aplicação.{0}Deve ser indicada a versão da aplicação.", Environment.NewLine);
            TooltipHeaderComment = string.Format("1.18 Texto 255{0}Comentários adicionais.", Environment.NewLine);
            TooltipTelephone = string.Format("1.19 Texto 20{0}Telefone.", Environment.NewLine);
            TooltipFax = string.Format("1.20 Texto 20{0}Fax.", Environment.NewLine);
            TooltipEmail = string.Format("1.21 Texto 60{0}Endereço de correio eletrónico da empresa.", Environment.NewLine);
            TooltipWebsite = string.Format("1.22 Texto 60{0}Endereço do sítio Web da empresa.", Environment.NewLine);
        }

        public string TooltipAuditFileVersion { get; set; }
        public string TooltipCompanyID { get; set; }
        public string TooltipTaxRegistrationNumber { get; set; }
        public string TooltipTaxAccountingBasis { get; set; }
        public string TooltipCompanyName { get; set; }
        public string TooltipBusinessName { get; set; }
        public string TooltipCompanyAddress { get; set; }
        public string TooltipBuildingNumber { get; set; }
        public string TooltipStreetName { get; set; }
        public string TooltipAddressDetail { get; set; }
        public string TooltipCity { get; set; }
        public string TooltipPostalCode { get; set; }
        public string TooltipRegion { get; set; }
        public string TooltipCountry { get; set; }
        public string TooltipFiscalYear { get; set; }
        public string TooltipStartDate { get; set; }
        public string TooltipEndDate { get; set; }
        public string TooltipCurrencyCode { get; set; }
        public string TooltipDateCreated { get; set; }
        public string TooltipTaxEntity { get; set; }
        public string TooltipProductCompanyTaxID { get; set; }
        public string TooltipSoftwareCertificateNumber { get; set; }
        public string TooltipProductID { get; set; }
        public string TooltipProductVersion { get; set; }
        public string TooltipHeaderComment { get; set; }
        public string TooltipTelephone { get; set; }
        public string TooltipFax { get; set; }
        public string TooltipEmail { get; set; }
        public string TooltipWebsite { get; set; }

        public ValidationError ValidateTaxRegistrationNumber()
        {
            ValidationError erro = null;
            if (!Validations.CheckTaxRegistrationNumber(TaxRegistrationNumber))
            {
                erro = new ValidationError { Description = "NIF inválido", Field = "TaxRegistrationNumber", TypeofError = GetType(), Value = TaxRegistrationNumber, UID = Pk };
                TooltipTaxRegistrationNumber += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateAuditFileVersion()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(AuditFileVersion) || AuditFileVersion.Length > 10)
            {
                erro = new ValidationError { Description = "Versão do ficheiro SAF-T PT incorrecta.", Field = "AuditFileVersion", TypeofError = GetType(), Value = AuditFileVersion, UID = Pk };
                TooltipAuditFileVersion += Environment.NewLine + erro.Description;
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
                    erro = new ValidationError { Description = "Designação comercial incorrecta.", Field = "BusinessName", TypeofError = GetType(), Value = BusinessName, UID = Pk };
                    TooltipBusinessName += Environment.NewLine + erro.Description;
                }
            }
            return erro;
        }
        public ValidationError ValidateAddressDetail()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(CompanyAddress?.AddressDetail) || CompanyAddress.AddressDetail.Length > 100)
            {
                erro = new ValidationError { Description = "Morada detalhada incorrecta.", Field = "AddressDetail", TypeofError = GetType(), Value = CompanyAddress?.AddressDetail, UID = Pk };
                TooltipAddressDetail += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateBuildingNumber()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(CompanyAddress?.BuildingNumber) && CompanyAddress.BuildingNumber.Length > 10)
            {
                erro = new ValidationError { Description = "Número polícia incorrecto.", Field = "BuildingNumber", TypeofError = GetType(), Value = CompanyAddress?.BuildingNumber, UID = Pk };
                TooltipBuildingNumber += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateCity()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(CompanyAddress?.City) || CompanyAddress.City.Length > 50)
            {
                erro = new ValidationError { Description = "Localidade incorrecta.", Field = "City", TypeofError = GetType(), Value = CompanyAddress?.City, UID = Pk };
                TooltipCity += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateCountry()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(CompanyAddress?.Country) || CompanyAddress.Country != "PT")
            {
                erro = new ValidationError { Description = "Localidade incorrecta.", Field = "Country", TypeofError = GetType(), Value = CompanyAddress?.Country, UID = Pk };
                TooltipCountry += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidatePostalCode()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(CompanyAddress?.PostalCode) || CompanyAddress.PostalCode.Length > 50)
            {
                erro = new ValidationError { Description = "Código postal incorrecto.", Field = "PostalCode", TypeofError = GetType(), Value = CompanyAddress?.PostalCode, UID = Pk };
                TooltipPostalCode += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateRegion()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(CompanyAddress?.Region) && CompanyAddress.Region.Length > 50)
            {
                erro = new ValidationError { Description = "Distrito incorrecto.", Field = "Region", TypeofError = GetType(), Value = CompanyAddress?.Region, UID = Pk };
                TooltipRegion += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateStreetName()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(CompanyAddress?.StreetName) && CompanyAddress.StreetName.Length > 90)
            {
                erro = new ValidationError { Description = "Nome da rua incorrecto.", Field = "StreetName", TypeofError = GetType(), Value = CompanyAddress?.StreetName, UID = Pk };
                TooltipStreetName += Environment.NewLine + erro.Description;
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
                    erro = new ValidationError { Description = "Registo comercial incorrecto.", Field = "CompanyID", TypeofError = GetType(), Value = CompanyID, UID = Pk };
                    TooltipCompanyID += Environment.NewLine + erro.Description;
                }
            }
            return erro;
        }
        public ValidationError ValidateCompanyName()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
            {
                erro = new ValidationError { Description = "Nome empresa incorrecto.", Field = "CompanyName", TypeofError = GetType(), Value = CompanyName, UID = Pk };
                TooltipCompanyName += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateCurrencyCode()
        {
            ValidationError erro = null;
            if (CurrencyCode == null || CurrencyCode.ToString() != "EUR")
            {
                erro = new ValidationError { Description = "Código moeda incorrecto.", Field = "CurrencyCode", TypeofError = GetType(), Value = string.Format("{0}", CurrencyCode ?? "null"), UID = Pk };
                TooltipCurrencyCode += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateDateCreated()
        {
            ValidationError erro = null;
            if (DateCreated > DateTime.Now)
            {
                erro = new ValidationError { Description = "Data de criação do ficheiro incorrecta.", Field = "DateCreated", TypeofError = GetType(), Value = DateCreated.ToString(), UID = Pk };
                TooltipDateCreated += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateEmail()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Email) && (Email.Length > 60 || !Regex.IsMatch(Email, @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.IgnoreCase)))
            {
                erro = new ValidationError { Description = "Email incorrecto.", Field = "Email", TypeofError = GetType(), Value = Email, UID = Pk };
                TooltipEmail += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateEndDate()
        {
            ValidationError erro = null;
            if (EndDate == DateTime.MinValue)
            {
                erro = new ValidationError { Description = "Data do fim do periodo incorrecta.", Field = "EndDate", TypeofError = GetType(), Value = EndDate.ToString(), UID = Pk };
                TooltipEndDate += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateFax()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
            {
                erro = new ValidationError { Description = "Fax incorrecto.", Field = "Fax", TypeofError = GetType(), Value = Fax, UID = Pk };
                TooltipFax += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateFiscalYear()
        {
            ValidationError erro = null;
            int.TryParse(FiscalYear, out int ano);
            if (string.IsNullOrEmpty(FiscalYear) || FiscalYear.Length > 4 || ano == -1)
            {
                erro = new ValidationError { Description = "Ano fiscal incorrecto.", Field = "FiscalYear", TypeofError = GetType(), Value = FiscalYear, UID = Pk };
                TooltipFiscalYear += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateHeaderComment()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(HeaderComment) && HeaderComment.Length > 255)
            {
                erro = new ValidationError { Description = "Comentário demasiado longo.", Field = "HeaderComment", TypeofError = GetType(), Value = HeaderComment, UID = Pk };
                TooltipHeaderComment += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateProductCompanyTaxID()
        {
            ValidationError erro = null;
            if (!Validations.CheckTaxRegistrationNumber(ProductCompanyTaxID))
            {
                erro = new ValidationError { Description = "NIF da empresa produtora de saftware inválido.", Field = "ProductCompanyTaxID", TypeofError = GetType(), Value = ProductCompanyTaxID, UID = Pk };
                TooltipProductCompanyTaxID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateProductID()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductID) || ProductID.Length > 255 || !ProductID.Contains('/'))
            {
                erro = new ValidationError { Description = "Nome da aplicação incorrecto.", Field = "ProductID", TypeofError = GetType(), Value = ProductID, UID = Pk };
                TooltipProductID += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateProductVersion()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductVersion) || ProductVersion.Length > 30)
            {
                erro = new ValidationError { Description = "Versão da aplicação incorrecta.", Field = "ProductVersion", TypeofError = GetType(), Value = ProductVersion, UID = Pk };
                TooltipProductVersion += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateSoftwareCertificateNumber()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(SoftwareCertificateNumber) || SoftwareCertificateNumber.Length > 20)
            {
                erro = new ValidationError { Description = "Número de certificação incorrecto.", Field = "SoftwareCertificateNumber", TypeofError = GetType(), Value = SoftwareCertificateNumber, UID = Pk };
                TooltipSoftwareCertificateNumber += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateTaxAccountingBasis()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(TaxAccountingBasis.ToString()) || TaxAccountingBasis.ToString().Length > 1)
            {
                erro = new ValidationError { Description = "Sistema contabilístico incorrecto.", Field = "TaxAccountingBasis", TypeofError = GetType(), Value = TaxAccountingBasis.ToString(), UID = Pk };
                TooltipTaxAccountingBasis += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateTaxEntity()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(TaxEntity) || TaxEntity.Length > 20)
            {
                erro = new ValidationError { Description = "Identificação do estabelecimento incorrecta.", Field = "TaxEntity", TypeofError = GetType(), Value = TaxEntity, UID = Pk };
                TooltipTaxEntity += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateTelephone()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
            {
                erro = new ValidationError { Description = "Identificação do estabelecimento incorrecta.", Field = "Telephone", TypeofError = GetType(), Value = Telephone, UID = Pk };
                TooltipTelephone += Environment.NewLine + erro.Description;
            }
            return erro;
        }
        public ValidationError ValidateWebsite()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
            {
                erro = new ValidationError { Description = "Website incorrecto.", Field = "Website", TypeofError = GetType(), Value = Website, UID = Pk };
                TooltipWebsite += Environment.NewLine + erro.Description;
            }
            return erro;
        }
    }

    public partial class Product : BaseData
    {
        public string Prices { get; set; }
        public string Taxes { get; set; }

        public Product()
        {
            TooltipProductType = string.Format("2.4.1 * Texto 1{0}Indicador de produto ou serviço.{0}Deve ser preenchido com:{0}«P» - Produtos;{0}«S» - Serviços;{0}«O» - Outros (Exemplo: portes debitados);{0}«I» - Impostos, taxas e encargos parafiscais exceto IVA e IS que deverão ser refletidos na tabela de impostos (TaxTable).", Environment.NewLine);
            TooltipProductCode = string.Format("2.4.2 * Texto 30{0}Identificador do produto ou serviço.{0}Código único do produto na lista de produtos.", Environment.NewLine);
            TooltipProductGroup = string.Format("2.4.3 Texto 50{0}Família do produto ou serviço.", Environment.NewLine);
            TooltipProductDescription = string.Format("2.4.4 * Texto 200{0}Descrição do produto ou serviço.", Environment.NewLine);
            TooltipProductNumberCode = string.Format("2.4.5 * Texto 50{0}Código do produto.{0}Deve ser utilizado o código EAN (código de barras) do produto.{0}Quando este não existir, preencher com o identificador do produto ou serviço (ProductCode).", Environment.NewLine);
        }
        public string TooltipProductType { get; set; }
        public string TooltipProductCode { get; set; }
        public string TooltipProductGroup { get; set; }
        public string TooltipProductDescription { get; set; }
        public string TooltipProductNumberCode { get; set; }

        public ValidationError ValidateProductCode()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductCode) || ProductCode.Length > 30)
            {
                erro = new ValidationError { Description = "Código do produto inválido", Field = "ProductCode", TypeofError = GetType(), Value = ProductCode, UID = Pk };
                TooltipProductCode += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateProductGroup()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(ProductGroup) && ProductGroup.Length > 50)
            {
                erro = new ValidationError { Description = "Família do produto ou serviço inválida", Field = "ProductGroup", TypeofError = GetType(), Value = ProductGroup, UID = Pk };
                TooltipProductGroup += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateProductDescription()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductDescription) || ProductDescription.Length > 200)
            {
                erro = new ValidationError { Description = "Descrição do produto ou serviço inválida", Field = "ProductDescription", TypeofError = GetType(), Value = ProductDescription, UID = Pk };
                TooltipProductDescription += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateProductNumberCode()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(ProductNumberCode) || ProductNumberCode.Length > 50)
            {
                erro = new ValidationError { Description = "Família do produto ou serviço inválida", Field = "ProductNumberCode", TypeofError = GetType(), Value = ProductNumberCode, UID = Pk };
                TooltipProductNumberCode += Environment.NewLine + erro.Description;
            }

            return erro;
        }
    }

    public partial class Customer : BaseData
    {
        public Customer()
        {
            TooltipCustomerID = string.Format("2.2.1 * Texto 30{0}Identificador único do cliente.{0}Na lista de clientes não pode existir mais do que um registo com o mesmo CustomerID.{0}Para o caso de consumidores finais, deve ser criado um cliente genérico com a designação «Consumidor final».", Environment.NewLine);
            TooltipAccountID = string.Format("2.2.2 * Texto 30{0}Código da conta.{0}Deve ser indicada a respetiva conta-corrente do cliente no plano de contas da contabilidade, caso esteja definida.{0}Caso contrário deverá ser preenchido com a designação «Desconhecido».", Environment.NewLine);
            TooltipCustomerTaxID = string.Format(" 2.2.3 * Texto 20{0}Número de identificação fiscal do cliente.{0}Deve ser indicado sem o prefixo do país.{0}O cliente genérico, correspondente ao designado «Consumidor final», deverá ser identificado com o NIF «999999990».", Environment.NewLine);
            TooltipCompanyName = string.Format("2.2.4 * Texto 100{0}Nome da empresa.{0}O cliente genérico deverá ser identificado com a designação «Consumidor final».{0}No caso do setor bancário, para as atividades não sujeitas a IVA, deverá ser preenchido com a designação «Desconhecido».", Environment.NewLine);
            TooltipContact = string.Format("2.2.5 Texto 50{0}Nome do contacto na empresa.", Environment.NewLine);
            TooltipBillingAddress = string.Format("2.2.6 * N/A{0}Morada de faturação.{0}Corresponde à morada da sede ou do estabelecimento estável em território nacional.", Environment.NewLine);
            TooltipBillingAddressBuildingNumber = string.Format("2.2.6.1 * Texto 10{0}Número de polícia.", Environment.NewLine);
            TooltipBillingAddressStreetName = string.Format("2.2.6.2 Texto 90{0}Nome da rua.", Environment.NewLine);
            TooltipBillingAddressAddressDetail = string.Format("2.2.6.3 * Texto 100{0}Morada detalhada.{0}Deve incluir o nome da rua, número de polícia e andar, se aplicável.{0}Deverá ser preenchido com a designação «Desconhecido», nas seguintes situações:{0}• Sistemas não integrados, se a informação não for conhecida;{0}• Operações realizadas com «Consumidor final»;{0}• No caso do setor bancário, para as atividades não sujeitas a IVA.", Environment.NewLine);
            TooltipBillingAddressCity = string.Format("2.2.6.4 * Texto 50{0}Deverá ser preenchido com a designação «Desconhecido», nas seguintes situações:{0}• Sistemas não integrados, se a informação não for conhecida;", Environment.NewLine);
            TooltipBillingAddressPostalCode = string.Format("2.2.6.5 * Texto 20{0}Código postal.{0}Deverá ser preenchido com a designação «Desconhecido» nas seguintes situações:{0}• Sistemas não integrados, se a informação não for conhecida;{0}• Operações realizadas com «Consumidor final»; e{0}• No caso do setor bancário, para as atividades não sujeitas a IVA.", Environment.NewLine);
            TooltipBillingAddressRegion = string.Format("2.2.6.6 Texto 50{0}Distrito.", Environment.NewLine);
            TooltipBillingAddressCountry = string.Format("2.2.6.7 * Texto 12{0}País.{0}Sendo conhecido, deve ser preenchido de acordo com a norma ISO 3166-1-alpha-2.{0}Deverá ser preenchido com a designação «Desconhecido», nas seguintes situações:{0}• Sistemas não integrados, se a informação não for conhecida;{0}• Operações realizadas com «Consumidor final;{0}• No caso do setor bancário, para as atividades não sujeitas a IVA.", Environment.NewLine);
            TooltipShipToAddress = "2.2.7 Morada de expedição.";
            TooltipShipToAddressBuildingNumber = string.Format("2.2.7.1 Texto 10{0}Número de polícia.", Environment.NewLine);
            TooltipShipToAddressStreetName = string.Format("2.2.7.2 Texto 90{0}Nome da rua.", Environment.NewLine);
            TooltipShipToAddressAddressDetail = string.Format("2.2.7.3 * Texto 100{0}Morada detalhada.{0}Deve incluir o nome da rua, número de polícia e andar, se aplicável.{0}Deverá ser preenchido com a designação «Desconhecido», nas seguintes situações:{0}• Sistemas não integrados, se a informação não for conhecida;{0}• Operações realizadas com «Consumidor final»; e{0}• No caso do setor bancário, para as atividades não sujeitas a IVA.", Environment.NewLine);
            TooltipShipToAddressCity = string.Format("2.2.7.4 * Texto 50{0}Localidade.{0}Deverá ser preenchido com a designação «Desconhecido», nas seguintes situações:{0}• Sistemas não integrados, se a informação não for conhecida;{0}• Operações realizadas com «Consumidor final»; e{0}• No caso do setor bancário, para as atividades não sujeitas a IVA.", Environment.NewLine);
            TooltipShipToAddressPostalCode = string.Format("2.2.7.5 * Texto 20{0}Código postal.{0}Deverá ser preenchido com a designação «Desconhecido», nas seguintes situações:{0}• Sistemas não integrados, se a informação não for conhecida;{0}• Operações realizadas com «Consumidor final»; e{0}• No caso do setor bancário, para as atividades não sujeitas a IVA.", Environment.NewLine);
            TooltipShipToAddressRegion = string.Format("2.2.7.6 Texto 50{0}Distrito.", Environment.NewLine);
            TooltipShipToAddressCountry = string.Format("2.2.7.7 * Texto 12{0}País.{0}Deve ser preenchido de acordo com a norma ISO 3166-1-alpha-2.{0}Deverá ser preenchido com a designação «Desconhecido», nas seguintes situações:{0}• Sistemas não integrados, se a informação não for conhecida;{0}• Operações realizadas com «Consumidor final»; e{0}• No caso do setor bancário, para as atividades não sujeitas a IVA.", Environment.NewLine);
            TooltipTelephone = string.Format("2.2.8 Texto 20{0}Telefone.", Environment.NewLine);
            TooltipFax = string.Format("2.2.9 Texto 20{0}Fax.", Environment.NewLine);
            TooltipEmail = string.Format("2.2.10 Texto 60{0}Endereço de correio eletrónico da empresa.", Environment.NewLine);
            TooltipWebsite = string.Format("2.2.11 Texto 60{0}Endereço do sítio web da empresa.", Environment.NewLine);
            TooltipSelfBillingIndicator = string.Format("2.2.12 * Inteiro 1{0}Indicador de autofaturação.{0}Indicador da existência de acordo de autofaturação entre o cliente e o fornecedor.{0}Deve ser preenchido com «1» se houver acordo e com «0» (zero) no caso contrário.", Environment.NewLine);
        }

        public string TooltipCustomerID { get; set; }
        public string TooltipAccountID { get; set; }
        public string TooltipCustomerTaxID { get; set; }
        public string TooltipCompanyName { get; set; }
        public string TooltipContact { get; set; }
        public string TooltipBillingAddress { get; set; }
        public string TooltipBillingAddressBuildingNumber { get; set; }
        public string TooltipBillingAddressStreetName { get; set; }
        public string TooltipBillingAddressAddressDetail { get; set; }
        public string TooltipBillingAddressCity { get; set; }
        public string TooltipBillingAddressPostalCode { get; set; }
        public string TooltipBillingAddressRegion { get; set; }
        public string TooltipBillingAddressCountry { get; set; }
        public string TooltipShipToAddress { get; set; }
        public string TooltipShipToAddressBuildingNumber { get; set; }
        public string TooltipShipToAddressStreetName { get; set; }
        public string TooltipShipToAddressAddressDetail { get; set; }
        public string TooltipShipToAddressCity { get; set; }
        public string TooltipShipToAddressPostalCode { get; set; }
        public string TooltipShipToAddressRegion { get; set; }
        public string TooltipShipToAddressCountry { get; set; }
        public string TooltipTelephone { get; set; }
        public string TooltipFax { get; set; }
        public string TooltipEmail { get; set; }
        public string TooltipWebsite { get; set; }
        public string TooltipSelfBillingIndicator { get; set; }

        public ValidationError ValidateCustomerID()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(CustomerID) || CustomerID.Length > 30)
            {
                erro = new ValidationError { Description = "Identificador único do cliente inválido", Field = "CustomerID", TypeofError = GetType(), Value = CustomerID, UID = Pk };
                TooltipCustomerID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateAccountID()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(AccountID) || AccountID.Length > 30)
            {
                erro = new ValidationError { Description = "Código da conta inválido", Field = "AccountID", TypeofError = GetType(), Value = AccountID, UID = Pk };
                TooltipAccountID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateCustomerTaxID()
        {
            ValidationError erro = null;
            if (BillingAddress?.Country == "PT" && !Validations.CheckTaxRegistrationNumber(CustomerTaxID))
            {
                erro = new ValidationError { Description = "Número de identificação fiscal inválido", Field = "CustomerTaxID", TypeofError = GetType(), Value = CustomerTaxID, UID = Pk };
                TooltipCustomerTaxID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateCompanyName()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
            {
                erro = new ValidationError { Description = "Nome da empresa inválido", Field = "CompanyName", TypeofError = GetType(), Value = CompanyName, UID = Pk };
                TooltipCompanyName += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateContact()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Contact) && Contact.Length > 50)
            {
                erro = new ValidationError { Description = "Nome do contacto na empresa inválido.", Field = "Contact", TypeofError = GetType(), Value = Contact, UID = Pk };
                TooltipContact += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateTelephone()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
            {
                erro = new ValidationError { Description = "Telefone inválido", Field = "Telephone", TypeofError = GetType(), Value = Telephone, UID = Pk };
                TooltipTelephone += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateFax()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
            {
                erro = new ValidationError { Description = "Fax inválido", Field = "Fax", TypeofError = GetType(), Value = Fax, UID = Pk };
                TooltipFax += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateEmail()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Email) && Email.Length > 60)
            {
                erro = new ValidationError { Description = "Email inválido", Field = "Email", TypeofError = GetType(), Value = Email, UID = Pk };
                TooltipEmail += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateWebsite()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
            {
                erro = new ValidationError { Description = "Website inválido", Field = "Website", TypeofError = GetType(), Value = Website, UID = Pk };
                TooltipWebsite += Environment.NewLine + erro.Description;
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
                erro = new ValidationError { Description = "Nº de conta inválido", Field = "SelfBillingIndicator", TypeofError = GetType(), Value = SelfBillingIndicator, UID = Pk };
                TooltipSelfBillingIndicator += Environment.NewLine + erro.Description;
            }

            return erro;
        }

        public ValidationError[] ValidateBillingAddress()
        {
            List<ValidationError> listErro = new List<ValidationError>();

            if (BillingAddress == null)
                listErro.Add(new ValidationError { Description = "Morada de faturação inexistente", Field = "BillingAddress", TypeofError = GetType(), UID = Pk });

            if (BillingAddress != null)
            {
                if (string.IsNullOrEmpty(BillingAddress.AddressDetail) || BillingAddress.AddressDetail.Length > 100)
                    listErro.Add(new ValidationError { Description = "Tamanho da morada detalhada.", Field = "AddressDetail", TypeofError = GetType(), Value = BillingAddress.AddressDetail, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.BuildingNumber) == false && BillingAddress.BuildingNumber.Length > 10)
                    listErro.Add(new ValidationError { Description = "Tamanho do número de polícia incorrecto.", Field = "BuildingNumber", TypeofError = GetType(), Value = BillingAddress.BuildingNumber, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.City) || BillingAddress.City.Length > 50)
                    listErro.Add(new ValidationError { Description = "Tamanho da localidade incorrecto.", Field = "City", TypeofError = GetType(), Value = BillingAddress.City, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.PostalCode) || BillingAddress.PostalCode.Length > 20)
                    listErro.Add(new ValidationError { Description = "Tamanho do código postal incorrecto.", Field = "PostalCode", TypeofError = GetType(), Value = BillingAddress.PostalCode, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.Region) == false && BillingAddress.Region.Length > 50)
                    listErro.Add(new ValidationError { Description = "Tamanho do distrito incorrecto.", Field = "Region", TypeofError = GetType(), Value = BillingAddress.Region, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.StreetName) == false && BillingAddress.StreetName.Length > 90)
                    listErro.Add(new ValidationError { Description = "Tamanho do nome da rua incorrecto.", Field = "StreetName", TypeofError = GetType(), Value = BillingAddress.StreetName, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.Country) || (BillingAddress.Country.Length != 2 && BillingAddress.Country != "Desconhecido"))
                    listErro.Add(new ValidationError { Description = "Tamanho do País incorrecto.", Field = "Country", TypeofError = GetType(), Value = BillingAddress.Country, UID = Pk });
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
                    if (string.IsNullOrEmpty(morada.AddressDetail) || morada.AddressDetail.Length > 100)
                        listErro.Add(new ValidationError { Description = "Tamanho da morada detalhada.", Field = "AddressDetail", TypeofError = GetType(), Value = morada.AddressDetail, UID = Pk });
                    if (string.IsNullOrEmpty(morada.BuildingNumber) == false && morada.BuildingNumber.Length > 10)
                        listErro.Add(new ValidationError { Description = "Tamanho do número de polícia incorrecto.", Field = "BuildingNumber", TypeofError = GetType(), Value = morada.BuildingNumber, UID = Pk });
                    if (string.IsNullOrEmpty(morada.City) || morada.City.Length > 50)
                        listErro.Add(new ValidationError { Description = "Tamanho da localidade incorrecto.", Field = "City", TypeofError = GetType(), Value = morada.City, UID = Pk });
                    if (string.IsNullOrEmpty(morada.Country) || morada.Country.Length != 2 || morada.Country != "Desconhecido")
                        listErro.Add(new ValidationError { Description = "Tamanho do País incorrecto.", Field = "Country", TypeofError = GetType(), Value = morada.Country, UID = Pk });
                    if (string.IsNullOrEmpty(morada.PostalCode) || morada.PostalCode.Length > 20)
                        listErro.Add(new ValidationError { Description = "Tamanho do código postal incorrecto.", Field = "PostalCode", TypeofError = GetType(), Value = morada.PostalCode, UID = Pk });
                    if (string.IsNullOrEmpty(morada.Region) == false && morada.Region.Length > 50)
                        listErro.Add(new ValidationError { Description = "Tamanho do distrito incorrecto.", Field = "Region", TypeofError = GetType(), Value = morada.Region, UID = Pk });
                    if (string.IsNullOrEmpty(morada.StreetName) == false && morada.StreetName.Length > 90)
                        listErro.Add(new ValidationError { Description = "Tamanho do nome da rua incorrecto.", Field = "StreetName", TypeofError = GetType(), Value = morada.StreetName, UID = Pk });
                }
            }

            return listErro.ToArray();
        }
    }

    public partial class TaxTableEntry : BaseData
    {
        public TaxTableEntry()
        {
            TooltipTaxTableEntry = string.Format("2.5.1 * Registo na tabela de impostos.", Environment.NewLine);
            TooltipTaxType = string.Format("2.5.1.1 * Texto 3{0}Código do tipo de imposto.{0}Neste campo deve ser indicado o tipo de imposto.{0}Deve ser preenchido com:{0}«IVA» - Imposto sobre o valor acrescentado;{0}«IS» - Imposto do selo.", Environment.NewLine);
            TooltipTaxCountryRegion = string.Format("2.5.1.2 * Texto 5{0}País ou região do imposto.{0}Deve ser preenchido de acordo com a norma ISO 3166-1-alpha 2.{0}No caso das Regiões Autónomas da Madeira e Açores deve ser preenchido com: «PT-AC» - Espaço fiscal da Região Autónoma dos Açores; e «PT-MA» - Espaço fiscal da Região Autónoma da Madeira.", Environment.NewLine);
            TooltipTaxCode = string.Format("2.5.1.3 * Texto 10{0}Código do imposto.{0}No caso do código do tipo de imposto (TaxType) = IVA, deve ser preenchido com:{0}«RED» - Taxa reduzida;{0}«INT» - Taxa intermédia;{0}«NOR» - Taxa normal;{0}«ISE» - Isenta;{0}«OUT» - Outros, aplicável para os regimes especiais de IVA.{0}No caso do código do tipo de imposto (TaxType) = IS, deve ser preenchido com o código da verba respetiva.", Environment.NewLine);
            TooltipDescription = string.Format("2.5.1.4 * Texto 255{0}Descrição do imposto.{0}No caso do imposto do selo deve ser preenchido com a descrição da verba respetiva.", Environment.NewLine);
            TooltipTaxExpirationDate = string.Format("2.5.1.5 Data{0}Data de fim de vigência.{0}Última data legal de aplicação da taxa de imposto, no caso de alteração da mesma, na vigência do período de tributação.", Environment.NewLine);
            TooltipTaxPercentage = string.Format("2.5.1.6 ** Decimal{0}Percentagem da taxa do imposto.{0}O preenchimento é obrigatório, no caso de se tratar de uma percentagem do imposto.", Environment.NewLine);
            TooltipTaxAmount = string.Format("2.5.1.7 ** Monetário{0}Montante do imposto.{0}O preenchimento é obrigatório, no caso de se tratar de uma verba fixa de imposto do selo.", Environment.NewLine);
        }

        public string TooltipTaxTableEntry { get; set; }
        public string TooltipTaxType { get; set; }
        public string TooltipTaxCountryRegion { get; set; }
        public string TooltipTaxCode { get; set; }
        public string TooltipDescription { get; set; }
        public string TooltipTaxExpirationDate { get; set; }
        public string TooltipTaxPercentage { get; set; }
        public string TooltipTaxAmount { get; set; }

        public ValidationError ValidateTaxCountryRegion()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(TaxCountryRegion) || TaxCountryRegion.Length > 5)
            {
                erro = new ValidationError { Description = "País ou região do imposto inválido", Field = "TaxCountryRegion", TypeofError = GetType(), Value = TaxCountryRegion, UID = Pk };
                TooltipTaxCountryRegion += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateTaxCode()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(TaxCode) || TaxCode.Length > 10)
            {
                erro = new ValidationError { Description = "Código do imposto inválido", Field = "TaxCode", TypeofError = GetType(), Value = TaxCode, UID = Pk };
                TooltipTaxCode += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateDescription()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(Description) || Description.Length > 255)
            {
                erro = new ValidationError { Description = "Identificador único do cliente inválido", Field = "Description", TypeofError = GetType(), Value = Description, UID = Pk };
                TooltipDescription += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        //public Error ValidateTaxExpirationDate()
        //{
        //    Error erro = null;
        //    if (string.IsNullOrEmpty(TaxExpirationDate) || TaxExpirationDate.Length > 30)
        //    {
        //        erro = new Error { Description = "Data de fim de vigência inválida", Field = "TaxExpirationDate", TypeofError = GetType(), ID = TaxExpirationDate, UID = Pk };
        //        if (appendError)
        //            Tooltip.TaxExpirationDate = Tooltip.TaxExpirationDate.FormatTooltipWithError(erro.Description);
        //    }

        //    return erro;
        //}
        //public Error ValidateTaxPercentage()
        //{
        //    Error erro = null;
        //    if (string.IsNullOrEmpty(TaxPercentage) || TaxPercentage.Length > 30)
        //    {
        //        erro = new Error { Description = "Percentagem da taxa do imposto inválida.", Field = "TaxPercentage", TypeofError = GetType(), ID = TaxPercentage, UID = Pk };
        //        if (appendError)
        //            Tooltip.TaxPercentage = Tooltip.TaxPercentage.FormatTooltipWithError(erro.Description);
        //    }

        //    return erro;
        //}
        //public Error ValidateTaxAmount()
        //{
        //    Error erro = null;
        //    if (string.IsNullOrEmpty(TaxAmount) || TaxAmount.Length > 30)
        //    {
        //        erro = new Error { Description = "Identificador único do cliente inválido", Field = "TaxAmount", TypeofError = GetType(), ID = TaxAmount, UID = Pk };
        //        if (appendError)
        //            Tooltip.TaxAmount = Tooltip.TaxAmount.FormatTooltipWithError(erro.Description);
        //    }

        //    return erro;
        //}
    }

    public partial class Supplier : BaseData
    {
        public Supplier()
        {
            TooltipSupplierID = string.Format("2.3.1 * Texto 30{0}Identificador único do fornecedor.{0}Na lista de fornecedores não pode existir mais do que um registo com o mesmo SupplierID.", Environment.NewLine);
            TooltipAccountID = string.Format("2.3.2 * Texto 30{0}Código da conta.{0}Deve ser indicada a respetiva conta -corrente do fornecedor no plano de contas da contabilidade, caso esteja definida.{0}Caso contrário deverá ser preenchido com a designação «Desconhecido».", Environment.NewLine);
            TooltipSupplierTaxID = string.Format("2.3.3 * Texto 20{0}Número de identificação fiscal do fornecedor.{0}Deve ser indicado sem o prefixo do país.", Environment.NewLine);
            TooltipCompanyName = string.Format("2.3.4 * Texto 100{0}Nome da empresa.", Environment.NewLine);
            TooltipContact = string.Format("2.3.5 Texto 50{0}Nome do contacto na empresa (Contact).", Environment.NewLine);
            TooltipBillingAddress = string.Format("2.3.6 * Morada de faturação.{0}Corresponde à morada da sede ou do estabelecimento estável em território nacional.", Environment.NewLine);
            TooltipBillingAddressBuildingNumber = string.Format("2.3.6.1 Texto 10{0}Número de polícia.", Environment.NewLine);
            TooltipBillingAddressStreetName = string.Format("2.3.6.2 Texto 90{0}Nome da rua.", Environment.NewLine);
            TooltipBillingAddressAddressDetail = string.Format("2.3.6.3 * Texto 100{0}Morada detalhada.{0}Deve incluir o nome da rua, número de polícia e andar, se aplicável.{0}", Environment.NewLine);
            TooltipBillingAddressCity = string.Format("2.3.6.4 * Texto 50{0}Localidade.", Environment.NewLine);
            TooltipBillingAddressPostalCode = string.Format("2.3.6.5 * Texto 20{0}Código postal.", Environment.NewLine);
            TooltipBillingAddressRegion = string.Format("2.3.6.6 Texto 50{0}Distrito.", Environment.NewLine);
            TooltipBillingAddressCountry = string.Format("2.3.6.7 * Texto 2{0}País.{0}Deve ser preenchido de acordo com a norma ISO 3166-1-alpha-2.", Environment.NewLine);
            TooltipShipFromAddress = string.Format("2.3.7 Morada da expedição.", Environment.NewLine);
            TooltipShipFromAddressBuildingNumber = string.Format("2.3.7.1 Texto 10{0}Número de polícia.", Environment.NewLine);
            TooltipShipFromAddressStreetName = string.Format("2.3.7.2 Texto 90{0}Nome da rua.", Environment.NewLine);
            TooltipShipFromAddressAddressDetail = string.Format("2.3.7.3 * Texto 100{0}Morada detalhada.{0}Deve incluir o nome da rua, número de polícia e andar, se aplicável.", Environment.NewLine);
            TooltipShipFromAddressCity = string.Format("2.3.7.4 * Texto 50{0}Localidade.", Environment.NewLine);
            TooltipShipFromAddressPostalCode = string.Format("2.3.7.5 * Texto 20{0}Código postal.", Environment.NewLine);
            TooltipShipFromAddressRegion = string.Format("2.3.7.6 Texto 50{0}Distrito.", Environment.NewLine);
            TooltipShipFromAddressCountry = string.Format("2.3.7.7 * Texto 2{0}País.{0}Deve ser preenchido de acordo com a norma ISO 3166-1-alpha-2.", Environment.NewLine);
            TooltipTelephone = string.Format("2.3.8 Texto 20{0}Telefone.", Environment.NewLine);
            TooltipFax = string.Format("2.3.9 Texto 20{0}Fax.", Environment.NewLine);
            TooltipEmail = string.Format("2.3.10 Texto 60{0}Endereço de correio eletrónico da empresa.", Environment.NewLine);
            TooltipWebsite = string.Format("2.3.11 Texto 60{0}Endereço do sítio web da empresa.", Environment.NewLine);
            TooltipSelfBillingIndicator = string.Format("2.3.12 * Inteiro 1{0}Indicador de autofaturação.{0}Indicador da existência de acordo de autofaturação entre o cliente e o fornecedor.{0}Deve ser preenchido com «1» se houver acordo e com «0» (zero) no caso contrário.", Environment.NewLine);
        }
        public string TooltipSupplierID { get; set; }
        public string TooltipAccountID { get; set; }
        public string TooltipSupplierTaxID { get; set; }
        public string TooltipCompanyName { get; set; }
        public string TooltipContact { get; set; }
        public string TooltipBillingAddress { get; set; }
        public string TooltipBillingAddressBuildingNumber { get; set; }
        public string TooltipBillingAddressStreetName { get; set; }
        public string TooltipBillingAddressAddressDetail { get; set; }
        public string TooltipBillingAddressCity { get; set; }
        public string TooltipBillingAddressPostalCode { get; set; }
        public string TooltipBillingAddressRegion { get; set; }
        public string TooltipBillingAddressCountry { get; set; }
        public string TooltipShipFromAddress { get; set; }
        public string TooltipShipFromAddressBuildingNumber { get; set; }
        public string TooltipShipFromAddressStreetName { get; set; }
        public string TooltipShipFromAddressAddressDetail { get; set; }
        public string TooltipShipFromAddressCity { get; set; }
        public string TooltipShipFromAddressPostalCode { get; set; }
        public string TooltipShipFromAddressRegion { get; set; }
        public string TooltipShipFromAddressCountry { get; set; }
        public string TooltipTelephone { get; set; }
        public string TooltipFax { get; set; }
        public string TooltipEmail { get; set; }
        public string TooltipWebsite { get; set; }
        public string TooltipSelfBillingIndicator { get; set; }

        public ValidationError ValidateCustomerID()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(SupplierID) || SupplierID.Length > 30)
            {
                erro = new ValidationError { Description = "Identificador único do fornecedor inválido", Field = "SupplierID", TypeofError = GetType(), Value = SupplierID, UID = Pk };
                TooltipSupplierID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateAccountID()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(AccountID) || AccountID.Length > 30)
            {
                erro = new ValidationError { Description = "Código da conta inválido", Field = "AccountID", TypeofError = GetType(), Value = AccountID, UID = Pk };
                TooltipAccountID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateSupplierTaxID()
        {
            ValidationError erro = null;
            if (BillingAddress?.Country == "PT" && !Validations.CheckTaxRegistrationNumber(SupplierTaxID))
            {
                erro = new ValidationError { Description = "Número de identificação fiscal inválido", Field = "SupplierTaxID", TypeofError = GetType(), Value = SupplierTaxID, UID = Pk };
                TooltipSupplierTaxID += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateCompanyName()
        {
            ValidationError erro = null;
            if (string.IsNullOrEmpty(CompanyName) || CompanyName.Length > 100)
            {
                erro = new ValidationError { Description = "Nome da empresa inválido", Field = "CompanyName", TypeofError = GetType(), Value = CompanyName, UID = Pk };
                TooltipCompanyName += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateContact()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Contact) && Contact.Length > 50)
            {
                erro = new ValidationError { Description = "Nome do contacto na empresa inválido.", Field = "Contact", TypeofError = GetType(), Value = Contact, UID = Pk };
                TooltipContact += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateTelephone()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Telephone) && Telephone.Length > 20)
            {
                erro = new ValidationError { Description = "Telefone inválido", Field = "Telephone", TypeofError = GetType(), Value = Telephone, UID = Pk };
                TooltipTelephone += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateFax()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Fax) && Fax.Length > 20)
            {
                erro = new ValidationError { Description = "Fax inválido", Field = "Fax", TypeofError = GetType(), Value = Fax, UID = Pk };
                TooltipFax += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateEmail()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Email) && Email.Length > 60)
            {
                erro = new ValidationError { Description = "Email inválido", Field = "Email", TypeofError = GetType(), Value = Email, UID = Pk };
                TooltipEmail += Environment.NewLine + erro.Description;
            }

            return erro;
        }
        public ValidationError ValidateWebsite()
        {
            ValidationError erro = null;
            if (!string.IsNullOrEmpty(Website) && Website.Length > 60)
            {
                erro = new ValidationError { Description = "Website inválido", Field = "Website", TypeofError = GetType(), Value = Website, UID = Pk };
                TooltipWebsite += Environment.NewLine + erro.Description;
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
                erro = new ValidationError { Description = "Nº de conta inválido", Field = "SelfBillingIndicator", TypeofError = GetType(), Value = SelfBillingIndicator, UID = Pk };
                TooltipSelfBillingIndicator += Environment.NewLine + erro.Description;
            }

            return erro;
        }

        public ValidationError[] ValidateBillingAddress()
        {
            List<ValidationError> listErro = new List<ValidationError>();

            if (BillingAddress == null)
                listErro.Add(new ValidationError { Description = "Morada de faturação inexistente", Field = "BillingAddress", TypeofError = GetType(), UID = Pk });

            if (BillingAddress != null)
            {
                if (string.IsNullOrEmpty(BillingAddress.AddressDetail) || BillingAddress.AddressDetail.Length > 100)
                    listErro.Add(new ValidationError { Description = "Tamanho da morada detalhada.", Field = "AddressDetail", TypeofError = GetType(), Value = BillingAddress.AddressDetail, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.BuildingNumber) == false && BillingAddress.BuildingNumber.Length > 10)
                    listErro.Add(new ValidationError { Description = "Tamanho do número de polícia incorrecto.", Field = "BuildingNumber", TypeofError = GetType(), Value = BillingAddress.BuildingNumber, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.City) || BillingAddress.City.Length > 50)
                    listErro.Add(new ValidationError { Description = "Tamanho da localidade incorrecto.", Field = "City", TypeofError = GetType(), Value = BillingAddress.City, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.Country) || BillingAddress.Country.Length != 2)
                    listErro.Add(new ValidationError { Description = "Tamanho do País incorrecto.", Field = "Country", TypeofError = GetType(), Value = BillingAddress.Country, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.PostalCode) || BillingAddress.PostalCode.Length > 20)
                    listErro.Add(new ValidationError { Description = "Tamanho do código postal incorrecto.", Field = "PostalCode", TypeofError = GetType(), Value = BillingAddress.PostalCode, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.Region) == false && BillingAddress.Region.Length > 50)
                    listErro.Add(new ValidationError { Description = "Tamanho do distrito incorrecto.", Field = "Region", TypeofError = GetType(), Value = BillingAddress.Region, UID = Pk });
                if (string.IsNullOrEmpty(BillingAddress.StreetName) == false && BillingAddress.StreetName.Length > 90)
                    listErro.Add(new ValidationError { Description = "Tamanho do nome da rua incorrecto.", Field = "StreetName", TypeofError = GetType(), Value = BillingAddress.StreetName, UID = Pk });
            }

            return listErro.ToArray();
        }
        public ValidationError[] ValidateShipFromAddress()
        {
            List<ValidationError> listErro = new List<ValidationError>();

            if (ShipFromAddress != null && ShipFromAddress.Length > 0)
            {
                foreach (var morada in ShipFromAddress)
                {
                    if (string.IsNullOrEmpty(morada.AddressDetail) || morada.AddressDetail.Length > 100)
                        listErro.Add(new ValidationError { Description = "Tamanho da morada detalhada.", Field = "AddressDetail", TypeofError = GetType(), Value = morada.AddressDetail, UID = Pk });
                    if (string.IsNullOrEmpty(morada.BuildingNumber) == false && morada.BuildingNumber.Length > 10)
                        listErro.Add(new ValidationError { Description = "Tamanho do número de polícia incorrecto.", Field = "BuildingNumber", TypeofError = GetType(), Value = morada.BuildingNumber, UID = Pk });
                    if (string.IsNullOrEmpty(morada.City) || morada.City.Length > 50)
                        listErro.Add(new ValidationError { Description = "Tamanho da localidade incorrecto.", Field = "City", TypeofError = GetType(), Value = morada.City, UID = Pk });
                    if (string.IsNullOrEmpty(morada.Country) || morada.Country.Length != 2)
                        listErro.Add(new ValidationError { Description = "Tamanho do País incorrecto.", Field = "Country", TypeofError = GetType(), Value = morada.Country, UID = Pk });
                    if (string.IsNullOrEmpty(morada.PostalCode) || morada.PostalCode.Length > 20)
                        listErro.Add(new ValidationError { Description = "Tamanho do código postal incorrecto.", Field = "PostalCode", TypeofError = GetType(), Value = morada.PostalCode, UID = Pk });
                    if (string.IsNullOrEmpty(morada.Region) == false && morada.Region.Length > 50)
                        listErro.Add(new ValidationError { Description = "Tamanho do distrito incorrecto.", Field = "Region", TypeofError = GetType(), Value = morada.Region, UID = Pk });
                    if (string.IsNullOrEmpty(morada.StreetName) == false && morada.StreetName.Length > 90)
                        listErro.Add(new ValidationError { Description = "Tamanho do nome da rua incorrecto.", Field = "StreetName", TypeofError = GetType(), Value = morada.StreetName, UID = Pk });
                }
            }

            return listErro.ToArray();
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
}
