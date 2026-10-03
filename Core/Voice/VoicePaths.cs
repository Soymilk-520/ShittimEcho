using System;
using System.IO;
using System.Text.Json;

namespace ShittimEcho.Core.Voice
{
    public static class VoicePaths
    {
        public static string RootDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "ShittimEcho");
            }
        }

        public static string ConfigurationFilePath
        {
            get
            {
                return Path.Combine(
                    RootDirectory,
                    "configuration.json");
            }
        }

        public static string VoiceDirectory
        {
            get
            {
                try
                {
                    if (File.Exists(ConfigurationFilePath))
                    {
                        string json =
                            File.ReadAllText(
                                ConfigurationFilePath);

                        using JsonDocument document =
                            JsonDocument.Parse(json);

                        if (document.RootElement.TryGetProperty(
                                "VoiceDirectory",
                                out JsonElement voiceDirectoryElement))
                        {
                            string? configuredDirectory =
                                voiceDirectoryElement.GetString();

                            if (!string.IsNullOrWhiteSpace(
                                    configuredDirectory))
                            {
                                return configuredDirectory;
                            }
                        }

                        // 兼容 MainWindow 当前保存的配置字段。
                        // 当前界面保存的是 VoiceFolder。
                        if (document.RootElement.TryGetProperty(
                                "VoiceFolder",
                                out JsonElement voiceFolderElement))
                        {
                            string? configuredDirectory =
                                voiceFolderElement.GetString();

                            if (!string.IsNullOrWhiteSpace(
                                    configuredDirectory))
                            {
                                return configuredDirectory;
                            }
                        }
                    }
                }
                catch
                {
                    // 配置文件读取失败时使用默认目录。
                }

                return Path.Combine(
                    RootDirectory,
                    "Voices");
            }
        }

        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(
                RootDirectory);

            Directory.CreateDirectory(
                VoiceDirectory);
        }
    }
}