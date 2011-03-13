using System;
using System.Runtime.InteropServices;

namespace XplaneServices.SharedMemory
{

    /// <summary>
    /// Provides a wrapper for the SharedMemory object to be used with data structures.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SharedMemoryStruct<T> : IDisposable where T : struct
    {
        #region Class Variables

        private readonly global::XplaneServices.SharedMemory.SharedMemory _sharedMemory;
        private readonly object _dataReceivedEventLock = new object();
        private EventHandler<EventArgs<T>> _dataReceivedEvent;
        private bool _disposed;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedMemoryStruct&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="sharedMemoryName">Name of the dataStruct memory.</param>
        public SharedMemoryStruct(string sharedMemoryName)
        {
            _sharedMemory = new global::XplaneServices.SharedMemory.SharedMemory(sharedMemoryName, (uint)Marshal.SizeOf(typeof(T)));
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Writes the specified data struct.
        /// </summary>
        /// <param name="dataStruct">The data struct.</param>
        public void Write(T dataStruct)
        {
            var byteArray = ConvertStructToByteArray(dataStruct);
            _sharedMemory.Write(byteArray);
        }

        /// <summary>
        /// Gets the current shared memory.
        /// </summary>
        /// <returns></returns>
        public T GetCurrentSharedMemory()
        {
            return (T)ConvertByteArrayToStruct(_sharedMemory.GetCurrentSharedMemory(), typeof(T));
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                   _sharedMemory.Close();
                }
            }
            _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SharedMemoryStruct&lt;T&gt;"/> is reclaimed by garbage collection.
        /// </summary>
        ~SharedMemoryStruct()
        {
            Dispose(false);
        }

        #endregion

        #region Data Event Handling

        /// <summary>
        /// Event raised after receiving new data.
        /// </summary>
        public event EventHandler<EventArgs<T>> DataReceived
        {
            add
            {
                lock (_dataReceivedEventLock)
                {
                    // If first subscriber, subscribe (once) to the underlying dataStruct memory object 
                    if (_dataReceivedEvent == null) 
                        _sharedMemory.DataReceived += SharedMemoryDataReceived;

                    _dataReceivedEvent += value;
                }
            }
            remove
            {
                lock (_dataReceivedEventLock)
                {
                    _dataReceivedEvent -= value;

                    // if no more subsribers to this event, remove this class' subscription to the dataStruct memory class event
                    if (_dataReceivedEvent == null)
                        _sharedMemory.DataReceived -= SharedMemoryDataReceived;
                }
            }
        }

        /// <summary>
        /// Handles the DataReceived Event
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        void SharedMemoryDataReceived(byte[] dataItem)
        {
            T value = (T)ConvertByteArrayToStruct(dataItem, typeof(T));
            OnDataReceived(new EventArgs<T>(value));
        }

        /// <summary>
        /// Raises the <see cref="DataReceived" /> event.
        /// </summary>
        /// <param name="e"><see cref="EventArgs<T>" /> object that provides the arguments for the event.</param>
        protected virtual void OnDataReceived(EventArgs<T> e)
        {
            EventHandler<EventArgs<T>> handler = null;

            lock (_dataReceivedEventLock)
            {
                handler = _dataReceivedEvent;

                if (handler == null)
                    return;
            }

            handler(this, e);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Converts the byte array to struct.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object ConvertByteArrayToStruct(byte[] data, Type type)
        {
            var size = Marshal.SizeOf(type);
            if (size > data.Length)
            {
                return null;
            }
            var buffer = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, buffer, size);
            var returnVal = Marshal.PtrToStructure(buffer, type);
            Marshal.FreeHGlobal(buffer);
            return returnVal;
        }

        /// <summary>
        /// Converts the struct to byte array.
        /// </summary>
        /// <param name="theStruct">The struct.</param>
        /// <returns></returns>
        public static byte[] ConvertStructToByteArray(object theStruct)
        {
            var data = new byte[Marshal.SizeOf(theStruct)];
            var p = Marshal.AllocHGlobal(Marshal.SizeOf(theStruct));
            Marshal.StructureToPtr(theStruct, p, false);
            Marshal.Copy(p, data, 0, data.Length);
            Marshal.FreeHGlobal(p);
            return data;
        }

        #endregion
    }

}

