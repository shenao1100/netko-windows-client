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
    public BaiduFileList? baiduFileList;
    public BDDir SelfDir;
    public BDFile SelfFile;
    public bool isFile;
    public bool isDir;
    private bool is_selected = false;
    public Grid OverlayReservedGrid { get; set; }

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
            BorderBackground[!Border.BackgroundProperty] = new DynamicResourceExtension("CatalogBaseMediumColor");
        }
        else
        {
            BorderBackground[!Border.BackgroundProperty] = new DynamicResourceExtension("CatalogBaseHighColor");
            //BorderBackground.Background = new SolidColorBrush(LeaveBG);
        }

    }

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
    }


    private void MouseInput(object sender, PointerPressedEventArgs e)
    {
        return;
        var textBlock = sender as Button;
        var pointerPoint = e.GetCurrentPoint(this);

        // 检查哪个按键被按下
        if (pointerPoint.Properties.IsRightButtonPressed)
        {
            
            if (baiduFileList != null && baiduFileList.GetSelectedItem() == null)
            {
                DockpanelOpen.IsVisible = true;
                DockpanelDuplicate.IsVisible = true;

            }
            else
            {
                DockpanelOpen.IsVisible = false;
                DockpanelDuplicate.IsVisible = false;
            }
        }
    }

    private void LeftClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var currentTime = DateTime.Now;
        var timeDiff = currentTime - lastClickedTime;
        lastClickedTime = currentTime;
        
        // Trace.WriteLine(timeDiff.TotalMilliseconds.ToString());
        if (timeDiff.TotalMilliseconds <= 250)
        {
            Func();
        }
        else
        {
            if (!baiduFileList.DirIsSelected(SelfDir))
            {
                is_selected = true;
            }
            else
            {
                is_selected = false;

            }
            UpdateColor();
            baiduFileList.ToggleSelectDir(SelfDir);
        }
    }

    private void OepnOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Func();
    }

    private async void NewFolderOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DialogOverlay inputName = new DialogOverlay();
        OverlayReservedGrid.Children.Add(inputName);
        string? filename = await inputName.ShowDialog("请输入新建文件夹的名称", "创建");
        if (await baiduFileList.CreateDir(ParentPath + "/" + filename))
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
        if (!string.IsNullOrEmpty(target_path) && await baiduFileList.Copy([self_path], [self_name], [target_path]))
        {
            Refresh();
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
        if (!string.IsNullOrEmpty(target_path) && await baiduFileList.Move([self_path], [self_name], [target_path])) {
            Refresh();
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
        if (await baiduFileList.Rename([self_path], [filename]))
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
    private async void DeleteOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string delete_filelist, self_name;
        if (isDir)
        {
            BDDir[] dirlist = new BDDir[1];
            dirlist[0] = SelfDir;
            delete_filelist = baiduFileList.IntegrateFilelist(null, dirlist);
            self_name = SelfDir.Name;
        }
        else
        {
            BDFile[] filelist = new BDFile[1];
            filelist[0] = SelfFile;
            delete_filelist = baiduFileList.IntegrateFilelist(filelist, null);
            self_name = SelfFile.Name;
        }
        
        Trace.WriteLine(delete_filelist);
        if (await baiduFileList.DeleteFile(delete_filelist)){
            Refresh();
            return;
        }
        else
        {
            MessageOverlay message = new MessageOverlay();
            OverlayReservedGrid.Children.Add(message);
            message.SetMessage("删除失败", $"删除 {self_name} 时遇到错误");

            return;
        }

    }
    private void DockPanel_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Trace.WriteLine("dir rgclicked");
    }
    private async void ShareOnMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
}