﻿using DarkSide.WPF.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace DarkSide.WPF.Views.Pages;

/// <summary>
///     Interaction logic for DataView.xaml
/// </summary>
public partial class DataPage : INavigableView<DataViewModel>
{
    public DataPage(DataViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }

    public DataViewModel ViewModel { get; }
}