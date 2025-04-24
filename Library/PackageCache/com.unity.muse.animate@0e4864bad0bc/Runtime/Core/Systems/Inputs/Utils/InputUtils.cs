using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    static class InputUtils
    {
        static Dictionary<KeyCode, KeyDownEntry> s_KeysDown = new();

        public static event Action<KeyCode> OnKeyDown;
        public static event Action<KeyCode> OnKeyUp;

        struct KeyDownEntry
        {
            public DateTime DownSince;
        }

        public static bool IsExclusiveSelection => !IsControl() && !IsShift();

        public static void ClearKeys()
        {
            s_KeysDown.Clear();
        }

        public static void AltKeyDown()
        {
            KeyDown(KeyCode.LeftAlt);
        }

        public static void AltKeyUp()
        {
            KeyUp(KeyCode.LeftAlt);
        }

        public static void ControlKeyDown()
        {
            KeyDown(KeyCode.LeftControl);
        }

        public static void ControlKeyUp()
        {
            KeyUp(KeyCode.LeftControl);
        }

        public static void ShiftKeyDown()
        {
            KeyDown(KeyCode.LeftShift);
        }

        public static void ShiftKeyUp()
        {
            KeyUp(KeyCode.LeftShift);
        }

        public static void KeyDown(KeyCode keyCode)
        {
            if (s_KeysDown.ContainsKey(keyCode))
                return;

            s_KeysDown.Add(
                keyCode,
                new KeyDownEntry()
                {
                    DownSince = DateTime.Now
                });

            OnKeyDown?.Invoke(keyCode);
        }

        public static void KeyUp(KeyCode keyCode)
        {
            if (!s_KeysDown.ContainsKey(keyCode))
                return;

            OnKeyUp?.Invoke(keyCode);
            s_KeysDown.Remove(keyCode);
        }

        public static KeyPressEvent GetKeyPressEvent(KeyCode keyCode)
        {
            KeyPressEvent.Pool.Get(out var ev);
            ev.KeyCode = keyCode;
            ev.IsAlt = IsAlt();
            ev.IsControlOrCommand = IsControl();
            ev.IsShift = IsShift();
            return ev;
        }

        static bool IsKeyDown(KeyCode keyCode)
        {
            return s_KeysDown.ContainsKey(keyCode);
        }

        public static bool IsKeyHeld(KeyCode keyCode)
        {
            return IsKeyDown(keyCode);
        }

        public static bool IsShift()
        {
            foreach (var keyCode in s_KeysDown.Keys)
            {
                if (IsShiftKey(keyCode))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsShiftKey(KeyCode code)
        {
            return code == KeyCode.LeftShift || code == KeyCode.RightShift;
        }

        public static bool IsControl()
        {
            foreach (var keyCode in s_KeysDown.Keys)
                if (IsControlKey(keyCode))
                    return true;

            return false;
        }

        static bool IsControlKey(KeyCode code)
        {
            return code == KeyCode.LeftCommand || code == KeyCode.LeftControl || code == KeyCode.RightCommand || code == KeyCode.RightControl;
        }

        public static bool IsAlt()
        {
            foreach (var keyCode in s_KeysDown.Keys)
                if (IsAltKey(keyCode))
                    return true;

            return false;
        }

        static bool IsAltKey(KeyCode code)
        {
            return code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        }

        public static void UpdateKeyHolds(Action<KeyPressEvent> keyHold)
        {
            foreach (var key in s_KeysDown.Keys)
            {
                if (IsKeyHeld(key))
                {
                    var ev = GetKeyPressEvent(key);
                    keyHold?.Invoke(ev);
                    ev.Release();
                }
            }
        }
    }
}
