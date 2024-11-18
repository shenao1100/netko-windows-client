using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
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

    public void addTask(List<string> download_url, long size, string download_path, string user_agent)
    {
        DownloadProgress download_progress = new DownloadProgress();
        download_progress.init(download_url, size, download_path, user_agent);
        DownloadListContiner.Children.Add(download_progress);
    }
}