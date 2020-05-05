using ReactiveUI;
using Solria.SAFT.Desktop.Models;
using Solria.SAFT.Desktop.Services;
using Splat;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class HeaderPageViewModel : ViewModelBase
    {
        readonly ISaftValidator saftValidator;

        public HeaderPageViewModel(IScreen screen) : base(screen, MenuIds.HEADER_PAGE)
        {
            saftValidator = Locator.Current.GetService<ISaftValidator>();
        }

        protected override void HandleActivation()
        {
            if (saftValidator?.SaftFileV4?.Header != null)
            {
                var h = saftValidator.SaftFileV4.Header;
                Header = new Models.Saft.Header
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
                    Website = h.Website
                };
            }
            else if (saftValidator?.SaftFileV3?.Header != null)
            {
                var h = saftValidator.SaftFileV3.Header;
                Header = new Models.Saft.Header
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
                    Website = h.Website
                };
            }
        }

        private Models.Saft.Header header;
        public Models.Saft.Header Header
        {
            get => header;
            set => this.RaiseAndSetIfChanged(ref header, value);
        }

    }
}
