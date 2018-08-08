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

        [BsonRef("AlbumEntity")]
        public AlbumEntity AlbumEntity { get; set; }

        [BsonIgnore]
        public string Album => AlbumEntity.Name;

        [BsonIgnore]
        public IEnumerable<PerformerEntity> ArtistEntities => MusicDatabase.SongArtistColle.Find(x => x.Song == this).Select(x=>x.Performer);
        [BsonIgnore]
        public string Artists => string.Join(", ", ArtistEntities);

        [BsonRef("GenreEntity")]
        public GenreEntity GenreEntity { get; set; }
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
        [BsonField("cover_data")]
        public byte[] CoverData { get; set; } = null;

        [BsonIgnore]
        public ImageSource Cover
        {
            get
            {
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
    }
    public enum CoverPathType { NoneCover, FromAudioFile, FromImageFile };

    public class AlbumEntity : ICover
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonIgnore]
        public IEnumerable<PerformerEntity> AlbumArtistEntities => MusicDatabase.AlbumArtistColle.Find(x => x.Album == this).Select(x => x.Performer);
        [BsonIgnore]
        public string AlbumArtists => string.Join(", ", AlbumArtistEntities);

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
    }

    public class AlbumArtistRelationShip
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonRef("AlbumEntity")]
        public AlbumEntity Album { get; set ; }

        [BsonRef("PerformerEntity")]
        public PerformerEntity Performer { get; set; }
    }
    public class SongArtistRelationShip
    {
        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonRef("SongEntity")]
        public SongEntity Song { get; set; }

        [BsonRef("PerformerEntity")]
        public  PerformerEntity Performer { get; set; }
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

    public class ScannedDirectoryInfo
    {
        public readonly DirectoryInfo DirectoryInfo;
        public string FullName => DirectoryInfo.FullName;
        public bool IsScanAllSubDirectories { get; set; } = true;
        public ScannedDirectoryInfo(string path)
        {
            DirectoryInfo = new DirectoryInfo(path);
        }
        public ScannedDirectoryInfo(string path, bool ScanAllSubDirectories )
        {
            DirectoryInfo = new DirectoryInfo(path);
            IsScanAllSubDirectories = ScanAllSubDirectories;
        }
        public override string ToString()
        {
            return FullName;
        }
    }
}
