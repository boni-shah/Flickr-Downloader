using Flickr_Set_Downloader.Classes;
using System.Diagnostics;
using System.Windows;

namespace Flickr_Set_Downloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var thisProc = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                MessageBox.Show(Constants.OneInstanceAllowedmsg, Constants.UserErrormsg);
                Current.Shutdown();
                return;
            }
            base.OnStartup(e);
        }
    }
}
