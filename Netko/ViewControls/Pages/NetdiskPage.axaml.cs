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
    private UserControl NowShowPage;

    private int CurrentPageIndex = 0;
    private int PageIndex = 0;
    public TransferPage TransferPage { get; set; }
    public NetdiskPage()
    {
        InitializeComponent();
        UserSection UserSectionObj = new UserSection();
        LoginMainPage loginPage = new LoginMainPage();
        UserSectionObj.SetAvatar("avares://Netko/Assets/add.png", false);
        UserSectionObj.SetName("µÇÈë");
        PageIndex++;
        int ThisPageIndex = PageIndex;
        UserSectionObj.ChangeAction = () => ChangePage(loginPage, ThisPageIndex);
        UserSectionObj.DeleteAction = () => RemovePage(loginPage, UserSectionObj);
        // UserSectionPageDict.Add(UserSectionObj, loginPage);
        ChangePage(loginPage, ThisPageIndex);
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
        PageIndex++;
        int ThisPageIndex = PageIndex;
        Trace.WriteLine(TransferPage.ToString());
        FilePage.TransferPage = TransferPage;
        UserSectionObj.ChangeAction = () => ChangePage(FilePage, ThisPageIndex);
        
        FilePage.UpdateUserSectionName = (name) => UserSectionObj.SetName(name);
        ChangePage(FilePage, ThisPageIndex);
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

    private async void FlyFromRight()
    {
        var RaiseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.18),
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
        await RaiseAnimation.RunAsync(FileListGrid);
    }
    private async void FlyFromLeft()
    {
        var RaiseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.18),
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
        await RaiseAnimation.RunAsync(FileListGrid);
    }
    /// <summary>
    /// change current page into Page
    /// </summary>
    /// <param name="Page"></param>
    public async void ChangePage(UserControl Page, int PageIndex)
    {
        

        FileListGrid.Children.Clear();
        FileListGrid.Children.Add(Page);
        NowShowPage = Page;
        if (PageIndex < CurrentPageIndex)
        {
            FlyFromLeft();
        }
        else if (PageIndex > CurrentPageIndex)
        {
            FlyFromRight();
        }
        CurrentPageIndex = PageIndex;
        //await RaiseAnimation.RunAsync(FileListGrid);
    }
    /// <summary>
    /// Create Login page and usersection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddLoginPage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        PageIndex++;
        UserSection UserSectionObj = new UserSection();
        LoginMainPage loginPage = new LoginMainPage();
        UserSectionObj.SetAvatar("avares://Netko/Assets/add.png", false);
        UserSectionObj.SetName("µÇÈë");
        int ThisPageIndex = PageIndex;
        UserSectionObj.ChangeAction = () => ChangePage(loginPage, ThisPageIndex);
        UserSectionObj.DeleteAction = () => RemovePage(loginPage, UserSectionObj);
        // UserSectionPageDict.Add(UserSectionObj, loginPage);
        ChangePage(loginPage, ThisPageIndex);
        UserSectionDockPanel.Children.Add(UserSectionObj);

    }
}