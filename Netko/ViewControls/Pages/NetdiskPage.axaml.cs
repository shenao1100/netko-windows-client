using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Netko.NetDisk;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Netko;

public partial class NetdiskPage : UserControl
{
    private Dictionary<UserSection, UserControl> UserSectionPageDict = new Dictionary<UserSection, UserControl>();
    private UserControl _userControl;

    private int _currentPageIndex = 0;
    private int _pageIndex = 0;
    public TransferPage TransferPage { get; set; }
    public NetdiskPage()
    {
        InitializeComponent();
        UserSection userSectionObj = new UserSection();
        LoginMainPage loginPage = new LoginMainPage();
        userSectionObj.SetAvatar("avares://Netko/Assets/add.png", false);
        userSectionObj.SetName("添加账号");
        _pageIndex++;
        int thisPageIndex = _pageIndex;
        userSectionObj.ChangeAction = () => ChangePage(loginPage, thisPageIndex);
        userSectionObj.DeleteAction = () => RemovePage(loginPage, userSectionObj);
        // UserSectionPageDict.Add(UserSectionObj, loginPage);
        ChangePage(loginPage, thisPageIndex);
        UserSectionDockPanel.Children.Add(userSectionObj);
        

        // login server to receive cookie
        CookieReciver cookieReciver = new CookieReciver();
        cookieReciver.Listen();
        // call this func when receive cookie
        cookieReciver.CallBack = (cookie) => AddAccount(cookie);

        
    }
    /// <summary>
    /// init user from config file
    /// </summary>
    public void Init()
    {
        // init user from config file
        foreach (AccountStruct account in MeowSetting.GetAllAccount())
        {
            if (account.cookie == null) continue;
            AddAccount(account.cookie);
        }
    }
    /// <summary>
    /// Remove account from both config file and page
    /// </summary>
    /// <param name="filePage"></param>
    /// <param name="userSectionObj"></param>
    /// <param name="token"></param>
    private void RemoveAccount(UserControl filePage,  UserControl userSectionObj, string token)
    {
        MeowSetting.RemoveAccount(token);
        RemovePage(filePage, userSectionObj);
    }
    /// <summary>
    /// Create a new file page and user section
    /// </summary>
    /// <param name="cookie">user cookie</param>
    private async void AddAccount(string cookie)
    {
        UserSection userSectionObj = new UserSection();
        NetdiskFilePage filePage = new NetdiskFilePage();
        _pageIndex++;
        int thisPageIndex = _pageIndex;
        Trace.WriteLine(TransferPage.ToString());
        filePage.TransferPage = TransferPage;
        userSectionObj.ChangeAction = () => ChangePage(filePage, thisPageIndex);
        
        filePage.UpdateUserSectionName = (name) => userSectionObj.SetName(name);
        ChangePage(filePage, thisPageIndex);
        UserSectionDockPanel.Children.Add(userSectionObj);
        string token = await filePage.InitUser(cookie);

        userSectionObj.DeleteAction = () => RemoveAccount(filePage, userSectionObj, token);
    }
    /// <summary>
    /// Remove user section and file page
    /// </summary>
    /// <param name="page"></param>
    /// <param name="userSection"></param>
    public void RemovePage(UserControl page, UserControl userSection)
    {
        UserSectionDockPanel.Children.Remove(userSection);

        if (page == _userControl)
        {
            FileListGrid.Children.Remove(page);
        }
    }
    /// <summary>
    /// Page change animation
    /// </summary>
    private async void FlyFromRight()
    {
        var raiseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.15),
            Easing = new QuadraticEaseInOut(),
            Children = {
                new KeyFrame
                {
                Cue = new Cue(0),

                Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(200, 0, -200, 0)),
                        new Setter(UserControl.OpacityProperty, 0.3)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(0, 0, 0, 0)),
                        new Setter(UserControl.OpacityProperty, 1.0)
                    }
                }

            },

        };
        await raiseAnimation.RunAsync(FileListGrid);
    }
    /// <summary>
    /// Page change animation
    /// </summary>
    private async void FlyFromLeft()
    {
        var raiseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.15),
            Easing = new QuadraticEaseInOut(),
            Children = {
                new KeyFrame
                {
                Cue = new Cue(0),

                Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(-200, 0, 200, 0)),
                        new Setter(UserControl.OpacityProperty, 0.3)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(UserControl.MarginProperty, new Thickness(0, 0, 0, 0)),
                        new Setter(UserControl.OpacityProperty, 1.0)
                    }
                }

            },

        };
        await raiseAnimation.RunAsync(FileListGrid);
    }
    /// <summary>
    /// change current page into Page
    /// </summary>
    /// <param name="page">Page you want to switch to</param>
    /// <param name="pageIndex">Index of this page, control animation direction</param>
    public async void ChangePage(UserControl page, int pageIndex)
    {
        

        FileListGrid.Children.Clear();
        FileListGrid.Children.Add(page);
        _userControl = page;
        if (pageIndex < _currentPageIndex)
        {
            FlyFromLeft();
        }
        else if (pageIndex > _currentPageIndex)
        {
            FlyFromRight();
        }
        _currentPageIndex = pageIndex;
        //await RaiseAnimation.RunAsync(FileListGrid);
    }
    /// <summary>
    /// Create Login page and usersection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddLoginPage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _pageIndex++;
        UserSection userSectionObj = new UserSection();
        LoginMainPage loginPage = new LoginMainPage();
        userSectionObj.SetAvatar("avares://Netko/Assets/add.png", false);
        userSectionObj.SetName("添加账号");
        int thisPageIndex = _pageIndex;
        userSectionObj.ChangeAction = () => ChangePage(loginPage, thisPageIndex);
        userSectionObj.DeleteAction = () => RemovePage(loginPage, userSectionObj);
        // UserSectionPageDict.Add(UserSectionObj, loginPage);
        ChangePage(loginPage, thisPageIndex);
        UserSectionDockPanel.Children.Add(userSectionObj);

    }
}