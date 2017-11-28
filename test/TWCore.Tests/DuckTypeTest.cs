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
        }

        public class HelloClass
        {
            public void SayHello()
            {
                Core.Log.Warning("Hello World");
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
                return base.TryInvokeMember(binder, args, out result);
            }
        }
    }
}