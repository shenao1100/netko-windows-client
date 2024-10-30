using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace Netko;

public partial class DialogOverlay : UserControl
{
    private TaskCompletionSource<string> _taskCompletionSource;
    public DialogOverlay()
    {
        InitializeComponent();
        this.Opacity = 0;

    }
    public Task<string> ShowDialog(string message, string button_message)
    {
        this.Opacity = 1;
        Message.Content = message;
        SendButton.Content = button_message;
        _taskCompletionSource = new TaskCompletionSource<string>();
        this.IsVisible = true;
        return _taskCompletionSource.Task;
    }

    private void Send(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _taskCompletionSource?.SetResult(Input.Text);
        this.IsVisible = false;
    }

}