using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Xml.XPath;
using SEWilson.ScreenSaver.Caching;
using System.Diagnostics;

namespace SEWilson.ScreenSaver.TheArmory
{
    public class ItemInfoProvider
    {
        private static LocalObjectCache<Uri, ItemInfo> itemInfoCache = new LocalObjectCache<Uri, ItemInfo>();

        public static ItemInfo LoadFrom(string itemId)
        {
            Uri itemInfoUri = new Uri(string.Format("http://www.wowarmory.com/item-info.xml?i={0}", itemId));
            Uri itemTooltipUri = new Uri(string.Format("http://www.wowarmory.com/item-tooltip.xml?i={0}", itemId));
            return LoadFrom(itemInfoUri, itemTooltipUri);
        }

        private static ItemInfo LoadFrom(Uri itemInfoUri, Uri itemTooltipUri)
        {
            ItemInfo itemInfo = null;
            
            // use cached data if less than 12 hours old
            try
            {
                itemInfo = itemInfoCache.Get(itemInfoUri, 43200.0);
                if (itemInfo != null)
                {
                    return itemInfo;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            }

            // attempt to load from URI, if this fails, we use whatever cached data we have available.
            XDocument itemInfoXml;
            XDocument itemTooltipXml;
            {
                HttpWebRequest httpWebRequest = HttpWebRequest.Create(itemInfoUri) as HttpWebRequest;
                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618)";
                httpWebRequest.Referer = itemInfoUri.ToString();
                httpWebRequest.Accept = "text/xml";
                Stream resourceStream = null;
                try
                {
                    resourceStream = (httpWebRequest.GetResponse() as HttpWebResponse).GetResponseStream();
                    using (StreamReader reader = new StreamReader(resourceStream))
                    {
                        itemInfoXml = XDocument.Parse(reader.ReadToEnd());
                    }
                }
                catch
                {
                    itemInfo = itemInfoCache.Get(itemInfoUri);
                    if (itemInfo == null)
                        throw;
                    return itemInfo;
                }
            }
            {
                HttpWebRequest httpWebRequest = HttpWebRequest.Create(itemTooltipUri) as HttpWebRequest;
                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618)";
                httpWebRequest.Referer = itemInfoUri.ToString();
                httpWebRequest.Accept = "text/xml";
                Stream resourceStream = null;
                try
                {
                    resourceStream = (httpWebRequest.GetResponse() as HttpWebResponse).GetResponseStream();
                    using (StreamReader reader = new StreamReader(resourceStream))
                    {
                        itemTooltipXml = XDocument.Parse(reader.ReadToEnd());
                    }
                }
                catch
                {
                    itemInfo = itemInfoCache.Get(itemInfoUri);
                    if (itemInfo == null)
                        throw;
                    return itemInfo;
                }
            }

            itemInfo = LoadFrom(itemInfoXml, itemTooltipXml);
            itemInfoCache.Set(itemInfoUri, itemInfo);
            return itemInfo;
        }

        private static ItemInfo LoadFrom(XDocument itemInfoXml, XDocument itemTooltipXml)
        {
            ItemInfo itemInfo = new ItemInfo();
            PopulateWithItemInfoXml(itemInfo, itemInfoXml);
            PopulateWithItemTooltipXml(itemInfo, itemTooltipXml);
            return itemInfo;
        }

        public static void PopulateWithItemInfoXml(ItemInfo itemInfo, XDocument xdocument)
        {
            itemInfo.ItemInfoXml = xdocument;
            XElement xelement = xdocument.XPathSelectElement("/page/itemInfo/item");

            foreach (XAttribute attribute in xelement.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case "id":
                        itemInfo.ItemId = attribute.Value;
                        break;
                    case "name":
                        itemInfo.Name = attribute.Value;
                        break;
                    case "icon":
                        itemInfo.Icon = attribute.Value;
                        break;
                    case "level":
                        itemInfo.Level = attribute.Value;
                        break;
                    case "quality":
                        itemInfo.Quality = attribute.Value;
                        break;
                    case "type":
                        itemInfo.Type = attribute.Value;
                        break;
                    default:
                        // TODO log unsupported attribute
                        break;
                }
            }

            foreach (XElement subelement in xelement.Elements())
            {
                switch (subelement.Name.LocalName)
                {
                    case "cost":
                        break;
                    default:
                        // TODO log unsupported sub-element
                        break;
                }
            }
        }

        public static void PopulateWithItemTooltipXml(ItemInfo itemInfo, XDocument itemTooltipXml)
        {
            itemInfo.ItemTooltipXml = itemTooltipXml;
            XElement xelement = itemTooltipXml.XPathSelectElement("/page/itemTooltips/itemTooltip");

            foreach (XElement child in xelement.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "id":
                        itemInfo.ItemId = child.Value;
                        break;
                    case "name":
                        itemInfo.Name = child.Value;
                        break;
                    case "icon":
                        itemInfo.Icon = child.Value;
                        break;
                    case "level":
                        itemInfo.Level = child.Value;
                        break;
                    case "quality":
                    case "overallQualityId":
                        itemInfo.Quality = child.Value;
                        break;
                    case "bonding":
                        itemInfo.Bonding = int.Parse(child.Value);
                        break;
                    case "classId":
                        itemInfo.ClassId = int.Parse(child.Value);
                        break;
                    case "equipData":
                        {
                            foreach (XElement equipDataElement in child.Elements())
                            {
                                switch (equipDataElement.Name.LocalName)
                                {
                                    case "inventoryType":
                                        itemInfo.InventoryType = int.Parse(equipDataElement.Value);
                                        break;
                                    case "subclassName":
                                        itemInfo.SubClassName = equipDataElement.Value;
                                        break;
                                }
                            }
                        }
                        break;
                    case "damageData":
                        {
                            // TODO damageData
                            //foreach (XElement damageDataElement in child.Elements())
                            //{
                            //    switch (damageDataElement.Name.LocalName)
                            //    {
                            //        case "inventoryType":
                            //            itemInfo.InventoryType = int.Parse(damageDataElement.Value);
                            //            break;
                            //        case "subclassName":
                            //            itemInfo.SubClassName = damageDataElement.Value;
                            //            break;
                            //    }
                            //}
                        }
                        break;
                    case "bonusIntellect":
                        itemInfo.BonusIntellect = int.Parse(child.Value);
                        break;
                    case "bonusSpirit":
                        itemInfo.BonusSpirit = int.Parse(child.Value);
                        break;
                    case "bonusStamina":
                        itemInfo.BonusStamina = int.Parse(child.Value);
                        break;
                    case "bonusStrength":
                        itemInfo.BonusStrength = int.Parse(child.Value);
                        break;
                    case "bonusAgility":
                        itemInfo.BonusAgility = int.Parse(child.Value);
                        break;
                    case "bonusWisdom":
                        itemInfo.BonusWisdom = int.Parse(child.Value);
                        break;
                    case "armor":
                        {
                            itemInfo.Armor = int.Parse(child.Value);
                            foreach (XAttribute attribute in child.Attributes())
                            {
                                switch (attribute.Name.LocalName)
                                {
                                    case "armorBonus":
                                        itemInfo.ArmorBonus = int.Parse(attribute.Value);
                                        break;
                                }
                            }
                        }
                        break;
                    case "durability":
                        {
                            foreach (XAttribute attribute in child.Attributes())
                            {
                                switch (attribute.Name.LocalName)
                                {
                                    case "current":
                                        itemInfo.DurabilityCurrent = int.Parse(attribute.Value);
                                        break;
                                    case "max":
                                        itemInfo.DurabilityMax = int.Parse(attribute.Value);
                                        break;
                                }
                            }
                        }
                        break;
                    case "itemSource":
                        itemInfo.ItemSource = child.Value;
                        break;
                    case "spellData":
                        {
                            itemInfo.SpellData = new List<ItemInfo.Spell>();
                            foreach (XElement spellXml in child.Elements().Where((x) => x.Name.LocalName == "spell"))
                            {
                                SEWilson.ScreenSaver.TheArmory.ItemInfo.Spell spell = new ItemInfo.Spell();
                                foreach (XElement spellData in spellXml.Elements())
                                {
                                    switch (spellData.Name.LocalName)
                                    {
                                        case "trigger":
                                            spell.Trigger = int.Parse(spellData.Value);
                                            break;
                                        case "desc":
                                            spell.Description = spellData.Value;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                itemInfo.SpellData.Add(spell);
                            }
                        }
                        break;
                    case "requiredLevel":
                        itemInfo.RequiredLevel = int.Parse(child.Value);
                        break;

                    default:
                        // TODO log unsupported attribute
                        break;
                }
            }

            foreach (XElement subelement in xelement.Elements())
            {
                switch (subelement.Name.LocalName)
                {
                    case "cost":
                        break;
                    default:
                        // TODO log unsupported sub-element
                        break;
                }
            }
        }

        internal static void ResetCache()
        {
            itemInfoCache.Reset();
        }
    }
}
