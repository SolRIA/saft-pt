using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Reactive.Disposables;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class SaftHeaderPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;

        public SaftHeaderPageViewModel(IScreen screen) : base(screen, MenuIds.SAFT_HEADER_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
        }

        public static Models.Saft.Header GetHeader(ISaftValidator saftValidator)
        {
            Models.Saft.Header header = null;

            if (saftValidator?.SaftFileV4?.Header != null)
            {
                var h = saftValidator.SaftFileV4.Header;
                header = new Models.Saft.Header
                {
                    AuditFileVersion = h.AuditFileVersion,
                    BusinessName = h.BusinessName,
                    CompanyAddress = new Models.Saft.AddressStructurePT
                    {
                        AddressDetail = h.CompanyAddress?.AddressDetail,
                        BuildingNumber = h.CompanyAddress?.BuildingNumber,
                        City = h.CompanyAddress?.City,
                        Country = h.CompanyAddress?.Country?.ToString(),
                        PostalCode = h.CompanyAddress?.PostalCode,
                        Region = h.CompanyAddress?.Region,
                        StreetName = h.CompanyAddress?.StreetName
                    },
                    CompanyID = h.CompanyID,
                    CompanyName = h.CompanyName,
                    CurrencyCode = h.CurrencyCode?.ToString(),
                    DateCreated = h.DateCreated,
                    Email = h.Email,
                    EndDate = h.EndDate,
                    Fax = h.Fax,
                    FiscalYear = h.FiscalYear,
                    HeaderComment = h.HeaderComment,
                    ProductCompanyTaxID = h.ProductCompanyTaxID,
                    ProductID = h.ProductID,
                    ProductVersion = h.ProductVersion,
                    SoftwareCertificateNumber = h.SoftwareCertificateNumber,
                    StartDate = h.StartDate,
                    TaxAccountingBasis = h.TaxAccountingBasis.ToString(),
                    TaxEntity = h.TaxEntity,
                    TaxRegistrationNumber = h.TaxRegistrationNumber,
                    Telephone = h.Telephone,
                    Website = h.Website,
                    TooltipAddressDetail = h.TooltipAddressDetail,
                    TooltipAuditFileVersion = h.TooltipAuditFileVersion,
                    TooltipBuildingNumber = h.TooltipBuildingNumber,
                    TooltipBusinessName = h.TooltipBusinessName,
                    TooltipCity = h.TooltipCity,
                    TooltipCompanyAddress = h.TooltipCompanyAddress,
                    TooltipCompanyID = h.TooltipCompanyID,
                    TooltipCompanyName = h.TooltipCompanyName,
                    TooltipCountry = h.TooltipCountry,
                    TooltipCurrencyCode = h.TooltipCurrencyCode,
                    TooltipDateCreated = h.TooltipDateCreated,
                    TooltipEmail = h.TooltipEmail,
                    TooltipEndDate = h.TooltipEndDate,
                    TooltipFax = h.TooltipFax,
                    TooltipFiscalYear = h.TooltipFiscalYear,
                    TooltipHeaderComment = h.TooltipHeaderComment,
                    TooltipPostalCode = h.TooltipPostalCode,
                    TooltipProductCompanyTaxID = h.TooltipProductCompanyTaxID,
                    TooltipProductID = h.TooltipProductID,
                    TooltipProductVersion = h.TooltipProductVersion,
                    TooltipRegion = h.TooltipRegion,
                    TooltipSoftwareCertificateNumber = h.TooltipSoftwareCertificateNumber,
                    TooltipStartDate = h.TooltipStartDate,
                    TooltipStreetName = h.TooltipStreetName,
                    TooltipTaxAccountingBasis = h.TooltipTaxAccountingBasis,
                    TooltipTaxEntity = h.TooltipTaxEntity,
                    TooltipTaxRegistrationNumber = h.TooltipTaxRegistrationNumber,
                    TooltipTelephone = h.TooltipTelephone,
                    TooltipWebsite = h.TooltipWebsite
                };
            }
            else if (saftValidator?.SaftFileV3?.Header != null)
            {
                var h = saftValidator.SaftFileV3.Header;
                header = new Models.Saft.Header
                {
                    AuditFileVersion = h.AuditFileVersion,
                    BusinessName = h.BusinessName,
                    CompanyAddress = new Models.Saft.AddressStructurePT
                    {
                        AddressDetail = h.CompanyAddress?.AddressDetail,
                        BuildingNumber = h.CompanyAddress?.BuildingNumber,
                        City = h.CompanyAddress?.City,
                        Country = h.CompanyAddress?.Country?.ToString(),
                        PostalCode = h.CompanyAddress?.PostalCode,
                        Region = h.CompanyAddress?.Region,
                        StreetName = h.CompanyAddress?.StreetName
                    },
                    CompanyID = h.CompanyID,
                    CompanyName = h.CompanyName,
                    CurrencyCode = h.CurrencyCode?.ToString(),
                    DateCreated = h.DateCreated,
                    Email = h.Email,
                    EndDate = h.EndDate,
                    Fax = h.Fax,
                    FiscalYear = h.FiscalYear,
                    HeaderComment = h.HeaderComment,
                    ProductCompanyTaxID = h.ProductCompanyTaxID,
                    ProductID = h.ProductID,
                    ProductVersion = h.ProductVersion,
                    SoftwareCertificateNumber = h.SoftwareCertificateNumber,
                    StartDate = h.StartDate,
                    TaxAccountingBasis = h.TaxAccountingBasis.ToString(),
                    TaxEntity = h.TaxEntity,
                    TaxRegistrationNumber = h.TaxRegistrationNumber,
                    Telephone = h.Telephone,
                    Website = h.Website,
                    TooltipAddressDetail = h.TooltipAddressDetail,
                    TooltipAuditFileVersion = h.TooltipAuditFileVersion,
                    TooltipBuildingNumber = h.TooltipBuildingNumber,
                    TooltipBusinessName = h.TooltipBusinessName,
                    TooltipCity = h.TooltipCity,
                    TooltipCompanyAddress = h.TooltipCompanyAddress,
                    TooltipCompanyID = h.TooltipCompanyID,
                    TooltipCompanyName = h.TooltipCompanyName,
                    TooltipCountry = h.TooltipCountry,
                    TooltipCurrencyCode = h.TooltipCurrencyCode,
                    TooltipDateCreated = h.TooltipDateCreated,
                    TooltipEmail = h.TooltipEmail,
                    TooltipEndDate = h.TooltipEndDate,
                    TooltipFax = h.TooltipFax,
                    TooltipFiscalYear = h.TooltipFiscalYear,
                    TooltipHeaderComment = h.TooltipHeaderComment,
                    TooltipPostalCode = h.TooltipPostalCode,
                    TooltipProductCompanyTaxID = h.TooltipProductCompanyTaxID,
                    TooltipProductID = h.TooltipProductID,
                    TooltipProductVersion = h.TooltipProductVersion,
                    TooltipRegion = h.TooltipRegion,
                    TooltipSoftwareCertificateNumber = h.TooltipSoftwareCertificateNumber,
                    TooltipStartDate = h.TooltipStartDate,
                    TooltipStreetName = h.TooltipStreetName,
                    TooltipTaxAccountingBasis = h.TooltipTaxAccountingBasis,
                    TooltipTaxEntity = h.TooltipTaxEntity,
                    TooltipTaxRegistrationNumber = h.TooltipTaxRegistrationNumber,
                    TooltipTelephone = h.TooltipTelephone,
                    TooltipWebsite = h.TooltipWebsite
                };
            }

            return header;
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            Header = GetHeader(saftValidator);
        }

        private Models.Saft.Header header;
        public Models.Saft.Header Header
        {
            get => header;
            set => this.RaiseAndSetIfChanged(ref header, value);
        }
    }
}
