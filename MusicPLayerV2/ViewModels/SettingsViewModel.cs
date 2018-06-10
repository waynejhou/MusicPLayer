﻿using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml;
using System.Xml.Serialization;

namespace MusicPLayerV2.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private ResourceDictionary R => App.Current.Resources;
        private MusicPlayer PM => App.PlayerModel;
        private MusicItem NPI => App.PlayerModel.NowPlayingItem;
        private ControllerViewModel C => App.Controller;
        private PlayingListViewModel L => App.PlayingList;

        private XmlLanguage _appLanguage = XmlLanguage.GetLanguage("zh-tw");
        [XmlIgnore]
        public XmlLanguage AppLanguage
        {
            get => _appLanguage;
            set {
                if (_appLanguage != value)
                {
                    _fontFamiliesList = null;
                    App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri($@"Resources/Strings/Lang.{value}.xaml", UriKind.Relative)
                    });
                    App.Current.Resources.MergedDictionaries.Remove(
                        App.Current.Resources.MergedDictionaries.
                        First(x => x.Source.OriginalString == $@"Resources/Strings/Lang.{_appLanguage}.xaml"));
                    _appLanguage = value;
                    App.MainWin.Language = value;
                    NotifyPropertyChanged(nameof(LanguageToHuman));
                }
            }
        }
        public string Language
        {
            get => AppLanguage.ToString();
            set => AppLanguage = XmlLanguage.GetLanguage(value);
        }
        public string LanguageToHuman => (string)R[$"Setting_Language_{Language}"];
        [XmlIgnore]
        public KeyValuePair<XmlLanguage, string> LanguageKeyValue
        {
            get => LanguageList.First(x => x.Key == AppLanguage);
            set => AppLanguage = (value.Key);
        }
        [XmlIgnore]
        public Dictionary<XmlLanguage, string> LanguageList => new Dictionary<XmlLanguage, string>()
                {
                    { XmlLanguage.GetLanguage("zh-tw"), (string)R["Setting_Language_zh-tw"] },
                    { XmlLanguage.GetLanguage("en-us"), (string)R["Setting_Language_en-us"] }
                };

        [XmlIgnore]
        public Color PrimaryColor
        {
            get => (R["PrimaryColor"] as SolidColorBrush).Color;
            set
            {
                R["PrimaryColor"] = new SolidColorBrush(value);
                var invertCOlor = Color.FromRgb((byte)(255 - ((int)(value.R))), (byte)(255 - ((int)(value.G))), (byte)(255 - ((int)(value.B))));
                R["PanelOpacity"] = new SolidColorBrush(invertCOlor) { Opacity = (R["PanelOpacity"] as SolidColorBrush).Opacity };
                R["PanelOpacityL"] = new SolidColorBrush(invertCOlor) { Opacity = (R["PanelOpacityL"] as SolidColorBrush).Opacity };
            }
        }
        public string PrimaryColorHex
        {
            get => PrimaryColor.ToString();
            set => PrimaryColor = (Color)ColorConverter.ConvertFromString(value);
        }
        [XmlIgnore]
        public Color SecondaryColor { get => (R["SecondaryColor"] as SolidColorBrush).Color; set => R["SecondaryColor"] = new SolidColorBrush(value); }
        public string SecondaryColorHex
        {
            get => SecondaryColor.ToString();
            set => SecondaryColor = (Color)ColorConverter.ConvertFromString(value);
        }
        [XmlIgnore]
        public Color SecondaryColorL { get => (R["SecondaryColorL"] as SolidColorBrush).Color; set => R["SecondaryColorL"] = new SolidColorBrush(value); }
        public string SecondaryColorLHex
        {
            get => SecondaryColorL.ToString();
            set => SecondaryColorL = (Color)ColorConverter.ConvertFromString(value);
        }
        [XmlIgnore]
        public Color ForegroundColor
        {
            get => (R["ForegroundColor"] as SolidColorBrush).Color;
            set => R["ForegroundColor"] = new SolidColorBrush(value);
        }
        public string ForegroundColorLHex
        {
            get => ForegroundColor.ToString();
            set => ForegroundColor = (Color)ColorConverter.ConvertFromString(value);
        }

        [XmlIgnore]
        public Color LyricForegroundColor
        {
            get => (R["LyricForegroundColor"] as SolidColorBrush).Color;
            set => R["LyricForegroundColor"] = new SolidColorBrush(value);
        }
        public string LyricForegroundColorHex
        {
            get => LyricForegroundColor.ToString();
            set => LyricForegroundColor = (Color)ColorConverter.ConvertFromString(value);
        }
        [XmlIgnore]
        public Color LyricHighlightColor
        {
            get => (R["LyricHighlightColor"] as SolidColorBrush).Color;
            set => R["LyricHighlightColor"] = new SolidColorBrush(value);
        }
        public string LyricHighlightColorHex
        {
            get => LyricHighlightColor.ToString();
            set => LyricHighlightColor = (Color)ColorConverter.ConvertFromString(value);
        }

        [XmlIgnore]
        public Color LyricShadowColor
        {
            get => (R["LyricShadowEffect"] as DropShadowEffect).Color;
            set
            {
                var lse = R["LyricShadowEffect"] as DropShadowEffect;
                R["LyricShadowEffect"] = new DropShadowEffect() { BlurRadius = lse.BlurRadius, ShadowDepth = lse.ShadowDepth, Color = value };
            }
        }
        public string LyricShadowColorHex
        {
            get => LyricShadowColor.ToString();
            set => LyricShadowColor = (Color)ColorConverter.ConvertFromString(value);
        }
        [XmlIgnore]
        public Color TextShadowColor
        {
            get => (R["TextShadowEffect"] as DropShadowEffect).Color;
            set
            {
                var tse = R["TextShadowEffect"] as DropShadowEffect;
                R["TextShadowEffect"] = new DropShadowEffect() { BlurRadius = tse.BlurRadius, ShadowDepth = tse.ShadowDepth, Color = value };
            }
        }
        public string TextShadowColorHex
        {
            get => TextShadowColor.ToString();
            set => TextShadowColor = (Color)ColorConverter.ConvertFromString(value);
        }
        [XmlIgnore]
        private IEnumerable<KeyValuePair<string, FontFamily>> _fontFamiliesList;
        [XmlIgnore]
        public IEnumerable<KeyValuePair<string, FontFamily>> FontFamiliesList
        {
            get
            {
                if (_fontFamiliesList != null)
                    return _fontFamiliesList;
                else
                    return _fontFamiliesList = Fonts.SystemFontFamilies.ToList().ConvertAll(font =>
                    {
                        if (font.FamilyNames.Keys.Contains(AppLanguage))
                            return new KeyValuePair<string, FontFamily>(font.FamilyNames[AppLanguage], font);
                        else if (font.FamilyNames.Keys.ToList().Exists(x => x != XmlLanguage.GetLanguage("en-us")))
                            return new KeyValuePair<string, FontFamily>(font.FamilyNames.First(x => x.Key != XmlLanguage.GetLanguage("en-us")).Value, font);
                        else
                            return new KeyValuePair<string, FontFamily>(font.FamilyNames.First().Value, font);
                    }).OrderBy(x => x.Key);
            }
        }


        public double TextMediumFontSize { get => (double)R[nameof(TextMediumFontSize)]; set => R[nameof(TextMediumFontSize)] = value; }
        public double TextSmallFontSize { get => (double)R[nameof(TextSmallFontSize)]; set => R[nameof(TextSmallFontSize)] = value; }
        public double LyricMediumFontSize { get => (double)R[nameof(LyricMediumFontSize)]; set => R[nameof(LyricMediumFontSize)] = value; }
        public double LyricSmallFontSize { get => (double)R[nameof(LyricSmallFontSize)]; set => R[nameof(LyricSmallFontSize)] = value; }
        public double PanelOpacity {
            get => (R[nameof(PanelOpacity)] as SolidColorBrush).Opacity;
            set => R[nameof(PanelOpacity)] = new SolidColorBrush((R[nameof(PanelOpacity)] as SolidColorBrush).Color) { Opacity = value };
        }

        public double MusicVolume { get => C.MusicVolume; set => C.MusicVolume = value; }

        [XmlIgnore]
        public KeyValuePair<string, FontFamily> PrimaryFont
        {
            get => FontFamiliesList.ToList().Find(x => x.Value.Source == (R[nameof(PrimaryFont)] as FontFamily).Source);
            set => R[nameof(PrimaryFont)] = value.Value;
        }
        public string PrimaryFontSource
        {
            get => PrimaryFont.Value.Source;
            set
            {
                PrimaryFont = FontFamiliesList.ToList().Find(x => value == x.Value.Source);
            }
        }
        [XmlIgnore]
        public KeyValuePair<string, FontFamily> LyricFont
        {
            get => FontFamiliesList.ToList().Find(x => x.Value.Source == (R[nameof(LyricFont)] as FontFamily).Source);
            set => R[nameof(LyricFont)] = value.Value;
        }
        public string LyricFontFontSource
        {
            get => LyricFont.Value.Source;
            set
            {
                LyricFont = FontFamiliesList.ToList().Find(x => value == x.Value.Source);
            }
        }


        [XmlIgnore]
        public ICommand SaveSettingsCmd => new RelayCommand(SaveSettingAsXml, () => true);
        public static string SaveFilePath { get; set; } = $@"{AppDomain.CurrentDomain.BaseDirectory}Setting.xml";
        public void SaveSettingAsXml()
        {
            Console.WriteLine("saved");
            SaveSettingAsXml(this, SaveFilePath);
        }
        public static void SaveSettingAsXml(SettingsViewModel settings, string fileName)
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
                Console.WriteLine($"{typeof(SettingsViewModel)}.SaveSettingAsXml: {ex}");
            }
        }
        public static SettingsViewModel LoadSettingFromXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { throw new ArgumentNullException("NameNull"); }
            SettingsViewModel Setting = new SettingsViewModel();
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;
                using (StringReader read = new StringReader(xmlString))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SettingsViewModel));
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        Setting = (SettingsViewModel)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(SettingsViewModel)}.LoadSettingFromXml: {ex}");
            }
            return Setting;
        }
        public static SettingsViewModel LoadOrNew()
        {
            SettingsViewModel Loadded = null, newone = new SettingsViewModel();
            if (File.Exists(SaveFilePath))
            {
                return Loadded = LoadSettingFromXml(SaveFilePath);
            }
            return newone;
        }
    }

}
