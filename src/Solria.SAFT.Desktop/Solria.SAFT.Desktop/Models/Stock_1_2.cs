﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.8009
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.3038.
// 
namespace SolRIA.SAFT.Desktop.Models.StocksV2
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:StockFile:PT_1_02")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="urn:StockFile:PT_1_02", IsNullable=false)]
    public partial class StockFile {
        
        private StockHeader stockHeaderField;
        
        private Stock[] stockField;
        
        /// <remarks/>
        public StockHeader StockHeader {
            get {
                return this.stockHeaderField;
            }
            set {
                this.stockHeaderField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Stock")]
        public Stock[] Stock {
            get {
                return this.stockField;
            }
            set {
                this.stockField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:StockFile:PT_1_02")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="urn:StockFile:PT_1_02", IsNullable=false)]
    public partial class StockHeader {
        
        private string fileVersionField;
        
        private string taxRegistrationNumberField;
        
        private string fiscalYearField;
        
        private System.DateTime endDateField;
        
        private bool noStockField;
        
        /// <remarks/>
        public string FileVersion {
            get {
                return this.fileVersionField;
            }
            set {
                this.fileVersionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="integer")]
        public string TaxRegistrationNumber {
            get {
                return this.taxRegistrationNumberField;
            }
            set {
                this.taxRegistrationNumberField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="integer")]
        public string FiscalYear {
            get {
                return this.fiscalYearField;
            }
            set {
                this.fiscalYearField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="date")]
        public System.DateTime EndDate {
            get {
                return this.endDateField;
            }
            set {
                this.endDateField = value;
            }
        }
        
        /// <remarks/>
        public bool NoStock {
            get {
                return this.noStockField;
            }
            set {
                this.noStockField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:StockFile:PT_1_02")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="urn:StockFile:PT_1_02", IsNullable=false)]
    public partial class Stock {
        
        private ProductCategory productCategoryField;
        
        private string productCodeField;
        
        private string productDescriptionField;
        
        private string productNumberCodeField;
        
        private decimal closingStockQuantityField;
        
        private string unitOfMeasureField;
        
        /// <remarks/>
        public ProductCategory ProductCategory {
            get {
                return this.productCategoryField;
            }
            set {
                this.productCategoryField = value;
            }
        }
        
        /// <remarks/>
        public string ProductCode {
            get {
                return this.productCodeField;
            }
            set {
                this.productCodeField = value;
            }
        }
        
        /// <remarks/>
        public string ProductDescription {
            get {
                return this.productDescriptionField;
            }
            set {
                this.productDescriptionField = value;
            }
        }
        
        /// <remarks/>
        public string ProductNumberCode {
            get {
                return this.productNumberCodeField;
            }
            set {
                this.productNumberCodeField = value;
            }
        }
        
        /// <remarks/>
        public decimal ClosingStockQuantity {
            get {
                return this.closingStockQuantityField;
            }
            set {
                this.closingStockQuantityField = value;
            }
        }
        
        /// <remarks/>
        public string UnitOfMeasure {
            get {
                return this.unitOfMeasureField;
            }
            set {
                this.unitOfMeasureField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:StockFile:PT_1_02")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="urn:StockFile:PT_1_02", IsNullable=false)]
    public enum ProductCategory {
        
        /// <remarks/>
        M,
        
        /// <remarks/>
        P,
        
        /// <remarks/>
        A,
        
        /// <remarks/>
        S,
        
        /// <remarks/>
        T,
    }
}
