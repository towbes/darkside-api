using System.Linq;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Skia;
using DarkSide.AvaloniaUI.Controls;
using FluentAvalonia.UI.Controls;

namespace DarkSide.AvaloniaUI.Views
{
    public partial class MainWindow : DarkSideWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavView_OnItemInvoked(object? sender, NavigationViewItemInvokedEventArgs args)
        {
            MessageBox.Show(args.InvokedItem.ToString());
        }
    }
}
