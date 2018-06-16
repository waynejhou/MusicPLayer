using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TagLib;
using MusicPLayer.Utils;

namespace MusicPLayer.Models
{
    /// <summary>
    /// 音樂播放整合物件
    /// </summary>
    public class MusicPlayer :IDisposable
    {
        #region 內部變數

        IWaveSource _waveSource = null;
        ISoundOut _soundOut = null;
        MusicItem _nowPlayingItem = MusicItem.UnknowItem;
        float _volume = 0f;
        TimeSpan _position = TimeSpan.Zero;
        Thread _wavePostionUpdThd;
        bool _manualStop = false;
        #endregion

        #region 屬性

        /// <summary>
        /// 現正撥放的音樂項目
        /// </summary>
        public MusicItem NowPlayingItem { get => _nowPlayingItem; set => _nowPlayingItem = value; }

        /// <summary>
        /// 是否有載入音樂
        /// </summary>
        public bool IsLoadded => !string.IsNullOrWhiteSpace(NowPlayingItem.Path);

        /// <summary>
        /// 播放音量
        /// (0.0~1.0)
        /// </summary>
        public float Volume
        {
            get => (float)Math.Pow(_volume, -2);
            set
            {
                _volume = (float)Math.Pow(Math.Max(0, Math.Min(1, value)), 2);
                if (_soundOut != null)
                    _soundOut.Volume = _volume;
            }
        }

        /// <summary>
        /// 音樂撥放位置
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                if (_waveSource != null)
                    _position = _waveSource.GetPosition();
                return _position;
            }
            set
            {
                if (_waveSource != null)
                {
                    _waveSource.SetPosition(value);
                    _position = _waveSource.GetPosition();
                }
            }
        }

        /// <summary>
        /// 現正撥放狀態
        /// </summary>
        public PlaybackState PlaybackState => _soundOut != null ? _soundOut.PlaybackState : PlaybackState.Stopped;

        /// <summary>
        /// 人工中止
        /// </summary>
        public bool ManualStop { get => _manualStop; set => _manualStop = value; }

        #endregion

        #region 函式

        /// <summary>
        /// 播放音樂
        /// </summary>
        public void Play() => _soundOut?.Play();

        /// <summary>
        /// 回復音樂
        /// </summary>
        public void Resume() => _soundOut?.Resume();

        /// <summary>
        /// 暫停音樂
        /// </summary>
        public void Pause() => _soundOut?.Pause();

        /// <summary>
        /// 停止音樂
        /// </summary>
        public void Stop() {
            if (_soundOut != null)
            {
                if (PlaybackState != PlaybackState.Stopped)
                    _manualStop = true;
                _soundOut.Stop();
            }
        } 

        /// <summary>
        /// 讀取音樂檔案
        /// </summary>
        /// <param name="fileName">檔案路徑</param>
        public void Load(string fileName)
        {
            Stop();
            Dispose();
            NowPlayingItem = MusicItem.CreatFromFile(fileName);
            _waveSource = CodecFactory.Instance.GetCodec(fileName)
                .ToSampleSource()
                .ToStereo()
                .ToWaveSource();
            _soundOut = new WasapiOut() { Latency = 100 };
            _soundOut.Initialize(_waveSource);
            _soundOut.Volume = _volume;
            //_soundOut.Stopped += (object sender, PlaybackStoppedEventArgs e) => { StoppedEvent?.Invoke(this); };
            LoaddedEvent?.Invoke(this);
            Position = TimeSpan.Zero;
            Play();
            _wavePostionUpdThd = new Thread(() =>
            {
                TimeSpan last = TimeSpan.Zero;
                while (IsLoadded)
                {
                    //if(PlaybackState==PlaybackState.Stopped) StoppedEvent?.Invoke(this);
                    var newone = _waveSource.GetPosition();
                    if (newone != last)
                        WavePositionChangedEvent?.Invoke(this, newone);
                    while (_soundOut.PlaybackState != PlaybackState.Playing) { Thread.Sleep(1); }
                    Thread.Sleep(1);
                }
            });
            _wavePostionUpdThd.Start();
        }

        /// <summary>
        /// 關閉撥放器
        /// </summary>
        public void Dispose()
        {
            _wavePostionUpdThd?.Abort();
            _wavePostionUpdThd = null;
            NowPlayingItem = MusicItem.UnknowItem;
            _waveSource?.Dispose();
            _waveSource = null;
            _soundOut?.Dispose();
            _soundOut = null;
        }

        #endregion

        #region 靜態函式
        static public bool SupportCheck(string fileName)
        {
            var str = ((string)App.Current.Resources["Filter_AudioFile"]).Split('|')[1].Split(';');
            foreach(var s in str)
            {
                if (fileName.EndsWith(s.Remove(0, 1)))
                    return true;
            }
            return false;
        }
        #endregion

        #region 事件

        /// <summary>
        /// 讀取事件委派處理
        /// </summary>
        /// <param name="sender">事件當事者</param>
        public delegate void LoaddedEventHandler(object sender);

        /// <summary>
        /// 當音樂檔案讀取時觸發
        /// </summary>
        public event LoaddedEventHandler LoaddedEvent;

        /// <summary>
        /// 停止事件委派處理
        /// </summary>
        /// <param name="sender">事件當事者</param>
        /// <param name="e">是否人工中止/param>
        //public delegate void StoppedEventHandler(object sender);

        /// <summary>
        /// 當音樂停止時觸發
        /// </summary>
        //public event StoppedEventHandler StoppedEvent;

        /// <summary>
        /// 位置變化委派處理
        /// </summary>
        /// <param name="sender">事件當事者</param>
        public delegate void WavePositionChangedEventHandler(object sender, TimeSpan position);

        /// <summary>
        /// 當位置變化時觸發
        /// </summary>
        public event WavePositionChangedEventHandler WavePositionChangedEvent;

        #endregion
    }
}
