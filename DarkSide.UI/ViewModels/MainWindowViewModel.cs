using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.Chrome;
using Avalonia.Remote.Protocol.Viewport;
using DarkSide.Core;
using ReactiveUI;



namespace DarkSide.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _Greeting;


        public string Greeting
        {
            get => _Greeting;
            set => _Greeting = this.RaiseAndSetIfChanged(ref _Greeting,value);
        }

        //demo code test .core project non mvvm
        public void InjectDll()
        {
            var injectedGame = new DarkSide.Core.Injector();
            var stuff = injectedGame.ToString();
        }

        public void LoadAutomation() => Greeting = "1";
    }
}
