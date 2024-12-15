using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Netko.NetDisk;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Netko;
public enum TaskStatusIndicate
{
    InQueue,
    Working,
    Done,
}

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
    public void SetTask(Func<string, Task<Netko.NetDisk.TaskStatus>> GetStatusFunc, string TaskID, Action? Callback=null) {
        Task.Run(() =>
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    Netko.NetDisk.TaskStatus status = GetStatusFunc(TaskID).Result;
                    string secondary_content = $"";
                    switch (status.Status)
                    {
                        case TaskStatusIndicate.InQueue:
                            secondary_content += "���ڶ�����";
                            break;
                        case TaskStatusIndicate.Working:
                            secondary_content += "���ڴ���";
                            break;
                        case TaskStatusIndicate.Done:
                            secondary_content += "�����";
                            break;
                        default:
                            secondary_content += "δ֪";
                            break;
                    }
                    secondary_content += $"\t{status.Progress.ToString()}%";
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SetText(secondary_content: secondary_content);
                        progress_bar.Value = status.Progress;

                    });
                    if (status.Status == TaskStatusIndicate.Done)
                    {
                        break;
                    }
                };
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            if (Callback != null)
            {
                Dispatcher.UIThread.InvokeAsync(() => { Callback(); });
            
            }
            
        });
        
    }
    public void SetProgress(int progress)
    {
        progress_bar.Value = progress;
    }
}