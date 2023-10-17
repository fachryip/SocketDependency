using System.Collections;
using UnityEngine;

namespace NagihSkeleton
{
    [System.Serializable]
    public struct StringPlatform
    {
        public string Development;
        public string Staging;
        public string Production;

        public StringPlatform(string dev, string stag, string prod)
        {
            Development = dev;
            Staging = stag;
            Production = prod;
        }

        public string Value =>
#if ENV_PROD
                Production;
#elif ENV_STAG
                Staging;
#else
                Development;
#endif
    }
}