using System;

namespace ShittimEcho.Core.Audio
{
    public sealed class AudioEndpoint
    {
        public AudioEndpoint(AudioDeviceInfo device)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public AudioDeviceInfo Device { get; }

        public string Id => Device.Id;

        public string Name => Device.Name;

        public string State => Device.State;

        public bool IsDefault => Device.IsDefault;
    }
}