using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics;
using SEWilson.ScreenSaver.Caching;

namespace SEWilson.ScreenSaver.TheArmory
{
    public sealed class CharacterInfoProvider
    {
        static CharacterInfoProvider()
        {
            characterInfoCache = new LocalObjectCache<Uri, CharacterInfo>();
        }

        private static LocalObjectCache<Uri, CharacterInfo> characterInfoCache = null;

        public static CharacterInfo LoadFrom(string realmName, string characterName)
        {
            return LoadFrom(new Uri(
                string.Format(
                    "http://www.wowarmory.com/character-sheet.xml?r={0}&n={1}",
                    realmName,
                    characterName)));
        }

        public static CharacterInfo LoadFrom(Uri uri)
        {
            // use cached data if less than 6 hours old
            try
            {
                CharacterInfo characterInfo = characterInfoCache.Get(uri, 21600.0);
                if (characterInfo != null)
                {
                    return characterInfo;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            }

            // TODO load uri from cache if cache object is available, this will allow us to auto-update cache in the background, and not take the http hit elsewhere
            HttpWebRequest httpWebRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
            httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618)";
            httpWebRequest.Referer = uri.ToString();
            httpWebRequest.Accept = "text/xml";

            Stream resourceStream = null;
            XDocument xdocument;
            try
            {
                resourceStream = (httpWebRequest.GetResponse() as HttpWebResponse).GetResponseStream();
                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    xdocument = XDocument.Parse(reader.ReadToEnd());
                    CharacterInfo characterInfo = LoadFrom(xdocument);
                    characterInfoCache.Set(uri, characterInfo);
                    return characterInfo;
                }
            }
            catch
            {
                CharacterInfo characterInfo = characterInfoCache.Get(uri);
                if (characterInfo == null)
                    throw;
                return characterInfo;
            }
        }

        public static CharacterInfo LoadFrom(XDocument xdocument)
        {
            CharacterInfo characterInfo = new CharacterInfo();
            PopulateFrom(characterInfo, xdocument); 
            return characterInfo;
        }

        public static void PopulateFrom(CharacterInfo characterInfo, XDocument xdocument)
        {
            characterInfo.XDocument = xdocument;
            PopulateCharacterData(xdocument, characterInfo);
            PopulatePvpData(xdocument, characterInfo);
            PopulateItemData(xdocument, characterInfo);
        }

        public static void PopulateItemData(XDocument xdocument, CharacterInfo characterSheet)
        {
            try
            {
                XElement xelement = xdocument.XPathSelectElement("/page/characterInfo/characterTab/items");
                foreach (XElement itemElement in xelement.XPathSelectElements("item"))
                {
                    ItemInfo itemInfo = null;
                    foreach (XAttribute attribute in itemElement.Attributes())
                    {
                        if (attribute.Name.LocalName == "id")
                        {
                            itemInfo = ItemInfoProvider.LoadFrom(attribute.Value);
                            break;
                        }
                    }

                    if (itemInfo == null)
                        continue;

                    // process character-specific item data
                    foreach (XAttribute attribute in itemElement.Attributes())
                    {
                        switch (attribute.Name.LocalName)
                        {
                            //case "icon":
                            //    itemInfo.Icon = attribute.Value as string;
                            //    break;
                            case "durability":
                                itemInfo.Durability = attribute.Value as string;
                                break;
                            case "maxDurability":
                                itemInfo.MaxDurability = attribute.Value as string;
                                break;
                            case "slot":
                                itemInfo.Slot = attribute.Value as string;
                                switch (itemInfo.Slot)
                                {
                                    case "0": // Head
                                        characterSheet.Head = itemInfo;
                                        break;
                                    case "1": // Neck
                                        characterSheet.Neck = itemInfo;
                                        break;
                                    case "2": // Shoulders
                                        characterSheet.Shoulders = itemInfo;
                                        break;
                                    case "3": // Shirt
                                        characterSheet.Shirt = itemInfo;
                                        break;
                                    case "4": // Chest
                                        characterSheet.Chest = itemInfo;
                                        break;
                                    case "5": // Waist
                                        characterSheet.Waist = itemInfo;
                                        break;
                                    case "6": // Legs
                                        characterSheet.Legs = itemInfo;
                                        break;
                                    case "7": // Feet
                                        characterSheet.Feet = itemInfo;
                                        break;
                                    case "8": // Wrist
                                        characterSheet.Wrist = itemInfo;
                                        break;
                                    case "10": // Finger1
                                        characterSheet.Finger1 = itemInfo;
                                        break;
                                    case "11": // Finger2
                                        characterSheet.Finger2 = itemInfo;
                                        break;
                                    case "12": // Trinket1
                                        characterSheet.Trinket1 = itemInfo;
                                        break;
                                    case "13": // Trinket2
                                        characterSheet.Trinket2 = itemInfo;
                                        break;
                                    case "14": // Back
                                        characterSheet.Back = itemInfo;
                                        break;
                                    case "15": // MainHand
                                        characterSheet.MainHand = itemInfo;
                                        break;
                                    case "16": // OffHand
                                        characterSheet.OffHand = itemInfo;
                                        break;
                                    case "17": // Ranged
                                        characterSheet.Ranged = itemInfo;
                                        break;
                                    default:
                                        // TODO log unknown slot
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    // TODO
                }
            }
            catch (Exception ex)
            {
                // TODO error reporting
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            }
        }

        public static void PopulatePvpData(XDocument xdocument, CharacterInfo characterSheet)
        {
            XElement xelement = null;
            try
            {
                xelement = xdocument.XPathSelectElement("/page/characterInfo/characterTab/pvp");
                if (xelement != null)
                {
                    characterSheet.LifetimeHonorableKills = xelement.XPathSelectElement("lifetimehonorablekills").Attributes(XName.Get("value")).ElementAt(0).Value as string;
                }
            }
            catch (Exception ex)
            {
                // TODO error reporting
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                DebugWriteElement(xelement);
            }
        }

        private static void DebugWriteElement(XElement xelement)
        {
            if (xelement != null)
            {
                Debug.WriteLine(xelement.ToString());
            }
        }

        public static void PopulateCharacterData(XDocument xdocument, CharacterInfo characterSheet)
        {
            XElement xelement = xdocument.XPathSelectElement("/page/characterInfo/character");
            if (xelement == null)
            {
                throw new Exception("characterInfo either invalid, mal-formatted, or the requested character could not be found.", xdocument != null ? new Exception("xml=" + xdocument.ToString()) : null);
            }
            foreach (XAttribute attribute in xelement.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case "battleGroup":
                        characterSheet.BattleGroup = attribute.Value;
                        break;
                    case "class":
                        characterSheet.Class = attribute.Value;
                        break;
                    case "faction":
                        characterSheet.Faction = attribute.Value;
                        break;
                    case "gender":
                        characterSheet.Gender = attribute.Value;
                        break;
                    case "guildName":
                        characterSheet.GuildName = attribute.Value;
                        break;
                    case "lastModified":
                        characterSheet.LastModified = attribute.Value;
                        break;
                    case "level":
                        characterSheet.Level = attribute.Value;
                        break;
                    case "name":
                        characterSheet.Name = attribute.Value;
                        break;
                    case "prefix":
                        characterSheet.Prefix = attribute.Value;
                        break;
                    case "race":
                        characterSheet.Race = attribute.Value;
                        break;
                    case "realm":
                        characterSheet.Realm = attribute.Value;
                        break;
                    case "suffix":
                        characterSheet.Suffix = attribute.Value;
                        break;
                    default:
                        break;
                }
            }
        }

        internal static void ResetCache()
        {
            characterInfoCache.Reset();
        }
    }
}
