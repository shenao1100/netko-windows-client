using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk;
using System.Collections.Generic;

namespace Netko;

public partial class NetdiskPage : UserControl
{
    private Dictionary<UserSection, UserControl> UserSectionPageDict = new Dictionary<UserSection, UserControl>();
    private UserControl NowShowPage;

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
    /// <summary>
    /// Create a new file page and user section
    /// </summary>
    /// <param name="cookie">user cookie</param>
    private void AddAccount(string cookie)
    {
        UserSection UserSectionObj = new UserSection();
        NetdiskFilePage FilePage = new NetdiskFilePage();
        UserSectionObj.ChangeAction = () => ChangePage(FilePage);
        UserSectionObj.DeleteAction = () => RemovePage(FilePage, UserSectionObj);
        FilePage.UpdateUserSectionName = (name) => UserSectionObj.SetName(name);
        ChangePage(FilePage);
        UserSectionDockPanel.Children.Add(UserSectionObj);
        FilePage.initUser(cookie);

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