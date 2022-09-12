using DarkSideModernGUI.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace DarkSideModernGUI.Views.Pages;

/// <summary>
///     Interaction logic for TestPage.xaml
/// </summary>
public partial class ClassSettingsPage : INavigableView<ClassSettingsViewModel>
{
    public ClassSettingsPage(ClassSettingsViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }

    public ClassSettingsViewModel ViewModel { get; }
}