using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Styling;
using Netko.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reactive;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
namespace Netko.Views;

public partial class MainView : UserControl
{
    NetdiskPage _netdiskPage = new NetdiskPage();
    HomePage _homePage = new HomePage();
    TransferPage _transferPage = new TransferPage();

    SettingPage _settingPage = new SettingPage();
    UploadPage _uploadPage = new UploadPage();
    HistoryPage _historyPage = new HistoryPage();
    TransmitPage _transmitPage = new TransmitPage();
    UserControl CurrentPage {  get; set; }


    
    public MainView()
    {
        
        InitializeComponent();
        ContentPanel1.Children.Add(_homePage);
        _homePage.Opacity = 1;

        
        Home.Background = new SolidColorBrush(Color.Parse("#30FFFFFF"));
        CurrentPage = _homePage;

        _netdiskPage.TransferPage = _transferPage;
        _netdiskPage.Init();

    }
    //Init all pages

    private void UnFold()
    {
        foreach (var button in SideButtonBar.Children)
        {
            if (button is Button)
            {
                button.Width =  130;
            }
        }
    }
    private void Shrink()
    {
        foreach (var button in SideButtonBar.Children)
        {
            if (button is Button)
            {
                button.Width = 50;
            }
        }
    }
    public void SwitchShrink()
    {
        if (Home.Width == 50)
        {
            UnFold();
        }
        else
        {
            Shrink();
        }
    }
    public async void Change(object sender, RoutedEventArgs e)
    {
        UserControl formerPage = CurrentPage;
        //Change page
        var clickedButton = sender as Button;
        if (clickedButton == null || clickedButton.Name == null)
        {
            return;
        }
        foreach (var button in SideButtonBar.Children)
        {
            if (button is Button)
            {
                Button button1 = (Button)button;
                button1.Background = new SolidColorBrush(Color.Parse("#00000000"));
            }
        }
        clickedButton.Background = new SolidColorBrush(Color.Parse("#30FFFFFF"));


        switch (clickedButton.Name.ToString())
        {
            case "Home":
                CurrentPage = _homePage;
                break;
            case "Manage":
                CurrentPage = _netdiskPage;
                break;
            case "Transfer":
                CurrentPage = _transferPage;
                break;
            case "Setting":
                CurrentPage = _settingPage;
                break;
            case "Upload":
                CurrentPage = _uploadPage;
                break;
            case "History":
                CurrentPage = _historyPage;
                break;
            case "Transmit":

                CurrentPage = _transmitPage;
                break;

        }
        if (formerPage == CurrentPage)
        {
            return;
        }
        ContentPanel1.Children.Clear();

        ContentPanel1.Children.Add(CurrentPage);
        var dropAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.25),
            Easing = new ExponentialEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(0,100,0,0)),
                        new Setter(UserControl.OpacityProperty, 0.0)
                    }

                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(0)),
                        new Setter(UserControl.OpacityProperty, 1.0)
                    }
                }
            }
        };
        await dropAnimation.RunAsync(ContentPanel1);
        CurrentPage.Margin = new Thickness(0);
        ContentPanel1.Margin = new Thickness(0);
        return;
    }
}
