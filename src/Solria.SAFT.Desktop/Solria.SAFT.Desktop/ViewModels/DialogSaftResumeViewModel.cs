using ReactiveUI;
using Solria.SAFT.Desktop.Services;
using Solria.SAFT.Parser.Models;
using Splat;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class DialogSaftResumeViewModel : ReactiveObject
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;
        private readonly IScreen router;

        public DialogSaftResumeViewModel(IScreen router)
        {
            this.router = router;
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

            OpenErrorsCommand = ReactiveCommand.Create(OnOpenErrors);
            OpenHeaderCommand = ReactiveCommand.Create(OnOpenHeader);
            OpenCustomersCommand = ReactiveCommand.Create(OnOpenCustomers);
            CloseDialogCommand = ReactiveCommand.Create(OnCloseDialog);
            OpenInvoicesCommand = ReactiveCommand.Create(OnOpenInvoices);
        }

        public void Init()
        {
            if (saftValidator.SaftFile != null)
            {
                Header = new Header
                {
                    BusinessName = saftValidator.SaftFile.Header.BusinessName,
                    CompanyName = saftValidator.SaftFile.Header.CompanyName,
                    TaxRegistrationNumber = saftValidator.SaftFile.Header.TaxRegistrationNumber
                };

                HeaderErrors = saftValidator.MensagensErro.Where(m => m.TypeofError == typeof(Models.SaftV4.Header)).Count();
                CustomersErrors = saftValidator.MensagensErro.Where(m => m.TypeofError == typeof(Models.SaftV4.Customer)).Count();
            }

            TotalErrors = saftValidator.MensagensErro.Count;
            SaftHashValidationNumber = saftValidator.SaftHashValidationNumber;
            SaftHashValidationErrorNumber = saftValidator.SaftHashValidationErrorNumber;
        }

        public string Title { get; set; } = "Resumo";

        private Header header;
        public Header Header
        {
            get => header;
            set => this.RaiseAndSetIfChanged(ref header, value);
        }

        private int totalerrors;
        public int TotalErrors
        {
            get => totalerrors;
            set => this.RaiseAndSetIfChanged(ref totalerrors, value);
        }

        private int headerErrors;
        public int HeaderErrors
        {
            get => headerErrors;
            set => this.RaiseAndSetIfChanged(ref headerErrors, value);
        }
        private int customersErrors;
        public int CustomersErrors
        {
            get => customersErrors;
            set => this.RaiseAndSetIfChanged(ref customersErrors, value);
        }

        private int saftHashValidationNumber;
        public int SaftHashValidationNumber
        {
            get => saftHashValidationNumber;
            set => this.RaiseAndSetIfChanged(ref saftHashValidationNumber, value);
        }

        private int saftHashValidationErrorNumber;
        public int SaftHashValidationErrorNumber
        {
            get => saftHashValidationErrorNumber;
            set => this.RaiseAndSetIfChanged(ref saftHashValidationErrorNumber, value);
        }

        public ReactiveCommand<Unit, Unit> OpenErrorsCommand { get; }
        private void OnOpenErrors()
        {
            router.Router.NavigateAndReset.Execute(new SaftErrorPageViewModel(router));
            dialogManager.CloseDialog();
        }

        public ReactiveCommand<Unit, Unit> OpenHeaderCommand { get; }
        private void OnOpenHeader()
        {
            router.Router.NavigateAndReset.Execute(new SaftHeaderPageViewModel(router));
            dialogManager.CloseDialog();
        }

        public ReactiveCommand<Unit, Unit> OpenCustomersCommand { get; }
        private void OnOpenCustomers()
        {
            router.Router.NavigateAndReset.Execute(new SaftCustomersPageViewModel(router));
            dialogManager.CloseDialog();
        }

        public ReactiveCommand<Unit, Unit> CloseDialogCommand { get; }
        private void OnCloseDialog()
        {
            dialogManager.CloseDialog();
        }

        public ReactiveCommand<Unit, Unit> OpenInvoicesCommand { get; }
        private void OnOpenInvoices()
        {
            router.Router.NavigateAndReset.Execute(new SaftInvoicesPageViewModel(router));
            dialogManager.CloseDialog();
        }
    }
}
