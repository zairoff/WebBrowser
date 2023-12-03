using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebBrowser
{
    public partial class Form1 : Form
    {
        private readonly Loading loading;
        public Form1()
        {
            InitializeComponent();
            loading = new Loading();
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }

        //System level functions to be used for hook and unhook keyboard input  
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);
        //Declaring Global objects     
        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));

                // Disabling Windows keys 

                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin ||
                    objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) ||
                    objKeyInfo.key == Keys.Escape && (ModifierKeys & Keys.Control) == Keys.Control)
                {
                    return (IntPtr)1; // if 0 is returned then All the above keys will be enabled
                }
            }

            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        static bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webView21.Source = new Uri(ConfigurationManager.AppSettings["url"]);
            objKeyboardProcess = new LowLevelKeyboardProc(CaptureKey);
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ModifierKeys == Keys.Alt || ModifierKeys == Keys.F4)
            {
                var password = ConfigurationManager.AppSettings["password"];
                var passwordForm = new Password();
                passwordForm.ShowDialog();

                if (!password.Equals(passwordForm.textBox1.Text))
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.TaskManagerClosing)
            {
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
        }

        private void webView21_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            this.loading.Show();
        }

        private void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            this.loading.Hide();
        }

        private void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView21.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        }

        private void webView21_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

        private void webView21_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyData == Keys.F1)
            //{
            //    MessageBox.Show("Bum");
            //}
        }
    }
}
