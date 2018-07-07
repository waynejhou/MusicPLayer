using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using System;
using System.Collections;
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
        private SongEntity NPI => App.PlayerModel.NowPlayingItem;

        public ICommand RemoveItemFromListCmd => new RelayCommand(RemoveSelectedItems, () => PlayingList.Count > 0 && SelectedItems != null && SelectedItems.Count > 0);
        public ICommand RemoveAllItemFromListCmd => new RelayCommand(() =>
        {
            PlayingList?.Clear();
        }, () => true);

        private void RemoveSelectedItems()
        {
            var values = SelectedItems.Cast<SongEntity>().ToList().ConvertAll(x => PlayingList.IndexOf(x)).OrderByDescending(x => x).ToList();
            foreach (int mi in values)
                PlayingList.RemoveAt(mi);
        }

        public PlayingListViewModel()
        {
            if (App.PlayingList == null)
                App.PlayingList = this;
        }


        public ObservableCollection<SongEntity> PlayingList { get; set; } = new ObservableCollection<SongEntity>();

        public void AddEntityToList(SongEntity musicItem)
        {
            PlayingList.Add(musicItem);
        }
        public void AddEntityToList(SongEntity[] musicItems)
        {
            foreach(var mi in musicItems)
            {
                PlayingList.Add(mi);
            }
        }
        public void LoadEntity(SongEntity musicItem)
        {
            if (!IsGetPrev)
            {
                PlayingHistory.Push(NPI);
                IsGetPrev = false;
            }
            if (!PlayingList.Contains(musicItem))
                AddEntityToList(musicItem);
            PM.LoadFromMusicItem(musicItem);
        }



        public Stack<SongEntity> PlayingHistory { get; set; } = new Stack<SongEntity>();
        public NextOneMode NextModeType { get; set; } = NextOneMode.RepeatList;
        public bool CanGetLast => ((NextModeType == NextOneMode.Random) && PlayingHistory.Count > 0) || ((NextModeType == NextOneMode.RepeatList) && PlayingList.Count > 1 || (NextModeType == NextOneMode.RepeatOne));
        public bool CanGetNext => ((NextModeType == NextOneMode.Random) && PlayingHistory.Count > 0) || (NextModeType == NextOneMode.RepeatList) && PlayingList.Count > 1 || (NextModeType == NextOneMode.RepeatOne);
        Random _rnd = new Random();
        bool IsGetPrev = false;
        public IList _selectItems;
        public IList SelectedItems { get=>_selectItems; set
            {
                _selectItems = value;
                NotifyPropertyChanged(nameof(PlayingList));
            }
        }

        int NowPlayIndex => PlayingList.Contains(NPI) ? PlayingList.IndexOf(NPI) : -1;
        public SongEntity GetNextMusic()
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
        public SongEntity GetPrevMusic()
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
