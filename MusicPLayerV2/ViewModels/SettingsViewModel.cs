using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    using SettingList = List<ISettingItem>;
    using LanguagePair = KeyValuePair<XmlLanguage, string>;
    public partial class SettingsViewModel : ViewModelBase
    {
        private ResourceDictionary R => App.Current.Resources;
        private MusicPlayer PM => App.PlayerModel;
        private SongEntity NPI => App.PlayerModel.NowPlayingItem;
        private ControllerViewModel C => App.Controller;
        private PlayingListViewModel L => App.PlayingList;
        private LibraryViewModel Lib = App.Library;

        SettingList SettingList => new List<ISettingItem>()
            {
            AppLanguage,

            PrimaryColor,
            SecondaryColor,
            SecondaryColorL,
            ForegroundColor,
            LyricForegroundColor,
            LyricHighlightColor,
            LyricShadowColor,
            TextShadowColor,

            PrimaryFont,
            LyricFont,
            TextMediumFontSize,
            TextSmallFontSize,
            LyricMediumFontSize,
            LyricSmallFontSize,

            PanelOpacity,

            MusicVolume,
            BackgroundCoverVisibility,
            MiniBackgroundCoverVisibility,

            LibraryDirectories
            };



        public LanguageSetting AppLanguage { get; set; } = new LanguageSetting() { Name = nameof(AppLanguage) };
        [XmlIgnore]
        public LanguagePair LanguagePair { get => AppLanguage.Value; set => AppLanguage.Value = value; }
        [XmlIgnore]
        public string LanguageString => AppLanguage.Value.Value;

        public ColorSetting PrimaryColor { get; set; } = new ColorSetting() { Name = nameof(PrimaryColor) };
        public ColorSetting SecondaryColor { get; set; } = new ColorSetting() { Name = nameof(SecondaryColor) };
        public ColorSetting SecondaryColorL { get; set; } = new ColorSetting() { Name = nameof(SecondaryColorL) };
        public ColorSetting ForegroundColor { get; set; } = new ColorSetting() { Name = nameof(ForegroundColor) };
        public ColorSetting LyricForegroundColor { get; set; } = new ColorSetting() { Name = nameof(LyricForegroundColor) };
        public ColorSetting LyricHighlightColor { get; set; } = new ColorSetting() { Name = nameof(LyricHighlightColor) };
        public ShadowColorSetting LyricShadowColor { get; set; } = new ShadowColorSetting() { Name = "LyricShadowEffect" };
        public ShadowColorSetting TextShadowColor { get; set; } = new ShadowColorSetting() { Name = "TextShadowEffect" };

        public FontSetting PrimaryFont { get; set; } = new FontSetting() { Name = nameof(PrimaryFont) };
        public FontSetting LyricFont { get; set; } = new FontSetting() { Name = nameof(LyricFont) };

        public DoubleSetting TextMediumFontSize { get; set; } = new DoubleSetting() { Name = nameof(TextMediumFontSize) };
        public DoubleSetting TextSmallFontSize { get; set; } = new DoubleSetting() { Name = nameof(TextSmallFontSize) };
        public DoubleSetting LyricMediumFontSize { get; set; } = new DoubleSetting() { Name = nameof(LyricMediumFontSize) };
        public DoubleSetting LyricSmallFontSize { get; set; } = new DoubleSetting() { Name = nameof(LyricSmallFontSize) };

        public OpacitySetting PanelOpacity { get; set; } = new OpacitySetting() { Name = nameof(PanelOpacity) };

        public VolumeSetting MusicVolume { get; set; } = new VolumeSetting() { Name = nameof(MusicVolume) };

        public VisibilitySetting BackgroundCoverVisibility { get; set; } = new VisibilitySetting() { Name = nameof(BackgroundCoverVisibility) };
        public VisibilitySetting MiniBackgroundCoverVisibility { get; set; } = new VisibilitySetting() { Name = nameof(MiniBackgroundCoverVisibility) };

        public DirectoriesSetting LibraryDirectories { get; set; } = new DirectoriesSetting() { Name = nameof(LibraryDirectories) };
        public ICommand AddDirectoryCmd => new RelayCommand(AddDirectory, () => true);
        public ICommand RemoveDirectoryCmd => new RelayCommand(RemoveDirectory, () => DirectorySelectedIndex >= 0);
        public ICommand ScanDirectories => new RelayCommand(Lib.ScanDirectory, () => true);
        [XmlIgnore]
        public int DirectorySelectedIndex { get; set; } = 0;
        public void AddDirectory()
        {
            var settings = new MvvmDialogs.FrameworkDialogs.FolderBrowser.FolderBrowserDialogSettings()
            {
                Description = "Choose directory",
            };
            var success = new MvvmDialogs.DialogService().ShowFolderBrowserDialog(this, settings);
            if (success == true)
            {
                LibraryDirectories.Value.Add(new ScannedDirectoryInfo(settings.SelectedPath));
                NotifyPropertyChanged(nameof(LibraryDirectories.Value));
                LibraryDirectories.ApplyChange();
            }
        }
        public void RemoveDirectory()
        {
            LibraryDirectories.Value.RemoveAt(DirectorySelectedIndex);
            NotifyPropertyChanged(nameof(LibraryDirectories.Value));
            LibraryDirectories.ApplyChange();
        }
        public void ApplySetting()
        {
            SettingList.ForEach(x => {
                x.ApplyChange();
            });
            NotifyPropertyChanged(nameof(AppLanguage));
            NotifyPropertyChanged(nameof(LanguagePair));
            NotifyPropertyChanged(nameof(LanguageString));
        }
    }

    #region Setting base class
    public interface ISettingItem
    {
        string Name { get; set; }
        string String { get; set; }
        void ApplyChange();
    }
    public abstract class SettingItemStruct<T> : ISettingItem where T : struct
    {
        [XmlIgnore]
        protected ResourceDictionary R => App.Current.Resources;
        [XmlIgnore]
        public bool IsValueChanged { get; private set; }
        private T? _value;
        [XmlIgnore]
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    Console.WriteLine("_value is null Try to Get Value");
                    _value = GetValue();
                    Console.WriteLine($"_value is {_value} After Try to Get Value");
                }
                return _value.Value;
            }
            set
            {
                _value = value;
                IsValueChanged = true;
            }
        }
        public string Name { get; set; } = "SettingItem";
        public string String
        {
            get
            {
                return ConvertToString(Value);
            }
            set
            {
                _value = ConvertFromString(value);
            }
        }

        abstract public T GetValue();
        abstract public void SetValue(T newValue);
        abstract public string ConvertToString(T value);
        abstract public T ConvertFromString(string valueString);
        public void ApplyChange()
        {
            SetValue(Value);
        }
    }
    public abstract class SettingItemClass<T> : ISettingItem where T : new()
    {
        [XmlIgnore]
        protected ResourceDictionary R => App.Current.Resources;
        [XmlIgnore]
        public bool IsValueChanged { get; private set; }
        private T _value;
        [XmlIgnore]
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    Console.WriteLine("_value is null Try to Get Value");
                    _value = GetValue();
                    Console.WriteLine($"_value is {_value} After Try to Get Value");
                }
                return _value;
            }
            set
            {
                _value = value;
                IsValueChanged = true;
            }
        }
        public string Name { get; set; } = "SettingItem";
        public string String
        {
            get
            {
                return ConvertToString(Value);
            }
            set
            {
                _value = ConvertFromString(value);
            }
        }

        abstract public T GetValue();
        abstract public void SetValue(T newValue);
        abstract public string ConvertToString(T value);
        abstract public T ConvertFromString(string valueString);
        public void ApplyChange()
        {
            SetValue(Value);
        }
    }
    #endregion

    #region Color Related Setting CLass
    public class ColorSetting : SettingItemStruct<Color>
    {
        public override Color GetValue()
        {
            return (R[Name] as SolidColorBrush).Color;
        }
        public override void SetValue(Color value)
        {
            R[Name] = new SolidColorBrush() { Color = value };
        }
        public override Color ConvertFromString(string valueString)
        {
            return (Color)ColorConverter.ConvertFromString(valueString);
        }
        public override string ConvertToString(Color value)
        {
            return value.ToString();
        }
    }

    public class ShadowColorSetting:SettingItemStruct<Color>
    {
        public override Color GetValue()
        {
            return (R[Name] as DropShadowEffect).Color;
        }
        public override void SetValue(Color value)
        {
            var dse = R[Name] as DropShadowEffect;
            R[Name] = new DropShadowEffect() { BlurRadius = dse.BlurRadius, ShadowDepth = dse.ShadowDepth, Color = value };
        }
        public override Color ConvertFromString(string valueString)
        {
            return (Color)ColorConverter.ConvertFromString(valueString);
        }
        public override string ConvertToString(Color value)
        {
            return value.ToString();
        }
    }
    #endregion

    #region Language Setting CLass
    public class LanguageSetting : SettingItemStruct<LanguagePair>
    {
        [XmlIgnore]
        public List<LanguagePair> LanguageList => new List<LanguagePair>()
        {
            GetLanguagePair("zh-tw"),
            GetLanguagePair("en-us")
        };

        public LanguageSetting()
        {
            Value = ConvertFromString("zh-tw");
        }

        LanguagePair GetLanguagePair(string language)
        {
            return new LanguagePair(XmlLanguage.GetLanguage(language), (string)R[$"Setting_Language_{language}"]);
        }

        public override LanguagePair ConvertFromString(string valueString)
        {
            return LanguageList.First(x => x.Key == XmlLanguage.GetLanguage(valueString));
        }

        public override string ConvertToString(LanguagePair value)
        {
            return value.Key.ToString();
        }

        public override LanguagePair GetValue()
        {
            return Value;
        }

        public override void SetValue(LanguagePair newValue)
        {
            if(newValue.Key!= App.MainWin.Language)
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri($@"Resources/Strings/Lang.{newValue.Key.ToString()}.xaml", UriKind.Relative)
                });
                App.Current.Resources.MergedDictionaries.Remove(
                    App.Current.Resources.MergedDictionaries.
                    First(x => x.Source.OriginalString == $@"Resources/Strings/Lang.{App.MainWin.Language.ToString()}.xaml"));
                App.MainWin.Language = newValue.Key;
            }
        }

        public override string ToString()
        {
            return Value.Value;
        }
    }
    #endregion

    #region Font Setting CLass
    public class FontSetting : SettingItemStruct<FontFamilyPair>
    {
        [XmlIgnore]
        static IEnumerable<FontFamilyPair> _fontFamiliesList;
        [XmlIgnore]
        static public IEnumerable<FontFamilyPair> FontFamiliesList
        {
            get
            {
                if (_fontFamiliesList != null)
                    return _fontFamiliesList;
                _fontFamiliesList = Fonts.SystemFontFamilies.ToList().ConvertAll(font => new FontFamilyPair() { Source = font.Source, FontFamily = font })
                    .OrderBy(x => x.Source);
                return _fontFamiliesList;
            }
        }

        public void ResetFontFamiliesList()
        {
            _fontFamiliesList = null;
            var a = FontFamiliesList;
        }
        public override FontFamilyPair ConvertFromString(string valueString)
        {
            return FontFamiliesList.ToList().First(x => valueString == x.Source);
        }

        public override string ConvertToString(FontFamilyPair value)
        {
            return (R[Name] as FontFamily).Source;
        }

        public override FontFamilyPair GetValue()
        {
            return FontFamiliesList.ToList().First((font) => font.Source == (R[Name] as FontFamily).Source);
        }

        public override void SetValue(FontFamilyPair newValue)
        {
            R[Name] = newValue.FontFamily;
        }
    }

    public struct FontFamilyPair
    {
        public string Source { get; set; }
        public string Name
        {
            get
            {
                if (FontFamily.FamilyNames.Keys.Contains(App.MainWin.Language))
                    return FontFamily.FamilyNames[App.MainWin.Language];
                else if (FontFamily.FamilyNames.Keys.ToList().Exists(x => x != XmlLanguage.GetLanguage("en-us")))
                    return FontFamily.FamilyNames.First(x => x.Key != XmlLanguage.GetLanguage("en-us")).Value;
                else
                    return FontFamily.FamilyNames.First().Value;
            }
        }
        public FontFamily FontFamily { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is FontFamilyPair)
                if (((FontFamilyPair)obj).Source == Source)
                    return true;
            return false;
        }
        public override int GetHashCode()
        {
            return Source.GetHashCode();
        }
    }
    #endregion

    #region Double Related Setting Class
    public class DoubleSetting : SettingItemStruct<double>
    {
        public override double ConvertFromString(string valueString)
        {
            if (double.TryParse(valueString, out double result))
                return result;
            else
                return GetValue();
        }

        public override string ConvertToString(double value)
        {
            return value.ToString("F");
        }

        public override double GetValue()
        {
            return (double)R[Name];
        }

        public override void SetValue(double newValue)
        {
            R[Name] = newValue;
        }
    }

    public class OpacitySetting : DoubleSetting
    {

        public override double GetValue()
        {
            return (R[Name] as SolidColorBrush).Opacity;
        }

        public override void SetValue(double newValue)
        {
            R[Name] = new SolidColorBrush((R[Name] as SolidColorBrush).Color) { Opacity = newValue };
        }
    }

    public class VolumeSetting: DoubleSetting
    {
        private MusicPlayer PM => App.PlayerModel;
        public override double GetValue()
        {
            return PM.Volume;
        }
        public override void SetValue(double newValue)
        {
            PM.Volume = (float)newValue;
        }
    }
    #endregion

    #region Bool Related Setting Class
    public class BoolSetting : SettingItemStruct<bool>
    {
        public override bool ConvertFromString(string valueString)
        {
            if (bool.TryParse(valueString, out bool result))
                return result;
            else
                return GetValue();
        }
        public override string ConvertToString(bool value)
        {
            return value.ToString();
        }
        public override bool GetValue()
        {
            return (bool)R[Name];
        }
        public override void SetValue(bool newValue)
        {
            R[Name] = newValue;
        }
    }
    public class VisibilitySetting : BoolSetting
    {
        public override bool GetValue()
        {
            return ((Visibility)R[Name]) == Visibility.Visible;
        }
        public override void SetValue(bool newValue)
        {
            R[Name] = newValue ? Visibility.Visible : Visibility.Collapsed;
            if (Name.StartsWith("Mini"))
                App.MainWin.SetBackgroundCoverMode(Views.MainWindowMode.Mini);
            else
                App.MainWin.SetBackgroundCoverMode(Views.MainWindowMode.Normal);
        }
    }
    #endregion

    #region Directories Setting Class
    public class DirectoriesSetting : SettingItemClass<ObservableCollection<ScannedDirectoryInfo>>
    {
        [XmlIgnore]
        protected LibraryViewModel L => App.Library;

        public override ObservableCollection<ScannedDirectoryInfo> ConvertFromString(string valueString)
        {
            return new ObservableCollection<ScannedDirectoryInfo>(
                valueString.Split(';').ToList().ConvertAll(
                    x =>
                    {
                        var xsp = x.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                        return new ScannedDirectoryInfo(
                            xsp[0],
                            bool.Parse(xsp[1]));
                    }));
        }

        public override string ConvertToString(ObservableCollection<ScannedDirectoryInfo> value)
        {
            return string.Concat(value.Select(x => $"; {x.ToString()}::{x.IsScanAllSubDirectories.ToString()}")).Trim("; ".ToCharArray());
        }

        public override ObservableCollection<ScannedDirectoryInfo> GetValue()
        {
            return new ObservableCollection<ScannedDirectoryInfo>(L.ScannedDirectoryInfo);
        }

        public override void SetValue(ObservableCollection<ScannedDirectoryInfo> newValue)
        {
            L.ScannedDirectoryInfo = newValue.ToList();
        }
    }
    #endregion

    public partial class SettingsViewModel
    {
        [XmlIgnore]
        public ICommand SaveSettingsCmd => new RelayCommand(()=> {
            ApplySetting();
            SaveSettingAsXml();
            MessageBox.Show((string)R["Setting_Saved"], App.MainWin.Title);
        }, () => true);
        [XmlIgnore]
        public ICommand ApplySettingsCmd => new RelayCommand(() =>
        {
            ApplySetting();
        }, () => true);
        public static string SaveFilePath { get; set; } = $@"{App.ExecuteFilePath}Setting.xml";
        public void SaveSettingAsXml()
        {
            ObjectSaveToXML<SettingsViewModel>.SaveSettingAsXml(this, SaveFilePath);
        }
        public static SettingsViewModel LoadOrNew()
        {
            SettingsViewModel Loadded = null, newone = new SettingsViewModel();
            if (File.Exists(SaveFilePath))
            {
                Loadded = ObjectSaveToXML<SettingsViewModel>.LoadSettingFromXml(SaveFilePath);
                Loadded.ApplySetting();
                return Loadded;
            }
            return newone;
        }
    }
}
