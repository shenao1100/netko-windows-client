using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Interactivity;
using Avalonia.Media;
using Netko.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
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

    public void Change(object sender, RoutedEventArgs e)
    {
        //Change page
        ContentPanel1.Children.Clear();
        var clicked_button = sender as Button;
        if (clicked_button.Name == null)
        {
            return;
        }
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
        }
        return;
    }
}
