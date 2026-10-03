using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ShittimEcho.Core.Audio
{
    public sealed class AudioDeviceInfo
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string State { get; init; } = string.Empty;

        public bool IsDefault { get; init; }
    }

    public sealed class AudioDeviceManager
    {
        private const int EDataFlowRender = 0;
        private const int ERoleMultimedia = 1;

        public IReadOnlyList<AudioDeviceInfo> GetOutputDevices()
        {
            var devices = new List<AudioDeviceInfo>();

            IMMDeviceEnumerator enumerator =
                (IMMDeviceEnumerator)new MMDeviceEnumerator();

            enumerator.EnumAudioEndpoints(
                EDataFlowRender,
                0x00000001,
                out IMMDeviceCollection collection);

            collection.GetCount(out uint count);

            string? defaultDeviceId = GetDefaultDeviceId(enumerator);

            for (uint i = 0; i < count; i++)
            {
                collection.Item(i, out IMMDevice device);

                device.GetId(out string id);

                device.OpenPropertyStore(
                    0,
                    out IPropertyStore propertyStore);

                string name = GetDeviceName(propertyStore);

                device.GetState(out uint state);

                devices.Add(new AudioDeviceInfo
                {
                    Id = id,
                    Name = name,
                    State = GetStateName(state),
                    IsDefault = string.Equals(
                        id,
                        defaultDeviceId,
                        StringComparison.OrdinalIgnoreCase)
                });

                Marshal.ReleaseComObject(propertyStore);
                Marshal.ReleaseComObject(device);
            }

            Marshal.ReleaseComObject(collection);
            Marshal.ReleaseComObject(enumerator);

            return devices;
        }

        private static string? GetDefaultDeviceId(
            IMMDeviceEnumerator enumerator)
        {
            enumerator.GetDefaultAudioEndpoint(
                EDataFlowRender,
                ERoleMultimedia,
                out IMMDevice device);

            device.GetId(out string id);

            Marshal.ReleaseComObject(device);

            return id;
        }

        private static string GetStateName(uint state)
        {
            return state switch
            {
                1 => "Active",
                2 => "Disabled",
                4 => "Not Present",
                8 => "Unplugged",
                _ => $"Unknown ({state})"
            };
        }

        private static string GetDeviceName(
            IPropertyStore propertyStore)
        {
            var propertyKey = new PROPERTYKEY
            {
                fmtid = new Guid(
                    "a45c254e-df1c-4efd-8020-67d146a850e0"),
                pid = 14
            };

            propertyStore.GetValue(
                ref propertyKey,
                out PROPVARIANT value);

            try
            {
                return value.GetString();
            }
            finally
            {
                PropVariantClear(ref value);
            }
        }

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator
        {
        }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(
                int dataFlow,
                uint stateMask,
                out IMMDeviceCollection devices);

            int GetDefaultAudioEndpoint(
                int dataFlow,
                int role,
                out IMMDevice device);

            int GetDevice(
                [MarshalAs(UnmanagedType.LPWStr)] string id,
                out IMMDevice device);

            int RegisterEndpointNotificationCallback(
                IntPtr client);

            int UnregisterEndpointNotificationCallback(
                IntPtr client);
        }

        [ComImport]
        [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceCollection
        {
            int GetCount(out uint count);

            int Item(
                uint index,
                out IMMDevice device);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(
                ref Guid iid,
                uint clsCtx,
                IntPtr activationParams,
                out IntPtr interfacePointer);

            int OpenPropertyStore(
                uint access,
                out IPropertyStore properties);

            int GetId(
                [MarshalAs(UnmanagedType.LPWStr)] out string id);

            int GetState(out uint state);
        }

        [ComImport]
        [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            int GetCount(out uint count);

            int GetAt(
                uint index,
                out PROPERTYKEY key);

            int GetValue(
                ref PROPERTYKEY key,
                out PROPVARIANT value);

            int SetValue(
                ref PROPERTYKEY key,
                ref PROPVARIANT value);

            int Commit();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROPERTYKEY
        {
            public Guid fmtid;
            public uint pid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROPVARIANT
        {
            public ushort vt;
            public ushort wReserved1;
            public ushort wReserved2;
            public ushort wReserved3;
            public IntPtr ptr;

            public string GetString()
            {
                return Marshal.PtrToStringUni(ptr) ?? string.Empty;
            }
        }

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(
            ref PROPVARIANT pvar);
    }
}