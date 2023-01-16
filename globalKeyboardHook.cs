using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using System.Windows.Forms;

namespace Utilities
{
    /// <summary>
    /// A class that manages a global low level keyboard hook
    /// </summary>
    public class globalKeyboardHook
    {
        #region Constant, Structure and Delegate Definitions

        /// <summary>
        /// defines the callback type for the hook
        /// </summary>
        public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);

        public struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        #endregion Constant, Structure and Delegate Definitions

        #region Instance Variables

        /// <summary>
        /// The collections of keys to watch for
        /// </summary>
        public List<Keys> HookedKeys = new List<Keys>();

        /// <summary>
        /// Handle to the hook, need this to unhook and call the next hook
        /// </summary>
        private IntPtr hhook = IntPtr.Zero;

        private keyboardHookProc hookProcDelegate;
        public bool skipInjected = true;
        public bool alwaysAllowInjected = false;

        #endregion Instance Variables

        #region Events

        /// <summary>
        /// Occurs when one of the hooked keys is pressed
        /// </summary>
        public event KeyEventHandler KeyDown;

        /// <summary>
        /// Occurs when one of the hooked keys is released
        /// </summary>
        public event KeyEventHandler KeyUp;

        #endregion Events

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="globalKeyboardHook"/> class and installs the keyboard hook.
        /// </summary>
        public globalKeyboardHook()
        {
            hookProcDelegate = hookProc;

            hook();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="globalKeyboardHook"/> is reclaimed by garbage collection and uninstalls the keyboard hook.
        /// </summary>
        ~globalKeyboardHook()
        {
            unhook();
        }

        #endregion Constructors and Destructors

        #region Public Methods

        /// <summary>
        /// Installs the global hook
        /// </summary>
        ///

        public void hook()
        {
            IntPtr hInstance = LoadLibrary("User32");

            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, hookProcDelegate, hInstance, 0);
        }

        /// <summary>
        /// Uninstalls the global hook
        /// </summary>
        public void unhook()
        {
            UnhookWindowsHookEx(hhook);
        }

        /// <summary>
        /// The callback for the keyboard hook
        /// </summary>
        /// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
        /// <param name="wParam">The event type</param>
        /// <param name="lParam">The keyhook event information</param>
        /// <returns></returns>
        public int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
        {
            // if the kb event is injected, then we skip it

            if (lParam.flags == 0x00000010 && !alwaysAllowInjected && skipInjected)
            {
                return CallNextHookEx(hhook, code, wParam, ref lParam);
            }

            if (code >= 0)
            {
                Keys key = (Keys)lParam.vkCode;
                // if event is injected and has unicode character, it's gonna arrive as packet
                // and definition of packet is:
                // Summary:
                //     Used to pass Unicode characters as if they were keystrokes. The Packet key value
                //     is the low word of a 32-bit virtual-key value used for non-keyboard input methods.
                //Packet = 0xE7,
                // ~
                // gotta figure out what low word and high word are
                // here's an article about that:
                // https://www.scs.stanford.edu/05au-cs240c/lab/i386/s02_02.htm#:~:text=The%20word%20containing%20bit%200,the%20address%20of%20the%20doubleword.
                // dword = doubleword.
                // the first word in doubleword is the low word, the second word in doubleword is the high word
                // 32-bit virtual-key value = doubleword. So it's not enough to just have the low word, huh?
                // they keys in the enum Keys, what range do they lie in?
                // the max is 0x40000 (262144)
                // the min is 0x0 (0)
                // how much can we fit in 32 bits? 4294967296
                // what about 31 bit? 2147483648
                // 32/31 bits is way too much for the keys
                // what about one word, that is, 16 bits?
                // 16 bits can contain 65536 numbers
                // that's not enough to span all the keys

                // would be easier to just send ascii character as non unicode
                // other non-en layouts also send non unicode, but it is converted to unicode downstream.
                // I'm not 100% sure, though
                // or you could pass everything and hope .toUnicode in .onKeyDown works properly
                // .toUnicode struggles with packet key, it seems
                // we need to somehow transform packet to keyCode
                // Can't find how to do that

                if (true || HookedKeys.Contains(key)) // warning: pointless condition
                {
                    KeyEventArgs kea = new KeyEventArgs(key);
                    if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                    {
                        KeyDown(this, kea);
                    }
                    else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                    {
                        KeyUp(this, kea);
                    }
                    if (kea.Handled)
                        return 1;
                }
            }
            return CallNextHookEx(hhook, code, wParam, ref lParam);
        }

        #endregion Public Methods

        #region DLL imports

        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        #endregion DLL imports
    }
}