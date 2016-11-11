using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public int pid;
        //public int baseAddress = 0x0018FDA0;
        public int baseAddress = 0x0018FD9C;
        public int blood_addr, stage_addr, x_addr, y_addr;
        public int blood, stage, x, y;
        public bool locked = false;
        public bool add_blood = false;
        public string processName = "NS-SHAFT";
        public byte[] buffer = new byte[4];
        public IntPtr byteAddress;
        public Point defPnt = new Point();
        public IntPtr hwnd;

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory
            (
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                IntPtr lpBuffer,
                int nSize,
                IntPtr lpNumberOfBytesRead
            );

        [DllImportAttribute("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr OpenProcess
            (
                int dwDesiredAccess,
                bool bInheritHandle,
                int dwProcessId
            );

        [DllImport("kernel32.dll")]
        private static extern void CloseHandle
            (
                IntPtr hObject
            );

        [DllImportAttribute("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory
            (
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                int[] lpBuffer,
                int nSize,
                IntPtr lpNumberOfBytesWritten
            );

        [DllImport("user32.dll")]
        static extern bool GetCursorPos
            (
                ref Point lpPoint
            );

        [DllImport("user32.dll")]
        private static extern int GetWindowRect
            (
                IntPtr hwnd, out Rect lpRect
            );

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow
            (
                string strclassName, 
                string strWindowName
            );

        public static int GetPid(string windowTitle)
        {
            int rs = 0;
            Process[] arrayProcess = Process.GetProcesses();
            foreach (Process p in arrayProcess)
            {
                if (p.MainWindowTitle.IndexOf(windowTitle) != -1)
                {
                    rs = p.Id;
                    break;
                }
            }

            return rs;
        }
        public static int GetPidByProcessName(string processName)
        {
            Process[] arrayProcess = Process.GetProcessesByName(processName);

            foreach (Process p in arrayProcess)
            {
                return p.Id;
            }
            return 0;
        }


        public Form1()
        {
            InitializeComponent();
            try
            {
                hwnd = FindWindow(null, "NS-SHAFT");
                byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0); //獲取緩衝區位址
                pid = GetPid(processName);
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);
                ReadProcessMemory(hProcess, (IntPtr)baseAddress, byteAddress, 4, IntPtr.Zero);
                blood_addr = Marshal.ReadInt32(byteAddress) + 0x1170;
                stage_addr = Marshal.ReadInt32(byteAddress) + 0x1300;
                x_addr = Marshal.ReadInt32(byteAddress) + 0x1158;
                y_addr = Marshal.ReadInt32(byteAddress) + 0x115c;
                CloseHandle(hProcess);
            }
            catch
            {
                label1.Text = "GG";
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);
            if (add_blood)
            {
                WriteProcessMemory(hProcess, (IntPtr)0x00406312, new int[] { 0xAC }, 1, IntPtr.Zero);
                add_blood = false;
            }
            else
            {
                WriteProcessMemory(hProcess, (IntPtr)0x00406312, new int[] { 0x84 }, 1, IntPtr.Zero);
                add_blood = true;
            }
            CloseHandle(hProcess);
        }

        private void Timer1_Tick(object Sender, EventArgs e)
        {
            try
            {
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);

                ReadProcessMemory(hProcess, (IntPtr)blood_addr, byteAddress, 4, IntPtr.Zero);
                blood = Marshal.ReadInt32(byteAddress);
                ReadProcessMemory(hProcess, (IntPtr)stage_addr, byteAddress, 4, IntPtr.Zero);
                stage = Marshal.ReadInt32(byteAddress);
                ReadProcessMemory(hProcess, (IntPtr)x_addr, byteAddress, 4, IntPtr.Zero);
                x = Marshal.ReadInt32(byteAddress);
                //WriteProcessMemory(hProcess, (IntPtr)y_addr, new int[] { 190 }, 4, IntPtr.Zero);
                ReadProcessMemory(hProcess, (IntPtr)y_addr, byteAddress, 4, IntPtr.Zero);
                y = Marshal.ReadInt32(byteAddress);

                Rect rect = new Rect();
                GetWindowRect(hwnd, out rect);
                GetCursorPos(ref defPnt);
                WriteProcessMemory(hProcess, (IntPtr)x_addr, new int[] { defPnt.X - rect.Left - 30}, 4, IntPtr.Zero);
                WriteProcessMemory(hProcess, (IntPtr)y_addr, new int[] { defPnt.Y - rect.Top - 100}, 4, IntPtr.Zero);


                label2.Text = blood.ToString();
                label4.Text = stage.ToString();
                label6.Text = x.ToString();
                label8.Text = y.ToString();
                if (locked)
                {
                    WriteProcessMemory(hProcess, (IntPtr)blood_addr, new int[] { 12 }, 4, IntPtr.Zero);
                }
                CloseHandle(hProcess);
            }
            catch
            {
                label1.Text = "GG";
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);
            ReadProcessMemory(hProcess, (IntPtr)stage_addr, byteAddress, 4, IntPtr.Zero);
            stage = Marshal.ReadInt32(byteAddress);
            WriteProcessMemory(hProcess, (IntPtr)stage_addr, new int[] { stage + 1 }, 4, IntPtr.Zero);
            CloseHandle(hProcess);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);
            ReadProcessMemory(hProcess, (IntPtr)stage_addr, byteAddress, 4, IntPtr.Zero);
            stage = Marshal.ReadInt32(byteAddress);
            WriteProcessMemory(hProcess, (IntPtr)stage_addr, new int[] { stage + 100 }, 4, IntPtr.Zero);
            CloseHandle(hProcess);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked == true)
            {
                IntPtr hProcess = OpenProcess(0x1F0FFF, false, pid);
                WriteProcessMemory(hProcess, (IntPtr)blood_addr, new int[] { 200 }, 4, IntPtr.Zero);
                CloseHandle(hProcess);
                locked = true;
            }
            else
            {
                locked = false;
            }
        }
    }
}
