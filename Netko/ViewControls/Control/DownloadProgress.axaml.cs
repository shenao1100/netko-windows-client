using Avalonia;
using Avalonia.Controls;
using Netko.Download;
using System;
using System.Collections.Generic;
using Avalonia.Media;
using System.Diagnostics;

namespace Netko;

public partial class DownloadProgress : UserControl
{
    public IDownload DownloadInstance { get; set; }
    public Action ControlDestory { get; set; }

    public Action StatusUpdateCallback { get; set; }
    public DownloadProgress()
    {
        InitializeComponent();
    }
    /// <summary>
    /// Get Geometry svg from resource xaml
    /// </summary>
    /// <param name="resourceName">key for StreamGeometry you want</param>
    /// <returns></returns>
    private Geometry? TryGetGeometry(string resourceName)
    {
        var isResExist = Application.Current!.Resources.TryGetResource(resourceName, null, out var res);
        if (isResExist && res is Geometry geom)
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
        double calcSize = Convert.ToDouble(size);
        while (calcSize > 1024)
        {
            times++;
            calcSize /= 1024;
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
        calcSize = double.Round(calcSize, 2);
        return calcSize.ToString() + unit;
    }

    private void UpdateDownloadProgress(IDownload downloadInstance)
    {
        progress_bar.Value = Convert.ToInt32(downloadInstance.Status().DownloadProgress * 100);
        description.Content = FormatSize(downloadInstance.Status().Downloaded) + "/" + FormatSize(downloadInstance.Status().TotalSize) + "\t" + float.Round(downloadInstance.Status().DownloadProgress*100, 2).ToString() + "%";
        if (downloadInstance.Status().IsParsing)
        {
            description.Content += "\t正在解析";

        }
        else if (downloadInstance.Status().IsPaused)
        {
            description.Content += "\t已暂停";

        }
        else
        {
            description.Content += $"\t线程数:{downloadInstance.Status().DownloadingThread.ToString()}";
            description.Content += "\t正在下载";

        }
        if (downloadInstance.Status().Downloaded == downloadInstance.Status().TotalSize || downloadInstance.Status().IsComplete)
        {
            ControlDestory();
        }
        UpdatePauseStatus();
    }
    private void Delete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DownloadInstance.Cancel();
    }
    private void UpdatePauseStatus()
    {
        if (DownloadInstance.Status().IsPaused)
        {
            toogle_pause.Data = (StreamGeometry)this.FindResource("continue");
        }
        else
        {
            toogle_pause.Data = (StreamGeometry)this.FindResource("pause");
        }
    }
    private void TooglePause(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        
        if (DownloadInstance.Status().IsPaused)
        { 
             toogle_pause.Data = (StreamGeometry)this.FindResource("pause");
             DownloadInstance.Continue();
        }
        else
        {
             toogle_pause.Data = (StreamGeometry)this.FindResource("continue");
             DownloadInstance.Pause();
        }
        StatusUpdateCallback();
    }
    public void Init(List<string>? downloadUrl, DownloadConfig downloadConfig)
    {

        DownloadInstance = DownloadFactory.Create(downloadConfig);// new Downloader(download_url[0], "netdisk;P2SP;2.2.101.200;netdisk;12.17.2;PGEM10;android-android;9;JSbridge4.4.0;jointBridge;1.1.0;", download_path, size, 5, cookie);
        Trace.WriteLine(downloadConfig.Url);
        DownloadInstance.SetCallBack(() => { UpdateDownloadProgress(DownloadInstance); });
        filename.Content = downloadConfig.FileName;
        if (downloadUrl != null)
        {
            foreach (var item in downloadUrl)
            {
                DownloadInstance.AddUrl(item);
            }
        }
        DownloadInstance.Run();
    }
}