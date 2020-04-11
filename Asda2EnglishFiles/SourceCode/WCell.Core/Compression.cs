using ICSharpCode.SharpZipLib.Zip.Compression;

namespace WCell.Core
{
    /// <summary>Wrapper for ICSharpCode.SharpZipLib.</summary>
    public static class Compression
    {
        /// <summary>Performs deflate compression on the given data.</summary>
        /// <param name="input">the data to compress</param>
        /// <param name="output">the compressed data</param>
        public static void CompressZLib(byte[] input, byte[] output, int compressionLevel, out int deflatedLength)
        {
            Deflater deflater = new Deflater(compressionLevel);
            deflater.SetInput(input, 0, input.Length);
            deflater.Finish();
            deflatedLength = deflater.Deflate(output, 0, output.Length);
        }

        /// <summary>Performs inflate decompression on the given data.</summary>
        /// <param name="input">the data to decompress</param>
        /// <param name="output">the decompressed data</param>
        public static void DecompressZLib(byte[] input, byte[] output)
        {
            Inflater inflater = new Inflater();
            inflater.SetInput(input, 0, input.Length);
            inflater.Inflate(output, 0, output.Length);
        }
    }
}