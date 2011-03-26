using System.Collections.Generic;
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
        [WebGet(UriTemplate = "i?dataRef={dataRef}", ResponseFormat = WebMessageFormat.Json)]
        int ReadInt(string dataRef);

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="valueCount">The value count.</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "s?dataRef={dataRef}&valueCount={valueCount}", ResponseFormat = WebMessageFormat.Json)]
        string ReadString(string dataRef, int valueCount);

        /// <summary>
        /// Reads the ints.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="valueCount">The value count.</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "vi?dataRef={dataRef}&valueCount={valueCount}", ResponseFormat = WebMessageFormat.Json)]
        int[] ReadInts(string dataRef, int valueCount);

        /// <summary>
        /// Reads the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "f?dataRef={dataRef}", ResponseFormat = WebMessageFormat.Json)]
        float ReadFloat(string dataRef);

        /// <summary>
        /// Reads the floats.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="valueCount">The value count.</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "vf?dataRef={dataRef}&valueCount={valueCount}", ResponseFormat = WebMessageFormat.Json)]
        float[] ReadFloats(string dataRef, int valueCount);

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "d?dataRef={dataRef}", ResponseFormat = WebMessageFormat.Json)]
        double ReadDouble(string dataRef);

        /// <summary>
        /// Writes the int.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "i?dataRef={dataRef}", 
            Method = "PUT", 
            RequestFormat = WebMessageFormat.Json, 
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        void WriteInt(string dataRef, int newValue);

        /// <summary>
        /// Writes the ints.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "vi?dataRef={dataRef}",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        void WriteInts(string dataRef, int[] newValue);

        /// <summary>
        /// Writes the float.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "f?dataRef={dataRef}",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        void WriteFloat(string dataRef, float newValue);

        /// <summary>
        /// Writes the floats.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "vf?dataRef={dataRef}",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        void WriteFloats(string dataRef, float[] newValue);

        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "d?dataRef={dataRef}",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        void WriteDouble(string dataRef, double newValue);

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="dataRef">The data ref.</param>
        /// <param name="newValue">The new value.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "s?dataRef={dataRef}",
            Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        void WriteString(string dataRef, string newValue);
    }
}
