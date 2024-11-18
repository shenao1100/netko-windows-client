using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk;
using System.Collections.Generic;
using System.Diagnostics;

namespace Netko;

public partial class NetdiskPage : UserControl
{
    private Dictionary<UserSection, UserControl> UserSectionPageDict = new Dictionary<UserSection, UserControl>();
    private UserControl NowShowPage;

    public TransferPage TransferPage { get; set; }
    public NetdiskPage()
    {
        InitializeComponent();
        UserSection UserSectionObj = new UserSection();
        LoginMainPage loginPage = new LoginMainPage();
        UserSectionObj.SetAvatar("avares://Netko/Assets/add.png", false);
        UserSectionObj.SetName("µÇÈë");
        UserSectionObj.ChangeAction = () => ChangePage(loginPage);
        UserSectionObj.DeleteAction = () => RemovePage(loginPage, UserSectionObj);
        // UserSectionPageDict.Add(UserSectionObj, loginPage);
        ChangePage(loginPage);
        UserSectionDockPanel.Children.Add(UserSectionObj);
        

        // login server to receive cookie
        CookieReciver cookieReciver = new CookieReciver();
        cookieReciver.Listen();
        // call this func when receive cookie
        cookieReciver.CallBack = (cookie) => AddAccount(cookie);

        
    }
    public void init()
    {
        // init user from config file
        foreach (AccountStruct account in MeowSetting.GetAllAccount())
        {
            if (account.cookie == null) continue;
            AddAccount(account.cookie);
        }
    }
    private void RemoveAccount(UserControl FilePage,  UserControl UserSectionObj, string token)
    {
        MeowSetting.RemoveAccount(token);
        RemovePage(FilePage, UserSectionObj);
    }
    /// <summary>
    /// Create a new file page and user section
    /// </summary>
    /// <param name="cookie">user cookie</param>
    private async void AddAccount(string cookie)
    {
        UserSection UserSectionObj = new UserSection();
        NetdiskFilePage FilePage = new NetdiskFilePage();
        Trace.WriteLine(TransferPage.ToString());
        FilePage.TransferPage = TransferPage;
        UserSectionObj.ChangeAction = () => ChangePage(FilePage);
        
        FilePage.UpdateUserSectionName = (name) => UserSectionObj.SetName(name);
        ChangePage(FilePage);
        UserSectionDockPanel.Children.Add(UserSectionObj);
        string token = await FilePage.initUser(cookie);

        UserSectionObj.DeleteAction = () => RemoveAccount(FilePage, UserSectionObj, token);
    }
    /// <summary>
    /// Remove user section and file page
    /// </summary>
    /// <param name="page"></param>
    /// <param name="user_section"></param>
    public void RemovePage(UserControl page, UserControl user_section)
    {
        UserSectionDockPanel.Children.Remove(user_section);

        if (page == NowShowPage)
        {
            FileListGrid.Children.Remove(page);
        }
    }
    /// <summary>
    /// change current page into Page
    /// </summary>
    /// <param name="Page"></param>
    public void ChangePage(UserControl Page)
    {
        FileListGrid.Children.Clear();
        FileListGrid.Children.Add(Page);
        NowShowPage = Page;
    }
    /// <summary>
    /// Create Login page and usersection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddLoginPage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UserSection UserSectionObj = new UserSection();
        LoginMainPage loginPage = new LoginMainPage();
        UserSectionObj.SetAvatar("avares://Netko/Assets/add.png", false);
        UserSectionObj.SetName("µÇÈë");
        UserSectionObj.ChangeAction = () => ChangePage(loginPage);
        UserSectionObj.DeleteAction = () => RemovePage(loginPage, UserSectionObj);
        // UserSectionPageDict.Add(UserSectionObj, loginPage);
        ChangePage(loginPage);
        UserSectionDockPanel.Children.Add(UserSectionObj);

    }
}