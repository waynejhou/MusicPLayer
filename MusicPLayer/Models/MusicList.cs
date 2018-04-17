using CSCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using MusicPLayer.Utils;

namespace MusicPLayer.Models
{
    public partial class MusicList
    {
        ObservableCollection<MusicItem> _list = new ObservableCollection<MusicItem>();
        public ObservableCollection<MusicItem> List { get => _list; set => _list = value; }
        int _nowPlayIndex = 0;
        NextOneMode _nextModeType = NextOneMode.RepeatList;
        Stack<string> _playingHistory = new Stack<string>();
        bool _isGetLast = false;
        public bool CanGetLast => ((NextModeType == NextOneMode.Random) && _playingHistory.Count > 0) || ((NextModeType == NextOneMode.RepeatList) && List.Count > 1 || (NextModeType == NextOneMode.RepeatOne));
        public bool CanGetNext => ((NextModeType == NextOneMode.Random) && _playingHistory.Count > 0) ||(NextModeType==NextOneMode.RepeatList)|| (NextModeType == NextOneMode.RepeatOne);
        public int NowPlayIndex
        {
            get => _nowPlayIndex;
            set
            {
                if (!_isGetLast)
                {
                    if (value != NowPlayIndex)
                        _playingHistory.Push(List[NowPlayIndex].Path);
                } else
                    _isGetLast = false;
                _nowPlayIndex = value;
            }
        }

        public NextOneMode NextModeType { get => _nextModeType; set => _nextModeType = value; }

        Random _rnd = new Random();
        public string GetNextMusic()
        {
            switch (NextModeType)
            {
                case NextOneMode.Random:
                    var r = 0;
                    while (
                        ((r = _rnd.Next(0, List.Count - 1)) == NowPlayIndex)
                        ) { }
                    return List[r].Path;
                case NextOneMode.RepeatList:
                    var n = NowPlayIndex + 1;
                    return List[n >= List.Count ? 0 : n].Path;
                case NextOneMode.RepeatOne:
                    return List[NowPlayIndex].Path;
                default:
                    throw new FormatException();
            }
        }
        public string GetLastMusic()
        {
            switch (NextModeType)
            {
                case NextOneMode.Random:
                    _isGetLast = true;
                    return _playingHistory.Pop();
                case NextOneMode.RepeatList:
                    var n = NowPlayIndex - 1;
                    return List[n < 0 ? List.Count - 1 : n].Path;
                case NextOneMode.RepeatOne:
                    return List[NowPlayIndex].Path;
                default:
                    throw new FormatException();
            }
        }

        public void SaveListAsXml(string fileName)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(typeof(MusicList));
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, this);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(MusicList)}.SaveSettingAsXml: {ex}");
            }
        }
        public static MusicList LoadListFromXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { throw new FileNotFoundException("NotFound"); }

            MusicList db = new MusicList();
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;
                using (StringReader read = new StringReader(xmlString))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MusicList));
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        db = (MusicList)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(MusicList)}.LoadSettingFromXml: {ex}");
            }
            return db;
        }
    }
    public enum NextOneMode { Random, RepeatList, RepeatOne }
}
