using MusicPLayer.Utils;
using MusicPLayer.ViewModels;
using MusicPLayer.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPLayer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
        }

        private void volumeBtn_Click(object sender, RoutedEventArgs e)
        {
            VolumePopup.IsOpen = !VolumePopup.IsOpen;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.PlayerModel?.Dispose();
            Environment.Exit(0);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (TagCtrl != null)
            {
                if ((sender as RadioButton) == AlbumArtTab)
                {
                    TagCtrl.SelectedIndex = 0;
                }
                else if ((sender as RadioButton) == LyricTab){
                    TagCtrl.SelectedIndex = 1;
                }
                else if ((sender as RadioButton) == NowPLayingTab)
                {
                    TagCtrl.SelectedIndex = 2;
                }
            }


        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                (sender as Slider).Value += 0.05;
            else if (e.Delta < 0)
                (sender as Slider).Value -= 0.05;
        }

        private void LyricListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TimeSlider_ValueChanged(object sender, TimeSpan time)
        {

        }

        private void TaskBarThumbBtn_Click(object sender, EventArgs e)
        {
            var str = (DataContext as MainViewModel).PlayBackState;
            if (str == "play")
                TaskBarThumbBtn.ImageSource = (BitmapImage)Resources["pause"];
            else
                TaskBarThumbBtn.ImageSource = (BitmapImage)Resources["play"];
        }

        bool isMinTheWin = false;
        public string MinWindowsArrow => isMinTheWin ? "\uf103" : "\uf102";
        Size _originSize = new Size();
        WindowState _originState = WindowState.Normal;
        public bool IsMinTheWin
        {
            get => isMinTheWin;
            set
            {
                isMinTheWin = value;
                if (value == true)
                {
                    MainWin.WindowStyle = WindowStyle.None;
                    MainWin.ResizeMode = ResizeMode.NoResize;
                    _originState = MainWin.WindowState;
                    _originSize.Width = MainWin.RestoreBounds.Width;
                    _originSize.Height = MainWin.RestoreBounds.Height;
                    MainWin.WindowState = WindowState.Normal;
                    MainWin.MaxHeight = 300;
                    MainWin.MaxWidth = 600;
                    MainWin.Width = 600;
                    MainWin.Height = 300;
                    MainWin.Topmost = true;
                    TabBorder.Visibility = Visibility.Collapsed;
                    MainMenu.Visibility = Visibility.Collapsed;
                    AlbumImage.Visibility = Visibility.Collapsed;
                    BackImage2.Visibility = Visibility.Visible;
                    BackImage.Visibility = Visibility.Visible;
                    if ((DataContext as MainViewModel).HasImage)
                        BackImageBack.Background = (SolidColorBrush)Resources["BackGroundColor"];
                    else
                        BackImageBack.Background = null;
                    MinControlBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    MainWin.WindowStyle = WindowStyle.SingleBorderWindow;
                    MainWin.ResizeMode = ResizeMode.CanResize;
                    MainWin.MaxHeight = double.PositiveInfinity;
                    MainWin.MaxWidth = double.PositiveInfinity;
                    MainWin.Height = _originSize.Height;
                    MainWin.Width = _originSize.Width;
                    MainWin.WindowState = _originState;
                    MainWin.Topmost = false;
                    TabBorder.Visibility = Visibility.Visible;
                    MainMenu.Visibility = Visibility.Visible;
                    AlbumImage.Visibility = Visibility.Visible;
                    BackImage2.Visibility = Visibility.Collapsed;
                    BackImage.Visibility = Visibility.Collapsed;
                    MinControlBorder.Visibility = Visibility.Collapsed;
                }
                NotifyPropertyChanged(nameof(MinWindowsArrow));
            }
        }

        private void MainWin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (!isMinTheWin) return; DragMove(); }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainWin_LocationChanged(object sender, EventArgs e)
        {
            var win = sender as MainWindow;
            if (!isMinTheWin)
                return;
            Point fp = new Point(win.Left,win.Top);
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                var result = ScreenBoundaryCollisionDirection(win.RestoreBounds,
                    new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height));
                if (result.Is1stDirection(BaseDirection.Left))
                {
                    fp.X = screen.Bounds.Left;
                }
                if (result.Is1stDirection(BaseDirection.Right))
                {
                    fp.X = screen.Bounds.Right - win.RestoreBounds.Width;
                }
                if (result.Is1stDirection(BaseDirection.Top))
                {
                    fp.Y = screen.Bounds.Top;
                }
                if (result.Is1stDirection(BaseDirection.Bottom))
                {
                    fp.Y = screen.Bounds.Bottom - win.RestoreBounds.Height;
                }
            }
            if (win.Left != fp.X)
                win.Left = fp.X;
            if (win.Top != fp.Y)
                win.Top = fp.Y;
        }
        static private RelativeDirection ScreenBoundaryCollisionDirection(Rect A, Rect Screen)
        {

            RelativeDirection ret = new RelativeDirection(0x00);
            if (Dilate(Screen, stickyWindowPixel).IntersectsWith(A))
            {
                var TopOffset = Math.Abs(A.Top - Screen.Top);
                var LeftOffset = Math.Abs(A.Left - Screen.Left);
                var BottomOffset = Math.Abs(A.Bottom - Screen.Bottom);
                var RightOffset = Math.Abs(A.Right - Screen.Right);
                if (TopOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Top);
                if (LeftOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Left);
                if (BottomOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Bottom);
                if (RightOffset < stickyWindowPixel)
                    ret.Add1stDirection(BaseDirection.Right);
            }
            return ret;
        }
        struct RelativeDirection
        {
            private int Direct;
            /// <summary>
            /// 宣告新的相對方位 由於 C# 對於 byte 型別的計算十分不友善 所以這裡用 int 但是超出範圍的不受理
            /// </summary>
            /// <param name="direct">方向值 基本上範圍為 0x00~0xff 或是 0b0000_0000~0b1111_1111 或是 0~255 </param>
            public RelativeDirection(int direct)
            {
                Direct = direct;
            }
            static public RelativeDirection None => new RelativeDirection(0x00);
            static public RelativeDirection Top => new RelativeDirection(0x80);
            static public RelativeDirection Bottom => new RelativeDirection(0x40);
            static public RelativeDirection Left => new RelativeDirection(0x20);
            static public RelativeDirection Right => new RelativeDirection(0x10);
            static public RelativeDirection TopLeft => new RelativeDirection(0xa0);
            static public RelativeDirection TopRight => new RelativeDirection(0x90);
            static public RelativeDirection BottomLeft => new RelativeDirection(0x60);
            static public RelativeDirection BottomRight => new RelativeDirection(0x50);
            static public bool operator ==(RelativeDirection A, RelativeDirection B) => A.Direct == B.Direct;
            static public bool operator !=(RelativeDirection A, RelativeDirection B) => A.Direct != B.Direct;
            public bool Is1stDirection(BaseDirection bd)
            {
                return (Direct & (int)bd * 0x10) == (int)bd * 0x10;
            }
            public bool Is2ndDirection(BaseDirection bd)
            {
                return (Direct & (int)bd) == (int)bd;
            }
            public RelativeDirection Add1stDirection(BaseDirection bd)
            {
                Direct = Direct + (int)bd * 0x10;
                return this;
            }
            public RelativeDirection Add2ndDirection(BaseDirection bd)
            {
                Direct = Direct + (int)bd;
                return this;
            }
            public RelativeDirection Minus1stDirection(BaseDirection bd)
            {
                Direct = Direct - (int)bd * 0x10;
                return this;
            }
            public RelativeDirection Minus2ndDirection(BaseDirection bd)
            {
                Direct = Direct - (int)bd;
                return this;
            }
            public RelativeDirection Invert()
            {
                int a = 0, b = 0, c = 0, d = 0;
                if ((Direct & 0x80) != (Direct & 0x40))
                    a = (~(Direct & 0xc0)) & 0xc0;
                if ((Direct & 0x20) != (Direct & 0x10))
                    b = (~(Direct & 0x30)) & 0x30;
                if ((Direct & 0x08) != (Direct & 0x04))
                    c = (~(Direct & 0x0c)) & 0x0c;
                if ((Direct & 0x02) != (Direct & 01))
                    d = (~(Direct & 0x03)) & 0x03;
                return new RelativeDirection(a + b + c + d);
            }
            public override string ToString()
            {
                string f = "", a = "";
                if ((Direct & 0x80) == 0x80)
                    f += "Top";
                if ((Direct & 0x40) == 0x40)
                    f += "Bottom";
                if ((Direct & 0x20) == 0x20)
                    f += "Left";
                if ((Direct & 0x10) == 0x10)
                    f += "Right";
                if ((Direct & 0x08) == 0x08)
                    a += "Top";
                if ((Direct & 0x04) == 0x04)
                    a += "Bottom";
                if ((Direct & 0x02) == 0x02)
                    a += "Left";
                if ((Direct & 0x01) == 0x01)
                    a += "Right";
                return f + "-" + a;
            }
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        public enum BaseDirection { None = 0, Top = 8, Bottom = 4, Left = 2, Right = 1 }
        static private Rect Dilate(Rect rect, int pixel)
        {
            rect.X -= pixel;
            rect.Y -= pixel;
            rect.Width += pixel * 2;
            rect.Height += pixel * 2;
            return rect;
        }
        static int stickyWindowPixel = 15;

        private void NowPlayingListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            App.MainWinViewModel.LoadCmd.Execute(((sender as ListView).SelectedItem as MusicItem).Path);
            App.MainWinViewModel.PlayCmd.Execute(null);
        }

        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            for (var tabIndex = TagCtrl.Items.Count; tabIndex >= 0; tabIndex--)
            {
                TagCtrl.SelectedIndex = tabIndex;
                TagCtrl.UpdateLayout();
            }
        }
    }
}
