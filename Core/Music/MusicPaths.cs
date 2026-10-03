using System;
using System.IO;
using System.Text.Json;

namespace ShittimEcho.Core.Music
{
    /// <summary>
    /// ShittimEcho 音乐资源路径管理。
    /// 统一管理程序使用的音乐目录。
    /// </summary>
    public static class MusicPaths
    {
        /// <summary>
        /// ShittimEcho 的本地应用数据根目录。
        /// </summary>
        public static string ApplicationDataDirectory =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ShittimEcho");

        /// <summary>
        /// ShittimEcho 配置文件路径。
        /// </summary>
        public static string ConfigurationFilePath =>
            Path.Combine(
                ApplicationDataDirectory,
                "configuration.json");

        /// <summary>
        /// 音乐资源目录。
        ///
        /// 如果配置文件中存在有效的 MusicDirectory，
        /// 则使用用户配置的目录。
        ///
        /// 如果配置不存在、读取失败或路径无效，
        /// 则回退到默认 Music 目录。
        /// </summary>
        public static string MusicDirectory
        {
            get
            {
                string defaultDirectory =
                    Path.Combine(
                        ApplicationDataDirectory,
                        "Music");

                try
                {
                    if (!File.Exists(
                        ConfigurationFilePath))
                    {
                        return defaultDirectory;
                    }

                    string json =
                        File.ReadAllText(
                            ConfigurationFilePath);

                    using JsonDocument document =
                        JsonDocument.Parse(json);

                    if (!document.RootElement.TryGetProperty(
                        "MusicDirectory",
                        out JsonElement musicDirectoryElement))
                    {
                        // 兼容 MainWindow 当前保存的配置字段。
                        // 当前界面保存的是 MusicFolder。
                        if (!document.RootElement.TryGetProperty(
                            "MusicFolder",
                            out musicDirectoryElement))
                        {
                            return defaultDirectory;
                        }
                    }

                    string? configuredDirectory =
                        musicDirectoryElement.GetString();

                    if (string.IsNullOrWhiteSpace(
                        configuredDirectory))
                    {
                        return defaultDirectory;
                    }

                    return configuredDirectory;
                }
                catch
                {
                    return defaultDirectory;
                }
            }
        }

        /// <summary>
        /// 确保音乐资源目录存在。
        /// </summary>
        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(
                ApplicationDataDirectory);

            Directory.CreateDirectory(
                MusicDirectory);
        }
    }
}