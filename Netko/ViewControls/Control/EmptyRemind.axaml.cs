using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using System;

namespace Netko;

public partial class EmptyRemind : UserControl
{
    public Grid OverlayReservedGrid { get; set; }
    public BaiduFileList baiduFileList { get; set; }
    public Action Refresh { get; set; }
    public string ParentPath { get; set; }


    public EmptyRemind()
    {
        InitializeComponent();
    }

    private async void NewFolderOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogOverlay inputName = new DialogOverlay();
        OverlayReservedGrid.Children.Add(inputName);
        string? filename = await inputName.ShowDialog("请输入新建文件夹的名称", "创建");
        if (await baiduFileList.CreateDir(ParentPath + "/" + filename))
        {
            Refresh();
            return;
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            message.SetMessage("创建失败", $"创建{ParentPath + "/" + filename}时遇到错误");
            return;
        }
    }
}