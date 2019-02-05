/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

#pragma warning disable IDE0018 // Declaración de variables alineada
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Collections;
using TWCore.Reflection;
using TWCore.Security;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace TWCore.Diagnostics.Status
{
    /// <inheritdoc />
    /// <summary>
    /// Default status engine
    /// </summary>
    [IgnoreStackFrameLog]
    [StatusName("Application Information\\Status")]
    public class DefaultStatusEngine : IStatusEngine
    {
        private const int MaxItems = 2500;
        private static readonly ReferencePool<List<(WeakValue Value, int Index)>> ListPool = new ReferencePool<List<(WeakValue Value, int Index)>>();
        private static readonly ReferencePool<Dictionary<string, WeakValue>> DictioPool = new ReferencePool<Dictionary<string, WeakValue>>();
        private readonly WeakDictionary<object, WeakValue> _weakValues = new WeakDictionary<object, WeakValue>();
        private readonly WeakDictionary<object, WeakChildren> _weakChildren = new WeakDictionary<object, WeakChildren>();
        private readonly HashSet<WeakValue> _values = new HashSet<WeakValue>();
        private readonly HashSet<WeakChildren> _children = new HashSet<WeakChildren>();
        private readonly Action _throttledUpdate;
        private readonly Func<object, WeakValue> _createWeakValueFunc;
        private readonly Func<object, WeakChildren> _createWeakChildrenFunc;
        private StatusItemCollection _lastResult;
        private long _currentItems;

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
        public DefaultStatusEngine()
        {
            _throttledUpdate = new Action(UpdateStatus).CreateThrottledAction(1000);
            _createWeakValueFunc = new Func<object, WeakValue>(CreateWeakValue);
            _createWeakChildrenFunc = new Func<object, WeakChildren>(CreateWeakChildren);
            Transports = new ObservableCollection<IStatusTransport>();
            Transports.CollectionChanged += (s, e) =>
            {
                if (e is null) return;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems != null)
                        {
                            foreach (IStatusTransport item in e.NewItems)
                            {
                                if (item is null)
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
                                if (item is null)
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
                                if (item is null)
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
                                if (item is null)
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
                                if (item is null)
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
        ~DefaultStatusEngine()
        {
            Dispose();
        }
        #endregion

        #region Methods
        /// <inheritdoc />
        public void Attach(Func<StatusItem> statusItemDelegate, object objectToAttach = null)
        {
            if (Interlocked.Read(ref _currentItems) >= MaxItems) return;
            var obj = objectToAttach ?? statusItemDelegate.Target;
            lock (this)
            {
                var weakValue = _weakValues.GetOrAdd(obj, _createWeakValueFunc);
                weakValue.Add(statusItemDelegate);
            }
        }
        /// <inheritdoc />
        public void Attach(Action<StatusItemValuesCollection> valuesFillerDelegate, object objectToAttach = null)
        {
            if (Interlocked.Read(ref _currentItems) >= MaxItems) return;
            var obj = objectToAttach ?? valuesFillerDelegate.Target;
            lock (this)
            {
                var weakValue = _weakValues.GetOrAdd(obj, _createWeakValueFunc);
                weakValue.Add(valuesFillerDelegate);
            }
        }
        /// <inheritdoc />
        public void AttachObject(object objectToAttach)
        {
            if (Interlocked.Read(ref _currentItems) >= MaxItems) return;
            if (objectToAttach is null) return;
            lock (this)
            {
                _weakValues.GetOrAdd(objectToAttach, _createWeakValueFunc);
            }
        }
        /// <inheritdoc />
        public void AttachChild(object objectToAttach, object parent)
        {
            if (Interlocked.Read(ref _currentItems) >= MaxItems) return;
            if (objectToAttach is null) return;
            lock (this)
            {
                var value = _weakValues.GetOrAdd(objectToAttach, _createWeakValueFunc);
                if (parent is null) return;
                value.SetParent(parent);
                _weakValues.GetOrAdd(parent, _createWeakValueFunc);
                var wChildren = _weakChildren.GetOrAdd(parent, _createWeakChildrenFunc);
                if (!wChildren.Children.Any((i, io) => i.IsAlive && i.Target == io, objectToAttach))
                    wChildren.Children.Add(new WeakReference(objectToAttach));
            }
        }
        /// <inheritdoc />
        public void DeAttachObject(object objectToDetach)
        {
            if (Interlocked.Read(ref _currentItems) >= MaxItems) return;
            if (objectToDetach is null) return;
            lock (this)
            {
                var value = _weakValues.GetOrAdd(objectToDetach, _createWeakValueFunc);
                value.Enable = false;
            }
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
            Interlocked.Increment(ref _currentItems);
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
            if (_lastResult is null)
                UpdateStatus();
            else
                _throttledUpdate();
            return _lastResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateStatus()
        {
            lock (this)
            {
                try
                {
                    var startTime = Stopwatch.GetTimestamp();
                    int initialCount;
                    var lstWithSlashName = ListPool.New();
                    var dctByName = DictioPool.New();
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
                            if (value.CurrentStatusItem is null)
                            {
                                value.Update();
                                if (value.CurrentStatusItem is null)
                                    continue;
                            }

                            var name = value.CurrentStatusItem.Name;
                            if (name is null) continue;

                            var slashIdx = name.IndexOf('\\', StringComparison.Ordinal);
                            if (slashIdx > 0)
                                lstWithSlashName.Add((value, slashIdx));

                            if (!dctByName.TryGetValue(name, out var currentValue))
                                dctByName[name] = value;
                        }
                    } while (_weakValues.Count != initialCount);
                    #endregion

                    #region Merge Similar by name
                    foreach (var (value, index) in lstWithSlashName)
                    {
                        if (value.CurrentStatusItem is null) continue;
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
                            if (childStatus is null) continue;
                            if (parentStatus != null)
                            {
                                parentStatus.Children.Add(childStatus);
                                value.Processed = true;
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
                    ListPool.Store(lstWithSlashName);
                    DictioPool.Store(dctByName);

                    #region Clear Values
                    foreach (var value in _weakValues.Values)
                        value.Clean();
                    #endregion
                }
                catch(Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetIds(string prefix, List<StatusItem> items)
        {
            prefix = prefix ?? string.Empty;
            if (items is null) return;
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
        private static void SetIds(string prefix, StatusItemValuesCollection collection)
        {
            prefix = prefix ?? string.Empty;
            if (collection is null) return;
            foreach (var item in collection)
            {
                var key = prefix + item.Key;
                item.Id = key.GetHashSHA1();
                if (item.Values is null) continue;
                foreach (var itemValue in item.Values)
                {
                    var valueKey = key + itemValue.Name;
                    itemValue.Id = valueKey.GetHashSHA1();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckSameNames(List<StatusItem> items)
        {
            if (items is null) return;
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
                if (item.Children is null) continue;
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
            private static readonly ConcurrentDictionary<Type, Func<object, StatusItem>> AttributeFunc = new ConcurrentDictionary<Type, Func<object, StatusItem>>();
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
                        if (item is null)
                        {
                            item = sItemResult;
                            continue;
                        }
                        if (string.IsNullOrEmpty(sItemResult.Name)) continue;
                        item.Name = sItemResult.Name;
                        if (sItemResult.Values != null && sItemResult.Values.Count > 0)
                            item.Values.AddRange(sItemResult.Values);
                        if (sItemResult.Children != null && sItemResult.Children.Count > 0)
                            item.Children.AddRange(sItemResult.Children);
                    }
                }

                if (item is null)
                    item = new StatusItem { Name = GetName(obj) };

                foreach (var @delegate in ActionDelegates)
                    @delegate.TryInvokeAction(item.Values);

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
            private static string GetName(object value)
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
                    if (attrPropAttr is null && attrRefAttr is null) continue;
                    var fastProp = prop.GetFastPropertyInfo();
                    lstProps.Add((fastProp, attrPropAttr, attrRefAttr));
                }
                if (lstProps.Count == 0 && nameAttribute is null) return null;
                return item =>
                {
                    var sItem = new StatusItem();
                    if (nameAttribute != null)
                        sItem.Name = nameAttribute.Name ?? GetName(item);
                    else
                        sItem.Name = GetName(item);

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
#pragma warning restore IDE0018 // Declaración de variables alineada
