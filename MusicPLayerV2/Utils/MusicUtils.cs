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
                    CoverPath = (string)album["CoverPath"],
                    CoverPathType = (CoverPathType)Enum.Parse(typeof(CoverPathType),(string)album["CoverPathType"]),
                    CoverSize = Size.Parse((string)album["CoverSize"]),
                    CoverTrack = uint.Parse((string)album["CoverTrack"])
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
                    CoverPath = (string)song["CoverPath"],
                    CoverPathType = (CoverPathType)Enum.Parse(typeof(CoverPathType), (string)song["CoverPathType"]),
                    CoverSize = Size.Parse((string)song["CoverSize"])
                });
            }
            if (File.Exists($"{App.ExecuteFilePath}Cover.bin"))
            {
                using (var fs = File.OpenRead($"{App.ExecuteFilePath}Cover.bin"))
                {
                    fs.Position = 0;
                    while (fs.Position != fs.Length)
                    {
                        var idBytes = new byte[4];
                        var coverDataLengthBytes = new byte[4];
                        fs.Read(idBytes, 0, 4);
                        fs.Read(coverDataLengthBytes, 0, 4);
                        var coverDataLength = BitConverter.ToInt32(coverDataLengthBytes, 0);
                        var coverData = new byte[coverDataLength];
                        fs.Read(coverData, 0, coverDataLength);
                        Albums.First(x => x.Id == BitConverter.ToInt32(idBytes, 0)).CoverData = coverData;
                    }
                }
            }
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
                            $"  {JsonUnitString("CoverPath", column.CoverPath)},\n" +
                            $"  {JsonUnitString("CoverPathType", column.CoverPathType)},\n" +
                            $"  {JsonUnitString("CoverSize", column.CoverSize)},\n" +
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
                            $"  {JsonUnitString("CoverPath", column.CoverPath)},\n" +
                            $"  {JsonUnitString("CoverSize", column.CoverSize)},\n" +
                            $"  {JsonUnitString("CoverTrack", column.CoverTrack)},\n" +
                            $"  {JsonUnitString("CoverPathType", column.CoverPathType)},\n" +
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
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            File.WriteAllText(path, exportString);
                        }
                    }
                    else
                        File.WriteAllText(path, exportString);
                    goto default;
                case ExportType.XML:
                    throw new NotImplementedException();
                case ExportType.MultipleCSVs:
                    throw new NotImplementedException();
                default:
                    if (File.Exists($"{App.ExecuteFilePath}Cover.bin"))
                        File.Delete($"{App.ExecuteFilePath}Cover.bin");
                    using (var fs = File.OpenWrite($"{App.ExecuteFilePath}Cover.bin"))
                    {
                        fs.Position = 0;
                        foreach (var a in Albums.Where(x => x.CoverData != null))
                        {
                            var idBytes = BitConverter.GetBytes(a.Id);
                            var coverDataLengthBytes = BitConverter.GetBytes(a.CoverData.Length);
                            Console.WriteLine(a.CoverData.Length);
                            fs.Write(idBytes, 0, idBytes.Length);
                            fs.Write(coverDataLengthBytes, 0, coverDataLengthBytes.Length);
                            fs.Write(a.CoverData, 0, a.CoverData.Length);
                        }
                    }
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
    public enum BinaryTypeCode { }

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

        static double FormatSize { get; set; } = 500d;
        public CoverPathType CoverPathType { get; set; } = CoverPathType.NoneCover;
        public string CoverPath { get; set; }
        public ImageSource Cover { get; set; }
        public Size CoverSize { get; set; }

        private ImageSource FormatCover(byte[] data, out string base64String)
        {
            if (data == null)
            {
                base64String = "";
                return null;
            }
            BitmapImage ret = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(data, 0, data.Length, true, true))
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
            {
                System.Drawing.Bitmap bmp;
                if (Math.Max(image.Height, image.Width) > FormatSize)
                {
                    if (image.Width > image.Height)
                    {
                        var newWidth = (int)FormatSize;
                        var newHeight = (int)Math.Round(image.Height * ((float)FormatSize / image.Width));
                        bmp = new System.Drawing.Bitmap(image, newWidth, newHeight);
                    }
                    else
                    {
                        var newHeight = (int)FormatSize;
                        var newWidth = (int)Math.Round(image.Width * ((float)FormatSize / image.Height));
                        bmp = new System.Drawing.Bitmap(image, newWidth, newHeight);
                    }
                }
                else
                {
                    bmp = new System.Drawing.Bitmap(image, image.Width, image.Height);
                }
                CoverSize = new Size(bmp.Width, bmp.Height);
                using (var newms = new MemoryStream())
                {
                    bmp.Save(newms, System.Drawing.Imaging.ImageFormat.Png);
                    newms.Position = 0;
                    base64String = Convert.ToBase64String(newms.GetBuffer());
                    newms.Position = 0;
                    ret.BeginInit();
                    ret.StreamSource = newms;
                    ret.CacheOption = BitmapCacheOption.OnLoad;
                    ret.EndInit();
                }
            }
            return ret;
        }
        public void LoadCover()
        {
            if (CoverPathType == CoverPathType.FromImageFile)
                using (var fs = File.OpenRead(CoverPath))
                {
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, (int)fs.Length);
                    Cover = FormatCover(data, out string base64);
                }
            else if (CoverPathType == CoverPathType.FromAudioFile)
                using (var t = TagLib.File.Create(CoverPath))
                {
                    byte[] data = t.Tag.Pictures[0].Data.Data;
                    Cover = FormatCover(data, out string base64);
                }
            else
                return;
        }

        public static SongEntity CreateFromFile(string fileName)
        {
            SongEntity ret;
            if (TryFindOrCreateEntity(fileName, out ret)) return ret;
            using (var w = CSCore.Codecs.CodecFactory.Instance.GetCodec(fileName))
            using (var t = TagLib.File.Create(fileName, TagLib.ReadStyle.None))
            {
                var tag = t.Tag;
                var f = new FileInfo(fileName);
                ret.Title = string.IsNullOrEmpty(tag.Title) ? f.Name : tag.Title;
                ret.Track = tag.Track;
                ret.Year = tag.Year;
                ret.Length = w.GetLength();
                ret.FileLastModded = f.LastWriteTimeUtc;
                TryFindOrCreateEntity(tag.FirstGenre, out GenreEntity genre);
                ret.GenreEntity = genre;
                if (tag.Pictures.Length >= 1)
                {
                    ret.CoverPath = f.FullName;
                    ret.CoverPathType = CoverPathType.FromAudioFile;
                }
                else if (TryFindCover(f, tag, out string path))
                {
                    ret.CoverPath = path;
                    ret.CoverPathType = CoverPathType.FromImageFile;
                }
                if (!AlbumEntity.TryFindOrCreateAlbum(tag.Album, SplitNamesToEntity<PerformerEntity>(tag.FirstAlbumArtist), out AlbumEntity album))
                {
                    if (!album.GenreEntities.Contains(ret.GenreEntity))
                        album.GenreEntities.Add(ret.GenreEntity);

                    album.CoverPath = ret.CoverPath;
                    album.CoverPathType = ret.CoverPathType;
                    album.CoverTrack = tag.Track;

                }
                else
                {
                    if (tag.Track < album.CoverTrack)
                    {
                        album.CoverPath = ret.CoverPath;
                        album.CoverPathType = ret.CoverPathType;
                        album.CoverTrack = tag.Track;
                    }
                }
                ret.AlbumEntity = album;
                ret.ArtistEntities = SplitNamesToEntity<PerformerEntity>(tag.FirstPerformer);

            }
            return ret;
        }
        static bool TryFindCover(FileInfo file, TagLib.Tag tag, out string path,
            string possibleName = "Cover;cover;AlbumArt;", string possibleEx = ".png;.jpg;.jpeg")
        {
            var extensions = possibleEx.Split(new char[]{';'},StringSplitOptions.RemoveEmptyEntries);
            var names = possibleName.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var f in file.Directory.EnumerateFiles().Where(x => extensions.Contains(x.Extension)))
            {
                if (names.Contains(f.Name))
                {
                    path = f.FullName;
                    return true;
                }
            }
            path = "";
            return false;
        }

        public override int GetHashCode()
        {
            if (Id != -1)
                return Id;
            return (Id = (DateTime.UtcNow.GetHashCode() ^ Name.GetHashCode()));
        }

        /*public override byte[] GetBinaryObjectBytes()
        {
            byte[] idBytes = BitConverter.GetBytes(Id),         //int
                   nameBytes = Encoding.UTF8.GetBytes(Name),    //string
                   titleBytes = Encoding.UTF8.GetBytes(Title),  //string
                   albumIdBytes = new byte[4],                  //int
                   artistsBytes = new byte[4],                  //ints
                   genreIdBytes = new byte[4],                  //int
                   trackBytes = new byte[4],                    //uint
                   yearBytes = new byte[4],                     //uint
                   lengthBytes = new byte[8],                   //long
                   lastModifiedBytes = new byte[8],             //long
                   coverPathBytes = new byte[4],                //string
                   coverType = new byte[4],                     //int
                   coverSize = new byte[8];                     //double int

        }*/
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

        public byte[] CoverData { get; set; }
        public CoverPathType CoverPathType { get; set; } = CoverPathType.NoneCover;
        public string CoverPath { get; set; }
        public ImageSource Cover { get; set; }
        public Size CoverSize { get; set; }
        public uint CoverTrack { get; set; }
        public override int GetHashCode()
        {
            if (Id != -1)
                return Id;
            return (Id = Name.GetHashCode() ^ Artists.GetHashCode());
        }
        static double FormatSize { get; set; } = 500d;
        private ImageSource FormatCover(byte[] data, out byte[] formatteddata)
        {
            if (data == null)
            {
                formatteddata = null;
                return null;
            }
            BitmapImage ret = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(data, 0, data.Length, true, true))
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
            {
                System.Drawing.Bitmap bmp;
                if (Math.Max(image.Height, image.Width) > FormatSize)
                {
                    if (image.Width > image.Height)
                    {
                        var newWidth = (int)FormatSize;
                        var newHeight = (int)Math.Round(image.Height * ((float)FormatSize / image.Width));
                        bmp = new System.Drawing.Bitmap(image, newWidth, newHeight);
                    }
                    else
                    {
                        var newHeight = (int)FormatSize;
                        var newWidth = (int)Math.Round(image.Width * ((float)FormatSize / image.Height));
                        bmp = new System.Drawing.Bitmap(image, newWidth, newHeight);
                    }
                }
                else
                {
                    bmp = new System.Drawing.Bitmap(image, image.Width, image.Height);
                }
                CoverSize = new Size(bmp.Width, bmp.Height);
                using (var newms = new MemoryStream())
                {
                    bmp.Save(newms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    newms.Position = 0;
                    formatteddata = newms.GetBuffer();
                    newms.Position = 0;
                    ret.BeginInit();
                    ret.StreamSource = newms;
                    ret.CacheOption = BitmapCacheOption.OnLoad;
                    ret.EndInit();
                }
            }
            return ret;
        }
        public void LoadCover()
        {
            if (CoverData!=null)
            {
                Cover = FormatCover(CoverData, out byte[] data);
            }
            else if (CoverPathType == CoverPathType.FromImageFile)
                using (var fs = File.OpenRead(CoverPath))
                {
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, (int)fs.Length);
                    Cover = FormatCover(data, out byte[] data2);
                    CoverData = data2;
                }
            else if (CoverPathType == CoverPathType.FromAudioFile)
                using(var t = TagLib.File.Create(CoverPath))
                {
                    byte[] data = t.Tag.Pictures[0].Data.Data;
                    Cover = FormatCover(data, out byte[] data2);
                    CoverData = data2;
                }
            else
                return;
        }
        public static bool TryFindOrCreateAlbum(string name, List<PerformerEntity> performers, out AlbumEntity entity)
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
            {
                return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            if (Id != -1)
                return Id;
            return (Id =  Name.GetHashCode());
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
                TryFindOrCreateEntity(n.Trim(), out T entity);
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
