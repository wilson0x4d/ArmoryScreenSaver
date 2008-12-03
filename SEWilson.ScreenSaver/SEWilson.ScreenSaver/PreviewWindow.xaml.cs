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
using System.Diagnostics;

namespace SEWilson.ScreenSaver
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // quick and dirty method of getting the user to authorize the service when windows firewall is enabled
            try
            {
                P2P.PeerDiscoveryService.Start();
                P2P.CharacterFeedService characterFeedService = new SEWilson.ScreenSaver.P2P.CharacterFeedService();
                characterFeedService.CreateChannel().Advertise("Thunderlord", "Shaan");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            }
        }
    }
}
