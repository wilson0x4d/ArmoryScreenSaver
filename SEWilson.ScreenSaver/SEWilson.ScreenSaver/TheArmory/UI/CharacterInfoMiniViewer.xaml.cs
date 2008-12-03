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

namespace SEWilson.ScreenSaver.TheArmory.UI
{
    /// <summary>
    /// Interaction logic for CharacterInfoMiniViewer.xaml
    /// </summary>
    public partial class CharacterInfoMiniViewer : UserControl
    {
        public CharacterInfoMiniViewer()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(CharacterInfoMiniViewer_DataContextChanged);
        }

        void CharacterInfoMiniViewer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CharacterInfo characterInfo = this.DataContext as CharacterInfo;
            if (characterInfo != null)
            {
                this.imageFactionIcon.Source = new BitmapImage(new Uri("/wowarmory;component/TheArmory/Art/Faction/" + characterInfo.Faction + ".jpg", UriKind.Relative));
                this.imageFactionIcon.ToolTip = characterInfo.Faction;
                this.imageClassIcon.Source = new BitmapImage(new Uri("/wowarmory;component/TheArmory/Art/Class/" + characterInfo.Class.Replace(" ","") + ".png", UriKind.Relative));
                this.imageClassIcon.ToolTip = characterInfo.Class;
                BrushConverter bc = new BrushConverter();
                if (characterInfo.Faction.ToLower() == "horde")
                    this.Background = (bc.ConvertFrom("#422222") as Brush) ?? Brushes.Orange;
                else if (characterInfo.Faction.ToLower() == "alliance")
                    this.Background = (bc.ConvertFrom("#222242") as Brush) ?? Brushes.Teal;
            }
        }
    }
}
