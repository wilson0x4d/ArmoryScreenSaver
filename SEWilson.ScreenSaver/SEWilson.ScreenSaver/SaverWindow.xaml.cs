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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Net;
using System.IO;
using SEWilson.ScreenSaver.TheArmory;
using System.Diagnostics;
using System.Threading;
using System.Collections.ObjectModel;

namespace SEWilson.ScreenSaver
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SaverWindow : Window
    {
        private ObservableCollection<CharacterInfo> characterInfoList;
        private Queue<string[]> characterInfoLoadQueue;
        private Thread characterInfoSchedulingThread;

        private Queue<string> backgroundContentQueue;

        public SaverWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            characterInfoList = new ObservableCollection<CharacterInfo>();

            this.DataContext = characterInfoList;

            listCharacterSheets.SelectionChanged += new SelectionChangedEventHandler(listCharacterSheets_SelectionChanged);

            if (characterInfoSchedulingThread == null)
            {
                this.characterInfoLoadQueue = new Queue<string[]>();
                characterInfoSchedulingThread = new Thread(CharacterInfoSchedulingThreadMain);
                characterInfoSchedulingThread.IsBackground = true;
                characterInfoSchedulingThread.Name = "CHARINFO";
                characterInfoSchedulingThread.Start();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            exitYet = !e.Cancel;
        }

        private void characterFeedService_CharacterAdvertised(object sender, SEWilson.ScreenSaver.P2P.CharacterFeedService.CharacterFeedEventArgs e)
        {
            if (this.characterInfoList.Count <= 100)
            {
                Debug.WriteLine(string.Format("Enqueue: Advertised: realm={0} name={1}", e.RealmName, e.CharacterName));
                this.characterInfoLoadQueue.Enqueue(new string[] { e.RealmName, e.CharacterName });
            }
            else
            {
                this.characterFeedService = null;
            }
        }

        private P2P.CharacterFeedService characterFeedService = null;

        public void EnableFullscreenMode()
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
            this.WindowState = WindowState.Maximized;
        }

        public void DisableFullscreenMode()
        {
            this.Topmost = false;
            this.WindowState = WindowState.Minimized;
        }

        void listCharacterSheets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listCharacterSheets.SelectedItem != null)
            {
                this.characterSheetControl.Visibility = Visibility.Visible;
            }
            else
            {
                this.characterSheetControl.Visibility = Visibility.Hidden;
            }
            this.characterSheetControl.DataContext = listCharacterSheets.SelectedItem;
        }

        private void LoadArmoryMRU()
        {
            string armoryMRU = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("ArmoryMRU", "[Thunderlord] Shaan;") as string;
            string[] characterList = armoryMRU.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            Debug.WriteLine("mru=" + armoryMRU);
            if (this.characterFeedService == null)
            {
                try
                {
                    // TODO we may want to relocate this to the app level, so that it is available no matter which window is running, with an App object event handler we can hook to perform loading as appropriate for the active window
                    if (!bool.Parse(Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("DisablePeerNetwork", "False") as string))
                    {
                        P2P.PeerDiscoveryService.Start();
                        characterFeedService = new SEWilson.ScreenSaver.P2P.CharacterFeedService();
                        characterFeedService.CharacterAdvertised += new EventHandler<SEWilson.ScreenSaver.P2P.CharacterFeedService.CharacterFeedEventArgs>(characterFeedService_CharacterAdvertised);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                }
            }

            P2P.ICharacterFeedChannel characterFeedChannel = this.characterFeedService.CreateChannel();
            foreach (string characterName in characterList)
            {
                string[] parts = characterName.Replace("[", "").Replace("]", "").Trim().Split(' ');
                Debug.WriteLine(string.Format("Enqueue: MRU: realm={0} name={1}", parts[0], parts[1]));
                this.characterInfoLoadQueue.Enqueue(new string[] { parts[0], parts[1] });
                try
                {
                    if (characterFeedChannel != null)
                    {
                        characterFeedChannel.Advertise(parts[0], parts[1]);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                    characterFeedChannel = null;
                }
            }
        }

        internal ManualResetEvent characterInfoSchedulingThreadSignal = new ManualResetEvent(false);
        private bool exitYet = false;

        private void CharacterInfoSchedulingThreadMain()
        {
            bool armoryMruHasBeenLoaded = false;
            App.AddThreadExitSignal(characterInfoSchedulingThreadSignal);

            while (!exitYet)
            {
                if (backgroundContentQueue == null)
                {
                    backgroundContentQueue = new Queue<string>();
                    try
                    {
                        string backgroundsFolder = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("SEWilson").CreateSubKey("ScreenSaver").GetValue("ArmoryBackgroundsFolder", @"C:\Program Files\World of Warcraft\Screenshots\") as string;
                        if (Directory.Exists(backgroundsFolder))
                        {
                            List<string> backgroundFiles = new List<string>();
                            backgroundFiles.AddRange(Directory.GetFiles(backgroundsFolder, "*.avi"));
                            backgroundFiles.AddRange(Directory.GetFiles(backgroundsFolder, "*.wmv"));
                            backgroundFiles.AddRange(Directory.GetFiles(backgroundsFolder, "*.png"));
                            backgroundFiles.AddRange(Directory.GetFiles(backgroundsFolder, "*.jpg"));
                            backgroundFiles.AddRange(Directory.GetFiles(backgroundsFolder, "*.bmp"));
                            backgroundFiles.Sort();
                            foreach (string backgroundFile in backgroundFiles)
                            {
                                backgroundContentQueue.Enqueue(backgroundFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO report failure
                    }
                }
                else
                {
                    bool contentFailure = false;
                    this.Dispatcher.Invoke((ThreadStart)delegate()
                    {
                        this.Activate();
                        try
                        {
                            if (mediaElement1.Source == null)
                            {
                                string backgroundFile = backgroundContentQueue.Dequeue();
                                backgroundContentQueue.Enqueue(backgroundFile);
                                switch (System.IO.Path.GetExtension(backgroundFile).ToLower().Trim('.'))
                                {
                                    case "jpg":
                                    case "png":
                                    case "bmp":
                                        BitmapImage bitmapImage = new BitmapImage(new Uri(backgroundFile));
                                        imageBackground.Source = bitmapImage;
                                        imageBackground.Visibility = Visibility.Visible;
                                        mediaElement1.Visibility = Visibility.Hidden;
                                        break;
                                    case "avi":
                                    case "wmv":
                                        mediaElement1.Source = new Uri(backgroundFile);
                                        mediaElement1.Visibility = Visibility.Visible;
                                        imageBackground.Visibility = Visibility.Hidden;
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            contentFailure = true;
                            imageBackground.Visibility = Visibility.Hidden;
                            mediaElement1.Visibility = Visibility.Hidden;
                        }
                    },
                    null);

                    long ts = DateTime.Now.Ticks;
                    while ((TimeSpan.FromTicks(DateTime.Now.Ticks - ts).TotalSeconds < 15) && (!exitYet))
                    {
                        if (this.characterInfoLoadQueue.Count > 0)
                        {
                            string[] item = null;
                            try
                            {
                                item = this.characterInfoLoadQueue.Dequeue();
                                LoadCharacterInfo(item[0], item[1]);
                            }
                            catch (Exception ex)
                            {
                                if (item != null)
                                {
                                    Debug.WriteLine(string.Format("Enqueue: Exception in CIST: realm={0} name={1}", item[0], item[1]));
                                    this.characterInfoLoadQueue.Enqueue(item);
                                }
                                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            if (!armoryMruHasBeenLoaded)
                            {
                                armoryMruHasBeenLoaded = true;
                                LoadArmoryMRU();
                            }
                            else
                            {
                                Thread.Sleep(250);
                            }
                        }
                    }
                }
            }

            characterInfoSchedulingThreadSignal.Set();
        }

        private void LoadCharacterInfo(string realmName, string characterName)
        {
            realmName = realmName.Trim();
            characterName = characterName.Trim();
            // load character sheet, and use as the binding target for the specified UIElement
            if (characterInfoList.Where((c) => c.Realm.Equals(realmName) && c.Name.Equals(characterName)).Count() == 0)
            {
                CharacterInfo characterInfo = CharacterInfoProvider.LoadFrom(realmName, characterName);
                this.Dispatcher.Invoke((ThreadStart)delegate()
                {
                    lock (characterInfoList)
                    {
                        if (characterInfoList.Where((c) => c.Realm.ToLower() == realmName.ToLower() && c.Name.ToLower() == characterName.ToLower()).Count() == 0)
                        {
                            characterInfoList.Add(characterInfo);
                        }
                    }
                });
            }
        }

        private void buttonCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonRedX_Click(object sender, RoutedEventArgs e)
        {
            this.listCharacterSheets.SelectedItem = null;
            //this.characterSheetControl.Visibility = Visibility.Hidden;

        }

        private void mediaElement1_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement1.Source = null;
        }

        private void mediaElement1_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine(e.ErrorException.Message + "|" + e.ErrorException.StackTrace);
        }

        private void mediaElement1_MediaOpened(object sender, RoutedEventArgs e)
        {

        }

        private void label2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.mediaElement1.Source != null)
            {
                this.mediaElement1.Source = null;
            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            mediaElement1.Volume = 0;
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            // TODO implement volume slider
            mediaElement1.Volume = 0.2;
        }
    }
}
