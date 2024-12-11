using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Netko.NetDisk.Baidu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Netko.NetDisk;
namespace Netko;

public partial class NetdiskFilePage : UserControl
{
    public const string HomePath = "";
    public Action<string>? UpdateUserSectionName;

    // for history use
    private List<string> backHistory = new List<string>();
    private List<string> forwardHistory = new List<string>();
    private string? currentPath {  get; set; }
    private IFileList? baiduFileList {  get; set; }

    // for file action use
    private List<BDDir> selectDirList = new List<BDDir>();
    private List<BDFile> selectFileList = new List<BDFile>();
    // for color changing use
    private Color selectedColor;
    private Color unselectedColor;
    // for toogle select
    public Dictionary<BDDir, ItemShowLine> DirDict = new Dictionary<BDDir, ItemShowLine>();
    public Dictionary<BDFile, ItemShowLine> FileDict = new Dictionary<BDFile, ItemShowLine>();
    public Grid OverlayReservedGrid { get; set; }
    public StackPanel OverlayNotification {  get; set; }
    public TransferPage? TransferPage { get; set; }
    private int CurrentPageIndex = 1;
    private bool CanLoadMore = false;
    public NetdiskFilePage()
    {
        InitializeComponent();
        GetColor();
        OverlayReservedGrid = OverlayReserved;
        OverlayNotification = NotificationOverlay;
        task_prober.Close();
    }

    private async void FadeOut()
    {
        var DropAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.15),
            Easing = new ExponentialEaseOut(),
            Children =
    {
        new KeyFrame
        {
            Cue = new Cue(0),
            Setters =
            {
                new Setter(UserControl.OpacityProperty, 1.0)
            }

        },
        new KeyFrame
        {
            Cue = new Cue(1),
            Setters =
            {
                new Setter(UserControl.OpacityProperty, 0.0)
            }
        }
    }
        };
        await DropAnimation.RunAsync(FileListViewer);
        FileListViewer.Opacity = 0;

        //await Task.Delay(150);
    }
    private async void FadeIn()
    {
        var DropAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.15),
            Easing = new ExponentialEaseOut(),
            Children =
    {
        new KeyFrame
        {
            Cue = new Cue(0),
            Setters =
            {
                new Setter(UserControl.OpacityProperty, 0.0)
            }

        },
        new KeyFrame
        {
            Cue = new Cue(1),
            Setters =
            {
                new Setter(UserControl.OpacityProperty, 1.0)
            }
        }
    }
        };

        await DropAnimation.RunAsync(FileListViewer);
        FileListViewer.Opacity = 1;
        //await Task.Delay(150);
    }
    public void GetColor()
    {
        var CBH_backgound = Application.Current!.Resources.TryGetResource("CatalogBaseHighColor", null, out var Hresource);
        if (CBH_backgound && Hresource is Color Backgound)
        {
            selectedColor = Backgound;
        }
        var CBL_background = Application.Current!.Resources.TryGetResource("CatalogBaseHighColor", null, out var Lresource);
        if (CBL_background && Lresource is Color Background)
        {
            unselectedColor = Background;
        }
    }
    private void UpdateMenu()
    {
        if (selectFileList.Count + selectDirList.Count == 0)
        {
            //ActionMenu
        }
    }
    public void ToggleSelectedFile(BDFile file, UserControl userControl)
    {
        if (selectFileList.Contains(file))
        {
            userControl.Background = new SolidColorBrush(unselectedColor);
            selectFileList.Remove(file);
        }
        else
        {
            userControl.Background = new SolidColorBrush(selectedColor);
            selectFileList.Add(file);
        }

    }

    public void ToogleSelectedFolder(BDDir dir, UserControl userControl)
    {
        if (selectDirList.Contains(dir))
        {
            userControl.Background = new SolidColorBrush(unselectedColor);
            selectDirList.Remove(dir);
        }
        else
        {
            userControl.Background = new SolidColorBrush(selectedColor);
            selectDirList.Add(dir);
        }
    }

    private string GetForwardPath()
    {
        if (forwardHistory.Count > 0)
        {
            string result = forwardHistory[forwardHistory.Count - 1];
            forwardHistory.RemoveAt(forwardHistory.Count - 1);
            return result;
        }
        else
        {
            return string.Empty;
        }
    }
    private string GetBackPath()
    {
        if (currentPath == null)
        {
            return "/";
        }
        forwardHistory.Add(currentPath);

        if (backHistory[backHistory.Count - 1] == currentPath)
        {

            backHistory.RemoveAt(backHistory.Count - 1);
        }

        if (backHistory.Count != 0)
        {
            string result = backHistory[backHistory.Count - 1];
            backHistory.RemoveAt(backHistory.Count - 1);
            
            return result;
        }
        else
        {
            return "/";
        }
    }
    private void GotoDir(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null || PathTextBox.Text == null) { return; }
        ChangePage(baiduFileList, PathTextBox.Text, 1, insert_back_history: false);

    }


    private void Back(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null) { return; }
        ChangePage(baiduFileList, GetBackPath(), 1, reserve_forward_history:true);
    }
    private void Forward(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null) { return; }
        ChangePage(baiduFileList, GetForwardPath(), 1, insert_back_history: true, reserve_forward_history:true);

    }

    private void Home(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null) { return; }
        ChangePage(baiduFileList, "/", 1, insert_back_history: false, reserve_forward_history: false, reserve_back_history: false);
    }
    private void Refresh(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null || currentPath == null) { return; }
        ChangePage(baiduFileList, currentPath, 1, insert_back_history:false);
    }
    private void ExpandTaskProber(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        task_prober.Open();
    }
    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            var isAtBottom = scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height;

            if (isAtBottom && (baiduFileList != null && currentPath != null) && CanLoadMore)
            {
                ChangePage(baiduFileList, currentPath, CurrentPageIndex + 1, false, true, true);
            }
        }
    }
    /// <summary>
    /// To get and show file list in UI, items count 1000 at max
    /// </summary>
    /// <param name="user">This function will use BaiduFileList to parse file list</param>
    /// <param name="go_path">Path need to parse, for ep. "/"</param>
    /// <param name="page">1: 1-1000 items, 2: 1001-2000 items...</param>
    private async void ChangePage(IFileList user, 
        string go_path, 
        int page, 
        bool insert_back_history=true, 
        bool reserve_forward_history=false,
        bool reserve_back_history=true)
    {
        if (!reserve_forward_history) { forwardHistory.Clear(); }
        if (!reserve_back_history) { backHistory.Clear(); }
        if (insert_back_history) { backHistory.Add(go_path); }
        ForwardButton.IsEnabled = (forwardHistory.Count == 0) ? false : true;
        BackButton.IsEnabled = ((backHistory.Count == 1 && go_path == "/" && backHistory[0] == "/") || backHistory.Count == 0) ? false : true;

        PathTextBox.Text = go_path;
        currentPath = go_path;
        CurrentPageIndex = page;

        if (page == 1) { FileListViewer.Opacity = 0; }

        BDFileList list_ = await user.GetFileList(page, path: go_path);
        if (page == 1)
        {
            // clear pervious
            FileDict.Clear();
            DirDict.Clear();
            FileListViewer.Children.Clear();
            SelectAllOverlay.Children.Clear();
        }
        // select all button
        SelectAllButton selectAllButton = new SelectAllButton();
        SelectAllOverlay.Children.Add(selectAllButton);
        selectAllButton.baiduFileList = user;
        selectAllButton.list_ = list_;
        selectAllButton.DirDict = DirDict;
        selectAllButton.FileDict = FileDict;

        long itemCount = 0;
        if (list_.Dir == null || list_.File == null)
        {
            FileListViewer.Opacity = 1;
            SelectAllOverlay.Children.Remove(selectAllButton);
            EmptyRemind empty_label = new EmptyRemind();
            empty_label.ShowError("不存在的文件夹\n你来到了没有文件的荒原");
            FileListViewer.Children.Add(empty_label);
            return;
        }

        foreach (BDDir dir_b in list_.Dir)
        {
            itemCount++;
            ItemShowLine DirBlock = new ItemShowLine();
            DirDict.Add(dir_b, DirBlock);
            // set enter, refresh command
            DirBlock.Func = () => ChangePage(user, dir_b.Path, 1);
            DirBlock.Refresh = () => ChangePage(baiduFileList!, currentPath, 1, insert_back_history: false);
            // set name, BDDir, user, OverelayGrid
            DirBlock.SelfDir = dir_b;

            DirBlock.Init(dir_b.Name, is_dir: true);
            DirBlock.baiduFileList = user;
            DirBlock.OverlayReservedGrid = OverlayReservedGrid;
            DirBlock.OverlayNotification = OverlayNotification;
            DirBlock.taskProber = task_prober;
            DirBlock.ParentPath = currentPath;

            // append to viewer
            FileListViewer.Children.Add(DirBlock);

        }
        foreach (BDFile file_b in list_.File)
        {
            itemCount++;

            ItemShowLine FileBlock = new ItemShowLine();
            FileDict.Add(file_b, FileBlock);

            FileBlock.TransferPage = TransferPage!;
            // set enter, refresh command
            FileBlock.Func = () => ChangePage(user, file_b.Path, 1);
            FileBlock.Refresh = () => ChangePage(baiduFileList!, currentPath, 1, insert_back_history: false);
            // set name, BDDir, user, OverelayGrid
            FileBlock.SelfFile = file_b;

            FileBlock.Init(file_b.Name, is_dir: false);
            FileBlock.baiduFileList = user;
            FileBlock.OverlayReservedGrid = OverlayReservedGrid;
            FileBlock.OverlayNotification = OverlayNotification;
            FileBlock.taskProber = task_prober;

            FileBlock.ParentPath = currentPath;
            // append to viewer
            FileListViewer.Children.Add(FileBlock);
        }
        //FadeIn();
        CanLoadMore = (itemCount == 1000) ? true : false;
        FileListViewer.Opacity = 1;
        if (itemCount == 0)
        {

            SelectAllOverlay.Children.Remove(selectAllButton);

            EmptyRemind empty_label = new EmptyRemind();
            empty_label.OverlayReservedGrid = OverlayReservedGrid;
            empty_label.ParentPath = currentPath;
            empty_label.baiduFileList = user;
            empty_label.ParentPath = currentPath;
            empty_label.Refresh = () => ChangePage(baiduFileList!, currentPath, 1, insert_back_history: false);

            FileListViewer.Children.Add(empty_label);
        }

    }
    /// <summary>
    /// To init user info, create and change to new page, update user name
    /// </summary>
    /// <param name="cookie">user cookie</param>
    public async Task<string> initUser(string cookie)
    {
        INetdisk test_user = new Baidu(cookie);
        await test_user.init();
        if (UpdateUserSectionName != null) {
            UpdateUserSectionName(test_user.GetAccountInfo().Name);
        }

        IFileList Filelist = new BaiduFileList(test_user);
        baiduFileList = Filelist;
        MeowSetting.InsertAccount(cookie, test_user.GetAccountInfo().Token);
        ChangePage(Filelist, "/", 1);
        return test_user.GetAccountInfo().Token;
    }
    


}