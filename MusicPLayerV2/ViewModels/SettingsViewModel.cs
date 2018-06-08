using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MusicPLayerV2.ViewModels
{
    public class SettingsViewModel: ViewModelBase
    {
        private ResourceDictionary R => App.Current.Resources;
        private MusicPlayer PM => App.PlayerModel;
        private MusicItem NPI => App.PlayerModel.NowPlayingItem;
        private ControllerViewModel C => App.Controller;
        private PlayingListViewModel L => App.PlayingList;

        private string _language = "zh-tw";
        public string Language
        {
            get => _language;
            set
            {
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
        public Color PrimaryColor
        {
            get => (R["PrimaryColor"] as SolidColorBrush).Color;
            set
            {
                R["PrimaryColor"] = new SolidColorBrush(value);
                var max = Math.Max(Math.Max(value.R, value.G), value.B);
                var min = Math.Min(Math.Min(value.R, value.G), value.B);
                if((max + min) / 2d / 255d * 100d > 50)
                {
                    R["Color25"] = new SolidColorBrush(Colors.Black) { Opacity = 0.1 };
                    R["Color50"] = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };
                    R["Color75"] = new SolidColorBrush(Colors.Black) { Opacity = 0.75 };
                }
                else
                {
                    R["Color25"] = new SolidColorBrush(Colors.White) { Opacity = 0.1 };
                    R["Color50"] = new SolidColorBrush(Colors.White) { Opacity = 0.5 };
                    R["Color75"] = new SolidColorBrush(Colors.White) { Opacity = 0.75 };
                }
            }
        }
        public Color SecondaryColor { get => (R["SecondaryColor"] as SolidColorBrush).Color; set => R["SecondaryColor"] = new SolidColorBrush(value); }
        public Color SecondaryColorL { get => (R["SecondaryColorL"] as SolidColorBrush).Color; set => R["SecondaryColorL"] = new SolidColorBrush(value); }
        public Color ForegroundColor { get => (R["ForegroundColor"] as SolidColorBrush).Color; set => R["ForegroundColor"] = new SolidColorBrush(value); }

        private IEnumerable<KeyValuePair<string, FontFamily>> _fontFamiliesList;
        public IEnumerable<KeyValuePair<string, FontFamily>> FontFamiliesList
        {
            get
            {
                if (_fontFamiliesList != null)
                    return _fontFamiliesList;
                else
                    return _fontFamiliesList = Fonts.SystemFontFamilies.ToList().ConvertAll(font =>
                    {
                        if (font.FamilyNames.Keys.Contains(App.MainWin.Language))
                            return new KeyValuePair<string, FontFamily>(font.FamilyNames[App.MainWin.Language], font);
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

        public KeyValuePair<string, FontFamily> PrimaryFont
        {
            get => FontFamiliesList.ToList().Find(x => x.Value.Source == (R[nameof(PrimaryFont)] as FontFamily).Source);
            set => R[nameof(PrimaryFont)] = value.Value;
        }
        public KeyValuePair<string, FontFamily> LyricFont
        {
            get => FontFamiliesList.ToList().Find(x => x.Value.Source == (R[nameof(LyricFont)] as FontFamily).Source);
            set => R[nameof(LyricFont)] = value.Value;
        }

    }

}
