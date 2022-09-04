using DarkSide.Core;
using DarkSide.Modules.NavigationMenu.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DarkSide.Modules.NavigationMenu
{
    public class NavigationMenuModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public NavigationMenuModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.NavigationMenuRegion, typeof(ViewNavigationMenu));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}