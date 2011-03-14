using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using log4net;
using XplaneServices.SharedMemory;

namespace XplaneServices
{
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

        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="dataRefDataType">Type of the data ref data.</param>
        /// <returns></returns>
        private dynamic ReadData(string dataRef, XPlanePluginIcd.DataRefDataType dataRefDataType)
        {
            _sharedMemoryCommand.Write(new XPlanePluginIcd.DynamicQuery
            {
                DataRef = dataRef,
                DataType = dataRefDataType,
                QueryType = XPlanePluginIcd.XplaneQueryType.Read
            });

            var didRespond = _signal.WaitOne(2000);

            if (didRespond)
            {
                switch (dataRefDataType)
                {
                    case XPlanePluginIcd.DataRefDataType.IntVal:
                        return _response.IntValue;
                    case XPlanePluginIcd.DataRefDataType.FloatVal:
                        return _response.FloatValue;
                    case XPlanePluginIcd.DataRefDataType.DoubleVal:
                        return _response.DoubleValue;
                    default:
                        break;
                }
                return _response.IntValue;
            }
            else
            {
                return -999;
            }
        }

        /// <summary>
        /// Reads the int.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        public KeyValuePair<string, int> ReadInt(string dataRef)
        {
            var value = ReadData(dataRef, XPlanePluginIcd.DataRefDataType.IntVal);
            return new KeyValuePair<string, int>(dataRef, value);
        }

        /// <summary>
        /// Reads the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        public KeyValuePair<string, float> ReadFloat(string dataRef)
        {
            var value = ReadData(dataRef, XPlanePluginIcd.DataRefDataType.FloatVal);
            return new KeyValuePair<string, float>(dataRef, value);
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        public KeyValuePair<string, double> ReadDouble(string dataRef)
        {
            var value = ReadData(dataRef, XPlanePluginIcd.DataRefDataType.DoubleVal);
            return new KeyValuePair<string, double>(dataRef, value);
        }

        /// <summary>
        /// Writes the int.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        public void WriteInt(string dataRef, int newValue)
        {
            WriteDataRef(dataRef, newValue, XPlanePluginIcd.DataRefDataType.IntVal);
        }

        /// <summary>
        /// Writes the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        public void WriteFloat(string dataRef, float newValue)
        {
            WriteDataRef(dataRef, newValue, XPlanePluginIcd.DataRefDataType.FloatVal);
        }

        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        public void WriteDouble(string dataRef, double newValue)
        {
            WriteDataRef(dataRef, newValue, XPlanePluginIcd.DataRefDataType.DoubleVal);
        }

        /// <summary>
        /// Reads the data ref.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        public dynamic ReadDataRef(string dataRef)
        {
            _sharedMemoryCommand.Write(new XPlanePluginIcd.DynamicQuery
            {
                DataRef = dataRef,
                DataType = XPlanePluginIcd.DataRefDataType.IntVal,
                QueryType = XPlanePluginIcd.XplaneQueryType.Read
            });

            var didRespond = _signal.WaitOne(2000);

            if (didRespond)
            {
                return _response.IntValue;
            }
            else
            {
                return -999;
            }
        }

        /// <summary>
        /// Writes the data ref.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="dataRefDataType">Type of the data ref data.</param>
        public void WriteDataRef(string dataRef, dynamic newValue, XPlanePluginIcd.DataRefDataType dataRefDataType)
        {
            var query = new XPlanePluginIcd.DynamicQuery
            {
                DataRef = dataRef,
                DataType = dataRefDataType,
                QueryType = XPlanePluginIcd.XplaneQueryType.Write,
                IntValue = newValue,
                FloatValue = newValue,
                DoubleValue = newValue
            };
            _sharedMemoryCommand.Write(query);
        }

   
    }
}
