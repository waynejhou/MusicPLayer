
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicPLayer.Views
{
    /// <summary>
    /// MySlider.xaml 的互動邏輯
    /// </summary>
    public partial class TimeSlider : UserControl
    {

        public TimeSlider()
        {
            InitializeComponent();
        }

        public delegate void ValueHandChangedEventHandler(object sender, TimeSpan time);
        public event ValueHandChangedEventHandler ValueHandChanged;

        public TimeSpan Min
        {
            get
            {
                return (TimeSpan)GetValue(MinProperty);
            }
            set
            {
                SetValue(MinProperty, value < Max ? value : Max);
            }
        }
        public string MinToString => Min.ToString(@"mm\:ss");
        public TimeSpan Now {
            get
            {
                return (TimeSpan)GetValue(NowProperty);
            }
            set
            {
                SetValue(NowProperty, (value > Max) ? Max : (Max < Min) ? Min : value);
            }
        }
        public string NowToString => Now.ToString(@"mm\:ss");
        public TimeSpan Max
        {
            get
            {
                return (TimeSpan)GetValue(MaxProperty);
            }
            set {
                SetValue(NowProperty, value > Min ? value : Min);
            }
        }
        public string MaxToString => Max.ToString(@"mm\:ss");
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(nameof(Min), typeof(TimeSpan), typeof(TimeSlider),
            new FrameworkPropertyMetadata(TimeSpan.Zero));
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(nameof(Max), typeof(TimeSpan), typeof(TimeSlider),
            new FrameworkPropertyMetadata(TimeSpan.FromMinutes(5)));
        public static readonly DependencyProperty NowProperty = DependencyProperty.Register(nameof(Now), typeof(TimeSpan), typeof(TimeSlider),
            new FrameworkPropertyMetadata(TimeSpan.Zero,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (DependencyObject obj,DependencyPropertyChangedEventArgs e)=>
                {
                    if (!(obj as TimeSlider).IsChecking)
                    {
                        (obj as TimeSlider).NowTime.Content = (obj as TimeSlider).Now.ToString(@"mm\:ss");
                        (obj as TimeSlider).SetSliderForeMargin((obj as TimeSlider).Now);
                    }

                }));


        public SolidColorBrush BarForeColor { get => (SolidColorBrush)GetValue(BarForeColorProperty); set => SetValue(BarForeColorProperty, value); }
        public SolidColorBrush BarBackColor { get => (SolidColorBrush)GetValue(BarBackColorProperty); set => SetValue(BarBackColorProperty, value); }
        public SolidColorBrush BarWordColor { get => (SolidColorBrush)GetValue(BarWordColorProperty); set => SetValue(BarWordColorProperty, value); }
        public static readonly DependencyProperty BarForeColorProperty = DependencyProperty.Register(nameof(BarForeColor),typeof(SolidColorBrush), typeof(TimeSlider),
            new FrameworkPropertyMetadata(Brushes.Blue));
        public static readonly DependencyProperty BarBackColorProperty = DependencyProperty.Register(nameof(BarBackColor),typeof(SolidColorBrush), typeof(TimeSlider),
            new FrameworkPropertyMetadata(Brushes.Gray));
        public static readonly DependencyProperty BarWordColorProperty = DependencyProperty.Register(nameof(BarWordColor), typeof(SolidColorBrush), typeof(TimeSlider),
            new FrameworkPropertyMetadata(Brushes.Black));


        private void SliderBack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetSliderForeMargin(Now);
        }

        private void SliderBack_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var mod = ModTimeCount(e.GetPosition(SliderBack).X);
            NowTime.Content = mod.ToString(@"mm\:ss");
            if (IsModding)
                SetSliderForeMargin(mod);
        }

        private void SetSliderForeMargin( TimeSpan time)
        {
            var l = Math.Max(SliderBack.ActualWidth - (SliderBack.ActualWidth / (Max.TotalMilliseconds) * time.TotalMilliseconds), 0);
            l = double.IsNaN(l) ? 0d : l;
            SliderFore.Margin = new Thickness(0, 0, l, 0);
        }

        private void SliderBack_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            NowTime.Content = Now.ToString(@"mm\:ss");
            IsChecking = false;
            IsModding = false;
        }

        private void SliderBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsModding = true;
            var mod = ModTimeCount(e.GetPosition(SliderBack).X);
            if (IsModding)
                SetSliderForeMargin(mod);
        }

        private TimeSpan ModTimeCount(double x) => TimeSpan.FromMilliseconds(x / SliderBack.ActualWidth * Max.TotalMilliseconds);

        public bool IsChecking {
            get
            {
                return (bool)GetValue(IsCheckingProperty);
            }
            protected set
            {
                SetValue(IsCheckingProperty, value);
            }
        }
        public static readonly DependencyProperty IsCheckingProperty =
            DependencyProperty.Register(nameof(IsChecking), typeof(bool), typeof(TimeSlider),
                new PropertyMetadata(false));
        public bool IsModding
        {
            get
            {
                return (bool)GetValue(IsModdingProperty);
            }
            protected set
            {
                SetValue(IsModdingProperty, value);
            }
        }
        public static readonly DependencyProperty IsModdingProperty =
            DependencyProperty.Register(nameof(IsModding), typeof(bool), typeof(TimeSlider),
                new PropertyMetadata(false));

        private void SliderBack_MouseEnter(object sender, MouseEventArgs e)
        {
            IsChecking = true;
        }

        private void SliderBack_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
            if (IsModding == true)
            {
                Now = ModTimeCount(e.GetPosition(SliderBack).X);
                ValueHandChanged?.Invoke(this, Now);
            }
            IsModding = false;
            
        }

    }
    public class TimeSpanToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).ToString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.ParseExact((string)value, (string)parameter,null);
        }
    }
    public class TimeSpanToMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).ToString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.ParseExact((string)value, (string)parameter, null);
        }
    }
}
