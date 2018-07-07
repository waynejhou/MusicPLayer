using MvvmDialogs;
using log4net;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Input;
using System.Xml.Linq;
using MusicPLayerV2.Views;
using MusicPLayerV2.Utils;
using MusicPLayerV2.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.ComponentModel;

namespace MusicPLayerV2.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Parameters
        private readonly IDialogService DialogService;
        private  ResourceDictionary R => App.Current.Resources;
        private MusicPlayer PM => App.PlayerModel;
        private SongEntity NPI => App.PlayerModel.NowPlayingItem;
        private ControllerViewModel C => App.Controller;
        private PlayingListViewModel L => App.PlayingList;

        /// <summary>
        /// Title of the application, as displayed in the top bar of the window
        /// </summary>
        public string Title => NPI == null ? "MusicPLayer" : $"{NPI.Title} - MusicPLayer";
        #endregion

        #region Constructors
        public MainViewModel()
        {
            App.MainModel = this;
            // DialogService is used to handle dialogs
            this.DialogService = new MvvmDialogs.DialogService();
            PM.LoaddedEvent += PM_LoaddedEvent;
            PM.WavePositionChangedEvent += PM_WavePositionChangedEvent;
            loadFileToEntities.DoWork += LoadFileToEntities_DoWork;
            loadFileToEntities.ProgressChanged += LoadFileToEntities_ProgressChanged;
            loadFileToEntities.RunWorkerCompleted += LoadFileToEntities_RunWorkerCompleted;
        }
        #endregion

        #region Methods

        #endregion

        #region Property
        public ImageSource MusicPicture =>  NPI==null? (BitmapImage)R["NoImage"] : NPI.Cover ?? (BitmapImage)R["NoImage"];

        public string LRCPath => (PM.IsLoadded) ? NPI.Path.Replace(new FileInfo(NPI.Path).Extension, ".lrc") : "";

        public TimeSpan MusicPosition { get => PM.Position; set => PM.Position = value; }
        #endregion
        #region Commands
        public ICommand OpenFileDialogCmd => new RelayCommand<string>(OnOpenFileDialog, (s) => true);
        public ICommand ShowAboutDialogCmd => new RelayCommand(OnShowAboutDialog, () => true);
        public ICommand ShowSettingDialogCmd => new RelayCommand(OnShowSettingDialog, () => true);
        public ICommand ExitCmd => new RelayCommand(OnExitApp, () => true);


        private void OnOpenFileDialog(string arg)
        {
            var settings = new OpenFileDialogSettings
            {
                Title = arg,
                Filter = $"{R["Filter_AudioFile"]}|{R["Filter_AllFile"]}",
                CheckFileExists = true,
                Multiselect = true,
            };
            if (PM.IsLoadded)
                settings.InitialDirectory = new FileInfo(PM.NowPlayingItem.Path).Directory.FullName;
            bool? success = DialogService.ShowOpenFileDialog(this, settings);
            if (success == true)
            {
                if (arg == "Open")
                {
                    LoadingFiles(new LoadFileToEntitiesArgsAndResult
                    {
                        Files = settings.FileNames,
                        PlayFirstWhenComplete = true
                    });
                }
                if (arg == "Add")
                {
                    LoadingFiles(new LoadFileToEntitiesArgsAndResult
                    {
                        Files = settings.FileNames,
                        PlayFirstWhenComplete = false
                    });
                }
                Log.Info("Opening file: " + settings.FileName);
            }
        }

        public class LoadFileToEntitiesArgsAndResult
        {
            public string[] Files { get; set; }
            public SongEntity[] Entities { get; set; }
            public bool? PlayFirstWhenComplete { get; set; }
        }
        private LoadingViewModel LoadingVM = new LoadingViewModel()
        {
            Min = 0, Max = 100, Title = "Loading", Value = 0
        };
        public void LoadingFiles(LoadFileToEntitiesArgsAndResult args)
        {
            loadFileToEntities.RunWorkerAsync(args);
            if (args.Files.Length > 10)
            {
                LoadingVM.Value = 0;
                DialogService.ShowDialog<LoadingWindow>(this, LoadingVM);
            }
        }
        private readonly BackgroundWorker loadFileToEntities = new BackgroundWorker() {
            WorkerReportsProgress = true
        };
        private void LoadFileToEntities_DoWork(object sender, DoWorkEventArgs e)
        {
            var bg = (sender as BackgroundWorker);
            var argsAndresult = (e.Argument as LoadFileToEntitiesArgsAndResult);
            var files = argsAndresult.Files;
            var entities = argsAndresult.Entities = new SongEntity[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                entities[i] = SongEntity.CreateFromFile(files[i]);
                bg.ReportProgress((int)(i / (double)(files.Length) * 100d),files[i]);
            }
            e.Result = argsAndresult;
        }
        private void LoadFileToEntities_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LoadingVM.Message = e.UserState as string;
            LoadingVM.Value = e.ProgressPercentage;
        }
        private void LoadFileToEntities_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadingVM.Value = LoadingVM.Max;
            var argsAndresult = (e.Result as LoadFileToEntitiesArgsAndResult);
            var entities = argsAndresult.Entities;
            L.AddEntityToList(entities);
            if (argsAndresult.PlayFirstWhenComplete.Value == true)
            {
                L.LoadEntity(entities[0]);
                C.PlayCmd.Execute(null);
            }
        }


        private void OnShowAboutDialog()
        {
            Log.Info("Opening About dialog");
            AboutViewModel dialog = new AboutViewModel();
            var result = DialogService.ShowDialog<About>(this, dialog);
        }
        private void OnShowSettingDialog()
        {
            DialogService.Show<SettingWindow>(this, App.Settings);
        }

        private void OnExitApp()
        {
            System.Windows.Application.Current.MainWindow.Close();
        }
        #endregion

        #region Events

        private void PM_LoaddedEvent(object sender)
        {
            NotifyPropertyChanged(nameof(LRCPath));
            NotifyPropertyChanged(nameof(MusicPicture));
            App.MainWin.Dispatcher.Invoke(()=> { App.MainWin.AlbumImage_SourceUpdated(); });
            NotifyPropertyChanged(nameof(Title));
        }


        private void PM_WavePositionChangedEvent(object sender, TimeSpan position)
        {
            NotifyPropertyChanged(nameof(MusicPosition));
        }
        #endregion
    }
}
