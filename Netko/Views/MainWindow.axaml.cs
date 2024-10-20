using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Netko.Views;

public partial class MainWindow : Window
{
    //IReadOnlyList<WindowTransparencyLevel> TransparencyLevelHint { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        //this.PointerPressed += MainWindow_PointerPressed;
        //TransparencyLevelHint = new WindowTransparencyLevel[] { WindowTransparencyLevel.AcrylicBlur };

    }

    private void DragBar_OnDrag(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {

            this.BeginMoveDrag(e);
        }
    }

    private void DragBar_MaxWin(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        // None Blur AcrylicBlur Mica Transparent
        //TransparencyLevelHint = new WindowTransparencyLevel[] { WindowTransparencyLevel.AcrylicBlur };


        this.WindowState = WindowState.Maximized;
        TitleBlock.Text = string.Empty;
    }
}
