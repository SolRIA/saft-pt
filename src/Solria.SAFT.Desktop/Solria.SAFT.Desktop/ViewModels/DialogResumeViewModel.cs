using ReactiveUI;
using Solria.SAFT.Desktop.Models.Saft;
using Solria.SAFT.Desktop.Services;
using Splat;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Solria.SAFT.Desktop.ViewModels
{
	public class DialogResumeViewModel : ReactiveObject
    {
        private readonly ISaftValidator saftValidator;
        private readonly IDialogManager dialogManager;
		private readonly IScreen router;

		public DialogResumeViewModel(IScreen router)
        {
			this.router = router;
            saftValidator = Locator.Current.GetService<ISaftValidator>();
            dialogManager = Locator.Current.GetService<IDialogManager>();

			OpenHeaderCommand = ReactiveCommand.Create(OnOpenHeader);
			OpenCustomersCommand = ReactiveCommand.Create(OnOpenCustomers);
			CloseDialogCommand = ReactiveCommand.Create(OnCloseDialog);
			OpenInvoicesCommand = ReactiveCommand.Create(OnOpenInvoices);
		}

		public void Init()
		{
			if (saftValidator.SaftFileV4 != null)
			{
				Header = new Header
				{
					BusinessName = saftValidator.SaftFileV4.Header.BusinessName,
					CompanyName = saftValidator.SaftFileV4.Header.CompanyName,
					TaxRegistrationNumber = saftValidator.SaftFileV4.Header.TaxRegistrationNumber
				};

				HeaderErrors = saftValidator.MensagensErro.Where(m => m.TypeofError == typeof(Models.SaftV4.Header)).Count();
				CustomersErrors = saftValidator.MensagensErro.Where(m => m.TypeofError == typeof(Models.SaftV4.Customer)).Count();
			}
			else if (saftValidator.SaftFileV3 != null)
			{
				Header = new Header
				{
					BusinessName = saftValidator.SaftFileV3.Header.BusinessName,
					CompanyName = saftValidator.SaftFileV3.Header.CompanyName,
					TaxRegistrationNumber = saftValidator.SaftFileV3.Header.TaxRegistrationNumber
				};

				HeaderErrors = saftValidator.MensagensErro.Where(m => m.TypeofError == typeof(Models.SaftV3.Header)).Count();
				CustomersErrors = saftValidator.MensagensErro.Where(m => m.TypeofError == typeof(Models.SaftV3.Customer)).Count();
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

		public ReactiveCommand<Unit, Unit> OpenHeaderCommand { get; }
		private void OnOpenHeader()
		{
			router.Router.NavigateAndReset.Execute(new HeaderPageViewModel(router));
			dialogManager.CloseDialog();
		}

		public ReactiveCommand<Unit, Unit> OpenCustomersCommand { get; }
		private void OnOpenCustomers()
		{
			router.Router.NavigateAndReset.Execute(new CustomersPageViewModel(router));
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
			router.Router.NavigateAndReset.Execute(new InvoicesPageViewModel(router));
			dialogManager.CloseDialog();
		}
	}
}
