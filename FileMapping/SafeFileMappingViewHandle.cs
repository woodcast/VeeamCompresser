using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace com.veeam.Compresser.FileMapping
{
    sealed class SafeFileMappingViewHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFileMappingViewHandle(IntPtr handle, ulong numBytes)
            : base(true)
        {
            Debug.Assert(handle != IntPtr.Zero, "handle is invalid (zero)");
            Debug.Assert(handle != new IntPtr(-1), "handle is invalid (-1)");

            SetHandle(handle);
            ByteLength = numBytes;
        }

        protected override bool ReleaseHandle()
        {
            // TODO: посмотреть, если будет время
            IntPtr prevHandle = this.handle;
            SetHandle(IntPtr.Zero);
            return Win32.UnmapViewOfFile(prevHandle); /* == true If the function succeeds*/
        }

        /// <summary>
        /// Gets the size of the buffer, in bytes.
        /// </summary>
        public ulong ByteLength { get; private set; }
    }
}
