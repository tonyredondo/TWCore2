using System;
using System.Dynamic;
using TWCore.Services;
// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class DuckTypeTest : ContainerParameterService
    {
        public DuckTypeTest() : base("ducktypetest", "DuckType Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting DUCK TYPE TEST");

            var helloObject = new HelloClass();
            var iHello = helloObject.ActAs<IHello>();
            dynamic helloDynamic = new HelloDynamicObject();
            var iHelloDynamic = ((object)helloDynamic).ActAs<IHello>();

            helloObject.Test(Guid.NewGuid(), "ES");
            iHello.Test(Guid.NewGuid(), "ES");
            helloDynamic.Test(Guid.NewGuid(), "ES");
            iHelloDynamic.Test(Guid.NewGuid(), "ES");
            
            //
            Core.Log.InfoBasic(helloObject.Suma(12, 12).ToString());
            Core.Log.InfoBasic(iHello.Suma(12, 12).ToString());
            Core.Log.InfoBasic(helloDynamic.Suma(12, 12).ToString());
            Core.Log.InfoBasic(iHelloDynamic.Suma(12, 12).ToString());
            
            for (var i = 0; i < 50; i++)
            {

                Core.Log.InfoBasic("Calling the object method");
                helloObject.SayHello();

                Core.Log.InfoBasic("Calling the interface method on static object");
                iHello.SayHello();

                Core.Log.InfoBasic("Calling the dynamic method");
                helloDynamic.SayHello();

                Core.Log.InfoBasic("Calling the interface method on dynamic object");
                iHelloDynamic.SayHello();

            }
        }

        public interface IHello
        {
            void SayHello();

            object Test(Guid id, string culture);
            int Suma(int a, int b);
        }

        public class HelloClass
        {
            public void SayHello()
            {
                Core.Log.Warning("Hello World");
            }

            public object Test(Guid id, string culture)
            {
                Core.Log.InfoBasic("Id: {0}, Culture: {1}", id, culture);
                return "Hola mundo";
            }

            public int Suma(int a, int b)
            {
                return a + b;
            }
        }
        public class HelloDynamicObject : DynamicObject
        {
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                if (binder.Name == "SayHello")
                {
                    result = null;
                    Core.Log.Warning("Hello World");
                    return true;
                }
                if (binder.Name == "Test")
                {
                    Core.Log.InfoBasic("Id: {0}, Culture: {1}", args[0], args[1]);
                    result = "Hola mundo";
                    return true;
                }
                if (binder.Name == "Suma")
                {
                    result = (int)args[0] + (int)args[1];
                    return true;
                }
                return base.TryInvokeMember(binder, args, out result);
            }
        }
    }
}