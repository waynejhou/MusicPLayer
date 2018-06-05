using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MusicPLayerV2.ViewModels
{
    public class PlayingListViewModel : ViewModelBase
    {
        private ResourceDictionary R => App.Current.Resources;
        private MusicPlayer PM => App.PlayerModel;
        private MusicItem NPI => App.PlayerModel.NowPlayingItem;

        public ICommand LoadFileCmd => new RelayCommand<string>(OnLoadFile, (string s) => true);
        public ICommand AddFilesCmd => new RelayCommand<string[]>((paths) => AddToList(paths), (paths) => true);
        public ICommand AddFileCmd => new RelayCommand<string>((path) => AddToList(path), (path) => true);

        public PlayingListViewModel()
        {
            App.PlayingList = this;
        }

        public ObservableCollection<MusicItem> PlayingList { get; set; } = new ObservableCollection<MusicItem>();

        public void AddToList(string path)
        {
            PlayingList.Add(MusicItem.CreatFromFile(path, false));
        }
        public void AddToList(string[] paths)
        {
            foreach (var s in paths)
            {
                PlayingList.Add(MusicItem.CreatFromFile(s, false));
            }
        }
        public void AddToList(MusicItem musicItem)
        {
            PlayingList.Add(musicItem);
        }
        public void AddToList(MusicItem[] musicItems)
        {
            foreach(var mi in musicItems)
            {
                PlayingList.Add(mi);
            }
        }
        private void OnLoadFile(string path)
        {
            if (!MusicPlayer.SupportCheck(path, (string)R["Filter_AudioFile"]))
                return;
            MusicItem newone = MusicItem.CreatFromFile(path);
            AddToList(newone);
            PM.LoadFromMusicItem(newone);
        }
        public void Load(MusicItem musicItem)
        {
            if (!IsGetPrev)
            {
                PlayingHistory.Push(NPI);
                IsGetPrev = false;
            }
            if (!PlayingList.Contains(musicItem))
                AddToList(musicItem);
            PM.LoadFromMusicItem(musicItem);
        }

        public Stack<MusicItem> PlayingHistory { get; set; } = new Stack<MusicItem>();
        public NextOneMode NextModeType { get; set; } = NextOneMode.RepeatList;
        public bool CanGetLast => ((NextModeType == NextOneMode.Random) && PlayingHistory.Count > 0) || ((NextModeType == NextOneMode.RepeatList) && PlayingList.Count > 1 || (NextModeType == NextOneMode.RepeatOne));
        public bool CanGetNext => ((NextModeType == NextOneMode.Random) && PlayingHistory.Count > 0) || (NextModeType == NextOneMode.RepeatList) || (NextModeType == NextOneMode.RepeatOne);
        Random _rnd = new Random();
        bool IsGetPrev = false;

        int NowPlayIndex => PlayingList.Contains(NPI) ? PlayingList.IndexOf(NPI) : -1;
        public MusicItem GetNextMusic()
        {
            switch (NextModeType)
            {
                case NextOneMode.Random:
                    var r = 0;
                    while (
                        ((r = _rnd.Next(0, PlayingList.Count - 1)) == NowPlayIndex)
                        ) { }
                    return PlayingList[r];
                case NextOneMode.RepeatList:
                    var n = NowPlayIndex + 1;
                    return PlayingList[n >= PlayingList.Count ? 0 : n];
                case NextOneMode.RepeatOne:
                    return PlayingList[NowPlayIndex];
                default:
                    throw new FormatException();
            }
        }
        public MusicItem GetPrevMusic()
        {
            switch (NextModeType)
            {
                case NextOneMode.Random:
                    IsGetPrev = true;
                    return PlayingHistory.Pop();
                case NextOneMode.RepeatList:
                    var n = NowPlayIndex - 1;
                    return PlayingList[n < 0 ? PlayingList.Count - 1 : n];
                case NextOneMode.RepeatOne:
                    return PlayingList[NowPlayIndex];
                default:
                    throw new FormatException();
            }
        }
    }
    public enum NextOneMode { Random, RepeatList, RepeatOne }
}
