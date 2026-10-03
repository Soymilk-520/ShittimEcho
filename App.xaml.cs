using System;
using System.Windows;
using ShittimEcho.Core.Audio;
using ShittimEcho.Core.Diagnostics;
using ShittimEcho.Core.Music;
using ShittimEcho.Core.Voice;
using ShittimEcho.Windows;

namespace ShittimEcho
{
    public partial class App : System.Windows.Application
    {
        private StartupLogger? _logger;
        private AudioCore? _audioCore;
        private WindowsSessionMonitor? _sessionMonitor;
        private WindowsPowerMonitor? _powerMonitor;

        private ShittimEcho.Core.Music.MusicLibrary? _musicLibrary;
        private MusicManager? _musicManager;

        private VoiceLibrary? _voiceLibrary;
        private VoiceManager? _voiceManager;

        private SessionAudioController? _sessionAudioController;

        private MainWindow? _mainWindow;

        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            StartupLogger logger =
                new StartupLogger();

            _logger = logger;

            logger.WriteSystemInformation();
            logger.Write(
                "Application startup sequence entered.");

            try
            {
                // --------------------------------------------------
                // 先显示主窗口
                // --------------------------------------------------
                // 这样 MainWindow 的 Loaded 事件可以立即启动
                // 身份认证动画，而不会被后面的音频系统初始化阻塞。
                // --------------------------------------------------

                _mainWindow =
                    new MainWindow();

                MainWindow = _mainWindow;

                _mainWindow.Show();

                logger.Write(
                    "MainWindow created and shown.");

                // --------------------------------------------------
                // Windows 锁屏音频策略
                // --------------------------------------------------

                CheckWindowsAudioPolicy();

                // --------------------------------------------------
                // 音乐 / 语音资源目录
                // --------------------------------------------------

                MusicPaths.EnsureDirectories();
                VoicePaths.EnsureDirectories();

                logger.Write(
                    $"Music resource directory: " +
                    $"{MusicPaths.MusicDirectory}");

                logger.Write(
                    $"Voice resource directory: " +
                    $"{VoicePaths.VoiceDirectory}");

                // --------------------------------------------------
                // 音乐库
                // --------------------------------------------------

                _musicLibrary =
                    new ShittimEcho.Core.Music.MusicLibrary(
                        MusicPaths.MusicDirectory);

                _musicLibrary.Scan();

                logger.Write(
                    $"Music Library initialized. " +
                    $"Tracks detected: " +
                    $"{_musicLibrary.Tracks.Count}");

                foreach (MusicTrack track in
                    _musicLibrary.Tracks)
                {
                    logger.Write(
                        $"Music Library Track: " +
                        $"SortOrder={track.SortOrder}, " +
                        $"Enabled={track.IsEnabled}, " +
                        $"Name={track.DisplayName}, " +
                        $"Path={track.FilePath}");
                }

                // --------------------------------------------------
                // 欢迎语音库
                // --------------------------------------------------

                _voiceLibrary =
                    new VoiceLibrary(
                        VoicePaths.VoiceDirectory);

                _voiceLibrary.Scan();

                logger.Write(
                    $"Voice Library initialized. " +
                    $"Tracks detected: " +
                    $"{_voiceLibrary.Tracks.Count}");

                foreach (VoiceTrack voice in
                    _voiceLibrary.Tracks)
                {
                    logger.Write(
                        $"Voice Library Track: " +
                        $"SortOrder={voice.SortOrder}, " +
                        $"Enabled={voice.IsEnabled}, " +
                        $"Name={voice.DisplayName}, " +
                        $"Path={voice.FilePath}");
                }

                // --------------------------------------------------
                // 音频核心
                // --------------------------------------------------

                _audioCore =
                    new AudioCore(logger);

                _audioCore.Initialize();

                // --------------------------------------------------
                // 音乐管理器
                // --------------------------------------------------

                _musicManager =
                    new MusicManager(
                        logger,
                        _audioCore,
                        _musicLibrary);

                _musicManager.PlayMode =
                    MusicPlayMode.LoopPlaylist;

                logger.Write(
                    "Music Manager initialized.");

                logger.Write(
                    "Music Manager play mode: LoopPlaylist.");

                // --------------------------------------------------
                // 欢迎语音管理器
                // --------------------------------------------------

                _voiceManager =
                    new VoiceManager(
                        logger,
                        _audioCore,
                        _voiceLibrary);

                logger.Write(
                    "Voice Manager initialized.");

                // --------------------------------------------------
                // 会话音频控制器
                // --------------------------------------------------

                _sessionAudioController =
                    new SessionAudioController(
                        logger,
                        _musicManager,
                        _voiceManager);

                _sessionAudioController.Start();

                logger.Write(
                    "Session Audio Controller connected.");

                // --------------------------------------------------
                // Windows Session Monitor
                // --------------------------------------------------

                _sessionMonitor =
                    new WindowsSessionMonitor(
                        logger);

                _sessionMonitor.StateChanged +=
                    OnWindowsSessionStateChanged;

                _sessionMonitor.Start();

                logger.Write(
                    "Windows Session Monitor connected " +
                    "to application.");

                // --------------------------------------------------
                // Windows Power Monitor
                // --------------------------------------------------

                _powerMonitor =
                    new WindowsPowerMonitor(
                        logger);

                _powerMonitor.PowerStateChanged +=
                    OnWindowsPowerStateChanged;

                _powerMonitor.Start();

                logger.Write(
                    "Windows Power Monitor connected " +
                    "to application.");

                logger.Write(
                    "Application startup sequence completed.");
            }
            catch (Exception ex)
            {
                logger.Write(
                    $"Application startup failed: {ex}");

                System.Windows.MessageBox.Show(
                    "Shittim Chest: Echo 启动失败。" +
                    Environment.NewLine +
                    Environment.NewLine +
                    ex.Message,
                    "Shittim Chest: Echo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }

        private void CheckWindowsAudioPolicy()
        {
            if (_logger == null)
                return;

            _logger.Write(
                "Checking Windows lock-screen audio policy.");

            var checker =
                new WindowsAudioPolicyChecker();

            WindowsAudioPolicyResult result =
                checker.Check();

            _logger.Write(
                $"Windows lock-screen audio policy: " +
                $"Configured={result.IsConfigured}, " +
                $"Value={result.Value?.ToString() ?? "<null>"}, " +
                $"Status={result.Status}");

            _logger.Write(
                $"Windows lock-screen audio playback allowed: " +
                $"{result.IsPlaybackAllowed}");

            _logger.Write(
                $"Windows lock-screen audio policy message: " +
                result.Message);
        }

        private void OnWindowsSessionStateChanged(
            object? sender,
            WindowsSessionState state)
        {
            if (_logger != null)
            {
                _logger.Write(
                    $"Application received Windows session " +
                    $"state change: {state}");
            }

            if (_sessionAudioController == null)
            {
                if (_logger != null)
                {
                    _logger.Write(
                        "Session Audio Controller is not available.");
                }

                return;
            }

            try
            {
                _sessionAudioController.HandleSessionState(
                    state);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.Write(
                        $"Session Audio Controller failed " +
                        $"to handle state {state}: {ex}");
                }
            }
        }

        private void OnWindowsPowerStateChanged(
            object? sender,
            WindowsPowerState state)
        {
            if (_logger != null)
            {
                _logger.Write(
                    $"Application received Windows power " +
                    $"state change: {state}");
            }
        }

        protected override void OnExit(
            ExitEventArgs e)
        {
            if (_logger != null)
            {
                _logger.Write(
                    "Application shutdown sequence entered.");
            }

            if (_powerMonitor != null)
            {
                _powerMonitor.PowerStateChanged -=
                    OnWindowsPowerStateChanged;

                _powerMonitor.Dispose();

                _powerMonitor = null;
            }

            if (_sessionMonitor != null)
            {
                _sessionMonitor.StateChanged -=
                    OnWindowsSessionStateChanged;

                _sessionMonitor.Dispose();

                _sessionMonitor = null;
            }

            if (_sessionAudioController != null)
            {
                _sessionAudioController.Dispose();

                _sessionAudioController = null;
            }

            if (_voiceManager != null)
            {
                _voiceManager.Dispose();

                _voiceManager = null;
            }

            if (_musicManager != null)
            {
                _musicManager.Dispose();

                _musicManager = null;
            }

            if (_audioCore != null)
            {
                _audioCore.Dispose();

                _audioCore = null;
            }

            _voiceLibrary = null;
            _musicLibrary = null;
            _mainWindow = null;

            if (_logger != null)
            {
                _logger.Write(
                    "Application shutdown sequence completed.");
            }

            base.OnExit(e);
        }
    }
}