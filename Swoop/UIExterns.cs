using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SwoopLib {
    public static class UIExterns {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")] public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")] public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern IntPtr GetAncestor(IntPtr hWnd, GA_FLAGS gaFlags);

        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

        [DllImport("user32.dll")] public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void minimize_window() {
            ShowWindow(actual_window_handle, 6);
        }
        public static void maximize_window() {
            ShowWindow(actual_window_handle, 3);
        }
        public static void restore_window() {
            ShowWindow(actual_window_handle, 9);
        }

        public static bool in_foreground() { 
            return (GetForegroundWindow() == actual_window_handle);
        }

        public static IntPtr actual_window_handle = current_process_monogame_window_handle();
        public static void find_window_handle() { actual_window_handle = current_process_monogame_window_handle(); }

        public static Rectangle get_window_rect() {
            RECT rect;
            GetWindowRect(actual_window_handle, out rect);
            return new Rectangle(rect.Location, rect.Size);
        }

        public static string get_window_title() {
            StringBuilder sb = new StringBuilder();
            GetWindowText(actual_window_handle, sb, 255);
            return sb.ToString();
        }

        static IntPtr current_process_monogame_window_handle() {
            IntPtr current_window = IntPtr.Zero;
            int this_process_id = Process.GetCurrentProcess().Id;
            StringBuilder sb = new StringBuilder();
            do {
                current_window = FindWindowEx(IntPtr.Zero, current_window, null, null);
                int procid = 0; int threadid = GetWindowThreadProcessId(current_window, out procid);
                if (procid == this_process_id ) {
                    sb.Clear(); GetClassName(current_window, sb, 20);
                    if (sb.ToString() == "SDL_app") {
                        return current_window;
                    }
                }
            } while (current_window != IntPtr.Zero);

            return IntPtr.Zero;
        }

        public enum GA_FLAGS : uint { GA_PARENT = 1, GA_ROOT = 2, GA_ROOTOWNER = 3 }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;

            public static WINDOWPLACEMENT Default {
                get {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left_, int top_, int right_, int bottom_) {
                Left = left_;
                Top = top_;
                Right = right_;
                Bottom = bottom_;
            }

            public int Height { get { return Bottom - Top; } }
            public int Width { get { return Right - Left; } }
            public Size Size { get { return new Size(Width, Height); } }

            public Point Location { get { return new Point(Left, Top); } }

            // Handy method for converting to a System.Drawing.Rectangle
            public Rectangle ToRectangle() { return Rectangle.FromLTRB(Left, Top, Right, Bottom); }

            public static RECT FromRectangle(Rectangle rectangle) {
                return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            public override int GetHashCode() {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                  ^ ((Width << 0x1a) | (Width >> 6))
                  ^ ((Height << 7) | (Height >> 0x19));
            }

            #region Operator overloads

            public static implicit operator Rectangle(RECT rect) {
                return rect.ToRectangle();
            }

            public static implicit operator RECT(Rectangle rect) {
                return FromRectangle(rect);
            }

            #endregion
        }
    }
}
