#if MAUI
using Microsoft.Maui.ApplicationModel;

namespace Psicho_Support.Services.Platform
{
    public class PlatformSpecificService
    {
        public static double GetScreenWidth()
        {
            return DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        }

        public static double GetScreenHeight()
        {
            return DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        }
    }
}
#endif