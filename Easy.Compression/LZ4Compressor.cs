namespace Easy.Compression
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using Easy.Compression.Common;
    using LZ4;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// An abstraction to easily compress and decompress using the <c>LZ4</c> and the <c>LZ4HC</c> algorithm.
    /// <para><see href="https://en.wikipedia.org/wiki/LZ4_(compression_algorithm)"/>.</para>
    /// </summary>
    public sealed class LZ4Compressor : ICompressor
    {
        private const int BufferSize = 1048576;
        private readonly ThreadLocal<byte[]> _readBuffer, _writeBuffer;
        private readonly ThreadLocalDisposable<MemoryStream> _streamIn, _streamOut;

        /// <summary>
        /// Create an instance of the <see cref="LZ4Compressor"/>.
        /// </summary>
        public LZ4Compressor()
        {
            _readBuffer = new ThreadLocal<byte[]>(() => new byte[BufferSize]);
            _writeBuffer = new ThreadLocal<byte[]>(() => new byte[BufferSize]);
            _streamIn = new ThreadLocalDisposable<MemoryStream>(() => new MemoryStream());
            _streamOut = new ThreadLocalDisposable<MemoryStream>(() => new MemoryStream());
        }

        /// <summary>
        /// Compresses the given <paramref name="streamIn"/> and copies the result to <paramref name="compressedOutput"/>.
        /// </summary>
        public void Compress(Stream streamIn, Stream compressedOutput, CompressionLevel level = CompressionLevel.Standard)
        {
            if (level == CompressionLevel.Standard)
            {
                EncodeStandard(streamIn, compressedOutput); 
            } else
            {
                EncodeAggresive(streamIn, compressedOutput);
            }
            compressedOutput.Flush();
        }

        /// <summary>
        /// Compresses the given <paramref name="bytes"/> with the <paramref name="level"/>.
        /// </summary>
        /// <returns>The compressed bytes.</returns>
        public byte[] Compress(byte[] bytes, CompressionLevel level = CompressionLevel.Standard)
        {
            if (level == CompressionLevel.Standard)
            {
                return LZ4Codec.Encode(bytes, 0, bytes.Length);
            }
            return LZ4Codec.EncodeHC(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Compresses the given <paramref name="input"/> with the <paramref name="level"/>. 
        /// </summary>
        /// <returns>The compressed bytes as <see cref="Encoding.UTF8"/>.</returns>
        public byte[] Compress(string input, CompressionLevel level = CompressionLevel.Standard)
        {
            return Compress(Encoding.UTF8.GetBytes(input), level);
        }

        /// <summary>
        /// Decompresses the given <paramref name="compressedInput"/> and copies the result to <paramref name="streamOut"/>.
        /// </summary>
        public void DeCompress(Stream compressedInput, Stream streamOut)
        {
            Decode(compressedInput, streamOut);
        }

        /// <summary>
        /// DeCompresses the given <paramref name="bytes"/>.
        /// </summary>
        /// <returns>The decompressed bytes.</returns>
        public byte[] DeCompress(byte[] bytes)
        {
            return Decode(bytes);
        }

        /// <summary>
        /// DeCompresses the given <paramref name="bytes"/> as <see cref="string"/>.
        /// </summary>
        /// <returns>The decompressed string.</returns>
        public string DeCompressAsString(byte[] bytes)
        {
            return DeCompressAsString(bytes, Encoding.UTF8);
        }

        /// <summary>
        /// DeCompresses the given <paramref name="bytes"/> as <see cref="string"/>.
        /// </summary>
        /// <returns>The decompressed string.</returns>
        public string DeCompressAsString(byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(Decode(bytes));
        }

        /// <summary>
        /// Releases all the resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            _readBuffer.Dispose();
            _writeBuffer.Dispose();
            _streamIn.Dispose();
            _streamOut.Dispose();
        }

    #region Internals
        internal byte[] Wrap(string input, CompressionLevel level = CompressionLevel.Standard)
        {
            return Wrap(input, Encoding.UTF8, level);
        }

        internal byte[] Wrap(string input, Encoding encoding, CompressionLevel level = CompressionLevel.Standard)
        {
            return Wrap(encoding.GetBytes(input), level);
        }

        internal byte[] Wrap(byte[] bytes, CompressionLevel level = CompressionLevel.Standard)
        {
            switch (level)
            {
                case CompressionLevel.Standard:
                    return LZ4Codec.Wrap(bytes, 0, bytes.Length);
                case CompressionLevel.Aggressive:
                    return LZ4Codec.WrapHC(bytes, 0, bytes.Length);
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        internal byte[] UnWrap(byte[] bytes)
        {
            return LZ4Codec.Unwrap(bytes);
        }

        internal string UnWrapAsString(byte[] bytes)
        {
            return UnWrapAsString(bytes, Encoding.UTF8);
        }

        internal string UnWrapAsString(byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(UnWrap(bytes));
        }
        #endregion

        private void EncodeStandard(Stream streamIn, Stream streamOut)
        {
            var readBuffer = _readBuffer.Value;
            var writeBuffer = _writeBuffer.Value;

            int bytesRead;
            while ((bytesRead = streamIn.Read(readBuffer, 0, readBuffer.Length)) != 0)
            {
                var bytesCompressed = LZ4Codec.Encode(readBuffer, 0, bytesRead, writeBuffer, 0, writeBuffer.Length);
                streamOut.Write(writeBuffer, 0, bytesCompressed);
                Array.Clear(readBuffer, 0, readBuffer.Length);
                Array.Clear(writeBuffer, 0, writeBuffer.Length);
            }
        }

        private void EncodeAggresive(Stream streamIn, Stream streamOut)
        {
            var readBuffer = _readBuffer.Value;
            var writeBuffer = _writeBuffer.Value;

            int bytesRead;
            while ((bytesRead = streamIn.Read(readBuffer, 0, readBuffer.Length)) != 0)
            {
                var bytesCompressed = LZ4Codec.EncodeHC(readBuffer, 0, bytesRead, writeBuffer, 0, writeBuffer.Length);
                streamOut.Write(writeBuffer, 0, bytesCompressed);
                Array.Clear(readBuffer, 0, readBuffer.Length);
                Array.Clear(writeBuffer, 0, writeBuffer.Length);
            }
        }

        private void Decode(Stream streamIn, Stream streamOut)
        {
            var readBuffer = _readBuffer.Value;
            var writeBuffer = _writeBuffer.Value;

            int bytesRead;
            while ((bytesRead = streamIn.Read(readBuffer, 0, readBuffer.Length)) != 0)
            {
                var bytesCompressed = LZ4Codec.Decode(readBuffer, 0, bytesRead, writeBuffer, 0, writeBuffer.Length);
                streamOut.Write(writeBuffer, 0, bytesCompressed);
                Array.Clear(readBuffer, 0, readBuffer.Length);
                Array.Clear(writeBuffer, 0, writeBuffer.Length);
            }
        }

        private byte[] Decode(byte[] compressedBytes)
        {
            var writeBuffer = _writeBuffer.Value;
            var bytesCompressed = LZ4Codec.Decode(compressedBytes, 0, compressedBytes.Length, writeBuffer, 0, writeBuffer.Length);

            return writeBuffer.SubArray(0, bytesCompressed);
        }
    }
}
