using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MusicPLayer.Styles
{
    class ColorLightnessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Int32.TryParse((string)parameter, out int val))
                val = 10;
            return ChangeLightness((value as SolidColorBrush).Color, val);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        private const int MinLightness = -100;
        private const int MaxLightness = 100;
        private static Color ChangeLightness(Color color, int lightness)
        {
            if (lightness > MaxLightness)
                return Color.FromRgb(255, 255, 255);
            if (lightness < MinLightness)
                return Color.FromRgb(0, 0, 0);
            if (lightness == 0)
                return color;
            Color ret = Color.FromArgb(
                color.A,
                (byte)ChangeValue((int)color.R, lightness),
                (byte)ChangeValue((int)color.G, lightness),
                (byte)ChangeValue((int)color.B, lightness)
                );
            if (ret == color)
                lightness = -lightness;
            ret = Color.FromArgb(
                color.A,
                (byte)ChangeValue((int)color.R, lightness),
                (byte)ChangeValue((int)color.G, lightness),
                (byte)ChangeValue((int)color.B, lightness)
                );
            return ret;
        }
        private static int ChangeValue(int value, int percent)
        {
            if (value != 0)
                return (int)Math.Max(Math.Min(Math.Round(value * (1d + percent / 100d)), 255), 0);
            else
                return value + (int)(255*percent/100d);
        }
    }

}
