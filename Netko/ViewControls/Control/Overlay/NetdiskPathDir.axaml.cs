using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Netko;

public partial class NetdiskPathDir : UserControl
{
    public BDDir selfDir {  get; set; }
    public BaiduFileList baiduFileList { get; set; }
    //public List<NetdiskPathDir> netdiskPathDirs { get; set; }
    public Button ExpandButton { get; set; }
    public Action<string, NetdiskPathDir> tooglePathCommand {  get; set; } 
    private bool isExpand = false;
    private bool isLoad = false;
    public NetdiskPathDir()
    {
        InitializeComponent();
        ExpandButton = Expander;
    }
    public void Show()
    {
        FolderName.Content = selfDir.Name;
    }
    private void ToogleExpand(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        tooglePathCommand(selfDir.Path, this);

        if (isExpand)
        {
            isExpand = false;
            ExpandPanel.IsVisible = false;
        }
        else
        {
            
            isExpand = true;
            ExpandPanel.IsVisible = true;
            if (!isLoad)
            {
                Expander.IsEnabled = false;
                GetChildContent(selfDir.Path);
                Expander.IsEnabled = true;
                isLoad = true;
            }
            
        }
    }
    private async void GetChildContent(string go_path)
    {
        
        BDFileList list_ = await baiduFileList.GetFileList(1, path: go_path, clear_select_list:false);
        foreach (BDDir dir_b in list_.Dir)
        {
            NetdiskPathDir netdiskPathDir = new NetdiskPathDir();
            netdiskPathDir.selfDir = dir_b;
            netdiskPathDir.baiduFileList = baiduFileList;
            netdiskPathDir.tooglePathCommand = tooglePathCommand;
            netdiskPathDir.Show();

            ExpandPanel.Children.Add(netdiskPathDir);

        }
        
    }

}