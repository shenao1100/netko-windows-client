using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace Netko;

public partial class NetdiskFilePage : UserControl
{
    public const string HomePath = "";

    public NetdiskFilePage()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UserControl FileBlock = new FileShowLine();
        
        FileListViewer.Children.Add(FileBlock);

    }
    private async void ChangePage(BaiduFileList user, string go_path, int page)
    {
        Trace.WriteLine("CHANGE");
        FileListViewer.Children.Clear();
        BDFileList list_ = await user.GetFileList(1, path:go_path);
        foreach (BDDir dir_b in list_.Dir)
        {
            DirShowLine DirBlock = new DirShowLine();
            DirBlock.SetName(dir_b.Name);
            Trace.WriteLine("Addfunc");
            DirBlock.Func = () => ChangePage(user, dir_b.Path, 1);

            FileListViewer.Children.Add(DirBlock);


        }
        foreach (BDFile file_b in list_.File)
        {
            FileShowLine FileBlock = new FileShowLine();
            FileBlock.SetName(file_b.Name);
            FileListViewer.Children.Add(FileBlock);

            Trace.WriteLine(file_b.ToString());
        }
    }
    private async void NetdiskParse(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        /*var button = sender as Button;
        if (button == null) { return; }
        button.IsEnabled = false;
        try
        {*/
            string cookie = "BAIDUID_BFESS=053A9BCAA181F6052A8D3D0FD18A9BA9:FG=1; ZFY=9T9J44YsMv7pVWfXh:AJdw:AT3:ADbH7y9xj67FEC:Afn:B8:C; PANWEB=1; PSTM=1727963670; H_PS_PSSID=60770_60844; BIDUPSID=E6C2932FDD4F4E5108E4E89472955D16; BDCLND=r%2BNIyZLRd8xKNwAERcw%2BkbnQcV5VzWn3Bl7epMk3CUI%3D; H_WISE_SIDS=110085_603321_307086_618462_618439_619527_619881_620008_613974_620871_621318_621398_610631_621478_621633_621725_620180_621040_622288_622223_622348_614595_620092_622512_622513_622493_622495_622731_1991638_1991570_311849_1991970_1992001_1991941_1992028_1992069_615157_1992049_1992043_617086_622907_622886_620472_1991789_623159_622439_623271_623458_620485_623601_617569_623046_623663_612886_623100_612517_623785_1991728_623757_621954_623889_623884_607111_623756_622733_623961_623996_623908_624009_624002_624014_624017_624023_622640_622376_624168_624049_622875_624227_624289_624264_624282_612952_624381_624247_624416_620145_624465_624481_624486_624487; H_WISE_SIDS_BFESS=110085_603321_307086_618462_618439_619527_619881_620008_613974_620871_621318_621398_610631_621478_621633_621725_620180_621040_622288_622223_622348_614595_620092_622512_622513_622493_622495_622731_1991638_1991570_311849_1991970_1992001_1991941_1992028_1992069_615157_1992049_1992043_617086_622907_622886_620472_1991789_623159_622439_623271_623458_620485_623601_617569_623046_623663_612886_623100_612517_623785_1991728_623757_621954_623889_623884_607111_623756_622733_623961_623996_623908_624009_624002_624014_624017_624023_622640_622376_624168_624049_622875_624227_624289_624264_624282_612952_624381_624247_624416_620145_624465_624481_624486_624487; newlogin=1; BDUSS=FrT0NIRTRXUE00Qjk3YlI2VUpzeE1GV0V1QU1yeVUxc011Q1N1eWU3Mnk3akZuRVFBQUFBJCQAAAAAAQAAAAEAAABO-55-vrY0d280czFsYzIwYncAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALJhCmeyYQpna2; BDUSS_BFESS=FrT0NIRTRXUE00Qjk3YlI2VUpzeE1GV0V1QU1yeVUxc011Q1N1eWU3Mnk3akZuRVFBQUFBJCQAAAAAAQAAAAEAAABO-55-vrY0d280czFsYzIwYncAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALJhCmeyYQpna2; STOKEN=9927a3e6ee12a31682ed825b93a7e0a4a56fc993f50dca3fa8bb4211b6bacbd6; Hm_lvt_182d6d59474cf78db37e0b2248640ea5=1728815474; BCLID=8123825881063429834; BCLID_BFESS=8123825881063429834; BDSFRCVID=GmCOJeC6270jbvoJBXp_UShuitEp5_JTH6_nKeJZbRnjNkn2VXEFEG0PRM8g0Kuh5b8yogKK02OTHzLF_2uxOjjg8UtVJeC6EG0Ptf8g0x5; BDSFRCVID_BFESS=GmCOJeC6270jbvoJBXp_UShuitEp5_JTH6_nKeJZbRnjNkn2VXEFEG0PRM8g0Kuh5b8yogKK02OTHzLF_2uxOjjg8UtVJeC6EG0Ptf8g0x5; H_BDCLCKID_SF=JRkHoKPbtDD3jR5m-R5HMtCyqxby2C62aJ3xWlOvWJ5TMC_6D-6D544pLR7jyhQg2TT0Lp5MJx7kShPCBPTvbfIl-R5Xaf6WaJrphp5I3l02VMoEe-t2ynLJjfKOh4RMW20joq7mWPcdsxA45J7cM4IseboJLfT-0bc4KKJxbnLWeIJ9jj6jK4JKDHLOJfK; H_BDCLCKID_SF_BFESS=JRkHoKPbtDD3jR5m-R5HMtCyqxby2C62aJ3xWlOvWJ5TMC_6D-6D544pLR7jyhQg2TT0Lp5MJx7kShPCBPTvbfIl-R5Xaf6WaJrphp5I3l02VMoEe-t2ynLJjfKOh4RMW20joq7mWPcdsxA45J7cM4IseboJLfT-0bc4KKJxbnLWeIJ9jj6jK4JKDHLOJfK; csrfToken=RZ8B9b_19Nssr1ePV0F9XQHu; ndut_fmt=8BC46F052DAA6AFA92466C0B7F9077A345516196762798814848E7A2D36C8427";
            Baidu test_user = new Baidu(cookie);
            await test_user.init();
            BaiduFileList Filelist = new BaiduFileList(test_user);
            BDFileList list_ = await Filelist.GetFileList(1);
        foreach (BDDir dir_b in list_.Dir)
        {
            DirShowLine DirBlock = new DirShowLine();
            DirBlock.SetName(dir_b.Name);
            FileListViewer.Children.Add(DirBlock);
            DirBlock.Func = () => ChangePage(Filelist, dir_b.Path, 1);

        }
        foreach (BDFile file_b in list_.File)
            {
            FileShowLine FileBlock = new FileShowLine();
            FileBlock.SetName(file_b.Name);
            FileListViewer.Children.Add(FileBlock);

            Trace.WriteLine(file_b.ToString());
            }
            
        /*}
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            button.IsEnabled = true;
        }*/


        // BaiduFileList Filelist = new BaiduFileList(test_user);

        //await test_user.initial_vars();
        // Trace.WriteLine("Thread 1");

        // await test_user.initial_vars();
        // Trace.WriteLine("Thread");
    }

}