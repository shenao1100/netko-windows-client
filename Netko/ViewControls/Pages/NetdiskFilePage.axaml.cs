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
namespace Netko;

public partial class NetdiskFilePage : UserControl
{
    public const string HomePath = "";
    public Action<string>? UpdateUserSectionName;

    // for history use
    private List<string> backHistory = new List<string>();
    private List<string> forwardHistory = new List<string>();
    private string currentPath;
    private BaiduFileList baiduFileList;

    // for file action use
    private List<BDDir> selectDirList = new List<BDDir>();
    private List<BDFile> selectFileList = new List<BDFile>();
    // for color changing use
    private Color selectedColor;
    private Color unselectedColor;
    public Grid OverlayReservedGrid { get; set; }
    public TransferPage TransferPage { get; set; }

    public NetdiskFilePage()
    {
        InitializeComponent();
        GetColor();
        OverlayReservedGrid = OverlayReserved;
        // FileListViewer.PointerPressed += FileListViewerOnClick;
    }

    /*private void FileListViewerOnClick(object sender, PointerPressedEventArgs e)
    {
        Trace.WriteLine("btn pres");
        if (e.GetCurrentPoint(FileListViewer).Properties.IsRightButtonPressed && !ActionMenuPopup.IsOpen)
        {
            ActionMenuPopup.IsOpen = true;
        }
        else
        {
            ActionMenuPopup.IsOpen = false;
        }
    }*/
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

        await Task.Delay(150);
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
        await Task.Delay(150);
    }
    public void GetColor()
    {
        var CBH_backgound = Application.Current.Resources.TryGetResource("CatalogBaseHighColor", null, out var Hresource);
        if (CBH_backgound && Hresource is Color Backgound)
        {
            selectedColor = Backgound;
        }
        var CBL_background = Application.Current.Resources.TryGetResource("CatalogBaseHighColor", null, out var Lresource);
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
    private async void GotoDir(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ChangePage(baiduFileList, PathTextBox.Text, 1, insert_back_history: false);

    }


    private async void Back(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ChangePage(baiduFileList, GetBackPath(), 1, reserve_forward_history:true);
    }
    private async void Forward(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ChangePage(baiduFileList, GetForwardPath(), 1, insert_back_history: true, reserve_forward_history:true);

    }

    private async void Home(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ChangePage(baiduFileList, "/", 1, insert_back_history: false);
    }
    private async void Refresh(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ChangePage(baiduFileList, currentPath, 1, insert_back_history:false);
    }
    /// <summary>
    /// To get and show file list in UI, items count 1000 at max
    /// </summary>
    /// <param name="user">This function will use BaiduFileList to parse file list</param>
    /// <param name="go_path">Path need to parse, for ep. "/"</param>
    /// <param name="page">1: 1-1000 items, 2: 1001-2000 items...</param>
    private async void ChangePage(BaiduFileList user, string go_path, int page, bool insert_back_history=true, bool reserve_forward_history=false)
    {
        if (!reserve_forward_history)
        {
            forwardHistory.Clear();
        }
        if (insert_back_history)
        {
            backHistory.Add(go_path);
        }
        if (forwardHistory.Count == 0) 
        {
            ForwardButton.IsEnabled = false;
        }
        else
        {
            ForwardButton.IsEnabled = true;
        }
        if ((backHistory.Count == 1 && go_path == "/" && backHistory[0] == "/") || backHistory.Count == 0)
        {
            BackButton.IsEnabled = false;
        }
        else
        {
            BackButton.IsEnabled = true;
        }
        
        PathTextBox.Text = go_path;
        currentPath = go_path;
        //FadeOut();
        FileListViewer.Opacity = 0;

        BDFileList list_ = await user.GetFileList(1, path:go_path);
        FileListViewer.Children.Clear();

        foreach (BDDir dir_b in list_.Dir)
        {
            ItemShowLine DirBlock = new ItemShowLine();
            // set enter, refresh command
            DirBlock.Func = () => ChangePage(user, dir_b.Path, 1);
            DirBlock.Refresh = () => ChangePage(baiduFileList, currentPath, 1, insert_back_history: false);
            // set name, BDDir, user, OverelayGrid
            DirBlock.SelfDir = dir_b;

            DirBlock.Init(dir_b.Name, is_dir: true);
            DirBlock.baiduFileList = user;
            DirBlock.OverlayReservedGrid = OverlayReservedGrid;
            DirBlock.ParentPath = currentPath;
            // append to viewer
            FileListViewer.Children.Add(DirBlock);

        }
        foreach (BDFile file_b in list_.File)
        {
            ItemShowLine FileBlock = new ItemShowLine();
            FileBlock.TransferPage = TransferPage;
            // set enter, refresh command
            FileBlock.Func = () => ChangePage(user, file_b.Path, 1);
            FileBlock.Refresh = () => ChangePage(baiduFileList, currentPath, 1, insert_back_history: false);
            // set name, BDDir, user, OverelayGrid
            FileBlock.SelfFile = file_b;

            FileBlock.Init(file_b.Name, is_dir: false);
            FileBlock.baiduFileList = user;
            FileBlock.OverlayReservedGrid = OverlayReservedGrid;
            FileBlock.ParentPath = currentPath;
            // append to viewer
            FileListViewer.Children.Add(FileBlock);
        }
        //FadeIn();
        FileListViewer.Opacity = 1;


    }
    /// <summary>
    /// To init user info, create and change to new page, update user name
    /// </summary>
    /// <param name="cookie">user cookie</param>
    public async Task<string> initUser(string cookie)
    {
        Baidu test_user = new Baidu(cookie);
        await test_user.init();
        if (UpdateUserSectionName != null) {
            UpdateUserSectionName(test_user.name);
        }

        BaiduFileList Filelist = new BaiduFileList(test_user);
        baiduFileList = Filelist;
        MeowSetting.InsertAccount(cookie, test_user.bdstoken);
        ChangePage(Filelist, "/", 1);
        return test_user.bdstoken;
    }
    


}