using System;
using Microsoft.Win32;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Windows
{
    public enum WindowsPowerState
    {
        Unknown,
        Active,
        Suspended
    }

    public sealed class WindowsPowerMonitor : IDisposable
    {
        private readonly StartupLogger _logger;

        private bool _started;

        public WindowsPowerState CurrentState { get; private set; }
            = WindowsPowerState.Unknown;

        public event EventHandler<WindowsPowerState>? PowerStateChanged;

        public WindowsPowerMonitor(
            StartupLogger logger)
        {
            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));
        }

        public void Start()
        {
            if (_started)
                return;

            _logger.Write(
                "Windows Power Monitor starting.");

            SystemEvents.PowerModeChanged +=
                OnPowerModeChanged;

            _started = true;

            _logger.Write(
                "Windows Power Monitor started.");
        }

        private void OnPowerModeChanged(
            object? sender,
            PowerModeChangedEventArgs e)
        {
            _logger.Write(
                $"Windows Power Monitor: " +
                $"power event received: {e.Mode}");

            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    HandleSuspend();
                    break;

                case PowerModes.Resume:
                    HandleResume();
                    break;

                default:
                    _logger.Write(
                        $"Windows Power Monitor: " +
                        $"unhandled power event: {e.Mode}");
                    break;
            }
        }

        private void HandleSuspend()
        {
            WindowsPowerState newState =
                WindowsPowerState.Suspended;

            _logger.Write(
                "----------------------------------------");

            _logger.Write(
                "Windows Power Monitor: " +
                "Windows suspend detected.");

            if (CurrentState == newState)
            {
                _logger.Write(
                    "Windows Power Monitor: " +
                    "state already Suspended, " +
                    "ignoring duplicate event.");

                _logger.Write(
                    "----------------------------------------");

                return;
            }

            CurrentState = newState;

            _logger.Write(
                "Windows Power Monitor: " +
                "state changed to Suspended.");

            PowerStateChanged?.Invoke(
                this,
                CurrentState);

            _logger.Write(
                "----------------------------------------");
        }

        private void HandleResume()
        {
            WindowsPowerState newState =
                WindowsPowerState.Active;

            _logger.Write(
                "----------------------------------------");

            _logger.Write(
                "Windows Power Monitor: " +
                "Windows resume detected.");

            if (CurrentState == newState)
            {
                _logger.Write(
                    "Windows Power Monitor: " +
                    "state already Active, " +
                    "ignoring duplicate event.");

                _logger.Write(
                    "----------------------------------------");

                return;
            }

            CurrentState = newState;

            _logger.Write(
                "Windows Power Monitor: " +
                "state changed to Active.");

            PowerStateChanged?.Invoke(
                this,
                CurrentState);

            _logger.Write(
                "----------------------------------------");
        }

        public void Stop()
        {
            if (!_started)
                return;

            _logger.Write(
                "Windows Power Monitor stopping.");

            SystemEvents.PowerModeChanged -=
                OnPowerModeChanged;

            _started = false;

            _logger.Write(
                "Windows Power Monitor stopped.");
        }

        public void Dispose()
        {
            Stop();
        }
    }
}