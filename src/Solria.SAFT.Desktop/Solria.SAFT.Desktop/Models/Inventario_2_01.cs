﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código foi gerado por uma ferramenta.
//     Versão de Tempo de Execução:4.0.30319.42000
//
//     As alterações a este ficheiro poderão provocar um comportamento incorrecto e perder-se-ão se
//     o código for regenerado.
// </auto-generated>
//------------------------------------------------------------------------------
// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 

namespace SolRIA.SAFT.Desktop.Models.StocksV21
{
    /// <observações/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:StockFile:PT_2_01")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:StockFile:PT_2_01", IsNullable = false)]
    public partial class StockFile
    {

        private StockHeader stockHeaderField;

        private Stock[] stockField;

        /// <observações/>
        public StockHeader StockHeader
        {
            get
            {
                return this.stockHeaderField;
            }
            set
            {
                this.stockHeaderField = value;
            }
        }

        /// <observações/>
        [System.Xml.Serialization.XmlElementAttribute("Stock")]
        public Stock[] Stock
        {
            get
            {
                return this.stockField;
            }
            set
            {
                this.stockField = value;
            }
        }
    }

    /// <observações/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:StockFile:PT_2_01")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:StockFile:PT_2_01", IsNullable = false)]
    public partial class StockHeader
    {

        private string fileVersionField;

        private string taxRegistrationNumberField;

        private string fiscalYearField;

        private System.DateTime endDateField;

        private bool noStockField;

        /// <observações/>
        public string FileVersion
        {
            get
            {
                return this.fileVersionField;
            }
            set
            {
                this.fileVersionField = value;
            }
        }

        /// <observações/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string TaxRegistrationNumber
        {
            get
            {
                return this.taxRegistrationNumberField;
            }
            set
            {
                this.taxRegistrationNumberField = value;
            }
        }

        /// <observações/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string FiscalYear
        {
            get
            {
                return this.fiscalYearField;
            }
            set
            {
                this.fiscalYearField = value;
            }
        }

        /// <observações/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime EndDate
        {
            get
            {
                return this.endDateField;
            }
            set
            {
                this.endDateField = value;
            }
        }

        /// <observações/>
        public bool NoStock
        {
            get
            {
                return this.noStockField;
            }
            set
            {
                this.noStockField = value;
            }
        }
    }

    /// <observações/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:StockFile:PT_2_01")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:StockFile:PT_2_01", IsNullable = false)]
    public partial class Stock
    {

        private ProductCategory productCategoryField;

        private string productCodeField;

        private string productDescriptionField;

        private string productNumberCodeField;

        private decimal closingStockQuantityField;

        private string unitOfMeasureField;

        private decimal closingStockValueField;

        /// <observações/>
        public ProductCategory ProductCategory
        {
            get
            {
                return this.productCategoryField;
            }
            set
            {
                this.productCategoryField = value;
            }
        }

        /// <observações/>
        public string ProductCode
        {
            get
            {
                return this.productCodeField;
            }
            set
            {
                this.productCodeField = value;
            }
        }

        /// <observações/>
        public string ProductDescription
        {
            get
            {
                return this.productDescriptionField;
            }
            set
            {
                this.productDescriptionField = value;
            }
        }

        /// <observações/>
        public string ProductNumberCode
        {
            get
            {
                return this.productNumberCodeField;
            }
            set
            {
                this.productNumberCodeField = value;
            }
        }

        /// <observações/>
        public decimal ClosingStockQuantity
        {
            get
            {
                return this.closingStockQuantityField;
            }
            set
            {
                this.closingStockQuantityField = value;
            }
        }

        /// <observações/>
        public string UnitOfMeasure
        {
            get
            {
                return this.unitOfMeasureField;
            }
            set
            {
                this.unitOfMeasureField = value;
            }
        }

        /// <observações/>
        public decimal ClosingStockValue
        {
            get
            {
                return this.closingStockValueField;
            }
            set
            {
                this.closingStockValueField = value;
            }
        }
    }

    /// <observações/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:StockFile:PT_2_01")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:StockFile:PT_2_01", IsNullable = false)]
    public enum ProductCategory
    {

        /// <observações/>
        M,

        /// <observações/>
        P,

        /// <observações/>
        A,

        /// <observações/>
        S,

        /// <observações/>
        T,

        /// <observações/>
        B,
    }
}