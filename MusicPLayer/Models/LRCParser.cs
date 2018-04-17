using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MusicPLayer.Models
{
    class LRCParser
    {
        List<LyricWithTime> _lyrics = new List<LyricWithTime>();
        string _fileName = "";
        bool isLoaded = false;
        public bool IsLoaded{ get => isLoaded; set => isLoaded = value; }

        public List<LyricWithTime> Lyrics
        {
            get => _lyrics;
        }

        public string FileName
        {
            get => _fileName; set
            {
                if (File.Exists(value))
                    IsLoaded = true;
                else
                {
                    IsLoaded = false;
                    _lyrics.Clear();
                    return;
                }
                _lyrics.Clear();
                _fileName = value;
                var lines = File.ReadAllLines(_fileName);
                Regex timeTag = new Regex(@"\[[0-9]*\:[0-9]*\.[0-9]*\]");
                foreach(var s in lines)
                {
                    var ms = timeTag.Matches(s);
                    var sr = timeTag.Replace(s, "");
                    
                    foreach (var ss in ms)
                    {
                        var sss = (ss as Match).Value.Trim("[]".ToCharArray());
                        if (TimeSpan.TryParseExact(sss, @"mm\:ss\.f", null, out TimeSpan result))
                            AddLyric(result, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:ss\.ff", null, out TimeSpan result1))
                            AddLyric(result1, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:ss\.fff", null, out TimeSpan result2))
                            AddLyric(result2, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:ss\.fffff", null, out TimeSpan result3))
                            AddLyric(result3, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:ss\.ffffff", null, out TimeSpan result4))
                            AddLyric(result4, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:ss\.fffffff", null, out TimeSpan result5))
                            AddLyric(result5, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:ss\.f", null, out TimeSpan result7))
                            AddLyric(result7, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:ss\.ff", null, out TimeSpan result8))
                            AddLyric(result8, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:ss\.fff", null, out TimeSpan result9))
                            AddLyric(result9, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:ss\.fffff", null, out TimeSpan result10))
                            AddLyric(result10, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:ss\.ffffff", null, out TimeSpan result11))
                            AddLyric(result11, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:ss\.fffffff", null, out TimeSpan result12))
                            AddLyric(result12, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:s\.f", null, out TimeSpan result13))
                            AddLyric(result13, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:s\.ff", null, out TimeSpan result14))
                            AddLyric(result14, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:s\.fff", null, out TimeSpan result15))
                            AddLyric(result15, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:s\.fffff", null, out TimeSpan result16))
                            AddLyric(result16, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:s\.ffffff", null, out TimeSpan result17))
                            AddLyric(result17, sr);
                        else if (TimeSpan.TryParseExact(sss, @"mm\:s\.fffffff", null, out TimeSpan result18))
                            AddLyric(result18, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:s\.f", null, out TimeSpan result19))
                            AddLyric(result19, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:s\.ff", null, out TimeSpan result20))
                            AddLyric(result10, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:s\.fff", null, out TimeSpan result21))
                            AddLyric(result21, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:s\.fffff", null, out TimeSpan result22))
                            AddLyric(result22, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:s\.ffffff", null, out TimeSpan result23))
                            AddLyric(result23, sr);
                        else if (TimeSpan.TryParseExact(sss, @"m\:s\.fffffff", null, out TimeSpan result24))
                            AddLyric(result24, sr);

                        else
                            throw new FormatException("...");
                    }

                    _lyrics.Add(new LyricWithTime() { Time = TimeSpan.MaxValue, Lyric = "" });
                }
                _lyrics = _lyrics.OrderBy(x => x.Time, new TimespanCompare()).ToList();

            }
            
        }

        void AddLyric(TimeSpan time, string lyric)
        {
            if (_lyrics.Select(x => x.Time).Contains(time)) {
                var lt = _lyrics[_lyrics.FindIndex(x => x.Time == time)];
                _lyrics[_lyrics.FindIndex(x => x.Time == time)] =
                    new LyricWithTime() { Time = lt.Time, Lyric = lt.Lyric + "\n" + lyric };
            }
            else
                _lyrics.Add(new LyricWithTime() { Time = time, Lyric = lyric });
        }

        internal object SearchLyricFromTime(TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        class TimespanCompare : IComparer<TimeSpan>
        {
            public int Compare(TimeSpan x, TimeSpan y)
            {
                return TimeSpan.Compare(x, y);
            }
        }

        public LyricWithTime GetLyricFromTime(TimeSpan timeSpan)
        {
            var idx = GetLyricIdxFromTime(timeSpan);
            if (idx < Lyrics.Count())
                return Lyrics[idx];
            else
                return new LyricWithTime();
        }
        public LyricWithTime GetNextLyricFromTime(TimeSpan timeSpan)
        {
            var idx = GetLyricIdxFromTime(timeSpan);
            if (idx+1 < Lyrics.Count())
                return Lyrics[idx+1];
            else
                return new LyricWithTime();
        }
        public int GetLyricIdxFromTime(TimeSpan timeSpan)
        {
            for(int i = 0; i <= Lyrics.Count(); i++)
                if (timeSpan <= Lyrics[i].Time)
                    return Math.Max(i-1,0);
            return 0;
        }
    }
    struct LyricWithTime
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
