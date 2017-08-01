using TWCore.Security;
using TWCore.Services;

namespace TWCore.Tests
{
    public class EncryptionTest : ContainerParameterService
    {
        public EncryptionTest() : base("encryptiontest", "Encryption Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting ENCRYPTION TEST");
            
            var message = "Hola Mundo esto es una prueba de encriptación, espero que funcione";
            var skp = new SymmetricKeyProvider();
            Core.Log.InfoBasic(message);
            var enc = skp.Encrypt(message, "password");
            Core.Log.InfoBasic(enc);
            var dec = skp.Decrypt(enc, "password");
            Core.Log.InfoBasic(dec);
        }
    }
}