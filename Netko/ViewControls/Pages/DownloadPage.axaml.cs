using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Netko.Download;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Netko;

public partial class TransferPage : UserControl
{
    public List<DownloadProgress> downloadTaskList = new List<DownloadProgress>();
    public TransferPage()
    {
        InitializeComponent();
    }

    public bool UpdateTooglePauseButton()
    {
        if (downloadTaskList.Count == 0)
        {
            return false;
        }
        foreach (DownloadProgress progress in downloadTaskList)
        {
            Trace.WriteLine(progress.DownloadInstance.Status().isDownloading.ToString());
            if (!progress.DownloadInstance.Status().isPaused)
            {
                pause_label.Content = "全部暂停";
                pause_icon.Data = (StreamGeometry)this.FindResource("pause")!;
                // all pause
                return false;
            }
        }
        pause_label.Content = "全部继续";
        pause_icon.Data = (StreamGeometry)this.FindResource("resume")!;

        // all resume
        return true;
    }
    public void RemoveAll(object sender, RoutedEventArgs e)
    {
        if (downloadTaskList.Count == 0)
        {
            return;

        }
        int count = downloadTaskList.Count;
        for (int i = 0; i < count; i++)
        {
            downloadTaskList[0].DownloadInstance.Cancel();
        }

    }
    public void tooglePauseAll(object sender, RoutedEventArgs e)
    {
        if (!UpdateTooglePauseButton())
        {
            foreach (DownloadProgress progress in downloadTaskList)
            {
                progress.DownloadInstance.Pause();
            }
        }
        else
        {
            foreach (DownloadProgress progress in downloadTaskList)
            {
                progress.DownloadInstance.Continue();
            }
        }
        

        UpdateTooglePauseButton();
    }
    public void destoryTask(DownloadProgress downloadProgress)
    {
        if (downloadTaskList.Contains(downloadProgress))
        {
            downloadTaskList.Remove(downloadProgress);
        }
        if (DownloadListContiner.Children.Contains(downloadProgress))
        { 
            DownloadListContiner.Children.Remove(downloadProgress);
        }
    }

    public void addTask(List<string> download_url, DownloadConfig downloadConfig)
    {
        DownloadProgress download_progress = new DownloadProgress();
        downloadTaskList.Add(download_progress);
        download_progress.ControlDestory = () => { destoryTask(download_progress); };
        download_progress.StatusUpdateCallback = () => {UpdateTooglePauseButton(); };
        download_progress.init(download_url, downloadConfig);
        DownloadListContiner.Children.Add(download_progress);
    }
}