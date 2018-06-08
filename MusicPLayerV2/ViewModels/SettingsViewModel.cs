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
        public Color PrimaryColor { get => (R["PrimaryColor"] as SolidColorBrush).Color; set => R["PrimaryColor"] = new SolidColorBrush(value); }
        public Color SecondaryColor { get => (R["SecondaryColor"] as SolidColorBrush).Color; set => R["SecondaryColor"] = new SolidColorBrush(value); }
        public Color SecondaryColorL { get => (R["SecondaryColorL"] as SolidColorBrush).Color; set => R["SecondaryColorL"] = new SolidColorBrush(value); }
        public Color ForegroundColor { get => (R["ForegroundColor"] as SolidColorBrush).Color; set => R["ForegroundColor"] = new SolidColorBrush(value); }
        public IEnumerable<KeyValuePair<string, FontFamily>> FontFamiliesList =>
            Fonts.SystemFontFamilies.ToList().ConvertAll<KeyValuePair<string, FontFamily>>(font =>
        {
            if (font.FamilyNames.Keys.Contains(App.MainWin.Language))
            {
                return new KeyValuePair<string, FontFamily>(font.FamilyNames[App.MainWin.Language], font);
            }
            else if (font.FamilyNames.Keys.ToList().Exists(x=>x!=XmlLanguage.GetLanguage("en-us")))
            {
                return new KeyValuePair<string, FontFamily>(font.FamilyNames.First(x => x.Key != XmlLanguage.GetLanguage("en-us")).Value, font);
            }
            else
            {
                return new KeyValuePair<string, FontFamily>(font.FamilyNames.First().Value, font);
            }
        }).OrderBy(x => x.Key);

        public double SmallFontSize { get => (double)R["SmallFontSize"]; set => R["SmallFontSize"] = value; }
        public double MediumFontSize { get => (double)R["MediumFontSize"]; set => R["MediumFontSize"] = value; }

        public KeyValuePair<string, FontFamily> LyricFont
        {
            get => FontFamiliesList.ToList().Find(x => x.Value == (R["LyricFont"] as FontFamily));
            set => R["LyricFont"] = value.Value;
        }

        public SettingsViewModel()
        {
            foreach(var f in FontFamiliesList)
            {
                Console.WriteLine(f.Key);
            }
        }
    }

}
