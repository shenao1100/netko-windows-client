using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Netko.NetDisk.Baidu;
using System.Diagnostics;

namespace Netko;

public partial class NetdiskFilePage : UserControl
{
    public NetdiskFilePage()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UserControl FileBlock = new FileShowLine();
        FileListViewer.Children.Add(FileBlock);
    }
    private async void NetdiskParse(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string cookie = "BIDUPSID=E6C2932FDD4F4E5108E4E89472955D16; PSTM=1706332120; BAIDUID=8F842A56FD7CF7E8AF1CAF9AC2AD787D:FG=1; PANWEB=1; BDUSS=3U0bVpsemJYRUw1N3cwamR5fndZNlZWMXh1bWRzeEhIRjdJMC1qN2R0YVNQa2RtRVFBQUFBJCQAAAAAAAAAAAEAAAAZMFsoxcvW0MG8NQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJKxH2aSsR9mN; BDUSS_BFESS=3U0bVpsemJYRUw1N3cwamR5fndZNlZWMXh1bWRzeEhIRjdJMC1qN2R0YVNQa2RtRVFBQUFBJCQAAAAAAAAAAAEAAAAZMFsoxcvW0MG8NQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJKxH2aSsR9mN; MCITY=-%3A; BAIDUID_BFESS=8F842A56FD7CF7E8AF1CAF9AC2AD787D:FG=1; H_PS_PSSID=60269_60274_60288_60297_60253; BAIDU_WISE_UID=wapp_1716380697519_799; STOKEN=9f04d2bfd18145933110c7994b9b23963b2a7d7b3eef2ac0c2ee69ea07b316f0; BDCLND=w9by%2F%2BUn%2BnYLMwGNoKjN7y42biN5kfqmHQ3bhjU3Lg8%3D; __bid_n=18fecac790a612815cc4dc; Hm_lvt_7a3960b6f067eb0085b7f96ff5e660b0=1716903338,1717163104,1717212966,1718027926; ndut_fmt=C73E68288F214953DB96F29A9ABF0F31841863A6AA96E825AFE7ABD88BC987D3; ZFY=a3064XUEvLtDAppedlsx1:AC2YfVISjS8R:A:AFW2stDsM:C; RT=\"z=1&dm=baidu.com&si=f322da3b-87a5-47d3-bce9-ae8fcf4d91dc&ss=lxg78ttp&sl=6&tt=dpb&bcn=https%3A%2F%2Ffclog.baidu.com%2Flog%2Fweirwood%3Ftype%3Dperf&ld=40jh&ul=3ylyp&hd=3ym01\"";
        Baidu test_user = new Baidu(cookie);
        await test_user.initial_vars();
        // Trace.WriteLine("Thread 1");

        // await test_user.initial_vars();
        // Trace.WriteLine("Thread");
    }

}