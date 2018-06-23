using CSCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using static MusicPLayerV2.Utils.MusicDatabase;

namespace MusicPLayerV2.Utils
{
    #region oldMusicItem
    /// <summary>
    /// 音樂項目
    /// </summary>

    public class MusicItem : INotifyPropertyChanged
    {
        #region 屬性

        /// <summary>
        /// 音樂檔案路徑
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 音樂標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 音樂專輯
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// 音樂演出者
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// 音樂專輯演出者
        /// </summary>
        public string AlbumArtist { get; set; }

        /// <summary>
        /// 音樂分類
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// 音樂年分
        /// </summary>
        public uint Year { get; set; }

        /// <summary>
        /// 音樂軌數
        /// </summary>
        public uint Track { get; set; }

        /// <summary>
        /// 檔案最後修改日期
        /// </summary>
        public DateTime FileLastModded { get; set; }

        /// <summary>
        /// 音樂長度
        /// </summary>
        public TimeSpan Length { get; set; }

        /// <summary>
        /// 音樂長度字串
        /// 給懶得打 ToString(@"mm\:ss") 人用的
        /// </summary>
        [XmlIgnore]
        public string LengthString => Length.ToString(@"mm\:ss");

        /// <summary>
        /// 音樂專輯圖片字串
        /// (Base64)
        /// </summary>
        public string PictureBase64 { get; set; }

        /// <summary>
        /// 音樂專輯圖片
        /// </summary>
        [XmlIgnore]
        public ImageSource Picture { get; set; }

        [XmlIgnore]
        public Size PictureSize { get; private set; }

        bool _isNowPlaying;
        public bool IsNowPlaying
        {
            get => _isNowPlaying;
            set
            {
                _isNowPlaying = value;
                NotifyPropertyChanged(nameof(IsNowPlaying));
            }
        }

        /// <summary>
        /// 未知的音樂項目
        /// </summary>
        public static MusicItem UnknowItem { get; } = new MusicItem
        {
            Title = "Unknow Title",
            Album = "Unknow Album",
            Artist = "Unknow Artist",
            AlbumArtist = "Unknow Album Artist",
            Genre = "Unknow Genre",
            Year = 0,
            Track = 0,
            FileLastModded = DateTime.MinValue,
            Length = TimeSpan.Zero,
            Picture = null
        };

        #endregion

        #region 成員函式

        /// <summary>
        /// 阿...就 ToString阿
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"{Path}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void NotifyMusicItem()
        {
            NotifyPropertyChanged(nameof(IsNowPlaying));
        }

        public void TryUpdatePicture(double imgSize = 500)
        {
            using (var t = TagLib.File.Create(Path))
            {
                var tag = t.Tag;
                if (tag.Pictures.Length <= 0)
                {
                    var picPath =
                        new FileInfo(Path).Directory.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                        .Where(x => x.Name.Contains("Cover"))
                        .Where(x => x.Name.EndsWith(".png")
                        || x.Name.EndsWith(".jpg")
                        || x.Name.EndsWith(".PNG")
                        || x.Name.EndsWith(".JPG")
                        || x.Name.EndsWith(".jpeg")
                        || x.Name.EndsWith(".JPEG")).Select(x => x.FullName).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(picPath))
                        Picture = new BitmapImage(new Uri(picPath));
                }
                else
                    Picture = FormatImage(tag.Pictures[0].Data.Data, imgSize);
            }
        }
        #endregion

        #region 靜態函式

        /// <summary>
        /// 影像資料轉 BitmapImage
        /// </summary>
        /// <param name="data">影像資料</param>
        /// <returns></returns>
        private ImageSource FormatImage(byte[] data, double imgSize)
        {
            if (data == null)
                return null;
            BitmapImage ret = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(data, 0, data.Length, true, true))
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
            {
                System.Drawing.Bitmap bmp;
                if (Math.Max(image.Height, image.Width) > imgSize)
                {
                    if (image.Width > image.Height)
                    {
                        var newWidth = (int)imgSize;
                        var newHeight = (int)Math.Round(image.Height * ((float)imgSize / image.Width));
                        bmp = new System.Drawing.Bitmap(image, newWidth, newHeight);
                    }
                    else
                    {
                        var newHeight = (int)imgSize;
                        var newWidth = (int)Math.Round(image.Width * ((float)imgSize / image.Height));
                        bmp = new System.Drawing.Bitmap(image, newWidth, newHeight);
                    }
                }
                else
                {
                    bmp = new System.Drawing.Bitmap(image, image.Width, image.Height);
                }
                PictureSize = new Size(bmp.Width, bmp.Height);
                using (var newms = new MemoryStream())
                {
                    bmp.Save(newms, System.Drawing.Imaging.ImageFormat.Png);
                    newms.Position = 0;
                    ret.BeginInit();
                    ret.StreamSource = newms;
                    ret.CacheOption = BitmapCacheOption.OnLoad;
                    ret.EndInit();
                }
            }
            return ret;
        }
        /// <summary>
        /// 讀取檔案回傳
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>音樂項目</returns>
        public static MusicItem CreateFromFile(string fileName, bool loadImage = true, double imgSize = 500d)
        {

            MusicItem ret;
            TagLib.ReadStyle rs = loadImage ? TagLib.ReadStyle.Average : TagLib.ReadStyle.None;
            using (var t = TagLib.File.Create(fileName, rs))
            {
                var tag = t.Tag;
                ret = new MusicItem()
                {
                    Title = tag.Title,
                    Album = tag.Album,
                    Artist = tag.FirstPerformer,
                    AlbumArtist = tag.FirstAlbumArtist,
                    Genre = tag.FirstGenre,
                    Path = fileName,
                    Year = tag.Year,
                    Track = tag.Track,
                    Length = CSCore.Codecs.CodecFactory.Instance.GetCodec(fileName).GetLength()
                };
            }
            ret.TryUpdatePicture(imgSize);
            return ret;
        }

        #endregion
    }

    #endregion

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

    public static class MusicDatabase
    {
        static MusicDatabase()
        {

        }
        public static Dictionary<Type, IList> Tables => new Dictionary<Type, IList>()
        {
            { typeof(SongEntity), Songs },
            { typeof(AlbumEntity), Albums },
            { typeof(PerformerEntity), Performers },
            { typeof(GenreEntity), Genres }
        };
        public static List<SongEntity> Songs { get; set; } = new List<SongEntity>() { };
        public static List<AlbumEntity> Albums { get; set; } = new List<AlbumEntity>() { };
        public static List<PerformerEntity> Performers { get; set; } = new List<PerformerEntity>() { };
        public static List<GenreEntity> Genres { get; set; } = new List<GenreEntity>() { };
        public static void ExportTables(ExportType type)
        {
            switch (type)
            {
                case ExportType.JSON:
                    string exportString = "{\n";
                    foreach (var column in Songs)
                    {
                        exportString +=
                            $"{{\n" +
                            $"\"Id\":{column.Id},\n" +
                            $"\"Path\":{column.Name},\n" +
                            $"\"Title\":{column.Title},\n" +
                            $"\"Album\":{column.AlbumId},\n" +
                            $"\"Artists\":[{column.ArtistIds.ConcatListIds()}],\n" +
                            $"\"Genre\":[{column.Genre}],\n" +
                            $"\"Track\":[{column.Track}],\n" +
                            $"\"Year\":[{column.Track}],\n" +
                            $"\"Length\":[{column.Length}],\n" +
                            $"\"FileLastModded\":[{column.FileLastModded}],\n" +
                            $"}}\n";
                    }
                    foreach (var column in Albums)
                    {
                        exportString +=
                             "{\n" +
                            $"\"Id\":{column.Id},\n" +
                            $"\"AlbumName\":{column.Name},\n" +
                            $"\"Artists\":[{column.ArtistIds.ConcatListIds()}],\n" +
                            $"\"Genre\":[{column.GenreIds.ConcatListIds()}],\n" +
                            $"\"CoverBase64String\":[{column.CoverBase64String}],\n" +
                            $"\"CoverPath\":[{column.CoverPath}],\n" +
                             "}\n";
                    }
                    foreach (var column in Performers)
                    {
                        exportString +=
                             "{\n" +
                            $"\"Id\":{column.Id},\n" +
                            $"\"PerformName\":{column.Name},\n" +
                             "}\n";
                    }
                    foreach (var column in Genres)
                    {
                        exportString +=
                             "{\n" +
                            $"\"Id\":{column.Id},\n" +
                            $"\"GenreName\":{column.Name},\n" +
                             "}\n";
                    }
                    exportString = "}\n";
                    File.WriteAllText("DB.json", exportString);
                    break;
                case ExportType.XML:
                    throw new NotImplementedException();
                case ExportType.MultipleCSVs:
                    throw new NotImplementedException();
                default:
                    break;
            }

        }
    }
    public enum ExportType { JSON, XML, MultipleCSVs }

    public class SongEntity: MusicEntity, INotifyPropertyChanged
    {
        public bool IsRelativePath { get; set; } = false;

        public string Title { get; set; }

        public AlbumEntity AlbumEntity { get; set; }
        public int AlbumId => AlbumEntity.Id;
        public string Album => AlbumEntity.Name;

        public List<PerformerEntity> ArtistEntities { get; set; }
        public List<int> ArtistIds => ArtistEntities.Select(x => x.Id).ToList();
        public string Artists =>
            ArtistEntities.ConcatListNames();

        public GenreEntity GenreEntity { get; set; }
        public int GenreId => GenreEntity.Id;
        public string Genre => GenreEntity.Name;

        public uint Track { get; set; }

        public uint Year { get; set; }

        public TimeSpan Length { get; set; }
        public string LengthString => Length.ToString(@"mm\:ss");

        public DateTime FileLastModded { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        bool _isNowPlaying;
        public bool IsNowPlaying
        {
            get => _isNowPlaying;
            set
            {
                _isNowPlaying = value;
                NotifyPropertyChanged(nameof(IsNowPlaying));
            }
        }

        public static SongEntity CreateFromFile(string fileName)
        {
            SongEntity ret ;
            using (var w = CSCore.Codecs.CodecFactory.Instance.GetCodec(fileName))
            using (var t = TagLib.File.Create(fileName, TagLib.ReadStyle.None))
            {
                var tag = t.Tag;
                var f = new FileInfo(fileName);
                if (TryFindOrCreateEntity(fileName, out ret)) return ret;
                ret = new SongEntity()
                {
                    Name = fileName,
                    Id = fileName.GetHashCode(),
                    Title = string.IsNullOrEmpty(tag.Title) ? tag.Title : f.Name,
                    Track = tag.Track,
                    Year = tag.Year,
                    Length = w.GetLength(),
                    FileLastModded = f.LastWriteTimeUtc
                };
                if (TryFindOrCreateEntity(tag.FirstGenre, out GenreEntity genre)) ret.GenreEntity = genre;
                if (TryFindOrCreateEntity(tag.Album, out AlbumEntity album))
                {
                    album.ArtistEntities = SplitNamesToEntity<PerformerEntity>(tag.FirstAlbumArtist);
                    if (!album.GenreEntities.Contains(ret.GenreEntity))
                        album.GenreEntities.Add(ret.GenreEntity);
                    if (tag.Pictures.Length >= 1)
                    {
                        album.CoverPath = f.FullName;
                        album.CoverPathType = CoverPathType.FromAudioFile;
                    }
                    ret.AlbumEntity = album;
                }

                ret.ArtistEntities = SplitNamesToEntity<PerformerEntity>(tag.FirstPerformer);

            }
            return ret;
        }
        
    }

    public class AlbumEntity : MusicEntity
    {
        public List<PerformerEntity> ArtistEntities { get; set; }
        public List<int> ArtistIds => ArtistEntities.Select(x => x.Id).ToList();
        public string Artists =>
            ArtistEntities.ConcatListNames();

        public List<GenreEntity> GenreEntities { get; set; }
        public List<int> GenreIds => GenreEntities.Select(x => x.Id).ToList();
        public string Genre =>
            GenreEntities.ConcatListNames();

        public string CoverBase64String { get; set; }
        public CoverPathType CoverPathType { get; set; } = CoverPathType.NoneCover;
        public string CoverPath { get; set; }


    }
    public enum CoverPathType { NoneCover, FromAudioFile, FromImageFile };

    public class PerformerEntity : MusicEntity
    {
    }
    public class GenreEntity : MusicEntity
    {
    }

    public abstract class MusicEntity
    {
        protected MusicEntity()
        {
        }
        public int Id { get; set; } = -1;
        public string Name { get; set; } = "Unknown";
        public override string ToString()
        {
            return Name;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;
            if ((obj as MusicEntity).Id != Id)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        protected static bool TryFindOrCreateEntity<T>(string name, out T　entity) where T : MusicEntity, new()
        {
            if (Tables[typeof(T)] is List<T> table)
            {
                if(table.Exists(x => x.Name == name))
                {
                    entity = table.Find(x => x.Name == name);
                    return true;
                }
                else
                {
                    entity = new T()
                    {
                        Name = name,
                        Id = name.GetHashCode()
                    };
                    table.Add(entity);
                    return true;
                }
            }
            throw new Exception("Database Type Match false");
        }
        protected static List<T> SplitNamesToEntity<T>(string namesString, string splitString = ",;|") where T : MusicEntity, new()
        {
            var ret = new List<T>();
            var names = namesString.Split(splitString.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach(var n in names)
                if (TryFindOrCreateEntity(n, out T entity)) ret.Add(entity);
            return ret;
        }
    }

    public static class MusicEntityExtentionMethods
    {
        static Predicate<MusicEntity> compareId = (x => x.Id == comparedHash);
        static int comparedHash = 0;
        public static T FindEntityByHash<T>(this List<T> list, int hash) where T : MusicEntity
        {
            comparedHash = hash;
            if (list.Exists(compareId))
                return list.Find(compareId);
            else
                throw new KeyNotFoundException();
        }
        public static string ConcatListNames<T>(this List<T> list, string splitString = ", ") where T : MusicEntity
        {
            return string.Concat(list.Select(x => x.Name + splitString)).Trim(splitString.ToCharArray());
        }
        public static string ConcatListIds(this List<int> list, string splitString = ", ")
        {
            return string.Concat(list.Select(x => x + splitString)).Trim(splitString.ToCharArray());
        }
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
