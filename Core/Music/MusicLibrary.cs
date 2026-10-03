using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShittimEcho.Core.Music
{
    /// <summary>
    /// 音乐库。
    /// 负责发现、管理和排序音乐资源。
    /// 不负责实际音频播放。
    /// </summary>
    public sealed class MusicLibrary
    {
        private static readonly string[] SupportedExtensions =
        {
            ".mp3",
            ".wav",
            ".flac",
            ".ogg",
            ".m4a"
        };

        private readonly List<MusicTrack> _tracks = new();

        public MusicLibrary(
            string musicDirectory)
        {
            if (string.IsNullOrWhiteSpace(musicDirectory))
            {
                throw new ArgumentException(
                    "Music directory cannot be empty.",
                    nameof(musicDirectory));
            }

            MusicDirectory = musicDirectory;

            Directory.CreateDirectory(
                MusicDirectory);
        }

        /// <summary>
        /// 音乐文件所在目录。
        /// </summary>
        public string MusicDirectory { get; }

        /// <summary>
        /// 当前音乐库中的全部音乐。
        /// </summary>
        public IReadOnlyList<MusicTrack> Tracks =>
            _tracks.AsReadOnly();

        /// <summary>
        /// 扫描音乐目录并重新建立音乐库。
        /// </summary>
        public void Scan()
        {
            _tracks.Clear();

            string[] files =
                Directory.GetFiles(
                    MusicDirectory,
                    "*.*",
                    SearchOption.TopDirectoryOnly)
                .Where(IsSupportedAudioFile)
                .OrderBy(
                    file => file,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                _tracks.Add(
                    new MusicTrack(files[i])
                    {
                        SortOrder = i
                    });
            }
        }

        /// <summary>
        /// 获取当前启用的音乐。
        /// </summary>
        public IReadOnlyList<MusicTrack> GetEnabledTracks()
        {
            return _tracks
                .Where(track => track.IsEnabled)
                .OrderBy(track => track.SortOrder)
                .ToList()
                .AsReadOnly();
        }

        private static bool IsSupportedAudioFile(
            string filePath)
        {
            string extension =
                Path.GetExtension(filePath);

            return SupportedExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}