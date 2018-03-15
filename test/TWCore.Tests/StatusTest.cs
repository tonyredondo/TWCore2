using System;
using System.Collections.Generic;
using TWCore.Services;
using TWCore.Diagnostics.Status;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class StatusTest : ContainerParameterService
    {
        public StatusTest() : base("statustest", "Status Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting STATUS TEST");

            var rootNodeOne = new Node {Name = "Root Node 1", Value = "Root Value 1" };
            var rootNodeTwo = new Node {Name = "Root Node 2", Value = "Root Value 2" };

            var nodeOne = new Node { Name = "Node 1", Value = "Value 1" };
            var nodeTwo = new Node { Name = "Node 2", Value = "Value 2" };
            var lst = new List<Node>();
            var lst2 = new List<Node>();
            var dct = new Dictionary<string, Node>();
            Core.Status.AttachChild(nodeOne, this);
            Core.Status.AttachChild(nodeTwo, this);
            Core.Status.AttachChild(lst, this);
            Core.Status.AttachChild(lst2, this);
            Core.Status.AttachObject(dct);

            Console.ReadLine();
        }

        public class Node
        {
            public string Name { get; set; }
            [StatusProperty]
            public string Value { get; set; }

            public Node()
            {
                Core.Status.Attach(col =>
                {
                    col.AddOk(nameof(Name), Name);
                    col.SortValues = true;
                }, this);
            }
        }
    }
}