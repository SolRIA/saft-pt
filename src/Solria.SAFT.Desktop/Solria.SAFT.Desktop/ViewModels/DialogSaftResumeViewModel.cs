using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolRIA.SAFT.Desktop.Services;
using SolRIA.SAFT.Parser.Models;
using System.Linq;

namespace SolRIA.SAFT.Desktop.ViewModels;

public partial class DialogSaftResumeViewModel : ViewModelBase
{
    private readonly ISaftValidator saftValidator;
    private readonly IDialogManager dialogManager;
    private readonly INavigationService navigationService;

    public DialogSaftResumeViewModel()
    {
        saftValidator = AppBootstrap.Resolve<ISaftValidator>();
        dialogManager = AppBootstrap.Resolve<IDialogManager>();
        navigationService = AppBootstrap.Resolve<INavigationService>();
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

    [ObservableProperty]
    private Header header;
    
    [ObservableProperty]
    private int totalErrors;
    
    [ObservableProperty]
    private int headerErrors;
    
    [ObservableProperty]
    private int customersErrors;
    
    [ObservableProperty]
    private int saftHashValidationNumber;
    
    [ObservableProperty]
    private int saftHashValidationErrorNumber;

    [RelayCommand]
    private void OnOpenErrors()
    {
        navigationService.NavigateTo(new SaftErrorPageViewModel());
        dialogManager.CloseDialog();
    }

    [RelayCommand]
    private void OnOpenHeader()
    {
        navigationService.NavigateTo(new SaftHeaderPageViewModel());
        dialogManager.CloseDialog();
    }

    [RelayCommand]
    private void OnOpenCustomers()
    {
        navigationService.NavigateTo(new SaftCustomersPageViewModel());
        dialogManager.CloseDialog();
    }

    [RelayCommand]
    private void OnCloseDialog()
    {
        dialogManager.CloseDialog();
    }

    [RelayCommand]
    private void OnOpenInvoices()
    {
        navigationService.NavigateTo(new SaftInvoicesPageViewModel());
        dialogManager.CloseDialog();
    }
}
