using System.Runtime.InteropServices;



namespace XplaneServices
{
    /// <summary>
    /// Contains structure for communicating with xplane rest plugin
    /// </summary>
    public static class XPlanePluginIcd
    {

        ///// <summary>
        ///// The type being passed
        ///// </summary>
        //public enum DataRefDataType : byte
        //{
        //    IntVal,
        //    FloatVal,
        //    DoubleVal,
        //    CharVal
        //}

        public enum DataRefDataType : byte
        {
            XplmTypeUnknown = 0x00,
            XplmTypeInt = 0x01,
            XplmTypeFloat = 0x02,
            XplmTypeDouble = 0x04,
            XplmTypeFloatArray = 0x08,
            XplmTypeIntArray = 0x10,
            XplmTypeData = 0x20
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
            public byte ValueCount;
            [MarshalAs(UnmanagedType.Bool)]
            public bool TreatAsArray;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 500)]
            public string TextValue;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public int[] IntValues;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public float[] FloatValues;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public double[] DoubleValues;
        }


    }
}
