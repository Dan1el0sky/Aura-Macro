using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AuraMacro.Core.Interfaces;
using AuraMacro.Core.Models;

namespace AuraMacro.Core.Windows
{
    public class WindowsInputHook : IInputHook
    {
        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_KEYDOWN = 0x0100;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr _mouseHookId = IntPtr.Zero;
        private IntPtr _keyboardHookId = IntPtr.Zero;

        private LowLevelMouseProc _mouseProc;
        private LowLevelKeyboardProc _keyboardProc;

        public event EventHandler<MacroAction>? OnActionRecorded;

        public WindowsInputHook()
        {
            _mouseProc = MouseHookCallback;
            _keyboardProc = KeyboardHookCallback;
        }

        public void StartRecording()
        {
            _mouseHookId = SetHook(WH_MOUSE_LL, _mouseProc);
            _keyboardHookId = SetHook(WH_KEYBOARD_LL, _keyboardProc);
        }

        public void StopRecording()
        {
            UnhookWindowsHookEx(_mouseHookId);
            UnhookWindowsHookEx(_keyboardHookId);
        }

        private IntPtr SetHook(int idHook, Delegate proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                return SetWindowsHookEx(idHook, proc, GetModuleHandle(curModule.ModuleName!), 0);
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                OnActionRecorded?.Invoke(this, new ClickAction { X = hookStruct.pt.x, Y = hookStruct.pt.y });
            }
            return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                OnActionRecorded?.Invoke(this, new KeyPressAction { Key = ((Keys)vkCode).ToString() });
            }
            return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private enum Keys
        {
            A = 65, B = 66, C = 67, // Just basic for now
            // Add full enum if needed or use System.Windows.Forms.Keys (requires WinForms ref)
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}