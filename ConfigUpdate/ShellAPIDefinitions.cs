using System;
using System.Runtime.InteropServices;
using System.Text;

namespace InToolSet.Util.Win32Shell
{
    public static class ShellAPIDefinitions
    {
        public static Guid IID_IShellFolder = new Guid("{000214E6-0000-0000-C000-000000000046}");
        public const int MAX_PATH = 260;
        public const int S_FALSE = 1;
        public const int S_OK = 0;

        [DllImport("shell32.dll")]
        public static extern int SHGetDesktopFolder(out IntPtr ppshf);
        [DllImport("shlwapi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int StrRetToBuf(IntPtr pstr, IntPtr pidl, StringBuilder pszBuf, int cchBuf);

        [Flags]
        public enum SFGAO : uint
        {
            BROWSABLE = 0x8000000,
            CANCOPY = 1,
            CANDELETE = 0x20,
            CANLINK = 4,
            CANMONIKER = 0x400000,
            CANMOVE = 2,
            CANRENAME = 0x10,
            CAPABILITYMASK = 0x177,
            COMPRESSED = 0x4000000,
            CONTENTSMASK = 0x80000000,
            DISPLAYATTRMASK = 0xfc000,
            DROPTARGET = 0x100,
            ENCRYPTED = 0x2000,
            FILESYSANCESTOR = 0x10000000,
            FILESYSTEM = 0x40000000,
            FOLDER = 0x20000000,
            GHOSTED = 0x8000,
            HASPROPSHEET = 0x40,
            HASSTORAGE = 0x400000,
            HASSUBFOLDER = 0x80000000,
            HIDDEN = 0x80000,
            ISSLOW = 0x4000,
            LINK = 0x10000,
            NEWCONTENT = 0x200000,
            NONENUMERATED = 0x100000,
            READONLY = 0x40000,
            REMOVABLE = 0x2000000,
            SHARE = 0x20000,
            STORAGE = 8,
            STORAGEANCESTOR = 0x800000,
            STORAGECAPMASK = 0x70c50008,
            STREAM = 0x400000,
            VALIDATE = 0x1000000
        }

        [Flags]
        public enum SHCONTF
        {
            FOLDERS = 0x20,
            INCLUDEHIDDEN = 0x80,
            INIT_ON_FIRST_NEXT = 0x100,
            NETPRINTERSRCH = 0x200,
            NONFOLDERS = 0x40,
            SHAREABLE = 0x400,
            STORAGE = 0x800
        }

        [Flags]
        public enum SHGNO
        {
            FORADDRESSBAR = 0x4000,
            FOREDITING = 0x1000,
            FORPARSING = 0x8000,
            INFOLDER = 1,
            NORMAL = 0
        }
    }
}

