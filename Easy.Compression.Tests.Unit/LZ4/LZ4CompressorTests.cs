namespace Easy.Compression.Tests.Unit.LZ4
{
    using System.IO;
    using System.Text;
    using NUnit.Framework;
    using Shouldly;

    // ReSharper disable once InconsistentNaming
    [TestFixture]
    internal class LZ4CompressorTests
    {
        private const string InputStr = "Did you know now awesome this library really is? I guess not well see for yourself.";
        private readonly byte[] _inputBytes = Encoding.Default.GetBytes(InputStr);

        [Test]
        public void When_compressing_bytes()
        {
            using (ICompressor compressor = new LZ4Compressor())
            {
                var compressedBytesStd = compressor.Compress(_inputBytes);
                var compressedBytesAggr = compressor.Compress(_inputBytes, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldBe(compressedBytesAggr);

                compressor.DeCompress(compressedBytesStd).ShouldBe(_inputBytes);
                compressor.DeCompress(compressedBytesAggr).ShouldBe(_inputBytes);
            }
        }

        [Test]
        public void When_compressing_a_stream_standard()
        {
            var inputBytes = Encoding.UTF8.GetBytes(InputStr);

            using (var streamIn = new MemoryStream(inputBytes))
            using (ICompressor compressor = new LZ4Compressor())
            using (var compressedStream = new MemoryStream())
            {
                compressedStream.Length.ShouldBe(0);
                compressor.Compress(streamIn, compressedStream);
                
                var compressedBytes = compressedStream.ToArray();
                compressedBytes.Length.ShouldBe(84);

                compressedStream.Position = 0;
                using (var deCompressedStream = new MemoryStream())
                {
                    compressor.DeCompress(compressedStream, deCompressedStream);

                    var deCompressedBytes = deCompressedStream.ToArray();
                    deCompressedBytes.ShouldBe(_inputBytes);
                }

                compressor.DeCompressAsString(compressedBytes, Encoding.UTF8).ShouldBe(InputStr);
            }
        }

        [Test]
        public void When_compressing_a_stream_aggresive()
        {
            var inputBytes = Encoding.UTF8.GetBytes(InputStr);

            using (var streamIn = new MemoryStream(inputBytes))
            using (ICompressor compressor = new LZ4Compressor())
            using (var compressedStream = new MemoryStream())
            {
                compressedStream.Length.ShouldBe(0);
                compressor.Compress(streamIn, compressedStream, CompressionLevel.Aggressive);

                var compressedBytes = compressedStream.ToArray();
                compressedBytes.Length.ShouldBe(84);

                compressedStream.Position = 0;
                using (var deCompressedStream = new MemoryStream())
                {
                    compressor.DeCompress(compressedStream, deCompressedStream);

                    var deCompressedBytes = deCompressedStream.ToArray();
                    deCompressedBytes.ShouldBe(_inputBytes);
                }

                compressor.DeCompressAsString(compressedBytes, Encoding.UTF8).ShouldBe(InputStr);
            }
        }

        [Test]
        public void When_compressing_string_with_default_method()
        {
            using (ICompressor compressor = new LZ4Compressor())
            {
                var compressedBytesStd = compressor.Compress(InputStr);
                var compressedBytesAggr = compressor.Compress(InputStr, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldBe(compressedBytesAggr);

                compressor.DeCompressAsString(compressedBytesStd).ShouldBe(InputStr);
                compressor.DeCompressAsString(compressedBytesAggr).ShouldBe(InputStr);

                compressor.DeCompress(compressedBytesStd).ShouldBe(_inputBytes);
                compressor.DeCompress(compressedBytesAggr).ShouldBe(_inputBytes);
            }
        }

        [Test]
        public void When_compressing_string_with_encoding()
        {
            using (ICompressor compressor = new LZ4Compressor())
            {
                var inputBytes = Encoding.UTF32.GetBytes(InputStr);

                var compressedBytesStd = compressor.Compress(inputBytes);
                var compressedBytesAggr = compressor.Compress(inputBytes, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldNotBe(compressedBytesAggr);

                compressor.DeCompressAsString(compressedBytesStd, Encoding.UTF32).ShouldBe(InputStr);
                compressor.DeCompressAsString(compressedBytesAggr, Encoding.UTF32).ShouldBe(InputStr);

                compressor.DeCompress(compressedBytesStd).ShouldBe(inputBytes);
                compressor.DeCompress(compressedBytesAggr).ShouldBe(inputBytes);
            }
        }
    }
}
