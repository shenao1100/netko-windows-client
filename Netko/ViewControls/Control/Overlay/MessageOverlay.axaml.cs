using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Threading;
using System.Threading.Tasks;

namespace Netko;

public partial class MessageOverlay : UserControl
{
    public MessageOverlay()
    {
        InitializeComponent();
        Opacity = 0;
        Opacity = 1;
    }
    public void SetMessage(string title_text, string content_text)
    {
        title.Content = title_text;
        content.Content = content_text;
    }
    private async void Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Opacity = 0;
        await Task.Delay(200);
        this.IsVisible = false;

    }
}