using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using Netko.Download;
using System;
using System.Collections.Generic;

namespace Netko;

public partial class DownloadProgress : UserControl
{
    public Downloader DownloadInstance { get; set; }
    public DownloadProgress()
    {
        InitializeComponent();
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
        description.Content = FormatSize(downloadInstance.downloaded) + "/" + FormatSize(downloadInstance.totalSize) + "\t" + float.Round(downloadInstance.downloadProgress, 2).ToString() + "%";
    }

    public void init(List<string> download_url, long size, string download_path, string user_agent)
    {

        DownloadInstance = new Downloader(download_url[0], user_agent, download_path, size, 1);
        DownloadInstance.CallBack = (downloadIst) => { updateDownloadProgress(downloadIst); };
        foreach (var item in download_url)
        {
            DownloadInstance.AddUrl(item);
        }
        DownloadInstance.Run();
    }
}