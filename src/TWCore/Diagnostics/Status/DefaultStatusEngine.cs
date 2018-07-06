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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly StatusContainerCollection _statusCollection = new StatusContainerCollection();

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Current status engine transport
        /// </summary>
        public ObservableCollection<IStatusTransport> Transports { get; } = new ObservableCollection<IStatusTransport>();
        /// <inheritdoc />
        /// <summary>
        /// Enable or Disable the Log engine
        /// </summary>
        [StatusProperty]
        public bool Enabled { get; set; } = Core.GlobalSettings.StatusEnabled;
        #endregion

        #region .ctor
        /// <summary>
        /// Default status engine
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultStatusEngine()
        {
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
        ~DefaultStatusEngine()
        {
            Dispose();
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Attach a status item delegate 
        /// </summary>
        /// <param name="statusItemDelegate">Status Item delegate</param>
        /// <param name="parent">Object Parent</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(Func<StatusItem> statusItemDelegate, object parent = null)
        {
            if (statusItemDelegate == null) return;
            //
            _statusCollection.Add(statusItemDelegate.Target, statusItemDelegate, parent != statusItemDelegate.Target ? parent : null);
        }
        /// <inheritdoc />
        /// <summary>
        /// Attach a values filler delegate
        /// </summary>
        /// <param name="valuesFillerDelegate">Values filler delegate</param>
        /// <param name="objectToAttach">Object to attach, if is null is extracted from the delegate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(Action<StatusItemValuesCollection> valuesFillerDelegate, object objectToAttach = null)
        {
            if (valuesFillerDelegate == null) return;
            objectToAttach = objectToAttach ?? valuesFillerDelegate.Target;
            _statusCollection.Add(objectToAttach, valuesFillerDelegate, null);
        }
        /// <inheritdoc />
        /// <summary>
        /// Attach an object without values
        /// </summary>
        /// <param name="objectToAttach">Object to attach, if is null is extracted from the delegate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachObject(object objectToAttach)
        {
            if (objectToAttach != null)
            {
                _statusCollection.Add(objectToAttach, null);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Attach a child object
        /// </summary>
        /// <param name="childObject">Child object</param>
        /// <param name="parent">Parent object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachChild(object childObject, object parent)
        {
            if (parent == null) return;
            if (childObject != parent)
                _statusCollection.Add(childObject, parent);
        }

        /// <inheritdoc />
        /// <summary>
        /// DeAttach all handlers for an object
        /// </summary>
        /// <param name="objectToDeattach">Object to deattach</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeAttachObject(object objectToDeattach)
        {
            if (objectToDeattach == null) return;
            _statusCollection.RemoveTarget(objectToDeattach);
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StatusItemCollection Transport_OnFetchStatus()
        {
            if (!Enabled)
                return null;

            var sw = Stopwatch.StartNew();
            var items = _statusCollection.GetStatus();
            return new StatusItemCollection
            {
                InstanceId = Core.InstanceId,
                Timestamp = Core.Now,
                EnvironmentName = Core.EnvironmentName,
                MachineName = Core.MachineName,
                ApplicationDisplayName = Core.ApplicationDisplayName,
                ApplicationName = Core.ApplicationName,
                Items = items,
                ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds,
                StartTime = Process.GetCurrentProcess().StartTime
            };
        }
        #endregion

        #region Nested Class
        private sealed class StatusContainerCollection
        {
            private readonly object _locker = new object();
            private readonly List<StatusContainer> _statusList = new List<StatusContainer>();
            private readonly List<StatusAttributesContainer> _statusAttributeContainer = new List<StatusAttributesContainer>();
            private readonly ReferencePool<List<StatusContainer>> _containerListPool = new ReferencePool<List<StatusContainer>>(1, lst => lst.Clear());
            private readonly ReferencePool<List<StatusData>> _statusListPool = new ReferencePool<List<StatusData>>(2, lst => lst.Clear());
            private readonly ReferencePool<StatusItem> _statusItemPool = new ReferencePool<StatusItem>(0, s => { s.Values.Clear(); s.Children.Clear(); });
            private readonly ReferencePool<StatusData> _statusDataPool = new ReferencePool<StatusData>(0, s => s.Clear());
            private bool _firstTime = true;

            #region Public Methods
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(object target, Func<StatusItem> func, object parent)
            {
                lock (_locker)
                {
                    EnsureParent(parent);
                    var sItem = EnsureTarget(target, parent);
                    sItem.AddStatusItemDelegate(func);
                }
                try
                {
                    func();
                }
                catch
                {
                    // ignored
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(object target, Action<StatusItemValuesCollection> action, object parent)
            {
                lock (_locker)
                {
                    EnsureParent(parent);
                    var sItem = EnsureTarget(target, parent);
                    sItem.AddStatusItemValuesCollection(action);
                }
                var sI = _statusItemPool.New();
                try
                {
                    action(sI.Values);
                }
                catch
                {
                    // ignored
                }
                _statusItemPool.Store(sI);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(object target, object parent)
            {
                lock (_locker)
                {
                    EnsureParent(parent);
                    var sItem = EnsureTarget(target, parent);
                    sItem.Parent = parent;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveTarget(object target)
            {
                lock (_locker)
                {
                    _statusList.RemoveAll(i => i.Object == target || i.Parent == target);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public List<StatusItem> GetStatus()
            {
                lock (_locker)
                {
                    if (_firstTime)
                    {
                        var sInit = _containerListPool.New();
                        sInit.AddRange(_statusList);
                        foreach (var item in sInit)
                        {
                            if (item.Object == null) continue;
                            item.GetStatusItems();
                        }
                        _firstTime = false;
                        _containerListPool.Store(sInit);
                    }
                    var sList = _containerListPool.New();
                    sList.AddRange(_statusList);
                    var values = _statusListPool.New();
                    var roots = _statusListPool.New();
                    foreach (var item in sList)
                    {
                        if (item.Object == null) continue;
                        var value = _statusDataPool.New();
                        value.Init(item.Object, item.GetStatusItems(), item.Parent);
                        values.Add(value);
                        if (item.Parent == null)
                            roots.Add(value);
                    }
                    _containerListPool.Store(sList);

                    var rValues = new List<StatusItem>();
                    foreach (var root in roots)
                        CreateTree(root, values);
                    foreach (var gItem in roots.GroupBy(r => r.Value?.Name).ToArray())
                    {
                        if (gItem.Count() == 1) continue;
                        var group = new StatusData
                        {
                            Value = new StatusItem { Name = "Instances of: " + gItem.Key }
                        };
                        gItem.Each((item, index) => 
                        {
                            item.Value.Name += " [" + index + "]";
                            group.Value.Children.Add(item.Value);
                            roots.Remove(item);
                        });
                        roots.Add(group);
                    }
                    foreach (var root in roots)
                    {
                        SetIds(string.Empty, root.Value);
                        rValues.Add(root.Value);
                    }
                    //
                    foreach (var value in values)
                        _statusDataPool.Store(value);
                    _statusListPool.Store(values);
                    _statusListPool.Store(roots);
                    MergeSimilar(rValues);
                    //
                    rValues.Sort((a, b) =>
                    {
                        if (a.Name == "Application Information") return -1;
                        if (b.Name == "Application Information") return 1;
                        if (a.Name.StartsWith("TWCore.", StringComparison.Ordinal) &&
                            !b.Name.StartsWith("TWCore.", StringComparison.Ordinal)) return -1;
                        if (!a.Name.StartsWith("TWCore.", StringComparison.Ordinal) &&
                            b.Name.StartsWith("TWCore.", StringComparison.Ordinal)) return 1;
                        return string.CompareOrdinal(a.Name, b.Name);
                    });
                    return rValues;
                }
            }
            #endregion

            #region Private Methods
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void EnsureParent(object parent)
            {
                if (parent == null || _statusList.FirstOrDefault((s, mParent) => s.Object == mParent, parent) != null) return;

                var sParent = new StatusContainer { Object = parent };
                _statusList.Add(sParent);
                var gFuncInstance = new StatusAttributesContainer(parent);
                _statusAttributeContainer.Add(gFuncInstance);
                var parentFunc = gFuncInstance.GetFuncByAttribute();
                if (parentFunc != null)
                {
                    sParent.AddStatusItemDelegate(parentFunc);
                    parentFunc();
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private StatusContainer EnsureTarget(object target, object parent)
            {
                var sItem = _statusList.FirstOrDefault((s, mTarget) => s.Object == mTarget, target);
                if (sItem != null) return sItem;

                sItem = new StatusContainer { Object = target, Parent = parent };
                _statusList.Add(sItem);
                var gFuncInstance = new StatusAttributesContainer(target);
                _statusAttributeContainer.Add(gFuncInstance);
                var baseFunc = gFuncInstance.GetFuncByAttribute();
                if (baseFunc != null)
                {
                    sItem.AddStatusItemDelegate(baseFunc);
                    baseFunc();
                }
                return sItem;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void CreateTree(StatusData status, IEnumerable<StatusData> data)
            {
                if (status.Processed) return;

                (var equalSet, var notEqualSet) = data.Split(item => item.Parent == status.Object);
                FlattenStatus(status);
                var equalSetDistinct = equalSet.Where((s, st) => s != st, status).Each(FlattenStatus).ToArray();
                if (equalSetDistinct.Length > 0)
                {
                    foreach (var item in equalSetDistinct)
                    {
                        CreateTree(item, notEqualSet);
                        if (item.Value != null)
                            status.Value.Children.Add(item.Value);
                    }
                    MergeSimilar(status.Value.Children);
                    foreach (var gItem in status.Value.Children.GroupBy(g => g.Name).ToArray())
                    {
                        if (gItem.Count() == 1) continue;
                        var group = new StatusItem {Name = "Instances of : " + gItem.Key};
                        gItem.Each((item, index) =>
                        {
                            item.Name += " [I:" + index + "]";
                            group.Children.Add(item);
                            status.Value.Children.Remove(item);
                        });
                        status.Value.Children.Add(group);
                    }
                    status.Value.Children.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
                }
                status.Processed = true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void FlattenStatus(StatusData status)
            {
                if (status.Value != null) return;

                var type = status.Object.GetType();
                var attr = type.GetAttribute<StatusNameAttribute>();

                if (status.Statuses == null || status.Statuses.Count == 0)
                {
                    if (status.Object == StatusContainer.Root) return;

                    var name = attr?.Name;
                    if (name == null)
                    {
                        if (type.IsGenericType)
                        {
                            if (type.GetInterface("IList") != null)
                            {
                                var innerType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments[0];
                                name = (type.IsArray ? "Array Of ~ " : "List Of ~ ") + innerType.Namespace + "." + innerType.Name;
                            }
                            else
                                name = type.Namespace + "." + type.Name + " [" + type.GenericTypeArguments.Select(ga => ga.Namespace + "." + ga.Name).Join(", ") + "]";
                        }
                        else
                            name = type.Namespace + "." + type.Name;
                    }
                    status.Value = new StatusItem
                    {
                        Name = name
                    };
                    return;
                }

                var sValue = status.Statuses.Count > 1
                    ? status.Statuses.FirstOrDefault(s => s.Name == null)
                    : status.Statuses[0];
                if (sValue == null)
                    sValue = new StatusItem();
                else
                    status.Statuses.Remove(sValue);
                if (sValue.Name == null)
                {
                    var name = attr?.Name;
                    if (name == null)
                    {
                        if (type.IsGenericType)
                        {
                            if (type.GetInterface("IList") != null)
                            {
                                var innerType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments[0];
                                name = (type.IsArray ? "Array Of ~ " : "List Of ~ ") + innerType.Namespace + "." + innerType.Name;
                            }
                            else
                                name = type.Namespace + "." + type.Name + " [" + type.GenericTypeArguments.Select(ga => ga.Namespace + "." + ga.Name).Join(", ") + "]";
                        }
                        else
                            name = type.Namespace + "." + type.Name;
                    }
                    sValue.Name = name;
                }
                sValue.Children.AddRange(status.Statuses);
                status.Statuses = null;
                status.Value = sValue;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void MergeSimilar(List<StatusItem> statusValuesChildren)
            {
                var sSlash = statusValuesChildren.Where(s => s.Name?.IndexOf("\\", StringComparison.Ordinal) > 0).ToArray();
                foreach (var sSlashItem in sSlash)
                {
                    var baseIndex = sSlashItem.Name.IndexOf("\\", StringComparison.Ordinal);
                    var baseName = sSlashItem.Name.Substring(0, baseIndex);
                    var baseRest = sSlashItem.Name.Substring(baseIndex + 1);
                    var baseStatus = statusValuesChildren.FirstOrDefault((c, mBaseName) => c.Name == mBaseName, baseName);
                    if (baseStatus == null)
                    {
                        baseStatus = new StatusItem { Name = baseName };
                        statusValuesChildren.Add(baseStatus);
                    }
                    statusValuesChildren.Remove(sSlashItem);
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
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SetIds(string baseId, StatusItem item)
            {
                if (item == null) return;
                item.Id = (baseId + item.Name).GetHashSHA1();
                if (item.Values != null)
                {
                    foreach (var value in item.Values)
                    {
                        value.Id = (item.Id + value.Key).GetHashSHA1();
                        if (value.Values == null) continue;
                        foreach (var valueVal in value.Values)
                            valueVal.Id = (value.Id + valueVal.Name).GetHashSHA1();
                    }
                }
                if (item.Children != null)
                {
                    foreach (var child in item.Children)
                        SetIds(item.Id, child);
                }
            }
            #endregion

            #region Nested Types
            private class StatusData
            {
                public object Object;
                public object Parent;
                public StatusItem Value;
                public List<StatusItem> Statuses;
                public bool Processed;

                #region .ctor
                public void Init(object obj, List<StatusItem> statuses, object parent)
                {
                    Object = obj;
                    Statuses = statuses;
                    Parent = parent;
                }
                public void Clear()
                {
                    Object = null;
                    Parent = null;
                    Value = null;
                    Statuses = null;
                    Processed = false;
                }
                #endregion
            }
            private class StatusAttributesContainer
            {
                private static readonly NonBlocking.ConcurrentDictionary<Type, (FastPropertyInfo, StatusPropertyAttribute)[]> StatusPropPerType = new NonBlocking.ConcurrentDictionary<Type, (FastPropertyInfo, StatusPropertyAttribute)[]>();
                private static readonly NonBlocking.ConcurrentDictionary<Type, (FastPropertyInfo, StatusReferenceAttribute)[]> StatusRefPropPerType = new NonBlocking.ConcurrentDictionary<Type, (FastPropertyInfo, StatusReferenceAttribute)[]>();
                private readonly WeakReference<object> _target;
                private readonly (FastPropertyInfo, StatusPropertyAttribute)[] _statusAttributes;
                private readonly (FastPropertyInfo, StatusReferenceAttribute)[] _statusReferenceAttributes;

                #region .ctor
                public StatusAttributesContainer(object target)
                {
                    if (target == null) return;
                    _target = new WeakReference<object>(target);
                    var type = target.GetType();
                    _statusAttributes = StatusPropPerType.GetOrAdd(type, mType => mType.GetProperties().Select(p =>
                    {
                        var attr = p.GetAttribute<StatusPropertyAttribute>();
                        if (attr == null || p == null)
                            return (null, null);
                        var fastProp = p.GetFastPropertyInfo();
                        return (fastProp, attr);
                    }).Where(t => t.Item2 != null).ToArray());
                    _statusReferenceAttributes = StatusRefPropPerType.GetOrAdd(type, mType => mType.GetProperties().Select(p =>
                    {
                        var attr = p.GetAttribute<StatusReferenceAttribute>();
                        if (attr == null || p == null)
                            return (null, null);
                        var fastProp = p.GetFastPropertyInfo();
                        return (fastProp, attr);
                    }).Where(t => t.Item2 != null).ToArray());
                }
                #endregion

                #region Public Methods
                public Func<StatusItem> GetFuncByAttribute()
                {
                    if (_target == null) return null;
                    if (_statusAttributes.Length == 0 && _statusReferenceAttributes.Length == 0)
                        return null;
                    return () =>
                    {
                        if (!_target.TryGetTarget(out var obj)) return null;
                        var sItem = new StatusItem();
                        for (var i = 0; i < _statusAttributes.Length; i++)
                        {
                            (var fProp, var attr) = _statusAttributes[i];
                            var name = attr.Name ?? fProp.Name;
                            var value = fProp.GetValue(obj);
                            var status = attr.Status;
                            var plot = attr.PlotEnabled;
                            sItem.Values.Add(name, value, status, plot);
                        }
                        for (var i = 0; i < _statusReferenceAttributes.Length; i++)
                        {
                            (var fProp, var _) = _statusReferenceAttributes[i];
                            var value = fProp.GetValue(obj);
                            Core.Status.AttachChild(value, obj);
                        }
                        return sItem;
                    };
                }
                #endregion  
            }
            #endregion
        }
        private sealed class StatusContainer
        {
            public static readonly object Root = new object();
            private readonly object _locker = new object();
            private WeakReference<object> _object;
            private WeakReference<object> _parent;
            private readonly List<WeakAction<StatusItemValuesCollection>> _lstStatusValueCollection = new List<WeakAction<StatusItemValuesCollection>>();
            private readonly List<WeakFunc<StatusItem>> _lstStatusItem = new List<WeakFunc<StatusItem>>();

            #region Properties
            public object Object
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _object == null ? Root : _object.TryGetTarget(out var item) ? item : null;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    switch (value)
                    {
                        case null:
                            _object = null;
                            break;
                        case WeakReference<object> reference:
                            _object = reference;
                            break;
                        default:
                            _object = new WeakReference<object>(value);
                            break;
                    }
                }
            }
            public object Parent
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _parent == null ? null : _parent.TryGetTarget(out var item) ? item : null;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    switch (value)
                    {
                        case null:
                            _parent = null;
                            break;
                        case WeakReference<object> reference:
                            _parent = reference;
                            break;
                        default:
                            _parent = new WeakReference<object>(value);
                            break;
                    }
                }
            }
            #endregion

            #region Public Methods
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddStatusItemDelegate(Func<StatusItem> statusItemFunc)
            {
                lock (_locker)
                {
                    var wCreate = WeakDelegate.Create(statusItemFunc);
                    _lstStatusItem.Add(wCreate);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddStatusItemValuesCollection(Action<StatusItemValuesCollection> statusItemValuesAction)
            {
                lock (_locker)
                {
                    var wCreate = WeakDelegate.Create(statusItemValuesAction);
                    _lstStatusValueCollection.Add(wCreate);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public List<StatusItem> GetStatusItems()
            {
                lock (_locker)
                {
                    var lstStatus = new List<StatusItem>();
                    if (Object == null) return lstStatus;

                    foreach(var item in _lstStatusItem)
                    {
                        if (item == null) continue;
                        try
                        {
                            var (ran, result) = item();
                            if (ran)
                                lstStatus.Add(result);
                        }
                        catch(Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }
                    foreach(var item in _lstStatusValueCollection)
                    {
                        if (item == null) continue;
                        try
                        {
                            var sItem = new StatusItem();
                            item(sItem.Values);
                            lstStatus.Add(sItem);

                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }

                    var results = GetAndGroup(lstStatus);
                    return results;
                }
            }
            #endregion

            private static List<StatusItem> GetAndGroup(List<StatusItem> col)
            {
                if (col.Count < 2) return col;
                var response = new List<StatusItem>();
                var group = col.GroupBy(s => s.Name);
                foreach (var item in group)
                {
                    if (item.Count() == 1)
                    {
                        response.Add(item.First());
                    }
                    else
                    {
                        var values = item.SelectMany(i => i.Values).DistinctBy(i => i.Key).ToArray();
                        var children = GetAndGroup(item.SelectMany(i => i.Children).ToList());
                        response.Add(new StatusItem
                        {
                            Name = item.Key,
                            Values = new StatusItemValuesCollection(values),
                            Children = children
                        });
                    }
                }
                return response;
            }
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Transports?.Clear();
        }
    }
}
#pragma warning restore IDE0018 // Declaración de variables alineada
