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

namespace MusicPLayerV2.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private PlayingListViewModel L => App.PlayingList;
        private MusicPlayer PM => App.PlayerModel;
        public LibraryViewModel()
        {
            FeaturedDatabase();
        }

        public IEnumerable<string> StyleList => Enum.GetValues(typeof(LibraryStyle)).Cast<LibraryStyle>().Select(x=>x.ToString().Replace('_',' '));
        public int SelectedStyleIndex { get; set; } = 0;
        public LibraryStyle Style => Enum.GetValues(typeof(LibraryStyle)).Cast<LibraryStyle>().ElementAt(SelectedStyleIndex);

        public void ScanDirectory()
        {
            var LoadingFileVM = new LoadingViewModel<IEnumerable<LibraryEntity>>()
            {
                Min = 0,
                Max = 100,
                Title = "Loading",
                Value = 0
            };
            LoadingFileVM.DoWork += (bgw, vm, libs, e) =>
            {
                Dictionary<int, IEnumerable<string>> dirs = new Dictionary<int, IEnumerable<string>>();
                foreach (var dir in libs)
                {
                    dirs.Add(dir.Id, Directory.EnumerateFiles(dir.Path, "*",
                        dir.IsScanAllSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                        ).Where(x => MusicDatabase.CheckFileSupported(new FileInfo(x).Extension)));
                }
                int count = 0;
                int allfile = dirs.Select(x => x.Value).Select(x => x.Count()).Sum();
                foreach (var dir in dirs)
                {
                    foreach (var file in dir.Value)
                    {
                        Console.WriteLine($"Scanning {dir.Key} {file}");
                        var entity = MusicDatabase.CreateSongEntity(file);
                        if (MusicDatabase.LibrarySongColle.FindOne(x => x.Name == dir.Key + "-" + entity.Id) == null)
                            MusicDatabase.LibrarySongColle.Insert(new LibrarySongRelationship()
                            {
                                Name = dir.Key + "-" + entity.Id,
                                LibraryId = dir.Key,
                                SongId = entity.Id
                            });
                        bgw.ReportProgress((int)(count / (double)(allfile) * 100d), file);
                        count += 1;
                    }
                }
            };
            LoadingFileVM.RunWorkerCompleted += (bgw, vm, result, e) =>
            {
                vm.Value = vm.Max;
                FeaturedDatabase();
                NotifyPropertyChanged(nameof(AlbumList));
                NotifyPropertyChanged(nameof(GenreList));
            };
            LoadingFileVM.RunWorkerAsync(MusicDatabase.LibraryColle.FindAll(), x => true);
        }

        Dictionary<int, IEnumerable<int>> _GenreAlbumMap;
        Dictionary<int, IEnumerable<int>> _AlbumSongMap;

        public void FeaturedDatabase()
        {
            var allLibSong = MusicDatabase.LibrarySongColle.FindAll().Select(x => x.Song).ToList();
            _GenreAlbumMap =
                allLibSong
                .GroupBy(x => x.GenreId)
                .ToDictionary(
                    group => group.Key,
                    members => members
                        .OrderByDescending(y => y.Year).Select(y => y.AlbumId).Distinct()
                );
            _AlbumSongMap =
                allLibSong
                .GroupBy(x => x.AlbumId)
                .ToDictionary(
                    group => group.Key,
                    members => members
                        .OrderBy(x => x.Track).Select(y => y.Id)
                );
        }


        public IEnumerable<GenreEntity> GenreList => _GenreAlbumMap.Keys.Select(x=>MusicDatabase.GenreColle.FindById(x));
        int _SelectedGenreIndex = 0;
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
        public GenreEntity SelectedGenre => GenreList.ElementAt(SelectedGenreIndex == -1 ? 0 : SelectedGenreIndex);

        public IEnumerable<AlbumEntity> AlbumList => _GenreAlbumMap[SelectedGenre.Id].Select(x=>MusicDatabase.AlbumColle.FindById(x));

        public enum LibraryStyle { Album_Grid, Album_List, Album_CoverFlow, Artist_List }

        public ICommand PlayCmd => new RelayCommand<int?>((albumId) =>
        {
            L.PlayingList.Clear();
            L.AddEntityToList(_AlbumSongMap[albumId.Value].Select(x => MusicDatabase.SongColle.FindById(x)).ToArray());
            PM.LoadFromMusicItem(L.PlayingList[0]);
            PM.Play();
        }, (x) => true);
        public ICommand AddCmd => new RelayCommand<int?>((albumId) =>
        {
            L.AddEntityToList(_AlbumSongMap[albumId.Value].Select(x => MusicDatabase.SongColle.FindById(x)).ToArray());
        }, (x) => true);


    }


}
