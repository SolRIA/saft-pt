namespace Solria.SAFT.Parser.Models
{
    public partial class StockFile
    {
        public StockHeader StockHeader { get; set; }
        public Stock[] Stock { get; set; }
    }

    public partial class StockHeader
    {
        public string FileVersion { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string FiscalYear { get; set; }
        public System.DateTime EndDate { get; set; }
        public bool NoStock { get; set; }
    }

    public partial class Stock
    {
        public ProductCategory ProductCategory { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public string ProductNumberCode { get; set; }
        public decimal ClosingStockQuantity { get; set; }
        public string UnitOfMeasure { get; set; }
    }

    public enum ProductCategory { M, P, A, S, T }
}
