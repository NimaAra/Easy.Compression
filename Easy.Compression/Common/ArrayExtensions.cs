namespace Easy.Compression.Common
{
    using System;

    internal static class ArrayExtensions
    {
        internal static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}