using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ShittimEcho.Core.Diagnostics;
using ShittimEcho.Core.Music;
using ShittimEcho.Core.Voice;

namespace ShittimEcho.Windows
{
    public sealed class SessionAudioController : IDisposable
    {
        private readonly StartupLogger _logger;
        private readonly MusicManager _musicManager;
        private readonly VoiceManager _voiceManager;

        private bool _started;
        private bool _disposed;

        public SessionAudioController(
            StartupLogger logger,
            MusicManager musicManager,
            VoiceManager voiceManager)
        {
            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));

            _musicManager =
                musicManager ?? throw new ArgumentNullException(
                    nameof(musicManager));

            _voiceManager =
                voiceManager ?? throw new ArgumentNullException(
                    nameof(voiceManager));

            _logger.Write(
                "Session Audio Controller created.");
        }

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(
                    nameof(SessionAudioController));

            if (_started)
                return;

            _logger.Write(
                "Session Audio Controller starting.");

            _started = true;

            _logger.Write(
                "Session Audio Controller started.");
        }

        public void HandleSessionState(
            WindowsSessionState state)
        {
            if (_disposed)
                return;

            _logger.Write(
                $"Session Audio Controller received state: {state}");

            switch (state)
            {
                case WindowsSessionState.Locked:
                    HandleLockedState();
                    break;

                case WindowsSessionState.Unlocked:
                    HandleUnlockedState();
                    break;

                case WindowsSessionState.Unknown:
                default:
                    _logger.Write(
                        "Session Audio Controller: " +
                        "unknown session state, no audio action.");
                    break;
            }
        }

        private void HandleLockedState()
        {
            _logger.Write(
                "Session Audio Controller: " +
                "Windows session locked.");

            _logger.Write(
                "Session Audio Controller: " +
                "starting lock-screen music.");

            if (!ReadAudioSwitch(
                    "PlayMusicOnLock",
                    true))
            {
                _logger.Write(
                    "Session Audio Controller: " +
                    "lock-screen music disabled by configuration.");

                return;
            }

            double musicVolume =
                ReadMusicVolume();

            float musicVolumeNormalized =
                (float)(
                    musicVolume / 100.0);

            _logger.Write(
                "Session Audio Controller: " +
                $"applying music volume: {musicVolume}% " +
                $"({musicVolumeNormalized:0.00})");

            _musicManager.SetVolume(
                musicVolumeNormalized);

            _musicManager.Start();
        }

        private async void HandleUnlockedState()
        {
            _logger.Write(
                "Session Audio Controller: " +
                "Windows session unlocked.");

            if (ReadAudioSwitch(
                    "StopMusicOnUnlock",
                    true))
            {
                _logger.Write(
                    "Session Audio Controller: " +
                    "fading out lock-screen music.");

                double unlockFadeDuration =
                    ReadUnlockFadeDuration();

                int unlockFadeDurationMilliseconds =
                    (int)Math.Round(
                        unlockFadeDuration);

                _logger.Write(
                    "Session Audio Controller: " +
                    $"applying unlock fade duration: " +
                    $"{unlockFadeDurationMilliseconds} ms.");

                try
                {
                    await _musicManager
                        .FadeOutAsync(
                            unlockFadeDurationMilliseconds,
                            30);
                }
                catch (Exception ex)
                {
                    _logger.Write(
                        "Session Audio Controller: " +
                        $"music fade-out failed: {ex}");

                    // 淡出失败时仍确保音乐停止，
                    // 防止欢迎语音与锁屏音乐同时继续播放。
                    _musicManager.Stop();
                }
            }
            else
            {
                _logger.Write(
                    "Session Audio Controller: " +
                    "music stop/fade disabled by configuration.");
            }

            if (!ReadAudioSwitch(
                    "PlayWelcomeVoice",
                    true))
            {
                _logger.Write(
                    "Session Audio Controller: " +
                    "welcome voice disabled by configuration.");

                return;
            }

            double welcomeVoiceVolume =
                ReadWelcomeVoiceVolume();

            float welcomeVoiceVolumeNormalized =
                (float)(
                    welcomeVoiceVolume / 100.0);

            _logger.Write(
                "Session Audio Controller: " +
                $"applying welcome voice volume: " +
                $"{welcomeVoiceVolume}% " +
                $"({welcomeVoiceVolumeNormalized:0.00})");

            _voiceManager.SetVolume(
                welcomeVoiceVolumeNormalized);

            _logger.Write(
                "Session Audio Controller: " +
                "starting welcome voice.");

            _voiceManager.Play();
        }

        /*
         * ============================================================
         * 音频开关读取
         * ============================================================
         *
         * 直接读取当前配置文件。
         * 配置不存在或读取失败时保持原有默认行为：开启。
         */
        private bool ReadAudioSwitch(
            string propertyName,
            bool defaultValue)
        {
            try
            {
                string configurationDirectory =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "ShittimEcho");

                string configurationFilePath =
                    Path.Combine(
                        configurationDirectory,
                        "configuration.json");

                if (!File.Exists(
                        configurationFilePath))
                {
                    return defaultValue;
                }

                string json =
                    File.ReadAllText(
                        configurationFilePath);

                AppConfiguration? configuration =
                    JsonSerializer.Deserialize<AppConfiguration>(
                        json);

                if (configuration == null)
                {
                    return defaultValue;
                }

                switch (propertyName)
                {
                    case "PlayMusicOnLock":
                        return configuration.PlayMusicOnLock;

                    case "StopMusicOnUnlock":
                        return configuration.StopMusicOnUnlock;

                    case "PlayWelcomeVoice":
                        return configuration.PlayWelcomeVoice;

                    default:
                        return defaultValue;
                }
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Session Audio Controller: " +
                    $"failed to read audio switch {propertyName}: {ex}");

                return defaultValue;
            }
        }

        private double ReadMusicVolume()
        {
            const double defaultVolume = 100.0;

            try
            {
                string configurationDirectory =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "ShittimEcho");

                string configurationFilePath =
                    Path.Combine(
                        configurationDirectory,
                        "configuration.json");

                if (!File.Exists(
                        configurationFilePath))
                {
                    return defaultVolume;
                }

                string json =
                    File.ReadAllText(
                        configurationFilePath);

                AppConfiguration? configuration =
                    JsonSerializer.Deserialize<AppConfiguration>(
                        json);

                if (configuration == null)
                {
                    return defaultVolume;
                }

                double volume =
                    configuration.MusicVolume;

                if (double.IsNaN(volume) ||
                    double.IsInfinity(volume))
                {
                    return defaultVolume;
                }

                if (volume < 0)
                {
                    return 0;
                }

                if (volume > 100)
                {
                    return 100;
                }

                return volume;
            }
            catch (Exception ex)
            {
                _logger.Write(
                    "Session Audio Controller: " +
                    $"failed to read music volume: {ex}");

                return defaultVolume;
            }
        }

        /*
         * ============================================================
         * 欢迎语音音量读取
         * ============================================================
         *
         * 读取设置窗口保存的 WelcomeVoiceVolume。
         *
         * 音量范围：
         *
         * 0   = 静音
         * 100 = 最大音量
         *
         * 配置不存在或读取失败时，
         * 保持默认 100% 的原有行为。
         */
        private double ReadWelcomeVoiceVolume()
        {
            const double defaultVolume = 100.0;

            try
            {
                string configurationDirectory =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "ShittimEcho");

                string configurationFilePath =
                    Path.Combine(
                        configurationDirectory,
                        "configuration.json");

                if (!File.Exists(
                        configurationFilePath))
                {
                    return defaultVolume;
                }

                string json =
                    File.ReadAllText(
                        configurationFilePath);

                AppConfiguration? configuration =
                    JsonSerializer.Deserialize<AppConfiguration>(
                        json);

                if (configuration == null)
                {
                    return defaultVolume;
                }

                double volume =
                    configuration.WelcomeVoiceVolume;

                if (double.IsNaN(volume) ||
                    double.IsInfinity(volume))
                {
                    return defaultVolume;
                }

                if (volume < 0)
                {
                    return 0;
                }

                if (volume > 100)
                {
                    return 100;
                }

                return volume;
            }
            catch (Exception ex)
            {
                _logger.Write(
                    "Session Audio Controller: " +
                    $"failed to read welcome voice volume: {ex}");

                return defaultVolume;
            }
        }

        /*
         * ============================================================
         * 解锁淡出时长读取
         * ============================================================
         *
         * 读取设置窗口保存的 UnlockFadeDuration。
         *
         * 音量范围对应的设置范围：
         *
         * 500  = 0.5 秒
         * 5000 = 5 秒
         *
         * 配置不存在或读取失败时，
         * 保持原有默认值 1500 ms。
         */
        private double ReadUnlockFadeDuration()
        {
            const double defaultDuration = 1500.0;

            try
            {
                string configurationDirectory =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "ShittimEcho");

                string configurationFilePath =
                    Path.Combine(
                        configurationDirectory,
                        "configuration.json");

                if (!File.Exists(
                        configurationFilePath))
                {
                    return defaultDuration;
                }

                string json =
                    File.ReadAllText(
                        configurationFilePath);

                AppConfiguration? configuration =
                    JsonSerializer.Deserialize<AppConfiguration>(
                        json);

                if (configuration == null)
                {
                    return defaultDuration;
                }

                double duration =
                    configuration.UnlockFadeDuration;

                if (double.IsNaN(duration) ||
                    double.IsInfinity(duration))
                {
                    return defaultDuration;
                }

                if (duration < 500)
                {
                    return 500;
                }

                if (duration > 5000)
                {
                    return 5000;
                }

                return duration;
            }
            catch (Exception ex)
            {
                _logger.Write(
                    "Session Audio Controller: " +
                    $"failed to read unlock fade duration: {ex}");

                return defaultDuration;
            }
        }

        private sealed class AppConfiguration
        {
            public string MusicFolder { get; set; } = string.Empty;

            public string VoiceFolder { get; set; } = string.Empty;

            public bool PlayMusicOnLock { get; set; } = true;

            public bool StopMusicOnUnlock { get; set; } = true;

            public bool PlayWelcomeVoice { get; set; } = true;

            public double MusicVolume { get; set; } = 100;

            public double WelcomeVoiceVolume { get; set; } = 100;

            public double UnlockFadeDuration { get; set; } = 1500;
        }

        public void Stop()
        {
            if (!_started)
                return;

            _logger.Write(
                "Session Audio Controller stopping.");

            _musicManager.Stop();
            _voiceManager.Stop();

            _started = false;

            _logger.Write(
                "Session Audio Controller stopped.");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _logger.Write(
                "Session Audio Controller disposing.");

            Stop();

            _disposed = true;

            _logger.Write(
                "Session Audio Controller disposed.");
        }
    }
}