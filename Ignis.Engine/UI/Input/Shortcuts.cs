using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Ignis.Engine.UI.Input
{
    /// <summary>
    /// Represents a keyboard shortcut (e.g., "Ctrl+Z", "Shift+Del").
    /// </summary>
    public class Shortcut
    {
        public Keys Key { get; }
        public ModifierKeys Modifiers { get; }
        public Action Action { get; }

        public Shortcut(Keys key, ModifierKeys modifiers, Action action)
        {
            Key = key;
            Modifiers = modifiers;
            Action = action;
        }

        public bool Matches(Keys key, ModifierKeys modifiers)
        {
            return Key == key && Modifiers == modifiers;
        }
    }

    /// <summary>
    /// Builder for declarative shortcut definitions.
    /// </summary>
    public class ShortcutBuilder
    {
        private readonly List<Shortcut> _shortcuts = new();

        public ShortcutBuilder Bind(string combo, Action action)
        {
            var (key, modifiers) = ParseCombo(combo);
            _shortcuts.Add(new Shortcut(key, modifiers, action));
            return this;
        }

        public IReadOnlyList<Shortcut> Build() => _shortcuts;

        private static (Keys key, ModifierKeys modifiers) ParseCombo(string combo)
        {
            var parts = combo.Split('+');
            var modifiers = ModifierKeys.None;
            Keys key = Keys.None;

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                switch (trimmed.ToLowerInvariant())
                {
                    case "ctrl":
                    case "control":
                        modifiers |= ModifierKeys.Control;
                        break;
                    case "shift":
                        modifiers |= ModifierKeys.Shift;
                        break;
                    case "alt":
                        modifiers |= ModifierKeys.Alt;
                        break;
                    default:
                        // Handle special key name mappings
                        var keyName = trimmed.ToLowerInvariant() switch
                        {
                            "del" => "Delete",
                            "esc" => "Escape",
                            "ins" => "Insert",
                            "pgup" => "PageUp",
                            "pgdn" => "PageDown",
                            _ => trimmed
                        };
                        
                        // Try to parse as a key
                        if (Enum.TryParse<Keys>(keyName, true, out var parsedKey))
                        {
                            key = parsedKey;
                        }
                        break;
                }
            }

            return (key, modifiers);
        }
    }

    /// <summary>
    /// Collection of shortcuts attached to a view.
    /// </summary>
    public class ShortcutCollection
    {
        private readonly List<Shortcut> _shortcuts = new();

        public void Add(Shortcut shortcut) => _shortcuts.Add(shortcut);
        
        public IReadOnlyList<Shortcut> Shortcuts => _shortcuts;

        /// <summary>
        /// Attempts to handle a key event with registered shortcuts.
        /// Returns true if a shortcut was executed.
        /// </summary>
        public bool TryHandle(Keys key, ModifierKeys modifiers)
        {
            foreach (var shortcut in _shortcuts)
            {
                if (shortcut.Matches(key, modifiers))
                {
                    shortcut.Action();
                    return true;
                }
            }
            return false;
        }
    }
}

