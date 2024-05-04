using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
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

    public void Change(object sender, RoutedEventArgs e)
    {
        //Change page
        ContentPanel.Children.Clear();
        var clicked_button = sender as Button;
        if (clicked_button.Name == null)
        {
            return;
        }
        switch (clicked_button.Name.ToString())
        {
            case "Home":
                ContentPanel.Children.Add(Page_Home);
                break;
            case "Manage":
                ContentPanel.Children.Add(Page_Netdisk);
                break;
            case "Transfer":
                ContentPanel.Children.Add(Page_Transfer);
                break;
            case "Setting":
                ContentPanel.Children.Add(Page_Setting);
                break;
            case "Upload":
                ContentPanel.Children.Add(Page_Upload);
                break;
        }
        return;
    }
}
