using DarkSide.Views;
using Prism.Ioc;
using System.Windows;
using Prism.Modularity;
using Prism.Regions;

namespace DarkSide
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
           // moduleCatalog.AddModule<MainModule>();
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
           base.ConfigureRegionAdapterMappings(regionAdapterMappings);
          // regionAdapterMappings.RegisterMapping(typeof(NavigationStore),Container.Resolve<NavigationStoreRegionAdapter>());

        }
    }
}
