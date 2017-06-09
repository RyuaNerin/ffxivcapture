using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFXIVScreenShot
{
    public partial class Form1 : Form
    {
        private readonly NativeMethods.keyboardHookProc m_proc;
        private IntPtr m_hookPtr;
        private Keys m_hookKey;

        public Form1()
        {
            InitializeComponent();

            this.m_proc = this.hookProc;

            this.SetKey(Keys.PrintScreen | Keys.Control);
            this.m_hookKey = Keys.PrintScreen | Keys.Control;
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            var user32 = NativeMethods.LoadLibrary("User32");
            this.m_hookPtr = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, this.m_proc, user32, 0);

            if (this.m_hookPtr == IntPtr.Zero)
            {
                MessageBox.Show(this, "오류가 발생했습니다!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            NativeMethods.UnhookWindowsHookEx(this.m_hookPtr);
        }

        private IntPtr hookProc(int code, IntPtr wParam, ref NativeMethods.keyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                var key = (Keys)lParam.vkCode;
                var wparam = wParam.ToInt64();

                key = key | Control.ModifierKeys;

                if (wparam == NativeMethods.WM_KEYDOWN && key == this.m_hookKey)
                {
                    Console.WriteLine(key);

                    // 스크린샷 찍기
                    var fg = NativeMethods.GetForegroundWindow();
                    if (fg != this.Handle)
                    {
                        var ff = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "FFXIVGAME", null);

                        if (ff != IntPtr.Zero && ff == fg)
                        {
                            Task.Factory.StartNew(CaptureFFXIV, (object)ff);

                            return new IntPtr(1);
                        }
                    }
                }
            }
            return NativeMethods.CallNextHookEx(this.m_hookPtr, code, wParam, ref lParam);
        }

        private static void CaptureFFXIV(object o_ffHwnd)
        {
            var ffHwnd = (IntPtr)o_ffHwnd;

            if (!NativeMethods.GetWindowRect(ffHwnd, out NativeMethods.RECT wRect))
                return;

            var nRect = new Rectangle(wRect.Left, wRect.Top, wRect.Right - wRect.Left, wRect.Bottom - wRect.Top);

            using (var img = new Bitmap(nRect.Width, nRect.Height, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(img))
                {
                    var hdc = g.GetHdc();
                    NativeMethods.PrintWindow(ffHwnd, hdc, NativeMethods.PW_RENDERFULLCONTENT);
                    g.ReleaseHdc(hdc);

                    var hRgn = IntPtr.Zero;
                    try
                    {
                        hRgn = NativeMethods.CreateRectRgn(0, 0, 0, 0);

                        if (hRgn != IntPtr.Zero)
                        {
                            NativeMethods.GetWindowRgn(ffHwnd, hRgn);
                            var region = Region.FromHrgn(hRgn);
                            if (!region.IsEmpty(g))
                            {
                                g.ExcludeClip(region);
                                g.Clear(Color.White);
                            }
                        }
                    }
                    finally
                    {
                        if (hRgn != IntPtr.Zero)
                            NativeMethods.DeleteObject(hRgn);
                    }
                }

                var path =
                    Path.Combine(
                        Path.GetDirectoryName(Application.ExecutablePath),
                        DateTime.Now.ToString("\"ffxiv\"_yyyyMMdd_HHmmss_ffff\".png\"")
                    );
                img.Save(path,
                    ImageFormat.Png);
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            this.SetKey(e.KeyData);

            this.m_hookKey = e.KeyData;
            this.Focus();
        }

        private static readonly Keys[] DisallowKeys =
        {
            Keys.None,
            Keys.LButton,
            Keys.RButton,
            Keys.Cancel,
            Keys.MButton,
            Keys.XButton1,
            Keys.XButton2,
            Keys.LineFeed,
            Keys.Clear,
            Keys.ShiftKey,
            Keys.ControlKey,
            Keys.Menu,
            Keys.Capital,
            Keys.CapsLock,
            Keys.KanaMode,
            Keys.HanguelMode,
            Keys.HangulMode,
            Keys.JunjaMode,
            Keys.FinalMode,
            Keys.HanjaMode,
            Keys.KanjiMode,
            Keys.IMEConvert,
            Keys.IMENonconvert,
            Keys.IMEAccept,
            Keys.IMEAceept,
            Keys.IMEModeChange,
            Keys.Select,
            Keys.Print,
            Keys.Execute,
            Keys.Help,
            Keys.LWin,
            Keys.RWin,
            Keys.Apps,
            Keys.Sleep,
            Keys.Decimal,
            Keys.NumLock,
            Keys.LShiftKey,
            Keys.RShiftKey,
            Keys.LControlKey,
            Keys.RControlKey,
            Keys.LMenu,
            Keys.RMenu,
            Keys.BrowserBack,
            Keys.BrowserForward,
            Keys.BrowserRefresh,
            Keys.BrowserStop,
            Keys.BrowserSearch,
            Keys.BrowserFavorites,
            Keys.BrowserHome,
            Keys.VolumeMute,
            Keys.VolumeDown,
            Keys.VolumeUp,
            Keys.MediaNextTrack,
            Keys.MediaPreviousTrack,
            Keys.MediaStop,
            Keys.MediaPlayPause,
            Keys.LaunchMail,
            Keys.SelectMedia,
            Keys.LaunchApplication1,
            Keys.LaunchApplication2,
            Keys.OemSemicolon,
            Keys.Oem1,
            Keys.Oemplus,
            Keys.Oemcomma,
            Keys.OemMinus,
            Keys.OemPeriod,
            Keys.OemQuestion,
            Keys.Oem2,
            Keys.Oemtilde,
            Keys.Oem3,
            Keys.OemOpenBrackets,
            Keys.Oem4,
            Keys.OemPipe,
            Keys.Oem5,
            Keys.OemCloseBrackets,
            Keys.Oem6,
            Keys.OemQuotes,
            Keys.Oem7,
            Keys.Oem8,
            Keys.OemBackslash,
            Keys.Oem102,
            Keys.ProcessKey,
            Keys.Packet,
            Keys.Attn,
            Keys.Crsel,
            Keys.Exsel,
            Keys.EraseEof,
            Keys.Play,
            Keys.Zoom,
            Keys.NoName,
            Keys.Pa1,
            Keys.OemClear,
        };
        private void SetKey(Keys key)
        {
            var vkey = (key & ~Keys.Modifiers);

            if (Array.IndexOf<Keys>(DisallowKeys, vkey) >= 0)
                return;

            var sb = new StringBuilder(128);
            
            if (key.HasFlag(Keys.Control))
                sb.Append("Ctrl + ");

            if (key.HasFlag(Keys.Alt))
                sb.Append("Alt + ");

            if (key.HasFlag(Keys.Shift))
                sb.Append("Shift + ");

            if (key.HasFlag(Keys.LWin) || key.HasFlag(Keys.RWin))
                sb.Append("Win + ");
            
            sb.Append(vkey.ToString());

            this.label2.Tag  = key;
            this.label2.Text = sb.ToString();
        }

        private static class NativeMethods
        {
            public delegate IntPtr keyboardHookProc(int code, IntPtr wParam, ref keyboardHookStruct lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hInstance);

            [DllImport("user32.dll")]
            public static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, ref keyboardHookStruct lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

            [DllImport("user32.dll")]
            public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteObject(IntPtr hObject);

            public const int WH_KEYBOARD_LL = 13;
            public const int WM_KEYDOWN = 0x100;

            public const int PW_RENDERFULLCONTENT = 2;

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct keyboardHookStruct
            {
                public int vkCode;
                public int scanCode;
                public int flags;
                public int time;
                public int dwExtraInfo;
            }
        }
    }
}
