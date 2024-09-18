using System.Runtime.InteropServices;

namespace Glyphion.Core.IO;

/// <summary>
/// Provides methods for interacting with the keyboard.
/// </summary>
public partial class Keyboard
{
    // Windows implementation
    /// <summary>
    /// Checks the state of a specific key at the time the function is called.
    /// </summary>
    /// <param name="vKey">The virtual-key code of the key to be checked.</param>
    /// <returns>
    /// If the high-order bit is 1, the key is down. If the low-order bit is 0, the key is up.
    /// </returns>
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    /// <summary>
    /// Checks if a specified key is currently pressed on a Windows platform.
    /// </summary>
    /// <param name="vKey">The virtual-key code of the key to check.</param>
    /// <returns>True if the specified key is pressed; otherwise, false.</returns>
    private static bool IsKeyPressedWindows(int vKey)
    {
        return (GetAsyncKeyState(vKey) & 0x8000) != 0;
    }
}