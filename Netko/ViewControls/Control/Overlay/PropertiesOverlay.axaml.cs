using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using System;
using System.Threading.Tasks;
using Netko.NetDisk;

namespace Netko;

public partial class PropertiesOverlay : UserControl
{
    public BDDir selfDir {  get; set; }
    public BDFile selfFile { get; set; }
    public bool isFile = false;
    public PropertiesOverlay()
    {
        InitializeComponent();
    }
    private string FormatDate(long timeStamp)
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime dateTime = startTime.AddSeconds(timeStamp).ToLocalTime();
        return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
    }
    public void init()
    {
        if (isFile)
        {
            name_label.Content = selfFile.Name;
            path_label.Content = selfFile.Path;

            mtime_label.Content = FormatDate(selfFile.ServerMtime);
            ctime_label.Content = FormatDate(selfFile.ServerCtime);
            atime_label.Content = FormatDate(selfFile.ServerAtime);

            size_label.Content = DownloadProgress.FormatSize(selfFile.Size) + "\t(" + selfFile.Size.ToString() + " bytes)";
        }
        else
        {
            name_label.Content = selfDir.Name;
            path_label.Content = selfDir.Path;

            mtime_label.Content = FormatDate(selfDir.ServerMtime);
            ctime_label.Content = FormatDate(selfDir.ServerCtime);
            atime_label.Content = FormatDate(selfDir.ServerAtime);
            size_block.IsVisible = false;

        }
    }
    public void Show()
    {
        init(); 
        this.Opacity = 0;
        this.Opacity = 1;
    }

    private async void Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Opacity = 0;
        await Task.Delay(200);
        this.IsVisible = false;

    }
}