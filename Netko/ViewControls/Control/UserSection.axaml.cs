using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Netko.NetDisk;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using TextCopy;

namespace Netko;

public partial class UserSection : UserControl
{

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

    public Action DeleteAction;
    public Action ChangeAction;
    public INetdisk? NetdiskInsdence = null ;
    public UserSection()
    {
        InitializeComponent();
    }

    public void SetName(string name)
    {
        UserName.Content = name;
    }

    public void SetAvatar(string path, bool is_web)
    {
        var uri = new Uri(path);
        if (!is_web) 
        {
            var bitmap = ImageHelper.LoadFromResource(uri);
            UserHeadPhoto.Source = bitmap;

        }
        else
        {
            var bitmap = ImageHelper.LoadFromWeb(uri);
            UserHeadPhoto.Source = bitmap.Result;

        }
    }
    private void CpoyCookie(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (NetdiskInsdence != null)
        {
            ClipboardService.SetText(NetdiskInsdence.GetCookie());

        }
    }
    public void SetAsNetdisk(INetdisk netdisk)
    {
        NetdiskInsdence = netdisk;
        AccountInfo info = netdisk.GetAccountInfo();

    }
    private void RemoveBtClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DeleteAction != null)
        {
            DeleteAction();

        }
    }
    private void PageChangeBtClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ChangeAction != null)
        {

            ChangeAction();
        }
        
    }
}