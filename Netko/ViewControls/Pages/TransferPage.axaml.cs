using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace Netko;

public partial class TransferPage : UserControl
{
    public TransferPage()
    {
        InitializeComponent();
    }


    public void add_task_test(object sender, RoutedEventArgs e)
    {
        UserControl task_demo = new DownloadProgress();
        DownloadListContiner.Children.Add(task_demo);
    }


}