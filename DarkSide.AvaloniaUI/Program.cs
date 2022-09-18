using Avalonia;

namespace DarkSide.AvaloniaUI;

internal class Program
{
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().With(new Win32PlatformOptions() { UseWindowsUIComposition = true, CompositionBackdropCornerRadius = 8f });
    }
}