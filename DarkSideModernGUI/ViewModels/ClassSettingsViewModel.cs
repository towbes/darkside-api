using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Wpf.Ui.Common.Interfaces;

namespace DarkSideModernGUI.ViewModels
{
    public partial class ClassSettingsViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private int _counter = 0;

        public void OnNavigatedTo()
        {
        }

        public void OnNavigatedFrom()
        {
        }

        [ICommand]
        private void OnCounterIncrement()
        {
            Counter++;
        }
    }
}
