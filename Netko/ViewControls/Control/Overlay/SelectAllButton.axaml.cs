using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk;
using Netko.NetDisk.Baidu;
using System.Collections.Generic;

namespace Netko;

public partial class SelectAllButton : UserControl
{
    public IFileList baiduFileList { get; set; }
    public FileList list_ { get; set; }
    // for toogle select
    public Dictionary<NetDir, ItemShowLine> DirDict = new Dictionary<NetDir, ItemShowLine>();
    public Dictionary<NetFile, ItemShowLine> FileDict = new Dictionary<NetFile, ItemShowLine>();
    public SelectAllButton()
    {
        InitializeComponent();
    }
    public bool IsSelectAll()
    {
        bool isSelectAll = true;
        foreach (NetDir dir in list_.Dir)
        {
            if (!baiduFileList.DirIsSelected(dir))
            {
                isSelectAll = false;
                break;
            }
        }
        foreach (NetFile file in list_.File)
        {
            if (isSelectAll && !baiduFileList.FileIsSelected(file))
            {
                isSelectAll = false;
                break;
            }
        }
        return isSelectAll;
    }

    public void ToogleSelect(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (IsSelectAll())
        {
            foreach (NetDir dir in list_.Dir)
            {
                DirDict[dir].ToogleSelect();
            }
            foreach (NetFile file in list_.File)
            {
                FileDict[file].ToogleSelect();
            }

        }
        else
        {
            foreach (NetDir dir in list_.Dir)
            {
                if (!baiduFileList.DirIsSelected(dir))
                {
                    DirDict[dir].ToogleSelect();
                }
            }
            foreach (NetFile file in list_.File)
            {
                if (!baiduFileList.FileIsSelected(file))
                {
                    FileDict[file].ToogleSelect();
                }
            }
        }
    }
}