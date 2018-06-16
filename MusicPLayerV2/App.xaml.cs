using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.ComponentModel;
using MusicPLayerV2.ViewModels;
using MusicPLayerV2.Views;
using MusicPLayerV2.Models;
using Microsoft.Shell;
using System.Windows.Markup;

namespace MusicPLayerV2
{
    public partial class App : Application, ISingleInstanceApp
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static MainWindow MainWin { get; set; }
        public static SettingsViewModel Settings { get; set; }
        public static MusicPlayer PlayerModel { get; set; } = new MusicPlayer();
        public static ControllerViewModel Controller { get; set; } = new ControllerViewModel();
        public static PlayingListViewModel PlayingList { get; set; } = new PlayingListViewModel();
        public static MainViewModel MainModel { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Log.Info("Application Startup");

            // For catching Global uncaught exception
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionOccured);

            Log.Info("Starting App");
            LogMachineDetails();
            MainWin = new MainWindow();
            App.MainWin.Language = XmlLanguage.GetLanguage("zh-tw");
            Settings = SettingsViewModel.LoadOrNew();
            Settings.SaveSettingAsXml();
            MainWin.Show();

            if (e.Args.Length > 0)
            {
                MainModel.OpenFilesCmd.Execute(e.Args);
                Controller.PlayCmd.Execute(null);
            }
        }

        static void UnhandledExceptionOccured(object sender, UnhandledExceptionEventArgs args)
        {
            // Here change path to the log.txt file
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                                                    + "\\wayne\\MusicPLayerV2\\log.txt";

            // Show a message before closing application
            var dialogService = new MvvmDialogs.DialogService();
            dialogService.ShowMessageBox((INotifyPropertyChanged)(MainWin.DataContext),
                "Oops, something went wrong and the application must close. Please find a " +
                "report on the issue at: " + path + Environment.NewLine +
                "If the problem persist, please contact wayne.",
                "Unhandled Error",
                MessageBoxButton.OK);

            Exception e = (Exception)args.ExceptionObject;
            Log.Fatal("Application has crashed", e);
        }

        private void LogMachineDetails()
        {
            var computer = new Microsoft.VisualBasic.Devices.ComputerInfo();

            string text = "OS: " + computer.OSPlatform + " v" + computer.OSVersion + Environment.NewLine +
                          computer.OSFullName + Environment.NewLine +
                          "RAM: " + computer.TotalPhysicalMemory.ToString() + Environment.NewLine +
                          "Language: " + computer.InstalledUICulture.EnglishName;
            Log.Info(text);
        }

        private const string Unique = "MusicPLayerV2";
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

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            args.RemoveAt(0);
            if (args.Count > 0)
            {
                string[] a = new string[args.Count];
                args.CopyTo(a, 0);
                MainModel.OpenFilesCmd.Execute(a);
                Controller.PlayCmd.Execute(null);
            }
            return true;
        }
    }
}
