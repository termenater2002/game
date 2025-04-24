using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    [Flags]
    internal enum KeyModifier
    {
        /// <summary>
        ///   <para>No modifier keys.</para>
        /// </summary>
        None = 0,
        /// <summary>
        ///   <para>Alt key (or Option key on macOS).</para>
        /// </summary>
        Alt = 1,
        /// <summary>
        ///   <para>Control key on Windows and Linux. Command key on macOS.</para>
        /// </summary>
        Action = 2,
        /// <summary>
        ///   <para>Shift key.</para>
        /// </summary>
        Shift = 4,
        /// <summary>
        ///   <para>Marks that the Control key modifier is part of the key combination. Resolves to control key on Windows, macOS, and Linux.</para>
        /// </summary>
        Control = 8,
    }

    internal class MuseShortcut
    {
        public readonly string id;
        public readonly KeyCode keyCode;
        public readonly KeyModifier keyModifier;
        public readonly Action action;

        /// <summary>
        /// Source element related to the shortcut.
        /// Used to detect which panel to apply shortcut to.
        /// </summary>
        public readonly VisualElement source;

        /// <summary>
        /// Shortcut requires focus on the source element or its children.
        /// </summary>
        internal bool requireFocus { get; set; }

        /// <summary>
        /// Muse Shortcut
        /// </summary>
        /// <param name="id">Shortcut id</param>
        /// <param name="action">Shortcut action</param>
        /// <param name="keyCode">Shortcut keycode</param>
        /// <param name="keyModifier">Shortcut key modifier</param>
        /// <param name="source">Shortcut source panel</param>
        public MuseShortcut(string id, Action action, KeyCode keyCode, KeyModifier keyModifier = KeyModifier.None, VisualElement source = null)
        {
            this.id = id;
            this.keyCode = keyCode;
            this.keyModifier = keyModifier;
            this.action = action;
            this.source = source;
        }
    }

    internal static class MuseShortcuts
    {
        static Dictionary<KeyCode, List<MuseShortcut>> s_Shortcuts = new Dictionary<KeyCode, List<MuseShortcut>>();

        public static IList<MuseShortcut> GetShortcuts(KeyCode keyCode, KeyModifier keyModifier)
        {
            if (s_Shortcuts.TryGetValue(keyCode, out var shortcuts))
            {
                if (shortcuts != null)
                    return shortcuts.Where(s => s.keyModifier == keyModifier).ToList();
            }

            return null;
        }

        public static void AddShortcut(MuseShortcut shortcut)
        {
            if (shortcut == null)
                return;

            var keyCode = shortcut.keyCode;
            if (s_Shortcuts.ContainsKey(keyCode))
            {
                s_Shortcuts[keyCode] ??= new List<MuseShortcut>();
                s_Shortcuts[keyCode].Add(shortcut);
            }
            else
                s_Shortcuts[keyCode] = new List<MuseShortcut> { shortcut };
        }

        public static void AddShortcuts(IEnumerable<MuseShortcut> shortcuts)
        {
            if (shortcuts == null)
                return;

            foreach (var shortcut in shortcuts)
                AddShortcut(shortcut);
        }

        public static void RemoveShortcut(MuseShortcut shortcut)
        {
            if (shortcut == null)
                return;

            var keyCode = shortcut.keyCode;
            if (s_Shortcuts.ContainsKey(keyCode))
            {
                if (s_Shortcuts[keyCode] != null)
                {
                    s_Shortcuts[keyCode].Remove(shortcut);
                    if (s_Shortcuts[keyCode].Count == 0)
                        s_Shortcuts.Remove(keyCode);
                }
            }
        }

        public static void RemoveShortcuts(IEnumerable<MuseShortcut> shortcuts)
        {
            if (shortcuts == null)
                return;

            foreach (var shortcut in shortcuts)
                RemoveShortcut(shortcut);
        }
    }
}
