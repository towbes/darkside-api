using Prism.Ioc;
using System.Windows;
using DarkSideWPFUI.Views;

namespace DarkSideWPFUI
{
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<DarkSideMainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
