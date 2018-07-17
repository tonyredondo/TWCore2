using System;
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

        public class WStatusEngine : IStatusEngine
        {
            private const int MaxItems = 2500;
            private static readonly ReferencePool<List<(object Key, WeakValue Value, int Index)>> _listPool = new ReferencePool<List<(object Key, WeakValue Value, int Index)>>();
            private static readonly ReferencePool<Dictionary<string, WeakValue>> _dictioPool = new ReferencePool<Dictionary<string, WeakValue>>();
            private readonly WeakDictionary<object, WeakValue> _weakValues = new WeakDictionary<object, WeakValue>();
            private readonly WeakDictionary<object, WeakChildren> _weakChildren = new WeakDictionary<object, WeakChildren>();
            private readonly HashSet<WeakValue> _values = new HashSet<WeakValue>();
            private readonly HashSet<WeakChildren> _children = new HashSet<WeakChildren>();
            private readonly Action _throttledUpdate;
            private StatusItemCollection _lastResult;
            private long CurrentItems;

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
                _throttledUpdate = new Action(UpdateStatus).CreateThrottledAction(1000);
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
                if (Interlocked.Read(ref CurrentItems) >= MaxItems) return;
                var obj = objectToAttach ?? statusItemDelegate.Target;
                var weakValue = _weakValues.GetOrAdd(obj, CreateWeakValue);
                weakValue.Add(statusItemDelegate);
            }
            /// <inheritdoc />
            public void Attach(Action<StatusItemValuesCollection> valuesFillerDelegate, object objectToAttach = null)
            {
                if (Interlocked.Read(ref CurrentItems) >= MaxItems) return;
                var obj = objectToAttach ?? valuesFillerDelegate.Target;
                var weakValue = _weakValues.GetOrAdd(obj, CreateWeakValue);
                weakValue.Add(valuesFillerDelegate);
            }
            /// <inheritdoc />
            public void AttachObject(object objectToAttach)
            {
                if (Interlocked.Read(ref CurrentItems) >= MaxItems) return;
                if (objectToAttach == null) return;
                _weakValues.GetOrAdd(objectToAttach, CreateWeakValue);
            }
            /// <inheritdoc />
            public void AttachChild(object objectToAttach, object parent)
            {
                if (Interlocked.Read(ref CurrentItems) >= MaxItems) return;
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
                if (Interlocked.Read(ref CurrentItems) >= MaxItems) return;
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
                Interlocked.Increment(ref CurrentItems);
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
                if (_lastResult == null)
                    UpdateStatus();
                else
                    _throttledUpdate();
                return _lastResult;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void UpdateStatus()
            {
                var startTime = Stopwatch.GetTimestamp();
                int initialCount;
                var lstWithSlashName = _listPool.New();
                var dctByName = _dictioPool.New();
                var items = new List<StatusItem>();

                #region Update Values
                do
                {
                    lstWithSlashName.Clear();
                    dctByName.Clear();
                    initialCount = _weakValues.Count;

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

                        if (!dctByName.TryGetValue(name, out var currentValue))
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

                #region Get Roots
                foreach (var item in _weakValues.Values)
                {
                    if (!item.Enable || item.Processed || item.ObjectParent != null) continue;
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

                #region Fixes
                CheckSameNames(items);
                SetIds(string.Empty, items);
                #endregion

                var endTime = Stopwatch.GetTimestamp();

                #region Status Collection
                _lastResult = new StatusItemCollection
                {
                    InstanceId = Core.InstanceId,
                    Timestamp = Core.Now,
                    EnvironmentName = Core.EnvironmentName,
                    MachineName = Core.MachineName,
                    ApplicationDisplayName = Core.ApplicationDisplayName,
                    ApplicationName = Core.ApplicationName,
                    Items = items,
                    ElapsedMilliseconds = (((double)(endTime - startTime)) / Stopwatch.Frequency) * 1000,
                    StartTime = Process.GetCurrentProcess().StartTime
                };
                #endregion

                lstWithSlashName.Clear();
                dctByName.Clear();
                _listPool.Store(lstWithSlashName);
                _dictioPool.Store(dctByName);

                #region Clear Values
                foreach (var value in _weakValues.Values)
                    value.Clean();
                #endregion
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SetIds(string prefix, List<StatusItem> items)
            {
                prefix = prefix ?? string.Empty;
                if (items == null) return;
                foreach (var item in items)
                {
                    var key = prefix + item.Name;
                    item.Id = key.GetHashSHA1();
                    if (item.Children?.Count > 0)
                        SetIds(key, item.Children);
                    if (item.Values?.Count > 0)
                        SetIds(key, item.Values);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SetIds(string prefix, StatusItemValuesCollection collection)
            {
                prefix = prefix ?? string.Empty;
                if (collection == null) return;
                foreach (var item in collection)
                {
                    var key = prefix + item.Key;
                    item.Id = key.GetHashSHA1();
                    if (item.Values == null) continue;
                    foreach (var itemValue in item.Values)
                    {
                        var valueKey = key + itemValue.Name;
                        itemValue.Id = valueKey.GetHashSHA1();
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void CheckSameNames(List<StatusItem> items)
            {
                if (items == null) return;
                if (items.Count == 0) return;
                if (items.Count > 1)
                {
                    var group = items.GroupBy(i => i.Name).ToArray();
                    foreach (var item in group)
                    {
                        if (item.Count() == 1) continue;
                        var itemArray = item.ToArray();

                        var statusGroup = new StatusItem
                        {
                            Name = "Instances of: " + item.Key
                        };
                        for (var idx = 0; idx < itemArray.Length; idx++)
                        {
                            var value = itemArray[idx];
                            value.Name += " [" + idx + "]";
                            statusGroup.Children.Add(value);
                            items.Remove(value);
                        }
                        items.Add(statusGroup);
                    }
                }
                foreach (var item in items)
                {
                    if (item.Children == null) continue;
                    CheckSameNames(item.Children);
                }
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
                        item = new StatusItem { Name = GetName(obj) };

                    foreach (var @delegate in ActionDelegates)
                        @delegate.TryInvokeAction(item.Values);

                    CurrentStatusItem = item;

                    string GetName(object value)
                    {
                        var type = value.GetType();
                        if (type.IsGenericType)
                        {
                            if (type.GetInterface("IList") != null)
                            {
                                var innerType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments[0];
                                return (type.IsArray ? "Array Of ~ " : "List Of ~ ") + innerType.Namespace + "." + innerType.Name;
                            }
                            else
                                return type.Namespace + "." + type.Name + " [" + type.GenericTypeArguments.Select(ga => ga.Namespace + "." + ga.Name).Join(", ") + "]";
                        }
                        else
                            return type.Namespace + "." + type.Name;
                    }
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