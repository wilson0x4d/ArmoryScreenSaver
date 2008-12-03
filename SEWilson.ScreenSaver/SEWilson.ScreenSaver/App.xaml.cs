using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace SEWilson.ScreenSaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            IntPtr parentHwnd = IntPtr.Zero;

            //if
            //    //(true)
            //    (System.Diagnostics.Debugger.IsAttached)
            //{
            //    this.MainWindow = new InstallerWindow();
            //}
            //else
            {
                for (int i = 0; i < e.Args.Length; i++)
                {
                    string arg = e.Args[i];
                    switch (arg.Substring(0, 2).ToLower())
                    {
                        case "/i": // inspector window
                            {
                                this.MainWindow = new SEWilson.ScreenSaver.Util.RequestInspectorWindow();
                            }
                            break;
                        case "/p": // preview mode
                            {
                                parentHwnd = ExtractParentHwnd(e, i);
                                this.MainWindow = new PreviewWindow();
                            }
                            break;
                        case "/c": // config
                            {
                                this.MainWindow = new ConfigDialog();
                            }
                            break;
                        case "/s": // show
                            {
                                parentHwnd = ExtractParentHwnd(e, i);
                                this.MainWindow = new SaverWindow();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (this.MainWindow != null)
            {
                this.MainWindow.Show();
                if (parentHwnd != IntPtr.Zero)
                {
                    HwndSource hs = HwndSource.FromVisual(this.MainWindow) as HwndSource;
                    SetParent(hs.Handle, parentHwnd);
                    this.MainWindow.Top = 0;
                    this.MainWindow.Left = 0;
                }
            }
            else
            {
                this.MainWindow = new SaverWindow();
                this.MainWindow.Show();
            }
            SaverWindow saverWindow = this.MainWindow as SaverWindow;
            if (saverWindow != null)
            {
                saverWindow.EnableFullscreenMode();
            }
            this.MainWindow.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            this.MainWindow.Closed += new EventHandler(MainWindow_Closed);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // perform atmoic exit
            lock (threadExitSignalListLock)
            {
                if (threadExitSignalList.Count > 0)
                {
                    System.Threading.ManualResetEvent.WaitAll(
                        threadExitSignalList.ToArray(),
                        30000,
                        false);
                }
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        // TODO init in static ctor, relo to static instance region
        private static object threadExitSignalListLock = new object();
        private static List<System.Threading.WaitHandle> threadExitSignalList = new List<System.Threading.WaitHandle>();
        internal static void AddThreadExitSignal(System.Threading.WaitHandle waitHandle)
        {
            lock (threadExitSignalListLock)
            {
                threadExitSignalList.Add(waitHandle);
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private static IntPtr ExtractParentHwnd(StartupEventArgs e, int i)
        {
            int argNumber;
            if (e.Args[i].Contains(":"))
            {
                string[] parts = e.Args[i].Split(':');
                if (parts.Length > 1)
                {
                    return new IntPtr(int.Parse(parts[1]));
                }
            }
            if ((i < e.Args.Length - 1) && (int.TryParse(e.Args[i + 1], out argNumber)))
            {
                return new IntPtr(argNumber);
            }
            return IntPtr.Zero;
        }
    }
}
