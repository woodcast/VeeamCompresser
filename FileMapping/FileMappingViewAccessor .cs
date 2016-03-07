using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace com.veeam.Compresser.FileMapping
{
    /// <summary>
    /// Represents a randomly accessed view of a memory-mapped file.
    /// </summary>
    sealed class FileMappingViewAccessor : IDisposable
    {
        private bool disposed = false;

        internal FileMappingViewAccessor(SafeFileMappingViewHandle handle, long offset, long capacity)
        {
            Debug.Assert(handle != null);
            Debug.Assert(!handle.IsInvalid, "handle is invalid");

            FileMappingViewHandle = handle;
            PointerOffset = offset;
            Capacity = capacity;
        }

        /// <summary>
        /// Gets the file handle of a memory-mapped file.
        /// </summary>
        public SafeFileMappingViewHandle FileMappingViewHandle
        {
            get;
            private set;
        }

        private IntPtr Address
        {
            get { return FileMappingViewHandle.DangerousGetHandle(); }
        }

        /// <summary>
        /// Gets the number of bytes by which the starting position of this view is offset 
        /// from the beginning of the memory-mapped file.
        /// </summary>
        public long PointerOffset { get; private set; }

        /// <summary>
        /// Gets the capacity of the accessor.
        /// </summary>
        public long Capacity { get; private set; }

        /// <summary>
        /// Reads a 32-bit integer from the accessor.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public byte ReadByte(long position)
        {
            if (position < 0 || position >= Capacity)
                throw new ArgumentOutOfRangeException("position");
            byte[] value = new byte[1];
            Marshal.Copy(new IntPtr(Address.ToInt64() + position), value, 0, 1);

            return value[0];
        }

        /// <summary>
        /// Reads a 32-bit integer from the accessor.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int ReadBytes(long position, byte[] bytes, int size)
        {
            if (position < 0 || position >= Capacity)
                throw new ArgumentOutOfRangeException("position");
            if (size < 0 || position + size > Capacity)
                throw new ArgumentOutOfRangeException("size");

            Marshal.Copy(new IntPtr(Address.ToInt64() + position), bytes, 0, size);
            
            return size;
        }

        /// <summary>
        /// Reads a 32-bit integer from the accessor.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int ReadInt32(long position)
        {
            if (position < 0 || position >= Capacity)
                throw new ArgumentOutOfRangeException("position");
            int[] value = new int[1];
            Marshal.Copy(new IntPtr(Address.ToInt64() + position), value, 0, 1);

            return value[0];
        }

        /// <summary>
        /// Reads a 64-bit integer from the accessor.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public long ReadInt64(long position)
        {
            if (position < 0 || position >= Capacity)
                throw new ArgumentOutOfRangeException("position");
            long[] value = new long[1];

            IntPtr oldPtr64 = new IntPtr(Address.ToInt64());
            IntPtr newPtr64 = new IntPtr(Address.ToInt64() + position);

            Marshal.Copy(new IntPtr(Address.ToInt64() + position), value, 0, 1);

            return value[0];
        }

        /// <summary>
        /// Reads a structure of type T from the accessor into a provided reference.
        /// </summary>
        /// <typeparam name="T">The type of structure.</typeparam>
        /// <param name="position">The position in the accessor at which to begin reading.</param>
        /// <param name="structure">The structure to contain the read data.</param>
        public void Read<T>(long position, out T structure) where T : struct
        {
            if (position < 0 || position >= Capacity)
                throw new ArgumentOutOfRangeException("position");

            T retValue = default(T);
            int size = Marshal.SizeOf(retValue);
            byte[] arr = new byte[size];
            Marshal.Copy(new IntPtr(Address.ToInt64() + position), arr, 0, size);
            
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(arr, 0, ptr, size);

            retValue = (T)Marshal.PtrToStructure(ptr, retValue.GetType());
            Marshal.FreeHGlobal(ptr);

            // throws
            // ArgumentException
            // ArgumentOutOfRangeException
            // NotSupportedException
            // ObjectDisposedException

            structure = retValue;
        }

        /// <summary>
        /// Writes a byte into the accessor.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        public void WriteByte(long position, byte value)
        {
            if (position < 0 || position >= Capacity)
                throw new ArgumentOutOfRangeException("position");
            Marshal.Copy(new byte[] { value }, 0, new IntPtr(Address.ToInt64() + position), 1);
        }


        /// <summary>
        /// Writes a structure into the accessor.
        /// </summary>
        /// <typeparam name="T">The type of structure.</typeparam>
        /// <param name="position">The number of bytes into the accessor at which to begin writing.</param>
        /// <param name="structure">The structure to write.</param>
        public void Write<T>(long position, ref T structure) where T : struct
        {
            if (position < 0 || position >= Capacity)
                throw new ArgumentOutOfRangeException("position");

            int size = Marshal.SizeOf(structure);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, ptr, true);

            byte[] arr = new byte[size];
            Marshal.Copy(ptr, arr, 0, size);            
            Marshal.FreeHGlobal(ptr);

            Marshal.Copy(arr, 0, new IntPtr(Address.ToInt64() + position), size);

            // throws
            // ArgumentException           
            // ArgumentOutOfRangeException
            // NotSupportedException
            // ObjectDisposedException
        }


        public void Dispose()
        {
            // TODO: implement
            if (!disposed)
            {
                IDisposable disposable = FileMappingViewHandle;
                if (disposable != null)
                    disposable.Dispose();
                disposed = true;
            }
        }
    }
}
