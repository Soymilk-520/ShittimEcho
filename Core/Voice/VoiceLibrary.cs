using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShittimEcho.Core.Voice
{
    public sealed class VoiceLibrary
    {
        private readonly string _voiceDirectory;

        private static readonly string[] SupportedExtensions =
        {
            ".wav",
            ".mp3",
            ".ogg"
        };

        public VoiceLibrary(string voiceDirectory)
        {
            if (string.IsNullOrWhiteSpace(voiceDirectory))
            {
                throw new ArgumentException(
                    "Voice directory cannot be empty.",
                    nameof(voiceDirectory));
            }

            _voiceDirectory = voiceDirectory;

            Directory.CreateDirectory(
                _voiceDirectory);
        }

        public List<VoiceTrack> Tracks { get; private set; } =
            new List<VoiceTrack>();

        public void Scan()
        {
            Directory.CreateDirectory(
                _voiceDirectory);

            List<string> files =
                Directory
                    .GetFiles(
                        _voiceDirectory,
                        "*.*",
                        SearchOption.AllDirectories)
                    .Where(IsSupportedAudioFile)
                    .OrderBy(path => path)
                    .ToList();

            Tracks.Clear();

            for (int i = 0;
                 i < files.Count;
                 i++)
            {
                string filePath =
                    files[i];

                Tracks.Add(
                    new VoiceTrack
                    {
                        SortOrder = i,
                        IsEnabled = true,
                        DisplayName =
                            Path.GetFileNameWithoutExtension(
                                filePath),
                        FilePath = filePath
                    });
            }
        }

        public List<VoiceTrack> GetEnabledTracks()
        {
            return Tracks
                .Where(track => track.IsEnabled)
                .OrderBy(track => track.SortOrder)
                .ToList();
        }

        private static bool IsSupportedAudioFile(
            string filePath)
        {
            string extension =
                Path.GetExtension(filePath);

            return SupportedExtensions
                .Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}