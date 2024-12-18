using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;
using System;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
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
    public Action Func { get; set; }
    public Action Refresh { get; set; }

    public string ParentPath { get; set; }

    //private StackPanel FileListViewer;
    //public Color HoverBG;
    //public Color LeaveBG;
    private DateTime _lastClickedTime;
    public IFileList baiduFileList { get; set; }

    public NetDir SelfDir;
    public NetFile SelfFile;
    private bool _isFile;
    private bool _isDir;
    private bool _isSelected = false;
    private bool _singleMenuOperation = true;
    public Grid OverlayReservedGrid { get; set; }
    public StackPanel OverlayNotification { get; set; }
    public TransferPage TransferPage { get; set; }
    public TaskProber taskProber { get; set; }

    public ItemShowLine()
    {
        InitializeComponent();
        _lastClickedTime = DateTime.Now;
        Application.Current!.ActualThemeVariantChanged += (sender, args) => { UpdateColor(); };
    }

    private void UpdateColor()
    {
        if (_isSelected)
        {
            BorderBackground[!Border.BackgroundProperty] = new DynamicResourceExtension("HoverColor");
        }
        else
        {
            BorderBackground[!Border.BackgroundProperty] = new DynamicResourceExtension("CatalogBaseHighColor");
        }
    }

    private void ShowInfo(string message)
    {
        FlyNoticeOverlay flyNoticeOverlay = new FlyNoticeOverlay();
        OverlayNotification.Children.Add(flyNoticeOverlay);
        flyNoticeOverlay.Run(message);
    }

    private string FormatDate(long timeStamp)
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime dateTime = startTime.AddSeconds(timeStamp).ToLocalTime();
        return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
    }
    private void DockPanel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        return;
    }
    /// <summary>
    /// Init item, set type and icon
    /// </summary>
    /// <param name="name">file name</param>
    /// <param name="isDir"></param>
    public void Init(string name, bool isDir)
    {
        Uri uri;
        if (isDir)
        {
            _isDir = true;
            _isFile = false;
            uri = new Uri("avares://Netko/Assets/FileIcon/icons8-folder-188.png");
        }
        else
        {
            DockpanelOpen.IsVisible = false;
            _isDir = false;
            _isFile = true;
            uri = new Uri("avares://Netko/Assets/FileIcon/icons8-document-188.png");
        }

        ItemIcon.Source = ImageHelper.LoadFromResource(uri);
        FileName.Content = name;
        detail_label.Content =
            isDir
                ? $"{FormatDate(SelfDir.ServerCtime)}"
                : $"{FormatDate(SelfFile.ServerCtime)}\t{DownloadProgress.FormatSize(SelfFile.Size)}";
    }

    private void RefreshCallback(string previousPath)
    {
        if (previousPath == ParentPath)
        {
            Refresh();
        }
    }

    private void SetMultiOperationCommand()
    {
        if (!_singleMenuOperation)
        {
            return;
        }

        //DockpanelOpen.Click -= OepnOnMenu;
        DockpanelDelete.Click -= DeleteOnMenu;
        DockpanelMove.Click -= MoveOnMenu;
        DockpanelDuplicate.Click -= CopyOnMenu;
        DockpanelShare.Click -= ShareOnMenu;
        DockpanelDownload.Click -= DownloadOnMenu;


        DockpanelDelete.Click += MultiDeleteOnMenu;
        DockpanelMove.Click += MultiMoveOnMenu;
        DockpanelDuplicate.Click += MultiCopyOnMenu;
        DockpanelShare.Click += MultiShareOnMenu;
        DockpanelDownload.Click += MultiDownloadOnMenu;

        _singleMenuOperation = false;
    }

    private void SetSingleOperationCommand()
    {
        if (_singleMenuOperation)
        {
            return;
        }

        DockpanelDelete.Click -= MultiDeleteOnMenu;
        DockpanelMove.Click -= MultiMoveOnMenu;
        DockpanelDuplicate.Click -= MultiCopyOnMenu;
        DockpanelShare.Click -= MultiShareOnMenu;
        DockpanelDownload.Click -= MultiDownloadOnMenu;


        DockpanelDelete.Click += DeleteOnMenu;
        DockpanelMove.Click += MoveOnMenu;
        DockpanelDuplicate.Click += CopyOnMenu;
        DockpanelShare.Click += ShareOnMenu;
        DockpanelDownload.Click += DownloadOnMenu;
        _singleMenuOperation = true;
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

        if (_isFile)
        {
            FileList filelist = baiduFileList.GetSelectedItem();
            if (!filelist.File.Contains(SelfFile))
            {
                // single select
                SetSingleOperationCommand();
            }
            else if (filelist.File.Contains(SelfFile) && filelist.File.Count() == 1 && filelist.Dir.Any())
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

        if (_isDir)
        {
            FileList filelist = baiduFileList.GetSelectedItem();
            if (!filelist.Dir.Contains(SelfDir))
            {
                // single select
                SetSingleOperationCommand();
            }
            else if (filelist.Dir.Contains(SelfDir) && filelist.Dir.Count() == 1 && filelist.File.Any())
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

    public void ToogleSelect()
    {
        if (_isDir)
        {
            _isSelected = (!baiduFileList.DirIsSelected(SelfDir));
            baiduFileList.ToggleSelectDir(SelfDir);
        }

        if (_isFile)
        {
            _isSelected = (!baiduFileList.FileIsSelected(SelfFile));
            baiduFileList.ToggleSelectFile(SelfFile);
        }

        UpdateColor();
    }

    /// <summary>
    /// left click and double click, toggle select
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LeftClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var currentTime = DateTime.Now;
        var timeDiff = currentTime - _lastClickedTime;
        _lastClickedTime = currentTime;

        if (timeDiff.TotalMilliseconds <= 250)
        {
            if (_isDir)
            {
                Func();
            }

            if (_isFile)
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
            ToogleSelect();
        }
    }

    /// <summary>
    /// Menu function: open
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
        string filename = await inputName.ShowDialog("请输入新建文件夹的名称", "创建");
        try
        {
            if ((await baiduFileList.CreateDir(ParentPath + "/" + filename)).Success)
            {
                Refresh();
            }
            else
            {
                MessageOverlay message = new MessageOverlay();
                OverlayReservedGrid.Children.Add(message);
                message.SetMessage("创建失败", $"创建{ParentPath + "/" + filename}时遇到错误");
            }
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
        
    }

    /// <summary>
    /// Menu function: copy
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CopyOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NetdiskPathOverlay netdiskPathOverlay = new NetdiskPathOverlay();
        netdiskPathOverlay.baiduFileList = baiduFileList;
        netdiskPathOverlay.ShowInitDir();
        OverlayReservedGrid.Children.Add(netdiskPathOverlay);
        string targetPath = await netdiskPathOverlay.ShowDialog("请选择目标文件夹", "复制");
        string selfPath, selfName;
        if (_isDir)
        {
            selfName = SelfDir.Name;
            selfPath = SelfDir.Path;
        }
        else
        {
            selfName = SelfFile.Name;
            selfPath = SelfFile.Path;
        }

        try
        {
            if (!string.IsNullOrEmpty(targetPath))
            {
                NetdiskResult netdiskResult =
                    await baiduFileList.Copy([selfPath], [selfName], [targetPath], isAsync: true);
                if (netdiskResult.Success && netdiskResult.TaskId != null)
                {
                    if ((await baiduFileList.GetProgress(netdiskResult.TaskId)).Status == TaskStatusIndicate.Done)
                    {
                        Refresh();
                        return;
                    }

                    TaskView taskView = taskProber.AddTask();
                    taskView.SetText("复制文件", $"复制{selfName}到{targetPath}", "请稍后\t0%");
                    try
                    {
                        string currentPath = ParentPath;
                        taskView.SetTask(baiduFileList.GetProgress, netdiskResult.TaskId,
                            () => { RefreshCallback(currentPath); });
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
                    errMsg += $"\t 错误码: {netdiskResult.ResultId.ToString()}";
                    message.SetMessage("创建失败", errMsg);
                }

            }
            else
            {
                MessageOverlay message = new MessageOverlay();
                OverlayReservedGrid.Children.Add(message);
                message.SetMessage("创建失败", $"移动文件{selfPath}时遇到错误");
            }
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
        
    }

    /// <summary>
    /// Menu function: move
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MoveOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NetdiskPathOverlay netdiskPathOverlay = new NetdiskPathOverlay();
        netdiskPathOverlay.baiduFileList = baiduFileList;
        netdiskPathOverlay.ShowInitDir();
        OverlayReservedGrid.Children.Add(netdiskPathOverlay);
        string targetPath = await netdiskPathOverlay.ShowDialog("请选择目标文件夹", "移动");
        string selfPath, selfName;
        if (_isDir)
        {
            selfName = SelfDir.Name;
            selfPath = SelfDir.Path;
        }
        else
        {
            selfName = SelfFile.Name;
            selfPath = SelfFile.Path;
        }

        try
        {
            if (!string.IsNullOrEmpty(targetPath))
            {
                NetdiskResult netdiskResult =
                    await baiduFileList.Move([selfPath], 
                        [selfName], 
                        [targetPath], 
                        isAsync: true);
                if (netdiskResult.Success && netdiskResult.TaskId != null)
                {
                    if ((await baiduFileList.GetProgress(netdiskResult.TaskId)).Status == TaskStatusIndicate.Done)
                    {
                        Refresh();
                        return;
                    }

                    TaskView taskView = taskProber.AddTask();
                    taskView.SetText("移动文件", 
                        $"移动{selfName}到{targetPath}", "请稍后\t0%");
                    try
                    {
                        string currentPath = ParentPath;
                        taskView.SetTask(baiduFileList.GetProgress, netdiskResult.TaskId,
                            () => { RefreshCallback(currentPath); });
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
                    errMsg += $"\t 错误码: {netdiskResult.ResultId.ToString()}";
                    message.SetMessage("创建失败", errMsg);
                }
            }
            else
            {
                MessageOverlay message = new MessageOverlay();
                OverlayReservedGrid.Children.Add(message);
                message.SetMessage("创建失败", $"移动文件{selfPath}时遇到错误");

            }
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
        
    }

    /// <summary>
    /// Menu function: rename
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RenameOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogOverlay inputName = new DialogOverlay();
        OverlayReservedGrid.Children.Add(inputName);
        string filename = await inputName.ShowDialog("请输入文件的新名称", "重命名", placeHolderS: SelfDir.Name);
        string selfPath;
        if (_isDir)
        {
            selfPath = SelfDir.Path;
        }
        else
        {
            selfPath = SelfFile.Path;
        }

        try
        {
            NetdiskResult netdiskResult = await baiduFileList.Rename([selfPath], [filename], isAsync: true);
            if (netdiskResult.Success && netdiskResult.TaskId != null)
            {
                if ((await baiduFileList.GetProgress(netdiskResult.TaskId)).Status == TaskStatusIndicate.Done)
                {
                    Refresh();
                    return;
                }

                TaskView taskView = taskProber.AddTask();
                taskView.SetText("重命名文件", $"正在重命名{selfPath}", "请稍后\t0%");
                try
                {
                    string currentPath = ParentPath;
                    taskView.SetTask(baiduFileList.GetProgress, netdiskResult.TaskId,
                        () => { RefreshCallback(currentPath); });
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
                errMsg += $"\t 错误码: {netdiskResult.ResultId.ToString()}";
                message.SetMessage("创建失败", errMsg);
            }
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
    }

    /// <summary>
    /// Menu function: delete
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeleteOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string deleteFilelist;
        int count = 0;
        if (_isDir)
        {
            count++;
            List<NetDir> dirList = new List<NetDir>();
            dirList.Add(SelfDir);
            deleteFilelist = baiduFileList.IntegrateFilelist(null, dirList);
        }
        else
        {
            count++;
            List<NetFile> fileList = new List<NetFile>();
            fileList.Add(SelfFile);
            deleteFilelist = baiduFileList.IntegrateFilelist(fileList, null);
        }

        try
        {
            NetdiskResult netdiskResult = await baiduFileList.DeleteFile(deleteFilelist, isAsync: true);


            if (netdiskResult.Success && netdiskResult.TaskId != null)
            {
                if ((await baiduFileList.GetProgress(netdiskResult.TaskId)).Status == TaskStatusIndicate.Done)
                {
                    Refresh();
                    return;
                }

                TaskView taskView = taskProber.AddTask();

                taskView.SetText("删除文件", $"正在删除{count.ToString()}个项目", "请稍后\t0%");
                try
                {
                    string currentPath = ParentPath;
                    taskView.SetTask(baiduFileList.GetProgress, netdiskResult.TaskId,
                        () => { RefreshCallback(currentPath); });
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
                errMsg += $"\t 错误码: {netdiskResult.ResultId.ToString()}";
                message.SetMessage("创建失败", errMsg);
            }
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
    }

    /// <summary>
    /// Menu function: share
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ShareOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ShareLinkOverlay shareLinkOverlay = new ShareLinkOverlay();
        shareLinkOverlay.baiduFileList = baiduFileList;
        //shareLinkOverlay.FileList = null;
        if (_isDir)
        {
            shareLinkOverlay.DirList = new List<NetDir>() { SelfDir };
        }
        else
        {
            shareLinkOverlay.FileList = new List<NetFile>() { SelfFile };
        }

        shareLinkOverlay.Opacity = 0;

        OverlayReservedGrid.Children.Add(shareLinkOverlay);
        shareLinkOverlay.Opacity = 1;
    }

    private void PropertiesOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PropertiesOverlay propertiesOverlay = new PropertiesOverlay();
        if (_isDir)
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
        if (_isFile)
        {
            flyNoticeOverlay.Run($"{SelfFile.Name} 已添加进下载队列");
            try
            {
                Console.WriteLine((TransferPage == null) ? "NOT EXIST":"EXIST");

                List<string> urlList = await baiduFileList.GetFileDownloadLink(SelfFile.Path);
                DownloadConfig downloadConfig = baiduFileList.ChooseDownloadMethod();
                downloadConfig.FileName = SelfFile.Name;
                downloadConfig.FilePath = FilePathOperate.GetAvailablePath(subPath: null, fileName: SelfFile.Name);
                downloadConfig.FileSize = SelfFile.Size;
                downloadConfig.Url = urlList[0];
                string tmpPath = SelfFile.Path;
                downloadConfig.GetUrlFunc = () => { return baiduFileList.GetFileDownloadLink(tmpPath); };
                urlList.Remove(urlList[0]);
                TransferPage.addTask(urlList, downloadConfig);
            }
            catch (Exception ex)
            {
                FlyNoticeOverlay err = new FlyNoticeOverlay();

                err.Run($"{SelfFile.Name}下载出错：{ex}");
            }
        }
        else
        {
            flyNoticeOverlay.Run($"正在解析文件夹");
            try
            {
                TaskView taskView = taskProber.AddTask();
                taskView.SetText("正在解析需要下载的文件", $"正在解析: {SelfDir.Path}", "请稍后...");   
                FileList mappedFileList = await baiduFileList.MapFileList(SelfDir.Path);
                taskView.SetText("正在解析需要下载的文件", $"正在解析: {SelfDir.Path}", "已获取文件数");   

                foreach (NetFile file in mappedFileList.File)
                {
                    taskView.SetText(mainContent : $"正在解析: {SelfDir.Path}", secondaryContent : $"正在获取下载链接:{file.Name}");   

                    List<string> urlList = await baiduFileList.GetFileDownloadLink(file.Path);
                    DownloadConfig downloadConfig = baiduFileList.ChooseDownloadMethod();
                    downloadConfig.FileName = file.Name;
                    string tmpPath = FilePathOperate.NormalizePath(file.Path);
                    string unusedPath = FilePathOperate.NormalizePath(ParentPath);
                    downloadConfig.FilePath = FilePathOperate.GetAvailablePath(subPath: FilePathOperate.RemovePrefixPath(tmpPath, unusedPath));
                    downloadConfig.FileSize = file.Size;
                    Console.WriteLine(urlList[0]);
                    downloadConfig.Url = urlList[0];
                    urlList.Remove(urlList[0]);
                    Console.WriteLine(TransferPage.ToString());

                    TransferPage.addTask(urlList, downloadConfig);
                }
                taskView.SetText(secondaryContent:"已完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                FlyNoticeOverlay err = new FlyNoticeOverlay();

                err.Run($"{SelfFile.Name}下载出错：{ex}");
            }
        }

        
    }
    /*
     * ==========================
     *      Multi file operation
     * ==========================
     */

    /// <summary>
    /// Menu function: share
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MultiShareOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
    /// Menu function: delete
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MultiDeleteOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        List<NetDir> dirList = baiduFileList.GetSelectedItem().Dir;
        List<NetFile> fileList = baiduFileList.GetSelectedItem().File;
        string deleteFilelist = baiduFileList.IntegrateFilelist(fileList, dirList);
        try
        {
            NetdiskResult netdiskResult = await baiduFileList.DeleteFile(deleteFilelist, isAsync: true);
            if (netdiskResult.Success && netdiskResult.TaskId != null)
            {
                if ((await baiduFileList.GetProgress(netdiskResult.TaskId)).Status == TaskStatusIndicate.Done)
                {
                    Refresh();
                    return;
                }

                TaskView taskView = taskProber.AddTask();
                taskView.SetText("删除文件", $"生在删除{(dirList.Count() + fileList.Count()).ToString()}个对象", "请稍后\t0%");
                try
                {
                    string currentPath = ParentPath;
                    taskView.SetTask(baiduFileList.GetProgress, netdiskResult.TaskId,
                        () => { RefreshCallback(currentPath); });
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
                errMsg += $"\t 错误码: {netdiskResult.ResultId.ToString()}";
                message.SetMessage("创建失败", errMsg);
            }
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
    }

    /// <summary>
    /// Menu function: move
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MultiMoveOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NetdiskPathOverlay netdiskPathOverlay = new NetdiskPathOverlay();
        netdiskPathOverlay.baiduFileList = baiduFileList;
        netdiskPathOverlay.ShowInitDir();
        OverlayReservedGrid.Children.Add(netdiskPathOverlay);
        string targetPath = await netdiskPathOverlay.ShowDialog("请选择目标文件夹", "移动");

        List<string> selfPath = new List<string>();
        List<string> selfName = new List<string>();
        List<string> targetPathList = new List<string>();
        if (baiduFileList.GetSelectedItem().File.Any())
        {
            foreach (NetFile file in baiduFileList.GetSelectedItem().File)
            {
                selfName.Add(file.Name);
                selfPath.Add(file.Path);
            }
        }
        else
        {
            return;
        }

        if (baiduFileList.GetSelectedItem().Dir.Any())
        {
            foreach (NetDir dir in baiduFileList.GetSelectedItem().Dir)
            {
                selfPath.Add(dir.Path);
                selfName.Add(dir.Name);
            }
        }

        for (int i = 0; i < selfPath.Count; i++)
        {
            targetPathList.Add(targetPath);
        }

        try
        {
            if (!string.IsNullOrEmpty(targetPath))
            {
                NetdiskResult netdiskResult = await baiduFileList.Move(selfPath.ToArray(), selfName.ToArray(),
                    targetPathList.ToArray(), isAsync: true);
                if (netdiskResult.Success && netdiskResult.TaskId != null)
                {
                    if ((await baiduFileList.GetProgress(netdiskResult.TaskId)).Status == TaskStatusIndicate.Done)
                    {
                        Refresh();
                        return;
                    }

                    TaskView taskView = taskProber.AddTask();
                    taskView.SetText("移动文件", $"移动{targetPathList.Count.ToString()}个对象到{targetPath}", "请稍后\t0%");
                    try
                    {
                        string currentPath = ParentPath;
                        taskView.SetTask(baiduFileList.GetProgress, netdiskResult.TaskId,
                            () => { RefreshCallback(currentPath); });
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
                    errMsg += $"\t 错误码: {netdiskResult.ResultId.ToString()}";
                    message.SetMessage("创建失败", errMsg);
                }
            }
            else
            {
                MessageOverlay message = new MessageOverlay();
                OverlayReservedGrid.Children.Add(message);
                message.SetMessage("创建失败", $"移动文件时遇到错误");
            }
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
        
    }

    /// <summary>
    /// Menu function: copy
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MultiCopyOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        NetdiskPathOverlay netdiskPathOverlay = new NetdiskPathOverlay();
        netdiskPathOverlay.baiduFileList = baiduFileList;
        netdiskPathOverlay.ShowInitDir();
        OverlayReservedGrid.Children.Add(netdiskPathOverlay);
        string targetPath = await netdiskPathOverlay.ShowDialog("请选择目标文件夹", "复制");
        List<string> selfPath = new List<string>();
        List<string> selfName = new List<string>();
        List<string> targetPathList = new List<string>();
        if (baiduFileList.GetSelectedItem().File.Any())
        {
            foreach (NetFile file in baiduFileList.GetSelectedItem().File)
            {
                selfName.Add(file.Name);
                selfPath.Add(file.Path);
            }
        }

        if (baiduFileList.GetSelectedItem().Dir.Any())
        {
            foreach (NetDir dir in baiduFileList.GetSelectedItem().Dir)
            {
                selfPath.Add(dir.Path);
                selfName.Add(dir.Name);
            }
        }

        for (int i = 0; i < selfPath.Count; i++)
        {
            targetPathList.Add(targetPath);
        }

        try
        {
            if (!string.IsNullOrEmpty(targetPath))
            {
                NetdiskResult netdiskResult = await baiduFileList.Copy(selfPath.ToArray(), selfName.ToArray(),
                    targetPathList.ToArray(), isAsync: true);
                Trace.WriteLine(netdiskResult.Success.ToString() + netdiskResult.TaskId);
                if (netdiskResult.Success && netdiskResult.TaskId != null)
                {
                    if ((await baiduFileList.GetProgress(netdiskResult.TaskId)).Status == TaskStatusIndicate.Done)
                    {
                        Refresh();
                        return;
                    }

                    TaskView taskView = taskProber.AddTask();
                    taskView.SetText("复制文件", $"复制{targetPathList.Count.ToString()}个对象到{targetPath}", "");
                    try
                    {
                        string currentPath = ParentPath;
                        taskView.SetTask(baiduFileList.GetProgress, netdiskResult.TaskId,
                            () => { RefreshCallback(currentPath); });
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
                    errMsg += $"\t 错误码: {netdiskResult.ResultId.ToString()}";
                    message.SetMessage("创建失败", errMsg);
                }
            }
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
        
    }

    private async void MultiDownloadOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //List<NetDir> dirList = 
        //List<NetFile> fileList = 
        TaskView taskView = taskProber.AddTask();
        taskView.SetText("正在解析需要下载的文件", "正在准备", "已解析: 0");
        FileList parsedFileList = new FileList();
        parsedFileList.File = new List<NetFile>();
        parsedFileList.Dir = new List<NetDir>();
        foreach (NetFile file in baiduFileList.GetSelectedItem().File) { parsedFileList.File.Add(file); }
        taskView.SetText(mainContent:"正在解析", secondaryContent : $"已解析: {parsedFileList.File.Count()}");
        foreach (NetDir dir in baiduFileList.GetSelectedItem().Dir)
        {
            parsedFileList.Dir.Add(dir);
            FileList mappedFileList = await baiduFileList.MapFileList(dir.Path);
            foreach (NetFile fileObj in mappedFileList.File) { parsedFileList.File.Add(fileObj); }
            foreach (NetDir dirObj in mappedFileList.Dir) { parsedFileList.Dir.Add(dirObj); }
            taskView.SetText( secondaryContent : $"已解析: {parsedFileList.File.Count()}");
        }

        foreach (NetFile file in parsedFileList.File)
        {
            taskView.SetText( secondaryContent : $"正在获取下载链接: {file.Name}");
            List<string> urlList = await baiduFileList.GetFileDownloadLink(file.Path);
            DownloadConfig downloadConfig = baiduFileList.ChooseDownloadMethod();
            downloadConfig.FileName = file.Name;
            string tmpPath = FilePathOperate.NormalizePath(file.Path);
            string unusedPath = FilePathOperate.NormalizePath(ParentPath);
            downloadConfig.FilePath = FilePathOperate.GetAvailablePath(subPath: FilePathOperate.RemovePrefixPath(tmpPath, unusedPath));
            downloadConfig.FileSize = file.Size;
            downloadConfig.Url = urlList[0];
            urlList.Remove(urlList[0]);
            TransferPage.addTask(urlList, downloadConfig);
        }
        taskView.SetText(mainContent:"已完成", secondaryContent : $"已解析: {parsedFileList.File.Count()}");
        taskView.SetProgress(100);
        

    }

}