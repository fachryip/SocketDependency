using Newtonsoft.Json;

namespace NagihSkeleton
{
    public class AutomaticJsonNameTable : DefaultJsonNameTable
    {
        private int _autoAdded = 0;
        private int _maxToAutoAdd;

        public AutomaticJsonNameTable(int maxToAdd)
        {
            _maxToAutoAdd = maxToAdd;
        }

        public override string Get(char[] key, int start, int length)
        {
            var s = base.Get(key, start, length);

            if (s == null && _autoAdded < _maxToAutoAdd)
            {
                s = new string(key, start, length);
                Add(s);
                _autoAdded++;
            }

            return s;
        }
    }
}