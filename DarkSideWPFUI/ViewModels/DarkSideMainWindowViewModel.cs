using Prism.Mvvm;
namespace DarkSideWPFUI.ViewModels
{
    public class DarkSideMainWindowViewModel : BindableBase
    {
        private string _title = "DarkSideWPFUI";
        public string Ttitle
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DarkSideMainWindowViewModel()
        {
            
        }
    }
}
