namespace Easy.Compression.Tests.Unit.LZ4
{
    using System.Text;
    using NUnit.Framework;
    using Shouldly;

    // ReSharper disable once InconsistentNaming
    [TestFixture]
    internal class LZ4CompressorInternalTests
    {
        private const string InputStr = "Did you know now awesome this library really is? I guess not well see for yourself.";
        private readonly byte[] _inputBytes = Encoding.Default.GetBytes(InputStr);

        [Test]
        public void When_decoding_bytes()
        {
            using (var compressor = new LZ4Compressor())
            {
                var compressedBytesStd = compressor.Wrap(_inputBytes);
                var compressedBytesAggr = compressor.Wrap(_inputBytes, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldBe(compressedBytesAggr);

                compressor.UnWrap(compressedBytesStd).ShouldBe(_inputBytes);
                compressor.UnWrap(compressedBytesAggr).ShouldBe(_inputBytes);
            }
        }

        [Test]
        public void When_compressing_string_with_default_method()
        {
            using (var compressor = new LZ4Compressor())
            {
                var compressedBytesStd = compressor.Wrap(InputStr);
                var compressedBytesAggr = compressor.Wrap(InputStr, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldBe(compressedBytesAggr);

                compressor.UnWrapAsString(compressedBytesStd).ShouldBe(InputStr);
                compressor.UnWrapAsString(compressedBytesAggr).ShouldBe(InputStr);

                compressor.UnWrap(compressedBytesStd).ShouldBe(_inputBytes);
                compressor.UnWrap(compressedBytesAggr).ShouldBe(_inputBytes);
            }
        }

        [Test]
        public void When_compressing_string_with_encoding()
        {
            using (var compressor = new LZ4Compressor())
            {
                var inputBytes = Encoding.UTF32.GetBytes(InputStr);

                var compressedBytesStd = compressor.Wrap(inputBytes);
                var compressedBytesAggr = compressor.Wrap(inputBytes, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldNotBe(compressedBytesAggr);

                compressor.UnWrapAsString(compressedBytesStd, Encoding.UTF32).ShouldBe(InputStr);
                compressor.UnWrapAsString(compressedBytesAggr, Encoding.UTF32).ShouldBe(InputStr);

                compressor.UnWrap(compressedBytesStd).ShouldBe(inputBytes);
                compressor.UnWrap(compressedBytesAggr).ShouldBe(inputBytes);
            }
        }
    }
}
