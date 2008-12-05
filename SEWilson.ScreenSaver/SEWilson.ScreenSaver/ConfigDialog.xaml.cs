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
using System.Xml.Linq;
using System.Xml.XPath;
using SEWilson.ScreenSaver.TheArmory;
using System.Diagnostics;

namespace SEWilson.ScreenSaver
{
    /// <summary>
    /// Interaction logic for ConfigDialog.xaml
    /// </summary>
    public partial class ConfigDialog : Window
    {
        public ConfigDialog()
        {
            InitializeComponent();
            this.IsEnabled = false;
            this.checkDisablePeerNetwork.IsChecked = bool.Parse(Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("DisablePeerNetwork", "False") as string);
            Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate()
                {
                    labelBackgroundsFolder.Content = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("ArmoryBackgroundsFolder", @"C:\Program Files\World of Warcraft\Screenshots\") as string;
                    textBackgroundsPath.Text = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("ArmoryBackgroundsFolder", @"C:\Program Files\World of Warcraft\Screenshots\") as string;
                    List<string> realmList = new List<string>();
                    try
                    {
                        XDocument xdoc = XDocument.Load("http://www.worldofwarcraft.com/realmstatus/status.xml", LoadOptions.None);
                        foreach (XElement xelement in xdoc.XPathSelectElements("/page/rs/r"))
                        {
                            realmList.Add(xelement.Attribute(XName.Get("n")).Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "|" + ex.StackTrace, "Could Not Retrieve Realm List!");
                    }
                    comboRealmSelection.ItemsSource = realmList;
                    comboRealmSelection.SelectedValue = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("ArmoryRealmName", "Thunderlord") as string;
                    textCharacterName.Text = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("ArmoryCharacterName", "Shaan") as string;
                    this.IsEnabled = true;
                },
                null);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                CharacterInfo characterSheet = CharacterInfoProvider.LoadFrom(comboRealmSelection.SelectedValue as string, textCharacterName.Text as string);
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").SetValue("ArmoryRealmName", comboRealmSelection.SelectedValue as string);
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").SetValue("ArmoryCharacterName", textCharacterName.Text);
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").SetValue("ArmoryBackgroundsFolder", textBackgroundsPath.Text);

                UpdateArmoryMRU(comboRealmSelection.SelectedValue as string, textCharacterName.Text as string);

                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                Util.UI.ExceptionInspectorWindow.Inspect(ex);
            }
        }

        private void UpdateArmoryMRU(string realmName, string characterName)
        {
            string armoryMRU = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("ArmoryMRU", "") as string;
            string[] characterList = armoryMRU.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            bool contains = false;
            foreach (string item in characterList)
            {
                string[] parts = item.Replace("[", "").Replace("]", "").Trim().Split(' ');
                if (parts[0].Equals(realmName) && parts[1].Equals(characterName))
                {
                    contains = true;
                    break;
                }
            }

            if (!contains)
            {
                armoryMRU += "[" + realmName + "] " + characterName + ";";
            }

            Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").SetValue("ArmoryMRU", armoryMRU);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            bool cacheReset = MessageBox.Show(
                "Do you wish to purge offline content?",
                "Cache Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;

            bool charactersReset = MessageBox.Show(
                "Do you wish to reset the character list?",
                "Character List Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;

            if (cacheReset)
            {
                CharacterInfoProvider.ResetCache();
                ItemInfoProvider.ResetCache();
            }

            if (charactersReset)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").DeleteValue("ArmoryMRU", false);
            }
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void checkDisablePeerNetwork_Checked(object sender, RoutedEventArgs e)
        {
            if (checkDisablePeerNetwork.IsChecked.GetValueOrDefault(false))
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").SetValue("DisablePeerNetwork", "True");
            }
            else
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").DeleteValue("DisablePeerNetwork", false);
            }
        }
    }
}
