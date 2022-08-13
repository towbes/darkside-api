using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace DarkSideModernGUI.ViewModels
{
    public partial class ContainerViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _applicationTitle = String.Empty;

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        public ContainerViewModel(INavigationService navigationService)
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ApplicationTitle = "DAOC DarkSide by Towbes, Darkjamin & G-Man";

            NavigationItems = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Home",
                    PageTag = "dashboard",
                    Icon = SymbolRegular.Home24,
                    PageType = typeof(Views.Pages.DashboardPage)
                },

                            new NavigationItem()
                {
                    Content = "Class Settings",
                    PageTag = "ClassSettings",
                    Icon = SymbolRegular.Accessibility24,
                    PageType = typeof(Views.Pages.ClassSettingsPage)
                },
                                   new NavigationItem()
                {
                    Content = "Waypoints (Nav)",
                    PageTag = "waypoints",
                    Icon = SymbolRegular.VehicleSubway24,
                    PageType = typeof(Views.Pages.WaypointsPage)
                },
                   new NavigationItem()
                {
                    Content = "Test",
                    PageTag = "test",
                    Icon = SymbolRegular.Beach24,
                    PageType = typeof(Views.Pages.TestPage)
                }

            };

            NavigationFooter = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Settings",
                    PageTag = "settings",
                    Icon = SymbolRegular.Settings24,
                    PageType = typeof(Views.Pages.SettingsPage)
                }
            };

            TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    Header = "Home",
                    Tag = "tray_home"
                }
            };

            _isInitialized = true;
        }
    }
}
