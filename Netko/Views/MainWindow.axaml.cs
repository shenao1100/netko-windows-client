﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;


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
            TitleBlock.Text = "Netko";
            this.BeginMoveDrag(e);
        }
    }
    public void SwitchShrink(object sender, RoutedEventArgs e)
    {
        this.FindControl<MainView>("MainviewPanel").SwitchShrink();
    }
    private void DragBar_MaxWin(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        // None Blur AcrylicBlur Mica Transparent
        TransparencyLevelHint = new WindowTransparencyLevel[] { WindowTransparencyLevel.AcrylicBlur };
        if (this.WindowState == WindowState.Normal)
        {

            this.WindowState = WindowState.Maximized;
        }
        else
        {
            this.WindowState = WindowState.Normal;

        }
        TitleBlock.Text = "(^=•ω•=^)~";
    }
}
