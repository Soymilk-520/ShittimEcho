using System;
using System.Collections.Generic;
using System.IO;
using ShittimEcho.Core.Audio;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Core.Voice
{
    public sealed class VoiceManager : IDisposable
    {
        private readonly StartupLogger _logger;
        private readonly AudioCore _audioCore;
        private readonly VoiceLibrary _voiceLibrary;

        private readonly Random _random =
            new Random();

        private float _volume = 1.0f;

        private bool _isPlaying;
        private bool _disposed;


        public VoiceManager(
            StartupLogger logger,
            AudioCore audioCore,
            VoiceLibrary voiceLibrary)
        {
            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));

            _audioCore =
                audioCore ?? throw new ArgumentNullException(
                    nameof(audioCore));

            _voiceLibrary =
                voiceLibrary ?? throw new ArgumentNullException(
                    nameof(voiceLibrary));


            _audioCore.PlaybackCompleted +=
                OnPlaybackCompleted;


            _logger.Write(
                "Voice Manager created.");
        }


        public bool IsPlaying =>
            _isPlaying;


        public VoiceTrack? CurrentVoice
        {
            get;
            private set;
        }


        public void RefreshLibrary()
        {
            ThrowIfDisposed();

            _voiceLibrary.Scan();


            _logger.Write(
                $"Voice Manager library refreshed. " +
                $"Tracks={_voiceLibrary.Tracks.Count}");
        }


        public void SetVolume(
            float volume)
        {
            ThrowIfDisposed();


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
                $"Voice Manager SetVolume(): " +
                $"Volume={_volume}");


            _audioCore.SetVolume(
                _volume);
        }



        /// <summary>
        /// 播放指定音频文件。
        /// 用于 TTS 欢迎语播放。
        /// 播放完成后可继续调用 Play()
        /// 播放语音数据库内容。
        /// </summary>
        public void PlayFile(
            string filePath)
        {
            ThrowIfDisposed();


            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.Write(
                    "Voice Manager PlayFile(): empty file path.");

                _isPlaying = false;
                CurrentVoice = null;

                return;
            }


            if (!File.Exists(filePath))
            {
                _logger.Write(
                    $"Voice Manager PlayFile(): file does not exist: {filePath}");

                _isPlaying = false;
                CurrentVoice = null;

                return;
            }


            try
            {
                _logger.Write(
                    $"Voice Manager PlayFile(): playing file: {filePath}");


                _audioCore.SetVolume(
                    _volume);


                _audioCore.Play(
                    filePath);


                CurrentVoice = null;

                _isPlaying = true;
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Voice Manager PlayFile(): failed: {ex}");


                _isPlaying = false;

                CurrentVoice = null;
            }
        }



        public void Play()
        {
            ThrowIfDisposed();


            RefreshLibrary();


            List<VoiceTrack> voices =
                _voiceLibrary
                    .GetEnabledTracks();


            if (voices.Count == 0)
            {
                _logger.Write(
                    "Voice Manager Play(): " +
                    "no enabled voice tracks available.");


                _isPlaying = false;

                CurrentVoice = null;

                return;
            }


            VoiceTrack voice =
                voices[
                    _random.Next(
                        0,
                        voices.Count)];


            Play(voice);
        }



        public void Play(
            VoiceTrack voice)
        {
            ThrowIfDisposed();


            if (voice == null)
            {
                throw new ArgumentNullException(
                    nameof(voice));
            }


            if (!File.Exists(voice.FilePath))
            {
                _logger.Write(
                    $"Voice Manager: " +
                    $"voice file does not exist: {voice.FilePath}");


                _isPlaying = false;

                CurrentVoice = null;

                return;
            }


            try
            {
                _logger.Write(
                    $"Voice Manager: " +
                    $"playing voice: {voice.DisplayName}");


                _audioCore.SetVolume(
                    _volume);


                _logger.Write(
                    $"Voice Manager: " +
                    $"applying voice volume: {_volume}");


                _audioCore.Play(
                    voice.FilePath);


                CurrentVoice = voice;

                _isPlaying = true;
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Voice Manager: " +
                    $"failed to play voice " +
                    $"{voice.DisplayName}: {ex}");


                _isPlaying = false;

                CurrentVoice = null;
            }
        }



        public void Stop()
        {
            ThrowIfDisposed();


            _logger.Write(
                "Voice Manager Stop() called.");


            _isPlaying = false;

            CurrentVoice = null;


            _audioCore.Stop();
        }



        private void OnPlaybackCompleted(
            object? sender,
            EventArgs e)
        {
            _logger.Write(
                "Voice Manager: voice playback completed.");


            _isPlaying = false;

            CurrentVoice = null;
        }



        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(VoiceManager));
            }
        }



        public void Dispose()
        {
            if (_disposed)
                return;


            _logger.Write(
                "Voice Manager disposing.");


            _audioCore.PlaybackCompleted -=
                OnPlaybackCompleted;


            _isPlaying = false;

            CurrentVoice = null;


            _audioCore.Stop();


            _disposed = true;


            _logger.Write(
                "Voice Manager disposed.");
        }
    }
}