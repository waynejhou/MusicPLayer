using CSCore;
using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace MusicPLayerV2.Utils
{
    public class LyricWithTime : DependencyObject, INotifyPropertyChanged
    {
        public string Lyric { get; set; }
        public TimeSpan Time { get; set; }


        public bool IsHightLighted
        {
            get { return (bool)GetValue(IsHightLightedProperty); }
            set { SetValue(IsHightLightedProperty, value); }
        }
        public static readonly DependencyProperty IsHightLightedProperty =
            DependencyProperty.Register("IsHightLighted", typeof(bool), typeof(LyricWithTime),
                new FrameworkPropertyMetadata((DependencyObject obj, DependencyPropertyChangedEventArgs args) =>
                {
                    (obj as LyricWithTime).OnSetHighlighted((bool)args.NewValue);
                }));

        private void OnSetHighlighted(bool newValue)
        {
            NotifyPropertyChanged(nameof(IsHightLighted));
        }

        public override string ToString()
        {
            return Lyric;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class SongEntity : INotifyPropertyChanged, ICover
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("path")]
        public string Path { get; set; }

        [BsonField("title")]
        public string Title { get; set; }

        [BsonField("album_id")]
        public int AlbumId { get; set; }

        [BsonIgnore]
        public AlbumEntity AlbumEntity => MusicDatabase.AlbumColle.FindById(AlbumId);

        [BsonIgnore]
        public string Album => AlbumEntity.Name;

        [BsonIgnore]
        public IEnumerable<PerformerEntity> ArtistEntities => MusicDatabase.SongArtistColle.Find(x => x.SongId == Id).Select(x=>x.PerformerEntity);
        [BsonIgnore]
        public string Artists => string.Join(", ", ArtistEntities.Select(x=>x.Name));


        [BsonField("genre_id")]
        public int GenreId { get; set; }
        [BsonIgnore]
        public GenreEntity GenreEntity => MusicDatabase.GenreColle.FindById(GenreId);
        [BsonIgnore]
        public string Genre => GenreEntity.Name;

        [BsonField("track")]
        public uint Track { get; set; }
        [BsonField("year")]
        public uint Year { get; set; }
        [BsonField("length")]
        public TimeSpan Length { get; set; }
        [BsonIgnore]
        public string LengthString => Length.ToString(@"mm\:ss");
        [BsonField("file_last_modded")]
        public DateTime FileLastModded { get; set; }
        [BsonField("cover_path")]
        public string CoverPath { get; set; }
        [BsonField("cover_path_type")]
        public CoverPathType CoverPathType { get; set; }
        [BsonField("cover_size_width")]
        public int CoverSizeWidth { get; set; } = 0;
        [BsonField("cover_size_height")]
        public int CoverSizeHeight { get; set; } = 0;
        [BsonIgnore]
        public byte[] CoverData { get; set; } = null;

        [BsonIgnore]
        public ImageSource Cover
        {
            get
            {
                if (CoverPathType == CoverPathType.NoneCover)
                    return null;
                BitmapImage ret = new BitmapImage();
                if (CoverData == null)
                {
                    MusicDatabase.UpdateCover(this);
                }
                using (var ms = new MemoryStream(CoverData))
                {
                    ret.BeginInit();
                    ret.StreamSource = ms;
                    ret.CacheOption = BitmapCacheOption.OnLoad;
                    ret.EndInit();
                }
                return ret;
            }
        }

        [BsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        [BsonIgnore]
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        [BsonIgnore]
        bool _isNowPlaying;
        [BsonIgnore]
        public bool IsNowPlaying
        {
            get => _isNowPlaying;
            set
            {
                _isNowPlaying = value;
                NotifyPropertyChanged(nameof(IsNowPlaying));
            }
        }

        public void Cat()
        {
            Console.WriteLine(
                $"[{Path}\n" +
                $" {Title}\n" +
                $" {AlbumEntity.Id}: {AlbumEntity.Name}\n" +
                $" {Artists}\n" +
                $" {GenreEntity.Id}: {GenreEntity.Name}]");
        }
    }
    public enum CoverPathType { NoneCover, FromAudioFile, FromImageFile };
    public class AlbumEntity : ICover
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonIgnore]
        public IEnumerable<PerformerEntity> AlbumArtistEntities => MusicDatabase.AlbumArtistColle.Find(x => x.AlbumId == Id).Select(x => x.PerformerEntity);
        [BsonIgnore]
        public string Artists => string.Join(", ", AlbumArtistEntities.Select(x=>x.Name));

        [BsonField("cover_path")]
        public string CoverPath { get; set; } = null;
        [BsonField("cover_path_type")]
        public CoverPathType CoverPathType { get; set; } = CoverPathType.NoneCover;
        [BsonField("cover_track")]
        public uint CoverTrack { get; set; } = 0;
        [BsonField("cover_size")]
        public int CoverSizeWidth { get; set; } = 0;
        [BsonField("cover_height")]
        public int CoverSizeHeight { get; set; } = 0;
        [BsonField("cover_data")]
        public byte[] CoverData { get; set; }

        [BsonIgnore]
        public ImageSource Cover
        {
            get
            {
                if (CoverPathType == CoverPathType.NoneCover)
                    return App.Current.Resources["UnknowImage"] as BitmapImage;
                BitmapImage ret = new BitmapImage();
                if (CoverData == null)
                {
                    MusicDatabase.UpdateCover(this);
                }
                using (var ms = new MemoryStream(CoverData))
                {
                    ret.BeginInit();
                    ret.StreamSource = ms;
                    ret.CacheOption = BitmapCacheOption.OnLoad;
                    ret.EndInit();
                }
                return ret;
            }
        }
    }
    public class PerformerEntity
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }
    }
    public class GenreEntity
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class AlbumArtistRelationShip
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonField("album_id")]
        public int AlbumId { get; set; }

        [BsonIgnore]
        public AlbumEntity AlbumEntity => MusicDatabase.AlbumColle.FindById(AlbumId);

        [BsonIgnore]
        public string Album => AlbumEntity.Name;

        [BsonField("performer_id")]
        public int PerformerId { get; set; }

        [BsonIgnore]
        public PerformerEntity PerformerEntity => MusicDatabase.PerformerColle.FindById(PerformerId);

        [BsonIgnore]
        public string Performer => PerformerEntity.Name;
    }
    public class SongArtistRelationShip
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonField("song_id")]
        public int SongId { get; set; }

        [BsonIgnore]
        public SongEntity SongEntity => MusicDatabase.SongColle.FindById(SongId);

        [BsonIgnore]
        public string Song => SongEntity.Path;

        [BsonField("performer_id")]
        public int PerformerId { get; set; }

        [BsonIgnore]
        public PerformerEntity PerformerEntity => MusicDatabase.PerformerColle.FindById(PerformerId);

        [BsonIgnore]
        public string Performer => PerformerEntity.Name;
    }

    public class LibraryEntity
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("path")]
        public string Path { get; set; }

        [BsonField("scan_all_sub")]
        public bool IsScanAllSubDirectories { get; set; } = true;
    }
    public class LibrarySongRelationship
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonField("lib_id")]
        public int LibraryId { get; set; }

        [BsonIgnore]
        public LibraryEntity LibraryEntity => MusicDatabase.LibraryColle.FindById(LibraryId);

        [BsonField("song_id")]
        public int SongId { get; set; }

        [BsonIgnore]
        public SongEntity Song => MusicDatabase.SongColle.FindById(SongId);
    }



    public class SongCompareAlbum : IEqualityComparer<SongEntity>
    {
        public static SongCompareAlbum New => new SongCompareAlbum();
        public bool Equals(SongEntity x, SongEntity y)
        {
            return x.AlbumId == y.AlbumId;
        }

        public int GetHashCode(SongEntity obj)
        {
            return obj.AlbumId;
        }
    }

    public class AlbumComapreId : IEqualityComparer<AlbumEntity>
    {
        public static AlbumComapreId New => new AlbumComapreId();
        public bool Equals(AlbumEntity x, AlbumEntity y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(AlbumEntity obj)
        {
            return obj.Id;
        }
    }


    public interface ICover
    {
        string CoverPath { get; set; }
        CoverPathType CoverPathType { get; set; }
        int CoverSizeWidth { get; set; } 
        int CoverSizeHeight { get; set; }
        byte[] CoverData { get; set; }
        ImageSource Cover { get; }
    }




    public static class ObjectSaveToXML<T> where T : new()
    {
        public static void SaveSettingAsXml(T @object, string fileName)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(@object.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, @object);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(T)}.SaveSettingAsXml: {ex}");
            }
        }
        public static T LoadSettingFromXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { throw new ArgumentNullException("NameNull"); }
            T Setting = new T();
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;
                using (StringReader read = new StringReader(xmlString))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        Setting = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(T)}.LoadSettingFromXml: {ex}");
            }
            return Setting;
        }
    }

}
