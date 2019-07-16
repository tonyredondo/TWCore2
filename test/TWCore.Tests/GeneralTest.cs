using System;
using System.Linq;
using TWCore.Collections;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class GeneralTest : ContainerParameterService
    {
        public GeneralTest() : base("generaltest", "General Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            var lstMetadatas = new KeyValue[]
            {
                new KeyValue("Key01", "Value01"),
                new KeyValue("Key02", "Value02"),
                new KeyValue("Key01", "Value01 - New Value"),
                new KeyValue("Key02", "Value02 - New Value"),
                new KeyValue("Key03", "Value03")
            };

            var mergeEnumerable = lstMetadatas.Merge(item => item.Key, values =>
            {
                return new KeyValue(values.First().Key, string.Join(", ", values.Select(v => v.Value)));
            });

            var mergeList = mergeEnumerable.ToList();
            foreach(var keyValue in mergeList)
            {
                Console.WriteLine($"Key = {keyValue.Key}, Value = {keyValue.Value}");
            }
        }
    }
}