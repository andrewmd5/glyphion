using System.Runtime.InteropServices;

namespace Glyphion.Core.IO;

/// <summary>
/// Provides functionality to detect key presses on different operating systems.
/// </summary>
public partial class Keyboard
{
    /// <summary>
    /// Checks if a specific key is currently pressed down.
    /// </summary>
    /// <param name="keyCode">The key code of the key to check.</param>
    /// <returns>Returns true if the key is currently pressed down; otherwise, false.</returns>
    public static bool IsKeyDown(int keyCode)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return IsKeyPressedWindows(keyCode);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return IsKeyPressedMacOS(keyCode);
        }
        else
        {
            throw new PlatformNotSupportedException("Platform not supported for key press detection.");
        }
    }
}