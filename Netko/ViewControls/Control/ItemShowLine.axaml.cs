using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System.Diagnostics;
using System;
using System.Runtime.CompilerServices;
using Netko.NetDisk.Baidu;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System.Xml.Serialization;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using DynamicData;
using Netko.NetDisk;
using Netko.Download;

namespace Netko;
public static class ImageHelper
{
    public static Bitmap LoadFromResource(Uri resourceUri)
    {
        return new Bitmap(AssetLoader.Open(resourceUri));
    }

    public static async Task<Bitmap?> LoadFromWeb(Uri url)
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
            return null;
        }
    }
}
public partial class ItemShowLine : UserControl
{
    public Action Func {  get; set; }
    public Action Refresh {  get; set; }
    public string ParentPath { get; set; }
    private StackPanel FileListViewer;
    //public Color HoverBG;
    //public Color LeaveBG;
    private DateTime lastClickedTime;
    public IFileList baiduFileList {  get; set; }

    public BDDir SelfDir;
    public BDFile SelfFile;
    public bool isFile;
    public bool isDir;
    private bool is_selected = false;
    private bool single_menu_operation = true;
    public Grid OverlayReservedGrid { get; set; }
    public StackPanel OverlayNotification {  get; set; }
    public TransferPage TransferPage { get; set; }
    public TaskProber taskProber { get; set; }
    public ItemShowLine()
    {
        InitializeComponent();
        lastClickedTime = DateTime.Now;
        Application.Current.ActualThemeVariantChanged += (sender, args) =>
        {
            UpdateColor();
        };
    }

    private void UpdateColor() 
    {
        /*var hover_backgound = Application.Current.Resources.TryGetResource("CatalogBaseHighColor", null, out var Hresource);
        if (hover_backgound && Hresource is Color Backgound)
        {
             HoverBG = Backgound;
        }
        var leave_background = Application.Current.Resources.TryGetResource("CatalogBaseMediumColor", null, out var Lresource);
        if (leave_background && Lresource is Color Background)
        {
            LeaveBG = Background;
        }*/
        if (is_selected)
        {
            //BorderBackground.Background = new SolidColorBrush(HoverBG);
            BorderBackground[!Border.BackgroundProperty] = new DynamicResourceExtension("HoverColor");
        }
        else
        {
            BorderBackground[!Border.BackgroundProperty] = new DynamicResourceExtension("CatalogBaseHighColor");
            //BorderBackground.Background = new SolidColorBrush(LeaveBG);
        }

    }
    private string FormatDate(long timeStamp)
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime dateTime = startTime.AddSeconds(timeStamp).ToLocalTime();
        return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
    }

    /// <summary>
    /// Init item, set type and icon
    /// </summary>
    /// <param name="name">file name</param>
    /// <param name="is_dir"></param>
    public void Init(string name, bool is_dir)
    {
        Uri uri;
        if (is_dir)
        {
            isDir = true;
            isFile = false;
            uri = new Uri("avares://Netko/Assets/FileIcon/icons8-folder-188.png");
        }
        else
        {

            DockpanelOpen.IsVisible = false;
            isDir = false;
            isFile = true;
            uri = new Uri("avares://Netko/Assets/FileIcon/icons8-document-188.png");
        }
        ItemIcon.Source = ImageHelper.LoadFromResource(uri);
        FileName.Content = name;
        if (is_dir)
        {

            detail_label.Content = $"{FormatDate(SelfDir.ServerCtime)}";
        }
        else
        {
            detail_label.Content = $"{FormatDate(SelfFile.ServerCtime)}\t{DownloadProgress.FormatSize(SelfFile.Size)}";

        }
    }

    public void RefreshCallback(string pervious_path)
    {
        if (pervious_path == ParentPath)
        {
            Refresh();
        }
    }
    private void SetMultiOperationCommand()
    {
        if (!single_menu_operation) { return; }

        //DockpanelOpen.Click -= OepnOnMenu;
        DockpanelDelete.Click -= DeleteOnMenu;
        DockpanelMove.Click -= MoveOnMenu;
        DockpanelDuplicate.Click -= CopyOnMenu;
        DockpanelShare.Click -= ShareOnMenu;

        DockpanelDelete.Click += MultiDeleteOnMenu;
        DockpanelMove.Click += MultiMoveOnMenu;
        DockpanelDuplicate.Click += MultiCopyOnMenu;
        DockpanelShare.Click += MultiShareOnMenu;
        single_menu_operation = false;
    }
    private void SetSingleOperationCommand() 
    {
        if (single_menu_operation) { return; }
        DockpanelDelete.Click -= MultiDeleteOnMenu;
        DockpanelMove.Click -= MultiMoveOnMenu;
        DockpanelDuplicate.Click -= MultiCopyOnMenu;
        DockpanelShare.Click -= MultiShareOnMenu;

        DockpanelDelete.Click += DeleteOnMenu;
        DockpanelMove.Click += MoveOnMenu;
        DockpanelDuplicate.Click += CopyOnMenu;
        DockpanelShare.Click += ShareOnMenu;
        single_menu_operation = true;
    } 

    /// <summary>
    /// right click to open menu, has been abandoned
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RightClick(object sender, PointerPressedEventArgs e)
    {
        var pointerPoint = e.GetCurrentPoint(this);
        // 检查哪个按键被按下
        if (!pointerPoint.Properties.IsRightButtonPressed)
        {
            return;
        }
        if (isFile)
        {
            BDFileList filelist = baiduFileList.GetSelectedItem();
            if (filelist.File == null && filelist.Dir == null || !filelist.File!.Contains(SelfFile))
            {
                // single select
                SetSingleOperationCommand();
            }
            else if (filelist.File!.Contains(SelfFile) && filelist.File!.Count() == 1 && filelist.Dir.Count() == 0)
            {
                //single select
                SetSingleOperationCommand();
            }
            else
            {
                //multi file
                SetMultiOperationCommand();
            }
        }
        if (isDir)
        {
            BDFileList filelist = baiduFileList.GetSelectedItem();
            if (filelist.File == null && filelist.Dir == null || !filelist.Dir!.Contains(SelfDir))
            {
                // single select
                SetSingleOperationCommand();

            }
            else if (filelist.Dir!.Contains(SelfDir) && filelist.Dir!.Count() == 1 && filelist.File!.Count() == 0)
            {
                //single select
                SetSingleOperationCommand();

            }
            else
            {
                //multi file
                SetMultiOperationCommand();

            }
        }
    }
    public void toogleSelect()
    {
        if (isDir)
        {
            is_selected = (!baiduFileList.DirIsSelected(SelfDir)) ? true : false;
            baiduFileList.ToggleSelectDir(SelfDir);

        }
        if (isFile)
        {
            is_selected = (!baiduFileList.FileIsSelected(SelfFile)) ? true : false;
            baiduFileList.ToggleSelectFile(SelfFile);
        }
        UpdateColor();
    }

        /// <summary>
        /// left click and double click, toggle select
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
    private async void LeftClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var currentTime = DateTime.Now;
        var timeDiff = currentTime - lastClickedTime;
        lastClickedTime = currentTime;
        
        if (timeDiff.TotalMilliseconds <= 250)
        {
            if (isDir)
            {
                Func();

            }
            if (isFile)
            {

                /*FlyNoticeOverlay flyNoticeOverlay = new FlyNoticeOverlay();
                OverlayNotification.Children.Add(flyNoticeOverlay);
                flyNoticeOverlay.Run($"{SelfFile.Name} 已添加进下载队列");
                try
                {
                    List<string> url_list = await baiduFileList.GetFileDownloadLink(SelfFile.Path);
                    DownloadConfig downloadConfig = baiduFileList.ChooseDownloadMethod();
                    IDownload download = null;
                    TransferPage.addTask(url_list, SelfFile.Size, MeowSetting.GetDownloadPath() + "\\" + SelfFile.Name, "netdisk;P2SP;3.0.20.63;netdisk;7.46.5.113;PC;PC-Windows;10.0.22631;WindowsBaiduYunGuanJia", SelfFile.Name, baiduFileList.GetAccountInfo().InitCookie);

                }
                catch (Exception ex)
                {
                    FlyNoticeOverlay err = new FlyNoticeOverlay();

                    err.Run($"{SelfFile.Name}下载出错：{ex}");

                }*/
            }
        }
        else
        {
            toogleSelect();
        }
    }
    /// <summary>
    /// Menu funcion: open
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OepnOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Func();
    }

    private async void NewFolderOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogOverlay inputName = new DialogOverlay();
        OverlayReservedGrid.Children.Add(inputName);
        string? filename = await inputName.ShowDialog("请输入新建文件夹的名称", "创建");
        if ((await baiduFileList.CreateDir(ParentPath + "/" + filename)).Success)
        {
            Refresh();
            return;
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            message.SetMessage("创建失败", $"创建{ParentPath + "/" + filename}时遇到错误");
            return;
        }
    }
    /// <summary>
    /// Menu funcion: copy
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CopyOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NetdiskPathOverlay netdiskPathOverlay = new NetdiskPathOverlay();
        netdiskPathOverlay.baiduFileList = baiduFileList;
        netdiskPathOverlay.ShowInitDir();
        OverlayReservedGrid.Children.Add(netdiskPathOverlay);
        string? target_path = await netdiskPathOverlay.ShowDialog("请选择目标文件夹", "复制");
        string self_path, self_name;
        if (isDir)
        {
            self_name = SelfDir.Name;
            self_path = SelfDir.Path;
        }
        else
        {
            self_name = SelfFile.Name;
            self_path = SelfFile.Path;
        }
        if (!string.IsNullOrEmpty(target_path))
        {
            NetdiskResult netdiskResult = await baiduFileList.Copy([self_path], [self_name], [target_path], isAsync: true);
            if (netdiskResult.Success && netdiskResult.TaskID != null)
            {
                if ((await baiduFileList.GetProgress(netdiskResult.TaskID)).Status == TaskStatusIndicate.Done)
                {
                    Refresh();
                    return;
                }
                TaskView task_view = taskProber.AddTask();
                task_view.SetText("复制文件", $"复制{self_name}到{target_path}", "请稍后\t0%");
                try
                {
                    string current_path = ParentPath;
                    task_view.SetTask(baiduFileList.GetProgress, netdiskResult.TaskID, () => { RefreshCallback(current_path); });

                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
            else
            {
                MessageOverlay message = new MessageOverlay();
                OverlayReservedGrid.Children.Add(message);
                string errMsg = string.Empty;
                errMsg += (netdiskResult.Msg != null) ? netdiskResult.Msg : "未知错误";
                errMsg += $"\t 错误码: {netdiskResult.ResultID.ToString()}";
                message.SetMessage("创建失败", errMsg);
            }
            //Refresh();
            return;
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            message.SetMessage("创建失败", $"移动文件{self_path}时遇到错误");
            return;
        }
    }
    /// <summary>
    /// Menu funcion: move
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MoveOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NetdiskPathOverlay netdiskPathOverlay = new NetdiskPathOverlay();
        netdiskPathOverlay.baiduFileList = baiduFileList;
        netdiskPathOverlay.ShowInitDir();
        OverlayReservedGrid.Children.Add(netdiskPathOverlay);
        string? target_path = await netdiskPathOverlay.ShowDialog("请选择目标文件夹", "移动");
        string self_path, self_name;
        if (isDir)
        {
            self_name = SelfDir.Name;
            self_path = SelfDir.Path;
        }
        else
        {
            self_name = SelfFile.Name;
            self_path = SelfFile.Path;
        }
        if (!string.IsNullOrEmpty(target_path)) {
            NetdiskResult netdiskResult = await baiduFileList.Move([self_path], [self_name], [target_path], isAsync: true);
            if (netdiskResult.Success && netdiskResult.TaskID != null)
            {
                if ((await baiduFileList.GetProgress(netdiskResult.TaskID)).Status == TaskStatusIndicate.Done)
                {
                    Refresh();
                    return;
                }
                TaskView task_view = taskProber.AddTask();
                task_view.SetText("移动文件", $"移动{self_name}到{target_path}", "请稍后\t0%");
                try
                {
                    string current_path = ParentPath;
                    task_view.SetTask(baiduFileList.GetProgress, netdiskResult.TaskID, () => { RefreshCallback(current_path); });

                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
            else
            {
                MessageOverlay message = new MessageOverlay();
                OverlayReservedGrid.Children.Add(message);
                string errMsg = string.Empty;
                errMsg += (netdiskResult.Msg != null) ? netdiskResult.Msg : "未知错误";
                errMsg += $"\t 错误码: {netdiskResult.ResultID.ToString()}";
                message.SetMessage("创建失败", errMsg);
            }
            //Refresh();
            return;
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            message.SetMessage("创建失败", $"移动文件{self_path}时遇到错误");
            return;
        }
    }
    /// <summary>
    /// Menu funcion: rename
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RenameOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogOverlay inputName = new DialogOverlay();
        OverlayReservedGrid.Children.Add(inputName);
        string? filename = await inputName.ShowDialog("请输入文件的新名称", "重命名", place_holder:SelfDir.Name);
        string self_path;
        if (isDir)
        {
            self_path = SelfDir.Path;
        }
        else
        {
            self_path = SelfFile.Path;
        }

        NetdiskResult netdiskResult = await baiduFileList.Rename([self_path], [filename], isAsync: true);
        if (netdiskResult.Success && netdiskResult.TaskID != null)
        {
            if ((await baiduFileList.GetProgress(netdiskResult.TaskID)).Status == TaskStatusIndicate.Done)
            {
                Refresh();
                return;
            }
            TaskView task_view = taskProber.AddTask();
            task_view.SetText("重命名文件", $"正在重命名{self_path}", "请稍后\t0%");
            try
            {
                string current_path = ParentPath;
                task_view.SetTask(baiduFileList.GetProgress, netdiskResult.TaskID, () => { RefreshCallback(current_path); });

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            string errMsg = string.Empty;
            errMsg += (netdiskResult.Msg != null) ? netdiskResult.Msg : "未知错误";
            errMsg += $"\t 错误码: {netdiskResult.ResultID.ToString()}";
            message.SetMessage("创建失败", errMsg);
        }
        //Refresh();
        return;

    }
    /// <summary>
    /// Menu funcion: delete
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeleteOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string delete_filelist, self_name;
        int count = 0;
        if (isDir)
        {
            count++;
            BDDir[] dirlist = new BDDir[1];
            dirlist[0] = SelfDir;
            delete_filelist = baiduFileList.IntegrateFilelist(null, dirlist);
            self_name = SelfDir.Name;
        }
        else
        {
            count++;
            BDFile[] filelist = new BDFile[1];
            filelist[0] = SelfFile;
            delete_filelist = baiduFileList.IntegrateFilelist(filelist, null);
            self_name = SelfFile.Name;
        }


        NetdiskResult netdiskResult = await baiduFileList.DeleteFile(delete_filelist, isAsync: true);
        if (netdiskResult.Success && netdiskResult.TaskID != null)
        {
            if ((await baiduFileList.GetProgress(netdiskResult.TaskID)).Status == TaskStatusIndicate.Done)
            {
                Refresh();
                return;
            }
            TaskView task_view = taskProber.AddTask();
            task_view.SetText("删除文件", $"正在删除{count.ToString()}个项目", "请稍后\t0%");
            try
            {
                string current_path = ParentPath;
                task_view.SetTask(baiduFileList.GetProgress, netdiskResult.TaskID, () => { RefreshCallback(current_path); });

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            string errMsg = string.Empty;
            errMsg += (netdiskResult.Msg != null) ? netdiskResult.Msg : "未知错误";
            errMsg += $"\t 错误码: {netdiskResult.ResultID.ToString()}";
            message.SetMessage("创建失败", errMsg);
        }
        //Refresh();
        return;


    }
    private void DockPanel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Trace.WriteLine("dir rgclicked");
    }
    /// <summary>
    /// Menu funcion: share
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ShareOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ShareLinkOverlay shareLinkOverlay = new ShareLinkOverlay();
        shareLinkOverlay.baiduFileList = baiduFileList;
        shareLinkOverlay.FileList = null;
        if (isDir)
        {
            shareLinkOverlay.DirList = new BDDir[] { SelfDir };
        }
        else
        {
            shareLinkOverlay.FileList = new BDFile[] { SelfFile };
        }
        shareLinkOverlay.Opacity = 0;

        OverlayReservedGrid.Children.Add(shareLinkOverlay);
        shareLinkOverlay.Opacity = 1;
    }

    private void PropertiesOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PropertiesOverlay propertiesOverlay = new PropertiesOverlay();
        if (isDir)
        {
            propertiesOverlay.selfDir = SelfDir;
            propertiesOverlay.isFile = false;

        }
        else
        {
            propertiesOverlay.selfFile = SelfFile;
            propertiesOverlay.isFile = true;
        }

        OverlayReservedGrid.Children.Add(propertiesOverlay);
        propertiesOverlay.Show();
    }
    private async void DownloadOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        FlyNoticeOverlay flyNoticeOverlay = new FlyNoticeOverlay();
        OverlayNotification.Children.Add(flyNoticeOverlay);
        if (isFile)
        {
            flyNoticeOverlay.Run($"{SelfFile.Name} 已添加进下载队列");
        }
        else
        {
            flyNoticeOverlay.Run($"文件夹暂不支持下载");
            return;
        }
        try
        {
            List<string> url_list = await baiduFileList.GetFileDownloadLink(SelfFile.Path);
            DownloadConfig downloadConfig = baiduFileList.ChooseDownloadMethod();
            downloadConfig.FileName = SelfFile.Name;
            downloadConfig.FilePath = FilePathOperate.GetAvailablePath(sub_path:null, file_name: SelfFile.Name);
            downloadConfig.FileSize = SelfFile.Size;
            downloadConfig.Url = url_list[0];
            url_list.Remove(url_list[0]);
            TransferPage.addTask(url_list, downloadConfig);
        }
        catch (Exception ex)
        {
            FlyNoticeOverlay err = new FlyNoticeOverlay();

            err.Run($"{SelfFile.Name}下载出错：{ex}");

        }
    }
    /*
     * ==========================
     *      Multi file operation
     * ==========================
     */

    /// <summary>
    /// Menu funcion: share
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MultiShareOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ShareLinkOverlay shareLinkOverlay = new ShareLinkOverlay();
        shareLinkOverlay.baiduFileList = baiduFileList;

        shareLinkOverlay.DirList = baiduFileList.GetSelectedItem().Dir;
        shareLinkOverlay.FileList = baiduFileList.GetSelectedItem().File;
        shareLinkOverlay.Opacity = 0;

        OverlayReservedGrid.Children.Add(shareLinkOverlay);
        shareLinkOverlay.Opacity = 1;
    }
    /// <summary>
    /// Menu funcion: delete
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MultiDeleteOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string delete_filelist;

        BDDir[] dirlist = baiduFileList.GetSelectedItem().Dir;
        BDFile[] filelist = baiduFileList.GetSelectedItem().File;
        delete_filelist = baiduFileList.IntegrateFilelist(filelist, dirlist);
        NetdiskResult netdiskResult = await baiduFileList.DeleteFile(delete_filelist, isAsync: true);
        if (netdiskResult.Success && netdiskResult.TaskID != null)
        {
            if ((await baiduFileList.GetProgress(netdiskResult.TaskID)).Status == TaskStatusIndicate.Done)
            {
                Refresh();
                return;
            }
            TaskView task_view = taskProber.AddTask();
            task_view.SetText("删除文件", $"生在删除{(dirlist.Count() + filelist.Count()).ToString()}个对象到", "请稍后\t0%");
            try
            {
                string current_path = ParentPath;
                task_view.SetTask(baiduFileList.GetProgress, netdiskResult.TaskID, () => { RefreshCallback(current_path); });

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            string errMsg = string.Empty;
            errMsg += (netdiskResult.Msg != null) ? netdiskResult.Msg : "未知错误";
            errMsg += $"\t 错误码: {netdiskResult.ResultID.ToString()}";
            message.SetMessage("创建失败", errMsg);
        }
        return;

    }
    /// <summary>
    /// Menu funcion: move
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MultiMoveOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NetdiskPathOverlay netdiskPathOverlay = new NetdiskPathOverlay();
        netdiskPathOverlay.baiduFileList = baiduFileList;
        netdiskPathOverlay.ShowInitDir();
        OverlayReservedGrid.Children.Add(netdiskPathOverlay);
        string? target_path = await netdiskPathOverlay.ShowDialog("请选择目标文件夹", "移动");

        List<string> self_path = new List<string>();
        List<string> self_name = new List<string>();
        List<string> target_path_list = new List<string>();
        if (baiduFileList.GetSelectedItem().File != null)
        {
            foreach (BDFile file in baiduFileList.GetSelectedItem().File)
            {
                self_name.Add(file.Name);
                self_path.Add(file.Path);
            }
        }
        else
        {
            return;
        }
        if (baiduFileList.GetSelectedItem().Dir != null)
        {
            foreach (BDDir dir in baiduFileList.GetSelectedItem().Dir)
            {
                self_path.Add(dir.Path);
                self_name.Add(dir.Name);
            }
        }
        for (int i = 0; i < self_path.Count; i++)
        {
            target_path_list.Add(target_path);
        }
        if (!string.IsNullOrEmpty(target_path))
        {
            NetdiskResult netdiskResult = await baiduFileList.Move(self_path.ToArray(), self_name.ToArray(), target_path_list.ToArray(), isAsync: true);
            if (netdiskResult.Success && netdiskResult.TaskID != null)
            {
                if ((await baiduFileList.GetProgress(netdiskResult.TaskID)).Status == TaskStatusIndicate.Done)
                {
                    Refresh();
                    return;
                }
                TaskView task_view = taskProber.AddTask();
                task_view.SetText("移动文件", $"移动{target_path_list.Count.ToString()}个对象到{target_path}", "请稍后\t0%");
                try
                {
                    string current_path = ParentPath;
                    task_view.SetTask(baiduFileList.GetProgress, netdiskResult.TaskID, () => { RefreshCallback(current_path); });

                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
            else
            {
                MessageOverlay message = new MessageOverlay();
                OverlayReservedGrid.Children.Add(message);
                string errMsg = string.Empty;
                errMsg += (netdiskResult.Msg != null) ? netdiskResult.Msg : "未知错误";
                errMsg += $"\t 错误码: {netdiskResult.ResultID.ToString()}";
                message.SetMessage("创建失败", errMsg);
            }
            //Refresh();
            return;
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            message.SetMessage("创建失败", $"移动文件时遇到错误");
            return;
        }
    }
    /// <summary>
    /// Menu funcion: copy
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MultiCopyOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NetdiskPathOverlay netdiskPathOverlay = new NetdiskPathOverlay();
        netdiskPathOverlay.baiduFileList = baiduFileList;
        netdiskPathOverlay.ShowInitDir();
        OverlayReservedGrid.Children.Add(netdiskPathOverlay);
        string? target_path = await netdiskPathOverlay.ShowDialog("请选择目标文件夹", "复制");
        List<string> self_path = new List<string>();
        List<string> self_name = new List<string>();
        List<string> target_path_list = new List<string>();
        if (baiduFileList.GetSelectedItem().File != null)
        {
            foreach (BDFile file in baiduFileList.GetSelectedItem().File)
            {
                self_name.Add(file.Name);
                self_path.Add(file.Path);
            }
        }
        if (baiduFileList.GetSelectedItem().Dir != null)
        {
            foreach (BDDir dir in baiduFileList.GetSelectedItem().Dir)
            {
                self_path.Add(dir.Path);
                self_name.Add(dir.Name);
            }
        }
            
        for (int i = 0; i < self_path.Count; i++)
        {
            target_path_list.Add(target_path);
        }
        if (!string.IsNullOrEmpty(target_path))
        {
            NetdiskResult netdiskResult = await baiduFileList.Copy(self_path.ToArray(), self_name.ToArray(), target_path_list.ToArray(), isAsync:true);
            Trace.WriteLine(netdiskResult.Success.ToString() + netdiskResult.TaskID);
            if (netdiskResult.Success && netdiskResult.TaskID != null)
            {
                if ((await baiduFileList.GetProgress(netdiskResult.TaskID)).Status == TaskStatusIndicate.Done)
                {
                    Refresh();
                    return;
                }
                TaskView task_view = taskProber.AddTask();
                task_view.SetText("复制文件", $"复制{target_path_list.Count.ToString()}个对象到{target_path}", "");
                try
                {
                    string current_path = ParentPath;
                    task_view.SetTask(baiduFileList.GetProgress, netdiskResult.TaskID, () => { RefreshCallback(current_path); });

                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
            else
            {
                MessageOverlay message = new MessageOverlay();
                OverlayReservedGrid.Children.Add(message);
                string errMsg = string.Empty;
                errMsg += (netdiskResult.Msg != null) ? netdiskResult.Msg : "未知错误";
                errMsg += $"\t 错误码: {netdiskResult.ResultID.ToString()}";
                message.SetMessage("创建失败", errMsg);
            }
            //Refresh();
            return;
        }
        
    }
}