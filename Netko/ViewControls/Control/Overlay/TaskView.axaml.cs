using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Netko;

public partial class TaskView : UserControl
{
    public TaskView()
    {
        InitializeComponent();
    }
    public void SetText(string? title = null, string? main_content = null, string? secondary_content = null)
    {
        if (title != null) { Title.Text = title; }
        if (main_content != null) { Content.Text = main_content; }
        if (secondary_content != null) { SecondaryContent.Text = secondary_content; }
    }
    public void SetProgress(int progress)
    {
        progress_bar.Value = progress;
    }
}