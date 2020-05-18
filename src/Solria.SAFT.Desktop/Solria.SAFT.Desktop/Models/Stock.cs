namespace Solria.SAFT.Desktop.Models.Stock
{
    public class StockHeader
    {
        public string FileVersion { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string FiscalYear { get; set; }
        public System.DateTime EndDate { get; set; }
        public bool NoStock { get; set; }
    }

    public class Stock
    {
        public string ProductCategory { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public string ProductNumberCode { get; set; }
        public decimal ClosingStockQuantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal ClosingStockValue { get; set; }
    }
}
