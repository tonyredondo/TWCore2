﻿using System;
using TWCore.Injector;
using TWCore.Services;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace TWCore.Tests
{
    public class InjectorTest : ContainerParameterService
    {
        public InjectorTest() : base("injectorTest", "Injector Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            var n2obj1 = Core.Injector.New<IInjectTestA>("Default");
            var n2obj2 = Core.Injector.New<IInjectTestB>("Default");

            var instance1 = Core.Injector.New<IInjectTestA>("Default");
            var instance2 = Core.Injector.New<IInjectTestA>("Default");

            var times = 10_000_000;
            Core.Log.InfoBasic("Injector Number of times: {0}", times);

            using (var w = Watch.Create("Injector Object A"))
                for(var i = 0; i < times; i++)
                    Core.Injector.New<IInjectTestA>("Default");

            using (var w = Watch.Create("Injector Object B"))
                for (var i = 0; i < times; i++)
                    Core.Injector.New<IInjectTestB>("Default");


            if (instance1 != instance2)
                Core.Log.InfoBasic("The instance are differents. OK");
            else
                Core.Log.Error("The instance is the same. Error");

            var guid1 = instance1.GetGuid();
            var guid2 = instance2.GetGuid();

            if (guid1 != guid2)
                Core.Log.InfoBasic("The Guid are differents. OK");
            else
                Core.Log.Error("The Guid are the same. Error");
        }
    }


    public interface IInjectTestA
    {
        Guid GetGuid();
    }
    public interface IInjectTestB
    {
        Guid GuidValue { get; }
    }

    public class InjectTestB : IInjectTestB
    {
        public Guid GuidValue { get; }

        public InjectTestB()
        {
            GuidValue = Guid.NewGuid();
        }
    }

    public class InjectTestA : IInjectTestA
    {
        private IInjectTestB _bInstance;
        public string TestProperty { get; set; }

        public InjectTestA(IInjectTestB bInstance)
        {
            _bInstance = bInstance;
        }

        public Guid GetGuid()
        {
            return _bInstance.GuidValue;
        }
    }
}