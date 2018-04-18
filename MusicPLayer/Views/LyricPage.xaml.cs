using MusicPLayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace MusicPLayer.Views
{
    /// <summary>
    /// LyricPage.xaml 的互動邏輯
    /// </summary>
    public partial class LyricPage : UserControl
    {
        public LyricPage()
        {
            InitializeComponent();
        }

        private void LyricListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set {
                SetValue(FileNameProperty, value);
            }
        }
        static public readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
            nameof(FileName), typeof(string), typeof(LyricPage),
            new FrameworkPropertyMetadata(null,
                (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
                {
                    if(File.Exists((obj as LyricPage).FileName))
                    {
                        var file = new FileInfo((obj as LyricPage).FileName);
                        var l = file.DirectoryName + @"\" + file.Name.Replace(file.Extension, "") + ".lrc";
                        if (File.Exists(l))
                            (obj as LyricPage).LrcPath = l;
                        else
                            (obj as LyricPage).LrcPath = "";
                    }
                }));
        string LrcPath { set
            {
                _lyricParser.FileName = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    LyricListView.ItemsSource = null;
                    _linesHeight = null;
                }
                else
                {

                    LyricListView.ItemsSource = _lyricParser.Lyrics;
                }

            }
        }

        public BitmapImage BackgroundImg
        {
            set
            {
                //BgImg.Source = value;
            }

        }

        public TimeSpan Max
        {
            get => (TimeSpan)GetValue(MaxProperty);
            set=>SetValue(MaxProperty,value);
        }
        static public readonly DependencyProperty MaxProperty = DependencyProperty.Register(
            nameof(Max), typeof(TimeSpan), typeof(LyricPage),
            new PropertyMetadata(TimeSpan.MaxValue));

        public TimeSpan Min => TimeSpan.Zero;

        public TimeSpan Now
        {
            get => (TimeSpan)GetValue(NowProperty);
            set => SetValue(NowProperty, value);
        }
        public static SolidColorBrush HighlightColor => Brushes.Lime;
        public static SolidColorBrush NormalColor => Brushes.White;
        double CenterPosi => canvas.ActualHeight / 2;
        static public readonly DependencyProperty NowProperty = DependencyProperty.Register(
            nameof(Now), typeof(TimeSpan), typeof(LyricPage),
            new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
                {
                    var t = (obj as LyricPage);
                    if (!t._isShown || !t._lyricParser.IsLoaded)
                    {
                        return;
                    }
                    if(t.LyricListView.Items.Count == 0 || t.LyricListView.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        var n = t.LinesHeight;
                        return;
                    }
                    var now = (TimeSpan)args.NewValue;
                    var _lyricParser = t._lyricParser;
                    var offset = t.GetOffset(now);
                    if (offset == 0)
                    {
                        t.SizeChanged = true;
                    }
                    var lidx = Math.Max(_lyricParser.GetLyricIdxFromTime(now), 0);
                    var nlll = (t.LyricListView.ItemContainerGenerator.ContainerFromIndex(lidx) as ListViewItem);
                    if (nlll == null)
                    {
                        Console.WriteLine($"{t.LyricListView.ItemContainerGenerator.Status}   {t.LyricListView.ItemContainerGenerator.Items.Count}\n" +
                            $"{VisualTreeHelper.GetChildrenCount(t.LyricListView)}");
                        return;
                    }
                    var nll1 = VisualTreeHelper.GetChild(nlll, 0);
                    nll1 = VisualTreeHelper.GetChild(nll1, 0);
                    nll1 = VisualTreeHelper.GetChild(nll1, 0);
                    nll1 = VisualTreeHelper.GetChild(nll1, 1);
                    var nll = nll1 as Label;
                    if (lidx != 0)
                    {
                        var lll = VisualTreeHelper.GetChild(
                        VisualTreeHelper.GetChild(
                            VisualTreeHelper.GetChild(
                                VisualTreeHelper.GetChild(
                                    (t.LyricListView.ItemContainerGenerator.ContainerFromIndex(lidx - 1) as ListViewItem), 0), 0), 0), 1) as Label;
                        lll.Foreground = NormalColor;
                    }
                    nll.Foreground = HighlightColor;
                    Canvas.SetTop(t.LyricListView, t.CenterPosi - offset);


                }));


        LRCParser _lyricParser = new LRCParser();
        bool SizeChanged = false;
        double[] Zeros(int count)
        {
            if (count <= 0)
                return null;
            var d = new double[count];
            for(int i = 0; i < count; i++)
            {
                d[i] = 0;
            }
            return d;
        }
        List<double> _linesHeight = null;
        List<double> LinesHeight
        {
            get
            {
                if (_linesHeight != null)
                {
                    if (SizeChanged)
                    {
                        if (_linesHeight.Count < _lyricParser.Lyrics.Count())
                            _linesHeight.AddRange(Zeros(_lyricParser.Lyrics.Count()-_linesHeight.Count));
                        for (int i = 0; i < _lyricParser.Lyrics.Count(); i++)
                        {
                            var Do = VisualTreeHelper.GetChild(LyricListView.ItemContainerGenerator.ContainerFromIndex(i), 0) as Border;
                            if (i < _linesHeight.Count)
                                _linesHeight[i] = Do.ActualHeight;
                        }
                    }

                    SizeChanged = false;
                    return _linesHeight;
                }
                _linesHeight = new List<double>();
                for (int i = 0; i < _lyricParser.Lyrics.Count(); i++)
                {
                    var Do2 = LyricListView.ItemContainerGenerator.ContainerFromIndex(i);
                    if (Do2 == null)
                        return _linesHeight;
                    var Do = VisualTreeHelper.GetChild(Do2,0) as Border;
                    _linesHeight.Add(Do.ActualHeight);

                }
                return _linesHeight;
            }
        }
        private double GetOffset(TimeSpan p)
        {
            var NowLyricTime = _lyricParser.GetLyricFromTime(p).Time;
            var NextLyricTime = _lyricParser.GetNextLyricFromTime(p).Time;
            var NowLyricIdx = _lyricParser.GetLyricIdxFromTime(p);
            var offset1 = 0d;
            for (int i = 0; i < NowLyricIdx ; i++)
            {
                if (LinesHeight.Count > 0)
                    offset1 += LinesHeight[i];
            }
            var offset2 = 0d;
            if (LinesHeight.Count > 0)
            {
                offset2 = (p.TotalMilliseconds-NowLyricTime.TotalMilliseconds) / (NextLyricTime - NowLyricTime).TotalMilliseconds * LinesHeight[NowLyricIdx];
            }
            return offset1 + offset2;
        }

        bool _isShown = false;
        public bool IsShown { get => _isShown; set => _isShown = value; }

        public void ReSetColor()
        {
            if (!IsShown||!_lyricParser.IsLoaded)
                return;
            for (int i = 0; i < LyricListView.Items.Count; i++)
            {
                var lll =
                    VisualTreeHelper.GetChild(
                    VisualTreeHelper.GetChild(
                    VisualTreeHelper.GetChild(
                        VisualTreeHelper.GetChild(
                            (LyricListView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem), 0), 0), 0), 1) as Label;
                lll.Foreground = NormalColor;
            }
        }
        public void MoveTo(ListView target, double? newTop)
        {
            var top = Canvas.GetTop(target);
            var left = Canvas.GetLeft(target);
            Storyboard story = new Storyboard();
            DoubleAnimation AniTop = new DoubleAnimation();
            AniTop.From = top;
            AniTop.To = newTop;
            AniTop.Duration = new Duration(TimeSpan.FromMilliseconds(50));
            story.Children.Add(AniTop);
            Storyboard.SetTargetName(AniTop, target.Name);
            Storyboard.SetTargetProperty(AniTop, new PropertyPath(Canvas.TopProperty));
            story.Begin(this);
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeChanged = true;
            if (IsLoaded)
                ReSetColor();
        }

    }
}
