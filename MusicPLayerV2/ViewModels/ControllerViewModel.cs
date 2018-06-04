using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using MusicPLayerV2.Views;

namespace MusicPLayerV2.ViewModels
{
    class ControllerViewModel:ViewModelBase
    {
        public ControllerViewModel()
        {
            PM.LoaddedEvent += PM_LoaddedEvent;
            PM.PlaybackStateChangedChangedEvent += PM_PlaybackStateChangedChangedEvent;
            PM.WavePositionChangedEvent += PM_WavePositionChangedEvent;
            PM.StoppedEvent += PM_StoppedEvent;
        }

        private MusicPlayer PM => App.PlayerModel;
        private MusicItem NPI => App.PlayerModel.NowPlayingItem;
        private ResourceDictionary R => App.Current.Resources;

        public string MusicTitle => PM.NowPlayingItem.Title;
        public string MusicArtistAlbum => $"{NPI.Artist}\n{NPI.Album}";
        public string MusicPlayPauseBtnStr =>
            (PM.PlaybackState == CSCore.SoundOut.PlaybackState.Playing) ? (string)R["SymbolCode_Pause"] : (string)R["SymbolCode_Play"];
        public TimeSpan MusicPosition
        {
            get
            {
                if (PM.IsLoadded && PM.PlaybackState == CSCore.SoundOut.PlaybackState.Stopped)
                {
                    /*if (!PM.ManualStop)
                        NextCmd.Execute(null);*/
                    PM.ManualStop = false;
                }
                return PM.Position;
            }
            set => PM.Position = value;
        }
        public double MusicPositionDouble { get => MusicPosition.TotalMilliseconds; set => MusicPosition = TimeSpan.FromMilliseconds(value); }
        public TimeSpan MusicLength => NPI.Length;
        public double MusicLengthDouble => NPI.Length.TotalMilliseconds;
        public double MusicVolume { get => PM.Volume; set { PM.Volume = (float)value; } } 

        public ICommand PlayPauseCmd => new RelayCommand(OnPlayPause, () => PM.IsLoadded);
        public ICommand PlayCmd => new RelayCommand(() => PM.Play(), () => PM.IsLoadded);
        public ICommand PauseCmd => new RelayCommand(() => PM.Pause(), () => PM.IsLoadded);

        public ICommand ChangeMode => new RelayCommand(() => {
            switch ((App.Current.MainWindow as MainWindow).WindowMode)
            {
                case MainWindowMode.Mini:
                    (App.Current.MainWindow as MainWindow).WindowMode = MainWindowMode.Normal;
                    break;
                case MainWindowMode.Normal:
                    (App.Current.MainWindow as MainWindow).WindowMode = MainWindowMode.Mini;
                    break;
                case MainWindowMode.FullScreen:
                    break;
                default:
                    break;
            }

            }, () => true);
        private void OnPlayPause()
        {
            switch (PM.PlaybackState)
            {
                case CSCore.SoundOut.PlaybackState.Stopped:
                    PM.Position = TimeSpan.Zero;
                    PM.Play();
                    break;
                case CSCore.SoundOut.PlaybackState.Playing:
                    PM.Pause();
                    break;
                case CSCore.SoundOut.PlaybackState.Paused:
                    PM.Resume();
                    break;
                default:
                    break;
            }
        }

        private void PM_LoaddedEvent(object sender)
        {
            Console.WriteLine("loadded");
            NotifyPropertyChanged(nameof(MusicTitle));
            NotifyPropertyChanged(nameof(MusicArtistAlbum));
            NotifyPropertyChanged(nameof(MusicLength));
            NotifyPropertyChanged(nameof(MusicLengthDouble));
        }
        private void PM_PlaybackStateChangedChangedEvent(object sender, CSCore.SoundOut.PlaybackState oldValue, CSCore.SoundOut.PlaybackState newValue)
        {
            NotifyPropertyChanged(nameof(MusicPlayPauseBtnStr));
        }
        private void PM_WavePositionChangedEvent(object sender, TimeSpan position)
        {
            NotifyPropertyChanged(nameof(MusicPosition));
            NotifyPropertyChanged(nameof(MusicPositionDouble));
        }
        private void PM_StoppedEvent(object sender)
        {
            NotifyPropertyChanged(nameof(MusicPlayPauseBtnStr));
        }
    }
}
