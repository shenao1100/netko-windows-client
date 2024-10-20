using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System.Diagnostics;
using System;

namespace Netko;

public partial class DirShowLine : UserControl
{
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
        Trace.WriteLine("Left mouse button clicked!");
        var currentTime = DateTime.Now;
        var timeDiff = currentTime - lastClickedTime;
        lastClickedTime = currentTime;
        if (timeDiff.Milliseconds <= 250)
        {
            Trace.WriteLine("Double clicked");
        }
        else
        {
            Trace.WriteLine("Left clicked");
        }
    }
}