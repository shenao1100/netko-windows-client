using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Netko.NetDisk.Baidu;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Netko;

public partial class NetdiskPathOverlay : UserControl
{

    private string currentPath { get; set; }
    public BaiduFileList baiduFileList { get; set; }

    private TaskCompletionSource<string> _taskCompletionSource;

    private NetdiskPathDir? selectPathControl {  get; set; }
    public NetdiskPathOverlay()
    {
        InitializeComponent();
    }
    
    public Task<string> ShowDialog(string message, string button_message)
    {
        this.Opacity = 0;
        this.Opacity = 1;
        title.Content = message;
        SendButton.Content = button_message;
        _taskCompletionSource = new TaskCompletionSource<string>();
        this.IsVisible = true;
        currentPath = "/";
        return _taskCompletionSource.Task;
    }

    private void Send(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _taskCompletionSource?.SetResult(currentPath);
        this.IsVisible = false;
    }
    private async void Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Opacity = 0;
        await Task.Delay(200);
        this.IsVisible = false;

    }
    private void toogleSelect(string target_path, NetdiskPathDir dir_control)
    {
        if (selectPathControl != null)
        {
            selectPathControl.ExpandButton.Background = new SolidColorBrush(Color.Parse("#00000000"));

        }
        if (currentPath == target_path)
        {
            currentPath = "/";
            selectPathControl = null;
        }
        else
        {
            Trace.WriteLine("selected");
            currentPath = target_path;
            selectPathControl = dir_control;
            selectPathControl.ExpandButton.Background = new SolidColorBrush(Color.Parse("#30FFFFFF"));

}
        Trace.WriteLine(currentPath);
    }
    public async void ShowInitDir()
    {

        BDFileList list_ = await baiduFileList.GetFileList(1, path: "/", clear_select_list:false);
        foreach (BDDir dir_b in list_.Dir)
        {
            NetdiskPathDir netdiskPathDir = new NetdiskPathDir();
            netdiskPathDir.baiduFileList = baiduFileList;
            netdiskPathDir.selfDir = dir_b;
            netdiskPathDir.tooglePathCommand = this.toogleSelect;
            netdiskPathDir.Show();
            FileListViewer.Children.Add(netdiskPathDir);
        }
    }

}