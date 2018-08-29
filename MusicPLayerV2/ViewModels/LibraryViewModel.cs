using LiteDB;
using MusicPLayerV2.Models;
using MusicPLayerV2.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static MusicPLayerV2.Utils.MusicDatabase;

namespace MusicPLayerV2.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private PlayingListViewModel L => App.PlayingList;
        private MusicPlayer PM => App.PlayerModel;
        private GenreEntity AllGenre = new GenreEntity() { Id = -1, Name = "All Genre" };

        public LibraryViewModel()
        {
            ResetGenreList();
        }

        public IEnumerable<string> StyleList => Enum.GetValues(typeof(LibraryStyle)).Cast<LibraryStyle>().Select(x => x.ToString().Replace('_', ' '));
        public LibraryStyle Style { get; private set; } = LibraryStyle.Album_Grid;
        public int SelectedStyle
        {
            get => (int)Style;
            set
            {
                Console.WriteLine(value);
                Style = (LibraryStyle)value;
                NotifyPropertyChanged(nameof(SelectedStyle));
                NotifyPropertyChanged(nameof(TabIndex));
            }
        }
        public int TabIndex
        {
            get => (int)Style; set
            {
                Console.WriteLine(value);
                Style = (LibraryStyle)value;
                NotifyPropertyChanged(nameof(SelectedStyle));
                NotifyPropertyChanged(nameof(TabIndex));
            }
        }


        public IEnumerable<GenreEntity> GenreList { get; set; }
        public void ResetGenreList()
        {
            var list = new List<GenreEntity>() { AllGenre };
            list.AddRange(LibraryGenreColle.FindAll().Select(x => x.Genre).OrderBy(x=>x.Name));
            GenreList = list;
            if (GenreList.Count() > 2)
                SelectedGenreIndex = 1;
            else
                SelectedGenreIndex = 0;
            NotifyPropertyChanged(nameof(SelectedGenreIndex));
            NotifyPropertyChanged(nameof(GenreList));
        }
        int _SelectedGenreIndex = 1;
        public int SelectedGenreIndex
        {
            get
            {
                return _SelectedGenreIndex;
            }
            set
            {
                _SelectedGenreIndex = value;
                NotifyPropertyChanged(nameof(AlbumList));
            }
        }
        public GenreEntity SelectedGenre
        {
            get
            {
                if (GenreList.Count() > 2)
                    return GenreList.ElementAt(SelectedGenreIndex);
                else
                {
                    if(SelectedGenreIndex==-1)
                        return GenreList.ElementAt(0);
                    else
                        return GenreList.ElementAt(SelectedGenreIndex);
                }

            }
        }


        public IEnumerable<AlbumEntity> AlbumList
        {
            get
            {
                var list = GenreAlbumColle.Find(x => SelectedGenre.Id == -1 || x.GenreId == SelectedGenre.Id).Select(x => x.Album);
                return list;
            }
        }
        

        public IEnumerable<SongEntity> SongList => LibrarySongColle.FindAll().Select(x => x.Song);

        public enum LibraryStyle { Album_Grid, Album_List/*, Album_CoverFlow, Artist_List*/ }

        public ICommand PlayCmd => new RelayCommand<int?>((albumId) =>
        {
            L.PlayingList.Clear();
            L.AddEntityToList(AlbumSongColle.Find(x => x.AlbumId == albumId).Select(x => x.Song).OrderBy(x=>x.Track).ToArray());
            PM.LoadFromMusicItem(L.PlayingList[0]);
            PM.Play();
        }, (x) => true);
        public ICommand AddCmd => new RelayCommand<int?>((albumId) =>
        {
            L.AddEntityToList(AlbumSongColle.Find(x => x.AlbumId == albumId).Select(x => x.Song).OrderBy(x => x.Track).ToArray());
        }, (x) => true);


    }


}
