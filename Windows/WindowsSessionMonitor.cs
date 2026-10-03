using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using Microsoft.Win32;
using ShittimEcho.Core.Audio;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Windows
{
    public enum WindowsSessionState
    {
        Unknown,
        Locked,
        Unlocked
    }

    public sealed class WindowsSessionMonitor : IDisposable
    {
        private readonly StartupLogger _logger;
        private readonly AudioDeviceManager _audioDeviceManager;

        private bool _started;

        public WindowsSessionState CurrentState { get; private set; }
            = WindowsSessionState.Unknown;

        public event EventHandler<WindowsSessionState>? StateChanged;

        public WindowsSessionMonitor(
            StartupLogger logger)
        {
            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));

            _audioDeviceManager =
                new AudioDeviceManager();
        }

        public void Start()
        {
            if (_started)
                return;

            _logger.Write(
                "Windows Session Monitor starting.");

            SystemEvents.SessionSwitch +=
                OnSessionSwitch;

            _started = true;

            _logger.Write(
                "Windows Session Monitor started.");
        }

        private void OnSessionSwitch(
            object? sender,
            SessionSwitchEventArgs e)
        {
            _logger.Write(
                $"Windows Session Monitor: " +
                $"Windows session event received: {e.Reason}");

            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    HandleLockEvent(e);
                    break;

                case SessionSwitchReason.SessionUnlock:
                    HandleUnlockEvent(e);
                    break;

                case SessionSwitchReason.SessionLogon:
                    LogExtendedSessionEvent(
                        e.Reason,
                        "User session logon detected.");
                    break;

                case SessionSwitchReason.SessionLogoff:
                    LogExtendedSessionEvent(
                        e.Reason,
                        "User session logoff detected.");
                    break;

                case SessionSwitchReason.ConsoleConnect:
                    LogExtendedSessionEvent(
                        e.Reason,
                        "Console session connection detected.");
                    break;

                case SessionSwitchReason.ConsoleDisconnect:
                    LogExtendedSessionEvent(
                        e.Reason,
                        "Console session disconnection detected.");
                    break;

                case SessionSwitchReason.RemoteConnect:
                    LogExtendedSessionEvent(
                        e.Reason,
                        "Remote session connection detected.");
                    break;

                case SessionSwitchReason.RemoteDisconnect:
                    LogExtendedSessionEvent(
                        e.Reason,
                        "Remote session disconnection detected.");
                    break;

                case SessionSwitchReason.SessionRemoteControl:
                    LogExtendedSessionEvent(
                        e.Reason,
                        "Remote control session event detected.");
                    break;

                default:
                    LogExtendedSessionEvent(
                        e.Reason,
                        "Unhandled Windows session event detected.");
                    break;
            }
        }

        private void HandleLockEvent(
            SessionSwitchEventArgs e)
        {
            WindowsSessionState newState =
                WindowsSessionState.Locked;

            LogSessionEnvironment(
                newState,
                e.Reason);

            if (CurrentState == newState)
            {
                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"state already {CurrentState}, " +
                    $"ignoring duplicate event.");

                return;
            }

            CurrentState = newState;

            _logger.Write(
                $"Windows Session Monitor: " +
                $"state changed to {CurrentState}.");

            StateChanged?.Invoke(
                this,
                CurrentState);
        }

        private void HandleUnlockEvent(
            SessionSwitchEventArgs e)
        {
            WindowsSessionState newState =
                WindowsSessionState.Unlocked;

            LogSessionEnvironment(
                newState,
                e.Reason);

            if (CurrentState == newState)
            {
                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"state already {CurrentState}, " +
                    $"ignoring duplicate event.");

                return;
            }

            CurrentState = newState;

            _logger.Write(
                $"Windows Session Monitor: " +
                $"state changed to {CurrentState}.");

            StateChanged?.Invoke(
                this,
                CurrentState);
        }

        private void LogExtendedSessionEvent(
            SessionSwitchReason reason,
            string description)
        {
            _logger.Write(
                "----------------------------------------");

            _logger.Write(
                "Windows Session Monitor: " +
                "extended session event detected.");

            _logger.Write(
                $"Windows Session Monitor: Reason={reason}");

            _logger.Write(
                $"Windows Session Monitor: Description={description}");

            try
            {
                Process process =
                    Process.GetCurrentProcess();

                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"Process ID={process.Id}");

                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"Session ID={process.SessionId}");

                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"User={WindowsIdentity.GetCurrent().Name}");
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"failed to read process/session identity: {ex}");
            }

            _logger.Write(
                $"Windows Session Monitor: " +
                $"CurrentState={CurrentState}");

            _logger.Write(
                "----------------------------------------");
        }

        private void LogSessionEnvironment(
            WindowsSessionState state,
            SessionSwitchReason reason)
        {
            _logger.Write(
                "----------------------------------------");

            _logger.Write(
                "Windows Session Monitor: " +
                "session event received.");

            _logger.Write(
                $"Windows Session Monitor: Reason={reason}");

            _logger.Write(
                $"Windows Session Monitor: NewState={state}");

            try
            {
                Process process =
                    Process.GetCurrentProcess();

                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"Process ID={process.Id}");

                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"Session ID={process.SessionId}");

                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"User={WindowsIdentity.GetCurrent().Name}");
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"failed to read process/session identity: {ex}");
            }

            try
            {
                var devices =
                    _audioDeviceManager.GetOutputDevices();

                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"audio output devices detected={devices.Count}");

                foreach (var device in devices)
                {
                    _logger.Write(
                        $"Windows Session Monitor: " +
                        $"AudioDevice Name={device.Name}, " +
                        $"State={device.State}, " +
                        $"Default={device.IsDefault}");
                }

                var defaultDevice =
                    devices.FirstOrDefault(
                        device => device.IsDefault);

                if (defaultDevice != null)
                {
                    _logger.Write(
                        $"Windows Session Monitor: " +
                        $"Default audio endpoint={defaultDevice.Name}");

                    _logger.Write(
                        $"Windows Session Monitor: " +
                        $"Default audio endpoint state={defaultDevice.State}");

                    _logger.Write(
                        $"Windows Session Monitor: " +
                        $"Default audio endpoint ID={defaultDevice.Id}");
                }
                else
                {
                    _logger.Write(
                        "Windows Session Monitor: " +
                        "Default audio endpoint not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.Write(
                    $"Windows Session Monitor: " +
                    $"failed to inspect audio environment: {ex}");
            }

            _logger.Write(
                "----------------------------------------");
        }

        public void Stop()
        {
            if (!_started)
                return;

            _logger.Write(
                "Windows Session Monitor stopping.");

            SystemEvents.SessionSwitch -=
                OnSessionSwitch;

            _started = false;

            _logger.Write(
                "Windows Session Monitor stopped.");
        }

        public void Dispose()
        {
            Stop();
        }
    }
}