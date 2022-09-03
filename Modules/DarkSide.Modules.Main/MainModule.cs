using DarkSide.Core;
using DarkSide.Modules.Main.ViewModels;
using DarkSide.Modules.Main.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DarkSide.Modules.Main
{
    public class MainModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public MainModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.RootFrameRegion, typeof(ViewMain));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}