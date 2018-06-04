using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using MusicPLayerV2.Utils;

namespace MusicPLayerV2.Models
{
    class LRCParser
    {
        string _fileName = "";
        bool isLoaded = false;
        public bool IsLoaded{ get => isLoaded; set => isLoaded = value; }

        public List<LyricWithTime> Lyrics { get; private set; } = new List<LyricWithTime>();

        public string FileName
        {
            get => _fileName; set
            {
                if (File.Exists(value))
                    IsLoaded = true;
                else
                {
                    IsLoaded = false;
                    Lyrics.Clear();
                    return;
                }
                Lyrics.Clear();
                _fileName = value;
                var lines = File.ReadAllLines(_fileName);
                Regex timeTag = new Regex(@"\[[0-9]*\:[0-9]*\.[0-9]*\]");
                foreach(var s in lines)
                {
                    var ms = timeTag.Matches(s);
                    var sr = timeTag.Replace(s, "");
                    sr = sr.Replace("【", "\n【").Replace("〖", "\n〖");
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

                    Lyrics.Add(new LyricWithTime() { Time = TimeSpan.MaxValue, Lyric = "" });
                }
                Lyrics = Lyrics.OrderBy(x => x.Time).ToList();
            }
        }


        void AddLyric(TimeSpan time, string lyric)
        {
            if (Lyrics.Select(x => x.Time).Contains(time)) {
                var lt = Lyrics[Lyrics.FindIndex(x => x.Time == time)];
                Lyrics[Lyrics.FindIndex(x => x.Time == time)] =
                    new LyricWithTime() { Time = lt.Time, Lyric = lt.Lyric + "\n" + lyric };
            }
            else
                Lyrics.Add(new LyricWithTime() { Time = time, Lyric = lyric });
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
                return Lyrics[idx];
        }
        public LyricWithTime GetPrevLyricFromTime(TimeSpan timeSpan)
        {
            var idx = GetLyricIdxFromTime(timeSpan);
            if (idx - 1 >= 0)
                return Lyrics[idx - 1];
            else
                return Lyrics[0];
        }
        public int GetLyricIdxFromTime(TimeSpan timeSpan)
        {
            for(int i = 0; i < Lyrics.Count(); i++)
                if (timeSpan <= Lyrics[i].Time)
                    return Math.Max(i-1,0);
            return -1;
        }
        public static List<LyricWithTime> NoLyricMessage { get; } = new List<LyricWithTime>()
        {
            new LyricWithTime{ Time=TimeSpan.Zero, Lyric="No Lyric Found!"},
            new LyricWithTime{ Time=TimeSpan.MaxValue, Lyric=""}
        };
    }

}
