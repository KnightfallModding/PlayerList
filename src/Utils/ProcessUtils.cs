using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PlayerList.Utils;

public enum FocusedWindow
{
  None,
  Game,
  Overlay
}

// EventArgs class for focus changes.
public class FocusChangedEventArgs(bool isFocused, FocusedWindow focusedWindow) : EventArgs
{
  public bool IsFocused { get; } = isFocused;
  public FocusedWindow FocusedWindow { get; } = focusedWindow;
}

// EventArgs class for visibility changes.
public class VisibilityChangedEventArgs(bool isVisible) : EventArgs
{
  public bool IsVisible { get; } = isVisible;
}

public static class ProcessUtils
{
  // Source: http://www.pinvoke.net/default.aspx/Enums/SHOWWINDOW_FLAGS.html
  public enum ShowWindowCommands : uint

#pragma warning disable CA1069, RCS1234
  {
    SW_HIDE = 0,
    SW_SHOWNORMAL = 1,
    SW_NORMAL = 1,
    SW_SHOWMINIMIZED = 2,
    SW_SHOWMAXIMIZED = 3,
    SW_MAXIMIZE = 3,
    SW_SHOWNOACTIVATE = 4,
    SW_SHOW = 5,
    SW_MINIMIZE = 6,
    SW_SHOWMINNOACTIVE = 7,
    SW_SHOWNA = 8,
    SW_RESTORE = 9,
    SW_SHOWDEFAULT = 10,
    SW_FORCEMINIMIZE = 11,
    SW_MAX = 11
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

  // Used for checking focus.
  [DllImport("user32.dll")]
  private static extern IntPtr GetForegroundWindow();

  // Used for checking window visibility.
  [DllImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool IsWindowVisible(IntPtr hWnd);

  #region Window styles
  [Flags]

#pragma warning disable RCS1135
  public enum ExtendedWindowStyles
  {
    WSEXTOOLWINDOW = 1 << 7,
  }
#pragma warning restore RCS1135

  public enum GetWindowLongFields
  {
    GWLEXSTYLE = (-20),
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
      var tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
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
    {
      throw new System.ComponentModel.Win32Exception(error);
    }

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
  #endregion

  public static Process Process { get; set; }
  public static IntPtr GameWindowHandle { get; set; }
  public static IntPtr OverlayWindowHandle { get; set; }

  public static void HideOverlayFromTaskbar()
  {
    var exStyle = (int)GetWindowLong(OverlayWindowHandle, (int)GetWindowLongFields.GWLEXSTYLE);
    exStyle |= (int)ExtendedWindowStyles.WSEXTOOLWINDOW;
    SetWindowLong(OverlayWindowHandle, (int)GetWindowLongFields.GWLEXSTYLE, (IntPtr)exStyle);
  }

  public static void FocusGame()
  {
    SetForegroundWindow(GameWindowHandle);
    ShowWindow(GameWindowHandle, (int)ShowWindowCommands.SW_SHOW);
  }

  public static void FocusOverlay()
  {
    SetForegroundWindow(OverlayWindowHandle);
    ShowWindow(OverlayWindowHandle, (int)ShowWindowCommands.SW_SHOW);
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

  // Internal state to avoid firing duplicate events.
  private static bool s_wasGameFocused = true;
  private static bool s_wasGameVisible = true;
  public static Timer S_monitorTimer { get; set; }
  private static bool s_monitoringStarted;

  // Start monitoring automatically in the static constructor.
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
      S_monitorTimer = new Timer(CheckGameWindowState, null, 0, 100);
    }
  }

  /// <summary>
  /// Checks the current state of the game window and fires events on state changes.
  /// </summary>
  /// <param name="state">Unused state object.</param>
  private static void CheckGameWindowState(object state)
  {
    if (GameWindowHandle == IntPtr.Zero)
    {
      // Game window not set. Nothing to do.
      return;
    }

    // Check focus.
    var foreground = GetForegroundWindow();
    var isFocused = foreground == OverlayWindowHandle || foreground == GameWindowHandle;

    // Determine which window is focused.
    var focusedWindow = FocusedWindow.None;
    if (foreground == GameWindowHandle)
      focusedWindow = FocusedWindow.Game;
    else if (foreground == OverlayWindowHandle)
      focusedWindow = FocusedWindow.Overlay;

    // Check visibility.
    var isVisible = IsWindowVisible(GameWindowHandle);

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
  public static void FocusWindow(FocusedWindow window)
  {
    var targetHandle = window switch
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

    SetForegroundWindow(targetHandle);
    ShowWindow(targetHandle, (int)ShowWindowCommands.SW_SHOW);
  }
}
