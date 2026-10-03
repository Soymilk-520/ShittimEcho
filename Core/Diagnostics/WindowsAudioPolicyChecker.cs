using System;
using Microsoft.Win32;

namespace ShittimEcho.Core.Diagnostics
{
    public sealed class WindowsAudioPolicyChecker
    {
        private const string PolicyPath =
            @"SOFTWARE\Policies\Microsoft\Windows\Audio";

        private const string PolicyName =
            "ConfigureAudioOnLockScreen";

        public WindowsAudioPolicyResult Check()
        {
            using RegistryKey? key =
                Registry.LocalMachine.OpenSubKey(
                    PolicyPath,
                    writable: false);

            if (key == null)
            {
                return new WindowsAudioPolicyResult
                {
                    IsConfigured = false,
                    Value = null,
                    Status = WindowsAudioPolicyStatus.NotConfigured,
                    Message =
                        "ConfigureAudioOnLockScreen policy is not configured."
                };
            }

            object? rawValue =
                key.GetValue(
                    PolicyName,
                    null,
                    RegistryValueOptions.DoNotExpandEnvironmentNames);

            if (rawValue == null)
            {
                return new WindowsAudioPolicyResult
                {
                    IsConfigured = false,
                    Value = null,
                    Status = WindowsAudioPolicyStatus.NotConfigured,
                    Message =
                        "ConfigureAudioOnLockScreen policy value is not configured."
                };
            }

            int value;

            try
            {
                value = Convert.ToInt32(rawValue);
            }
            catch
            {
                return new WindowsAudioPolicyResult
                {
                    IsConfigured = true,
                    Value = null,
                    Status = WindowsAudioPolicyStatus.Invalid,
                    Message =
                        $"ConfigureAudioOnLockScreen contains an invalid value: {rawValue}"
                };
            }

            return value switch
            {
                0 => new WindowsAudioPolicyResult
                {
                    IsConfigured = true,
                    Value = 0,
                    Status = WindowsAudioPolicyStatus.Disabled,
                    Message =
                        "Lock-screen audio playback is disabled by policy."
                },

                1 => new WindowsAudioPolicyResult
                {
                    IsConfigured = true,
                    Value = 1,
                    Status = WindowsAudioPolicyStatus.PlaybackAllowed,
                    Message =
                        "Lock-screen audio playback is allowed by policy."
                },

                2 => new WindowsAudioPolicyResult
                {
                    IsConfigured = true,
                    Value = 2,
                    Status = WindowsAudioPolicyStatus.PlaybackAndRecordingAllowed,
                    Message =
                        "Lock-screen audio playback and recording are allowed by policy."
                },

                _ => new WindowsAudioPolicyResult
                {
                    IsConfigured = true,
                    Value = value,
                    Status = WindowsAudioPolicyStatus.Invalid,
                    Message =
                        $"ConfigureAudioOnLockScreen contains an unsupported value: {value}"
                }
            };
        }
    }

    public sealed class WindowsAudioPolicyResult
    {
        public bool IsConfigured { get; init; }

        public int? Value { get; init; }

        public WindowsAudioPolicyStatus Status { get; init; }

        public string Message { get; init; } = string.Empty;

        public bool IsPlaybackAllowed =>
            Status == WindowsAudioPolicyStatus.PlaybackAllowed ||
            Status == WindowsAudioPolicyStatus.PlaybackAndRecordingAllowed;
    }

    public enum WindowsAudioPolicyStatus
    {
        NotConfigured,

        Disabled,

        PlaybackAllowed,

        PlaybackAndRecordingAllowed,

        Invalid
    }
}