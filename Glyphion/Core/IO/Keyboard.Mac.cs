using System.Runtime.InteropServices;

namespace Glyphion.Core.IO;

/// <summary>
/// The Keyboard class provides an interface for interacting with keyboard input.
/// </summary>
public partial class Keyboard
{
    /// <summary>
    /// Determines if a specific key is currently pressed on macOS.
    /// </summary>
    /// <param name="keyCode">The key code of the key to check.</param>
    /// <returns>True if the key is pressed; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided key code is not supported.</exception>
    private static bool IsKeyPressedMacOS(int keyCode)
    {
       
        ushort cgKeyCode = MapKeyCodeToCGKeyCode(keyCode);


        if (cgKeyCode == 0xFFFF)
            throw new ArgumentException($"Key code '{keyCode}' is not supported.");
      
    
        return  CGEventSourceKeyState(0, cgKeyCode);
    }
    
    // macOS implementation
    /// <summary>
    /// Determines the state of a specific key on the keyboard.
    /// </summary>
    /// <param name="stateId">The state identifier (typically 0 for the current state).</param>
    /// <param name="keyCode">The key code representing the key whose state is to be checked.</param>
    /// <return>True if the specified key is pressed; otherwise, false.</return>
    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CGEventSourceKeyState(int stateId, ushort keyCode);

    /// <summary>
    /// Maps a given key code to a macOS-specific CGKeyCode.
    /// </summary>
    /// <param name="keyCode">The key code to be mapped.</param>
    /// <returns>Returns the corresponding CGKeyCode as an unsigned short.</returns>
    private static ushort MapKeyCodeToCGKeyCode(int keyCode)
        {
            // Mapping Windows Virtual-Key codes to macOS CGKeyCodes
            // Reference for Windows VK codes: https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            // Reference for macOS CGKeyCodes: https://stackoverflow.com/questions/3202629/where-can-i-find-a-list-of-mac-virtual-key-codes

            switch (keyCode)
            {
                // Arrow keys
                case 0x25: // VK_LEFT
                    return 0x7B; // kVK_LeftArrow
                case 0x26: // VK_UP
                    return 0x7E; // kVK_UpArrow
                case 0x27: // VK_RIGHT
                    return 0x7C; // kVK_RightArrow
                case 0x28: // VK_DOWN
                    return 0x7D; // kVK_DownArrow
                
                

                // Common keys
                case 0x41: // 'A' key
                    return 0x00;
                case 0x53: // 'S' key
                    return 0x01;
                case 0x44: // 'D' key
                    return 0x02;
                case 0x46: // 'F' key
                    return 0x03;
                // ... add other key mappings as needed

                // Function keys
                case 0x70: // VK_F1
                    return 0x7A; // kVK_F1
                case 0x71: // VK_F2
                    return 0x78; // kVK_F2
                // ... add other function key mappings as needed

                // Special keys
                case 0x1B: // VK_ESCAPE
                    return 0x35; // kVK_Escape
                case 0x0D: // VK_RETURN (Enter)
                    return 0x24; // kVK_Return
                case 0x09: // VK_TAB
                    return 0x30; // kVK_Tab
                case 0x08: // VK_BACK (Backspace)
                    return 0x33; // kVK_Delete (Backspace)
                case 0x20: // VK_SPACE
                    return 0x31; // kVK_Space
                
                // Numeric keys (Top row)
                case 0x30: // '0'
                    return 0x1D;
                case 0x31: // '1'
                    return 0x12;
                case 0x32: // '2'
                    return 0x13;
                case 0x33: // '3'
                    return 0x14;
                case 0x34: // '4'
                    return 0x15;
                case 0x35: // '5'
                    return 0x17;
                case 0x36: // '6'
                    return 0x16;
                case 0x37: // '7'
                    return 0x1A;
                case 0x38: // '8'
                    return 0x1C;
                case 0x39: // '9'
                    return 0x19;

                // Alphabet keys
                case 0x42: return 0x0B; // 'B'
                case 0x43: return 0x08; // 'C'
                case 0x45: return 0x0E; // 'E'
                case 0x47: return 0x05; // 'G'
                case 0x48: return 0x04; // 'H'
                case 0x49: return 0x22; // 'I'
                case 0x4A: return 0x26; // 'J'
                case 0x4B: return 0x28; // 'K'
                case 0x4C: return 0x25; // 'L'
                case 0x4D: return 0x2E; // 'M'
                case 0x4E: return 0x2D; // 'N'
                case 0x4F: return 0x1F; // 'O'
                case 0x50: return 0x23; // 'P'
                case 0x51: return 0x0C; // 'Q'
                case 0x52: return 0x0F; // 'R'
                case 0x54: return 0x11; // 'T'
                case 0x55: return 0x20; // 'U'
                case 0x56: return 0x09; // 'V'
                case 0x57: return 0x0D; // 'W'
                case 0x58: return 0x07; // 'X'
                case 0x59: return 0x10; // 'Y'
                case 0x5A: return 0x06; // 'Z'

                // Add more key mappings as needed

                default:
                    return 0xFFFF; // Invalid or unsupported key code
            }
        }
}