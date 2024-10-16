using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Netko.ViewModels;
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
        
        switch (clicked_button.Name.ToString())
        {
            case "Home":
                ContentPanel1.Children.Add(Page_Home);
                break;
            case "Manage":
                ContentPanel1.Children.Add(Page_Netdisk);
                break;
            case "Transfer":
                ContentPanel1.Children.Add(Page_Transfer);
                break;
            case "Setting":
                ContentPanel1.Children.Add(Page_Setting);
                break;
            case "Upload":
                ContentPanel1.Children.Add(Page_Upload);
                break;
            case "History":
                ContentPanel1.Children.Add(Page_History);
                break;
            case "Transmit":
                ContentPanel1.Children.Add(Page_Transmit);
                break;

        }
        return;
    }
}
