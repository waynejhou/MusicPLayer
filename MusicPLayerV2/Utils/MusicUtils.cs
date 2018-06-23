using CSCore;
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
using static MusicPLayerV2.Utils.MusicDatabase;

namespace MusicPLayerV2.Utils
{
    #region oldMusicItem
    /// <summary>
    /// 音樂項目
    /// </summary>
    /*
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
    */
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
        static Regex lcbRegex = new Regex(@"\{", RegexOptions.Compiled);
        static Regex rcbRegex = new Regex(@"\}", RegexOptions.Compiled);
        static Regex jsonUnitRegex = new Regex("\"([^\"]+)\"[' ']*:[' ']*\"([^\"]*)\"");
        static Regex jsonObjectRegex = new Regex("\"([^\"]+)\"[' ']*:[' '\\s]*\\[");
        public static void ImportTables(string path, ExportType type, bool isCompression = true)
        {
            string allText;
            if (isCompression)
            {
                if (!File.Exists(path + ".bin"))
                    return;
                using (var fs = File.OpenRead(path + ".bin"))
                using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                using (var sd = new StreamReader(ds))
                {
                    allText = sd.ReadToEnd();
                }
            }
            else
                allText = File.ReadAllText(path);
            Stack<int> poses = new Stack<int>();
            int indent = -1;
            string arrayName = "";
            List <Dictionary<string, object>> tables = new List<Dictionary<string, object>>();
            Stack<Dictionary<string, object>> tableStack = new Stack<Dictionary<string, object>>();
            string str = "";
            for (int i = 0; i < allText.Length; i++)
            {
                str += allText[i];
                if (jsonObjectRegex.IsMatch(str))
                {
                    var m = jsonObjectRegex.Match(str);
                    var n = new List<Dictionary<string, object>>();
                    tableStack.Peek().Add(m.Groups[1].Value, n);
                    arrayName = m.Groups[1].Value;
                    Console.WriteLine("array[");
                }
                else if (lcbRegex.IsMatch(str))
                {
                    indent += 1;
                    if (indent == 0)
                    {
                        var n = new Dictionary<string, object>();
                        tables.Add(n);
                        tableStack.Push(n);
                    }
                    else
                    {
                        var n = new Dictionary<string, object>();
                        (tableStack.Peek()[arrayName] as List<Dictionary<string, object>>).Add(n);
                        tableStack.Push(n);
                    }
                    Console.WriteLine($"push() {indent}");
                }
                else if (rcbRegex.IsMatch(str))
                {
                    indent -= 1;
                    tableStack.Pop();
                    Console.WriteLine($"pop() {indent}");
                }
                else if (jsonUnitRegex.IsMatch(str))
                {
                    var m = jsonUnitRegex.Match(str);
                    tableStack.Peek().Add(m.Groups[1].Value, m.Groups[2].Value);
                    //Console.WriteLine($"{m.Groups[1]}  --  {m.Groups[2]}");
                }
                else
                    continue;
                str = "";
                continue;
            }
            var items = tables[3]["Items"] as List<Dictionary<string, object>>;
            foreach(var genre in items)
            {
                Genres.Add(new GenreEntity()
                {
                    Id = Int32.Parse((string)genre["Id"]),
                    Name = (string)genre["GenreName"]
                });
            }
            items = tables[2]["Items"] as List<Dictionary<string, object>>;
            foreach (var performers in items)
            {
                Performers.Add(new PerformerEntity()
                {
                    Id = Int32.Parse((string)performers["Id"]),
                    Name = (string)performers["PerformName"]
                });
            }
            items = tables[1]["Items"] as List<Dictionary<string, object>>;
            foreach (var album in items)
            {
                Albums.Add(new AlbumEntity()
                {
                    Id = Int32.Parse((string)album["Id"]),
                    Name = (string)album["AlbumName"],
                    ArtistEntities = Performers.SplitHashesToEntity((string)album["Artists"]),
                    GenreEntities = Genres.SplitHashesToEntity((string)album["Genre"]),
                    CoverBase64String = (string)album["CoverBase64String"],
                    CoverPath = (string)album["CoverPath"],
                });
            }
            items = tables[0]["Items"] as List<Dictionary<string, object>>;
            foreach (var song in items)
            {
                Songs.Add(new SongEntity()
                {
                    Id = Int32.Parse((string)song["Id"]),
                    Name = (string)song["Path"],
                    Title = (string)song["Title"],
                    AlbumEntity = Albums.FindEntityByHash(int.Parse((string)song["Album"])),
                    ArtistEntities = Performers.SplitHashesToEntity((string)song["Artists"]),
                    GenreEntity = Genres.FindEntityByHash(int.Parse((string)song["Genre"])),
                    Track = uint.Parse((string)song["Track"]),
                    Year = uint.Parse((string)song["Year"]),
                    Length = TimeSpan.Parse((string)song["Length"]),
                    FileLastModded = DateTime.Parse((string)song["FileLastModded"]),
                });
            }
            Console.WriteLine(tables.Count);
        }
        public static void ExportTables(string path, ExportType type, bool isCompression = true)
        {
            switch (type)
            {
                case ExportType.JSON:
                    string exportString = "{\n";
                    exportString +=
                        $" {JsonUnitString("TableName", nameof(Songs))}\n" +
                        $" \"Items\":[\n";
                    foreach (var column in Songs)
                    {
                        exportString +=
                             " {\n" +
                            $"  {JsonUnitString("Id", column.Id)},\n" +
                            $"  {JsonUnitString("Path", column.Name)},\n" +
                            $"  {JsonUnitString("Title", column.Title)},\n" +
                            $"  {JsonUnitString("Album", column.AlbumId)},\n" +
                            $"  {JsonUnitString("Artists", column.ArtistIds.ConcatListIds(", "))},\n" +
                            $"  {JsonUnitString("Genre", column.GenreId)},\n" +
                            $"  {JsonUnitString("Track", column.Track)},\n" +
                            $"  {JsonUnitString("Year", column.Track)},\n" +
                            $"  {JsonUnitString("Length", column.Length)},\n" +
                            $"  {JsonUnitString("FileLastModded", column.FileLastModded)},\n" +
                             " }";
                        if (column != Songs.Last())
                            exportString += ",\n";
                        else
                            exportString += "]\n";
                    }
                    exportString += "}\n";
                    exportString += "{\n";
                    exportString +=
                        $" {JsonUnitString("TableName", nameof(Albums))}\n" +
                        $" \"Items\":[\n";
                    foreach (var column in Albums)
                    {
                        exportString +=
                             " {\n" +
                            $"  {JsonUnitString("Id", column.Id)},\n" +
                            $"  {JsonUnitString("AlbumName", column.Name)},\n" +
                            $"  {JsonUnitString("Artists", column.ArtistIds.ConcatListIds(", "))},\n" +
                            $"  {JsonUnitString("Genre", column.GenreIds.ConcatListIds(", "))},\n" +
                            $"  {JsonUnitString("CoverBase64String", column.CoverBase64String)},\n" +
                            $"  {JsonUnitString("CoverPath", column.CoverPath)},\n" +
                             " }";
                        if (column != Albums.Last())
                            exportString += ",\n";
                        else
                            exportString += "]\n";
                    }
                    exportString += "}\n";
                    exportString += "{\n";
                    exportString +=
                        $" {JsonUnitString("TableName", nameof(Performers))}\n" +
                        $" \"Items\":[\n";
                    foreach (var column in Performers)
                    {
                        exportString +=
                             " {\n" +
                            $"  {JsonUnitString("Id", column.Id)},\n" +
                            $"  {JsonUnitString("PerformName", column.Name)},\n" +
                             " }";
                        if (column != Performers.Last())
                            exportString += ",\n";
                        else
                            exportString += "]\n";
                    }
                    exportString += "}\n";
                    exportString += "{\n";
                    exportString +=
                        $" {JsonUnitString("TableName", nameof(Genres))}\n" +
                        $" \"Items\":[\n";
                    foreach (var column in Genres)
                    {
                        exportString +=
                             " {\n" +
                            $"  {JsonUnitString("Id", column.Id)},\n" +
                            $"  {JsonUnitString("GenreName", column.Name)},\n" +
                             " }";
                        if (column != Genres.Last())
                            exportString += ",\n";
                        else
                            exportString += "]\n";
                    }
                    exportString += "}\n";
                    if (isCompression)
                    {
                        using (var fs = File.Create(path + ".bin"))
                        using (var ds = new DeflateStream(fs, CompressionLevel.Optimal, true))
                        using (var sw = new StreamWriter(ds))
                        {
                            sw.Write(exportString);
                        }
                    }
                    else
                        File.WriteAllText(path, exportString);
                    break;
                case ExportType.XML:
                    throw new NotImplementedException();
                case ExportType.MultipleCSVs:
                    throw new NotImplementedException();
                default:
                    break;
            }
        }
        static string JsonUnitString(string key, object value, JsonValueType type = JsonValueType.Value)
        {
            switch (type)
            {
                case JsonValueType.Value:
                    return $"\"{key}\":\"{value}\"";
                case JsonValueType.Array:
                    return $"\"{key}\":[{value}]";
                default:
                    return "";
            }
        }
        enum JsonValueType { Value, Array }
    }
    public enum ExportType { JSON, XML, MultipleCSVs }

    public class SongEntity : MusicEntity, INotifyPropertyChanged
    {
        public bool IsRelativePath { get; set; } = false;

        public string Path { get => Name; set => Name = value; }

        public string Title { get; set; }

        public AlbumEntity AlbumEntity { get; set; }
        public int AlbumId => AlbumEntity.Id;
        public string Album => AlbumEntity.Name;

        public List<PerformerEntity> ArtistEntities { get; set; } = new List<PerformerEntity>();
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
            SongEntity ret;
            using (var w = CSCore.Codecs.CodecFactory.Instance.GetCodec(fileName))
            using (var t = TagLib.File.Create(fileName, TagLib.ReadStyle.None))
            {
                var tag = t.Tag;
                var f = new FileInfo(fileName);
                if (TryFindOrCreateEntity(fileName, out ret)) return ret;
                ret.Title = string.IsNullOrEmpty(tag.Title) ? f.Name : tag.Title;
                ret.Track = tag.Track;
                ret.Year = tag.Year;
                ret.Length = w.GetLength();
                ret.FileLastModded = f.LastWriteTimeUtc;
                TryFindOrCreateEntity(tag.FirstGenre, out GenreEntity genre);
                ret.GenreEntity = genre;
                if (!TryFindOrCreateAlbum(tag.Album, SplitNamesToEntity<PerformerEntity>(tag.FirstAlbumArtist), out AlbumEntity album))
                {
                    if (!album.GenreEntities.Contains(ret.GenreEntity))
                        album.GenreEntities.Add(ret.GenreEntity);
                    if (tag.Pictures.Length >= 1)
                    {
                        album.CoverPath = f.FullName;
                        album.CoverPathType = CoverPathType.FromAudioFile;
                    }
                }
                Console.WriteLine(album);
                ret.AlbumEntity = album;
                ret.ArtistEntities = SplitNamesToEntity<PerformerEntity>(tag.FirstPerformer);

            }
            return ret;
        }

    }

    public class AlbumEntity : MusicEntity
    {
        public List<PerformerEntity> ArtistEntities { get; set; } = new List<PerformerEntity>();
        public List<int> ArtistIds => ArtistEntities.Select(x => x.Id).ToList();
        public string Artists =>
            ArtistEntities.ConcatListNames();

        public List<GenreEntity> GenreEntities { get; set; } = new List<GenreEntity>();
        public List<int> GenreIds => GenreEntities.Select(x => x.Id).ToList();
        public string Genre =>
            GenreEntities.ConcatListNames();

        public string CoverBase64String { get; set; }
        public CoverPathType CoverPathType { get; set; } = CoverPathType.NoneCover;
        public string CoverPath { get; set; }
        public ImageSource Cover { get; set; }
        public Size CoverSize { get; set; }
        public override int GetHashCode()
        {
            return DateTime.UtcNow.ToString().GetHashCode() ^ Name.GetHashCode() ^ Artists.GetHashCode();
        }

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
            return DateTime.UtcNow.ToString().GetHashCode()^Name.GetHashCode();
        }
        protected static bool TryFindOrCreateAlbum(string name, List<PerformerEntity> performers, out AlbumEntity entity)
        {
            var n = new AlbumEntity()
            {
                Name = name,
                ArtistEntities = performers
            };
            var hash = n.GetHashCode();
            if (Albums.Exists(x => x.Id == hash))
            {
                entity = Albums.Find(x => x.Id == hash);
                return true;
            }
            entity = n;
            entity.Id = entity.GetHashCode();
            Albums.Add(entity);
            return false;
        }
        protected static bool TryFindOrCreateEntity<T>(string name, out T entity) where T : MusicEntity, new()
        {
            if (typeof(T)==typeof(AlbumEntity))
                throw new Exception();
            if (Tables[typeof(T)] is List<T> table)
            {
                if (table.Exists(x => x.Name == name))
                {
                    entity = table.Find(x => x.Name == name);
                    return true;
                }
                else
                {
                    entity = new T()
                    {
                        Name = name
                    };
                    entity.Id = entity.GetHashCode();
                    table.Add(entity);
                    return false;
                }
            }
            throw new Exception("Database Type Match false");
        }
        protected static List<T> SplitNamesToEntity<T>(string namesString, string splitString = ",;|，、") where T : MusicEntity, new()
        {
            var ret = new List<T>();
            var names = namesString.Split(splitString.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var n in names)
            {
                TryFindOrCreateEntity(n, out T entity);
                ret.Add(entity);
            }
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
        public static List<T> SplitHashesToEntity<T>(this List<T> list, string hasheString, string splitString = ",;|，、") where T : MusicEntity, new()
        {
            var ret = new List<T>();
            var ids = hasheString.Split(splitString.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var n in ids)
            {
                ret.Add(list.FindEntityByHash(int.Parse(n)));
            }
            return ret;
        }
        public static string ConcatListNames<T>(this List<T> list, string splitString = ", ", string leftBracket = "", string rightBracket = "") where T : MusicEntity
        {
            if (list == null)
                return "";
            return string.Concat(list.Select(x => $"{leftBracket}{x.Name}{rightBracket}" + splitString)).Trim(splitString.ToCharArray());
        }
        public static string ConcatListIds(this List<int> list, string splitString = ", ", string leftBracket = "", string rightBracket = "")
        {
            return string.Concat(list.Select(x => $"{leftBracket}{x}{rightBracket}" + splitString)).Trim(splitString.ToCharArray());
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
