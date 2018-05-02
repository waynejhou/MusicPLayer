using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MusicPLayer.Models
{
    public class SettingManager
    {
        private static string SaveFilePath = "Setting.xml";
        private string _language = "en-us";
        public string Langurage { get => _language; set => _language = value; }
        public double _version = 0.0001d;
        public double Version { get => _version; set => value = _version; }
        public string Info { get => $"Version: {Version}(debug)"; }
        public void SaveSettingAsXml()
        {
            SaveSettingAsXml(this, SaveFilePath);
        }
        public static void SaveSettingAsXml(SettingManager settings, string fileName)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(settings.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, settings);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(SettingManager)}.SaveSettingAsXml: {ex}");
            }
        }
        public static SettingManager LoadSettingFromXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { throw new ArgumentNullException("NameNull"); }
            SettingManager Setting = new SettingManager();
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;
                using (StringReader read = new StringReader(xmlString))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SettingManager));
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        Setting = (SettingManager)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(SettingManager)}.LoadSettingFromXml: {ex}");
            }
            return Setting;
        }
        public static SettingManager LoadOrNew()
        {
            SettingManager Loadded = null, newone = new SettingManager();
            if (File.Exists(SaveFilePath))
                Loadded = LoadSettingFromXml(SaveFilePath);
            if (Loadded != null && Loadded.Version==newone.Version)
            {
                return Loadded;
            }
            return newone;
        }
    }
}
