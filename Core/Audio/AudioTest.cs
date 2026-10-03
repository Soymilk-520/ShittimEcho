using System;
using System.IO;
using System.Threading.Tasks;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Core.Audio
{
    public sealed class AudioTest
    {
        private readonly AudioCore _audioCore;
        private readonly StartupLogger _logger;

        public AudioTest(
            AudioCore audioCore,
            StartupLogger logger)
        {
            _audioCore = audioCore;
            _logger = logger;
        }

        public async Task PlaybackCompletionTestAsync()
        {
            const string testFile =
                @"E:\蔚蓝档案音乐\Aoharu-test-5s.wav";

            _logger.Write(
                $"Audio completion test file: {testFile}");

            if (!File.Exists(testFile))
            {
                _logger.Write(
                    "Audio completion test failed: file not found.");

                throw new FileNotFoundException(
                    "Test audio file was not found.",
                    testFile);
            }

            _logger.Write(
                "Audio completion test file found.");

            _audioCore.PlaybackCompleted +=
                OnPlaybackCompleted;

            _audioCore.Play(testFile);

            _logger.Write(
                "Audio completion test playback started.");

            _logger.Write(
                "Waiting for natural playback completion.");

            await Task.Delay(
                TimeSpan.FromSeconds(10));

            _audioCore.PlaybackCompleted -=
                OnPlaybackCompleted;

            _logger.Write(
                "Audio completion test finished.");
        }

        private void OnPlaybackCompleted(
            object? sender,
            EventArgs e)
        {
            _logger.Write(
                "Audio Core playback completed naturally.");
        }
    }
}