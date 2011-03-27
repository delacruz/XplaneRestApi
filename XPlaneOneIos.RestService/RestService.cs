using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using log4net;
using XplaneServices.SharedMemory;

namespace XplaneServices
{
    /// <summary>
    /// Implementation of the IRestService interface.
    /// </summary>
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single)]
    public class RestService : IRestService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RestService));
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private XPlanePluginIcd.DynamicQuery _response;
        readonly SharedMemoryStruct<XPlanePluginIcd.DynamicQuery> _sharedMemoryCommand = new SharedMemoryStruct<XPlanePluginIcd.DynamicQuery>("SHAREDMEM_COMMAND");
        readonly SharedMemoryStruct<XPlanePluginIcd.DynamicQuery> _sharedMemoryResponse = new SharedMemoryStruct<XPlanePluginIcd.DynamicQuery>("SHAREDMEM_RESPONSE");

        /// <summary>
        /// Initializes a new instance of the <see cref="RestService"/> class.
        /// </summary>
        public RestService()
        {
            _sharedMemoryResponse.DataReceived += SharedMemoryResponseDataReceived;
            Log.Info("Started Service.");
        }

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The instance containing the event data.</param>
        void SharedMemoryResponseDataReceived(object sender, EventArgs<XPlanePluginIcd.DynamicQuery> e)
        {
            _response = e.Value;
            _signal.Set();
        }


        private dynamic ReadData(string dataRef, XPlanePluginIcd.DataRefDataType dataRefDataType, int valueCount)
        {
            _sharedMemoryCommand.Write(new XPlanePluginIcd.DynamicQuery
            {
                DataRef = dataRef,
                DataType = dataRefDataType,
                QueryType = XPlanePluginIcd.XplaneQueryType.Read,
                ValueCount = (byte)valueCount,
            });

            var didRespond = _signal.WaitOne(2000);

            if (didRespond)
            {
                switch (dataRefDataType)
                {
                    case XPlanePluginIcd.DataRefDataType.XplmTypeInt:
                        return _response.IntValues[0];
                    case XPlanePluginIcd.DataRefDataType.XplmTypeIntArray:
                        return _response.IntValues.Take(valueCount).ToArray();
                    case XPlanePluginIcd.DataRefDataType.XplmTypeFloat:
                        return _response.FloatValues[0];
                    case XPlanePluginIcd.DataRefDataType.XplmTypeFloatArray:
                        return _response.FloatValues.Take(valueCount).ToArray();
                    case XPlanePluginIcd.DataRefDataType.XplmTypeDouble:
                        return _response.DoubleValues[0];
                    case XPlanePluginIcd.DataRefDataType.XplmTypeData:
                        return _response.TextValue;
                    default:
                        break;
                }

            }
            return new [] {-999};
        }

        /// <summary>
        /// Reads the int.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns>int value</returns>
        public int ReadInt(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.XplmTypeInt, 1)[0];
        }

        /// <summary>
        /// Reads the ints.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="valueCount">The value count.</param>
        /// <returns></returns>
        public int[] ReadInts(string dataRef, int valueCount)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.XplmTypeIntArray, valueCount);
        }

        /// <summary>
        /// Reads the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns>float value</returns>
        public float ReadFloat(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.XplmTypeFloat, 1)[0];
        }

        /// <summary>
        /// Reads the floats.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="valueCount">The value count.</param>
        /// <returns></returns>
        public float[] ReadFloats(string dataRef, int valueCount)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.XplmTypeFloatArray, valueCount);
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns>double value</returns>
        public double ReadDouble(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.XplmTypeDouble, 1);
        }

        /// <summary>
        /// Writes the int.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        public void WriteInt(string dataRef, int newValue)
        {
            WriteDataRef(dataRef, new[] { newValue }, XPlanePluginIcd.DataRefDataType.XplmTypeInt);
        }

        /// <summary>
        /// Writes the ints.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValues">The new values.</param>
        public void WriteInts(string dataRef, int[] newValues)
        {
            WriteDataRef(dataRef, newValues, XPlanePluginIcd.DataRefDataType.XplmTypeIntArray);
        }

        /// <summary>
        /// Writes the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        public void WriteFloat(string dataRef, float newValue)
        {
            WriteDataRef(dataRef, new[] { newValue }, XPlanePluginIcd.DataRefDataType.XplmTypeFloat);
        }

        /// <summary>
        /// Writes the floats.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValues">The new values.</param>
        public void WriteFloats(string dataRef, float[] newValues)
        {
            WriteDataRef(dataRef, newValues, XPlanePluginIcd.DataRefDataType.XplmTypeFloatArray);
        }

        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        public void WriteDouble(string dataRef, double newValue)
        {
            WriteDataRef(dataRef, new[] { newValue }, XPlanePluginIcd.DataRefDataType.XplmTypeDouble);
        }

        //TODO: ValueCount seem unncecessary, use newValue.Length instead?
        public void WriteDataRef(string dataRef, dynamic newValue, XPlanePluginIcd.DataRefDataType dataRefDataType)
        {
            var query = new XPlanePluginIcd.DynamicQuery
                            {
                                DataRef = dataRef,
                                DataType = dataRefDataType,
                                QueryType = XPlanePluginIcd.XplaneQueryType.Write,
                                IntValues = new int[256],
                                FloatValues = new float[256],
                                DoubleValues = new double[256]
                            };

            switch (dataRefDataType)
            {
                case XPlanePluginIcd.DataRefDataType.XplmTypeInt:
                    query.IntValues[0] = newValue;
                    query.ValueCount = 1;
                    break;
                case XPlanePluginIcd.DataRefDataType.XplmTypeIntArray:
                    newValue.CopyTo(query.IntValues, 0);
                    query.ValueCount = newValue.Length;
                    break;
                case XPlanePluginIcd.DataRefDataType.XplmTypeFloat:
                    query.FloatValues[0] = newValue;
                    query.ValueCount = 1;
                    break;
                case XPlanePluginIcd.DataRefDataType.XplmTypeFloatArray:
                    newValue.CopyTo(query.FloatValues, 0);
                    query.ValueCount = newValue.Length;
                    break;
                case XPlanePluginIcd.DataRefDataType.XplmTypeDouble:
                    query.DoubleValues[0] = newValue;
                    query.ValueCount = 1;
                    break;
                case XPlanePluginIcd.DataRefDataType.XplmTypeData:
                    query.TextValue = newValue;
                    query.ValueCount = newValue.Length;
                    break;
                default:
                    throw new NotSupportedException();
            }

            _sharedMemoryCommand.Write(query);
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="valueCount">The value count.</param>
        /// <returns></returns>
        public string ReadString(string dataRef, int valueCount)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.XplmTypeData, valueCount);
        }

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        public void WriteString(string dataRef, string newValue)
        {
            WriteDataRef(dataRef, newValue, XPlanePluginIcd.DataRefDataType.XplmTypeData);
        }
    }
}
