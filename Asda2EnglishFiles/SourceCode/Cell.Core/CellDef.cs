namespace Cell.Core
{
    /// <summary>Global constants for the Cell framework.</summary>
    public static class CellDef
    {
        /// <summary>File name for the Cell framework error file.</summary>
        public const string CORE_LOG_FNAME = "CellCore";

        /// <summary>Internal version string.</summary>
        public const string SVER = "Cell v1.0 ALPHA";

        /// <summary>Internal version number.</summary>
        public const float VER = 1f;

        /// <summary>Maximum size of a packet buffer segment</summary>
        public const int MAX_PBUF_SEGMENT_SIZE = 8192;

        /// <summary>Maximum size of a packet buffer segment.</summary>
        public const int PBUF_SEGMENT_COUNT = 512;
    }
}