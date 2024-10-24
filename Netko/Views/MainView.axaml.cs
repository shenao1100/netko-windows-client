using Avalonia.Animation;
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
using System.Reactive;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
namespace Netko.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        
        InitializeComponent();
        
  

    }
    //Init all pages
    UserControl Page_Netdisk = new NetdiskPage();
    UserControl Page_Home = new HomePage();
    UserControl Page_Transfer = new TransferPage();
    UserControl Page_Setting = new SettingPage();
    UserControl Page_Upload = new UploadPage();
    UserControl Page_History = new HistoryPage();
    UserControl Page_Transmit = new TransmitPage();
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
    public void Change(object sender, RoutedEventArgs e)
    {
        //Change page
        ContentPanel1.Children.Clear();
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
        Page_History.Opacity = 0;
        Page_Home.Opacity = 0;
        Page_Netdisk.Opacity = 0;
        Page_Transfer.Opacity = 0;  
        Page_Setting.Opacity = 0;
        Page_Upload.Opacity = 0;
        Page_Transmit.Opacity = 0;

        switch (clicked_button.Name.ToString())
        {
            case "Home":
                ContentPanel1.Children.Add(Page_Home);
                Page_Home.Opacity = 1;
                break;
            case "Manage":
                ContentPanel1.Children.Add(Page_Netdisk);
                Page_Netdisk.Opacity = 1;
                break;
            case "Transfer":
                ContentPanel1.Children.Add(Page_Transfer);
                Page_Transfer.Opacity = 1;
                break;
            case "Setting":
                ContentPanel1.Children.Add(Page_Setting);
                Page_Setting.Opacity = 1;
                break;
            case "Upload":
                ContentPanel1.Children.Add(Page_Upload);
                Page_Upload.Opacity = 1;
                break;
            case "History":
                ContentPanel1.Children.Add(Page_History);
                Page_History.Opacity = 1;
                break;
            case "Transmit":
                ContentPanel1.Children.Add(Page_Transmit);
                Page_Transmit.Opacity = 1;
                break;

        }
        return;
    }
}
