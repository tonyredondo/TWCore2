using System;
using System.Threading.Tasks;
using TWCore.Services;
using TWCore.Threading;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class ProducerConsumerTest : ContainerParameterService
    {
        public ProducerConsumerTest() : base("producerconsumer", "Producer and Consumer Test")
        {
        }

        protected override void OnHandler(ParameterHandlerInfo info)
        {
            var consumer = new ProducerConsumerEnumerable<string>(async (producer, token) =>
            {
                for (var i = 0; i < 2500; i++)
                {
                    producer.Add("Valor: " + i);
                    await TaskUtil.Delay(1, token).ConfigureAwait(false);
                }
            });

            Task.Run(async () =>
            {
                await consumer.ForEachAsync(value =>
                {
                    Console.WriteLine(value);
                }).ConfigureAwait(false);

                Console.WriteLine("Async Finished 1");
            });

            Task.Run(async () =>
            {
                await consumer.ForEachAsync(value =>
                {
                    Console.WriteLine(value);
                }).ConfigureAwait(false);

                Console.WriteLine("Async Finished 2");
            });
            
            Task.Run(async () =>
            {
                await consumer.ForEachAsync(value =>
                {
                    Console.WriteLine(value);
                }).ConfigureAwait(false);

                Console.WriteLine("Async Finished 3");
            });


            Console.ReadLine();
        }
    }
}