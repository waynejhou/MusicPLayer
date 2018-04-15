using MusicPLayer.Models;
using MusicPLayer.ViewModels;
using System;
using System.Windows;

namespace MusicPLayer
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        #region 內部變數
        static MainWindow _mainWin;
        static MainViewModel _mainWinViewModel;
        static SettingManager _settingManager = SettingManager.LoadOrNew();
        static MusicPlayer _playerModel = new MusicPlayer();
        static MusicList _musicList = new MusicList();
        #endregion

        #region 屬性
        public static SettingManager Settings => _settingManager;
        public static MusicPlayer PlayerModel => _playerModel;
        public static MainWindow MainWin => _mainWin;
        public static MusicList NowPlayingList => _musicList;
        internal static MainViewModel MainWinViewModel => _mainWinViewModel;
        #endregion

        #region 成員函式
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _mainWin = new MainWindow();
            var context = (_mainWinViewModel = new MainViewModel());
            if (e.Args.Length > 0)
            {
                MainWinViewModel.OpenFileCmd.Execute(e.Args);
                MainWinViewModel.PlayCmd.Execute(null);
            }
            _mainWin.DataContext = context;
            _mainWin.Language = System.Windows.Markup.XmlLanguage.GetLanguage(Settings.Langurage);
            _mainWin.Show();
        }
        #endregion
    }
}
