using MusicPLayer.Models;
using MusicPLayer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace MusicPLayer.Views
{
    /// <summary>
    /// LyricPage.xaml 的互動邏輯
    /// </summary>
    public partial class LyricPage : UserControl, INotifyPropertyChanged
    {
        public LyricPage()
        {
            InitializeComponent();
        }

        LRCParser parser = new LRCParser();
        bool sizeChanging = false;

        public string FilePath
        {
            get => parser.FileName;
            set
            {
                parser.FileName = value;
                if (parser.IsLoaded)
                    Lyrics = parser.Lyrics;
                else
                    Lyrics = LRCParser.NoLyricMessage;
                NotifyPropertyChanged(nameof(Lyrics));
            }
        }

        public List<LyricWithTime> Lyrics { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SolidColorBrush BackColor { get => (SolidColorBrush)GetValue(BackColorProperty); set => SetValue(BackColorProperty, value); }
        public static readonly DependencyProperty BackColorProperty = DependencyProperty.Register(nameof(BackColor), typeof(SolidColorBrush), typeof(LyricPage),
            new FrameworkPropertyMetadata(Brushes.Black));

        public SolidColorBrush ForeColor { get => (SolidColorBrush)GetValue(ForeColorProperty); set => SetValue(ForeColorProperty, value); }
        public static readonly DependencyProperty ForeColorProperty = DependencyProperty.Register(nameof(ForeColor), typeof(SolidColorBrush), typeof(LyricPage),
            new FrameworkPropertyMetadata(Brushes.White));

        public SolidColorBrush ForeHighlightColor { get => (SolidColorBrush)GetValue(ForeHighlightColorProperty); set => SetValue(ForeHighlightColorProperty, value); }
        public static readonly DependencyProperty ForeHighlightColorProperty = DependencyProperty.Register(nameof(ForeHighlightColor), typeof(SolidColorBrush), typeof(LyricPage),
            new FrameworkPropertyMetadata(Brushes.Lime));

        public FontFamily ForeFont { get => (FontFamily)GetValue(ForeFontProperty); set => SetValue(ForeFontProperty, value); }
        public static readonly DependencyProperty ForeFontProperty = DependencyProperty.Register(nameof(ForeFont), typeof(FontFamily), typeof(LyricPage),
            new FrameworkPropertyMetadata(new FontFamily("Microsoft JhengHei")));

        public double ForeFontSize { get => (double)GetValue(ForeFontSizeProperty); set => SetValue(ForeFontSizeProperty, value); }
        public static readonly DependencyProperty ForeFontSizeProperty = DependencyProperty.Register(nameof(ForeFontSize), typeof(double), typeof(LyricPage),
            new FrameworkPropertyMetadata(20d));

        public TimeSpan NowValue
        {
            get => (TimeSpan)GetValue(NowValueProperty); set
            {
                SetValue(NowValueProperty, value);
            }
        }
        public static readonly DependencyProperty NowValueProperty = DependencyProperty.Register(nameof(NowValue), typeof(TimeSpan), typeof(LyricPage),
            new FrameworkPropertyMetadata(TimeSpan.Zero,(DependencyObject obj, DependencyPropertyChangedEventArgs args)=>
            {
                if(Mouse.LeftButton==MouseButtonState.Released)
                    (obj as LyricPage).sizeChanging = false;
                if (!(obj as LyricPage).sizeChanging)
                    if ((obj as LyricPage).parser.IsLoaded)
                        (obj as LyricPage).NotifyPropertyChanged("ListViewCTop");
            }));

        int lastIndex = -1;
        public double ListViewCTop
        {
            get
            {
                var nowTimeLyricIdx = parser.GetLyricIdxFromTime(NowValue);
                if (nowTimeLyricIdx == -1)
                    return (canvas.ActualHeight / 2);
                if (LinesHeight.Count > 0)
                    if (LinesHeight[nowTimeLyricIdx] ==0)
                        if (LyricListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            ResetLinesHeight(LyricListView.ItemContainerGenerator);
                var nowTimeLyricTime = parser.Lyrics[nowTimeLyricIdx].Time;
                var nextTimeLyricTime = parser.Lyrics[nowTimeLyricIdx + 1].Time;
                var offsetPastLineHeight = 0d;
                if (LinesHeight.Count > 0)
                    for (int i = 0; i < nowTimeLyricIdx; i++)
                        offsetPastLineHeight += LinesHeight[i];
                var offsetNowLineHeight = 0d;
                if (LinesHeight.Count > 0)
                {
                    offsetNowLineHeight = (NowValue - nowTimeLyricTime).TotalMilliseconds / (nextTimeLyricTime - nowTimeLyricTime).TotalMilliseconds * LinesHeight[nowTimeLyricIdx];
                }
                if (nowTimeLyricIdx != lastIndex)
                {
                    if (LyricListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        var c = LyricListView.ItemContainerGenerator.ContainerFromIndex(nowTimeLyricIdx);
                        if (c == null)
                            goto End;
                        c = VisualTreeHelper.GetChild(c, 0);
                        c = VisualTreeHelper.GetChild(c, 0);
                        var l = VisualTreeHelper.GetChild(c, 0) as Label;
                        l.Foreground = new SolidColorBrush(ForeColor.Color);
                        l.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
                        {
                            From = ForeColor.Color,
                            To = ForeHighlightColor.Color,
                            Duration = TimeSpan.FromMilliseconds(250)
                        });
                        if (lastIndex >= 0)
                        {
                            var cc = LyricListView.ItemContainerGenerator.ContainerFromIndex(lastIndex);
                            cc = VisualTreeHelper.GetChild(cc, 0);
                            cc = VisualTreeHelper.GetChild(cc, 0);
                            var ll = VisualTreeHelper.GetChild(cc, 0) as Label;
                            ll.Foreground = new SolidColorBrush(ForeHighlightColor.Color);
                            ll.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
                            {
                                From = ForeHighlightColor.Color,
                                To = ForeColor.Color,
                                Duration = TimeSpan.FromMilliseconds(250)
                            });
                        }

                    }
                    lastIndex = nowTimeLyricIdx;
                }
                End:
                return (canvas.ActualHeight / 2) - (offsetPastLineHeight + offsetNowLineHeight) /*+ ForeFontSize*/;
            }
        }

        private void userControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sizeChanging = true;
            Console.WriteLine("SizeChanged");
            ResetLinesHeight(LyricListView.ItemContainerGenerator);
        }

        List<double> _linesHeight = new List<double>();
        public List<double> LinesHeight => _linesHeight;

        private void LyricListView_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as ListView).ItemContainerGenerator.StatusChanged += (object esender, EventArgs ee)=>
            {
                var icg = (esender as ItemContainerGenerator);
                var status = icg.Status;
                if (status == GeneratorStatus.ContainersGenerated)
                    ResetLinesHeight(icg);
            };
        }
        public void ResetLinesHeight()
        {
            ResetLinesHeight(LyricListView.ItemContainerGenerator);
        }
        private void ResetLinesHeight(ItemContainerGenerator icg)
        {
            _linesHeight.Clear();
            _linesHeight.AddRange(Enumerable.Repeat(0d, icg.Items.Count));
            for (int i = 0; i < icg.Items.Count; i++)
            {
                var border = VisualTreeHelper.GetChild(LyricListView.ItemContainerGenerator.ContainerFromIndex(i), 0) as Border;
                _linesHeight[i] = border.ActualHeight;
            }
        }

    }
}
