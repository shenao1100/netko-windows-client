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


    public void destoryTask(DownloadProgress downloadProgress)
    {
        if (DownloadListContiner.Children.Contains(downloadProgress))
        { 
            DownloadListContiner.Children.Remove(downloadProgress);
        }
    }

    public void addTask(List<string> download_url, long size, string download_path, string user_agent, string name)
    {
        DownloadProgress download_progress = new DownloadProgress();
        download_progress.ControlDestory = () => { destoryTask(download_progress); };
        download_progress.init(download_url, size, download_path, user_agent, name);
        DownloadListContiner.Children.Add(download_progress);
    }
}