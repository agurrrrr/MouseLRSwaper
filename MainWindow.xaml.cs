using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace MouseLRSwaper
{
    public partial class MainWindow : Window
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;
        private int mouseXBefore = 0;

        private LowLevelMouseProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public MainWindow()
        {
            InitializeComponent();
            _proc = HookCallback;
        }

        private void StartHookButton_Click(object sender, RoutedEventArgs e)
        {
            _hookID = SetHook(_proc);
        }

        private void StopHookButton_Click(object sender, RoutedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }
        

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                int mouseX = hookStruct.pt.x; // Reverse the X coordinate
                // get screen width
                int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                Debug.WriteLine("mouseX: " + mouseX + " mouseXBefore: " + mouseXBefore);
                if (mouseXBefore == 0 && mouseX != 0 && mouseX != screenWidth && mouseX != screenWidth - 1 && mouseX != 1)
                {
                    mouseXBefore = mouseX;
                }
                if (mouseXBefore < mouseX)
                {
                    Debug.WriteLine(" mouse move to right");
                     // Mouse is moving to the right -> move the cursor to the left
                     int  diff =  Math.Abs(mouseX - mouseXBefore);
                     Debug.WriteLine("diff1 : " + diff);
                     if (( mouseX - diff*2) < 0)
                     {
                         mouseX = 0;
                     }
                     else
                     {
                         mouseX = ( mouseX - diff*2); 
                     }
                     
                     


                } else if (mouseXBefore > mouseX)
                {
                    Debug.WriteLine(" mouse move to left");
                    // Mouse is moving to the left -> move the cursor to the right
                    int  diff =  Math.Abs(mouseXBefore - mouseX );
                    Debug.WriteLine("diff2 : " + diff);
                    if (( mouseX + diff*2) > screenWidth)
                    {
                        mouseX = screenWidth;
                    }
                    else
                    {
                        mouseX = (mouseX + diff * 2);
                    }
                }
                mouseXBefore = mouseX;
                Debug.WriteLine("mouseX: " + mouseX);
                Debug.WriteLine("mouseXBefore: " + mouseXBefore);
                SetCursorPos(mouseX, hookStruct.pt.y);
                return (IntPtr)1; // Block the event    
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
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

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);
    }
}