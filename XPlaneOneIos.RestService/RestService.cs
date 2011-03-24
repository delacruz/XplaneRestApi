using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using log4net;
using XplaneServices.SharedMemory;
using System.Linq;

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
            //var query = new XPlanePluginIcd.DynamicQuery
            //                {
            //                    DataRef = "sim/time/sim_speed",
            //                    DataType = XPlanePluginIcd.DataRefDataType.IntVal,
            //                    QueryType = XPlanePluginIcd.XplaneQueryType.Write,
            //                    //Values = new XPlanePluginIcd.DataRefValueUnion[255]
            //                    IntValues = new int[255],
            //                    FloatValues = new float[255],
            //                    DoubleValues = new double[255]
            //                };

            //query.IntValues[0] = 1;

            //_sharedMemoryCommand.Write(query);

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
                //values = new XPlanePluginIcd.DataRefValueUnion
                //             {
                //                 IntValue = new int[255],
                //                 //FloatValues = new float[255],
                //                 //DoubleValues = new double[255]
                //             }
            });

            var didRespond = _signal.WaitOne(2000);

            if (didRespond)
            {
                switch (dataRefDataType)
                {
                    case XPlanePluginIcd.DataRefDataType.IntVal:
                        return _response.IntValues;
                    case XPlanePluginIcd.DataRefDataType.FloatVal:
                        return _response.FloatValues;
                    case XPlanePluginIcd.DataRefDataType.DoubleVal:
                        return _response.DoubleValues;
                    default:
                        break;
                }
                return _response.IntValues[0];
            }
            else
            {
                return new [] {-999};
            }
        }

        /// <summary>
        /// Reads the int.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns>int value</returns>
        public int ReadInt(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.IntVal, 1);
        }

        public int[] ReadInts(string dataRef, int valueCount)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.IntVal, valueCount);
            //int[] valuesAsIntegers = Array.ConvertAll(unionValues, new Converter<XPlanePluginIcd.DataRefValueUnion, int>(DataRefValueUnionToInt));
            //return valuesAsIntegers.Take(valueCount).ToArray();
        }

        /// <summary>
        /// Reads the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns>float value</returns>
        public float ReadFloat(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.FloatVal, 1);
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns>double value</returns>
        public double ReadDouble(string dataRef)
        {
            return ReadData(dataRef, XPlanePluginIcd.DataRefDataType.DoubleVal, 1);
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
                return _response.IntValues;
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


            query.IntValues[0] = (int) newValue;
            query.FloatValues[0] = (float)newValue;
            query.DoubleValues[0] = (double)newValue;

            _sharedMemoryCommand.Write(query);
        }

        //public static int DataRefValueUnionToInt(XPlanePluginIcd.DataRefValueUnion union)
        //{
        //    return union.IntValue;
        //}
        //public static float DataRefValueUnionToFloat(XPlanePluginIcd.DataRefValueUnion union)
        //{
        //    return union.FloatValue;
        //}
        //public static double DataRefValueUnionToDouble(XPlanePluginIcd.DataRefValueUnion union)
        //{
        //    return union.DoubleValue;
        //}
    }

    

}


