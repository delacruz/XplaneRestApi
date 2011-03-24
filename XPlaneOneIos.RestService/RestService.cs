using System.Collections.Generic;
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
            var query = new XPlanePluginIcd.DynamicQuery
                            {
                                DataRef = "sim/time/sim_speed",
                                DataType = XPlanePluginIcd.DataRefDataType.IntVal,
                                QueryType = XPlanePluginIcd.XplaneQueryType.Write,
                                values = new XPlanePluginIcd.DataRefValueUnion
                                             {
                                                 IntValues = new int[255],
                                                 FloatValues = new float[255],
                                                 DoubleValue = new double[1]
                                             }
                            };

            query.values.IntValues[0] = 1;

            _sharedMemoryCommand.Write(query);
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
        /// <returns>The dynamically typed dataref value.</returns>
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
                        return _response.values.IntValues;
                    case XPlanePluginIcd.DataRefDataType.FloatVal:
                        return _response.values.FloatValues;
                    case XPlanePluginIcd.DataRefDataType.DoubleVal:
                        return _response.values.DoubleValue;
                    default:
                        break;
                }
                return _response.values.IntValues;
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
        /// <returns>int value</returns>
        public int ReadInt(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.IntVal);
        }

        /// <summary>
        /// Reads the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns>float value</returns>
        public float ReadFloat(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.FloatVal);
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns>double value</returns>
        public double ReadDouble(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.DoubleVal);
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
                return _response.values.IntValues;
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
                                //values = new XPlanePluginIcd.DataRefValueUnion
                                //             {
                                //                 IntValues = new int[255],
                                //                FloatValues = new float[255]
                                //             },
                                DataRef = dataRef,
                                DataType = dataRefDataType,
                                QueryType = XPlanePluginIcd.XplaneQueryType.Write
                            };


            query.values.IntValues[0] = (int) newValue;
            query.values.FloatValues[0] = (float)newValue;
            query.values.DoubleValue[0] = (double)newValue;

            _sharedMemoryCommand.Write(query);
        }

   
    }
}
