using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]

namespace com.veeam.Compresser.FileMapping
{    
    /// <summary>
    /// This class includes several Win32 interop definitions.
    /// </summary>
    internal class Win32
    {
        public static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        public const UInt32 FILE_MAP_WRITE = 2;
        public const UInt32 PAGE_READWRITE = 0x04;

        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFileMapping(SafeFileHandle hFile,
            IntPtr pAttributes, UInt32 flProtect,
            UInt32 dwMaximumSizeHigh, UInt32 dwMaximumSizeLow, String pName);

        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        public static extern IntPtr OpenFileMapping(UInt32 dwDesiredAccess,
            Boolean bInheritHandle, String name);

        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        public static extern Boolean CloseHandle(IntPtr handle);

        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        public static extern IntPtr MapViewOfFile(SafeFileMappingHandle hFileMappingObject,
            UInt32 dwDesiredAccess,
            UInt32 dwFileOffsetHigh, UInt32 dwFileOffsetLow,
            IntPtr dwNumberOfBytesToMap);

        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        public static extern Boolean UnmapViewOfFile(IntPtr address);

        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        public static extern Boolean DuplicateHandle(IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle, ref IntPtr lpTargetHandle,
            UInt32 dwDesiredAccess, Boolean bInheritHandle, UInt32 dwOptions);
        
        public const UInt32 DUPLICATE_CLOSE_SOURCE = 0x00000001;
        public const UInt32 DUPLICATE_SAME_ACCESS = 0x00000002;

        [DllImport("Kernel32", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetCurrentProcess();


        public enum ProcessorArchitecture
        {
            X86 = 0,
            X64 = 9,
            @Arm = -1,
            Itanium = 6,
            Unknown = 0xFFFF,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemInfo
        {
            public ProcessorArchitecture ProcessorArchitecture; // WORD
            public uint PageSize; // DWORD
            public IntPtr MinimumApplicationAddress; // (long)void*
            public IntPtr MaximumApplicationAddress; // (long)void*
            public IntPtr ActiveProcessorMask; // DWORD*
            public uint NumberOfProcessors; // DWORD (WTF)
            public uint ProcessorType; // DWORD
            public uint AllocationGranularity; // DWORD
            public ushort ProcessorLevel; // WORD
            public ushort ProcessorRevision; // WORD
        }

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern void GetSystemInfo(out SystemInfo Info);
    }
}
