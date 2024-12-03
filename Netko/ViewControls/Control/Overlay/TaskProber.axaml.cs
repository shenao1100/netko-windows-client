using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Netko;

public partial class TaskProber : UserControl
{
    private List<TaskView> taskViews = new List<TaskView>();

    public TaskView AddTask()
    {
        TaskView task_view = new TaskView();
        taskViews.Add(task_view);
        TaskViewPanel.Children.Add(task_view);
        return task_view;
    }
    public async void Close()
    {
        MainFrame.Width = 0.0;
        var CloseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.25),
            Easing = new ExponentialEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(UserControl.WidthProperty, 300.0)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(UserControl.WidthProperty, 0.0)
                    }
                }
            }
        };
        await CloseAnimation.RunAsync(MainFrame);
    }

    public async void Open()
    {
        MainFrame.Width = 300.0;

        var CloseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.25),
            Easing = new ExponentialEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(UserControl.WidthProperty, 0.0)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(UserControl.WidthProperty, 300.0)
                    }
                }
            }
        };
        await CloseAnimation.RunAsync(MainFrame);
    }

    private void CloseTaskProber(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            Close();

        }catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }
    public TaskProber()
    {
        InitializeComponent();
    }
}