using System;
using TWCore.Security;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class HashTest : ContainerParameterService
    {
        public HashTest() : base("hashtest", "Hash Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting HASH TEST");
            
            var gId = Guid.NewGuid();
            var gIdBytes = gId.ToByteArray();
            Core.Log.InfoBasic("Guid: {0}", gId);

            var sha1 = gIdBytes.GetHashSHA1();
            Core.Log.InfoBasic("SHA1 {0}", sha1);

            var sha256 = gIdBytes.GetHashSHA256();
            Core.Log.InfoBasic("SHA256: {0}", sha256);

            var sha384 = gIdBytes.GetHashSHA384();
            Core.Log.InfoBasic("SHA384: {0}", sha384);

            var sha512 = gIdBytes.GetHashSHA512();
            Core.Log.InfoBasic("SHA512: {0}", sha512);

            var md5 = gIdBytes.GetHashMD5();
            Core.Log.InfoBasic("MD5: {0}", md5);
        }
    }

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
            str.GetHashCode();
            str.GetSuperFastHash();
            str.GetMurmurHash2();
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
            Core.Log.InfoBasic("Normal Hash: {0}", normalHash);
            Core.Log.InfoBasic("Superfast Hash: {0}", superFastHash);
            Core.Log.InfoBasic("MurmurHash2 Hash: {0}", murmurhash2);
        }
    }
}