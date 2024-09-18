using System.Runtime.InteropServices;

namespace Glyphion.Core.Renderer;

public partial class TerminalRenderer
{
    /// <summary>
    /// Tries to enable Virtual Terminal Processing on the Windows console.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the operation succeeded.
    /// </returns>
    private static bool TryEnableVirtualTerminalProcessing()
    {
        var handle = GetStdHandle(StdOutputHandle);
        if (!GetConsoleMode(handle, out var mode))
        {
            return false;
        }
        mode |= EnableVirtualTerminalProcessing;
        return SetConsoleMode(handle, mode);
    }

    /// <summary>
    /// The handle identifier for the standard output device in a Windows console.
    /// This constant is used with native methods to interact with the console's output settings
    /// and modes, such as enabling virtual terminal processing.
    /// </summary>
    private const int StdOutputHandle = -11;

    /// <summary>
    /// A constant that represents the flag used to enable virtual terminal processing in the console mode.
    /// This allows for ANSI escape sequences to be interpreted and processed by the Windows console,
    /// enabling enhanced text formatting and color capabilities.
    /// </summary>
    private const uint EnableVirtualTerminalProcessing = 0x0004;

    /// <summary>
    /// Retrieves the current input mode of a console's input buffer or the current output mode of a console screen buffer.
    /// </summary>
    /// <param name="hConsoleHandle">A handle to the console input buffer or a console screen buffer.</param>
    /// <param name="lpMode">A pointer to a variable that receives the current mode of the specified buffer.</param>
    /// <returns>true if the function succeeds; otherwise, false.</returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    /// <summary>
    /// Sets the mode of the specified console input buffer or output screen buffer.
    /// </summary>
    /// <param name="hConsoleHandle">A handle to the console input buffer or a console screen buffer.</param>
    /// <param name="dwMode">The input or output mode to be set.</param>
    /// <returns>True if the function succeeds, otherwise false.</returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    /// <summary>
    /// Retrieves a handle to the specified standard device (standard input, standard output, or standard error).
    /// </summary>
    /// <param name="nStdHandle">The standard device. This parameter can be one of the following values:
    /// -10 for standard input,
    /// -11 for standard output,
    /// or -12 for standard error.</param>
    /// <returns>If the function succeeds, the return value is a handle to the specified device. If the function fails, the return value is INVALID_HANDLE_VALUE.</returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(int nStdHandle);
}