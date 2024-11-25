using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using System.Collections.Generic;

namespace Netko;

public partial class SelectAllButton : UserControl
{
    public BaiduFileList baiduFileList { get; set; }
    public BDFileList list_ { get; set; }
    // for toogle select
    public Dictionary<BDDir, ItemShowLine> DirDict = new Dictionary<BDDir, ItemShowLine>();
    public Dictionary<BDFile, ItemShowLine> FileDict = new Dictionary<BDFile, ItemShowLine>();
    public SelectAllButton()
    {
        InitializeComponent();
    }
    public bool IsSelectAll()
    {
        bool isSelectAll = true;
        foreach (BDDir dir in list_.Dir)
        {
            if (!baiduFileList.DirIsSelected(dir))
            {
                isSelectAll = false;
                break;
            }
        }
        foreach (BDFile file in list_.File)
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
            foreach (BDDir dir in list_.Dir)
            {
                DirDict[dir].toogleSelect();
            }
            foreach (BDFile file in list_.File)
            {
                FileDict[file].toogleSelect();
            }

        }
        else
        {
            foreach (BDDir dir in list_.Dir)
            {
                if (!baiduFileList.DirIsSelected(dir))
                {
                    DirDict[dir].toogleSelect();
                }
            }
            foreach (BDFile file in list_.File)
            {
                if (!baiduFileList.FileIsSelected(file))
                {
                    FileDict[file].toogleSelect();
                }
            }
        }
    }
}