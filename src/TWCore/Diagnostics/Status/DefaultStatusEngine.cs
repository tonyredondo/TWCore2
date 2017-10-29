/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
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
    public class DefaultStatusEngine : IStatusEngine
    {
        private readonly List<StatusItemsDelegateItem> _statusItemsDelegates = new List<StatusItemsDelegateItem>();
        private readonly List<StatusDelegateItem> _statusValuesDelegates = new List<StatusDelegateItem>();
        private readonly ObjectHierarchyCollection _objectsHierarchy = new ObjectHierarchyCollection();
        private readonly List<WeakReference> _deattachList = new List<WeakReference>();
        private readonly StatusContainerCollection _statusCollection = new StatusContainerCollection();
        //bool applyProperties = false;

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
                            }
                        }
                        break;
                }
            };
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
            //_statusCollection.Add(statusItemDelegate.Target, statusItemDelegate, parent);
            //
            var weakFunc = parent != null ? WeakDelegate.Create(statusItemDelegate) : () => (true, statusItemDelegate());
            lock (_statusItemsDelegates)
                _statusItemsDelegates.Add(new StatusItemsDelegateItem { Function = weakFunc, WeakParent = parent != null ? new WeakReference<object>(parent) : null });
            AttachChild(null, parent);
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
            //
            var weakAction = WeakDelegate.Create(valuesFillerDelegate);
            lock (_statusValuesDelegates)
                _statusValuesDelegates.Add(new StatusDelegateItem
                {
                    Action = weakAction,
                    WeakObject = new WeakReference<object>(objectToAttach)
                });
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
                BindPropertiesFromAnObject(objectToAttach);
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
            
            lock (_objectsHierarchy)
            {
                var objParent = _objectsHierarchy.FindParent(parent);
                if (objParent == null)
                {
                    objParent = new ObjectHierarchyItem { WeakObject = new WeakReference<object>(parent) };
                    _objectsHierarchy.Add(objParent);
                }
                if (childObject != null && objParent.Childrens.All(i => i.Object != childObject))
                {
                    var childrens = _objectsHierarchy.Where(i => i.Object == childObject).ToList();
                    if (childrens.Any())
                    {
                        _objectsHierarchy.RemoveAll(o => childrens.Contains(o));
                        objParent.Childrens.AddRange(childrens);
                    }
                    else
                    {
                        objParent.Childrens.Add(new ObjectHierarchyItem { WeakObject = new WeakReference<object>(childObject) });
                    }
                }
            }
            BindPropertiesFromAnObject(parent);
            BindPropertiesFromAnObject(childObject);
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
            
            lock (_objectsHierarchy)
            {
                lock (_deattachList)
                    _deattachList.Add(new WeakReference(objectToDeattach));
                var item = _objectsHierarchy.RemoveParent(objectToDeattach);
                item?.GetAllObjectsInHierarchy().RemoveNulls().Each(hobj =>
                {
                    _statusItemsDelegates.RemoveAll(i => i.Parent == hobj);
                    _statusValuesDelegates.RemoveAll(i => i.Object == hobj);
                });
            }
        }
        #endregion

        #region Private Methods

        private List<object> _lstObj = new List<object>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<StatusItem> GetStatusItemsFromDelegates()
        {
            lock (_statusItemsDelegates)
            {
                return _statusItemsDelegates.Select(i =>
                {
                    var sw = Stopwatch.StartNew();
                    var val = Try.Do(()=> i.Function().Result);
                    sw.Stop();
                    if (val == null) return null;
                    val.ObjRef = i.Parent;
                    val.Parent = true;
                    return val;
                }).RemoveNulls();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<StatusItem> GetStatusItemsFromValuesDelegates()
        {
            lock (_statusValuesDelegates)
            {
                var groups = _statusValuesDelegates.Where(i => i?.Object != null).ToArray().GroupBy(i => i.Object);
                return groups.Select(item =>
                {
                    var sItem = new StatusItem { Name = item.Key.ToString(), ObjRef = item.Key, Parent = false };
                    _lstObj.Add(item.Key);
                    foreach (var i in item)
                        Try.Do(() => i.Action(sItem.Values));
                    var lstSType = _lstObj.Where(o => o.GetType() == item.Key.GetType()).ToArray();
                    sItem.Name = lstSType.Skip(1).Any() ? 
                        string.Format("{0} [{1}]", sItem.Name, lstSType.IndexOf(item.Key)) : 
                        string.Format("{0}", sItem.Name);
                    if (sItem.Values.SortValues)
                        sItem.Values.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
                    return sItem;
                }).RemoveNulls().ToArray() ?? new StatusItem[0];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StatusItemCollection Transport_OnFetchStatus()
        {
            if (!Enabled)
                return null;

            _statusCollection.GetStatus();
            
            lock (this)
            {
                var sw = Stopwatch.StartNew();
                var collection = new StatusItemCollection
                {
                    Timestamp = Core.Now,
                    EnvironmentName = Core.EnvironmentName,
                    MachineName = Core.MachineName,
                    ApplicationDisplayName = Core.ApplicationDisplayName,
                    ApplicationName = Core.ApplicationName
                };
                _lstObj = new List<object>();

                var allList = new List<StatusItem>();

                allList.AddRange(GetStatusItemsFromDelegates());

                if (_bounded)
                {
                    GetStatusItemsFromValuesDelegates();
                    _bounded = false;
                }
                allList.AddRange(GetStatusItemsFromValuesDelegates());

                lock (_deattachList)
                {
                    _deattachList.Each(obj =>
                    {
                        if (!obj.IsAlive) return;
                        lock (_objectsHierarchy)
                        {
                            var item = _objectsHierarchy.RemoveParent(obj.Target);
                            item?.GetAllObjectsInHierarchy().RemoveNulls().Each(hobj =>
                            {
                                _statusItemsDelegates.RemoveAll(i => i.Parent == hobj);
                                _statusValuesDelegates.RemoveAll(i => i.Object == hobj);
                            });
                        }
                    });
                }

                allList.Where(i => i.ObjRef == null).Each(item =>
                {
                    item.Children.AddRange(allList.Where(i => Equals(i.ObjRef, item)).ToList());
                    collection.Items.Add(item);
                });
                collection.Items.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

                allList.Where(i => i.ObjRef != null).ToList();
                FillHierarchy(allList, collection.Items, _objectsHierarchy);

                if (collection.Items != null)
                {
                    foreach (var collectionItem in collection.Items)
                        SetIds(string.Empty, collectionItem);
                }

                collection.ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds;
                return collection;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetIds(string baseId, StatusItem item)
        {
            if (item == null) return;
            item.Id = (baseId + item.Name).GetHashSHA1();
            if (item.Values != null)
            {
                foreach(var value in item.Values)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FillHierarchy(IReadOnlyCollection<StatusItem> remainingList, ICollection<StatusItem> statusList, ObjectHierarchyCollection objectsHierarchy)
        {
            lock (objectsHierarchy)
            {
                foreach (var obj in objectsHierarchy)
                {
                    if (obj.Object == null)
                        continue;
                    var equals = remainingList.Where(i => i.ObjRef == obj.Object).ToList();

                    var item = equals.FirstOrDefault(i => !i.Parent);
                    if (item != null)
                    {
                        item.Children.AddRange(equals.Where(i => i.Parent));
                        item.ObjRef = null;
                        statusList.Add(item);
                        FillHierarchy(remainingList, item.Children, obj.Childrens);
                    }
                    else if (obj.Object != null)
                    {
                        var sItem = new StatusItem { Name = obj.Object.ToString(), Parent = false };
                        _lstObj.Add(obj.Object);
                        var lstSType = _lstObj.Where(o => o.GetType() == obj.GetType()).ToArray();
                        sItem.Name = lstSType.Skip(1).Any() ? 
                            string.Format("{0} [{1}]", sItem.Name, lstSType.IndexOf(obj.Object)) : 
                            string.Format("{0}", sItem.Name);
                        statusList.Add(sItem);
                        FillHierarchy(remainingList, sItem.Children, obj.Childrens);
                    }
                }
            }
        }

        private bool _bindFirstTime = true;
        private bool _bounded;
        private static readonly ConcurrentBag<WeakReference> AlreadyAdded = new ConcurrentBag<WeakReference>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindPropertiesFromAnObject(object childObject)
        {
            if (_bindFirstTime)
            {
                _bindFirstTime = false;
                _bounded = true;
            }
            if (AlreadyAdded.Any(i => i.Target == childObject)) return;
            var statusDelegate = new StatusDelegateAttributeItem(childObject);
            lock (_statusValuesDelegates)
                _statusValuesDelegates.Add(statusDelegate);
            AlreadyAdded.Add(new WeakReference(childObject));
        }
        #endregion

        #region Nested Class
        private sealed class StatusDelegateAttributeItem : StatusDelegateItem
        {
            private static readonly ConcurrentDictionary<Type, (FastPropertyInfo, StatusPropertyAttribute)[]> StatusPropPerType = new ConcurrentDictionary<Type, (FastPropertyInfo, StatusPropertyAttribute)[]>();
            private static readonly ConcurrentDictionary<Type, (FastPropertyInfo, StatusReferenceAttribute)[]> StatusRefPropPerType = new ConcurrentDictionary<Type, (FastPropertyInfo, StatusReferenceAttribute)[]>();
            private static readonly ConcurrentBag<WeakReference> RefAdded = new ConcurrentBag<WeakReference>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StatusDelegateAttributeItem(object obj)
            {
                WeakObject = new WeakReference<object>(obj);
                Action = (col) => 
                { 
                    ActionMethod(col);
                    return true;
                };
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ActionMethod(StatusItemValuesCollection col)
            {
                var obj = Object;
                if (obj == null) return;
                var objType = obj.GetType();
                var statusAttributes = StatusPropPerType.GetOrAdd(objType, mType =>
                {
                    return mType.GetProperties().Select(p =>
                    {
                        var attr = p.GetAttribute<StatusPropertyAttribute>();
                        if (attr == null || p == null)
                            return (null, null);
                        var fastProp = p.GetFastPropertyInfo();
                        return (fastProp, attr);
                    }).Where(t => t.Item2 != null).ToArray();
                });
                var statusReferenceAttributes = StatusRefPropPerType.GetOrAdd(objType, mType =>
                {
                    return mType.GetProperties().Select(p =>
                    {
                        var attr = p.GetAttribute<StatusReferenceAttribute>();
                        if (attr == null || p == null)
                            return (null, null);
                        var fastProp = p.GetFastPropertyInfo();
                        return (fastProp, attr);
                    }).Where(t => t.Item2 != null).ToArray();
                });

                for (var i = 0; i < statusAttributes.Length; i++)
                {
                    var tuple = statusAttributes[i];
                    var name = tuple.Item2.Name ?? tuple.Item1.Name;
                    if (col.Any(c => c.Key == name)) continue;
                    var value = tuple.Item1.GetValue(obj);
                    var status = tuple.Item2.Status;
                    var plot = tuple.Item2.PlotEnabled;
                    col.Add(name, value, status, plot);
                }
                for (var i = 0; i < statusReferenceAttributes.Length; i++)
                {
                    var tuple = statusReferenceAttributes[i];
                    var value = tuple.Item1.GetValue(obj);
                    switch (value)
                    {
                        case null:
                            continue;
                        case IEnumerable ieVal:
                            ieVal.EachObject(mValue =>
                            {
                                if (RefAdded.Any(w => w.Target == mValue)) return;
                                Core.Status.AttachChild(mValue, obj);
                                RefAdded.Add(new WeakReference(mValue));
                            });
                            break;
                        default:
                            if (RefAdded.Any(w => w.Target == value)) continue;
                            Core.Status.AttachChild(value, obj);
                            RefAdded.Add(new WeakReference(value));
                            break;
                    }
                }
            }
        }
        private class StatusDelegateItem
        {
            public WeakReference<object> WeakObject { get; set; }
            public WeakDelegate.WeakAction<StatusItemValuesCollection> Action { get; set; }

            public object Object
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    object val = null;
                    WeakObject?.TryGetTarget(out val);
                    return val;
                }
            }
        }
        private class StatusItemsDelegateItem
        {
            public WeakReference<object> WeakParent { get; set; }
            public WeakDelegate.WeakFunc<StatusItem> Function { get; set; }

            public object Parent
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    object val = null;
                    WeakParent?.TryGetTarget(out val);
                    return val;
                }
            }
        }
        private class ObjectHierarchyCollection : List<ObjectHierarchyItem>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectHierarchyItem FindParent(object parent)
            {
                if (parent == null) return null;
                foreach (var item in this)
                {
                    if (item.Object == parent)
                        return item;
                    if (!item.Childrens.Any()) continue;
                    
                    var res = item.Childrens.FindParent(parent);
                    if (res != null)
                        return res;
                }
                return null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectHierarchyItem RemoveParent(object parent)
            {
                if (parent == null) return null;
                var toDelete = this.FirstOrDefault(i => i.Object == parent);
                if (toDelete != null)
                    Remove(toDelete);
                else
                {
                    foreach (var item in this)
                    {
                        if (!item.Childrens.Any()) continue;
                        toDelete = item.Childrens.RemoveParent(parent);
                        if (toDelete != null)
                            break;
                    }
                }
                return toDelete;
            }
        }
        private class ObjectHierarchyItem
        {
            public WeakReference<object> WeakObject { get; set; }
            public object Object
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    object val = null;
                    WeakObject?.TryGetTarget(out val);
                    return val;
                }
            }
            public ObjectHierarchyCollection Childrens { get; set; } = new ObjectHierarchyCollection();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IEnumerable<object> GetAllObjectsInHierarchy()
            {
                IEnumerable<object> objs = new List<object> { Object };
                foreach (var child in Childrens)
                    objs.Concat(child.GetAllObjectsInHierarchy());
                return objs;
            }
        }




        private sealed class StatusContainerCollection
        {
            private readonly object _locker = new object();
            private readonly List<StatusContainer> _statusList = new List<StatusContainer>();

            public void Add(object target, Func<StatusItem> func, object parent)
            {
                StatusContainer sItem;
                lock (_locker)
                {
                    sItem = _statusList.FirstOrDefault(s => s.Object == target);
                    if (sItem == null)
                    {
                        sItem = new StatusContainer {Object = target, Parent = parent};
                        _statusList.Add(sItem);
                    }
                }
                sItem.AddStatusItemDelegate(func);
                func();
            }
            public void Add(object target, Action<StatusItemValuesCollection> action, object parent)
            {
                StatusContainer sItem;
                lock (_locker)
                {
                    sItem = _statusList.FirstOrDefault(s => s.Object == target);
                    if (sItem == null)
                    {
                        sItem = new StatusContainer {Object = target, Parent = parent};
                        _statusList.Add(sItem);
                    }
                }
                sItem.AddStatusItemValuesCollection(action);
                var sI = new StatusItem();
                action(sI.Values);
            }
            public void Add(object target, object parent)
            {
                StatusContainer sItem;
                lock (_locker)
                {
                    sItem = _statusList.FirstOrDefault(s => s.Object == target);
                    if (sItem == null)
                    {
                        sItem = new StatusContainer {Object = target, Parent = parent};
                        _statusList.Add(sItem);
                    }
                }
                sItem.Parent = parent;
            }
            public void RemoveTarget(object target)
            {
                lock (_locker)
                    _statusList.RemoveAll(i => i.Object == target || i.Parent == target);
            }
            public void GetStatus()
            {
                lock (_locker)
                {
                    var values = _statusList
                        .Where(s => s.Object != null)
                        .Select(s => new StatusData(s.Object, s.GetStatusItems(), s.Parent))
                        .ToArray();

                    var roots = values.Where(i => i.Parent == null).ToArray();
                    foreach(var root in values)
                        CreateTree(root, values);
                    
                }

                void CreateTree(StatusData status, IEnumerable<StatusData> data)
                {
                    (var equalSet, var notEqualSet) = data.Split(item => item.Parent == status.Object);
                    status.Children.AddRange(equalSet.Where(s => s != status));
                    foreach (var st in status.Children)
                        CreateTree(st, notEqualSet);
                }
            }

            private class StatusData
            {
                public readonly object Object;
                public readonly object Parent;
                public List<StatusItem> Statuses;
                public readonly List<StatusData> Children = new List<StatusData>();

                public StatusData(object obj, List<StatusItem> statuses, object parent)
                {
                    Object = obj;
                    Statuses = statuses;
                    Parent = parent;
                }
            }
        }
        private sealed class StatusContainer
        {
            public static readonly object Root = new object();
            private readonly object _locker = new object();
            private WeakReference<object> _object;
            private WeakReference<object> _parent;
            private readonly List<WeakDelegate.WeakAction<StatusItemValuesCollection>> _lstStatusValueCollection = new List<WeakDelegate.WeakAction<StatusItemValuesCollection>>();
            private readonly List<WeakDelegate.WeakFunc<StatusItem>> _lstStatusItem = new List<WeakDelegate.WeakFunc<StatusItem>>();
            
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
            public void AddStatusItemDelegate(Func<StatusItem> statusItemFunc)
            {
                var wCreate = WeakDelegate.Create(statusItemFunc);
                lock (_locker)
                    _lstStatusItem.Add(wCreate);
            }
            public void AddStatusItemValuesCollection(Action<StatusItemValuesCollection> statusItemValuesAction)
            {
                var wCreate = WeakDelegate.Create(statusItemValuesAction);
                lock(_locker)
                    _lstStatusValueCollection.Add(wCreate);
            }
            public List<StatusItem> GetStatusItems()
            {
                var lstStatuses = new List<StatusItem>();
                lock (_locker)
                {
                    if (Object == null) return lstStatuses;
                    var results = _lstStatusItem
                        .Select(s => s())
                        .Where(r => r.Ran)
                        .Select(s => s.Result)
                        .Concat(_lstStatusValueCollection.Select(s => 
                        {
                            var item = new StatusItem();
                            s(item.Values);
                            return item;
                        }))
                        .GroupBy(s => s.Name)
                        .Select(s =>
                        {
                            if (s.Count() == 1) return s.First();
                            var value = s.SelectMany(i => i.Values).ToArray();
                            var children = s.SelectMany(i => i.Children).ToArray();
                            return new StatusItem
                            {
                                Name = s.Key,
                                Values = new StatusItemValuesCollection(value),
                                Children = new List<StatusItem>(children)
                            };
                        }).ToArray();
                    var nullItem = results.FirstOrDefault(s => s.Name == null);
                    if (nullItem != null)
                        nullItem.Name = Object?.GetType().Name;
                    lstStatuses.AddRange(results);
                }
                return lstStatuses;
            }
            #endregion
            
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
