﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsTerminalQuake.Native;
using static WindowsTerminalQuake.Native.User32.AnimateWindowFlags;

namespace WindowsTerminalQuake
{
    public class Toggler : IDisposable
    {
        private Process _process;

        public Toggler(Process process)
        {
            _process = process;

            // Hide from taskbar
            User32.SetWindowLong(_process.MainWindowHandle, User32.GWL_EX_STYLE,
                (User32.GetWindowLong(_process.MainWindowHandle, User32.GWL_EX_STYLE) | User32.WS_EX_TOOLWINDOW) &
                ~User32.WS_EX_APPWINDOW);
//            Console.WriteLine("Error:{0}",User32.GetLastError());

            User32.Rect rect = default;
            var ok = User32.GetWindowRect(_process.MainWindowHandle, ref rect);
            var isOpen = rect.Top >= GetScreenWithCursor().Bounds.Y;
            User32.ShowWindow(_process.MainWindowHandle, NCmdShow.RESTORE);

            var stepCount = 10;

            HotKeyManager.RegisterHotKey(Keys.Oemtilde, KeyModifiers.Control);
            HotKeyManager.RegisterHotKey(Keys.Q, KeyModifiers.Control);

            HotKeyManager.HotKeyPressed += (s, a) =>
            {
                isOpen = User32.GetForegroundWindow() == _process.MainWindowHandle;
                if (isOpen)
                {
                    Console.WriteLine("Close");
                    User32.ShowWindow(_process.MainWindowHandle, NCmdShow.MINIMIZE);
                }
                else
                {
                    Console.WriteLine("Open");
                    User32.ShowWindow(_process.MainWindowHandle, NCmdShow.RESTORE);
                    User32.SetForegroundWindow(_process.MainWindowHandle);
                }
            };
        }

        public void Dispose()
        {
            ResetTerminal(_process);
        }

        private static Screen GetScreenWithCursor()
        {
            return Screen.AllScreens.FirstOrDefault(s => s.Bounds.Contains(Cursor.Position));
        }

        private static void ResetTerminal(Process process)
        {
            var bounds = GetScreenWithCursor().Bounds;

            // Restore taskbar icon
            User32.SetWindowLong(process.MainWindowHandle, User32.GWL_EX_STYLE,
                (User32.GetWindowLong(process.MainWindowHandle, User32.GWL_EX_STYLE) | User32.WS_EX_TOOLWINDOW) &
                User32.WS_EX_APPWINDOW);

            // Reset position
            User32.MoveWindow(process.MainWindowHandle, bounds.X, bounds.Y, bounds.Width, bounds.Height, true);

            // Restore window
            User32.ShowWindow(process.MainWindowHandle, NCmdShow.MAXIMIZE);
        }
    }
}