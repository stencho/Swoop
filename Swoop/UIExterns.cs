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

        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")] public static extern void SetLastError(uint dwErrCode);

        [DllImport("user32.dll", SetLastError = true)] public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", SetLastError = true)] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll", SetLastError = true)] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);
        [DllImport("dwmapi.dll")] public static extern void DwmIsCompositionEnabled(ref int enabledptr);
        [DllImport("dwmapi.dll")] public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMarInset);
        [DllImport("user32.dll", SetLastError = true)] public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("dwmapi.dll")] static extern UInt32 DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out bool pvAttribute, int cbAttribute);
        [DllImport("dwmapi.dll")] static extern UInt32 DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out UInt32 pvAttribute, int cbAttribute);

        public static class Dwm {
            public static class GetWindowAttribute {
                public static bool ALLOW_NCPAINT() {
                    bool b = false;
                    UInt32 res = DwmGetWindowAttribute(actual_window_handle, DwmWindowAttribute.DWMWA_ALLOW_NCPAINT, out b, Marshal.SizeOf(typeof(bool)));
                    Debug.WriteLine(BitConverter.ToString(BitConverter.GetBytes(res)) + " " + BitConverter.ToString(BitConverter.GetBytes(b)));
                    return b;
                }

                public static Color DWMWA_BORDER_COLOR() {
                    UInt32 c;
                    UInt32 res = DwmGetWindowAttribute(actual_window_handle, DwmWindowAttribute.DWMWA_BORDER_COLOR, out c, Marshal.SizeOf(typeof(UInt32)));
                    Debug.WriteLine(BitConverter.ToString(BitConverter.GetBytes(res)) + " " + BitConverter.ToString(BitConverter.GetBytes(c)));
                    return Color.FromArgb(0,0,0,0);
                }
            }
        }

        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const int LWA_ALPHA = 0x00000002;
        public const int WS_EX_LAYERED = 0x00080000;
        //const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public static IntPtr HWND_TOPMOST = (IntPtr)(-1);
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int S_OK = 0x00000000;
        [Flags] public enum WS : long {
            /// <summary>The window has a thin-line border.</summary>
            BORDER = 0x800000,
            /// <summary>The window has a title bar (includes the BORDER style).</summary>
            CAPTION = 0xc00000,
            /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
            DLGFRAME = 0x400000,
            /// <summary>The window is initially maximized.</summary>
            MAXIMIZE = 0x1000000,
            /// <summary>The window has a maximize button. Cannot be combined with the EX_CONTEXTHELP style. The SYSMENU style must also be specified.</summary>
            MAXIMIZEBOX = 0x10000,
            /// <summary>The window is initially minimized.</summary>
            MINIMIZE = 0x20000000,
            /// <summary>The window has a minimize button. Cannot be combined with the EX_CONTEXTHELP style. The SYSMENU style must also be specified.</summary>
            MINIMIZEBOX = 0x20000,
            /// <summary>The window has a sizing border.</summary>
            SIZEFRAME = 0x40000,
            /// <summary>The window has a window menu on its title bar. The CAPTION style must also be specified.</summary>
            SYSMENU = 0x80000,
            THICKFRAME = 0x00040000L,
            POPUP = 0x80000000L
        }

        public enum DwmWindowAttribute {
            DWMWA_NCRENDERING_ENABLED,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_PASSIVE_UPDATE_MODE,
            DWMWA_USE_HOSTBACKDROPBRUSH,
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33,
            DWMWA_BORDER_COLOR,
            DWMWA_CAPTION_COLOR,
            DWMWA_TEXT_COLOR,
            DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
            DWMWA_SYSTEMBACKDROP_TYPE,
            DWMWA_LAST
        }

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
