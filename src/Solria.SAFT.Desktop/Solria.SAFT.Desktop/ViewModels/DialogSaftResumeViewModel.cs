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
        var erros = saftValidator.GetErrors();

        if (saftValidator.SaftFile != null)
        {
            Header = new Header
            {
                BusinessName = saftValidator.SaftFile.Header.BusinessName,
                CompanyName = saftValidator.SaftFile.Header.CompanyName,
                TaxRegistrationNumber = saftValidator.SaftFile.Header.TaxRegistrationNumber
            };

            HeaderErrors = erros.Count(m => m.TypeofError == typeof(Models.SaftV4.Header));
            CustomersErrors = erros.Count(m => m.TypeofError == typeof(Models.SaftV4.Customer));
        }

        TotalErrors = erros.Count;
        SaftHashValidationNumber = saftValidator.SaftHashValidationNumber;
        SaftHashValidationErrorNumber = saftValidator.SaftHashValidationErrorNumber;
    }

    public string Title { get; set; } = "Resumo";

    [ObservableProperty]
    public partial Header Header { get; set; }
    
    [ObservableProperty]
    public partial int TotalErrors { get; set; }
    
    [ObservableProperty]
    public partial int HeaderErrors {get; set; }
    
    [ObservableProperty]
    public partial int CustomersErrors {get; set; }
    
    [ObservableProperty]
    public partial int SaftHashValidationNumber {get; set; }
    
    [ObservableProperty]
    public partial int SaftHashValidationErrorNumber {get; set; }

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
