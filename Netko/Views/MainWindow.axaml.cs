using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Netko.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //this.PointerPressed += MainWindow_PointerPressed;
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
        this.WindowState = WindowState.Maximized;
        TitleBlock.Text = string.Empty;
    }
}
