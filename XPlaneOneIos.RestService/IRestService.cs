﻿using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace XplaneServices
{
    /// <summary>
    /// Specifies the rest service contract.
    /// </summary>
    [ServiceContract]
    public interface IRestService
    {
        /// <summary>
        /// Reads the int.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "readi?dataRef={dataRef}", ResponseFormat = WebMessageFormat.Json)]
        KeyValuePair<string, int> ReadInt(string dataRef);

        /// <summary>
        /// Reads the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "readf?dataRef={dataRef}", ResponseFormat = WebMessageFormat.Json)]
        KeyValuePair<string, float> ReadFloat(string dataRef);

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "readd?dataRef={dataRef}", ResponseFormat = WebMessageFormat.Json)]
        KeyValuePair<string, double> ReadDouble(string dataRef);

        /// <summary>
        /// Writes the int.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebGet(UriTemplate = "writei?newVal={newValue}&dataRef={dataRef}")]
        void WriteInt(string dataRef, int newValue);

        /// <summary>
        /// Writes the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebGet(UriTemplate = "writef?newVal={newValue}&dataRef={dataRef}")]
        void WriteFloat(string dataRef, float newValue);

        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebGet(UriTemplate = "writed?newVal={newValue}&dataRef={dataRef}")]
        void WriteDouble(string dataRef, double newValue);
    }
}