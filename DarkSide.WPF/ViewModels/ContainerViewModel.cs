using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DarkSide.WPF.Views.Pages;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace DarkSide.WPF.ViewModels;

public partial class ContainerViewModel : ObservableObject
{
    [ObservableProperty] private string _applicationTitle = string.Empty;
    private bool _isInitialized;

    [ObservableProperty] private ObservableCollection<INavigationControl> _navigationFooter = new();

    [ObservableProperty] private ObservableCollection<INavigationControl> _navigationItems = new();

    [ObservableProperty] private ObservableCollection<MenuItem> _trayMenuItems = new();

    public ContainerViewModel(INavigationService navigationService)
    {
        if (!_isInitialized)
        {
            InitializeViewModel();
        }
    }

    private void InitializeViewModel()
    {
        ApplicationTitle = "DAOC DarkSide by Towbes, Darkjamin & G-Man";

        NavigationItems = new ObservableCollection<INavigationControl> { new NavigationItem { Content = "Home", PageTag = "dashboard", Icon = SymbolRegular.Home24, PageType = typeof(DashboardPage) }, new NavigationItem { Content = "Class Settings", PageTag = "ClassSettings", Icon = SymbolRegular.Accessibility24, PageType = typeof(ClassSettingsPage) }, new NavigationItem { Content = "Waypoints (Nav)", PageTag = "waypoints", Icon = SymbolRegular.VehicleSubway24, PageType = typeof(WaypointsPage) }, new NavigationItem { Content = "Test", PageTag = "test", Icon = SymbolRegular.Beach24, PageType = typeof(TestPage) } };

        NavigationFooter = new ObservableCollection<INavigationControl> { new NavigationItem { Content = "Settings", PageTag = "settings", Icon = SymbolRegular.Settings24, PageType = typeof(SettingsPage) } };

        TrayMenuItems = new ObservableCollection<MenuItem> { new() { Header = "Home", Tag = "tray_home" } };

        _isInitialized = true;
    }
}