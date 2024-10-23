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
using System.Reflection;

namespace Netko;

public partial class SettingPage : UserControl
{
    public SettingPage()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        string AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Version not found";
        string AvaloniaVersion = typeof(Avalonia.Application).Assembly.GetName().Version.ToString() ?? "Version not found";

        // 获取版本信息

        VersionText.Text = "版本: " + AppVersion;
        AvaloniaVersionText.Text = "版本: " + AvaloniaVersion;

        var theme = Application.Current!.RequestedThemeVariant;
        if (theme == ThemeVariant.Default)
        {
            Default.IsChecked = true;
        }
        else if (theme == ThemeVariant.Light)
        {
            Light.IsChecked = true;
        }
        else if (theme == ThemeVariant.Dark)
        {
            Dark.IsChecked = true;
        }
        /*ThemeBox.SelectedItem = AppThemeUtils.CurrentTheme;
        ThemeBox.SelectionChanged += (sender, e) =>
        {
            
            if (ThemeBox.SelectedItem is ThemeType theme)
            {
                AppThemeUtils.SetTheme(theme);
            }
        };*/

        /*ThemeVariantsBox.SelectionChanged += (sender, e) =>
        {
            if (ThemeVariantsBox.SelectedItem is ThemeVariant themeVariant)
            {
                Application.Current!.RequestedThemeVariant = themeVariant;
            }
        };*/

    }
    private void ChangeTheme(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton) 
        { 
            switch (radioButton.Name)
            {
                case "Light":
                    //AppThemeUtils.SetTheme(ThemeType.Fluent);
                    Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
                    break;
                case "Dark":
                    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
                    break;

                case "Default":
                    Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
                    break;

            }
        }
        
    }
    public void ChangeDayNight(object sender, RoutedEventArgs e)
    {
        Trace.WriteLine(App.Current.RequestedThemeVariant.Key.ToString());
        // MainViewModel
        //App.NameProperty.
        //App.RequestedThemeVariantProperty.
    }

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        
    }

    private void Slider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        //thread_show.Text = Math.Round(slider.Value).ToString(); 
    }
}