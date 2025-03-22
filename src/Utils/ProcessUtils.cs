using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PlayerList.Utils;

public enum FocusedWindow
{
  None = 0,
  Game = 1,
  Overlay = 2,
}

/// <summary>
/// EventArgs class for focus changes.
/// </summary>
/// <param name="isFocused"></param>
/// <param name="focusedWindow"></param>
internal class FocusChangedEventArgs(bool isFocused, FocusedWindow focusedWindow) : EventArgs
{
  public bool IsFocused { get; } = isFocused;
  public FocusedWindow FocusedWindow { get; } = focusedWindow;
}

/// <summary>
/// EventArgs class for visibility changes.
/// </summary>
/// <param name="isVisible"></param>
internal class VisibilityChangedEventArgs(bool isVisible) : EventArgs
{
  public bool IsVisible { get; } = isVisible;
}

internal static class ProcessUtils
{
  /// <summary>
  /// Source: http://www.pinvoke.net/default.aspx/Enums/SHOWWINDOW_FLAGS.html
  /// </summary>
  public enum ShowWindowCommands : uint

#pragma warning disable CA1069, RCS1234
  {
    SWHIDE = 0,
    SWSHOWNORMAL = 1,
    SWNORMAL = 1,
    SWSHOWMINIMIZED = 2,
    SWSHOWMAXIMIZED = 3,
    SWMAXIMIZE = 3,
    SWSHOWNOACTIVATE = 4,
    SWSHOW = 5,
    SWMINIMIZE = 6,
    SWSHOWMINNOACTIVE = 7,
    SWSHOWNA = 8,
    SWRESTORE = 9,
    SWSHOWDEFAULT = 10,
    SWFORCEMINIMIZE = 11,
    SWMAX = 11,
  }
#pragma warning restore RCS1234, CA1069

  [DllImport("user32.dll")]
  internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

  [DllImport("user32.dll")]
  internal static extern bool ShowWindow(IntPtr hWnd, int ncmdshow);

  [DllImport("user32.dll", CharSet = CharSet.Unicode)]
#pragma warning disable CA1401
  public static extern IntPtr FindWindow(string lpclassname, string lpwindowname);
#pragma warning restore CA1401

  /// <summary>
  /// Used for checking focus.
  /// </summary>
  [DllImport("user32.dll")]
  private static extern IntPtr GetForegroundWindow();

  /// <summary>
  /// Used for checking window visibility.
  /// </summary>
  /// <param name="hWnd"></param>
  [DllImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool IsWindowVisible(IntPtr hWnd);

  #region Window styles
  [Flags]

#pragma warning disable RCS1135
  public enum ExtendedWindowStyles
  {
    WSEXTOOLWINDOW = 1 << 7
  }
#pragma warning restore RCS1135

  public enum GetWindowLongFields
  {
    GWLEXSTYLE = (-20)
  }

  [DllImport("user32.dll")]
#pragma warning disable CA1401
  public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
#pragma warning restore CA1401

  public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
  {
    // Win32 SetWindowLong doesn't clear error on success
    SetLastError(0);

    int error;
    IntPtr result;
    if (IntPtr.Size == 4)
    {
      // use SetWindowLong
      int tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
      error = Marshal.GetLastWin32Error();
      result = new IntPtr(tempResult);
    }
    else
    {
      // use SetWindowLongPtr
      result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
      error = Marshal.GetLastWin32Error();
    }

    if ((result == IntPtr.Zero) && (error != 0))
      throw new System.ComponentModel.Win32Exception(error);

    return result;
  }

  [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
  private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

  [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
  private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

  private static int IntPtrToInt32(IntPtr intptr)
  {
    return unchecked((int)intptr.ToInt64());
  }

  [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
  private static extern void SetLastError(int dwerrorcode);
  #endregion Window styles

  public static Process Process { get; set; }
  public static IntPtr GameWindowHandle { get; set; }
  public static IntPtr OverlayWindowHandle { get; set; }

  public static void HideOverlayFromTaskbar()
  {
    var exStyle = (int)GetWindowLong(OverlayWindowHandle, (int)GetWindowLongFields.GWLEXSTYLE);
    exStyle |= (int)ExtendedWindowStyles.WSEXTOOLWINDOW;
    _ = SetWindowLong(OverlayWindowHandle, (int)GetWindowLongFields.GWLEXSTYLE, (IntPtr)exStyle);
  }

  public static void FocusGame()
  {
    _ = SetForegroundWindow(GameWindowHandle);
    _ = ShowWindow(GameWindowHandle, (int)ShowWindowCommands.SWSHOW);
  }

  public static void FocusOverlay()
  {
    _ = SetForegroundWindow(OverlayWindowHandle);
    _ = ShowWindow(OverlayWindowHandle, (int)ShowWindowCommands.SWSHOW);
  }

  // --- New events for monitoring window focus and visibility ---

  /// <summary>
  /// Raised whenever the game window's focus state changes.
  /// The event argument contains a boolean (true if focused, false if not).
  /// </summary>
  public static event EventHandler<FocusChangedEventArgs> GameFocusChanged;

  /// <summary>
  /// Raised whenever the game window's visibility changes.
  /// The event argument contains a boolean (true if visible, false if hidden).
  /// </summary>
  public static event EventHandler<VisibilityChangedEventArgs> GameVisibilityChanged;

  /// <summary>
  /// Internal state to avoid firing duplicate events.
  /// </summary>
  private static bool s_wasGameFocused = true;
  private static bool s_wasGameVisible = true;
  public static Timer MonitorTimer { get; set; }
  private static bool s_monitoringStarted;

  /// <summary>
  /// Start monitoring automatically in the static constructor.
  /// </summary>
  static ProcessUtils()
  {
    StartMonitoring();
  }

  /// <summary>
  /// Starts a background timer to monitor the game window state.
  /// </summary>
  private static void StartMonitoring()
  {
    if (!s_monitoringStarted)
    {
      s_monitoringStarted = true;
      // Check every 100 milliseconds. Adjust interval as needed.
      MonitorTimer = new Timer(static _ => CheckGameWindowState(), null, 0, 100);
    }
  }

  /// <summary>
  /// Checks the current state of the game window and fires events on state changes.
  /// </summary>
  private static void CheckGameWindowState()
  {
    if (GameWindowHandle == IntPtr.Zero)
    {
      // Game window not set. Nothing to do.
      return;
    }

    // Check focus.
    IntPtr foreground = GetForegroundWindow();
    bool isFocused = foreground == OverlayWindowHandle || foreground == GameWindowHandle;

    // Determine which window is focused.
    var focusedWindow = FocusedWindow.None;
    if (foreground == GameWindowHandle)
    {
      focusedWindow = FocusedWindow.Game;
    }
    else if (foreground == OverlayWindowHandle)
    {
      focusedWindow = FocusedWindow.Overlay;
    }

    // Check visibility.
    bool isVisible = IsWindowVisible(GameWindowHandle);

    // Fire focus event if state changed.
    if (isFocused != s_wasGameFocused)
    {
      GameFocusChanged?.Invoke(null, new FocusChangedEventArgs(isFocused, focusedWindow));
      s_wasGameFocused = isFocused;
    }

    // Fire visibility event if state changed.
    if (isVisible != s_wasGameVisible)
    {
      GameVisibilityChanged?.Invoke(null, new VisibilityChangedEventArgs(isVisible));
      s_wasGameVisible = isVisible;
    }
  }

  /// <summary>
  /// Focuses the specified window (Game or Overlay).
  /// </summary>
  /// <param name="window">The window to focus: Game or Overlay.</param>
  /// <exception cref="NotImplementedException"></exception>
  public static void FocusWindow(FocusedWindow window)
  {
    IntPtr targetHandle = window switch
    {
      FocusedWindow.Game => GameWindowHandle,
      FocusedWindow.Overlay => OverlayWindowHandle,
      _ => IntPtr.Zero
    };

    if (targetHandle == IntPtr.Zero)
    {
      Console.WriteLine($"Error: {window} window handle is not set.");
      return;
    }

    _ = SetForegroundWindow(targetHandle);
    _ = ShowWindow(targetHandle, (int)ShowWindowCommands.SWSHOW);
  }
}
