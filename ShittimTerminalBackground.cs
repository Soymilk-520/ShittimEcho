using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShittimEcho
{
    public sealed class ShittimTerminalBackground : Grid
    {
        private readonly MediaElement _video;

        private bool _isReady;
        private bool _isStarting;

        public ShittimTerminalBackground()
        {
            HorizontalAlignment =
                System.Windows.HorizontalAlignment.Stretch;

            VerticalAlignment =
                System.Windows.VerticalAlignment.Stretch;

            ClipToBounds =
                true;

            IsHitTestVisible =
                false;

            Background =
                System.Windows.Media.Brushes.Black;

            _video =
                new MediaElement
                {
                    HorizontalAlignment =
                        System.Windows.HorizontalAlignment.Stretch,

                    VerticalAlignment =
                        System.Windows.VerticalAlignment.Stretch,

                    Stretch =
                        System.Windows.Media.Stretch.Fill,

                    LoadedBehavior =
                        MediaState.Manual,

                    UnloadedBehavior =
                        MediaState.Manual,

                    IsMuted =
                        true,

                    Volume =
                        0,

                    IsHitTestVisible =
                        false
                };

            Children.Add(
                _video);

            _video.MediaOpened +=
                Video_MediaOpened;

            _video.MediaEnded +=
                Video_MediaEnded;

            _video.MediaFailed +=
                Video_MediaFailed;

            Loaded +=
                Background_Loaded;

            Unloaded +=
                Background_Unloaded;
        }

        private void Background_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (_isStarting)
            {
                return;
            }

            _isStarting = true;

            string? videoPath =
                FindBackgroundVideo();

            if (videoPath == null)
            {
                _isStarting = false;
                return;
            }

            try
            {
                _isReady = false;

                _video.Stop();

                _video.Source = null;

                _video.Source =
                    new Uri(
                        videoPath,
                        UriKind.Absolute);
            }
            catch
            {
                _isReady = false;
                _isStarting = false;
            }
        }

        private string? FindBackgroundVideo()
        {
            string baseDirectory =
                AppContext.BaseDirectory;

            /*
             * 第一优先：
             * exe 所在目录
             */
            string path1 =
                Path.Combine(
                    baseDirectory,
                    "ShittimTerminalBackground.mp4");

            if (File.Exists(path1))
            {
                return path1;
            }

            /*
             * 第二优先：
             * 项目根目录。
             *
             * 方便你直接在 Visual Studio
             * 项目目录中放置 MP4 时使用。
             */
            DirectoryInfo? directory =
                new DirectoryInfo(
                    baseDirectory);

            for (int i = 0; i < 6 && directory != null; i++)
            {
                string candidate =
                    Path.Combine(
                        directory.FullName,
                        "ShittimTerminalBackground.mp4");

                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory =
                    directory.Parent;
            }

            return null;
        }

        private void Video_MediaOpened(
            object? sender,
            RoutedEventArgs e)
        {
            _isReady = true;
            _isStarting = false;

            try
            {
                _video.Position =
                    TimeSpan.Zero;

                _video.Play();
            }
            catch
            {
                _isReady = false;
            }
        }

        private void Video_MediaEnded(
            object? sender,
            RoutedEventArgs e)
        {
            if (!_isReady)
            {
                return;
            }

            try
            {
                _video.Position =
                    TimeSpan.Zero;

                _video.Play();
            }
            catch
            {
                _isReady = false;
            }
        }

        private void Video_MediaFailed(
            object? sender,
            ExceptionRoutedEventArgs e)
        {
            _isReady = false;
            _isStarting = false;
        }

        private void Background_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                _video.Stop();
            }
            catch
            {
            }

            _isReady = false;
            _isStarting = false;
        }
    }
}