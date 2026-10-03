using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;
using ShittimEcho.Core.Music;
using ShittimEcho.Core.Voice;

namespace ShittimEcho
{
    public partial class SettingsWindow : Window
    {
        private static readonly string ConfigurationDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ShittimEcho");

        private static readonly string ConfigurationFilePath =
            Path.Combine(
                ConfigurationDirectory,
                "configuration.json");

        public SettingsWindow()
        {
            InitializeComponent();

            AboutNavigationButton.Click +=
                AboutNavigationButton_Click;

            AudioNavigationButton.Click +=
                AudioNavigationButton_Click;

            LibraryNavigationButton.Click +=
                LibraryNavigationButton_Click;

            SystemNavigationButton.Click +=
                SystemNavigationButton_Click;

            MusicVolumeSlider.ValueChanged +=
                MusicVolumeSlider_ValueChanged;

            WelcomeVoiceVolumeSlider.ValueChanged +=
                WelcomeVoiceVolumeSlider_ValueChanged;

            UnlockFadeDurationSlider.ValueChanged +=
                UnlockFadeDurationSlider_ValueChanged;

            LoadAudioSettings();
            RefreshSystemStatus();
            LoadSystemSettings();

            StartupTaskCheckBox.Checked +=
    StartupTaskCheckBox_Checked;

            StartupTaskCheckBox.Unchecked +=
                StartupTaskCheckBox_Unchecked;

            RunInBackgroundCheckBox.Checked +=
                RunInBackgroundCheckBox_Checked;

            RunInBackgroundCheckBox.Unchecked +=
                RunInBackgroundCheckBox_Unchecked;
        }

        private void SettingsMoveArea_MouseLeftButtonDown(
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
                // 防止特殊鼠标状态影响设置窗口的其他功能。
            }
        }

        private void CloseSettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SaveAudioSettings();
            Close();
        }

        private void AboutNavigationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AboutContentPanel.Visibility =
                Visibility.Visible;

            AudioContentPanel.Visibility =
                Visibility.Collapsed;

            LibraryContentPanel.Visibility =
    Visibility.Collapsed;

            SystemContentPanel.Visibility =
    Visibility.Collapsed;
        }

        private void AudioNavigationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AboutContentPanel.Visibility =
                Visibility.Collapsed;

            AudioContentPanel.Visibility =
                Visibility.Visible;

            LibraryContentPanel.Visibility =
    Visibility.Collapsed;

            SystemContentPanel.Visibility =
    Visibility.Collapsed;
        }

        private void LibraryNavigationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AboutContentPanel.Visibility =
                Visibility.Collapsed;

            AudioContentPanel.Visibility =
                Visibility.Collapsed;

            LibraryContentPanel.Visibility =
    Visibility.Visible;

            SystemContentPanel.Visibility =
    Visibility.Collapsed;

            RefreshMusicLibraryDirectory();
            RefreshVoiceLibraryDirectory();

            _ = RefreshMusicLibraryInformationAsync();
            _ = RefreshVoiceLibraryInformationAsync();
        }

        private void RefreshSystemStatus()
        {
            try
            {
                SystemStatusText.Text =
                    "咸鱼喵喵 · 喵露露终端正在运行";

                SystemStatusDetailText.Text =
                    "喵喵音频监控模块已准备就绪";

                Process currentProcess =
                    Process.GetCurrentProcess();

                SystemStatusDetailText.Text =
                    $"喵喵音频监控模块已准备就绪 · PID {currentProcess.Id}";
            }
            catch
            {
                // 系统状态刷新失败时保持当前界面内容。
            }
        }

        private void LoadSystemSettings()
        {
            try
            {
                AppConfiguration configuration =
                    LoadConfiguration();

                StartupTaskCheckBox.IsChecked =
                    configuration.StartupTaskEnabled;

                RunInBackgroundCheckBox.IsChecked =
                    configuration.RunInBackground;
            }
            catch
            {
                StartupTaskCheckBox.IsChecked =
                    false;

                RunInBackgroundCheckBox.IsChecked =
                    false;
            }
        }

        private void StartupTaskCheckBox_Checked(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                CreateStartupTask();

                SaveSystemSettings();
            }
            catch
            {
                StartupTaskCheckBox.IsChecked =
                    false;
            }
        }

        private void StartupTaskCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                RemoveStartupTask();

                SaveSystemSettings();
            }
            catch
            {
                // 开机启动任务移除失败时保持当前界面状态。
            }
        }

        private void RunInBackgroundCheckBox_Checked(
    object sender,
    RoutedEventArgs e)
        {
            SaveSystemSettings();
        }

        private void RunInBackgroundCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            SaveSystemSettings();
        }

        private void SaveSystemSettings()
        {
            try
            {
                AppConfiguration configuration =
                    LoadConfiguration();

                configuration.StartupTaskEnabled =
                    StartupTaskCheckBox.IsChecked == true;

                configuration.RunInBackground =
                    RunInBackgroundCheckBox.IsChecked == true;

                Directory.CreateDirectory(
                    ConfigurationDirectory);

                string json =
                    JsonSerializer.Serialize(
                        configuration,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(
                    ConfigurationFilePath,
                    json);
            }
            catch
            {
                // 系统设置保存失败时保持当前界面状态。
            }
        }

        private void CreateStartupTask()
        {
            string executablePath =
                Process.GetCurrentProcess()
                    .MainModule?
                    .FileName
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(executablePath))
            {
                throw new InvalidOperationException(
                    "无法获取程序路径。");
            }

            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    FileName =
                        "schtasks.exe",

                    UseShellExecute =
                        true,

                    Verb =
                        "runas",

                    CreateNoWindow =
                        false
                };

            startInfo.ArgumentList.Add(
                "/Create");

            startInfo.ArgumentList.Add(
                "/TN");

            startInfo.ArgumentList.Add(
                "ShittimEcho");

            startInfo.ArgumentList.Add(
                "/SC");

            startInfo.ArgumentList.Add(
                "ONSTART");

            startInfo.ArgumentList.Add(
                "/TR");

            startInfo.ArgumentList.Add(
                $"\"{executablePath}\"");

            startInfo.ArgumentList.Add(
                "/RU");

            startInfo.ArgumentList.Add(
                "SYSTEM");

            startInfo.ArgumentList.Add(
                "/RL");

            startInfo.ArgumentList.Add(
                "HIGHEST");

            startInfo.ArgumentList.Add(
                "/F");

            using Process process =
                Process.Start(startInfo)
                ?? throw new InvalidOperationException(
                    "无法启动任务计划程序。");

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    "Windows 任务计划程序创建任务失败。");
            }
        }

        private void RemoveStartupTask()
        {
            ProcessStartInfo startInfo =
                new ProcessStartInfo
                {
                    FileName =
                        "schtasks.exe",

                    UseShellExecute =
                        false,

                    CreateNoWindow =
                        true,

                    RedirectStandardOutput =
                        true,

                    RedirectStandardError =
                        true
                };

            startInfo.ArgumentList.Add(
                "/Delete");

            startInfo.ArgumentList.Add(
                "/TN");

            startInfo.ArgumentList.Add(
                "ShittimEcho");

            startInfo.ArgumentList.Add(
                "/F");

            using Process process =
                Process.Start(startInfo)
                ?? throw new InvalidOperationException(
                    "无法启动 Windows 任务计划程序命令。");

            process.WaitForExit();
        }

        private void SystemNavigationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AboutContentPanel.Visibility =
                Visibility.Collapsed;

            AudioContentPanel.Visibility =
                Visibility.Collapsed;

            LibraryContentPanel.Visibility =
    Visibility.Collapsed;

            SystemContentPanel.Visibility =
                Visibility.Visible;
        }

        private void MusicVolumeSlider_ValueChanged(
    object sender,
    RoutedPropertyChangedEventArgs<double> e)
        {
            MusicVolumeValueText.Text =
                $"{e.NewValue:0}%";

            SaveAudioSettings();
        }

        private void WelcomeVoiceVolumeSlider_ValueChanged(
    object sender,
    RoutedPropertyChangedEventArgs<double> e)
        {
            WelcomeVoiceVolumeValueText.Text =
                $"{e.NewValue:0}%";

            SaveAudioSettings();
        }

        private void UnlockFadeDurationSlider_ValueChanged(
    object sender,
    RoutedPropertyChangedEventArgs<double> e)
        {
            UnlockFadeDurationValueText.Text =
                $"{e.NewValue:0} ms";

            SaveAudioSettings();
        }

        private void LoadAudioSettings()
        {
            try
            {
                if (!File.Exists(
                        ConfigurationFilePath))
                {
                    return;
                }

                string json =
                    File.ReadAllText(
                        ConfigurationFilePath);

                AppConfiguration? configuration =
                    JsonSerializer.Deserialize<AppConfiguration>(
                        json);

                if (configuration == null)
                {
                    return;
                }

                MusicVolumeSlider.Value =
                    Clamp(
                        configuration.MusicVolume,
                        0,
                        100);

                WelcomeVoiceVolumeSlider.Value =
                    Clamp(
                        configuration.WelcomeVoiceVolume,
                        0,
                        100);

                UnlockFadeDurationSlider.Value =
                    Clamp(
                        configuration.UnlockFadeDuration,
                        500,
                        5000);
            }
            catch
            {
                // 设置读取失败时保持界面默认值。
            }
        }

        private async Task RefreshMusicLibraryInformationAsync()
        {
            try
            {
                AppConfiguration configuration =
                    LoadConfiguration();

                if (string.IsNullOrWhiteSpace(
                        configuration.MusicFolder))
                {
                    MusicLibraryLiveInfoText.Text =
                        "未配置音乐目录";

                    MusicLibraryLiveCountText.Text =
                        "—";

                    return;
                }


                MusicLibraryLiveInfoText.Text =
                    "正在扫描音乐库...";


                MusicLibraryLiveCountText.Text =
                    "扫描中";


                await Task.Run(() =>
                {
                    MusicLibrary musicLibrary =
                        new MusicLibrary(
                            configuration.MusicFolder);

                    musicLibrary.Scan();


                    Dispatcher.Invoke(() =>
                    {
                        MusicLibraryLiveInfoText.Text =
                            "音乐库扫描完成";

                        MusicLibraryLiveCountText.Text =
                            $"{musicLibrary.Tracks.Count} 首";
                    });

                });

            }
            catch
            {
                MusicLibraryLiveInfoText.Text =
                    "音乐库扫描失败";

                MusicLibraryLiveCountText.Text =
                    "—";
            }
        }

        private void RefreshMusicLibraryDirectory()
        {
            try
            {
                AppConfiguration configuration =
                    LoadConfiguration();

                if (string.IsNullOrWhiteSpace(
                        configuration.MusicFolder))
                {
                    return;
                }

                MusicLibraryLiveDirectoryText.Text =
                    configuration.MusicFolder;
            }
            catch
            {
                // 音乐目录读取失败时保持当前界面内容。
            }
        }

        private void RefreshVoiceLibraryDirectory()
        {
            try
            {
                AppConfiguration configuration =
                    LoadConfiguration();

                if (string.IsNullOrWhiteSpace(
                        configuration.VoiceFolder))
                {
                    return;
                }

                VoiceLibraryLiveDirectoryText.Text =
                    configuration.VoiceFolder;
            }
            catch
            {
                // 语音目录读取失败时保持当前界面内容。
            }
        }

        private async Task RefreshVoiceLibraryInformationAsync()
        {
            try
            {
                AppConfiguration configuration =
                    LoadConfiguration();


                if (string.IsNullOrWhiteSpace(
                        configuration.VoiceFolder))
                {
                    VoiceLibraryLiveInfoText.Text =
                        "未配置语音目录";

                    VoiceLibraryLiveCountText.Text =
                        "—";

                    return;
                }


                VoiceLibraryLiveInfoText.Text =
                    "正在扫描语音库...";


                VoiceLibraryLiveCountText.Text =
                    "扫描中";


                await Task.Run(() =>
                {
                    VoiceLibrary voiceLibrary =
                        new VoiceLibrary(
                            configuration.VoiceFolder);


                    voiceLibrary.Scan();


                    Dispatcher.Invoke(() =>
                    {
                        VoiceLibraryLiveInfoText.Text =
                            "语音库扫描完成";


                        VoiceLibraryLiveCountText.Text =
                            $"{voiceLibrary.Tracks.Count} 条";
                    });

                });

            }
            catch
            {
                VoiceLibraryLiveInfoText.Text =
                    "语音库扫描失败";


                VoiceLibraryLiveCountText.Text =
                    "—";
            }
        }

        private void SaveAudioSettings()
        {
            try
            {
                Directory.CreateDirectory(
                    ConfigurationDirectory);

                AppConfiguration configuration =
                    LoadConfiguration();

                configuration.MusicVolume =
                    MusicVolumeSlider.Value;

                configuration.WelcomeVoiceVolume =
                    WelcomeVoiceVolumeSlider.Value;

                configuration.UnlockFadeDuration =
                    UnlockFadeDurationSlider.Value;

                JsonSerializerOptions options =
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                string json =
                    JsonSerializer.Serialize(
                        configuration,
                        options);

                File.WriteAllText(
                    ConfigurationFilePath,
                    json);
            }
            catch
            {
                // 设置保存失败时不影响设置窗口关闭。
            }
        }

        private static AppConfiguration LoadConfiguration()
        {
            try
            {
                if (File.Exists(
                        ConfigurationFilePath))
                {
                    string json =
                        File.ReadAllText(
                            ConfigurationFilePath);

                    AppConfiguration? configuration =
                        JsonSerializer.Deserialize<AppConfiguration>(
                            json);

                    if (configuration != null)
                    {
                        return configuration;
                    }
                }
            }
            catch
            {
                // 配置读取失败时使用默认配置。
            }

            return new AppConfiguration();
        }

        private static double Clamp(
            double value,
            double minimum,
            double maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }

        private sealed class AppConfiguration
        {
            public string MusicFolder { get; set; } =
                string.Empty;

            public string VoiceFolder { get; set; } =
                string.Empty;

            public bool PlayMusicOnLock { get; set; } =
                true;

            public bool StopMusicOnUnlock { get; set; } =
                true;

            public bool PlayWelcomeVoice { get; set; } =
                true;

            public double MusicVolume { get; set; } =
                100;

            public double WelcomeVoiceVolume { get; set; } =
                100;

            public double UnlockFadeDuration { get; set; } =
                1500;

            public bool StartupTaskEnabled { get; set; } =
    false;

            public bool RunInBackground { get; set; } =
                false;
        }
    }
}