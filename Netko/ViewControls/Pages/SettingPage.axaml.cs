using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;
using Avalonia.Styling;
using Netko.Views;
using System;
using System.Linq;
using System.Diagnostics;
using Netko.ViewModels;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia.Platform.Storage;

namespace Netko;

public partial class SettingPage : UserControl
{
    public SettingPage()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        string AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Version not found";
        string AvaloniaVersion = typeof(Avalonia.Application).Assembly.GetName().Version?.ToString() ?? "Version not found";

        // 获取版本信息

        VersionText.Text = "版本: " + AppVersion;
        AvaloniaVersionText.Text = "版本: " + AvaloniaVersion;

        string theme_setting = MeowSetting.GetTheme();
        switch (theme_setting)
        {
            case "Default":
                Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
                Default.IsChecked = true;
                break;
            case "Light":
                Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
                Light.IsChecked = true;
                break;
            case "Dark":
                Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
                Dark.IsChecked = true;
                break;

        }
        DownloadPath.Content = MeowSetting.GetDownloadPath();
        /*var theme = Application.Current!.RequestedThemeVariant;
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
        }*/
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
            MeowSetting.SetTheme(radioButton.Name);
        }
        
    }
    public void ChangeDayNight(object sender, RoutedEventArgs e)
    {
        Trace.WriteLine(App.Current.RequestedThemeVariant.Key.ToString());
        // MainViewModel
        //App.NameProperty.
        //App.RequestedThemeVariantProperty.
    }
    private async void SetDownloadPath(object sender, RoutedEventArgs e)
    {
        var storageProvider = TopLevel.GetTopLevel(this).StorageProvider;

        if (storageProvider.CanPickFolder)
        {
            var folder = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "选择文件夹"
            });

            if (folder != null && folder.Count > 0)
            {
                string folderPath = new Uri(folder[0].Path.ToString()).LocalPath;
                Trace.WriteLine("选择的文件夹路径: " + folderPath);
                DownloadPath.Content = folderPath;
                MeowSetting.SetDownloadPath(folderPath);
            }
        }
        else
        {
            Trace.WriteLine("当前平台不支持文件夹选择。");
        }
    
}
    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        
    }

    private void Slider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        //thread_show.Text = Math.Round(slider.Value).ToString(); 
    }
}