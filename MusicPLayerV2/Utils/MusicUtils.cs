using CSCore;
using System;
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
using System.Xml.Serialization;

namespace MusicPLayerV2.Utils
{
    /// <summary>
    /// 音樂項目
    /// </summary>
    public class MusicItem: INotifyPropertyChanged
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
                if(Math.Max(image.Height, image.Width)>imgSize)
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
            TagLib.ReadStyle rs = loadImage?TagLib.ReadStyle.Average:TagLib.ReadStyle.None;
            using (var t = TagLib.File.Create(fileName,rs))
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
                new FrameworkPropertyMetadata((DependencyObject obj, DependencyPropertyChangedEventArgs args)=>
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
}
