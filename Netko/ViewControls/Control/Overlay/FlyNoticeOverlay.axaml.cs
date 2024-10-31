using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Threading.Tasks;
using System;

namespace Netko;

public partial class FlyNoticeOverlay : UserControl
{
    public FlyNoticeOverlay()
    {
        InitializeComponent();
    }
    public async void OnWindowOpened()
    {
        // 等待窗口完全加载
        await Task.Delay(100);

        // 获取TranslateTransform并更改属性
        var translateTransform = (TranslateTransform)this.RenderTransform;
        translateTransform.X = 0;
        translateTransform.Y = 0;
    }
}