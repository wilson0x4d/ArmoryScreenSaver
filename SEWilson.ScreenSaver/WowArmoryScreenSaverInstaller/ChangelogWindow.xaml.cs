using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace WowArmoryScreenSaverInstaller
{
    /// <summary>
    /// Interaction logic for ChangelogWindow.xaml
    /// </summary>
    public partial class ChangelogWindow : Window
    {
        public ChangelogWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            string path = System.IO.Path.GetFullPath(@".\changelog.txt");
            if (File.Exists(path))
            {
                try
                {
                    using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            this.textBox1.Text = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.textBlock1.Text = ex.Message + "|" + ex.StackTrace;
                }
            }
        }
    }
}
