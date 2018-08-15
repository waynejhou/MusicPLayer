using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CSCore;
using CSCore.Codecs;
using LiteDB;

namespace MusicPLayerV2.Utils
{
    public static class MusicDatabase
    {
        static string _name = App.ExecuteFilePath+@"MusicDatabase.db";
        public static string Name { get => _name; set
            {
                if (value == _name)
                    return;
                Database.Dispose();
                Init();
            }
        }
        public static LiteDatabase Database { get; set; }
        public static LiteCollection<SongEntity> SongColle { get; set; }
        public static LiteCollection<AlbumEntity> AlbumColle { get; set; }
        public static LiteCollection<PerformerEntity> PerformerColle { get; set; }
        public static LiteCollection<GenreEntity> GenreColle { get; set; }
        public static LiteCollection<AlbumArtistRelationShip> AlbumArtistColle { get; set; }
        public static LiteCollection<SongArtistRelationShip> SongArtistColle { get; set; }
        public static LiteCollection<LibraryEntity> LibraryColle { get; set; }
        public static LiteCollection<LibrarySongRelationship> LibrarySongColle { get; set; }

        static MusicDatabase()
        {
            Init();
        }
        static void Init()
        {
            if (Directory.Exists(Name))
                throw new FileNotFoundException();
            Database = new LiteDatabase(Name);
            SongColle = Database.GetCollection<SongEntity>();
            AlbumColle = Database.GetCollection<AlbumEntity>();
            PerformerColle = Database.GetCollection<PerformerEntity>();
            GenreColle = Database.GetCollection<GenreEntity>();
            AlbumArtistColle = Database.GetCollection<AlbumArtistRelationShip>();
            SongArtistColle = Database.GetCollection<SongArtistRelationShip>();
            LibraryColle = Database.GetCollection<LibraryEntity>();
            LibrarySongColle = Database.GetCollection<LibrarySongRelationship>();
            SongColle.EnsureIndex(x => x.Path, true);
            AlbumColle.EnsureIndex(x => x.Name, true);
            PerformerColle.EnsureIndex(x => x.Name, true);
            GenreColle.EnsureIndex(x => x.Name, true);
            AlbumArtistColle.EnsureIndex(x => x.Name, true);
            SongArtistColle.EnsureIndex(x => x.Name, true);
            LibraryColle.EnsureIndex(x => x.Path, true);
            LibrarySongColle.EnsureIndex(x => x.Name, true);
        }
        static void Dispose()
        {
            Database.Dispose();
        }

        public static SongEntity CreateSongEntity(string fileName)
        {
            SongEntity ret;
            bool isNewOne = false;
            var f = new FileInfo(fileName);
            if ((ret = SongColle.FindOne(x => x.Path == fileName)) != null)
            {
                if (ret.FileLastModded >= f.LastWriteTime)
                {
                    Console.WriteLine("No");
                    return ret;
                }
                else
                {
                    Console.WriteLine("Update");
                    isNewOne = false;
                }
            }
            else
            {
                Console.WriteLine("insert");
                ret = new SongEntity();
                isNewOne = true;
            }
            ret.Path = fileName;
            using (var w = CodecFactory.Instance.GetCodec(fileName))
            using (var t = TagLib.File.Create(fileName, TagLib.ReadStyle.None))
            {
                var tag = t.Tag;
                ret.Title = string.IsNullOrEmpty(tag.Title) ? f.Name : tag.Title;
                ret.Track = tag.Track;
                ret.Year = tag.Year;
                ret.Length = w.GetLength();
                ret.FileLastModded = f.LastWriteTimeUtc;
                string genreString = tag.FirstGenre ?? "Unknown Genre";
                GenreEntity genre;
                if ((genre = GenreColle.FindOne(x => x.Name == genreString)) == null)
                {
                    genre = new GenreEntity() { Name = genreString };
                    GenreColle.Insert(genre);
                }
                ret.GenreId = genre.Id;

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
                else
                {
                    ret.CoverPath = null;
                    ret.CoverPathType = CoverPathType.NoneCover;
                }
                string albumString = tag.Album ?? "Unknown Album";
                AlbumEntity album;
                if ((album = AlbumColle.FindOne(x => x.Name == albumString)) == null)
                {
                    album = new AlbumEntity()
                    {
                        Name = albumString,
                        CoverPath = ret.CoverPath,
                        CoverPathType = ret.CoverPathType,
                        CoverTrack = tag.Track
                    };
                    AlbumColle.Insert(album);
                }
                else if (tag.Track < album.CoverTrack)
                {
                    album.CoverPath = ret.CoverPath;
                    album.CoverPathType = ret.CoverPathType;
                    album.CoverTrack = tag.Track;
                    AlbumColle.Update(album);
                }
                SplitPerformersToAlbumColle(album, tag.FirstAlbumArtist ?? "Unknown Performer");
                ret.AlbumId = album.Id;
                if (isNewOne)
                    SongColle.Insert(ret);
                else
                    SongColle.Update(ret);
                SplitPerformersToSongColle(ret, tag.FirstPerformer?? "Unknown Performer");
            }


            return ret;
        }
        static void SplitPerformersToAlbumColle(AlbumEntity entity, string namesString, string splitString = ",;|，、")
        {
            var names = namesString.Split(splitString.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var n in names)
            {
                var name = n.Trim();
                PerformerEntity performer;
                if ((performer = PerformerColle.FindOne(x => x.Name == name)) == null)
                {
                    performer = new PerformerEntity() { Name = name };
                    PerformerColle.Insert(performer);
                }
                AlbumArtistRelationShip R;
                if ((R = AlbumArtistColle.FindOne(x => x.Name == entity.Name+"-"+performer.Name)) == null)
                {
                    R = new AlbumArtistRelationShip() { Name = entity.Name + "-" + performer.Name, AlbumId = entity.Id, PerformerId = performer.Id };
                    AlbumArtistColle.Insert(R);
                }
            }
        }
        static void SplitPerformersToSongColle(SongEntity entity, string namesString, string splitString = ",;|，、")
        {
            var names = namesString.Split(splitString.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var n in names)
            {
                var name = n.Trim();
                PerformerEntity performer;
                if ((performer = PerformerColle.FindOne(x => x.Name == name)) == null)
                {
                    performer = new PerformerEntity() { Name = name };
                    PerformerColle.Insert(performer);
                }
                SongArtistRelationShip R;
                if ((R = SongArtistColle.FindOne(x => x.Name == entity.Path + "-" + performer.Name)) == null)
                {
                    R = new SongArtistRelationShip() { Name = entity.Path + "-" + performer.Name, SongId = entity.Id, PerformerId = performer.Id };
                    SongArtistColle.Insert(R);
                }
            }
        }
        static bool TryFindCover(FileInfo file, TagLib.Tag tag, out string path,
            string possibleName = "Cover;cover;AlbumArt;", string possibleEx = ".png;.jpg;.jpeg")
        {
            var extensions = possibleEx.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
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

        public static bool CheckFileSupported(string path)
        {
            return CodecFactory.Instance.GetSupportedFileExtensions().Contains(new FileInfo(path).Extension.TrimStart('.'));
        }

        public static byte[] FormatCover(byte[] data, int formattedSize, out Size newSize)
        {
            if (data == null)
            {
                newSize = Size.Empty;
                return null;
            }
            using (MemoryStream ms = new MemoryStream(data, 0, data.Length, true, true))
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
            {
                System.Drawing.Bitmap bmp;
                if (Math.Max(image.Height, image.Width) > formattedSize)
                {
                    if (image.Width > image.Height)
                    {
                        var newWidth = formattedSize;
                        var newHeight = (int)Math.Round(image.Height * ((float)formattedSize / image.Width));
                        bmp = new System.Drawing.Bitmap(image, newWidth, newHeight);
                    }
                    else
                    {
                        var newHeight = formattedSize;
                        var newWidth = (int)Math.Round(image.Width * ((float)formattedSize / image.Height));
                        bmp = new System.Drawing.Bitmap(image, newWidth, newHeight);
                    }
                }
                else
                {
                    bmp = new System.Drawing.Bitmap(image, image.Width, image.Height);
                }
                newSize = new Size(bmp.Width, bmp.Height);
                using (var newms = new MemoryStream())
                {
                    newms.Position = 0;
                    bmp.Save(newms, System.Drawing.Imaging.ImageFormat.Png);
                    return newms.GetBuffer();
                }
            }
        }
        public static void UpdateCover(SongEntity song)
        {
            LoadCover(song, 500);
            SongColle.Update(song);
        }
        public static void UpdateCover(AlbumEntity album)
        {
            LoadCover(album, 150);
            AlbumColle.Update(album);
        }
        static void LoadCover(ICover cover, int formattedSize)
        {
            if (cover.CoverPathType == CoverPathType.FromImageFile)
                using (var fs = File.OpenRead(cover.CoverPath))
                {
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, (int)fs.Length);
                    cover.CoverData = FormatCover(data, formattedSize, out Size s);
                    cover.CoverSizeHeight = (int)s.Height;
                    cover.CoverSizeWidth = (int)s.Width;
                }
            else if (cover.CoverPathType == CoverPathType.FromAudioFile)
                using (var t = TagLib.File.Create(cover.CoverPath))
                {
                    byte[] data = t.Tag.Pictures[0].Data.Data;
                    cover.CoverData = FormatCover(data, formattedSize, out Size s);
                    cover.CoverSizeHeight = (int)s.Height;
                    cover.CoverSizeWidth = (int)s.Width;
                }
        }
    }
}
