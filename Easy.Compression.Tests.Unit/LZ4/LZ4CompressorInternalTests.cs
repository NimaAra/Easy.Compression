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
                var compressedBytesStd = compressor.Encode(_inputBytes);
                var compressedBytesAggr = compressor.Encode(_inputBytes, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldBe(compressedBytesAggr);

                compressor.DeCode(compressedBytesStd).ShouldBe(_inputBytes);
                compressor.DeCode(compressedBytesAggr).ShouldBe(_inputBytes);
            }
        }

        [Test]
        public void When_compressing_string_with_default_method()
        {
            using (var compressor = new LZ4Compressor())
            {
                var compressedBytesStd = compressor.Encode(InputStr);
                var compressedBytesAggr = compressor.Encode(InputStr, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldBe(compressedBytesAggr);

                compressor.DeCodeAsString(compressedBytesStd).ShouldBe(InputStr);
                compressor.DeCodeAsString(compressedBytesAggr).ShouldBe(InputStr);

                compressor.DeCode(compressedBytesStd).ShouldBe(_inputBytes);
                compressor.DeCode(compressedBytesAggr).ShouldBe(_inputBytes);
            }
        }

        [Test]
        public void When_compressing_string_with_encoding()
        {
            using (var compressor = new LZ4Compressor())
            {
                var inputBytes = Encoding.UTF32.GetBytes(InputStr);

                var compressedBytesStd = compressor.Encode(inputBytes);
                var compressedBytesAggr = compressor.Encode(inputBytes, CompressionLevel.Aggressive);

                compressedBytesStd.ShouldNotBe(compressedBytesAggr);

                compressor.DeCodeAsString(compressedBytesStd, Encoding.UTF32).ShouldBe(InputStr);
                compressor.DeCodeAsString(compressedBytesAggr, Encoding.UTF32).ShouldBe(InputStr);

                compressor.DeCode(compressedBytesStd).ShouldBe(inputBytes);
                compressor.DeCode(compressedBytesAggr).ShouldBe(inputBytes);
            }
        }
    }
}
