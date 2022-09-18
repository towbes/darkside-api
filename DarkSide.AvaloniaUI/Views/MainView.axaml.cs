using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace DarkSide.AvaloniaUI.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            
        }
    }
}
