using System.Runtime.InteropServices;

namespace Support
{
    /// <inheritdoc/>
    /// <summary>
    /// HighResolutionDateTime
    /// 
    /// https://manski.net/2014/07/high-resolution-clock-in-csharp/
    /// 
    /// </summary>
    public static class Timing
    {
        public static bool IsAvailable { get; private set; }

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        public static long FileTime
        {
            get
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException(
                        "High resolution clock isn't available.");
                }

                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);

                return filetime;
            }
        }

        public static DateTime Now => DateTime.FromFileTime(FileTime);
        public static DateTime UtcNow => DateTime.FromFileTimeUtc(FileTime);

        static Timing()
        {
            try
            {
                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);
                IsAvailable = true;
            }
            catch (EntryPointNotFoundException)
            {
                // Not running Windows 8 or higher.
                IsAvailable = false;
            }
        }
    }
}
