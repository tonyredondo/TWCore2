using System;
using TWCore.Security;
using TWCore.Services;

namespace TWCore.Test.Tests
{
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
}