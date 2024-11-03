using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using System.Collections.Generic;

namespace Netko;

public partial class NetdiskPathOverlay : UserControl
{
    // for history use
    private List<string> backHistory = new List<string>();
    private List<string> forwardHistory = new List<string>();
    private string currentPath;
    private BaiduFileList baiduFileList;

    public NetdiskPathOverlay()
    {
        InitializeComponent();
    }
}