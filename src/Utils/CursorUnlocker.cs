// Found on https://github.com/Eveldee/NobetaTrainer/blob/master/NobetaTrainer/Behaviours/CursorUnlocker.cs (Credits where credits are due)

using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace PlayerList.Utils;

internal class CursorUnlocker : MonoBehaviour
{
  internal static bool IsCursorUnlocked { get; set; }

  private static readonly bool _currentlySettingCursor;
  private static CursorLockMode _lastLockMode;
  private static bool _lastVisibleState;

  private static void Awake()
  {
    MelonLogger.Msg("UnlockCursor Awake");

    _lastLockMode = Cursor.lockState;
    _lastVisibleState = Cursor.visible;

    // Fix shortcuts and display in fullscreen mode and force borderless
    Display.displays[0].Activate();
  }

  // TODO: Fix blinking cursor
  // private void Update()
  // {
  //   _currentlySettingCursor = true;

  //   if (IsCursorUnlocked)
  //   {
  //     Cursor.lockState = CursorLockMode.None;
  //     Cursor.visible = true;
  //   }
  //   else
  //   {
  //     Cursor.lockState = _lastLockMode;
  //     Cursor.visible = _lastVisibleState;
  //   }

  //   _currentlySettingCursor = false;
  // }

  [HarmonyPatch(typeof(Cursor), nameof(Cursor.lockState), MethodType.Setter)]
  [HarmonyPrefix]
  private static void CursorLockStateSetPrefix(ref CursorLockMode value)
  {
    if (!_currentlySettingCursor)
    {
      _lastLockMode = value;

      if (IsCursorUnlocked)
        value = CursorLockMode.None;
    }
  }

  [HarmonyPatch(typeof(Cursor), nameof(Cursor.visible), MethodType.Setter)]
  [HarmonyPrefix]
  private static void CursorVisibleSetPrefix(ref bool value)
  {
    if (!_currentlySettingCursor)
    {
      _lastVisibleState = value;

      if (IsCursorUnlocked)
        value = true;
    }
  }
}
