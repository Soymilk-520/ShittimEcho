using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Core.Audio
{
    public sealed class AudioCore : IDisposable
    {
        private readonly StartupLogger _logger;
        private readonly AudioDeviceManager _deviceManager;

        private AudioEngine? _audioEngine;

        public AudioCore(StartupLogger logger)
        {
            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));

            _deviceManager =
                new AudioDeviceManager();
        }

        public AudioEndpoint? CurrentEndpoint { get; private set; }

        public bool IsInitialized =>
            _audioEngine != null;

        public event EventHandler? PlaybackCompleted;

        public void Initialize()
        {
            _logger.Write(
                "Audio Core initializing.");

            try
            {
                var devices =
                    _deviceManager.GetOutputDevices();

                _logger.Write(
                    $"Audio output devices detected: {devices.Count}");

                foreach (var device in devices)
                {
                    _logger.Write(
                        "Audio device:");

                    _logger.Write(
                        $"    Name: {device.Name}");

                    _logger.Write(
                        $"    State: {device.State}");

                    _logger.Write(
                        $"    Default: {device.IsDefault}");

                    _logger.Write(
                        $"    ID: {device.Id}");
                }

                var defaultDevice =
                    devices.FirstOrDefault(
                        d => d.IsDefault);

                if (defaultDevice == null)
                {
                    CurrentEndpoint = null;

                    _logger.Write(
                        "Current audio endpoint: NOT FOUND.");

                    throw new InvalidOperationException(
                        "No default audio output device was found.");
                }

                CurrentEndpoint =
                    new AudioEndpoint(
                        defaultDevice);

                _logger.Write(
                    "Current audio endpoint:");

                _logger.Write(
                    $"    Name: {CurrentEndpoint.Name}");

                _logger.Write(
                    $"    State: {CurrentEndpoint.State}");

                _logger.Write(
                    $"    Default: {CurrentEndpoint.IsDefault}");

                _logger.Write(
                    $"    ID: {CurrentEndpoint.Id}");

                _audioEngine =
                    new AudioEngine(
                        _logger);

                _audioEngine.PlaybackCompleted +=
                    OnPlaybackCompleted;

                _audioEngine.Initialize();

                _logger.Write(
                    "Audio Engine initialized.");

                _logger.Write(
                    "Audio Core initialization completed.");
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Audio Core initialization failed: {ex}");

                Dispose();

                throw;
            }
        }

        public void Play(string filePath)
        {
            if (_audioEngine == null)
            {
                throw new InvalidOperationException(
                    "Audio Core has not been initialized.");
            }

            _logger.Write(
                $"Audio Core playback requested: {filePath}");

            _audioEngine.Play(
                filePath);

            _logger.Write(
                "Audio Core playback started.");
        }

        public void Stop()
        {
            if (_audioEngine == null)
                return;

            _logger.Write(
                "Audio Core Stop() requested.");

            _audioEngine.Stop();

            _logger.Write(
                "Audio Core playback stopped.");
        }

        public void SetVolume(
            float volume)
        {
            if (_audioEngine == null)
            {
                _logger.Write(
                    "Audio Core SetVolume(): " +
                    "Audio Engine is not initialized.");

                return;
            }

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

            _logger.Write(
                $"Audio Core SetVolume(): " +
                $"Volume={volume}");

            _audioEngine.SetVolume(
                volume);

            _logger.Write(
                "Audio Core SetVolume(): " +
                "Audio Engine volume updated.");
        }

        /*
         * ============================================================
         * 音乐淡出
         * ============================================================
         *
         * 仅作为 AudioEngine 淡出能力的转接层。
         * 不修改现有 Play() / Stop() 行为。
         */
        public async Task FadeOutAsync(
            int durationMilliseconds = 1500,
            int steps = 30,
            CancellationToken cancellationToken = default)
        {
            if (_audioEngine == null)
            {
                _logger.Write(
                    "Audio Core FadeOutAsync(): Audio Engine is not initialized.");

                return;
            }

            _logger.Write(
                $"Audio Core FadeOutAsync() requested. " +
                $"Duration={durationMilliseconds}ms, " +
                $"Steps={steps}");

            await _audioEngine
                .FadeOutAsync(
                    durationMilliseconds,
                    steps,
                    cancellationToken)
                .ConfigureAwait(false);

            _logger.Write(
                "Audio Core FadeOutAsync() completed.");
        }

        private void OnPlaybackCompleted(
            object? sender,
            EventArgs e)
        {
            _logger.Write(
                "Audio Core received natural playback completion.");

            PlaybackCompleted?.Invoke(
                this,
                EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_audioEngine != null)
            {
                _audioEngine.PlaybackCompleted -=
                    OnPlaybackCompleted;

                _audioEngine.Dispose();

                _audioEngine = null;
            }

            CurrentEndpoint = null;
        }
    }
}