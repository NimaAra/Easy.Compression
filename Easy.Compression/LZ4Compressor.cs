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
        private readonly ThreadLocal<byte[]> _copyBuffer;
        private readonly ThreadLocalDisposable<MemoryStream> _streamIn, _streamOut;
        private readonly ThreadLocalDisposable<LZ4Stream> _compressorStandard, _compressorAggressive, _deCompressor;

        /// <summary>
        /// Create an instance of the <see cref="LZ4Compressor"/>.
        /// </summary>
        public LZ4Compressor()
        {
            _copyBuffer = new ThreadLocal<byte[]>(() => new byte[4096]);
            _streamIn = new ThreadLocalDisposable<MemoryStream>(() => new MemoryStream());
            _streamOut = new ThreadLocalDisposable<MemoryStream>(() => new MemoryStream());

            _compressorStandard = new ThreadLocalDisposable<LZ4Stream>(
                () => new LZ4Stream(_streamOut.Value, LZ4StreamMode.Compress, LZ4StreamFlags.InteractiveRead | LZ4StreamFlags.IsolateInnerStream));

            _compressorAggressive = new ThreadLocalDisposable<LZ4Stream>(
                () => new LZ4Stream(_streamOut.Value, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression | LZ4StreamFlags.InteractiveRead | LZ4StreamFlags.IsolateInnerStream));

            _deCompressor = new ThreadLocalDisposable<LZ4Stream>(
                () => new LZ4Stream(_streamIn.Value, LZ4StreamMode.Decompress, LZ4StreamFlags.InteractiveRead | LZ4StreamFlags.IsolateInnerStream));
        }

        /// <summary>
        /// Compresses the given <paramref name="streamIn"/> and copies the result to <paramref name="compressedOutput"/>.
        /// </summary>
        public void Compress(Stream streamIn, Stream compressedOutput, CompressionLevel level = CompressionLevel.Standard)
        {
            var buffer = _copyBuffer.Value;
            var flags = LZ4StreamFlags.InteractiveRead | LZ4StreamFlags.IsolateInnerStream;
            if (level == CompressionLevel.Aggressive) { flags = flags | LZ4StreamFlags.HighCompression; }

            using (var compressor = new LZ4Stream(compressedOutput, LZ4StreamMode.Compress, flags))
            {
                CopyTo(streamIn, buffer, compressor);
            }
        }

        /// <summary>
        /// Compresses the given <paramref name="bytes"/> with the <paramref name="level"/>.
        /// </summary>
        /// <returns>The compressed bytes.</returns>
        public byte[] Compress(byte[] bytes, CompressionLevel level = CompressionLevel.Standard)
        {
            var buffer = _copyBuffer.Value;
            var source = _streamIn.Value;
            var destination = _streamOut.Value;
            var compressor = level == CompressionLevel.Standard ? _compressorStandard.Value : _compressorAggressive.Value;

            WriteAndReset(source, bytes, destination);
            CopyTo(source, buffer, compressor);
            compressor.Flush();
            return destination.ToArray();
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
            var buffer = _copyBuffer.Value;
            using (var deCompressor = new LZ4Stream(compressedInput, LZ4StreamMode.Decompress, LZ4StreamFlags.InteractiveRead | LZ4StreamFlags.IsolateInnerStream))
            {
                CopyTo(deCompressor, buffer, streamOut);
            }
        }

        /// <summary>
        /// DeCompresses the given <paramref name="bytes"/>.
        /// </summary>
        /// <returns>The decompressed bytes.</returns>
        public byte[] DeCompress(byte[] bytes)
        {
            var buffer = _copyBuffer.Value;
            var source = _streamIn.Value;
            var destination = _streamOut.Value;
            var deCompressor = _deCompressor.Value;

            WriteAndReset(source, bytes, destination);
            CopyTo(deCompressor, buffer, destination);
            return destination.ToArray();
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
            return encoding.GetString(DeCompress(bytes));
        }

        /// <summary>
        /// Releases all the resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            _copyBuffer.Dispose();
            _streamIn.Dispose();
            _streamOut.Dispose();
            _compressorStandard.Dispose();
            _compressorAggressive.Dispose();
            _deCompressor.Dispose();
        }

    #region Internals
        internal byte[] Encode(string input, CompressionLevel level = CompressionLevel.Standard)
        {
            return Encode(input, Encoding.UTF8, level);
        }

        internal byte[] Encode(string input, Encoding encoding, CompressionLevel level = CompressionLevel.Standard)
        {
            return Encode(encoding.GetBytes(input), level);
        }

        internal byte[] Encode(byte[] bytes, CompressionLevel level = CompressionLevel.Standard)
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

        internal byte[] DeCode(byte[] bytes)
        {
            return LZ4Codec.Unwrap(bytes);
        }

        internal string DeCodeAsString(byte[] bytes)
        {
            return DeCodeAsString(bytes, Encoding.UTF8);
        }

        internal string DeCodeAsString(byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(DeCode(bytes));
        }
    #endregion

        private static void WriteAndReset(Stream source, byte[] bytes, Stream destination)
        {
            source.SetLength(0);
            source.Write(bytes, 0, bytes.Length);
            source.Position = 0;
            destination.SetLength(0);
        }

        private static void CopyTo(Stream source, byte[] buffer, Stream destination)
        {
            Array.Clear(buffer, 0, buffer.Length);

            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, count);
            }
        }
    }
}
