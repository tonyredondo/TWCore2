using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Status;
using TWCore.Reflection;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class WeakTest: ContainerParameterService
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

        private class WStatusEngine : IStatusEngine
        {
            private WeakDictionary<object, WeakValue> _weakValues = new WeakDictionary<object, WeakValue>();
            private WeakDictionary<object, WeakChildren> _weakChildren = new WeakDictionary<object, WeakChildren>();
            private HashSet<WeakValue> _values = new HashSet<WeakValue>();
            private HashSet<WeakChildren> _children = new HashSet<WeakChildren>();

            #region Properties
            /// <inheritdoc />
            public ObservableCollection<IStatusTransport> Transports { get; }
            /// <inheritdoc />
            public bool Enabled { get; set; }
            #endregion
            
            #region Methods
            /// <inheritdoc />
            public void Attach(Func<StatusItem> statusItemDelegate, object objectToAttach = null)
            {
                var obj = objectToAttach ?? statusItemDelegate.Target;
                var weakValue = _weakValues.GetOrAdd(obj, key => new WeakValue(key));
                weakValue.Add(statusItemDelegate);
            }
            /// <inheritdoc />
            public void Attach(Action<StatusItemValuesCollection> valuesFillerDelegate, object objectToAttach = null)
            {
                var obj = objectToAttach ?? valuesFillerDelegate.Target;
                var weakValue = _weakValues.GetOrAdd(obj, key => new WeakValue(key));
                weakValue.Add(valuesFillerDelegate);
            }
            /// <inheritdoc />
            public void AttachObject(object objectToAttach)
            {
                if (objectToAttach == null) return;
                _weakValues.GetOrAdd(objectToAttach, key => new WeakValue(key));
            }
            /// <inheritdoc />
            public void AttachChild(object objectToAttach, object parent)
            {
                if (objectToAttach == null) return;
                var value = _weakValues.GetOrAdd(objectToAttach, key => new WeakValue(key));
                value.SetParent(parent);
            }
            /// <inheritdoc />
            public void DeAttachObject(object objectToDetach)
            {
                throw new NotImplementedException();
            }
            /// <inheritdoc />
            public void Dispose()
            {
                throw new NotImplementedException();
            }
            #endregion
            
            #region Nested Types
            private class WeakChildren
            {
                public List<WeakReference> Children { get; set; } = new List<WeakReference>();
            }
            private class WeakValue
            {
                public List<WeakDelegate> FuncDelegates;
                public List<WeakDelegate> ActionDelegates;
                public WeakReference ObjectAttached;
                public WeakReference ObjectParent;

                #region .ctor
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WeakValue(object objectToAttach)
                {
                    FuncDelegates = new List<WeakDelegate>();
                    ActionDelegates = new List<WeakDelegate>();
                    ObjectAttached = new WeakReference(objectToAttach);
                }
                #endregion

                #region Methods
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Add(Func<StatusItem> @delegate)
                {
                    FuncDelegates.Add(@delegate.GetWeak());
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Add(Action<StatusItemValuesCollection> @delegate)
                {
                    ActionDelegates.Add(@delegate.GetWeak());
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void SetParent(object parentObject)
                {
                    if (ObjectParent != null && ObjectParent.IsAlive) return;
                    ObjectParent = new WeakReference(parentObject);
                }
                #endregion
            }
            #endregion
        }
    }
}