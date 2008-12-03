using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace WowArmoryScreenSaverInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!File.Exists("wowarmory.scr"))
            {
                // TODO notify user
                Application.Current.Shutdown();
                return;
            }

            ChangelogWindow window = new ChangelogWindow();
            window.Closed += new EventHandler(window_Closed);
            window.Show();
        }

        void window_Closed(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("rundll32.exe", "desk.cpl,InstallScreenSaver " + Path.GetFullPath(".\\wowarmory.scr"));
            Application.Current.Shutdown();
        }
    }
}
