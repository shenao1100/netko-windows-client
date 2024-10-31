using Avalonia.Media;
using System.Reflection;

namespace Netko.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    public IBrush sdadad => Brushes.Black;
    public int MainViewMenuButtonWidth => 130;
    public string AppVersion { get; }


    //public IBrush sdadada => Brushes.;
    public MainViewModel()
    {
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Version not found";
    }

}
