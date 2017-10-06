using System;
using TWCore.Compression;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class CompressorTest : ContainerParameterService
    {
        public CompressorTest() : base("compressortest", "Compressor Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting COMPRESSOR TEST");
            
            Core.Log.InfoBasic("Generating Random Bytes");
            var rnd = new Random();
            var bytes = new byte[ushort.MaxValue * 2000];
            for(var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)rnd.Next(50, 60);
            Core.Log.InfoBasic("Total Bytes = {0}", bytes.Length.ToReadeableBytes());
            
            Core.Log.InfoBasic("Creating GZip Compressor");
            var gzipCompressor = CompressorManager.GetByEncodingType("gzip");
            using(Watch.Create("GZIP COMPRESSOR")) 
            {
                var gzipBytes = gzipCompressor.Compress(bytes);
                Core.Log.InfoBasic("Total GZIP Bytes = {0} - {1:0.000}%", gzipBytes.Count.ToReadeableBytes(), ((double)gzipBytes.Count * 100) / bytes.Length);
            }

            Core.Log.InfoBasic("Creating Deflate Compressor");
            var deflateCompressor = CompressorManager.GetByEncodingType("deflate");
            using(Watch.Create("DEFLATE COMPRESSOR"))
            {
                var deflateBytes = deflateCompressor.Compress(bytes);
                Core.Log.InfoBasic("Total DEFLATE Bytes = {0} - {1:0.000}%", deflateBytes.Count.ToReadeableBytes(), ((double)deflateBytes.Count * 100) / bytes.Length);
            }
        }
    }
}