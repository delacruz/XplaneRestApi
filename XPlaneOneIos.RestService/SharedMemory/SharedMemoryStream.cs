using System;
using System.IO;

namespace XplaneServices.SharedMemory
{
    internal class SharedMemoryStream : UnmanagedMemoryStream
    {
        private IntPtr _ptr;

        unsafe public SharedMemoryStream(IntPtr ptr, uint size)
            : base((byte*)ptr.ToPointer(), size, size, FileAccess.ReadWrite)
        {
            _ptr = ptr;
        }

        protected override void Dispose(bool disposing)
        {
            SharedMemory.UnmapViewOfFile(_ptr);
            _ptr = IntPtr.Zero;
            base.Dispose(disposing);
        }

        ~SharedMemoryStream()
        {
            Dispose(false);
        }
    };
}