using OnlineShop2.RMK.Pages.Rmk;
using ReactiveUI;

namespace OnlineShop2.RMK.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _contentViewModel;
        public string Greeting => "Welcome to Avalonia!";
        public MainWindowViewModel()
        {
            _contentViewModel = ViewModelBase.Create<RmkViewModel>();
        }

        public ViewModelBase ContentViewModel
        {
            get => _contentViewModel;
            private set
            {
                _contentViewModel.Dispose();
                this.RaiseAndSetIfChanged(ref _contentViewModel, value);
            }
        }
    }
}