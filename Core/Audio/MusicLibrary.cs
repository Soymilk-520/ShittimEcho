using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Core.Audio
{
    public sealed class MusicLibrary
    {
        private readonly StartupLogger _logger;

        private static readonly string[] SupportedExtensions =
        {
            ".wav",
            ".mp3",
            ".ogg"
        };

        public MusicLibrary(
            StartupLogger logger)
        {
            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));
        }

        public IReadOnlyList<string> Scan(
            string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException(
                    "Music directory path cannot be empty.",
                    nameof(directoryPath));
            }

            _logger.Write(
                $"Music Library scanning directory recursively: " +
                $"{directoryPath}");

            if (!Directory.Exists(directoryPath))
            {
                _logger.Write(
                    "Music Library root directory not found.");

                return Array.Empty<string>();
            }

            var musicFiles =
                new List<string>();

            try
            {
                ScanDirectory(
                    directoryPath,
                    musicFiles);
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Music Library recursive scan failed: {ex}");
            }

            musicFiles =
                musicFiles
                    .OrderBy(
                        path => path,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            _logger.Write(
                $"Music Library recursive scan completed. " +
                $"Music files found: {musicFiles.Count}");

            foreach (string filePath in musicFiles)
            {
                _logger.Write(
                    $"Music Library file: {filePath}");
            }

            return musicFiles;
        }

        private void ScanDirectory(
            string directoryPath,
            List<string> musicFiles)
        {
            string[] files;

            try
            {
                files =
                    Directory.GetFiles(
                        directoryPath,
                        "*.*",
                        SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Music Library could not access directory: " +
                    $"{directoryPath}");

                _logger.Write(
                    $"Music Library directory access error: {ex.Message}");

                return;
            }

            foreach (string filePath in files)
            {
                if (!IsSupportedAudioFile(filePath))
                    continue;

                musicFiles.Add(filePath);
            }

            string[] subDirectories;

            try
            {
                subDirectories =
                    Directory.GetDirectories(
                        directoryPath,
                        "*",
                        SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Music Library could not enumerate subdirectories: " +
                    $"{directoryPath}");

                _logger.Write(
                    $"Music Library subdirectory enumeration error: " +
                    $"{ex.Message}");

                return;
            }

            foreach (string subDirectory in subDirectories)
            {
                ScanDirectory(
                    subDirectory,
                    musicFiles);
            }
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