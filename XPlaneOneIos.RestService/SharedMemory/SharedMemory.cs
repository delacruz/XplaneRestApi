using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace XplaneServices.SharedMemory
{
    class SharedMemory : IDisposable
    {
        #region DLL Imports

        [DllImport("kernel32.dll")]
        internal static extern int VirtualQuery(ref IntPtr lpAddress,
                            ref MemoryBasicInformation lpBuffer,
                            int dwLength);

        [DllImport ("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateFileMapping (IntPtr hFile,
                            int lpAttributes,
                            FileMapProtection flProtect,
                            uint dwMaximumSizeHigh,
                            uint dwMaximumSizeLow,
                            string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenFileMapping(uint dwDesiredAccess, 
                            bool bInheritHandle, string lpName);


        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject,
                            FileMapAccess dwDesiredAccess,
                            uint dwFileOffsetHigh,
                            uint dwFileOffsetLow,
                            uint dwNumberOfBytesToMap);


        #endregion

        #region SM Enums

        [Flags]
        public enum FileMapAccess : uint
        {
            FileMapCopy = 0x0001,
            FileMapWrite = 0x0002,
            FileMapRead = 0x0004,
            FileMapAllAccess = 0x001f,
            FileMapExecute = 0x0020,
        }

        internal enum FileMapProtection : uint
        {
            PageReadonly = 0x02,
            PageReadWrite = 0x04,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MemoryBasicInformation
        {
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
            public uint AllocationProtect;
            public uint RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        #endregion SM Enums

        #region Class Variables

        private SharedMemoryHandle _sharedMemoryHandle;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly EventWaitHandle _ewhNewData;
        private static readonly ILog Log = LogManager.GetLogger(typeof(SharedMemory));
        private Task _listenForDataTask;

        #endregion

        #region Properties

        public string SharedMemoryName { get; protected set; }
        public uint Size { get; private set; }
        public SharedMemoryStream ShareMemoryStream { get; private set; }

        #endregion

        #region Methods

        private void CreateOrOpen()
        {
            var hHandle = CreateFileMapping(new IntPtr(-1), 0, FileMapProtection.PageReadWrite, 0, Size, SharedMemoryName);
            _sharedMemoryHandle = new SharedMemoryHandle(hHandle);
            if (_sharedMemoryHandle.IsInvalid)
            {
                Log.Error("Error");
                throw new InvalidOperationException("Error creating object");
            }
        }

        public SharedMemory(string sharedMemoryName, uint numberOfBytes)
        {
            Size = numberOfBytes;
            SharedMemoryName = sharedMemoryName.Trim();
            CreateOrOpen();
            MapView();

            _ewhNewData = new EventWaitHandle(false, EventResetMode.ManualReset, SharedMemoryName + "_EVENT");
            

        }

        private void MapView()
        {
            var ptr = MapViewOfFile(_sharedMemoryHandle.GetHandle(),
                                    FileMapAccess.FileMapRead | FileMapAccess.FileMapWrite,
                                    0,
                                    0,
                                    Size);
            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("File map not valid!");
            }
            if (Size == 0)
            {
                var info = new MemoryBasicInformation();

                VirtualQuery(ref ptr, ref info, Marshal.SizeOf(info));
                Size = info.RegionSize;
            }

            ShareMemoryStream = new SharedMemoryStream(ptr, Size);
        }

        public void Write(byte[] buffer)
        {
            try
            { 
                ShareMemoryStream.Position = 0;
                ShareMemoryStream.Write(buffer, 0, buffer.Length);
                _ewhNewData.Set();
            }
            catch (Exception)
            {
                Log.Error("Failed to write to shared memory");
            }
        }

        public void Close()
        {
            _cancellationTokenSource.Cancel();
            _listenForDataTask.Wait();
            Dispose();
        }

        public byte[] GetCurrentSharedMemory()
        {
            var buffer = new byte[Size];
            try
            {
                ShareMemoryStream.Position = 0;
                ShareMemoryStream.Read(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                Log.Error("Could not get shared memory, may already be closed.");
            }
            return buffer;
        }

        private void ListenForData(CancellationToken cancellationToken)
        {
            Log.InfoFormat("Listening for shared memory data on file map {0}.", SharedMemoryName);

            // Run until token request cancellation););
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait up to a second for new data in shared memory
                var isNewDataAvailable = _ewhNewData.WaitOne(1000);

                // No new data available? Start over unless token requests cancellation
                if (!isNewDataAvailable) continue;

                // New data available, raise event and reset event wait handle
                OnDataReceived(GetCurrentSharedMemory());
                _ewhNewData.Reset();
            }

            Log.InfoFormat("Listener task for {0} was cancelled.", SharedMemoryName);
        }

        #endregion

        #region IDisposable Members

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ShareMemoryStream.Close();
                    _sharedMemoryHandle.Close();
                }
            }
            _disposed = true;
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SharedMemory()
        {
            Dispose(false);
        }

        #endregion

        #region Data Event Handling 

        public delegate void DataHandler<T>(T dataItem);

        private DataHandler<byte[]> _dataReceived;
        public event DataHandler<byte[]> DataReceived
        {
            add
            {
                if(_dataReceived == null)
                {
                    _listenForDataTask = new Task(() => ListenForData(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                    _listenForDataTask.Start();
                }
                _dataReceived += value;
            }
            remove 
            { 
                _dataReceived -= value;
                if(_dataReceived == null)
                {
                    _cancellationTokenSource.Cancel();
                }
            }
        }

        private void OnDataReceived(byte[] bytes)
        {
            if(_dataReceived != null)
            {
                _dataReceived(bytes);
            }
        }

        #endregion
    }
}
