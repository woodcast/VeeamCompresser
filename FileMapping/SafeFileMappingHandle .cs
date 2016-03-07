using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace com.veeam.Compresser.FileMapping
{
    /// <summary>
    /// Provides a safe handle that represents a memory-mapped file for sequential access.
    /// </summary>
    sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFileMappingHandle(IntPtr handle)
            : base(true)
        {
            Debug.Assert(handle != IntPtr.Zero);
            Debug.Assert(handle != new IntPtr(-1));

            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            // TODO: посмотреть, если будет время
            IntPtr prevHandle = this.handle;
            SetHandle(IntPtr.Zero);
            return Win32.CloseHandle(prevHandle); /* == true If the function succeeds*/
        }
    }
}
