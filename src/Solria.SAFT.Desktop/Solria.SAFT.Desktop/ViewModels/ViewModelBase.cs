using ReactiveUI;
using System.Reactive.Disposables;

namespace Solria.SAFT.Desktop.ViewModels
{
    public class ViewModelBase : ReactiveObject, IRoutableViewModel, IActivatableViewModel
    {
        public string UrlPathSegment { get; }
        public IScreen HostScreen { get; }
        public ViewModelActivator Activator { get; }

        public ViewModelBase(IScreen screen, string urlPathSegment)
        {
            UrlPathSegment = urlPathSegment;
            HostScreen = screen;

            Activator = new ViewModelActivator();
            this.WhenActivated(disposables =>
            {
                HandleActivation();

                Disposable.Create(() => HandleDeactivation())
                .DisposeWith(disposables);
            });
        }

        protected virtual void HandleActivation()
        {

        }

        protected virtual void HandleDeactivation()
        {

        }
    }
}
