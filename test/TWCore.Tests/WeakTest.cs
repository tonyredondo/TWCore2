﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Security;
using TWCore.Collections;
using TWCore.Diagnostics.Status;
using TWCore.Reflection;
using TWCore.Services;
using System.Threading;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class WeakTest : ContainerParameterService
    {
        public WeakTest() : base("weaktest", "Weak Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting WEAK TEST");

            WeakSample();

            Console.ReadLine();
        }

        private static void WeakSample()
        {
            var context = new WeakContext();

            var weakDelegate = ((Action)context.Count).GetWeak();
            var result = weakDelegate.TryInvokeAction();

            context = null;

            var result2 = weakDelegate.TryInvokeAction();

            Task.Delay(100).ContinueWith(async _ =>
            {
                while (true)
                {
                    var result3 = weakDelegate.TryInvokeAction();
                    if (!result3)
                    {
                        Console.WriteLine("Reference was lost");
                        break;
                    }
                    await Task.Delay(100).ConfigureAwait(false);
                    GC.Collect();
                }
            });
        }

        private class WeakContext
        {
            private int _i;
            public void Count()
            {
                Console.WriteLine(_i++);
            }
        }

    }
}