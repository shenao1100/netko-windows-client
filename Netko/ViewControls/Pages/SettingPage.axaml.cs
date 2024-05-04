using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

namespace Netko;

public partial class SettingPage : UserControl
{
    public SettingPage()
    {
        InitializeComponent();
    }
    public void ChangeDayNight(object sender, RoutedEventArgs e)
    {
        Trace.WriteLine(App.Current.RequestedThemeVariant.Key.ToString());
        App.Current.RequestedThemeVariant.
    }
}