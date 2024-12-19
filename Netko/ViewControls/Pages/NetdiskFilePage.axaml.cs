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
using System.Linq;
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
    private List<string> _backHistory = new List<string>();
    private List<string> _forwardHistory = new List<string>();
    private string? currentPath {  get; set; }
    private IFileList? baiduFileList {  get; set; }

    // for file action use
    private List<NetDir> _selectDirList = new List<NetDir>();
    private List<NetFile> _selectFileList = new List<NetFile>();
    // for color changing use
    private Color _selectedColor;
    private Color _unselectedColor;
    // for toogle select
    public Dictionary<NetDir, ItemShowLine> DirDict = new Dictionary<NetDir, ItemShowLine>();
    public Dictionary<NetFile, ItemShowLine> FileDict = new Dictionary<NetFile, ItemShowLine>();
    public Grid OverlayReservedGrid { get; set; }
    public StackPanel OverlayNotification {  get; set; }
    public TransferPage? TransferPage { get; set; }
    private int _currentPageIndex = 1;
    private bool _canLoadMore = false;
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
        var dropAnimation = new Animation
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
        await dropAnimation.RunAsync(FileListViewer);
        FileListViewer.Opacity = 0;

        //await Task.Delay(150);
    }
    private async void FadeIn()
    {
        var dropAnimation = new Animation
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

        await dropAnimation.RunAsync(FileListViewer);
        FileListViewer.Opacity = 1;
        //await Task.Delay(150);
    }
    public void GetColor()
    {
        var cbhBackground = Application.Current!.Resources.TryGetResource("CatalogBaseHighColor", null, out var Hresource);
        if (cbhBackground && Hresource is Color bg)
        {
            _selectedColor = bg;
        }
        var cblBackground = Application.Current!.Resources.TryGetResource("CatalogBaseHighColor", null, out var Lresource);
        if (cblBackground && Lresource is Color background)
        {
            _unselectedColor = background;
        }
    }
    private void UpdateMenu()
    {
        if (_selectFileList.Count + _selectDirList.Count == 0)
        {
            //ActionMenu
        }
    }
    public void ToggleSelectedFile(NetFile file, UserControl userControl)
    {
        if (_selectFileList.Contains(file))
        {
            userControl.Background = new SolidColorBrush(_unselectedColor);
            _selectFileList.Remove(file);
        }
        else
        {
            userControl.Background = new SolidColorBrush(_selectedColor);
            _selectFileList.Add(file);
        }

    }

    public void ToogleSelectedFolder(NetDir dir, UserControl userControl)
    {
        if (_selectDirList.Contains(dir))
        {
            userControl.Background = new SolidColorBrush(_unselectedColor);
            _selectDirList.Remove(dir);
        }
        else
        {
            userControl.Background = new SolidColorBrush(_selectedColor);
            _selectDirList.Add(dir);
        }
    }

    private string GetForwardPath()
    {
        if (_forwardHistory.Count > 0)
        {
            string result = _forwardHistory[-1];
            _forwardHistory.RemoveAt(_forwardHistory.Count - 1);
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
        _forwardHistory.Add(currentPath);

        if (_backHistory[_backHistory.Count - 1] == currentPath)
        {

            _backHistory.RemoveAt(_backHistory.Count - 1);
        }

        if (_backHistory.Count != 0)
        {
            string result = _backHistory[_backHistory.Count - 1];
            _backHistory.RemoveAt(_backHistory.Count - 1);
            
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
        ChangePage(baiduFileList, PathTextBox.Text, 1, insertBackHistory: false);

    }


    private void Back(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null) { return; }
        ChangePage(baiduFileList, GetBackPath(), 1, reserveForwardHistory:true);
    }
    private void Forward(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null) { return; }
        ChangePage(baiduFileList, GetForwardPath(), 1, insertBackHistory: true, reserveForwardHistory:true);

    }

    private void Home(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null) { return; }
        ChangePage(baiduFileList, "/", 1, insertBackHistory: false, reserveForwardHistory: false, reserveBackHistory: false);
    }
    private void Refresh(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (baiduFileList == null || currentPath == null) { return; }
        ChangePage(baiduFileList, currentPath, 1, insertBackHistory:false);
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

            if (isAtBottom && (baiduFileList != null && currentPath != null) && _canLoadMore)
            {
                ChangePage(baiduFileList, currentPath, _currentPageIndex + 1, false, true, true);
            }
        }
    }
    /// <summary>
    /// To get and show file list in UI, items count 1000 at max
    /// </summary>
    /// <param name="user">This function will use BaiduFileList to parse file list</param>
    /// <param name="goPath">Path need to parse, for ep. "/"</param>
    /// <param name="page">1: 1-1000 items, 2: 1001-2000 items...</param>
    /// <param name="insertBackHistory">Whether to add the path to the back history</param>
    /// <param name="reserveForwardHistory">Whether to reserve forward history</param>
    /// <param name="reserveBackHistory">Whether to reserve back history</param>

    private async void ChangePage(IFileList user, 
        string goPath, 
        int page, 
        bool insertBackHistory=true, 
        bool reserveForwardHistory=false,
        bool reserveBackHistory=true)
    {
        if (!reserveForwardHistory) { _forwardHistory.Clear(); }
        if (!reserveBackHistory) { _backHistory.Clear(); }
        if (insertBackHistory) { _backHistory.Add(goPath); }
        ForwardButton.IsEnabled = (_forwardHistory.Count == 0) ? false : true;
        BackButton.IsEnabled = ((_backHistory.Count == 1 && goPath == "/" && _backHistory[0] == "/") || _backHistory.Count == 0) ? false : true;

        PathTextBox.Text = goPath;
        currentPath = goPath;
        _currentPageIndex = page;

        if (page == 1) { FileListViewer.Opacity = 0; }

        FileList list_ = await user.GetFileList(page, path: goPath);
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
        if (!list_.Dir.Any() && !list_.File.Any())
        {
            FileListViewer.Opacity = 1;
            SelectAllOverlay.Children.Remove(selectAllButton);
            EmptyRemind emptyLabel = new EmptyRemind();
            emptyLabel.ShowError("不存在的文件夹\\n你来到了没有文件的荒原");
            FileListViewer.Children.Add(emptyLabel);
            return;
        }

        foreach (NetDir dirObj in list_.Dir)
        {
            itemCount++;
            ItemShowLine dirBlock = new ItemShowLine();
            DirDict.Add(dirObj, dirBlock);
            // set enter, refresh command
            dirBlock.Func = () => ChangePage(user, dirObj.Path, 1);
            dirBlock.Refresh = () => ChangePage(baiduFileList!, currentPath, 1, insertBackHistory: false);
            // set name, BDDir, user, OverelayGrid
            dirBlock.SelfDir = dirObj;

            dirBlock.Init(dirObj.Name, isDir: true);
            dirBlock.baiduFileList = user;
            
            dirBlock.TransferPage = TransferPage!;
            dirBlock.OverlayReservedGrid = OverlayReservedGrid;
            dirBlock.OverlayNotification = OverlayNotification;
            dirBlock.TaskProber = task_prober;
            dirBlock.ParentPath = currentPath;

            // append to viewer
            FileListViewer.Children.Add(dirBlock);

        }
        foreach (NetFile fileObj in list_.File)
        {
            itemCount++;

            ItemShowLine fileBlock = new ItemShowLine();
            FileDict.Add(fileObj, fileBlock);

            fileBlock.TransferPage = TransferPage!;
            // set enter, refresh command
            fileBlock.Func = () => ChangePage(user, fileObj.Path, 1);
            fileBlock.Refresh = () => ChangePage(baiduFileList!, currentPath, 1, insertBackHistory: false);
            // set name, BDDir, user, OverelayGrid
            fileBlock.SelfFile = fileObj;

            fileBlock.Init(fileObj.Name, isDir: false);
            fileBlock.baiduFileList = user;
            fileBlock.OverlayReservedGrid = OverlayReservedGrid;
            fileBlock.OverlayNotification = OverlayNotification;
            fileBlock.TaskProber = task_prober;

            fileBlock.ParentPath = currentPath;
            // append to viewer
            FileListViewer.Children.Add(fileBlock);
        }
        //FadeIn();
        _canLoadMore = (itemCount == 1000) ? true : false;
        FileListViewer.Opacity = 1;
        if (itemCount == 0)
        {

            SelectAllOverlay.Children.Remove(selectAllButton);

            EmptyRemind emptyLabel = new EmptyRemind();
            emptyLabel.OverlayReservedGrid = OverlayReservedGrid;
            emptyLabel.ParentPath = currentPath;
            emptyLabel.baiduFileList = user;
            emptyLabel.ParentPath = currentPath;
            emptyLabel.Refresh = () => ChangePage(baiduFileList!, currentPath, 1, insertBackHistory: false);

            FileListViewer.Children.Add(emptyLabel);
        }

    }
    /// <summary>
    /// To init user info, create and change to new page, update user name
    /// </summary>
    /// <param name="cookie">user cookie</param>
    public async Task<string> InitUser(string cookie)
    {
        INetdisk testUser = new Baidu(cookie);
        await testUser.Init();
        if (UpdateUserSectionName != null) {
            UpdateUserSectionName(testUser.GetAccountInfo().Name);
        }

        IFileList filelist = new BaiduFileList(testUser);
        baiduFileList = filelist;
        MeowSetting.InsertAccount(cookie, testUser.GetAccountInfo().Token);
        ChangePage(filelist, "/", 1);
        return testUser.GetAccountInfo().Token;
    }
    


}