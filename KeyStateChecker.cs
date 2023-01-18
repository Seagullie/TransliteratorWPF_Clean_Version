using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Key = System.Windows.Input.Key;

namespace TransliteratorWPF_Version
{
    public class KeyStateChecker
    {
        private Key[] modifiers = { Key.LeftCtrl, Key.LeftAlt, Key.LWin, Key.RightCtrl, Key.RightAlt, Key.RWin };

        public bool isCAPSLOCKon()
        {
            var isCapsLockToggled = Keyboard.IsKeyToggled(Key.CapsLock);
            return isCapsLockToggled;
        }

        public bool isShiftPressedDown()
        {
            return Keyboard.IsKeyDown(Key.LeftShift);
        }

        public bool isUpperCase()
        {
            return isShiftPressedDown() || isCAPSLOCKon();
        }

        public bool isLowerCase()
        {
            return (!isShiftPressedDown() && !isCAPSLOCKon());
        }

        public bool isKeyDown(Key key)
        {
            return Keyboard.IsKeyDown(key);
        }

        public bool isModifierPressedDown()
        {
            foreach (Key modifier in modifiers)
            {
                if (Keyboard.IsKeyDown(modifier)) return true;
            }

            return false;
        }
    }
}