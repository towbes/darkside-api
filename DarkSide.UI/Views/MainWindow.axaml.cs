using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData.Binding;

namespace DarkSide.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ViewOne(object? sender, RoutedEventArgs e)
        {
            this.Title = "1";
        }
    }
}