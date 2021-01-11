using System;
using System.Runtime.InteropServices;

namespace e621_ReBot_v2.Modules
{
    public static class Module_VolumeControl
    {
        [DllImport("winmm.dll")]
        public static extern uint waveOutSetVolume(IntPtr hwo, uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern uint waveOutGetVolume(IntPtr hwo, ref uint pdwVolume);

        public static int GetApplicationVolume()
        {
            uint vol = 0;
            waveOutGetVolume(IntPtr.Zero, ref vol);
            return (int)((vol & 0xFFFF) / (ushort.MaxValue / 100));
        }

    }
}
