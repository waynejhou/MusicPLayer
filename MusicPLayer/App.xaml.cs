using MusicPLayer.Models;
using MusicPLayer.ViewModels;
using System;
using System.Windows;
using Microsoft.Shell;
using System.Collections.Generic;
using System.Linq;

namespace MusicPLayer
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application ,ISingleInstanceApp
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

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            args.RemoveAt(0);
            if (args.Count > 0)
            {
                string[] a = new string[args.Count];
                args.CopyTo(a,0);
                MainWinViewModel.OpenFilesCmd.Execute(a);
                MainWinViewModel.PlayCmd.Execute(null);
            }
            return true;
        }
        #endregion

        #region 成員函式
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _mainWin = new MainWindow();
            if (e.Args.Length > 0)
            {
                MainWinViewModel.OpenFilesCmd.Execute(e.Args);
                MainWinViewModel.PlayCmd.Execute(null);
            }
            _mainWinViewModel = (MainWin.DataContext as MainViewModel);
            _mainWin.Language = System.Windows.Markup.XmlLanguage.GetLanguage(Settings.Langurage);
            _mainWin.Show();
        }

        private const string Unique = "My_Unique_Application_String";
        [STAThread]
        private static void Main(string[] args)
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
        #endregion
    }
}
