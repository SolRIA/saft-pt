using Avalonia.Collections;
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
                HandleActivation(disposables);

                Disposable.Create(() => HandleDeactivation())
                .DisposeWith(disposables);
            });
        }

        protected virtual void HandleActivation(CompositeDisposable disposables)
        {

        }

        protected virtual void HandleDeactivation()
        {

        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => this.RaiseAndSetIfChanged(ref isLoading, value);
        }

        private string filter;
        public string Filter
        {
            get => filter;
            set => this.RaiseAndSetIfChanged(ref filter, value);
        }

        private DataGridCollectionView collectionView;
        public DataGridCollectionView CollectionView
        {
            get => collectionView;
            set => this.RaiseAndSetIfChanged(ref collectionView, value);
        }
    }
}
