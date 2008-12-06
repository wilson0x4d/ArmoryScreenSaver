using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Serialization;
using SEWilson.ScreenSaver.Caching;
using System.Diagnostics;

namespace SEWilson.ScreenSaver.TheArmory
{
    [Serializable]
    public class ItemInfo : INotifyPropertyChanged, ISerializable
    {
        internal XDocument ItemInfoXml;
        internal XDocument ItemTooltipXml;

        public string ItemId { get; set; }
        public string Name { get; set; }
        public string Quality { get; set; }
        public string QualityColor
        {
            get
            {
                switch (Quality ?? "0")
                {
                    case "0":
                        return "Silver";
                    case "1":
                        return "White";
                    case "2":
                        return "Chartreuse";
                    case "3":
                        return "Blue";
                    case "4":
                        return "Purple";
                    case "5":
                        return "Orange";
                    case "6":
                        return "Red";
                    default:
                        break;
                }
                return "Cyan";
            }
        }
        public string Type { get; set; }
        public string SellPrice { get; set; }
        public string Durability { get; set; }
        public string MaxDurability { get; set; }
        public string Slot { get; set; }
        public string SlotName
        {
            get
            {
                switch (Slot ?? "")
                {
                    case "0": return "Head";
                    case "1": return "Neck";
                    case "2": return "Shoulders";
                    case "3": return "Shirt";
                    case "4": return "Chest";
                    case "5": return "Waist";
                    case "6": return "Legs";
                    case "7": return "Feet";
                    case "8": return "Wrist";

                    case "10": return "Finger1";
                    case "11": return "Finger2";
                    case "12": return "Trinket1";
                    case "13": return "Trinket2";
                    case "14": return "Back";
                    case "15": return "Main Hand";
                    case "16": return "Off Hand";
                    case "17": return "Ranged";
                    default:
                        break;
                }
                return null;
            }
        }
        public string Level { get; set; }

        public string Icon
        {
            get { return icon; }
            set
            {
                if (icon != value)
                {
                    icon = value;
                    OnPropertyChanged("IconUri");
                    LoadItemImage(false);
                }
            }
        }

        private string icon;

        public Uri IconUri
        {
            get
            {
                return new Uri(string.Format("http://www.wowarmory.com/wow-icons/_images/64x64/{0}.jpg", Icon));
            }
        }

        public System.Windows.Media.Imaging.BitmapImage ItemImage
        {
            get
            {
                return itemImage;
            }
            set
            {
                if (itemImage != value)
                {
                    itemImage = value;
                    OnPropertyChanged("ItemImage");
                }
            }
        } System.Windows.Media.Imaging.BitmapImage itemImage;

        public int Bonding { get; set; }

        public string BondingDisplayText
        {
            get
            {
                switch (Bonding)
                {
                    case 0:
                        return ""; // Does not bind
                    case 1:
                        return "Binds when picked up";
                    case 2:
                        return "Binds when equipped";
                }
                return "Bonding: " + Bonding.ToString();
            }
        }

        public int ClassId { get; set; }

        public int InventoryType { get; set; }

        public string SubClassName { get; set; }

        public int BonusIntellect { get; set; }
        public int BonusSpirit { get; set; }
        public int BonusStamina { get; set; }
        public int BonusStrength { get; set; }
        public int BonusAgility { get; set; }
        public int BonusWisdom { get; set; }

        public int Armor { get; set; }
        public int ArmorBonus { get; set; }
        public int DurabilityCurrent { get; set; }
        public int DurabilityMax { get; set; }
        public int RequiredLevel { get; set; }

        public class Spell
        {
            public int Trigger { get; set; }
            public string Description { get; set; }
        }

        public List<Spell> SpellData { get; set; }

        public string ItemSource { get; set; }

        public ItemInfo()
        {
        }

        private void LoadItemImage(bool useCacheOnly)
        {
            try
            {
                string wassImageCachePath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @".wass\wow-icons\_images\64x64\");

                if (!System.IO.Directory.Exists(wassImageCachePath))
                {
                    System.IO.Directory.CreateDirectory(wassImageCachePath);
                }

                string iconCachePath = System.IO.Path.Combine(wassImageCachePath, Icon + ".jpg");

                if (System.IO.Directory.Exists(iconCachePath))
                {
                    Uri itemInfoIconUri = new Uri(iconCachePath, UriKind.Absolute);
                    ItemImage = new System.Windows.Media.Imaging.BitmapImage(itemInfoIconUri);
                }
                else if (!useCacheOnly)
                {
                    System.Net.WebClient webClient = new System.Net.WebClient();
                    webClient.BaseAddress = IconUri.ToString();
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
                    webClient.DownloadFileAsync(IconUri, iconCachePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            }
        }

        void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            LoadItemImage(true);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region ISerializable Members

        protected ItemInfo(SerializationInfo info, StreamingContext context)
        {
            this.Parse(info.GetString("item-info"), info.GetString("item-tooltip"));
        }

        private void Parse(string itemInfoXmlString, string itemTooltipXmlString)
        {
            XDocument itemInfoXml = XDocument.Parse(itemInfoXmlString);
            XDocument itemTooltipXml = XDocument.Parse(itemTooltipXmlString);
            ItemInfoProvider.PopulateWithItemInfoXml(this, itemInfoXml);
            ItemInfoProvider.PopulateWithItemTooltipXml(this, itemTooltipXml);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("item-info", this.ItemInfoXml.ToString(SaveOptions.None));
            info.AddValue("item-tooltip", this.ItemTooltipXml.ToString(SaveOptions.None));
        }

        #endregion
    }
}
