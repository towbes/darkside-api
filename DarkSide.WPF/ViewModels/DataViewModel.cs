using System;
using System.Collections.Generic;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DarkSide.WPF.Models;
using Wpf.Ui.Common.Interfaces;

namespace DarkSide.WPF.ViewModels;

public partial class DataViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty] private IEnumerable<DataColor> _colors;
    private bool _isInitialized;

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
        {
            InitializeViewModel();
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeViewModel()
    {
        var random = new Random();
        var colorCollection = new List<DataColor>();

        for (var i = 0; i < 8192; i++)
        {
            colorCollection.Add(
                new DataColor
                {
                    Color = new SolidColorBrush(
                        Color.FromArgb(
                            200,
                            (byte)random.Next(0, 250),
                            (byte)random.Next(0, 250),
                            (byte)random.Next(0, 250)))
                });
        }

        Colors = colorCollection;

        _isInitialized = true;
    }
}