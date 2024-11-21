using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using Netko.Download;
using System;
using System.Collections.Generic;
using Avalonia.Media;
using System.Diagnostics;

namespace Netko;

public partial class DownloadProgress : UserControl
{
    public Downloader DownloadInstance { get; set; }
    public Action ControlDestory { get; set; }

    public Action StatusUpdateCallback { get; set; }
    public DownloadProgress()
    {
        InitializeComponent();
    }
    /// <summary>
    /// Get Geometry svg from resource xaml
    /// </summary>
    /// <param name="resource_name">key for StreamGeometry you want</param>
    /// <returns></returns>
    private Geometry? TryGetGeometry(string resource_name)
    {
        var is_res_exist = Application.Current!.Resources.TryGetResource(resource_name, null, out var res);
        if (is_res_exist && res is Geometry geom)
        {
            return geom;
        }
        else
        {
            return null;
        }
    }
    public static string FormatSize(long size)
    {
        int times = 0;
        string unit = "bytes";
        double _size = Convert.ToDouble(size);
        while (_size > 1024)
        {
            times++;
            _size /= 1024;
        }
        switch (times)
        {
            case 0:
                unit = "bytes"; break;
            case 1:
                unit = "KB"; break;
            case 2:
                unit = "MB"; break;
            case 3:
                unit = "GB"; break;
            case 4:
                unit = "TB"; break;
            case 5:
                unit = "EB"; break;
            case 6:
                unit = "ZB"; break;
        }
        _size = double.Round(_size, 2);
        return _size.ToString() + unit;
    }

    public void updateDownloadProgress(Downloader downloadInstance)
    {
        progress_bar.Value = Convert.ToInt32(downloadInstance.downloadProgress * 100);
        description.Content = FormatSize(downloadInstance.downloaded) + "/" + FormatSize(downloadInstance.totalSize) + "\t" + float.Round(downloadInstance.downloadProgress*100, 2).ToString() + "%";
        if (downloadInstance.isPaused)
        {
            description.Content += "\t已暂停";
        }
        else
        {

            description.Content += $"\t线程数:{downloadInstance.downloadingThread.ToString()}";
            description.Content += "\t正在下载";

        }
        if (downloadInstance.downloaded == downloadInstance.totalSize || downloadInstance.isComplete)
        {
            ControlDestory();
        }
        updatePauseStatus();
    }
    private void Delete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DownloadInstance.Cancel();
    }
    private void updatePauseStatus()
    {
        if (DownloadInstance.isPaused)
        {
            toogle_pause.Data = (StreamGeometry)this.FindResource("continue");
        }
        else
        {
            toogle_pause.Data = (StreamGeometry)this.FindResource("pause");
        }
    }
    private void tooglePause(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StatusUpdateCallback();
        if (DownloadInstance.isPaused)
        { 
             toogle_pause.Data = (StreamGeometry)this.FindResource("pause");
             DownloadInstance.Continue();
        }
        else
        {
             toogle_pause.Data = (StreamGeometry)this.FindResource("continue");
             DownloadInstance.Pause();
        }
    }
    public void init(List<string> download_url, long size, string download_path, string user_agent, string name)
    {

        DownloadInstance = new Downloader(download_url[0], user_agent, download_path, size, 1);
        DownloadInstance.CallBack = (downloadIst) => { updateDownloadProgress(downloadIst); };
        filename.Content = name;
        foreach (var item in download_url)
        {
            DownloadInstance.AddUrl(item);
        }
        DownloadInstance.Run();
    }
}