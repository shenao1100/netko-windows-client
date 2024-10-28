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
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Netko;

public partial class DirShowLine : UserControl
{
    public Action Func;
    private StackPanel FileListViewer;
    public Color HoverBG;
    public Color LeaveBG;
    private DateTime lastClickedTime;
    public BaiduFileList? baiduFileList;
    public BDDir SelfDir;
    private bool is_selected = false;

    public DirShowLine()
    {
        InitializeComponent();
        lastClickedTime = DateTime.Now;
        Application.Current.ActualThemeVariantChanged += (sender, args) =>
        {
            UpdateColor();
        };
    }

    private void UpdateColor() 
    {
        /*var hover_backgound = Application.Current.Resources.TryGetResource("CatalogBaseHighColor", null, out var Hresource);
        if (hover_backgound && Hresource is Color Backgound)
        {
             HoverBG = Backgound;
        }
        var leave_background = Application.Current.Resources.TryGetResource("CatalogBaseMediumColor", null, out var Lresource);
        if (leave_background && Lresource is Color Background)
        {
            LeaveBG = Background;
        }*/
        if (is_selected)
        {
            Trace.WriteLine(HoverBG.ToString());
            //BorderBackground.Background = new SolidColorBrush(HoverBG);
            BorderBackground[!Border.BackgroundProperty] = new DynamicResourceExtension("CatalogBaseMediumColor");
        }
        else
        {
            Trace.WriteLine(LeaveBG.ToString());
            BorderBackground[!Border.BackgroundProperty] = new DynamicResourceExtension("CatalogBaseHighColor");
            //BorderBackground.Background = new SolidColorBrush(LeaveBG);
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

            if (baiduFileList != null && baiduFileList.GetSelectedItem() == null)
            {
                DockpanelOpen.IsVisible = true;
                DockpanelDuplicate.IsVisible = true;

            }
            else
            {
                DockpanelOpen.IsVisible = false;
                DockpanelDuplicate.IsVisible = false;
            }
        }
    }

    private void LeftClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var currentTime = DateTime.Now;
        var timeDiff = currentTime - lastClickedTime;
        lastClickedTime = currentTime;
        
        // Trace.WriteLine(timeDiff.TotalMilliseconds.ToString());
        if (timeDiff.TotalMilliseconds <= 250)
        {
            Func();
        }
        else
        {
            if (!baiduFileList.DirIsSelected(SelfDir))
            {
                is_selected = true;
            }
            else
            {
                is_selected = false;

            }
            UpdateColor();
            baiduFileList.ToggleSelectDir(SelfDir);
        }
    }

    private void DockPanel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Trace.WriteLine("dir rgclicked");
    }
}