﻿using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Common.Interfaces;

namespace DarkSide.WPF.ViewModels;

public partial class TestViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty] private int _counter;

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }

    //[ICommand]
    private void OnCounterIncrement()
    {
        Counter++;
    }
}