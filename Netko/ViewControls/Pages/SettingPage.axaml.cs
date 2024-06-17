using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;
using Avalonia.Styling;
using Netko.Views;
using System;
using System.Diagnostics;
using Netko.ViewModels;

namespace Netko;

public partial class SettingPage : UserControl
{
    public SettingPage()
    {
        InitializeComponent();
        ThemeBox.SelectedItem = AppThemeUtils.CurrentTheme;
        ThemeBox.SelectionChanged += (sender, e) =>
        {
            if (ThemeBox.SelectedItem is ThemeType theme)
            {
                AppThemeUtils.SetTheme(theme);
            }
        };

        ThemeVariantsBox.SelectionChanged += (sender, e) =>
        {
            if (ThemeVariantsBox.SelectedItem is ThemeVariant themeVariant)
            {
                Application.Current!.RequestedThemeVariant = themeVariant;
            }
        };

    }
    public void ChangeDayNight(object sender, RoutedEventArgs e)
    {
        Trace.WriteLine(App.Current.RequestedThemeVariant.Key.ToString());
        // MainViewModel
        //App.NameProperty.
        //App.RequestedThemeVariantProperty.
    }
    public void ChangeTheme(object sender, RoutedEventArgs e)
    {
        Trace.WriteLine(App.Current.RequestedThemeVariant.Key.ToString());
        //App.NameProperty.
        //App.RequestedThemeVariantProperty.
    }

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        
    }

    private void Slider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        thread_show.Text = Math.Round(slider.Value).ToString(); 
    }
}