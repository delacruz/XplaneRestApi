using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace XplaneServices.SharedMemory
{
    class SharedMemoryHandle : CriticalHandleMinusOneIsInvalid
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle) ? true : false;
        }
       

        public SharedMemoryHandle(IntPtr h)
        {
            handle = h;
        }

        public override bool IsInvalid
        {
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        internal IntPtr GetHandle()
        {
            return handle;
        }
    }
}

