using MusicPLayerV2.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MusicPLayerV2.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        public LibraryViewModel()
        {
        }

        public IEnumerable<LibraryStyle> StyleList => Enum.GetValues(typeof(LibraryStyle)).Cast<LibraryStyle>();
        public LibraryStyle Style { get; set; } = LibraryStyle.Grid;
        public ObservableCollection<SongEntity> SongLibrary { get; set; } = new ObservableCollection<SongEntity>();
        public ObservableCollection<AlbumEntity> AlbumLibrary { get; set; } = new ObservableCollection<AlbumEntity>();
        public ObservableCollection<PerformerEntity> PerformerLibrary { get; set; } = new ObservableCollection<PerformerEntity>();
        public ObservableCollection<GenreEntity> GenreLibrary { get; set; } = new ObservableCollection<GenreEntity>();
        public int SongLibraryCount => SongLibrary.Count;

        public List<ScannedDirectoryInfo> ScannedDirectoryInfo = new List<ScannedDirectoryInfo>() { new ScannedDirectoryInfo(App.MyMusic)};
        public class LoadingLibraryArgsAndResult
        {
            public ScannedDirectoryInfo[] Directories { get; set; }
            public List<SongEntity> Songs;
            public List<AlbumEntity> Albums;
            public List<PerformerEntity> Performers;
            public List<GenreEntity> Genres;
        }
        public void ScanDirectory()
        {
            LoadingViewModel<LoadingLibraryArgsAndResult> loadingViewModel = new LoadingViewModel<LoadingLibraryArgsAndResult>()
            {
                Title = "Loading Library",
                Max = 100,
                Min = 0,
                Value = 0
            };
            loadingViewModel.DoWork += (bgw, vm, args, e) =>
            {
                var dirs = args.Directories;
                var songs = args.Songs = new List<SongEntity>();
                var albums = args.Albums = new List<AlbumEntity>();
                var performers = args.Performers = new List<PerformerEntity>();
                var genres = args.Genres = new List<GenreEntity>();
                Console.WriteLine($"doWork");
                for (int i = 0; i < dirs.Length; i++)
                {
                    Console.WriteLine($"doWork_dirs[{i}]");
                    var di = dirs[i];
                    SearchOption option = SearchOption.AllDirectories;
                    if (!di.IsScanAllSubDirectories)
                        option = SearchOption.TopDirectoryOnly;
                    Console.WriteLine($"doWork_dirs[{i}]_option{option}");
                    var maxCount = di.DirectoryInfo.GetFiles("*",option).Length;
                    Console.WriteLine($"doWork_dirs[{i}]_maxCount{maxCount}");
                    if (maxCount <= 0)
                        continue;
                    var count = 0;
                    foreach (var f in di.DirectoryInfo.EnumerateFiles("*", option))
                    {
                        Console.WriteLine($"doWork_dirs[{i}]_files[{f}]");
                        var progress= (count++) / (double)maxCount * (100 / (dirs.Length)) * (i + 1);
                        Console.WriteLine($"doWork_dirs[{i}]progress[{progress}]");
                        bgw.ReportProgress((int)progress,f.FullName);
                        if (!Models.MusicPlayer.SupportCheck(f.FullName, (string)App.Current.Resources["Filter_AudioFile"]))
                            continue;
                        SongEntity song = MusicDatabase.CreateSongEntity(f.FullName);
                        songs.Add(song);
                        if (!albums.Contains(song.AlbumEntity))
                            albums.Add(song.AlbumEntity);
                        foreach (var p in song.ArtistEntities)
                            if (!performers.Contains(p))
                                performers.Add(p);
                        foreach (var p in song.AlbumEntity.AlbumArtistEntities)
                            if (!performers.Contains(p))
                                performers.Add(p);
                        if (!genres.Contains(song.GenreEntity))
                            genres.Add(song.GenreEntity);
                    }
                }
                e.Result = args;
            };
            loadingViewModel.RunWorkerCompleted += (bgw, vm, result, e) =>
            {
                vm.Value = vm.Max;
                SongLibrary = new ObservableCollection<SongEntity>(result.Songs);
                AlbumLibrary = new ObservableCollection<AlbumEntity>(result.Albums);
                PerformerLibrary = new ObservableCollection<PerformerEntity>(result.Performers);
                GenreLibrary = new ObservableCollection<GenreEntity>(result.Genres);
                NotifyPropertyChanged(nameof(SongLibraryCount));
                NotifyPropertyChanged(nameof(AlbumList));
            };
            loadingViewModel.RunWorkerAsync(new LoadingLibraryArgsAndResult()
            {
                Directories = ScannedDirectoryInfo.ToArray()
            },(args) => true);
        }
        public IEnumerable AlbumList
        {
            get
            {
                return AlbumLibrary;
            }
        }
        public enum LibraryStyle { Grid, CoverFlow }
        public void SaveLibrary()
        {
            string LibraryString = "";
            LibraryString += string.Concat(SongLibrary.Select(x => $", {x.Id}")).Trim(", ".ToCharArray());
            LibraryString += ";\n";
            LibraryString += string.Concat(AlbumLibrary.Select(x => $", {x.Id}")).Trim(", ".ToCharArray());
            LibraryString += ";\n";
            LibraryString += string.Concat(PerformerLibrary.Select(x => $", {x.Id}")).Trim(", ".ToCharArray());
            LibraryString += ";\n";
            LibraryString += string.Concat(GenreLibrary.Select(x => $", {x.Id}")).Trim(", ".ToCharArray());
            LibraryString += ";\n";
            File.WriteAllText($"{App.ExecuteFilePath}Library.txt", LibraryString);
        }
        public void LoadLibrary()
        {
            if (!File.Exists($"{App.ExecuteFilePath}Library.txt"))
                return;
            string LibraryString = File.ReadAllText($"{App.ExecuteFilePath}Library.txt");
            var strsplit = LibraryString.Split(new char[] { ';' }, StringSplitOptions.None);
            for (int i = 0; i < strsplit.Length; i++)
            {
                if (i == 0)
                    SongLibrary = new ObservableCollection<SongEntity>(MusicDatabase.SongColle.FindAll());
                if (i == 1)
                    AlbumLibrary = new ObservableCollection<AlbumEntity>(MusicDatabase.AlbumColle.FindAll());
                if (i == 2)
                    PerformerLibrary = new ObservableCollection<PerformerEntity>(MusicDatabase.PerformerColle.FindAll());
                if (i == 3)
                    GenreLibrary = new ObservableCollection<GenreEntity>(MusicDatabase.GenreColle.FindAll());
            }
        }
    }
}
