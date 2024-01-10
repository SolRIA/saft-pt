namespace SolRIA.SAFT.Desktop.Models.Reporting
{
    public class Tax
    {
        public string TaxType { get; set; }
        public string TaxCountryRegion { get; set; }
        public string TaxCode { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
    }
}
