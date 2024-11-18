using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
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
    NetdiskPage Page_Netdisk = new NetdiskPage();
    HomePage Page_Home = new HomePage();
    TransferPage Page_Transfer = new TransferPage();

    SettingPage Page_Setting = new SettingPage();
    UploadPage Page_Upload = new UploadPage();
    HistoryPage Page_History = new HistoryPage();
    TransmitPage Page_Transmit = new TransmitPage();
    UserControl CurrentPage {  get; set; }

    public MainView()
    {
        
        InitializeComponent();
        ContentPanel1.Children.Add(Page_Home);
        Page_Home.Opacity = 1;

        
        Home.Background = new SolidColorBrush(Color.Parse("#30FFFFFF"));
        CurrentPage = Page_Home;

        Page_Netdisk.TransferPage = Page_Transfer;
        Page_Netdisk.init();

    }
    //Init all pages

    public void unFold()
    {
        foreach (var button in SideButtonBar.Children)
        {
            if (button is Button)
            {
                button.Width =  130;
            }
        }
    }
    public void Shrink()
    {
        foreach (var button in SideButtonBar.Children)
        {
            if (button is Button)
            {
                button.Width = 50;
            }
        }
    }
    public void switchShrink()
    {
        if (Home.Width == 50)
        {
            unFold();
        }
        else
        {
            Shrink();
        }
    }
    public async void Change(object sender, RoutedEventArgs e)
    {
        UserControl FormerPage = CurrentPage;
        //Change page
        var clicked_button = sender as Button;
        if (clicked_button == null || clicked_button.Name == null)
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
        clicked_button.Background = new SolidColorBrush(Color.Parse("#30FFFFFF"));


        switch (clicked_button.Name.ToString())
        {
            case "Home":
                CurrentPage = Page_Home;
                break;
            case "Manage":
                CurrentPage = Page_Netdisk;
                break;
            case "Transfer":
                CurrentPage = Page_Transfer;
                break;
            case "Setting":
                CurrentPage = Page_Setting;
                break;
            case "Upload":
                CurrentPage = Page_Upload;
                break;
            case "History":
                CurrentPage = Page_History;
                break;
            case "Transmit":

                CurrentPage = Page_Transmit;
                break;

        }
        if (FormerPage == CurrentPage)
        {
            return;
        }
        ContentPanel1.Children.Clear();

        ContentPanel1.Children.Add(CurrentPage);
        var DropAnimation = new Animation
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
        await DropAnimation.RunAsync(ContentPanel1);
        CurrentPage.Margin = new Thickness(0);
        ContentPanel1.Margin = new Thickness(0);
        return;
    }
}
