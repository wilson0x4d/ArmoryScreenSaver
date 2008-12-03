using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Runtime.Serialization;

namespace SEWilson.ScreenSaver.TheArmory
{
    [Serializable]
    public class CharacterInfo : ISerializable
    {
        public XDocument XDocument { get; set; }

        public string Realm { get; set; }
        public string BattleGroup { get; set; }
        public string GuildName { get; set; }
        
        public string Prefix { get; set; }
        public string Name { get; set; }
        public string Suffix { get; set; }
        
        public string Level { get; set; }
        public string Race { get; set; }
        public string Class { get; set; }

        public string Faction { get; set; }
        public string Gender { get; set; }

        public string LastModified { get; set; }

        // equipment:
        public ItemInfo Head { get; set; }
        public ItemInfo Neck { get; set; }
        public ItemInfo Shoulders { get; set; }
        public ItemInfo Back { get; set; }
        public ItemInfo Chest { get; set; }
        public ItemInfo Shirt { get; set; }
        public ItemInfo Tabard { get; set; }
        public ItemInfo Wrist { get; set; }
        public ItemInfo Hands { get; set; }
        public ItemInfo Waist { get; set; }
        public ItemInfo Legs { get; set; }
        public ItemInfo Feet { get; set; }
        public ItemInfo Finger1 { get; set; }
        public ItemInfo Finger2 { get; set; }
        public ItemInfo Trinket1 { get; set; }
        public ItemInfo Trinket2 { get; set; }
        public ItemInfo MainHand { get; set; }
        public ItemInfo OffHand { get; set; }
        public ItemInfo Ranged { get; set; }
        public ItemInfo Ammo { get; set; }

        public string LifetimeHonorableKills { get; set; }

        public string DisplayName
        {
            get
            {
                return string.Format("[{0}] {1} {2} {3}",
                    Realm ?? "",
                    Prefix ?? "",
                    Name ?? "",
                    Suffix ?? "");
            }
        }

        public string DisplayRank
        {
            get
            {
                return string.Format("{0} {1} {2}",
                    Level,
                    Race,
                    Class);
            }
        }

        public CharacterInfo()
        {
        }

        #region ISerializable Members

        protected CharacterInfo(SerializationInfo info, StreamingContext context)
        {
            string xml = info.GetString("XML");
            XDocument xdocument = XDocument.Parse(xml,LoadOptions.None);
            CharacterInfoProvider.PopulateFrom(this, xdocument);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("XML", this.XDocument.ToString(SaveOptions.None));
        }

        #endregion
    }
}
