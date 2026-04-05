using System;
using System.Runtime.InteropServices;
using AuraMacro.Core.Interfaces;

namespace AuraMacro.Core.Windows
{
    public class WindowsInputSimulator : IInputSimulator
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        const uint INPUT_MOUSE = 0;
        const uint INPUT_KEYBOARD = 1;
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        public void SendClick(int x, int y)
        {
            // Note: Simplistic implementation, does not accurately scale for multi-monitor without extra logic
            int screenWidth = 1920; // Hardcoded for simplicity
            int screenHeight = 1080;

            var inputs = new INPUT[3];

            // Move
            inputs[0].type = INPUT_MOUSE;
            inputs[0].U.mi.dx = (int)(x * (65535.0f / screenWidth));
            inputs[0].U.mi.dy = (int)(y * (65535.0f / screenHeight));
            inputs[0].U.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE;

            // Down
            inputs[1].type = INPUT_MOUSE;
            inputs[1].U.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;

            // Up
            inputs[2].type = INPUT_MOUSE;
            inputs[2].U.mi.dwFlags = MOUSEEVENTF_LEFTUP;

            for (int i=0; i<3; i++) {
                SendInput(1, ref inputs[i], INPUT.Size);
            }
        }

        public void SendKeyPress(string key)
        {
            // Placeholder: Parse key string to virtual key code, send down and up
            // Needs Virtual Key mappings
        }
    }
}