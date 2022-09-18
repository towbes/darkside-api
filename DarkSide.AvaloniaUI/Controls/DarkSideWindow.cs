using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.Styling;

namespace DarkSide.AvaloniaUI.Controls
{
    public class DarkSideWindow : Window, IStyleable
    {
        Type IStyleable.StyleKey => typeof(Window);

        public DarkSideWindow()
        {
            //extend theme / content to tittle bar
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            CanResize = true;
            TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;

            //follow state properties
            this.GetObservable(WindowStateProperty)
                .Subscribe(
                    x =>
                    {
                        PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                        PseudoClasses.Set(":fullscreen", x == WindowState.FullScreen);
                    });

            //blur behind window
            this.GetObservable(IsExtendedIntoWindowDecorationsProperty)
                .Subscribe(
                    x =>
                    {
                        if (!x)
                        {
                            SystemDecorations = SystemDecorations.Full;
                            TransparencyLevelHint = WindowTransparencyLevel.Blur;
                        }
                    });
        }
    }
}