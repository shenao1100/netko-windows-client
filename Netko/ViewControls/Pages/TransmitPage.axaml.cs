using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Netko;

public partial class TransmitPage : UserControl
{
    UserControl PageDownload = new TransferPage();
    public TransmitPage()
    {
        InitializeComponent();
    }
    public void add_task_test(object sender, RoutedEventArgs e)
    {
        UserControl task_demo = new DownloadProgress();
        DownloadListContiner.Children.Add(task_demo);
    }

    public void download_page(object sender, RoutedEventArgs e)
    {
        DownloadListContiner.Children.Clear();

        DownloadListContiner.Children.Add(PageDownload);
    }
}