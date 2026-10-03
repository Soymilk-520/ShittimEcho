using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShittimEcho.Core.Audio;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Core.Music
{
    /// <summary>
    /// 音乐播放管理器。
    ///
    /// MusicLibrary 负责管理“有哪些音乐”。
    /// MusicManager 负责管理“如何播放音乐”。
    /// </summary>
    public sealed class MusicManager : IDisposable
    {
        private readonly StartupLogger _logger;
        private readonly AudioCore _audioCore;
        private readonly MusicLibrary _musicLibrary;

        private readonly Random _random =
            new Random();

        private List<MusicTrack> _playlist =
            new List<MusicTrack>();

        private int _currentIndex = -1;

        private bool _isPlaying;
        private bool _disposed;

        // 防止 Stop() 引起的 PlaybackCompleted
        // 重新触发自动切歌。
        private bool _isStopping;

        // 音乐播放器音量。
        // 0.0f = 0%
        // 1.0f = 100%
        private float _volume =
            1.0f;

        public MusicManager(
            StartupLogger logger,
            AudioCore audioCore,
            MusicLibrary musicLibrary)
        {
            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));

            _audioCore =
                audioCore ?? throw new ArgumentNullException(
                    nameof(audioCore));

            _musicLibrary =
                musicLibrary ?? throw new ArgumentNullException(
                    nameof(musicLibrary));

            _audioCore.PlaybackCompleted +=
                OnPlaybackCompleted;

            _logger.Write(
                "Music Manager created.");
        }

        /// <summary>
        /// 当前播放模式。
        /// </summary>
        public MusicPlayMode PlayMode { get; set; } =
            MusicPlayMode.LoopPlaylist;

        /// <summary>
        /// 当前是否正在播放。
        /// </summary>
        public bool IsPlaying =>
            _isPlaying;

        /// <summary>
        /// 当前播放的音乐。
        /// </summary>
        public MusicTrack? CurrentTrack
        {
            get
            {
                if (_currentIndex < 0 ||
                    _currentIndex >= _playlist.Count)
                {
                    return null;
                }

                return _playlist[_currentIndex];
            }
        }

        /// <summary>
        /// 从 MusicLibrary 建立播放列表。
        /// </summary>
        public void RefreshPlaylist()
        {
            _playlist =
                _musicLibrary
                    .GetEnabledTracks()
                    .OrderBy(track => track.SortOrder)
                    .ToList();

            _logger.Write(
                $"Music Manager playlist refreshed. " +
                $"Tracks={_playlist.Count}");

            if (_playlist.Count == 0)
            {
                _currentIndex = -1;

                _logger.Write(
                    "Music Manager playlist is empty.");

                return;
            }

            if (_currentIndex < 0 ||
                _currentIndex >= _playlist.Count)
            {
                _currentIndex = 0;

                _logger.Write(
                    "Music Manager current index reset to 0.");
            }
        }

        /// <summary>
        /// 设置音乐播放器音量。
        ///
        /// 0.0f = 静音
        /// 1.0f = 100%
        ///
        /// 该设置只影响音乐播放器自身，
        /// 不修改 Windows 系统主音量。
        /// </summary>
        public void SetVolume(
            float volume)
        {
            if (float.IsNaN(volume) ||
                float.IsInfinity(volume))
            {
                volume =
                    1.0f;
            }

            volume =
                Math.Clamp(
                    volume,
                    0.0f,
                    1.0f);

            _volume =
                volume;

            _logger.Write(
                $"Music Manager SetVolume(): " +
                $"Volume={_volume}");

            _audioCore.SetVolume(
                _volume);
        }

        /// <summary>
        /// 开始播放当前播放列表。
        /// </summary>
        public void Start()
        {
            ThrowIfDisposed();

            _isStopping = false;

            // 咸鱼喵喵锁屏音乐默认采用随机播放。
            // App.xaml.cs 无需修改；进入 Start() 时统一确保随机模式。
            PlayMode = MusicPlayMode.Random;

            RefreshPlaylist();

            if (_playlist.Count == 0)
            {
                _logger.Write(
                    "Music Manager Start(): " +
                    "no enabled tracks available.");

                _isPlaying = false;

                return;
            }

            if (_currentIndex < 0 ||
                _currentIndex >= _playlist.Count)
            {
                _currentIndex = 0;
            }

            // 首次启动时随机选择播放列表中的一首，
            // 避免每次锁屏都固定从第一首开始。
            _currentIndex =
                _random.Next(
                    0,
                    _playlist.Count);

            PlayCurrentTrack();
        }

        /// <summary>
        /// 停止播放。
        /// </summary>
        public void Stop()
        {
            ThrowIfDisposed();

            _logger.Write(
                "Music Manager Stop() called.");

            // 必须先进入停止状态。
            // 这样 AudioCore.Stop() 如果触发
            // PlaybackCompleted，也不会自动切歌。
            _isStopping = true;
            _isPlaying = false;

            _audioCore.Stop();

            _logger.Write(
                "Music Manager playback stopped.");
        }

        /// <summary>
        /// 播放下一首。
        /// </summary>
        public void Next()
        {
            ThrowIfDisposed();

            if (_isStopping)
            {
                _logger.Write(
                    "Music Manager Next(): " +
                    "ignored because manager is stopping.");

                return;
            }

            if (_playlist.Count == 0)
            {
                RefreshPlaylist();
            }

            if (_playlist.Count == 0)
            {
                _logger.Write(
                    "Music Manager Next(): " +
                    "playlist is empty.");

                return;
            }

            switch (PlayMode)
            {
                case MusicPlayMode.Random:
                    _currentIndex =
                        _random.Next(
                            0,
                            _playlist.Count);

                    break;

                default:
                    _currentIndex++;

                    if (_currentIndex >=
                        _playlist.Count)
                    {
                        _currentIndex = 0;
                    }

                    break;
            }

            PlayCurrentTrack();
        }

        /// <summary>
        /// 播放指定索引的音乐。
        /// </summary>
        public void Play(int index)
        {
            ThrowIfDisposed();

            _isStopping = false;

            if (_playlist.Count == 0)
            {
                RefreshPlaylist();
            }

            if (index < 0 ||
                index >= _playlist.Count)
            {
                _logger.Write(
                    $"Music Manager Play(): " +
                    $"invalid index {index}.");

                return;
            }

            _currentIndex = index;

            PlayCurrentTrack();
        }

        private void PlayCurrentTrack()
        {
            if (_isStopping)
            {
                _logger.Write(
                    "Music Manager PlayCurrentTrack(): " +
                    "ignored because manager is stopping.");

                return;
            }

            if (_currentIndex < 0 ||
                _currentIndex >= _playlist.Count)
            {
                _logger.Write(
                    "Music Manager PlayCurrentTrack(): " +
                    "current index is outside playlist range.");

                return;
            }

            MusicTrack track =
                _playlist[_currentIndex];

            if (!File.Exists(track.FilePath))
            {
                _logger.Write(
                    $"Music Manager: " +
                    $"file does not exist: {track.FilePath}");

                MoveToNextAvailableTrack();

                return;
            }

            _logger.Write(
                $"Music Manager: " +
                $"playing track: {track.DisplayName}");

            try
            {
                // 在真正开始播放当前音乐之前，
                // 将 MusicManager 当前保存的音量
                // 应用到 AudioEngine。
                _audioCore.SetVolume(
                    _volume);

                _audioCore.Play(
                    track.FilePath);

                _isPlaying = true;
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Music Manager: " +
                    $"failed to play track {track.DisplayName}: {ex}");

                MoveToNextAvailableTrack();
            }
        }

        private void MoveToNextAvailableTrack()
        {
            if (_isStopping)
            {
                _logger.Write(
                    "Music Manager MoveToNextAvailableTrack(): " +
                    "ignored because manager is stopping.");

                return;
            }

            if (_playlist.Count == 0)
            {
                _isPlaying = false;
                return;
            }

            int originalIndex =
                _currentIndex;

            for (int i = 0;
                 i < _playlist.Count;
                 i++)
            {
                if (PlayMode ==
                    MusicPlayMode.Random)
                {
                    _currentIndex =
                        _random.Next(
                            0,
                            _playlist.Count);
                }
                else
                {
                    _currentIndex++;

                    if (_currentIndex >=
                        _playlist.Count)
                    {
                        _currentIndex = 0;
                    }
                }

                MusicTrack candidate =
                    _playlist[_currentIndex];

                if (File.Exists(candidate.FilePath))
                {
                    PlayCurrentTrack();
                    return;
                }
            }

            _currentIndex =
                originalIndex;

            _isPlaying = false;

            _logger.Write(
                "Music Manager: " +
                "no playable tracks found.");
        }

        private void OnPlaybackCompleted(
            object? sender,
            EventArgs e)
        {
            if (_disposed)
                return;

            _logger.Write(
                "Music Manager: " +
                "playback completed.");

            // Stop() 导致的完成事件不能触发自动切歌。
            if (_isStopping)
            {
                _logger.Write(
                    "Music Manager: " +
                    "playback completion ignored " +
                    "because manager is stopping.");

                return;
            }

            _isPlaying = false;

            switch (PlayMode)
            {
                case MusicPlayMode.Single:
                    _logger.Write(
                        "Music Manager: " +
                        "single-track mode, playback stopped.");

                    return;

                case MusicPlayMode.LoopTrack:
                    _logger.Write(
                        "Music Manager: " +
                        "loop-track mode, replaying current track.");

                    PlayCurrentTrack();

                    return;

                case MusicPlayMode.Random:
                case MusicPlayMode.LoopPlaylist:
                default:
                    Next();

                    return;
            }
        }


        /*
         * ============================================================
         * 锁屏音乐淡出
         * ============================================================
         *
         * 解锁时先进入停止状态，防止淡出期间触发自动切歌。
         * 等 AudioCore 完成淡出并停止当前播放器后再返回。
         */
        public async Task FadeOutAsync(
            int durationMilliseconds = 1500,
            int steps = 30,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!_isPlaying)
            {
                _logger.Write(
                    "Music Manager FadeOutAsync(): " +
                    "no music is currently playing.");

                return;
            }

            _logger.Write(
                $"Music Manager FadeOutAsync() started. " +
                $"Duration={durationMilliseconds}ms, " +
                $"Steps={steps}");

            // 必须先进入停止状态。
            // 这样淡出完成后的播放器停止事件不会触发下一首随机音乐。
            _isStopping = true;
            _isPlaying = false;

            await _audioCore
                .FadeOutAsync(
                    durationMilliseconds,
                    steps,
                    cancellationToken)
                .ConfigureAwait(false);

            _logger.Write(
                "Music Manager FadeOutAsync() completed.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(MusicManager));
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _logger.Write(
                "Music Manager disposing.");

            _audioCore.PlaybackCompleted -=
                OnPlaybackCompleted;

            _isStopping = true;
            _isPlaying = false;

            _audioCore.Stop();

            _disposed = true;

            _logger.Write(
                "Music Manager disposed.");
        }
    }

    /// <summary>
    /// 音乐播放模式。
    /// </summary>
    public enum MusicPlayMode
    {
        /// <summary>
        /// 播放列表循环。
        /// </summary>
        LoopPlaylist,

        /// <summary>
        /// 单曲循环。
        /// </summary>
        LoopTrack,

        /// <summary>
        /// 播放当前音乐后停止。
        /// </summary>
        Single,

        /// <summary>
        /// 随机播放。
        /// </summary>
        Random
    }
}