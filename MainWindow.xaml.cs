using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IOPath = System.IO.Path;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

using WpfApplication = System.Windows.Application;
using WpfButton = System.Windows.Controls.Button;
using WpfImage = System.Windows.Controls.Image;
using WpfMessageBox = System.Windows.MessageBox;
using WpfPanel = System.Windows.Controls.Panel;
using WpfRadioButton = System.Windows.Controls.RadioButton;

using DrawingIcon = System.Drawing.Icon;
using FormsContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using FormsNotifyIcon = System.Windows.Forms.NotifyIcon;
using FormsToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ShittimEcho
{
    public partial class MainWindow : Window
    {
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /*
         * ============================================================
         * 喵喵移动控制终端
         * ============================================================
         *
         * 主界面顶部专用窗口拖动区域。
         * 仅负责移动无边框窗口，不改变其他现有功能。
         */
        private void WindowMoveControlTerminal_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // 防止特殊鼠标状态影响程序其他功能。
            }
        }

        private const string FirstPassword =
            "吾等所望，那七声的哀叹";

        private const string SecondPassword =
            "吾等犹记，杰里科的古则";

        /*
         * 登录文字现在采用“整段浮现”，不再逐字打字。
         *
         * 第一段：1000ms
         * 两段之间：400ms
         * 第二段：1000ms
         *
         * 从第一段开始浮现到第二段完成浮现：
         * 1000 + 400 + 1000 = 2400ms
         */
        private const double PasswordDurationMilliseconds =
            1000.0;

        private const double LoadingBarWidth =
            600.0;

        private const double LoadingDurationMilliseconds =
            7200.0;

        private bool _authenticationStarted;

        private FormsNotifyIcon? _trayIcon;

        private bool _reallyExit;

        private readonly Ellipse[] _particles;

        private readonly List<BarParticle> _barParticles =
            new List<BarParticle>();

        private DispatcherTimer? _barParticleTimer;

        private double _particleSpawnAccumulator;

        private double _currentLoadingPercentage;

        private readonly Random _random =
            new Random(24117);

        private WpfImage? _terminalBackgroundImage;

        /*
         * ============================================================
         * 配置文件
         * ============================================================
         *
         * 保存位置：
         *
         * %LocalAppData%\ShittimEcho\configuration.json
         */
        private static readonly string ConfigurationDirectory =
            IOPath.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ShittimEcho");

        private static readonly string ConfigurationFilePath =
            IOPath.Combine(
                ConfigurationDirectory,
                "configuration.json");

        public MainWindow()
        {
            InitializeComponent();

            InitializeTrayIcon();

            EnsureTerminalBackgroundImage();
            EnsureLoadingLogo();

            _particles =
                new Ellipse[]
                {
                    Particle1,
                    Particle2,
                    Particle3,
                    Particle4,
                    Particle5,
                    Particle6,
                    Particle7,
                    Particle8,
                    Particle9,
                    Particle10
                };

            BrowseMusicButton.Click +=
                BrowseMusicButton_Click;

            BrowseVoiceButton.Click +=
                BrowseVoiceButton_Click;


            SaveConfigurationButton.Click +=
                SaveConfigurationButton_Click;

            MinimizeWindowButton.Click +=
                MinimizeWindowButton_Click;

            CloseWindowButton.Click +=
                CloseWindowButton_Click;

            SettingsButton.Click +=
                SettingsButton_Click;

            Loaded +=
                MainWindow_Loaded;

            Closing +=
    MainWindow_Closing;

            Closed +=
                MainWindow_Closed;
        }

        private async void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (_authenticationStarted)
            {
                return;
            }

            _authenticationStarted =
                true;

            await RunStartupSequenceAsync();
        }

        private void MinimizeWindowButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            WindowState =
                WindowState.Minimized;
        }

        private void CloseWindowButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        /*
         * ============================================================
         * 系统托盘
         * ============================================================
         *
         * − ：正常最小化到任务栏
         *
         * × ：隐藏窗口并进入系统托盘
         *
         * 托盘双击：
         * 恢复主窗口
         *
         * 托盘菜单：
         * 打开咸鱼喵喵
         * 退出程序
         *
         * 只有“退出程序”才真正结束进程。
         */
        private void InitializeTrayIcon()
        {
            _trayIcon =
                new FormsNotifyIcon();

            _trayIcon.Text =
                "咸鱼喵喵 · 喵露露终端";

            try
            {
                string? executablePath =
                    Process.GetCurrentProcess()
                        .MainModule?
                        .FileName;

                if (!string.IsNullOrWhiteSpace(
                    executablePath))
                {
                    DrawingIcon? icon =
                        DrawingIcon.ExtractAssociatedIcon(
                            executablePath);

                    if (icon != null)
                    {
                        _trayIcon.Icon =
                            icon;
                    }
                }
            }
            catch
            {
                // 托盘图标获取失败不影响主程序运行。
            }

            var contextMenu =
                new FormsContextMenuStrip();

            var openMenuItem =
                new FormsToolStripMenuItem(
                    "打开咸鱼喵喵");

            var exitMenuItem =
                new FormsToolStripMenuItem(
                    "退出程序");

            openMenuItem.Click +=
                (_, _) =>
                {
                    Dispatcher.Invoke(
                        ShowMainWindow);
                };

            exitMenuItem.Click +=
                (_, _) =>
                {
                    Dispatcher.Invoke(
                        ExitApplication);
                };

            contextMenu.Items.Add(
                openMenuItem);

            contextMenu.Items.Add(
                new System.Windows.Forms.ToolStripSeparator());

            contextMenu.Items.Add(
                exitMenuItem);

            _trayIcon.ContextMenuStrip =
                contextMenu;

            _trayIcon.DoubleClick +=
                (_, _) =>
                {
                    Dispatcher.Invoke(
                        ShowMainWindow);
                };

            _trayIcon.Visible =
                true;
        }

        private void ShowMainWindow()
        {
            Show();

            ShowInTaskbar =
                true;

            if (WindowState ==
                WindowState.Minimized)
            {
                WindowState =
                    WindowState.Normal;
            }

            Activate();

            bool previousTopmost =
                Topmost;

            Topmost =
                true;

            Topmost =
                previousTopmost;

            Focus();
        }

        private void MainWindow_Closing(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            if (_reallyExit)
            {
                return;
            }

            e.Cancel =
                true;

            Hide();

            ShowInTaskbar =
                false;
        }

        private void MainWindow_Closed(
            object? sender,
            EventArgs e)
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible =
                    false;

                _trayIcon.Dispose();

                _trayIcon =
                    null;
            }
        }

        private void ExitApplication()
        {
            _reallyExit =
                true;

            if (_trayIcon != null)
            {
                _trayIcon.Visible =
                    false;
            }

            WpfApplication.Current.Shutdown();
        }

        private void SettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var settingsWindow =
                new SettingsWindow
                {
                    Owner = this,
                    WindowStartupLocation =
                        WindowStartupLocation.CenterOwner
                };

            settingsWindow.ShowDialog();
        }

        private void BrowseMusicButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string? selectedFolder =
                SelectFolder();

            if (string.IsNullOrWhiteSpace(
                selectedFolder))
            {
                return;
            }

            MusicPathTextBox.Text =
                selectedFolder;
        }

        private void BrowseVoiceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string? selectedFolder =
                SelectFolder();

            if (string.IsNullOrWhiteSpace(
                selectedFolder))
            {
                return;
            }

            VoicePathTextBox.Text =
                selectedFolder;
        }

        /*
         * ============================================================
         * Windows 原生选择文件夹
         * ============================================================
         */
        private static string? SelectFolder()
        {
            var dialog =
                new FileOpenDialog();

            var openDialog =
                (IFileOpenDialog)dialog;

            openDialog.GetOptions(
                out uint options);

            options |=
                FileOpenDialogOptions.PickFolders;

            options |=
                FileOpenDialogOptions.ForceFileSystem;

            openDialog.SetOptions(
                options);

            openDialog.SetTitle(
                "选择文件夹");

            try
            {
                int result =
                    openDialog.Show(
                        IntPtr.Zero);

                if (result != 0)
                {
                    return null;
                }

                openDialog.GetResult(
                    out IShellItem item);

                item.GetDisplayName(
                    ShellItemDisplayName.FileSystemPath,
                    out IntPtr pathPointer);

                try
                {
                    return Marshal.PtrToStringUni(
                        pathPointer);
                }
                finally
                {
                    if (pathPointer != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(
                            pathPointer);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /*
         * ============================================================
         * 保存配置
         * ============================================================
         */
        private void SaveConfigurationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string musicPath =
                    MusicPathTextBox.Text?.Trim()
                    ?? string.Empty;

                string voicePath =
                    VoicePathTextBox.Text?.Trim()
                    ?? string.Empty;


                var configuration =
    LoadConfiguration();


                configuration.MusicFolder =
                    musicPath;

                configuration.VoiceFolder =
                    voicePath;


                configuration.PlayMusicOnLock =
                    PlayMusicOnLockCheckBox.IsChecked == true;


                configuration.StopMusicOnUnlock =
                    StopMusicOnUnlockCheckBox.IsChecked == true;


                configuration.PlayWelcomeVoice =
                    PlayWelcomeVoiceCheckBox.IsChecked == true;

                Directory.CreateDirectory(
                    ConfigurationDirectory);

                var json =
                    JsonSerializer.Serialize(
                        configuration,
                        new JsonSerializerOptions
                        {
                            WriteIndented =
                                true
                        });

                File.WriteAllText(
                    ConfigurationFilePath,
                    json);

                WpfMessageBox.Show(
                    this,
                    "配置已保存。",
                    "咸鱼喵喵·喵露露终端",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                WpfMessageBox.Show(
                    this,
                    "保存配置失败。\n\n" +
                    exception.Message,
                    "咸鱼喵喵·喵露露终端",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /*
         * ============================================================
         * 读取配置
         * ============================================================
         */
        private static AppConfiguration
            LoadConfiguration()
        {
            try
            {
                if (!File.Exists(
                    ConfigurationFilePath))
                {
                    return new AppConfiguration();
                }

                string json =
                    File.ReadAllText(
                        ConfigurationFilePath);

                AppConfiguration? configuration =
                    JsonSerializer.Deserialize<AppConfiguration>(
                        json);

                return configuration
                    ?? new AppConfiguration();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    ex.ToString(),
                    "配置读取错误");

                return new AppConfiguration();
            }
        }

        /*
         * ============================================================
         * 启动背景
         * ============================================================
         */
        private void EnsureTerminalBackgroundImage()
        {
            if (_terminalBackgroundImage != null)
            {
                return;
            }

            var image =
                new WpfImage
                {
                    Stretch =
                        Stretch.UniformToFill,

                    IsHitTestVisible =
                        false,

                    Opacity =
                        0.0,

                    Source =
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/ShittimBackground.png",
                                UriKind.Absolute))
                };

            _terminalBackgroundImage =
                image;

            if (Content is WpfPanel rootPanel)
            {
                rootPanel.Children.Insert(
                    0,
                    image);

                System.Windows.Controls.Panel.SetZIndex(
                    image,
                    -100);
            }
        }

        /*
         * ============================================================
         * Loading Logo
         * ============================================================
         */
        private void EnsureLoadingLogo()
        {
            if (LoadingLogoImage == null)
            {
                return;
            }

            var imageBrush =
                new ImageBrush
                {
                    ImageSource =
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/ShittimLoadingLogo.png",
                                UriKind.Absolute)),

                    Stretch =
                        Stretch.Uniform
                };

            LoadingLogoImage.Fill =
                imageBrush;
        }

        private async Task RunStartupSequenceAsync()
        {
            if (_terminalBackgroundImage == null)
            {
                return;
            }

            _terminalBackgroundImage.Opacity =
                0.0;

            await AnimateOpacityAsync(
                _terminalBackgroundImage,
                0.0,
                1.0,
                560);

            await RunAuthenticationAsync();

            await RunLoadingViewAsync();

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    500));

            await ShowWelcomeSequenceAsync();

            await ShowConfigurationViewAsync();
        }

        private async Task RunAuthenticationAsync()
        {
            FirstPasswordText.Text =
                "「......." +
                FirstPassword +
                "」";

            SecondPasswordText.Text =
                "「......." +
                SecondPassword +
                "」";

            FirstPasswordText.Inlines.Clear();
            SecondPasswordText.Inlines.Clear();

            FirstPasswordText.Text =
                "「......." +
                FirstPassword +
                "」";

            SecondPasswordText.Text =
                "「......." +
                SecondPassword +
                "」";

            FirstPasswordText.Opacity =
                0.0;

            SecondPasswordText.Opacity =
                0.0;

            AuthenticationGlow.Opacity =
                0.0;

            await AnimateOpacityAsync(
                AuthenticationGlow,
                0.0,
                0.16,
                120);

            await AnimateOpacityAsync(
                AuthenticationGlow,
                0.16,
                0.035,
                180);

            await AnimateOpacityAsync(
                AuthenticationGlow,
                0.035,
                0.095,
                130);

            await AnimateOpacityAsync(
                AuthenticationGlow,
                0.095,
                0.025,
                180);

            await AnimateOpacityAsync(
                AuthenticationGlow,
                0.025,
                0.0,
                140);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    70));

            await AnimateOpacityAsync(
                FirstPasswordText,
                0.0,
                1.0,
                PasswordDurationMilliseconds);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    400));

            await AnimateOpacityAsync(
                SecondPasswordText,
                0.0,
                1.0,
                PasswordDurationMilliseconds);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    1400));

            Task firstTextFadeTask =
                AnimateOpacityAsync(
                    FirstPasswordText,
                    1.0,
                    0.0,
                    520);

            Task secondTextFadeTask =
                AnimateOpacityAsync(
                    SecondPasswordText,
                    1.0,
                    0.0,
                    520);

            Task glowFadeTask =
                AnimateOpacityAsync(
                    AuthenticationGlow,
                    AuthenticationGlow.Opacity,
                    0.0,
                    380);

            Task authenticationViewFadeTask =
                AnimateOpacityAsync(
                    AuthenticationView,
                    1.0,
                    0.0,
                    520);

            await Task.WhenAll(
                firstTextFadeTask,
                secondTextFadeTask,
                glowFadeTask,
                authenticationViewFadeTask);

            AuthenticationView.Visibility =
                Visibility.Collapsed;
        }

        private async Task RunLoadingViewAsync()
        {
            LoadingView.Visibility =
                Visibility.Visible;

            LoadingView.Opacity =
                0.0;

            LoadingTitleText.Opacity =
                0.0;

            LoadingCore.Opacity =
                0.0;

            LoadingMessagesPanel.Opacity =
                1.0;

            FinalLoadingText.Opacity =
                0.0;

            LoadingBarFill.BeginAnimation(
                FrameworkElement.WidthProperty,
                null);

            LoadingBarFill.Width =
                0.0;

            LoadingBarFill.Opacity =
                1.0;

            LoadingPercentageText.BeginAnimation(
                UIElement.OpacityProperty,
                null);

            LoadingPercentageText.Opacity =
                0.9;

            LoadingPercentageText.Text =
                "0%";

            _currentLoadingPercentage =
                0.0;

            ResetLoadingMessages();

            UpdateLoadingVisuals(
                0.0);

            await AnimateOpacityAsync(
                LoadingView,
                0.0,
                1.0,
                300);

            StartAmbientGlow();

            StartParticles();

            StartBarParticleSystem();

            await AnimateOpacityAsync(
                LoadingTitleText,
                0.0,
                1.0,
                420);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    80));

            await AnimateOpacityAsync(
                LoadingCore,
                0.0,
                1.0,
                420);

            await PulseCoreAsync();

            Task progressTask =
                AnimateLoadingProgressAsync(
                    100.0,
                    LoadingDurationMilliseconds);

            await ShowLoadingRowAsync(
                LoadingMessage1,
                LoadingDot1);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    170));

            await ShowLoadingRowAsync(
                LoadingMessage2,
                LoadingDot2);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    170));

            await ShowLoadingRowAsync(
                LoadingMessage3,
                LoadingDot3);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    170));

            await ShowLoadingRowAsync(
                LoadingMessage4,
                LoadingDot4);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    170));

            await ShowLoadingRowAsync(
                LoadingMessage5,
                LoadingDot5);

            await progressTask;

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    80));

            await ShowFinalLoadingMessageAsync(
                "连接确认",
                1000);

            await ShowFinalLoadingMessageAsync(
                "身份认证系统已通过",
                420);

            FinalLoadingText.Text =
                "正在确认终端权限……";

            FinalLoadingText.Opacity =
                0.0;

            Task permissionMessageTask =
                AnimateOpacityAsync(
                    FinalLoadingText,
                    0.0,
                    1.0,
                    400);

            Task loadingBarFadeTask =
                FadeCompletedLoadingBarAsync();

            await Task.WhenAll(
                permissionMessageTask,
                loadingBarFadeTask);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    280));

            FinalLoadingText.Text =
                "正在进入基沃托斯……";

            FinalLoadingText.Opacity =
                0.0;

            FinalLoadingTextTransform.Y =
                0.0;

            await AnimateOpacityAsync(
                FinalLoadingText,
                0.0,
                1.0,
                400);

            await PulseCoreAsync();

            Task titleFadeTask =
                AnimateOpacityAsync(
                    LoadingTitleText,
                    1.0,
                    0.0,
                    760);

            Task coreFadeTask =
                AnimateOpacityAsync(
                    LoadingCore,
                    1.0,
                    0.0,
                    760);

            Task messageFadeTask =
                AnimateOpacityAsync(
                    LoadingMessagesPanel,
                    1.0,
                    0.0,
                    760);

            Task finalTextMoveTask =
                AnimateTranslateYAsync(
                    FinalLoadingTextTransform,
                    0.0,
                    -150.0,
                    760);

            await Task.WhenAll(
                titleFadeTask,
                coreFadeTask,
                messageFadeTask,
                finalTextMoveTask);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    720));

            Task backgroundFadeTask =
                _terminalBackgroundImage == null
                    ? Task.CompletedTask
                    : AnimateOpacityAsync(
                        _terminalBackgroundImage,
                        _terminalBackgroundImage.Opacity,
                        0.0,
                        650);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    140));

            Task finalTextFadeTask =
                AnimateOpacityAsync(
                    FinalLoadingText,
                    1.0,
                    0.0,
                    500);

            await Task.WhenAll(
                finalTextFadeTask,
                backgroundFadeTask);

            if (_terminalBackgroundImage != null)
            {
                _terminalBackgroundImage.Opacity =
                    0.0;
            }

            StopBarParticleSystem();

            await StopParticlesAsync();

            await AnimateOpacityAsync(
                LoadingView,
                1.0,
                0.0,
                450);

            LoadingView.Visibility =
                Visibility.Collapsed;
        }

        private async Task ShowLoadingRowAsync(
            TextBlock textBlock,
            UIElement dot)
        {
            textBlock.Opacity =
                0.0;

            dot.Opacity =
                0.0;

            await AnimateOpacityAsync(
                dot,
                0.0,
                1.0,
                240);

            await AnimateOpacityAsync(
                textBlock,
                0.0,
                1.0,
                700);

            await AnimateOpacityAsync(
                dot,
                1.0,
                0.85,
                160);
        }

        private async Task ShowFinalLoadingMessageAsync(
            string message,
            int displayMilliseconds)
        {
            FinalLoadingText.Text =
                message;

            FinalLoadingText.Opacity =
                0.0;

            await AnimateOpacityAsync(
                FinalLoadingText,
                0.0,
                1.0,
                400);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    displayMilliseconds));

            await AnimateOpacityAsync(
                FinalLoadingText,
                1.0,
                0.0,
                260);
        }

        private async Task AnimateLoadingProgressAsync(
            double targetPercentage,
            double durationMilliseconds)
        {
            double startPercentage =
                _currentLoadingPercentage;

            double difference =
                targetPercentage -
                startPercentage;

            if (Math.Abs(difference) <
                0.01)
            {
                UpdateLoadingVisuals(
                    targetPercentage);

                return;
            }

            var stopwatch =
                Stopwatch.StartNew();

            var timer =
                new DispatcherTimer(
                    DispatcherPriority.Render)
                {
                    Interval =
                        TimeSpan.FromMilliseconds(
                            16)
                };

            var tcs =
                new TaskCompletionSource<bool>();

            EventHandler? tickHandler =
                null;

            tickHandler =
                (_, _) =>
                {
                    double elapsed =
                        stopwatch.Elapsed
                            .TotalMilliseconds;

                    double normalized =
                        elapsed /
                        durationMilliseconds;

                    if (normalized >=
                        1.0)
                    {
                        normalized =
                            1.0;
                    }

                    double easedProgress =
                        EaseInOutCubic(
                            normalized);

                    double percentage =
                        startPercentage +
                        difference *
                        easedProgress;

                    UpdateLoadingVisuals(
                        percentage);

                    if (normalized >=
                        1.0)
                    {
                        timer.Stop();

                        stopwatch.Stop();

                        UpdateLoadingVisuals(
                            targetPercentage);

                        if (tickHandler !=
                            null)
                        {
                            timer.Tick -=
                                tickHandler;
                        }

                        tcs.TrySetResult(
                            true);
                    }
                };

            timer.Tick +=
                tickHandler;

            timer.Start();

            await tcs.Task;

            if (tickHandler != null)
            {
                timer.Tick -=
                    tickHandler;
            }
        }

        private void UpdateLoadingVisuals(
            double percentage)
        {
            if (percentage < 0.0)
            {
                percentage =
                    0.0;
            }

            if (percentage > 100.0)
            {
                percentage =
                    100.0;
            }

            _currentLoadingPercentage =
                percentage;

            double width =
                LoadingBarWidth *
                percentage /
                100.0;

            LoadingBarFill.Width =
                width;

            LoadingPercentageText.Text =
                $"{percentage:0}%";

            UpdateTipPosition(
                width);
        }

        private void UpdateTipPosition(
            double width)
        {
            double left =
                10.0 +
                width;

            if (left > 610.0)
            {
                left =
                    610.0;
            }

            Canvas.SetLeft(
                LoadingBarTip,
                left);
        }

        private void UpdateTipParticles(
            double width)
        {
            double left =
                10.0 +
                width;

            if (left > 610.0)
            {
                left =
                    610.0;
            }

            double phase =
                width %
                18.0;

            double intensity =
                0.35 +
                phase /
                18.0 *
                0.45;

            TipParticle1.Opacity =
                intensity;

            TipParticle2.Opacity =
                0.30 +
                phase /
                18.0 *
                0.35;

            TipParticle3.Opacity =
                0.45 +
                phase /
                18.0 *
                0.45;

            TipParticle4.Opacity =
                0.20 +
                phase /
                18.0 *
                0.35;

            Canvas.SetLeft(
                TipParticle1,
                left - 2.0);

            Canvas.SetLeft(
                TipParticle2,
                left + 2.0);

            Canvas.SetLeft(
                TipParticle3,
                left);

            Canvas.SetLeft(
                TipParticle4,
                left + 1.0);
        }

        private UIElement? GetLoadingBarVisualContainer()
        {
            DependencyObject? current =
                LoadingBarFill;

            current =
                VisualTreeHelper.GetParent(
                    current);

            if (current == null)
            {
                return null;
            }

            current =
                VisualTreeHelper.GetParent(
                    current);

            return current as UIElement;
        }

        private async Task FadeCompletedLoadingBarAsync()
        {
            UIElement? container =
                GetLoadingBarVisualContainer();

            if (container != null)
            {
                await AnimateOpacityAsync(
                    container,
                    1.0,
                    0.0,
                    550);

                container.Opacity =
                    0.0;

                return;
            }

            Task barTask =
                AnimateOpacityAsync(
                    LoadingBarFill,
                    LoadingBarFill.Opacity,
                    0.0,
                    500);

            Task percentageTask =
                AnimateOpacityAsync(
                    LoadingPercentageText,
                    LoadingPercentageText.Opacity,
                    0.0,
                    350);

            await Task.WhenAll(
                barTask,
                percentageTask);
        }

        private void StartBarParticleSystem()
        {
            StopBarParticleSystem();

            _barParticles.Clear();

            _particleSpawnAccumulator =
                0.0;

            LoadingBarTip.Opacity =
                1.0;

            _barParticleTimer =
                new DispatcherTimer(
                    DispatcherPriority.Render)
                {
                    Interval =
                        TimeSpan.FromMilliseconds(
                            16)
                };

            _barParticleTimer.Tick +=
                BarParticleTimer_Tick;

            _barParticleTimer.Start();
        }

        private void BarParticleTimer_Tick(
            object? sender,
            EventArgs e)
        {
            const double deltaTime =
                0.016;

            _particleSpawnAccumulator +=
                deltaTime;

            if (_particleSpawnAccumulator >=
                0.08)
            {
                _particleSpawnAccumulator =
                    0.0;

                SpawnBarParticle();
            }

            for (int i =
                 _barParticles.Count - 1;
                 i >= 0;
                 i--)
            {
                BarParticle particle =
                    _barParticles[i];

                particle.Age +=
                    deltaTime;

                double lifeProgress =
                    particle.Age /
                    particle.Lifetime;

                if (lifeProgress >=
                    1.0)
                {
                    RemoveBarParticle(
                        particle);

                    _barParticles.RemoveAt(
                        i);

                    continue;
                }

                particle.X +=
                    particle.VelocityX *
                    deltaTime;

                particle.Y +=
                    particle.VelocityY *
                    deltaTime;

                particle.VelocityY +=
                    particle.Gravity *
                    deltaTime;

                Canvas.SetLeft(
                    particle.Shape,
                    particle.X);

                Canvas.SetTop(
                    particle.Shape,
                    particle.Y);

                particle.Rotation.Angle +=
                    particle.RotationSpeed *
                    deltaTime;

                double opacity;

                if (lifeProgress <
                    0.18)
                {
                    opacity =
                        lifeProgress /
                        0.18;
                }
                else
                {
                    opacity =
                        1.0 -
                        (lifeProgress - 0.18) /
                        0.82;
                }

                if (opacity <
                    0.0)
                {
                    opacity =
                        0.0;
                }

                if (opacity >
                    0.8)
                {
                    opacity =
                        0.8;
                }

                particle.Shape.Opacity =
                    opacity;
            }

            UpdateTipParticles(
                LoadingBarWidth *
                _currentLoadingPercentage /
                100.0);
        }

        private void SpawnBarParticle()
        {
            if (LoadingView.Visibility !=
                Visibility.Visible)
            {
                return;
            }

            if (_currentLoadingPercentage <=
                1.0)
            {
                return;
            }

            Canvas? canvas =
                LoadingBarTip.Parent
                as Canvas;

            if (canvas == null)
            {
                return;
            }

            double width =
                LoadingBarWidth *
                _currentLoadingPercentage /
                100.0;

            double tipX =
                10.0 +
                width;

            double centerY =
                16.0;

            Polygon triangle =
                new Polygon
                {
                    Points =
                        new PointCollection
                        {
                            new System.Windows.Point(0, 0),
                            new System.Windows.Point(7, 3.5),
                            new System.Windows.Point(0, 7)
                        },

                    Fill =
                        new SolidColorBrush(
                            System.Windows.Media.Colors.White),

                    Opacity =
                        0.0
                };

            var rotation =
                new RotateTransform(
                    0.0,
                    3.5,
                    3.5);

            triangle.RenderTransform =
                rotation;

            canvas.Children.Add(
                triangle);

            int rule =
                _random.Next(
                    0,
                    4);

            double angle;
            double speed;
            double verticalSpeed;
            double gravity;
            double rotationSpeed;
            double lifetime;

            switch (rule)
            {
                case 0:

                    angle =
                        _random.NextDouble() *
                        1.0 -
                        0.5;

                    speed =
                        34.0 +
                        _random.NextDouble() *
                        26.0;

                    verticalSpeed =
                        -20.0 +
                        _random.NextDouble() *
                        15.0;

                    gravity =
                        6.0;

                    rotationSpeed =
                        120.0 +
                        _random.NextDouble() *
                        160.0;

                    lifetime =
                        0.42 +
                        _random.NextDouble() *
                        0.24;

                    break;

                case 1:

                    angle =
                        -0.8 +
                        _random.NextDouble() *
                        1.6;

                    speed =
                        28.0 +
                        _random.NextDouble() *
                        28.0;

                    verticalSpeed =
                        -40.0 +
                        _random.NextDouble() *
                        20.0;

                    gravity =
                        12.0;

                    rotationSpeed =
                        -220.0 +
                        _random.NextDouble() *
                        440.0;

                    lifetime =
                        0.48 +
                        _random.NextDouble() *
                        0.25;

                    break;

                case 2:

                    angle =
                        -1.1 +
                        _random.NextDouble() *
                        2.2;

                    speed =
                        20.0 +
                        _random.NextDouble() *
                        24.0;

                    verticalSpeed =
                        -10.0 +
                        _random.NextDouble() *
                        35.0;

                    gravity =
                        -2.0;

                    rotationSpeed =
                        -160.0 +
                        _random.NextDouble() *
                        320.0;

                    lifetime =
                        0.55 +
                        _random.NextDouble() *
                        0.30;

                    break;

                default:

                    angle =
                        -0.35 +
                        _random.NextDouble() *
                        0.7;

                    speed =
                        42.0 +
                        _random.NextDouble() *
                        22.0;

                    verticalSpeed =
                        -8.0 +
                        _random.NextDouble() *
                        16.0;

                    gravity =
                        18.0;

                    rotationSpeed =
                        -300.0 +
                        _random.NextDouble() *
                        600.0;

                    lifetime =
                        0.38 +
                        _random.NextDouble() *
                        0.18;

                    break;
            }

            double velocityX =
                -Math.Cos(angle) *
                speed;

            double velocityY =
                Math.Sin(angle) *
                speed +
                verticalSpeed;

            var particle =
                new BarParticle
                {
                    Shape =
                        triangle,

                    Rotation =
                        rotation,

                    X =
                        tipX,

                    Y =
                        centerY - 3.5,

                    VelocityX =
                        velocityX,

                    VelocityY =
                        velocityY,

                    Gravity =
                        gravity,

                    RotationSpeed =
                        rotationSpeed,

                    Lifetime =
                        lifetime,

                    Age =
                        0.0
                };

            _barParticles.Add(
                particle);
        }

        private void RemoveBarParticle(
            BarParticle particle)
        {
            Canvas? canvas =
                particle.Shape.Parent
                as Canvas;

            canvas?.Children.Remove(
                particle.Shape);
        }

        private void StopBarParticleSystem()
        {
            if (_barParticleTimer !=
                null)
            {
                _barParticleTimer.Stop();

                _barParticleTimer.Tick -=
                    BarParticleTimer_Tick;

                _barParticleTimer =
                    null;
            }

            foreach (BarParticle particle
                     in _barParticles)
            {
                RemoveBarParticle(
                    particle);
            }

            _barParticles.Clear();

            _particleSpawnAccumulator =
                0.0;
        }

        private async Task PulseCoreAsync()
        {
            await AnimateOpacityAsync(
                LoadingCorePoint,
                0.55,
                1.0,
                120);

            await AnimateOpacityAsync(
                LoadingCorePoint,
                1.0,
                0.55,
                220);
        }

        private void StartAmbientGlow()
        {
            /*
             * 保持上一版状态：
             * 中央阴影光晕不启动。
             */
        }

        private void StartParticles()
        {
            foreach (Ellipse particle
                     in _particles)
            {
                particle.Opacity =
                    0.0;

                var animation =
                    new DoubleAnimation
                    {
                        From =
                            0.0,

                        To =
                            0.45,

                        Duration =
                            TimeSpan.FromMilliseconds(
                                1000),

                        AutoReverse =
                            true,

                        RepeatBehavior =
                            RepeatBehavior.Forever,

                        BeginTime =
                            TimeSpan.FromMilliseconds(
                                particle.Name.Length *
                                90),

                        EasingFunction =
                            new SineEase
                            {
                                EasingMode =
                                    EasingMode.EaseInOut
                            }
                    };

                particle.BeginAnimation(
                    UIElement.OpacityProperty,
                    animation);
            }
        }

        private async Task StopParticlesAsync()
        {
            var tasks =
                new List<Task>();

            foreach (Ellipse particle
                     in _particles)
            {
                particle.BeginAnimation(
                    UIElement.OpacityProperty,
                    null);

                tasks.Add(
                    AnimateOpacityAsync(
                        particle,
                        particle.Opacity,
                        0.0,
                        180));
            }

            await Task.WhenAll(
                tasks);
        }

        private async Task ShowWelcomeSequenceAsync()
        {
            WelcomeView.Visibility =
                Visibility.Visible;

            WelcomeView.Opacity =
                1.0;

            WelcomeText.Opacity =
                0.0;

            WelcomeText.Foreground =
                System.Windows.Media.Brushes.White;

            WelcomeGlow.Opacity =
                0.0;

            await AnimateOpacityAsync(
                WelcomeGlow,
                0.0,
                0.035,
                520);

            await AnimateOpacityAsync(
                WelcomeText,
                0.0,
                1.0,
                320);

            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    1900));

            await AnimateOpacityAsync(
                WelcomeText,
                1.0,
                0.0,
                800);

            await AnimateOpacityAsync(
                WelcomeGlow,
                0.035,
                0.0,
                450);

            await AnimateOpacityAsync(
                WelcomeView,
                1.0,
                0.0,
                450);

            WelcomeView.Visibility =
                Visibility.Collapsed;
        }

        private async Task ShowConfigurationViewAsync()
        {
            AppConfiguration configuration =
                LoadConfiguration();

            MusicPathTextBox.Text =
                configuration.MusicFolder
                ?? string.Empty;

            VoicePathTextBox.Text =
                configuration.VoiceFolder
                ?? string.Empty;

            PlayMusicOnLockCheckBox.IsChecked =
    configuration.PlayMusicOnLock;


            StopMusicOnUnlockCheckBox.IsChecked =
                configuration.StopMusicOnUnlock;


            PlayWelcomeVoiceCheckBox.IsChecked =
                configuration.PlayWelcomeVoice;

            ConfigurationView.Visibility =
                Visibility.Visible;

            ConfigurationView.Opacity =
                0.0;

            await AnimateOpacityAsync(
                ConfigurationView,
                0.0,
                1.0,
                750);
        }

        private void ConfigurationView_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            /*
             * 当前阶段保持现有布局不变。
             * 后续如果需要进行自适应布局，
             * 将只在这里追加相关逻辑。
             */
        }

        private static async Task AnimateTranslateYAsync(
            TranslateTransform transform,
            double from,
            double to,
            double durationMilliseconds)
        {
            if (transform == null)
            {
                return;
            }

            var animation =
                new DoubleAnimation
                {
                    From =
                        from,

                    To =
                        to,

                    Duration =
                        TimeSpan.FromMilliseconds(
                            durationMilliseconds),

                    EasingFunction =
                        new CubicEase
                        {
                            EasingMode =
                                EasingMode.EaseInOut
                        }
                };

            var tcs =
                new TaskCompletionSource<bool>();

            animation.Completed +=
                (_, _) =>
                    tcs.TrySetResult(
                        true);

            transform.BeginAnimation(
                TranslateTransform.YProperty,
                animation);

            await tcs.Task;
        }

        private async Task TypeTextAsync(
            TextBlock textBlock,
            string text,
            double totalDurationMilliseconds)
        {
            if (textBlock == null ||
                string.IsNullOrEmpty(text))
            {
                return;
            }

            textBlock.Text =
                string.Empty;

            textBlock.Inlines.Clear();

            textBlock.TextAlignment =
                TextAlignment.Center;

            const string openingQuote =
                "「";

            const string dots =
                ".......";

            const string closingQuote =
                "」";

            string body =
                text
                    .TrimStart('「')
                    .TrimEnd('」');

            if (string.IsNullOrEmpty(body))
            {
                return;
            }

            string fullText =
                openingQuote +
                dots +
                body +
                closingQuote;

            var transparentColor =
                System.Windows.Media.Color.FromArgb(
                    0,
                    216,
                    90,
                    74);

            var visibleColor =
                System.Windows.Media.Color.FromArgb(
                    255,
                    216,
                    90,
                    74);

            var runs =
                new List<Run>();

            for (int i = 0;
                 i < fullText.Length;
                 i++)
            {
                var brush =
                    new SolidColorBrush(
                        transparentColor);

                var run =
                    new Run(
                        fullText[i].ToString())
                    {
                        Foreground =
                            brush
                    };

                textBlock.Inlines.Add(
                    run);

                runs.Add(
                    run);
            }

            var groups =
                new List<List<int>>();

            groups.Add(
                new List<int>
                {
                    0,
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8
                });

            for (int index = 9;
                 index <
                 fullText.Length - 1;
                 index++)
            {
                groups.Add(
                    new List<int>
                    {
                        index
                    });
            }

            if (fullText.Length > 10)
            {
                int lastBodyIndex =
                    fullText.Length - 2;

                groups.RemoveAt(
                    groups.Count - 1);

                groups.Add(
                    new List<int>
                    {
                        lastBodyIndex,
                        fullText.Length - 1
                    });
            }
            else
            {
                groups.Add(
                    new List<int>
                    {
                        fullText.Length - 1
                    });
            }

            double intervalMilliseconds =
                totalDurationMilliseconds /
                Math.Max(
                    1,
                    groups.Count - 1);

            for (int groupIndex = 0;
                 groupIndex < groups.Count;
                 groupIndex++)
            {
                var animationTasks =
                    new List<Task>();

                foreach (int index
                         in groups[groupIndex])
                {
                    if (index < 0 ||
                        index >= runs.Count)
                    {
                        continue;
                    }

                    if (runs[index].Foreground
                        is not SolidColorBrush brush)
                    {
                        continue;
                    }

                    var animation =
                        new ColorAnimation
                        {
                            From =
                                transparentColor,

                            To =
                                visibleColor,

                            Duration =
                                TimeSpan.FromMilliseconds(
                                    65),

                            EasingFunction =
                                new CubicEase
                                {
                                    EasingMode =
                                        EasingMode.EaseOut
                                }
                        };

                    animationTasks.Add(
                        AnimateRunColorAsync(
                            brush,
                            animation));
                }

                await Task.WhenAll(
                    animationTasks);

                if (groupIndex <
                    groups.Count - 1)
                {
                    await Task.Delay(
                        TimeSpan.FromMilliseconds(
                            intervalMilliseconds));
                }
            }

            textBlock.Text =
                fullText;
        }

        private static async Task AnimateRunColorAsync(
            SolidColorBrush brush,
            ColorAnimation animation)
        {
            if (brush == null ||
                animation == null)
            {
                return;
            }

            var tcs =
                new TaskCompletionSource<bool>();

            animation.Completed +=
                (_, _) =>
                    tcs.TrySetResult(
                        true);

            brush.BeginAnimation(
                SolidColorBrush.ColorProperty,
                animation);

            await tcs.Task;
        }

        private void ResetLoadingMessages()
        {
            LoadingMessage1.Opacity =
                0.0;

            LoadingMessage2.Opacity =
                0.0;

            LoadingMessage3.Opacity =
                0.0;

            LoadingMessage4.Opacity =
                0.0;

            LoadingMessage5.Opacity =
                0.0;

            LoadingDot1.Opacity =
                0.0;

            LoadingDot2.Opacity =
                0.0;

            LoadingDot3.Opacity =
                0.0;

            LoadingDot4.Opacity =
                0.0;

            LoadingDot5.Opacity =
                0.0;

            FinalLoadingText.Opacity =
                0.0;
        }

        private static double EaseInOutCubic(
            double value)
        {
            if (value < 0.5)
            {
                return
                    4.0 *
                    value *
                    value *
                    value;
            }

            double t =
                -2.0 *
                value +
                2.0;

            return
                1.0 -
                t *
                t *
                t /
                2.0;
        }

        private static async Task AnimateOpacityAsync(
            UIElement? element,
            double from,
            double to,
            double durationMilliseconds)
        {
            if (element == null)
            {
                return;
            }

            var animation =
                new DoubleAnimation
                {
                    From =
                        from,

                    To =
                        to,

                    Duration =
                        TimeSpan.FromMilliseconds(
                            durationMilliseconds),

                    EasingFunction =
                        new CubicEase
                        {
                            EasingMode =
                                EasingMode.EaseOut
                        }
                };

            var tcs =
                new TaskCompletionSource<bool>();

            animation.Completed +=
                (_, _) =>
                    tcs.TrySetResult(
                        true);

            element.BeginAnimation(
                UIElement.OpacityProperty,
                animation);

            await tcs.Task;
        }

        private sealed class AppConfiguration
        {
            // 主界面配置

            public string MusicFolder { get; set; }
                = string.Empty;

            public string VoiceFolder { get; set; }
                = string.Empty;


            public bool PlayMusicOnLock { get; set; }
                = true;


            public bool StopMusicOnUnlock { get; set; }
                = true;


            public bool PlayWelcomeVoice { get; set; }
                = true;



            // 音频配置

            public double MusicVolume { get; set; }
    = 100;


            public double WelcomeVoiceVolume { get; set; }
                = 100;


            public double UnlockFadeDuration { get; set; }
                = 1500;



            // 系统配置

            public bool StartupTaskEnabled { get; set; }
                = false;


            public bool RunInBackground { get; set; }
                = true;
        }

        private sealed class BarParticle
        {
            public Polygon Shape { get; init; } =
                null!;

            public RotateTransform Rotation { get; init; } =
                null!;

            public double X { get; set; }

            public double Y { get; set; }

            public double VelocityX { get; set; }

            public double VelocityY { get; set; }

            public double Gravity { get; set; }

            public double RotationSpeed { get; set; }

            public double Lifetime { get; init; }

            public double Age { get; set; }
        }

        /*
         * ============================================================
         * Windows Common Item Dialog COM 定义
         * ============================================================
         */

        [ComImport]
        [Guid(
            "DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        private class FileOpenDialog
        {
        }

        [ComImport]
        [InterfaceType(
            ComInterfaceType.InterfaceIsIUnknown)]
        [Guid(
            "D57C7288-D4AD-4768-BE02-9D969532D960")]
        private interface IFileOpenDialog
        {
            [PreserveSig]
            int Show(
                IntPtr parent);

            void SetFileTypes(
                uint count,
                IntPtr filters);

            void SetFileTypeIndex(
                uint index);

            void GetFileTypeIndex(
                out uint index);

            void Advise(
                IntPtr events,
                out uint cookie);

            void Unadvise(
                uint cookie);

            void SetOptions(
                uint options);

            void GetOptions(
                out uint options);

            void SetDefaultFolder(
                IShellItem folder);

            void SetFolder(
                IShellItem folder);

            void GetFolder(
                out IShellItem folder);

            void GetCurrentSelection(
                out IShellItem item);

            void SetFileName(
                [MarshalAs(
                    UnmanagedType.LPWStr)]
                string name);

            void GetFileName(
                [MarshalAs(
                    UnmanagedType.LPWStr)]
                out string name);

            void SetTitle(
                [MarshalAs(
                    UnmanagedType.LPWStr)]
                string title);

            void SetOkButtonLabel(
                [MarshalAs(
                    UnmanagedType.LPWStr)]
                string text);

            void SetFileNameLabel(
                [MarshalAs(
                    UnmanagedType.LPWStr)]
                string text);

            void GetResult(
                out IShellItem item);

            void AddPlace(
                IShellItem item,
                int alignment);

            void Close(
                int hr);

            void SetClientGuid(
                ref Guid guid);

            void ClearClientData();

            void SetFilter(
                IntPtr filter);

            void GetResults(
                out IntPtr results);

            void GetSelectedItems(
                out IntPtr results);
        }

        [ComImport]
        [InterfaceType(
            ComInterfaceType.InterfaceIsIUnknown)]
        [Guid(
            "43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        private interface IShellItem
        {
            void BindToHandler(
                IntPtr pbc,
                ref Guid bhid,
                ref Guid riid,
                out IntPtr ppv);

            void GetParent(
                out IShellItem parent);

            void GetDisplayName(
                uint sigdnName,
                out IntPtr ppszName);

            void GetAttributes(
                uint sfgaoMask,
                out uint psfgaoAttribs);

            void Compare(
                IShellItem psi,
                uint hint,
                out int piOrder);
        }

        private static class FileOpenDialogOptions
        {
            public const uint PickFolders =
                0x00000020;

            public const uint ForceFileSystem =
                0x00000040;
        }

        private static class ShellItemDisplayName
        {
            public const uint FileSystemPath =
                0x80058000;
        }
    }
}