using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Threading.Tasks;
using System;
using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Styling;

namespace Netko;

public partial class FlyNoticeOverlay : UserControl
{
    public FlyNoticeOverlay()
    {
        InitializeComponent();
    }
    public async void Run(string content)
    {
        ContentLabel.Content = content;
        var RaiseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(3),
            Easing = new QuadraticEaseInOut(),
            Children = {
                new KeyFrame
                {
                Cue = new Cue(0),

                Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(7, 7, -200, 7)),
                        new Setter(UserControl.OpacityProperty, 0.3)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.1),
                    Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(7,7,7,7)),
                        new Setter(UserControl.OpacityProperty, 1.0)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.9),
                    Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(7,7,7,7)),
                        new Setter(UserControl.OpacityProperty, 1.0)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(7,7,-200,7)),
                        new Setter(UserControl.OpacityProperty, 0.0)
                    }
                }

            },

        };
        await RaiseAnimation.RunAsync(OutShell);
        
        this.IsVisible = false;
    }
}