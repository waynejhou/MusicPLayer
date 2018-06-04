using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Reflection;
using log4net;
using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using System.Windows.Shell;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicPLayerV2.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ResourceDictionary R => App.Current.Resources;
        private MusicPlayer PM => App.PlayerModel;
        private MusicItem NPI => App.PlayerModel.NowPlayingItem;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainView_Closing;
        }

        private void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /*
                if (((MainViewModel)(this.DataContext)).Data.IsModified)
                if (!((MainViewModel)(this.DataContext)).PromptSaveBeforeExit())
                {
                    e.Cancel = true;
                    return;
                }
            */
            byte[,,] a = new byte[5, 5, 5];
            a[1, 2, 3] = 5;
            App.PlayerModel.Dispose();
            Log.Info("Closing App");
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (TagCtrl != null)
            {
                if ((sender as RadioButton) == AlbumArtTab)
                {
                    TagCtrl.SelectedIndex = 0;
                }
                else if ((sender as RadioButton) == LyricTab)
                {
                    LyricP.ResetLinesHeight();
                    TagCtrl.SelectedIndex = 1;
                }
                else if ((sender as RadioButton) == NowPLayingTab)
                {
                    TagCtrl.SelectedIndex = 2;
                }
            }
        }


        public MainWindowMode WindowMode
        {
            get { return (MainWindowMode)GetValue(WindowModeProperty); }
            set { SetValue(WindowModeProperty, value);NotifyPropertyChanged(nameof(WindowMode)); }
        }

        // Using a DependencyProperty as the backing store for WindowMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowModeProperty =
            DependencyProperty.Register("WindowMode", typeof(MainWindowMode), typeof(MainWindow),
                new FrameworkPropertyMetadata(MainWindowMode.Normal,
                    (DependencyObject obj, DependencyPropertyChangedEventArgs args)=>
                    {
                        (obj as MainWindow).OnSetWindowMode((MainWindowMode)args.NewValue);
                        (obj as MainWindow).NotifyPropertyChanged(nameof(WindowMode));
                    }));
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        Size TSize = new Size();
        WindowState TState = WindowState.Normal;
        private void OnSetWindowMode(MainWindowMode value)
        {
            switch (value)
            {
                case MainWindowMode.Mini:
                    WindowChrome.GetWindowChrome(this).GlassFrameThickness = new Thickness(0, 0, 0, 0);
                    WindowChrome.GetWindowChrome(this).CaptionHeight = 0;
                    (Template.FindName("titleText", this) as TextBlock).Visibility = Visibility.Collapsed;
                    MainMenu.Visibility = Visibility.Collapsed;
                    TabBtnBorder.Visibility = Visibility.Collapsed;
                    ResizeMode = ResizeMode.NoResize;
                    TSize = RestoreBounds.Size;
                    TState = WindowState;
                    WindowState = WindowState.Normal;
                    Height = 400;
                    Width = 300;
                    BackImage.Visibility = Visibility.Visible;
                    BackImage2.Visibility = Visibility.Visible;
                    AlbumImage.Visibility = Visibility.Collapsed;
                    Topmost = true;
                    break;
                case MainWindowMode.Normal:
                    WindowChrome.GetWindowChrome(this).GlassFrameThickness = new Thickness(3, 30, 3, 3);
                    WindowChrome.GetWindowChrome(this).CaptionHeight = 30;
                    (Template.FindName("titleText", this) as TextBlock).Visibility = Visibility.Visible;
                    MainMenu.Visibility = Visibility.Visible;
                    TabBtnBorder.Visibility = Visibility.Visible;
                    ResizeMode = ResizeMode.CanResize;
                    Height = TSize.Height;
                    Width = TSize.Width;
                    WindowState = TState;
                    BackImage.Visibility = Visibility.Collapsed;
                    BackImage2.Visibility = Visibility.Collapsed;
                    AlbumImage.Visibility = Visibility.Visible;
                    Topmost = false;
                    break;
                case MainWindowMode.FullScreen:
                    break;
                default:
                    break;
            }
        }

        private void StateWinBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    break;
                case WindowState.Minimized:
                    break;
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    break;
                default:
                    break;
            }
        }

        private void CloseWinBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MiniWinBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MainWin_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    (Template.FindName("StateWinBtn", this) as Button).Content = (string)R["SymbolCode_ChromeMaximize"];
                    break;
                case WindowState.Minimized:
                    break;
                case WindowState.Maximized:
                    (Template.FindName("StateWinBtn", this) as Button).Content = (string)R["SymbolCode_ChromeRestore"];
                    break;
                default:
                    break;
            }

        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        internal void AlbumImage_SourceUpdated()
        {
            Size ss = new Size((AlbumImage.Source as BitmapImage).Width, (AlbumImage.Source as BitmapImage).Height);
            if (ss.Width> AlbumArtBorder.ActualWidth|| ss.Height> AlbumArtBorder.ActualHeight)
            {
                AlbumImage.Stretch = Stretch.Uniform;
            }
            else
            {
                AlbumImage.Stretch = Stretch.None;
            }
        }

        private void MainWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlbumImage_SourceUpdated();
        }
    }
    public enum MainWindowMode { Mini, Normal, FullScreen };
    partial class MainWindow
    {
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            var win = sender as MainWindow;
            if (WindowMode != MainWindowMode.Mini)
                return;
            Point fp = new Point(win.Left, win.Top);
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
    }
}
