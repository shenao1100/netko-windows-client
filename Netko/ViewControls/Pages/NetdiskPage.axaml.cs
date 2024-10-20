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
        Button bt = new Button();
        //UserSection.Children.Add()
        FileListGrid.Children.Add(FilePanel);
    }
}