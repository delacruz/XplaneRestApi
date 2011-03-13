using System.Runtime.InteropServices;

namespace XplaneServices
{
    /// <summary>
    /// Contains structure for communicating with xplane rest plugin
    /// </summary>
    public static class XPlanePluginIcd
    {

        /// <summary>
        /// The type being passed
        /// </summary>
        public enum DataRefDataType : byte
        {
            IntVal,
            FloatVal,
            DoubleVal,
            CharVal
        }

        /// <summary>
        /// The query type.
        /// </summary>
        public enum XplaneQueryType : byte
        {
            Read,
            Write,
            Response
        }

        /// <summary>
        /// The struct that gets written to and read from shared memory.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public struct DynamicQuery
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] 
            public string DataRef;
            public DataRefDataType DataType;
            public XplaneQueryType QueryType;
            public int IntValue;
            public float FloatValue;
            public double DoubleValue;
        }

    }
}
