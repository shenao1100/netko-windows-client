using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
namespace Netko;

public partial class LoginMainPage : UserControl
{
    public LoginMainPage()
    {
        InitializeComponent();
        
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start("./Executable/NetkoWebView.exe");
    }
}