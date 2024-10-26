using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System.Diagnostics;
using System;
using System.Runtime.CompilerServices;
using Netko.NetDisk.Baidu;

namespace Netko;

public partial class DirShowLine : UserControl
{
    public Action Func;
    private StackPanel FileListViewer;
    public Color HoverBG;
    public Color LeaveBG;
    private DateTime lastClickedTime;
    
    public DirShowLine()
    {
        InitializeComponent();

        var hover_backgound = Application.Current.Resources.TryGetResource("CatalogBaseHighColor", null, out var Hresource);
        if (hover_backgound && Hresource is Color Backgound)
        {
            HoverBG = Backgound;
        }
        var leave_background = Application.Current.Resources.TryGetResource("CatalogBaseHighColor", null, out var Lresource);
        if (leave_background && Lresource is Color Background)
        {
            LeaveBG = Background;
        }
        lastClickedTime = DateTime.Now;

    }
    public void SetName(string name)
    {
        FileName.Content = name;
    }
    private void MouseInput(object sender, PointerPressedEventArgs e)
    {
        var textBlock = sender as Button;
        var pointerPoint = e.GetCurrentPoint(this);

        // 检查哪个按键被按下
        if (pointerPoint.Properties.IsRightButtonPressed)
        {
            Trace.WriteLine("Right mouse button clicked!");
        }
    }

    private void LeftClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var currentTime = DateTime.Now;
        var timeDiff = currentTime - lastClickedTime;
        lastClickedTime = currentTime;
        
        Trace.WriteLine(timeDiff.TotalMilliseconds.ToString());
        if (timeDiff.TotalMilliseconds <= 250)
        {
            Func();
        }
        else
        { 

        }
    }

    private void DockPanel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Trace.WriteLine("dir rgclicked");
    }
}