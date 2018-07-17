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

        public class WStatusEngine : IStatusEngine
        {
            private readonly WeakDictionary<object, WeakValue> _weakValues = new WeakDictionary<object, WeakValue>();
            private readonly WeakDictionary<object, WeakChildren> _weakChildren = new WeakDictionary<object, WeakChildren>();
            private readonly HashSet<WeakValue> _values = new HashSet<WeakValue>();
            private readonly HashSet<WeakChildren> _children = new HashSet<WeakChildren>();

            #region Properties
            /// <inheritdoc />
            public ObservableCollection<IStatusTransport> Transports { get; }
            /// <inheritdoc />
            public bool Enabled { get; set; } = true;
            #endregion

            #region .ctor
            /// <summary>
            /// Status Engine
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public WStatusEngine()
            {
                Transports = new ObservableCollection<IStatusTransport>();
                Transports.CollectionChanged += (s, e) =>
                {
                    if (e == null) return;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (e.NewItems != null)
                            {
                                foreach (IStatusTransport item in e.NewItems)
                                {
                                    if (item == null)
                                    {
                                        Core.Log.LibDebug("The IStatusTransport item is null");
                                        continue;
                                    }
                                    item.OnFetchStatus += Transport_OnFetchStatus;
                                    AttachChild(item, this);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            if (e.OldItems != null)
                            {
                                foreach (IStatusTransport item in e.OldItems)
                                {
                                    if (item == null)
                                    {
                                        Core.Log.LibDebug("The IStatusTransport item is null");
                                        continue;
                                    }
                                    item.OnFetchStatus -= Transport_OnFetchStatus;
                                    DeAttachObject(item);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            if (e.NewItems != null)
                            {
                                foreach (IStatusTransport item in e.NewItems)
                                {
                                    if (item == null)
                                    {
                                        Core.Log.LibDebug("The IStatusTransport item is null");
                                        continue;
                                    }
                                    item.OnFetchStatus += Transport_OnFetchStatus;
                                    AttachChild(item, this);
                                }
                            }
                            if (e.OldItems != null)
                            {
                                foreach (IStatusTransport item in e.OldItems)
                                {
                                    if (item == null)
                                    {
                                        Core.Log.LibDebug("The IStatusTransport item is null");
                                        continue;
                                    }
                                    item.OnFetchStatus -= Transport_OnFetchStatus;
                                    DeAttachObject(item);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            if (e.OldItems != null)
                            {
                                foreach (IStatusTransport item in e.OldItems)
                                {
                                    if (item == null)
                                    {
                                        Core.Log.LibDebug("The IStatusTransport item is null");
                                        continue;
                                    }
                                    item.OnFetchStatus -= Transport_OnFetchStatus;
                                    DeAttachObject(item);
                                }
                            }
                            break;
                    }
                };

                AttachObject(this);

            }
            /// <summary>
            /// Destructor
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            ~WStatusEngine()
            {
                Dispose();
            }
            #endregion

            #region Methods
            /// <inheritdoc />
            public void Attach(Func<StatusItem> statusItemDelegate, object objectToAttach = null)
            {
                var obj = objectToAttach ?? statusItemDelegate.Target;
                var weakValue = _weakValues.GetOrAdd(obj, CreateWeakValue);
                weakValue.Add(statusItemDelegate);
            }
            /// <inheritdoc />
            public void Attach(Action<StatusItemValuesCollection> valuesFillerDelegate, object objectToAttach = null)
            {
                var obj = objectToAttach ?? valuesFillerDelegate.Target;
                var weakValue = _weakValues.GetOrAdd(obj, CreateWeakValue);
                weakValue.Add(valuesFillerDelegate);
            }
            /// <inheritdoc />
            public void AttachObject(object objectToAttach)
            {
                if (objectToAttach == null) return;
                _weakValues.GetOrAdd(objectToAttach, CreateWeakValue);
            }
            /// <inheritdoc />
            public void AttachChild(object objectToAttach, object parent)
            {
                if (objectToAttach == null) return;
                var value = _weakValues.GetOrAdd(objectToAttach, CreateWeakValue);
                if (parent == null) return;
                value.SetParent(parent);
                _weakValues.GetOrAdd(parent, CreateWeakValue);
                var wChildren = _weakChildren.GetOrAdd(parent, CreateWeakChildren);
                if (!wChildren.Children.Any((i, io) => i.IsAlive && i.Target == io, objectToAttach))
                    wChildren.Children.Add(new WeakReference(objectToAttach));
            }
            /// <inheritdoc />
            public void DeAttachObject(object objectToDetach)
            {
                if (objectToDetach == null) return;
                var value = _weakValues.GetOrAdd(objectToDetach, CreateWeakValue);
                value.Enable = false;
            }
            /// <inheritdoc />
            public void Dispose()
            {
                Transports?.Clear();
                _weakValues.Clear();
                _weakChildren.Clear();
                _values.Clear();
                _children.Clear();
            }
            #endregion

            #region Private Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private WeakValue CreateWeakValue(object key)
            {
                var wValue = new WeakValue(key);
                _values.Add(wValue);
                return wValue;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private WeakChildren CreateWeakChildren(object parent)
            {
                var wValue = new WeakChildren();
                _children.Add(wValue);
                return wValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private StatusItemCollection Transport_OnFetchStatus()
            {
                if (!Enabled)
                    return null;

                var initialTime = Stopwatch.GetTimestamp();
                var initialCount = _weakValues.Count;
                var lstWithSlashName = new List<(object Key, WeakValue Value, int Index)>(_weakValues.Count);
                var dctByName = new Dictionary<string, WeakValue>();

                #region Update Values
                do
                {
                    lstWithSlashName.Clear();
                    dctByName.Clear();

                    foreach (var weakItem in _weakValues)
                    {
                        var key = weakItem.Key;
                        var value = weakItem.Value;
                        if (value.CurrentStatusItem == null)
                        {
                            value.Update();
                            if (value.CurrentStatusItem == null)
                                continue;
                        }

                        var name = value.CurrentStatusItem.Name;
                        if (name == null) continue;

                        var slashIdx = name.IndexOf('\\', StringComparison.Ordinal);
                        if (slashIdx > 0)
                            lstWithSlashName.Add((key, value, slashIdx));

                        if (dctByName.TryGetValue(name, out var currentValue))
                        {
                            currentValue.CurrentStatusItem.Children.AddRange(value.CurrentStatusItem.Children);
                            currentValue.CurrentStatusItem.Values.AddRange(value.CurrentStatusItem.Values);
                            value.Processed = true;
                        }
                        else
                            dctByName.TryAdd(name, value);
                    }
                } while (_weakValues.Count != initialCount);
                #endregion

                #region Merge Similar by name
                foreach (var (key, value, index) in lstWithSlashName)
                {
                    if (value.CurrentStatusItem == null) continue;
                    var sSlashItem = value.CurrentStatusItem;
                    var name = sSlashItem.Name;
                    var baseName = name.Substring(0, index);
                    var baseRest = name.Substring(index + 1);
                    StatusItem baseStatus;
                    if (dctByName.TryGetValue(baseName, out var baseWeakValue))
                    {
                        baseStatus = baseWeakValue.CurrentStatusItem;
                        value.Processed = true;
                    }
                    else
                    {
                        baseStatus = new StatusItem { Name = baseName };
                        value.CurrentStatusItem = baseStatus;
                    }

                    if (!string.IsNullOrWhiteSpace(baseRest))
                    {
                        sSlashItem.Name = baseRest;
                        baseStatus.Children.Add(sSlashItem);
                    }
                    else
                    {
                        baseStatus.Values.AddRange(sSlashItem.Values);
                        baseStatus.Children.AddRange(sSlashItem.Children);
                    }
                }
                #endregion

                #region Children Tree
                foreach (var item in _weakChildren)
                {
                    if (!_weakValues.TryGetValue(item.Key, out var parentValue)) continue;
                    foreach (var itemWeakValue in item.Value.Children)
                    {
                        var itemValue = itemWeakValue.Target;
                        if (!itemWeakValue.IsAlive) continue;
                        if (!_weakValues.TryGetValue(itemValue, out var value)) continue;

                        var parentStatus = parentValue.CurrentStatusItem;
                        var childStatus = value.CurrentStatusItem;
                        if (childStatus != null)
                        {
                            if (parentStatus != null)
                            {
                                parentStatus.Children.Add(childStatus);
                                value.Processed = true;
                            }
                        }
                    }
                }
                #endregion

                var items = new List<StatusItem>();

                #region Get Roots
                foreach (var item in _weakValues.Values)
                {
                    if (!item.Enable || item.Processed) continue;
                    items.Add(item.CurrentStatusItem);
                }
                items.Sort((a, b) =>
                {
                    if (a.Name == "Application Information") return -1;
                    if (b.Name == "Application Information") return 1;
                    if (a.Name.StartsWith("TWCore.", StringComparison.Ordinal) &&
                        !b.Name.StartsWith("TWCore.", StringComparison.Ordinal)) return -1;
                    if (!a.Name.StartsWith("TWCore.", StringComparison.Ordinal) &&
                        b.Name.StartsWith("TWCore.", StringComparison.Ordinal)) return 1;
                    return string.CompareOrdinal(a.Name, b.Name);
                });
                #endregion

                var endTime = Stopwatch.GetTimestamp();

                var statusCollection = new StatusItemCollection
                {
                    InstanceId = Core.InstanceId,
                    Timestamp = Core.Now,
                    EnvironmentName = Core.EnvironmentName,
                    MachineName = Core.MachineName,
                    ApplicationDisplayName = Core.ApplicationDisplayName,
                    ApplicationName = Core.ApplicationName,
                    Items = items,
                    ElapsedMilliseconds = (((double)(endTime - initialTime)) / Stopwatch.Frequency) * 1000,
                    StartTime = Process.GetCurrentProcess().StartTime
                };

                dctByName.Clear();
                lstWithSlashName.Clear();

                #region Clear Values
                foreach (var value in _weakValues.Values)
                    value.Clean();
                #endregion

                return statusCollection;
            }
            #endregion


            #region Nested Types
            private class WeakChildren
            {
                public List<WeakReference> Children { get; set; } = new List<WeakReference>();
            }
            private class WeakValue
            {
                private static readonly NonBlocking.ConcurrentDictionary<Type, Func<object, StatusItem>> AttributeFunc = new NonBlocking.ConcurrentDictionary<Type, Func<object, StatusItem>>();
                public List<WeakDelegate> FuncDelegates;
                public List<WeakDelegate> ActionDelegates;
                public Func<object, StatusItem> AttributeStatus;
                public WeakReference ObjectAttached;
                public WeakReference ObjectParent;
                public StatusItem CurrentStatusItem;
                public bool Enable = true;
                public bool Processed = false;

                #region .ctor
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WeakValue(object objectToAttach)
                {
                    FuncDelegates = new List<WeakDelegate>();
                    ActionDelegates = new List<WeakDelegate>();
                    ObjectAttached = new WeakReference(objectToAttach);
                    AttributeStatus = AttributeFunc.GetOrAdd(objectToAttach.GetType(), GetAttributeStatusFunc);
                }

                #endregion

                #region Methods
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Add(Func<StatusItem> @delegate)
                    => FuncDelegates.Add(@delegate.GetWeak());

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Add(Action<StatusItemValuesCollection> @delegate)
                    => ActionDelegates.Add(@delegate.GetWeak());

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void SetParent(object parentObject)
                {
                    if (ObjectParent != null && ObjectParent.IsAlive) return;
                    ObjectParent = new WeakReference(parentObject);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Update()
                {
                    if (!Enable)
                    {
                        CurrentStatusItem = null;
                        return;
                    }
                    var obj = ObjectAttached.Target;
                    if (!ObjectAttached.IsAlive)
                    {
                        CurrentStatusItem = null;
                        return;
                    }
                    var item = AttributeStatus?.Invoke(obj);
                    foreach (var @delegate in FuncDelegates)
                    {
                        if (@delegate.TryInvoke(null, out var itemResult) && itemResult is StatusItem sItemResult)
                        {
                            if (item == null)
                            {
                                item = sItemResult;
                                continue;
                            }
                            if (!string.IsNullOrEmpty(sItemResult.Name))
                            {
                                item.Name = sItemResult.Name;
                                if (sItemResult.Values != null && sItemResult.Values.Count > 0)
                                    item.Values.AddRange(sItemResult.Values);
                                if (sItemResult.Children != null && sItemResult.Children.Count > 0)
                                    item.Children.AddRange(sItemResult.Children);
                            }
                        }
                    }
                    if (item == null)
                        item = new StatusItem { Name = obj.GetType().Name };

                    foreach (var @delegate in ActionDelegates)
                        @delegate.TryInvokeAction(item.Values);

                    item.Id = item.Name.GetHashSHA1();
                    CurrentStatusItem = item;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Clean()
                {
                    CurrentStatusItem = null;
                    Processed = false;
                }
                #endregion

                #region Private Methods
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static Func<object, StatusItem> GetAttributeStatusFunc(Type type)
                {
                    var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    var nameAttribute = type.GetAttribute<StatusNameAttribute>();
                    var lstProps = new List<(FastPropertyInfo, StatusPropertyAttribute, StatusReferenceAttribute)>();
                    foreach (var prop in props)
                    {
                        var attrPropAttr = prop.GetAttribute<StatusPropertyAttribute>();
                        var attrRefAttr = prop.GetAttribute<StatusReferenceAttribute>();
                        if (attrPropAttr == null && attrRefAttr == null) continue;
                        var fastProp = prop.GetFastPropertyInfo();
                        lstProps.Add((fastProp, attrPropAttr, attrRefAttr));
                    }
                    if (lstProps.Count == 0 && nameAttribute == null) return null;
                    return item =>
                    {
                        var sItem = new StatusItem();
                        if (nameAttribute != null)
                            sItem.Name = nameAttribute.Name;
                        foreach (var (fProp, propAttr, refAttr) in lstProps)
                        {
                            var value = fProp.GetValue(item);
                            if (propAttr != null)
                            {
                                var name = propAttr.Name ?? fProp.Name;
                                sItem.Values.Add(name, value, propAttr.Status, propAttr.PlotEnabled);
                            }
                            if (refAttr != null)
                            {
                                Core.Status.AttachChild(value, item);
                            }
                        }
                        return sItem;
                    };
                }
                #endregion
            }
            #endregion
        }
    }
}