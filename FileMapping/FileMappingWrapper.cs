using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace com.veeam.Compresser.FileMapping
{
    /// <summary>
    /// По условию задачи должен использовать .net 3.5 в котором нет 
    /// System.IO.MemoryMappedFiles.MemoryMappedFile
    /// </summary>
    sealed class FileMappingWrapper : IDisposable
    {
        private bool _disposed = false;

        private long _fileSize = -1;

        private FileMappingWrapper(SafeFileMappingHandle handle)
        {
            Debug.Assert(handle != null);
            Debug.Assert(!handle.IsInvalid, "handle is invalid");

            FileMappingHandle = handle;
        }

        /// <summary>
        /// Gets the file handle of a memory-mapped file.
        /// </summary>
        public SafeFileMappingHandle FileMappingHandle
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a memory-mapped file from a file on disk.
        /// </summary>
        /// <param name="path">The path to file to map.</param>
        /// <returns>A memory-mapped file.</returns>
        public static FileMappingWrapper CreateFromFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path wrong");
            if (!File.Exists(path))
                throw new ArgumentNullException("file doesn't exists");

            using (var file = File.Open(path, FileMode.Open, FileAccess.ReadWrite))
            {
                FileMappingAccess rights = new FileMappingAccess();
                FileMappingSecurity security = new FileMappingSecurity();

                return CreateFromFile(
                    file,
                    Guid.NewGuid().ToString(),
                    file.Length,
                    rights,
                    security,
                    HandleInheritability.Inheritable,
                    true // TODO: уточнить
                    );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="mapName"></param>
        /// <param name="capacity"></param>
        /// <param name="access"></param>
        /// <param name="memoryMappedFileSecurity"></param>
        /// <param name="inheritability"></param>
        /// <param name="leaveOpen"></param>
        /// <returns></returns>
        public static FileMappingWrapper CreateFromFile(
            FileStream fileStream,
            string mapName,
            long capacity,
            FileMappingAccess access,
            FileMappingSecurity memoryMappedFileSecurity,
            HandleInheritability inheritability,
            bool leaveOpen
            )
        {
            if (fileStream == null || fileStream.SafeFileHandle.IsInvalid
                || string.IsNullOrEmpty(mapName))
                throw new ArgumentNullException("fileStream or mapname is null");

            SafeFileMappingHandle handle = new SafeFileMappingHandle(Win32.CreateFileMapping(
                fileStream.SafeFileHandle,
                IntPtr.Zero, 
                Win32.PAGE_READWRITE,
                0, 
                unchecked((UInt32)capacity), 
                mapName
                )
            );
            
            if (handle.IsInvalid)
                throw new Exception("Could not create memory-mapped file.");

            // throws 
            // ArgumentException
            // ArgumentNullException
            // ArgumentOutOfRangeException
            // ObjectDisposedException
            // UnauthorizedAccessException
            // IOException
            var wrapper = new FileMappingWrapper(handle);
            wrapper._fileSize = fileStream.Length;

            return wrapper;
        }

        /// <summary>
        /// Creates a FileMappingViewAccessor that maps to a view of the memory-mapped file.
        /// </summary>
        /// <returns></returns>
        public FileMappingViewAccessor CreateViewAccessor()
        {
            // TODO: implement
            return CreateViewAccessor(0, 0);
        }

        /// <summary>
        /// Creates a FileMappingViewAccessor that maps to a view of the memory-mapped file, 
        /// and that has the specified offset and size.
        /// </summary>
        /// <param name="offset">The byte at which to start the view.</param>
        /// <param name="size">The size of the view. Specify 0 (zero) to create a view that 
        /// starts at offset and ends approximately at the end of the memory-mapped file.
        /// </param>
        /// <returns>A randomly accessible block of memory.</returns>
        public FileMappingViewAccessor CreateViewAccessor(long offset, long size)
        {
            long lengthFromOffsetToTheEnd = (size == 0) ? this._fileSize - offset : size;

            SafeFileMappingViewHandle handle = new SafeFileMappingViewHandle(Win32.MapViewOfFile(
                this.FileMappingHandle,
                Win32.FILE_MAP_WRITE,
                unchecked((UInt32)(offset >> 32)),
                unchecked((UInt32)(offset & 0xFFFFFFFF)),
                unchecked((IntPtr)size)
                ),
                (ulong)lengthFromOffsetToTheEnd);

            if (handle.IsInvalid)
                throw new Exception("Could not create a view of memory-mapped file.");

            return new FileMappingViewAccessor(handle, offset, lengthFromOffsetToTheEnd);

            // throws
            // ArgumentOutOfRangeException
            // UnauthorizedAccessException
            // IOException
        }

        /// <summary>
        /// Creates a stream that maps to a view of the memory-mapped file, 
        /// and that has the specified offset and size.
        /// </summary>
        /// <param name="offset">The byte at which to start the view.</param>
        /// <param name="size">The size of the view. Specify 0 (zero) to create a view that 
        /// starts at offset and ends approximately at the end of the memory-mapped file.</param>
        /// <returns>A stream of memory that has the specified offset and size.</returns>
        public FileMappingViewStream CreateViewStream(long offset, long size)
        {
            long lengthFromOffsetToTheEnd = (size == 0) ? this._fileSize - offset : size;

            // TODO: implement
            SafeFileMappingViewHandle handle = 
                new SafeFileMappingViewHandle(IntPtr.Zero, (ulong)lengthFromOffsetToTheEnd); // =

            return new FileMappingViewStream(handle, offset, lengthFromOffsetToTheEnd);
        }


        public void Dispose()
        {
            // TODO: implement
            if (!_disposed)
            {
                IDisposable disposable = FileMappingHandle;
                if (disposable != null)
                    disposable.Dispose();
                _disposed = true;
            }
        }
    }
}
