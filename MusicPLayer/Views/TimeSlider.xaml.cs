
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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
        TimeSpan _now = TimeSpan.Zero;
        TimeSpan _max = TimeSpan.FromMinutes(5);
        TimeSpan _min => TimeSpan.Zero;

        public delegate void ValueHandChangedEventHandler(object sender, TimeSpan time);
        public event ValueHandChangedEventHandler ValueHandChanged;

        public TimeSpan Now { get => _now;
            set
            {
                _now = (value > _max) ? _max : (value < _min) ? _min : value;
                SetSliderForeMargin();
            }
        }
        public TimeSpan Max
        {
            get => _max;
            set {
                _max = value;
            }
        }
        public TimeSpan Min => TimeSpan.Zero;
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(nameof(Max), typeof(TimeSpan), typeof(TimeSlider),
            new PropertyMetadata(TimeSpan.Zero,(DependencyObject obj,DependencyPropertyChangedEventArgs args )=>
            {
                (obj as TimeSlider).MaxTime.Content = ((TimeSpan)args.NewValue).ToString(@"mm\:ss");
                (obj as TimeSlider).Max = (TimeSpan)args.NewValue;
            }));

        public static readonly DependencyProperty NowProperty = DependencyProperty.Register(nameof(Now), typeof(TimeSpan), typeof(TimeSlider),
            new FrameworkPropertyMetadata(TimeSpan.Zero,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
                {
                    if (!(obj as TimeSlider).IsChecking)
                        (obj as TimeSlider).NowTime.Content = ((TimeSpan)args.NewValue).ToString(@"mm\:ss");
                    if (!(obj as TimeSlider).IsModding)
                        (obj as TimeSlider).Now = (TimeSpan)args.NewValue;
                }));


        private SolidColorBrush _barForeColor = new SolidColorBrush();
        private SolidColorBrush _barBackColor = new SolidColorBrush();
        private SolidColorBrush _barWordColor = new SolidColorBrush();
        public SolidColorBrush BarForeColor { get => _barForeColor; set => _barForeColor = value; }
        public SolidColorBrush BarBackColor { get => _barBackColor; set => _barBackColor = value; }
        public SolidColorBrush BarWordColor { get => _barWordColor; set => _barWordColor = value; }
        public static readonly DependencyProperty BarForeColorProperty = DependencyProperty.Register(nameof(BarForeColor),typeof(TimeSpan), typeof(TimeSlider),
            new PropertyMetadata((DependencyObject obj, DependencyPropertyChangedEventArgs args)=>
            {
                (obj as TimeSlider).SliderFore.Background = args.NewValue as SolidColorBrush;
            }));
        public static readonly DependencyProperty BarBackColorProperty = DependencyProperty.Register(nameof(BarBackColor),typeof(TimeSpan), typeof(TimeSlider),
            new PropertyMetadata((DependencyObject obj, DependencyPropertyChangedEventArgs args)=>
            {
                (obj as TimeSlider).SliderBack.Background = args.NewValue as SolidColorBrush;
            }));
        public static readonly DependencyProperty BarWordColorProperty = DependencyProperty.Register(nameof(BarWordColor), typeof(TimeSpan), typeof(TimeSlider),
            new PropertyMetadata((DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
            {
                (obj as TimeSlider).NowTime.Foreground = args.NewValue as SolidColorBrush;
                (obj as TimeSlider).MaxTime.Foreground = args.NewValue as SolidColorBrush;
            }));
        private void SliderBack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetSliderForeMargin();
        }

        private void SliderBack_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var mod = ModTimeCount(e.GetPosition(SliderBack).X);
            NowTime.Content = mod.ToString(@"mm\:ss");
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                Now = mod;
        }

        private void SetSliderForeMargin()
        {
            SliderFore.Margin = new Thickness(0, 0,  Math.Max(SliderBack.ActualWidth - (SliderBack.ActualWidth / (_max.TotalMilliseconds) * Now.TotalMilliseconds),0), 0);
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
            Now = ModTimeCount(e.GetPosition(SliderBack).X);
        }

        private TimeSpan ModTimeCount(double x) => TimeSpan.FromMilliseconds(x / SliderBack.ActualWidth * _max.TotalMilliseconds);

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
                SetValue(NowProperty, _now);
                ValueHandChanged?.Invoke(this, Now);
            }
            IsModding = false;

        }
    }
}
