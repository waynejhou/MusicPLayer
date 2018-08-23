using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
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
using System.Timers;

namespace MusicPLayerV2.Views.UserControls
{
    /// <summary>
    /// LyricPage.xaml 的互動邏輯
    /// </summary>
    public partial class LyricDisplayControl : UserControl
    {
        public LyricDisplayControl()
        {
            InitializeComponent();
            LyricsItem.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            timer.Elapsed += Timer_Elapsed;
        }

        private void ReloadLyricFile(object sender, RoutedEventArgs e)
        {
            ReloadLyricFile();
        }
        private void ReloadLyricFile()
        {
            if (parser.IsLoaded)
                OnFilePathSet(LyricFilePath);
        }
        private void OpenLyricFile(object sender, RoutedEventArgs e)
        {
            OpenLyricFile();
        }
        private void OpenLyricFile()
        {
            if (parser.IsLoaded)
                Process.Start(parser.FileName);
        }

        LRCParser parser = new LRCParser();
        public string LyricFilePath
        {
            get { return (string)GetValue(LyricFilePathProperty); }
            set { SetValue(LyricFilePathProperty, value); }
        }
        public static readonly DependencyProperty LyricFilePathProperty =
            DependencyProperty.Register(nameof(LyricFilePath), typeof(string), typeof(LyricDisplayControl),
                new FrameworkPropertyMetadata((DependencyObject obj, DependencyPropertyChangedEventArgs args)=>
                {
                    (obj as LyricDisplayControl).OnFilePathSet((string)args.NewValue);
                }));
        private void OnFilePathSet(string newValue)
        {
            parser.FileName = newValue;
            if (parser.IsLoaded)
                LyricsItem.ItemsSource = parser.Lyrics;
            else
                LyricsItem.ItemsSource = LRCParser.NoLyricMessage;
            LinesHeight = null;
        }

        public Brush HighLight
        {
            get { return (Brush)GetValue(HighLightProperty); }
            set { SetValue(HighLightProperty, value); }
        }
        public static readonly DependencyProperty HighLightProperty =
            DependencyProperty.Register(nameof(HighLight), typeof(Brush), typeof(LyricDisplayControl),
                new FrameworkPropertyMetadata(Brushes.Green, (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
                {
                    (obj as LyricDisplayControl).OnHighLightSet((Brush)args.NewValue);
                }));
        private void OnHighLightSet(Brush newValue)
        {
        }



        public TimeSpan Position
        {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(TimeSpan), typeof(LyricDisplayControl),
                new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
                    {
                        (obj as LyricDisplayControl).OnPositionSet((TimeSpan)args.NewValue);
                    }));

        public bool IsLyricLoaded => parser.IsLoaded;



        int lastIndex = -1;
        double CenterOffset => border.ActualHeight / 2;
        private void OnPositionSet(TimeSpan newValue)
        {
            var nowIdx = parser.GetLyricIdxFromTime(newValue);
            if (nowIdx == -1)
            {
                LyricsItem.Margin = new Thickness(0, CenterOffset / 2, 0, 0);
                return;
            }
            if (LinesHeight == null)
                CalcLinesHeight();
            else if (LinesHeight.Length != LyricsItem.Items.Count)
                CalcLinesHeight();
            else if (LinesHeight[nowIdx] <= 0)
                CalcLinesHeight();
            var nowTime = parser.Lyrics[nowIdx].Time;
            var nextTime = parser.Lyrics[nowIdx + 1].Time;
            var pastedLinesHeight = 0d;
            for (int i = 0; i < nowIdx; i++)
                pastedLinesHeight += LinesHeight[i];
            var offsetNowLineHeight = 0d;
            offsetNowLineHeight = (newValue - nowTime).TotalMilliseconds / (nextTime - nowTime).TotalMilliseconds * LinesHeight[nowIdx];
            LyricsItem.Margin = new Thickness(0,
                CenterOffset - (pastedLinesHeight + offsetNowLineHeight)
                , 0, 0);
            if (nowIdx != lastIndex)
            {
                if (nowIdx >= 0)
                    parser.Lyrics[nowIdx].IsHightLighted = true;
                if (lastIndex >= 0 && lastIndex < parser.Lyrics.Count)
                    parser.Lyrics[lastIndex].IsHightLighted = false;
                lastIndex = nowIdx;
            }
        }

        double[] LinesHeight { get; set; }
        void CalcLinesHeight()
        {
            LinesHeight = new double[LyricsItem.Items.Count];
            for (int i = 0; i < LyricsItem.Items.Count; i++)
            {
                if (LyricsItem.ItemContainerGenerator.ContainerFromIndex(i) == null)
                    return;
                if (VisualTreeHelper.GetChildrenCount(LyricsItem.ItemContainerGenerator.ContainerFromIndex(i)) <= 0)
                    return;
                var label = VisualTreeHelper.GetChild(LyricsItem.ItemContainerGenerator.ContainerFromIndex(i), 0) as Label;
                LinesHeight[i] = label.ActualHeight;
            }
        }
        Timer timer = new Timer();

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (LyricsItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    LyricsItem.Dispatcher.Invoke(CalcLinesHeight);
            }
            catch (TaskCanceledException) { }

        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (LyricsItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                CalcLinesHeight();
            timer.AutoReset = false;
            timer.Interval = 500;
            timer.Start();
        }

        private void LyricOne_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalcLinesHeight();
        }
        public void JumpToNextLyric()
        {
            if (IsLyricLoaded)
            {
                Position = parser.GetNextLyricFromTime(Position).Time;
            }
        }
        public void JumpToPrevLyric()
        {
            if (IsLyricLoaded)
            {
                Position = parser.GetPrevLyricFromTime(Position).Time;
            }
        }

    }
}
