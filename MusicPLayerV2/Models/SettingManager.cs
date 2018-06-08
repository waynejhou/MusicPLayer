using System;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Windows.Media;
using System.Windows.Markup;

namespace MusicPLayerV2.Models
{
    public class SettingManager
    {
        public static string SaveFilePath { get; set; } = "Setting.xml";
        private string _language = "zh-tw";
        public string Language
        {
            get => _language;
            set{
                if (_language != value)
                {
                    App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri($@"Resources/Strings/Lang.{value}.xaml", UriKind.Relative)
                    });
                    App.Current.Resources.MergedDictionaries.Remove(
                        App.Current.Resources.MergedDictionaries.First(x => x.Source.OriginalString == $@"Resources/Strings/Lang.{_language}.xaml"));
                    _language = value;
                    App.MainWin.Language = XmlLanguage.GetLanguage(value);
                }
            }
        }
        public void ChangePrimaryColor(SolidColorBrush color)
        {
            App.Current.Resources["PrimaryColor"] = color;
        }
        public double Version { get; set; } = 0.001d;
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
