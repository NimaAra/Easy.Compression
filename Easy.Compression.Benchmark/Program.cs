// ReSharper disable InconsistentNaming
namespace Easy.Compression.Benchmark
{
    using System;
    using System.Diagnostics;
    using System.Text;

    internal class Program
    {
        private static readonly byte[] _bytes = Encoding.UTF8.GetBytes("This is a relatively is a two but you two maybe gone is wind but maybe going nowhwee- to be found is great.");
        private const int IterationCount = 10000000;

        internal static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            CompressStreamLZ4();
            sw.Stop();

            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);

            Console.WriteLine($"Time taken: {sw.Elapsed.ToString()} | Gen-0: {gen0.ToString()} | Gen-1: {gen1.ToString()} | Gen-2: {gen2.ToString()}");
        }

        private static void CompressStreamLZ4()
        {
            using (var compressor = new LZ4Compressor())
            {
                for (var i = 0; i < IterationCount; i++)
                {
                    compressor.Compress(_bytes);
                }
            }
        }

        private static void DeCompressStreamLZ4()
        {
            using (var compressor = new LZ4Compressor())
            {
                for (var i = 0; i < IterationCount; i++)
                {
                    compressor.Compress(_bytes);
                }
            }
        }
    }
}
