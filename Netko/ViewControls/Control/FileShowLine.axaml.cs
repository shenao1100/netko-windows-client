using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Netko;

public partial class FileShowLine : UserControl
{
    public FileShowLine()
    {
        InitializeComponent();
    }
    public void SetName(string name)
    {
        FileName.Content = name;
    }
}