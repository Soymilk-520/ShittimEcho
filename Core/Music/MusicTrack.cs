using System;

namespace ShittimEcho.Core.Music
{
    /// <summary>
    /// 表示音乐库中的一首音乐。
    /// 该类只负责描述音乐资源，不负责实际播放。
    /// </summary>
    public sealed class MusicTrack
    {
        public MusicTrack(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "Music file path cannot be empty.",
                    nameof(filePath));
            }

            FilePath = filePath;
        }

        /// <summary>
        /// 音乐文件的完整路径。
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 音乐文件名称。
        /// </summary>
        public string FileName =>
            System.IO.Path.GetFileName(FilePath);

        /// <summary>
        /// 用户是否启用这首音乐。
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 音乐在播放列表中的排序位置。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 音乐显示名称。
        /// 默认使用文件名。
        /// </summary>
        public string DisplayName =>
            System.IO.Path.GetFileNameWithoutExtension(
                FilePath);
    }
}