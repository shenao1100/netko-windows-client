using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Netko;

public partial class NetdiskPage : UserControl
{
    public NetdiskPage()
    {
        InitializeComponent();
        UserControl FilePanel = new NetdiskFilePage();
        FileListPanel.Children.Add(FilePanel);
    }
}