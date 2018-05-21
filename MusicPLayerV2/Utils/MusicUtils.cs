using CSCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace MusicPLayerV2.Utils
{
    /// <summary>
    /// 音樂項目
    /// </summary>
    public class MusicItem: INotifyPropertyChanged
    {
        #region 內部變數

        string _path;
        string _title;
        string _album;
        string _artist;
        string _albumArtist;
        string _pictureBase64;
        DateTime _fileLastModded;
        uint _year;
        uint _track;
        string _genre;
        TimeSpan _length;
        BitmapImage _picture;

        #endregion

        #region 屬性

        /// <summary>
        /// 音樂檔案路徑
        /// </summary>
        public string Path { get => _path; set => _path = value; }

        /// <summary>
        /// 音樂標題
        /// (空白標題會直接引用檔名)
        /// </summary>
        public string Title
        {
            get => string.IsNullOrWhiteSpace(_title) ? new FileInfo(Path).Name : _title;
            set => _title = value;
        }

        /// <summary>
        /// 音樂專輯
        /// </summary>
        public string Album
        {
            get => string.IsNullOrWhiteSpace(_album) ? UnknowItem._album : _album;
            set => _album = value;
        }

        /// <summary>
        /// 音樂演出者
        /// </summary>
        public string Artist
        {
            get => string.IsNullOrWhiteSpace(_artist) ? UnknowItem._artist : _artist;
            set => _artist = value;
        }

        /// <summary>
        /// 音樂專輯演出者
        /// </summary>
        public string AlbumArtist
        {
            get => string.IsNullOrWhiteSpace(_albumArtist) ? UnknowItem._albumArtist : _albumArtist;
            set => _albumArtist = value;
        }

        /// <summary>
        /// 音樂分類
        /// </summary>
        public string Genre
        {
            get => string.IsNullOrWhiteSpace(_genre) ? UnknowItem._genre : _genre;
            set => _genre = value;
        }

        /// <summary>
        /// 音樂年分
        /// </summary>
        public uint Year { get => _year; set => _year = value; }

        /// <summary>
        /// 音樂軌數
        /// </summary>
        public uint Track { get => _track; set => _track = value; }

        /// <summary>
        /// 檔案最後修改日期
        /// </summary>
        public DateTime FileLastModded { get => _fileLastModded; set => _fileLastModded = value; }

        /// <summary>
        /// 音樂長度
        /// </summary>
        public TimeSpan Length { get => _length; set => _length = value; }

        /// <summary>
        /// 音樂長度字串
        /// 給懶得打 ToString(@"mm\:ss") 人用的
        /// </summary>
        public string LengthString => Length.ToString(@"mm\:ss");

        /// <summary>
        /// 音樂專輯圖片字串
        /// (Base64)
        /// </summary>
        public string PictureBase64
        {
            get => string.IsNullOrWhiteSpace(_pictureBase64) ? null : _pictureBase64;
            set => _pictureBase64 = value;
        }

        /// <summary>
        /// 音樂專輯圖片
        /// </summary>
        [XmlIgnore]
        public BitmapImage Picture
        {
            get => _picture;
            set => _picture = value;
        }

        bool _isNowPlaying;
        public bool IsNowPlaying { get=> _isNowPlaying;
            set { _isNowPlaying = value; NotifyPropertyChanged(nameof(IsNowPlaying)); } }

        /// <summary>
        /// 未知的音樂項目
        /// </summary>
        public static MusicItem UnknowItem => new MusicItem
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

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            if (Path != (obj as MusicItem).Path) return false;
            return true;
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
        #endregion

        #region 靜態函式

        /// <summary>
        /// 影像資料轉 BitmapImage
        /// </summary>
        /// <param name="data">影像資料</param>
        /// <returns></returns>
        public static BitmapImage ImageData2BitmapImage(byte[] data)
        {
            if (data == null)
                return null;
            BitmapImage ret = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(data))
            {
                ret.BeginInit();
                ret.StreamSource = ms;
                ret.CacheOption = BitmapCacheOption.OnLoad;
                ret.EndInit();
            }
            return ret;
        }

        /// <summary>
        /// 讀取檔案回傳
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>音樂項目</returns>
        public static MusicItem CreatFromFile(string fileName, bool loadImage = true)
        {

            MusicItem ret;
            TagLib.ReadStyle rs = loadImage?TagLib.ReadStyle.Average:TagLib.ReadStyle.None;
            using (var t = TagLib.File.Create(fileName,rs))
            {
                var tag = t.Tag;
                BitmapImage pic=null;
                if (loadImage)
                {
                    if (tag.Pictures.Length <= 0)
                    {
                        var picPath =
                            new FileInfo(fileName).Directory.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                            .Where(x => x.Name.Contains("Cover"))
                            .Where(x => x.Name.EndsWith(".png")
                            || x.Name.EndsWith(".jpg")
                            || x.Name.EndsWith(".PNG")
                            || x.Name.EndsWith(".JPG")
                            || x.Name.EndsWith(".jpeg")
                            || x.Name.EndsWith(".JPEG")).Select(x => x.FullName).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(picPath))
                            pic = new BitmapImage(new Uri(picPath));
                    }
                    else
                        pic = ImageData2BitmapImage(tag.Pictures[0].Data.Data);
                }

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
                    Length = CSCore.Codecs.CodecFactory.Instance.GetCodec(fileName).GetLength(),
                    Picture = pic
                };
            }
            return ret;
        }

        #endregion
    }
    public struct LyricWithTime
    {
        TimeSpan _time;
        string _lyric;

        public string Lyric { get => _lyric; set => _lyric = value; }
        public TimeSpan Time { get => _time; set => _time = value; }

        public override string ToString()
        {
            return _lyric;
        }
    }
}
