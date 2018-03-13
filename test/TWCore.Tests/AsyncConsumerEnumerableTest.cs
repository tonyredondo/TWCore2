using System;
using System.Threading.Tasks;
using TWCore.Services;
using TWCore.Threading;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class AsyncConsumerEnumerableTest : ContainerParameterService
    {
        public AsyncConsumerEnumerableTest() : base("asyncconsumer", "Async Consumer Enumerable Test")
        {
        }

        protected override void OnHandler(ParameterHandlerInfo info)
        {
            var consumer = new AsyncConsumerEnumerable<string>
            {
                Task.Run(() => "Hola"),
                TaskUtil.Delay(5000).ContinueWith(t => "Esperó 5 segundos"),
                TaskUtil.Delay(2000).ContinueWith(t => "Mundo (tenia una espera de 2 pero ya se habia cumplido)"),
                Task.Run(() => new[] { "Tambien", "Soporto", "Arrays" })
            };

            foreach(var c in consumer)
            {
                Console.WriteLine(c);
            }

            Console.ReadLine();
        }
    }
}