using Avalonia.Controls;
using Avalonia.Threading;
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
    public void SetText(string? title = null, string? mainContent = null, string? secondaryContent = null)
    {
        if (title != null) { Title.Text = title; }
        if (mainContent != null) { Content.Text = mainContent; }
        if (secondaryContent != null) { SecondaryContent.Text = secondaryContent; }
    }
    public void SetTask(Func<string, Task<Netko.NetDisk.TaskStatus>> getStatusFunc, string taskId, Action? callback=null) {
        Task.Run(() =>
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    Netko.NetDisk.TaskStatus status = getStatusFunc(taskId).Result;
                    string secondaryContent = $"";
                    switch (status.Status)
                    {
                        case TaskStatusIndicate.InQueue:
                            secondaryContent += "已在队列中";
                            break;
                        case TaskStatusIndicate.Working:
                            secondaryContent += "正在处理";
                            break;
                        case TaskStatusIndicate.Done:
                            secondaryContent += "已完成";
                            break;
                        default:
                            secondaryContent += "δ֪";
                            break;
                    }
                    secondaryContent += $"\t{status.Progress.ToString()}%";
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SetText(secondaryContent: secondaryContent);
                        progress_bar.Value = status.Progress;

                    });
                    if (status.Status == TaskStatusIndicate.Done)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            if (callback != null)
            {
                Dispatcher.UIThread.InvokeAsync(() => { callback(); });
            
            }
            
        });
        
    }
    public void SetProgress(int progress)
    {
        progress_bar.Value = progress;
    }
}