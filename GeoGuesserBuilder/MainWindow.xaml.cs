// SPDX-License-Identifier: GPL-3.0-only
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;
using GeoGuesserBuilder.Services;
using GeoGuesserBuilder.ViewModels;
using System.ComponentModel;

namespace GeoGuesserBuilder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var mainViewModel = new MainViewModel(
            new ProjectService(),
            new ERProcessService(),
            new MapEditorService(),
            new PackageModService());
        DataContext = mainViewModel;
    }

    private void Menu_Open_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.ImportProject();
    }

    private void Menu_Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.ExportProject();
    }

    private void Menu_Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        if (DataContext is MainViewModel vm)
        {
            await vm.ImportProjectAsync();
        }
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.ExportProjectAsync();

            if (vm is IDisposable disposable)
                disposable.Dispose();
        }

        base.OnClosing(e);
    }
}
