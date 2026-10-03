using System;
using ShittimEcho.Core.Diagnostics;

namespace ShittimEcho.Windows
{
    public sealed class WindowsSessionStateMachine : IDisposable
    {
        private readonly WindowsSessionMonitor _monitor;
        private readonly StartupLogger _logger;

        private bool _started;
        private bool _disposed;

        public WindowsSessionStateMachine(
            WindowsSessionMonitor monitor,
            StartupLogger logger)
        {
            _monitor =
                monitor ?? throw new ArgumentNullException(
                    nameof(monitor));

            _logger =
                logger ?? throw new ArgumentNullException(
                    nameof(logger));

            _monitor.StateChanged +=
                OnSessionStateChanged;
        }

        public WindowsSessionState CurrentState =>
            _monitor.CurrentState;

        public event EventHandler<WindowsSessionState>? StateChanged;

        public void Start()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(WindowsSessionStateMachine));
            }

            if (_started)
                return;

            _monitor.Start();

            _started = true;

            _logger.Write(
                $"Windows session state machine started. " +
                $"Current state: {CurrentState}");
        }

        public void Stop()
        {
            if (!_started)
                return;

            _monitor.Stop();

            _started = false;

            _logger.Write(
                "Windows session state machine stopped.");
        }

        private void OnSessionStateChanged(
            object? sender,
            WindowsSessionState state)
        {
            if (_disposed)
                return;

            _logger.Write(
                $"Windows session state changed: {state}");

            StateChanged?.Invoke(
                this,
                state);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _monitor.StateChanged -=
                OnSessionStateChanged;

            Stop();
        }
    }
}