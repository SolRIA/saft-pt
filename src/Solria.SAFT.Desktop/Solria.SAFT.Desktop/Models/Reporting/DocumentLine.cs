namespace SolRIA.SAFT.Desktop.Models.Reporting;

public sealed class DocumentLine
{
    public string DocNo { get; set; }

    public string LineNumber { get; set; }
    public string ProductCode { get; set; }
    public string ProductDescription { get; set; }
    public string TaxCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? TaxBase { get; set; }
    public string Description { get; set; }
    public string TaxExemptionReason { get; set; }
    public string TaxExemptionCode { get; set; }

    public string Reference { get; set; }
}
