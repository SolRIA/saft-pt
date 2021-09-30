using System;

namespace Solria.SAFT.Parser.Models
{
    public class ValidationError
	{
		public string Description { get; set; }

		public Type TypeofError { get; set; }

		public string Field { get; set; }

		public string Value { get; set; }

		/// <summary>
		/// Unique ID, to identify the register with error
		/// </summary>
		public string FileID { get; set; }
		public string UID { get; set; }

		public string SupUID { get; set; }

		public string GranSupUID { get; set; }

		string displayName;
		public string DisplayName
		{
			get
			{
				if (TypeofError == null)
					displayName = "??";
				else
				{
					displayName = TypeofError.Name switch
					{
						"SourceDocumentsSalesInvoicesInvoice" => "Documentos facturação",
						"SourceDocumentsSalesInvoices" => "Documentos facturação",
						"SourceDocumentsWorkingDocuments" => "Documentos de conferência",
						"SourceDocumentsWorkingDocumentsWorkDocument" => "Documentos de conferência",
						"SourceDocumentsWorkingDocumentsWorkDocumentLine" => "Linhas documentos de conferência",
						"SourceDocumentsMovementOfGoods" => "Mercadorias",
						"SourceDocumentsMovementOfGoodsStockMovement" => "Mercadorias",
						"SourceDocumentsMovementOfGoodsStockMovementLine" => "Linhas mercadorias",
						"SourceDocumentsPayments" => "Linhas mercadorias",
						"SourceDocumentsPaymentsPayment" => "Recibos",
						"SourceDocumentsPaymentsPaymentLine" => "Linhas recibos",
						"SourceDocumentsSalesInvoicesInvoiceLine" => "Linhas documentos de facturação",
						"Product" => "Produtos",
						"Customer" => "Clientes",
						"Header" => "Cabeçalho",
						"Tax" => "Impostos",
						"Supplier" => "Fornecedores",
						"SchemaResults" => "Schema",
						"HashResults" => "Assinaturas documentos",
						_ => "??",
					};
				}

				return displayName;
			}

			private set { displayName = value; }
		}
	}
}
