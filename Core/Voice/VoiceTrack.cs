using System;

namespace ShittimEcho.Core.Voice
{
    public sealed class VoiceTrack
    {
        public int SortOrder { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string DisplayName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}