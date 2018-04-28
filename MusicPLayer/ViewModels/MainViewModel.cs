using MusicPLayer.Models;
using MusicPLayer.Utils;
using MusicPLayer.View;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace MusicPLayer.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        #region 內部變數
        MvvmDialogs.IDialogService DialogService = new MvvmDialogs.DialogService();
        #endregion

        #region 建構式
        public MainViewModel()
        {
            /*PlayerModel.StoppedEvent += (object sender) =>
            {
                Console.WriteLine($"{DateTime.Now}__{PlayerModel.ManualStop}");
                if (!PlayerModel.ManualStop)
                    NextCmd.Execute(null);
                PlayerModel.ManualStop = false;
            };*/
            PlayerModel.WavePositionChangedEvent += (object sender, TimeSpan position) =>
            {
                NotifyPropertyChanged(nameof(MusicPosition));
                NotifyPropertyChanged(nameof(WindowTitle));
            };
            NowPlayingList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
            {
                NotifyAllPropotery();
            };
        }



        #endregion

        #region 屬性

        private MusicPlayer PlayerModel => App.PlayerModel;
        private MusicItem NowPlayingItem => App.PlayerModel.NowPlayingItem;

        /// <summary>
        /// 音樂撥放狀態
        /// </summary>
        public string PlayBackState =>
            (PlayerModel.PlaybackState == CSCore.SoundOut.PlaybackState.Playing) ? "pause" :
            (PlayerModel.PlaybackState == CSCore.SoundOut.PlaybackState.Paused) ? "play" : "play";

        /// <summary>
        /// 音樂長度
        /// </summary>
        public TimeSpan MusicLength => NowPlayingItem.Length;

        /// <summary>
        /// 音樂撥放位置
        /// </summary>
        public TimeSpan MusicPosition
        {
            get
            {
                if (PlayerModel.IsLoadded&&PlayerModel.PlaybackState == CSCore.SoundOut.PlaybackState.Stopped)
                {
                    if (!PlayerModel.ManualStop)
                        NextCmd.Execute(null);
                    PlayerModel.ManualStop = false;
                }
                return PlayerModel.Position;
            }
            set => PlayerModel.Position = value;
        }

        /// <summary>
        /// 音樂標題
        /// </summary>
        public string MusicTitle => PlayerModel.NowPlayingItem.Title;

        /// <summary>
        /// 音樂演出者+專輯
        /// </summary>
        public string MusicArtistAlbum => $"{NowPlayingItem.Artist}\n{NowPlayingItem.Album}";

        /// <summary>
        /// 音樂音量
        /// </summary>
        public float MusicVolume { get => PlayerModel.Volume; set => PlayerModel.Volume = value; }

        /// <summary>
        /// 音樂路徑
        /// </summary>
        public string MusicPath { get => NowPlayingItem.Path; }

        /// <summary>
        /// 音樂圖片
        /// </summary>
        public BitmapImage MusicPicture
        {
            get
            {
                return NowPlayingItem.Picture != null ? NowPlayingItem.Picture : (BitmapImage)App.MainWin.Resources["NoImage"];
            }
        }

        /// <summary>
        /// 現正撥放清單
        /// </summary>
        public ObservableCollection<MusicItem> NowPlayingList => App.NowPlayingList.List;

        /// <summary>
        /// 是否有圖片
        /// </summary>
        public bool HasImage => NowPlayingItem.Picture != null;

        /// <summary>
        /// 下一首
        /// </summary>
        public string NextMusicMode
        {
            get
            {
                switch (App.NowPlayingList.NextModeType)
                {
                    case NextOneMode.Random:
                        return "random";
                    case NextOneMode.RepeatList:
                        return "redo";
                    case NextOneMode.RepeatOne:
                        return "redo1";
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// 視窗標題
        /// </summary>
        public string WindowTitle => PlayerModel.IsLoadded ? $"{NowPlayingItem.Title} - {PlayerModel.PlaybackState}" : "MusicPLayer";

        #endregion

        #region 指令
        public ICommand OpenFileDialogCmd { get { return new RelayCommand<string>(OnOpenFileDialog, (string s) => true); } }
        public ICommand OpenFilesCmd { get { return new RelayCommand<string[]>(OnOpenFiles, (string[] s) => true); } }
        public ICommand AddFilesCmd { get { return new RelayCommand<string[]>(OnAddFiles, (string[] s) => true); } }
        public ICommand AddCmd { get { return new RelayCommand<string>(OnOpenFileDialog, (string s) => true); } }
        public ICommand AddFileCmd { get { return new RelayCommand<string>(OnAddFile, (string s) => true); } }
        public ICommand PlayPauseCmd { get { return new RelayCommand(OnPlayPause, () => PlayerModel.IsLoadded); } }
        public ICommand PlayCmd { get { return new RelayCommand(OnPlay, () => PlayerModel.IsLoadded); } }
        public ICommand LoadCmd { get { return new RelayCommand<string>(OnLoadFile, (string s) => true); } }
        public ICommand ExitCmd { get { return new RelayCommand(OnExitApp, () => true); } }
        public ICommand StopCmd { get { return new RelayCommand(OnStop, () => true); } }
        public ICommand ChangeToCnCmd { get { return new RelayCommand(OnChangeToCn, () => true); } }
        public ICommand ChangeToEnCmd { get { return new RelayCommand(OnChangeToEn, () => true); } }
        public ICommand ChangeNextModeCmd { get { return new RelayCommand(OnChangeNextMode, () => true); } }
        public ICommand NextCmd { get { return new RelayCommand(OnPlayNext, () => App.NowPlayingList.CanGetNext); } }
        public ICommand LastCmd { get { return new RelayCommand(OnPlayLast, () => App.NowPlayingList.CanGetLast); } }
        public ICommand OpenLrcEditorCmd { get { return new RelayCommand(OnOpenLrcEditor, () => true); } }
        public ICommand MinAppCmd { get { return new RelayCommand(OnMinApp, () => true); } }
        #endregion

        #region 私有成員函式
        private void OnPlayPause()
        {
            switch (PlayerModel.PlaybackState)
            {
                case CSCore.SoundOut.PlaybackState.Stopped:
                    PlayerModel.Position = TimeSpan.Zero;
                    PlayerModel.Play();
                    break;
                case CSCore.SoundOut.PlaybackState.Playing:
                    PlayerModel.Pause();
                    break;
                case CSCore.SoundOut.PlaybackState.Paused:
                    PlayerModel.Resume();
                    break;
                default:
                    break;
            }
            NotifyAllPropotery();
        }
        private void OnPlay()
        {
            PlayerModel.Play();
            NotifyAllPropotery();
        }
        private void OnOpenFileDialog(string parameter)
        {
            var settings = new MvvmDialogs.FrameworkDialogs.OpenFile.OpenFileDialogSettings
            {
                Title = (string)(App.Current.Resources["Dialog_OpenAudioFile"]),
                Filter = $"{(App.Current.Resources["Filter_AudioFile"])}|{(App.Current.Resources["Filter_AllFile"])}",
                CheckFileExists = true,
                Multiselect = true,
            };
            if (PlayerModel.IsLoadded)
                settings.InitialDirectory = new FileInfo(NowPlayingItem.Path).Directory.FullName;
            bool ? success = DialogService.ShowOpenFileDialog(this, settings);
            if (success == true)
            {
                if (parameter == "Open")
                {
                    OpenFilesCmd.Execute(settings.FileNames);
                    PlayCmd.Execute(null);
                }
                if (parameter == "Add")
                {
                    AddFilesCmd.Execute(settings.FileNames);
                }
            }
            NotifyAllPropotery();
        }
        private void OnOpenFiles(string[] fileNames)
        {
            for (int i = 0; i < fileNames.Count(); i++)
            {
                if (i == 0)
                {
                    LoadCmd.Execute(fileNames[i]);
                    PlayPauseCmd.Execute(null);
                }
                AddFileCmd.Execute(fileNames[i]);
            }
        }
        private void OnAddFiles(string[] fileNames)
        {
            for (int i = 0; i < fileNames.Count(); i++)
            {
                AddFileCmd.Execute(fileNames[i]);
            }
        }
        private void OnAddFile(string fileName)
        {
            if (!MusicPlayer.SupportCheck(fileName))
                return;
            MusicItem newone;
            if (!NowPlayingList.Contains(newone = MusicItem.CreatFromFile(fileName)))
                NowPlayingList.Add(newone);
        }
        private void OnExitApp()
        {
            System.Windows.Application.Current.MainWindow.Close();
        }
        private void OnMinApp()
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        private void OnStop()
        {
            PlayerModel.Stop();
            PlayerModel.Position = TimeSpan.Zero;
        }
        private void OnChangeToEn()
        {
            App.Settings.Langurage = "en-us";
            App.Current.Resources.Source =
                new Uri($@"Strings\Lang.{App.Settings.Langurage}.xaml", UriKind.Relative);
        }
        private void OnChangeToCn()
        {
            App.Settings.Langurage = "zh-tw";
            App.Current.Resources.Source =
                new Uri($@"Strings\Lang.{App.Settings.Langurage}.xaml", UriKind.Relative);
        }
        private void OnChangeNextMode()
        {
            switch (App.NowPlayingList.NextModeType)
            {
                case NextOneMode.Random:
                    App.NowPlayingList.NextModeType = NextOneMode.RepeatList;
                    break;
                case NextOneMode.RepeatList:
                    App.NowPlayingList.NextModeType = NextOneMode.RepeatOne;
                    break;
                case NextOneMode.RepeatOne:
                    App.NowPlayingList.NextModeType = NextOneMode.Random;
                    break;
                default:
                    break;
            }
            NotifyAllPropotery();
        }
        private void OnPlayNext()
        {
            LoadCmd.Execute(App.NowPlayingList.GetNextMusic());
            PlayCmd.Execute(null);
        }
        private void OnPlayLast()
        {
            LoadCmd.Execute(App.NowPlayingList.GetLastMusic());
            PlayCmd.Execute(null);
        }
        private void OnLoadFile(string fileName)
        {
            if (!MusicPlayer.SupportCheck(fileName))
                return;
            PlayerModel.Load(fileName);
            App.MainWin.LyricP.FilePath = fileName.Replace(new FileInfo(fileName).Extension, ".lrc");
            NotifyAllPropotery();
        }
        private void OnOpenLrcEditor()
        {
            new LrcEditorWindow().Show();
        }
        #endregion




        #region 成員函式


        private void NotifyAllPropotery()
        {
            NotifyPropertyChanged(nameof(PlayBackState));
            NotifyPropertyChanged(nameof(MusicLength));
            NotifyPropertyChanged(nameof(MusicTitle));
            NotifyPropertyChanged(nameof(MusicArtistAlbum));
            NotifyPropertyChanged(nameof(MusicPath));
            NotifyPropertyChanged(nameof(MusicPicture));
            NotifyPropertyChanged(nameof(NextMusicMode));
            for (int i = 0; i < NowPlayingList.Count; i++)
            {
                if (NowPlayingList[i].Path == NowPlayingItem.Path)
                {
                    App.NowPlayingList.NowPlayIndex = i;
                    NowPlayingList[i].IsNowPlaying = true;
                }
                else
                    NowPlayingList[i].IsNowPlaying = false;
            }
            //NotifyPropertyChanged(nameof(NowPlayingList));
        }
        #endregion
    }
}
