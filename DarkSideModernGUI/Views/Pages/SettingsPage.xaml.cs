using DarkSideModernGUI.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace DarkSideModernGUI.Views.Pages;

/// <summary>
///     Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : INavigableView<SettingsViewModel>
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }
}