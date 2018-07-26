using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class StringHashTest : ContainerParameterService
    {
        public StringHashTest() : base("stringhashtest", "String Hash Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting String Hash Test");

            var str = "Lorem ipsum dolor sit amet";
            int number = 10_000_000;
            int normalHash = 0;
            uint superFastHash = 0;
            uint murmurhash2 = 0;
            uint murmurhash3 = 0;
            str.GetHashCode();
            str.GetSuperFastHash();
            str.GetMurmurHash2();
            str.GetMurmurHash3();
            using (Watch.Create("Normal Hash - GetHashCode()"))
            {
                for (var i = 0; i < number; i++)
                {
                    normalHash = (str + i).GetHashCode();
                }
            }
            using (Watch.Create("Superfast Hash - GetSuperFastHash()"))
            {
                for (var i = 0; i < number; i++)
                {
                    superFastHash = (str + i).GetSuperFastHash();
                }
            }
            using (Watch.Create("MurmurHash2 Hash - GetMurmurHash2()"))
            {
                for (var i = 0; i < number; i++)
                {
                    murmurhash2 = (str + i).GetMurmurHash2();
                }
            }
            using (Watch.Create("MurmurHash3 Hash - GetMurmurHash3()"))
            {
                for (var i = 0; i < number; i++)
                {
                    murmurhash3 = (str + i).GetMurmurHash3();
                }
            }
            Core.Log.InfoBasic("Normal Hash: {0}", normalHash);
            Core.Log.InfoBasic("Superfast Hash: {0}", superFastHash);
            Core.Log.InfoBasic("MurmurHash2 Hash: {0}", murmurhash2);
            Core.Log.InfoBasic("MurmurHash3 Hash: {0}", murmurhash3);
        }
    }
}