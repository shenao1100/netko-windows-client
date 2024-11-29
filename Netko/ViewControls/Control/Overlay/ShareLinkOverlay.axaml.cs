using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using TextCopy;
using System;
using Avalonia.Media;
using Netko.NetDisk;

namespace Netko;

public partial class ShareLinkOverlay : UserControl
{
    public IFileList baiduFileList {  get; set; }
    public BDFile[] FileList { get; set; }
    public BDDir[] DirList { get; set; }

    private string ShareURL { get; set; }
    private string Password { get; set; }

    public ShareLinkOverlay()
    {
        InitializeComponent();
        InputPasswordTextBox.Text = GenRandomPwd();
        ShowSection.Height = 0;
        ShowSection.Opacity = 0;
    }
    private string GenRandomPwd()
    {
        string pwd = "";
        for (int i = 0; i < 4; i++)
        {
            string chars = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            pwd += chars[new Random().Next(chars.Length - 1)];
        }
        return pwd;
    }
    private async void Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Opacity = 0;
        await Task.Delay(200);
        this.IsVisible = false;
    }
    private async void GetShareLink(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int period = 1;
        switch (LastPeriodComboBox.SelectedIndex)
        {
            case 0:
                period = 1;
                break;
            case 1:
                period = 7;
                break;
            case 2:
                period = 30;
                break;
            case 3:
                period = 365;
                break;
            case 4:
                period = 0;
                break;
            default: period = 1; break;
        }
        if (InputPasswordTextBox.Text != null && InputPasswordTextBox.Text.Length == 4 && !Convert.ToString(InputPasswordTextBox.Text).Contains("_"))
        {
            SettingSection.Opacity = 0;

            await Task.Delay(200);
            SettingSection.Height = 0;

            await Task.Delay(200);

            //SettingSection.IsVisible = false;
            Password = InputPasswordTextBox.Text;
            ShowPasswordLabel.Content = Password;
            
            ShowSection.IsVisible = true;

            string? share_link = await baiduFileList.ShareFile(
                baiduFileList.IntegrateIDlist(FileList, DirList),
                InputPasswordTextBox.Text,
                period
            );

            //string share_link = "123456";
            if (share_link != null)
            {
                ShareLinkLabel.Content = share_link;
                ShareURL = share_link;
            }
            else
            {
                return;
            }
            

            ShowSection.Height = 250;
            ShowSection.Opacity = 1;
        }
        else
        {
            InfromMsg.Foreground = new SolidColorBrush(Color.Parse("#FFff2525"));
            return;
        }
    }

    private void CopyShareLink(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ClipboardService.SetText(ShareURL);
    }
    private void CopyShareLinkWithPwd(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ClipboardService.SetText(ShareURL + "&pwd=" + Password);
    }
    private void CopyPwd(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ClipboardService.SetText(Password);
    }
}