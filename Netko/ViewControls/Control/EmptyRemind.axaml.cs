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
        string? filename = await inputName.ShowDialog("�������½��ļ��е�����", "����");
        if (await baiduFileList.CreateDir(ParentPath + "/" + filename))
        {
            Refresh();
            return;
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            message.SetMessage("����ʧ��", $"����{ParentPath + "/" + filename}ʱ��������");
            return;
        }
    }
}