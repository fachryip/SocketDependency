using Newtonsoft.Json;
using System.Buffers;

namespace NagihSkeleton
{
    public class JsonArrayPool : IArrayPool<char>
    {
        public static readonly JsonArrayPool Instance = new JsonArrayPool();

        public char[] Rent(int minimumLength)
        {
            // use System.Buffers shared pool
            return ArrayPool<char>.Shared.Rent(minimumLength);
        }

        public void Return(char[] array)
        {
            // use System.Buffers shared pool
            ArrayPool<char>.Shared.Return(array);
        }
    }
}