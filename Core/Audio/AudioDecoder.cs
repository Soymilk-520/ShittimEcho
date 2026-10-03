using System;
using System.IO;
using NAudio.Vorbis;
using NAudio.Wave;

namespace ShittimEcho.Core.Audio
{
    /// <summary>
    /// 统一音频解码器。
    ///
    /// 根据文件扩展名选择对应的解码方式，
    /// 最终向 AudioEngine 提供统一的 WaveStream。
    ///
    /// 当前支持：
    /// .wav
    /// .mp3
    /// .ogg
    /// </summary>
    public sealed class AudioDecoder
    {
        /// <summary>
        /// 根据音频文件类型创建对应的 WaveStream。
        /// </summary>
        public WaveStream CreateWaveStream(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "Audio file path cannot be empty.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Audio file was not found.",
                    filePath);
            }

            string extension =
                Path.GetExtension(filePath);

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new NotSupportedException(
                    $"Audio file has no extension: {filePath}");
            }

            switch (extension.ToLowerInvariant())
            {
                case ".wav":
                    return CreateWaveFileReader(filePath);

                case ".mp3":
                    return CreateAudioFileReader(filePath);

                case ".ogg":
                    return CreateVorbisReader(filePath);

                default:
                    throw new NotSupportedException(
                        $"Unsupported audio format: {extension}");
            }
        }

        /// <summary>
        /// 创建 WAV 解码器。
        /// </summary>
        private static WaveStream CreateWaveFileReader(
            string filePath)
        {
            return new WaveFileReader(filePath);
        }

        /// <summary>
        /// 创建 MP3 解码器。
        ///
        /// 当前继续使用 NAudio 的 AudioFileReader。
        /// </summary>
        private static WaveStream CreateAudioFileReader(
            string filePath)
        {
            return new AudioFileReader(filePath);
        }

        /// <summary>
        /// 创建 OGG Vorbis 解码器。
        ///
        /// 这里明确使用 NAudio.Vorbis，
        /// 避免 OGG 文件进入 Media Foundation。
        /// </summary>
        private static WaveStream CreateVorbisReader(
            string filePath)
        {
            return new VorbisWaveReader(filePath);
        }
    }
}