namespace Easy.Compression
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Specifies the contract that an instance of <see cref="ICompressor"/> must implement.
    /// </summary>
    public interface ICompressor : IDisposable
    {
        /// <summary>
        /// Compresses the given <paramref name="streamIn"/> and copies the result to <paramref name="compressedOutput"/>.
        /// </summary>
        void Compress(Stream streamIn, Stream compressedOutput, CompressionLevel level = CompressionLevel.Standard);

        /// <summary>
        /// Compresses the given <paramref name="bytes"/> with the <paramref name="level"/>.
        /// </summary>
        /// <returns>The compressed bytes.</returns>
        byte[] Compress(byte[] bytes, CompressionLevel level = CompressionLevel.Standard);

        /// <summary>
        /// Compresses the given <paramref name="input"/> with the <paramref name="level"/>.
        /// </summary>
        /// <returns>The compressed bytes as <see cref="Encoding.UTF8"/>.</returns>
        byte[] Compress(string input, CompressionLevel level = CompressionLevel.Standard);

        /// <summary>
        /// Decompresses the given <paramref name="compressedInput"/> and copies the result to <paramref name="streamOut"/>.
        /// </summary>
        void DeCompress(Stream compressedInput, Stream streamOut);

        /// <summary>
        /// DeCompresses the given <paramref name="bytes"/>.
        /// </summary>
        /// <returns>The decompressed bytes.</returns>
        byte[] DeCompress(byte[] bytes);

        /// <summary>
        /// DeCompresses the given <paramref name="bytes"/> as <see cref="string"/>.
        /// </summary>
        /// <returns>The decompressed string.</returns>
        string DeCompressAsString(byte[] bytes);

        /// <summary>
        /// DeCompresses the given <paramref name="bytes"/> as <see cref="string"/>.
        /// </summary>
        /// <returns>The decompressed string.</returns>
        string DeCompressAsString(byte[] bytes, Encoding encoding);
    }
}