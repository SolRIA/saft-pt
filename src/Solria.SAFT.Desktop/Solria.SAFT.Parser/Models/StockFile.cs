namespace SolRIA.SAFT.Parser.Models
{
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "urn:StockFile:PT_1_02")]
    [System.Xml.Serialization.XmlRoot(Namespace = "urn:StockFile:PT_1_02", IsNullable = false)]
    public partial class StockFile
    {
        public StockHeader StockHeader { get; set; }

        [System.Xml.Serialization.XmlElement("Stock")]
        public Stock[] Stock { get; set; }
    }

    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "urn:StockFile:PT_1_02")]
    [System.Xml.Serialization.XmlRoot(Namespace = "urn:StockFile:PT_1_02", IsNullable = false)]
    public partial class StockHeader
    {
        public string FileVersion { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string FiscalYear { get; set; }
        public System.DateTime EndDate { get; set; }
        public bool NoStock { get; set; }
    }

    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "urn:StockFile:PT_1_02")]
    [System.Xml.Serialization.XmlRoot(Namespace = "urn:StockFile:PT_1_02", IsNullable = false)]
    public partial class Stock
    {
        public ProductCategory ProductCategory { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public string ProductNumberCode { get; set; }
        public decimal ClosingStockQuantity { get; set; }
        public decimal ClosingStockValue { get; set; }
        public string UnitOfMeasure { get; set; }
    }

    public enum ProductCategory { M, P, A, S, T, B }
}
