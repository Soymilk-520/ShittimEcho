using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Core.Audio
{
    public sealed class AudioEngine : IDisposable
    {
        private readonly AudioDecoder _decoder;
        private readonly StartupLogger _logger;

        private WaveOut? _outputDevice;
        private WaveStream? _audioStream;

        private float _volume = 1.0f;

        public bool IsPlaying { get; private set; }

        public string? CurrentFilePath { get; private set; }

        public event EventHandler? PlaybackCompleted;

        public AudioEngine(StartupLogger logger)
        {
            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));

            _decoder =
                new AudioDecoder();

            _logger.Write(
                "Audio Engine instance created.");
        }

        public void Initialize()
        {
            _logger.Write(
                "Audio Engine Initialize() called.");

            if (_outputDevice != null)
            {
                _logger.Write(
                    "Audio Engine Initialize(): output device already exists.");

                return;
            }

            // AudioEngine 不在 Initialize 阶段创建实际播放器。
            // 播放器将在 Play() 时根据当前音频文件创建。
            _logger.Write(
                "Audio Engine Initialize(): no output device created yet.");
        }

        public void Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "Audio file path cannot be empty.",
                    nameof(filePath));
            }

            _logger.Write(
                "========================================");

            _logger.Write(
                $"Audio Engine Play() requested: {filePath}");

            _logger.Write(
                $"Audio Engine state before Play(): " +
                $"IsPlaying={IsPlaying}, " +
                $"CurrentFilePath={CurrentFilePath ?? "<null>"}, " +
                $"OutputDeviceExists={_outputDevice != null}, " +
                $"AudioStreamExists={_audioStream != null}");

            // 停止上一首播放。
            _logger.Write(
                "Audio Engine Play(): calling Stop() for previous playback.");

            Stop();

            _logger.Write(
                "Audio Engine Play(): previous playback cleanup completed.");

            WaveOut? outputDevice = null;
            WaveStream? audioStream = null;

            try
            {
                // 根据文件扩展名选择解码器。
                //
                // WAV  -> WaveFileReader
                // MP3  -> AudioFileReader
                // OGG  -> VorbisWaveReader
                _logger.Write(
                    "Audio Engine Play(): creating audio stream.");

                audioStream =
                    _decoder.CreateWaveStream(filePath);

                _logger.Write(
                    $"Audio Engine Play(): audio stream created. " +
                    $"Type={audioStream.GetType().FullName}");

                _logger.Write(
                    $"Audio Engine Play(): stream length={audioStream.Length}");

                _logger.Write(
                    $"Audio Engine Play(): stream position={audioStream.Position}");

                // 创建输出设备。
                _logger.Write(
                    "Audio Engine Play(): creating WaveOut.");

                outputDevice =
                    new WaveOut();

                // 确保每次新建播放器时使用正常的满音量。
                // WaveOut.Volume 为播放器自身音量，不修改 Windows 系统主音量。
                // 这样锁屏音乐和解锁后的欢迎语音都不会因播放器音量为 0 而静音。
                outputDevice.Volume =
                    1.0f;

                // 应用 AudioEngine 当前保存的播放器音量设置。
                outputDevice.Volume =
                    _volume;

                _logger.Write(
                    $"Audio Engine Play(): WaveOut created. Volume={outputDevice.Volume}");

                outputDevice.PlaybackStopped +=
                    OnPlaybackStopped;

                _logger.Write(
                    "Audio Engine Play(): PlaybackStopped event subscribed.");

                // 将解码后的音频流交给 WaveOut。
                _logger.Write(
                    "Audio Engine Play(): calling WaveOut.Init().");

                outputDevice.Init(audioStream);

                _logger.Write(
                    "Audio Engine Play(): WaveOut.Init() completed.");

                // 到这里才正式建立当前播放状态。
                _outputDevice =
                    outputDevice;

                _audioStream =
                    audioStream;

                CurrentFilePath =
                    filePath;

                IsPlaying =
                    true;

                _logger.Write(
                    "Audio Engine Play(): current playback state assigned.");

                _logger.Write(
                    $"Audio Engine Play(): " +
                    $"IsPlaying={IsPlaying}, " +
                    $"CurrentFilePath={CurrentFilePath}, " +
                    $"OutputDeviceExists={_outputDevice != null}, " +
                    $"AudioStreamExists={_audioStream != null}");

                // 所有权转移给 AudioEngine。
                outputDevice = null;
                audioStream = null;

                _logger.Write(
                    "Audio Engine Play(): ownership transferred to AudioEngine.");

                _logger.Write(
                    "Audio Engine Play(): calling WaveOut.Play().");

                _outputDevice.Play();

                _logger.Write(
                    "Audio Engine Play(): WaveOut.Play() returned.");

                _logger.Write(
                    $"Audio Engine state after Play(): " +
                    $"IsPlaying={IsPlaying}, " +
                    $"CurrentFilePath={CurrentFilePath ?? "<null>"}, " +
                    $"OutputDeviceExists={_outputDevice != null}, " +
                    $"AudioStreamExists={_audioStream != null}");

                _logger.Write(
                    "========================================");
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Audio Engine Play() failed: {filePath}");

                _logger.Write(
                    $"Audio Engine Play() exception: {ex}");

                if (outputDevice != null)
                {
                    _logger.Write(
                        "Audio Engine Play() cleanup: disposing temporary WaveOut.");

                    outputDevice.PlaybackStopped -=
                        OnPlaybackStopped;

                    try
                    {
                        outputDevice.Stop();
                    }
                    catch (Exception stopException)
                    {
                        _logger.Write(
                            $"Audio Engine Play() cleanup Stop() failed: " +
                            $"{stopException.Message}");
                    }

                    outputDevice.Dispose();
                }

                audioStream?.Dispose();

                CurrentFilePath = null;
                IsPlaying = false;

                throw;
            }
        }

        public void Stop()
        {
            _logger.Write(
                $"Audio Engine Stop() called. " +
                $"IsPlaying={IsPlaying}, " +
                $"CurrentFilePath={CurrentFilePath ?? "<null>"}, " +
                $"OutputDeviceExists={_outputDevice != null}, " +
                $"AudioStreamExists={_audioStream != null}");

            WaveOut? outputDevice =
                _outputDevice;

            WaveStream? audioStream =
                _audioStream;

            if (outputDevice == null &&
                audioStream == null)
            {
                _logger.Write(
                    "Audio Engine Stop(): nothing to stop.");

                return;
            }

            // 先从当前实例解除引用。
            _outputDevice = null;
            _audioStream = null;

            CurrentFilePath = null;
            IsPlaying = false;

            _logger.Write(
                "Audio Engine Stop(): current playback state cleared.");

            if (outputDevice != null)
            {
                _logger.Write(
                    "Audio Engine Stop(): unsubscribing PlaybackStopped event.");

                outputDevice.PlaybackStopped -=
                    OnPlaybackStopped;

                try
                {
                    _logger.Write(
                        "Audio Engine Stop(): calling WaveOut.Stop().");

                    outputDevice.Stop();

                    _logger.Write(
                        "Audio Engine Stop(): WaveOut.Stop() returned.");
                }
                finally
                {
                    _logger.Write(
                        "Audio Engine Stop(): disposing WaveOut.");

                    outputDevice.Dispose();

                    _logger.Write(
                        "Audio Engine Stop(): WaveOut disposed.");
                }
            }

            if (audioStream != null)
            {
                _logger.Write(
                    "Audio Engine Stop(): disposing audio stream.");

                audioStream.Dispose();

                _logger.Write(
                    "Audio Engine Stop(): audio stream disposed.");
            }

            _logger.Write(
                "Audio Engine Stop() completed.");
        }

        /*
         * ============================================================
         * 设置播放器音量
         * ============================================================
         *
         * 音量范围：
         *
         * 0.0f = 静音
         * 1.0f = 100%
         *
         * 不修改 Windows 系统主音量。
         *
         * 如果当前正在播放，则立即更新当前 WaveOut。
         *
         * 如果当前没有播放，则保存音量，
         * 下一次 Play() 创建 WaveOut 时会自动应用。
         */
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
                $"Audio Engine SetVolume(): " +
                $"Volume={_volume}");

            if (_outputDevice != null)
            {
                _outputDevice.Volume =
                    _volume;

                _logger.Write(
                    "Audio Engine SetVolume(): " +
                    "current WaveOut volume updated.");
            }
        }

        private void OnPlaybackStopped(
            object? sender,
            StoppedEventArgs e)
        {
            _logger.Write(
                "----------------------------------------");

            _logger.Write(
                "Audio Engine OnPlaybackStopped() ENTERED.");

            _logger.Write(
                $"Audio Engine OnPlaybackStopped(): " +
                $"senderType={sender?.GetType().FullName ?? "<null>"}");

            _logger.Write(
                $"Audio Engine OnPlaybackStopped(): " +
                $"IsPlaying={IsPlaying}, " +
                $"CurrentFilePath={CurrentFilePath ?? "<null>"}, " +
                $"OutputDeviceExists={_outputDevice != null}, " +
                $"AudioStreamExists={_audioStream != null}");

            if (e.Exception != null)
            {
                _logger.Write(
                    $"Audio Engine OnPlaybackStopped(): exception={e.Exception}");
            }
            else
            {
                _logger.Write(
                    "Audio Engine OnPlaybackStopped(): exception=<null>");
            }

            if (sender is not WaveOut stoppedDevice)
            {
                _logger.Write(
                    "Audio Engine OnPlaybackStopped(): sender is not WaveOut.");

                _logger.Write(
                    "----------------------------------------");

                return;
            }

            // 如果这个播放器已经不是当前播放器，
            // 说明它属于旧的播放任务，直接忽略。
            if (!ReferenceEquals(
                    stoppedDevice,
                    _outputDevice))
            {
                _logger.Write(
                    "Audio Engine OnPlaybackStopped(): " +
                    "sender is NOT the current WaveOut.");

                _logger.Write(
                    "Audio Engine OnPlaybackStopped(): " +
                    "ignoring stale playback event.");

                _logger.Write(
                    "----------------------------------------");

                return;
            }

            _logger.Write(
                "Audio Engine OnPlaybackStopped(): " +
                "sender IS the current WaveOut.");

            WaveStream? audioStream =
                _audioStream;

            string? completedFile =
                CurrentFilePath;

            if (audioStream == null)
            {
                _logger.Write(
                    "Audio Engine OnPlaybackStopped(): " +
                    "current audio stream is null.");

                _logger.Write(
                    "----------------------------------------");

                return;
            }

            // 从当前播放状态中解除引用。
            _audioStream = null;
            _outputDevice = null;

            CurrentFilePath = null;
            IsPlaying = false;

            _logger.Write(
                "Audio Engine OnPlaybackStopped(): " +
                "current playback state cleared.");

            stoppedDevice.PlaybackStopped -=
                OnPlaybackStopped;

            _logger.Write(
                "Audio Engine OnPlaybackStopped(): " +
                "PlaybackStopped event unsubscribed.");

            audioStream.Dispose();

            _logger.Write(
                "Audio Engine OnPlaybackStopped(): audio stream disposed.");

            stoppedDevice.Dispose();

            _logger.Write(
                "Audio Engine OnPlaybackStopped(): WaveOut disposed.");

            // WaveOut 的 PlaybackStopped 会同时因为
            // Stop() 和自然播放结束而触发。
            //
            // AudioCore/MusicManager 当前通过自然播放流程
            // 使用该事件，因此这里保持现有事件行为。
            if (completedFile != null)
            {
                _logger.Write(
                    $"Audio Engine OnPlaybackStopped(): " +
                    $"reporting PlaybackCompleted for {completedFile}.");

                PlaybackCompleted?.Invoke(
                    this,
                    EventArgs.Empty);

                _logger.Write(
                    "Audio Engine OnPlaybackStopped(): " +
                    "PlaybackCompleted event invoked.");
            }
            else
            {
                _logger.Write(
                    "Audio Engine OnPlaybackStopped(): " +
                    "completed file is null, PlaybackCompleted not invoked.");
            }

            _logger.Write(
                "Audio Engine OnPlaybackStopped() EXITED.");

            _logger.Write(
                "----------------------------------------");
        }


        /*
         * ============================================================
         * 音乐淡出
         * ============================================================
         *
         * 仅负责将当前 WaveOut 的音量平滑降低。
         * 淡出完成后再停止当前播放器。
         *
         * 不修改现有 Play() / Stop() 的行为。
         */
        public async Task FadeOutAsync(
            int durationMilliseconds = 1500,
            int steps = 30,
            CancellationToken cancellationToken = default)
        {
            if (_outputDevice == null ||
                _audioStream == null ||
                !IsPlaying)
            {
                _logger.Write(
                    "Audio Engine FadeOutAsync(): nothing is currently playing.");

                return;
            }

            if (durationMilliseconds < 1)
            {
                durationMilliseconds = 1;
            }

            if (steps < 1)
            {
                steps = 1;
            }

            WaveOut? outputDevice =
                _outputDevice;

            float originalVolume =
                outputDevice.Volume;

            _logger.Write(
                $"Audio Engine FadeOutAsync() started. " +
                $"Duration={durationMilliseconds}ms, " +
                $"Steps={steps}, " +
                $"OriginalVolume={originalVolume}");

            int stepDelay =
                Math.Max(
                    1,
                    durationMilliseconds / steps);

            try
            {
                for (int step = 1;
                     step <= steps;
                     step++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!ReferenceEquals(
                            outputDevice,
                            _outputDevice) ||
                        !IsPlaying)
                    {
                        _logger.Write(
                            "Audio Engine FadeOutAsync(): " +
                            "current playback changed during fade.");

                        return;
                    }

                    float remainingRatio =
                        1f -
                        (float)step / steps;

                    outputDevice.Volume =
                        Math.Max(
                            0f,
                            originalVolume * remainingRatio);

                    await Task.Delay(
                        stepDelay,
                        cancellationToken)
                        .ConfigureAwait(false);
                }

                if (ReferenceEquals(
                        outputDevice,
                        _outputDevice))
                {
                    outputDevice.Volume = 0f;

                    _logger.Write(
                        "Audio Engine FadeOutAsync(): " +
                        "fade completed, stopping playback.");

                    Stop();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Write(
                    "Audio Engine FadeOutAsync(): cancelled.");

                throw;
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Audio Engine FadeOutAsync() failed: {ex}");

                throw;
            }
        }

        public void Dispose()
        {
            _logger.Write(
                "Audio Engine Dispose() called.");

            Stop();

            _logger.Write(
                "Audio Engine Dispose() completed.");
        }
    }
}