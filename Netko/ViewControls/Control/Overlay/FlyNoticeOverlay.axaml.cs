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
        // �ȴ�������ȫ����
        await Task.Delay(100);

        // ��ȡTranslateTransform����������
        var translateTransform = (TranslateTransform)this.RenderTransform;
        translateTransform.X = 0;
        translateTransform.Y = 0;
    }
}