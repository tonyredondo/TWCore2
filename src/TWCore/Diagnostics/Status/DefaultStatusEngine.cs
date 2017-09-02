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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Reflection;

namespace TWCore.Diagnostics.Status
{
    /// <summary>
    /// Default status engine
    /// </summary>
    [IgnoreStackFrameLog]
    public class DefaultStatusEngine : IStatusEngine
    {
        readonly List<StatusItemsDelegateItem> StatusItemsDelegates = new List<StatusItemsDelegateItem>();
        readonly List<StatusDelegateItem> StatusValuesDelegates = new List<StatusDelegateItem>();
        readonly ObjectHierarchyCollection ObjectsHierarchy = new ObjectHierarchyCollection();
        readonly List<WeakReference> DeattachList = new List<WeakReference>();
        //bool applyProperties = false;

        #region Properties
        /// <summary>
        /// Current status engine transport
        /// </summary>
        public ObservableCollection<IStatusTransport> Transports { get; } = new ObservableCollection<IStatusTransport>();
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
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null)
                    {
                        foreach (IStatusTransport item in e.NewItems)
                            item.OnFetchStatus += Transport_OnFetchStatus;
                    }
                }
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (e.OldItems != null)
                    {
                        foreach (IStatusTransport item in e.OldItems)
                            item.OnFetchStatus -= Transport_OnFetchStatus;
                    }
                }
                if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    if (e.NewItems != null)
                    {
                        foreach (IStatusTransport item in e.NewItems)
                            item.OnFetchStatus += Transport_OnFetchStatus;
                    }
                    if (e.OldItems != null)
                    {
                        foreach (IStatusTransport item in e.OldItems)
                            item.OnFetchStatus -= Transport_OnFetchStatus;
                    }
                }
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    if (e.OldItems != null)
                    {
                        foreach (IStatusTransport item in e.OldItems)
                            item.OnFetchStatus -= Transport_OnFetchStatus;
                    }
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
        /// <summary>
        /// Attach a status item delegate 
        /// </summary>
        /// <param name="statusItemDelegate">Status Item delegate</param>
        /// <param name="parent">Object Parent</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(Func<StatusItem> statusItemDelegate, object parent = null)
        {
            if (statusItemDelegate != null)
            {
                var weakFunc = WeakDelegate.Create(statusItemDelegate);
                lock (StatusItemsDelegates)
                    StatusItemsDelegates.Add(new StatusItemsDelegateItem { Function = weakFunc, WeakParent = parent != null ? new WeakReference<object>(parent) : null });
                AttachChild(null, parent);
            }
        }
        /// <summary>
        /// Attach a values filler delegate
        /// </summary>
        /// <param name="valuesFillerDelegate">Values filler delegate</param>
        /// <param name="objectToAttach">Object to attach, if is null is extracted from the delegate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(Action<StatusItemValuesCollection> valuesFillerDelegate, object objectToAttach = null)
        {
            if (valuesFillerDelegate != null)
            {
                objectToAttach = objectToAttach ?? valuesFillerDelegate.Target;
                var weakAction = WeakDelegate.Create(valuesFillerDelegate);
                lock (StatusValuesDelegates)
                    StatusValuesDelegates.Add(new StatusDelegateItem
                    {
                        Action = weakAction,
                        WeakObject = new WeakReference<object>(objectToAttach)
                    });
            }
        }
        /// <summary>
        /// Attach an object without values
        /// </summary>
        /// <param name="objectToAttach">Object to attach, if is null is extracted from the delegate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachObject(object objectToAttach)
        {
            if (objectToAttach != null)
            {
                BindPropertiesFromAnObject(objectToAttach);
            }
        }
        /// <summary>
        /// Attach a child object
        /// </summary>
        /// <param name="childObject">Child object</param>
        /// <param name="parent">Parent object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachChild(object childObject, object parent)
        {
            if (parent != null)
            {
                lock (ObjectsHierarchy)
                {
                    var objParent = ObjectsHierarchy.FindParent(parent);
                    if (objParent == null)
                    {
                        objParent = new ObjectHierarchyItem { WeakObject = new WeakReference<object>(parent) };
                        ObjectsHierarchy.Add(objParent);
                    }
                    if (childObject != null && !objParent.Childrens.Any(i => i.Object == childObject))
                    {
                        var childrens = ObjectsHierarchy.Where(i => i.Object == childObject).ToList();
                        if (childrens.Any())
                        {
                            ObjectsHierarchy.RemoveAll(o => childrens.Contains(o));
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
        }

        /// <summary>
        /// DeAttach all handlers for an object
        /// </summary>
        /// <param name="objectToDeattach">Object to deattach</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeAttachObject(object objectToDeattach)
        {
            if (objectToDeattach != null)
            {
                lock (ObjectsHierarchy)
                {
                    lock (DeattachList)
                        DeattachList.Add(new WeakReference(objectToDeattach));
                    var item = ObjectsHierarchy.RemoveParent(objectToDeattach);
                    if (item != null)
                        item.GetAllObjectsInHierarchy().RemoveNulls().Each(hobj =>
                        {
                            StatusItemsDelegates.RemoveAll(i => i.Parent == hobj);
                            StatusValuesDelegates.RemoveAll(i => i.Object == hobj);
                        });
                }
            }
        }
        #endregion

        #region Private Methods

        List<object> lstObj = new List<object>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerable<StatusItem> GetStatusItemsFromDelegates()
        {
            lock (StatusItemsDelegates)
            {
                return StatusItemsDelegates.Select(i =>
                {
                    var sw = Stopwatch.StartNew();
                    var val = Try.Do(i.Function);
                    sw.Stop();
                    if (val != null)
                    {
                        val.ObjRef = i.Parent;
                        val.Parent = true;
                    }
                    return val;
                }).RemoveNulls();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerable<StatusItem> GetStatusItemsFromValuesDelegates()
        {
            lock (StatusValuesDelegates)
            {
                var groups = StatusValuesDelegates?.Where(i => i?.Object != null).ToArray().GroupBy(i => i.Object);
                return groups?.Select(item =>
                {
                    var sItem = new StatusItem() { Name = item.Key.ToString(), ObjRef = item.Key, Parent = false };
                    lstObj.Add(item.Key);
                    foreach (var i in item)
                        Try.Do(() => i.Action(sItem.Values));
                    var lstSType = lstObj.Where(o => o.GetType() == item.Key.GetType());
                    if (lstSType.Skip(1).Any())
                        sItem.Name = string.Format("{0} [{1}]", sItem.Name, lstSType.IndexOf(item.Key));
                    else
                        sItem.Name = string.Format("{0}", sItem.Name);
                    if (sItem.Values.SortValues)
                        sItem.Values.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
                    return sItem;
                }).RemoveNulls().ToArray() ?? new StatusItem[0];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        StatusItemCollection Transport_OnFetchStatus()
        {
            if (!Enabled)
                return null;

            lock (this)
            {
                var collection = new StatusItemCollection()
                {
                    Timestamp = Core.Now,
                    EnvironmentName = Core.EnvironmentName,
                    MachineName = Core.MachineName,
                    ApplicationDisplayName = Core.ApplicationDisplayName,
                    ApplicationName = Core.ApplicationName
                };
                lstObj = new List<object>();

                var allList = new List<StatusItem>();

                allList.AddRange(GetStatusItemsFromDelegates());

                if (bounded)
                {
                    GetStatusItemsFromValuesDelegates();
                    bounded = false;
                }
                allList.AddRange(GetStatusItemsFromValuesDelegates());

                lock (DeattachList)
                {
                    DeattachList.Each(obj =>
                    {
                        if (obj.IsAlive)
                        {
                            lock (ObjectsHierarchy)
                            {
                                var item = ObjectsHierarchy.RemoveParent(obj.Target);
                                if (item != null)
                                    item.GetAllObjectsInHierarchy().RemoveNulls().Each(hobj =>
                                    {
                                        StatusItemsDelegates.RemoveAll(i => i.Parent == hobj);
                                        StatusValuesDelegates.RemoveAll(i => i.Object == hobj);
                                    });
                            }
                        }
                    });
                }

                allList.Where(i => i.ObjRef == null).Each(item =>
                {
                    item.Childrens.AddRange(allList.Where(i => object.Equals(i.ObjRef, item)).ToList());
                    collection.Items.Add(item);
                });
                collection.Items.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

                var remainings = allList.Where(i => i.ObjRef != null).ToList();
                FillHierarchy(allList, collection.Items, ObjectsHierarchy);

                return collection;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void FillHierarchy(List<StatusItem> remainingList, List<StatusItem> statusList, ObjectHierarchyCollection objectsHierarchy)
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
                        item.Childrens.AddRange(equals.Where(i => i.Parent));
                        item.ObjRef = null;
                        statusList.Add(item);
                        FillHierarchy(remainingList, item.Childrens, obj.Childrens);
                    }
                    else if (obj.Object != null)
                    {
                        var sItem = new StatusItem() { Name = obj.Object.ToString(), Parent = false };
                        lstObj.Add(obj.Object);
                        var lstSType = lstObj.Where(o => o.GetType() == obj.GetType());
                        if (lstSType.Skip(1).Any())
                            sItem.Name = string.Format("{0} [{1}]", sItem.Name, lstSType.IndexOf(obj.Object));
                        else
                            sItem.Name = string.Format("{0}", sItem.Name);
                        statusList.Add(sItem);
                        FillHierarchy(remainingList, sItem.Childrens, obj.Childrens);
                    }
                }
            }
        }

        bool bindFirstTime = true;
        bool bounded = false;
        static ConcurrentBag<WeakReference> alreadyAdded = new ConcurrentBag<WeakReference>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void BindPropertiesFromAnObject(object childObject)
        {
            if (bindFirstTime)
            {
                bindFirstTime = false;
                bounded = true;
            }
            if (!alreadyAdded.Any(i => i.Target == childObject))
            {
                var statusDelegate = new StatusDelegateAttributeItem(childObject);
                StatusValuesDelegates.Add(statusDelegate);
                alreadyAdded.Add(new WeakReference(childObject));
            }
        }
        #endregion

        #region Nested Class
        class StatusDelegateAttributeItem : StatusDelegateItem
        {
            static ConcurrentDictionary<Type, Tuple<FastPropertyInfo, StatusPropertyAttribute>[]> statusPropPerType = new ConcurrentDictionary<Type, Tuple<FastPropertyInfo, StatusPropertyAttribute>[]>();
            static ConcurrentDictionary<Type, Tuple<FastPropertyInfo, StatusReferenceAttribute>[]> statusRefPropPerType = new ConcurrentDictionary<Type, Tuple<FastPropertyInfo, StatusReferenceAttribute>[]>();
            static ConcurrentBag<WeakReference> refAdded = new ConcurrentBag<WeakReference>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StatusDelegateAttributeItem(object obj)
            {
                WeakObject = new WeakReference<object>(obj);
                Action = new Action<StatusItemValuesCollection>(ActionMethod);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void ActionMethod(StatusItemValuesCollection col)
            {
                var obj = Object;
                if (obj != null)
                {
                    var objType = obj.GetType();
                    var statusAttributes = statusPropPerType.GetOrAdd(objType, _type =>
                    {
                        return _type.GetProperties().Select(p =>
                        {
                            var attr = p.GetAttribute<StatusPropertyAttribute>();
                            if (attr != null && p != null)
                            {
                                var fastProp = p.GetFastPropertyInfo();
                                return Tuple.Create(fastProp, attr);
                            }
                            return Tuple.Create<FastPropertyInfo, StatusPropertyAttribute>(null, null);
                        }).Where(t => t.Item2 != null).ToArray();
                    });
                    var statusReferenceAttributes = statusRefPropPerType.GetOrAdd(objType, _type =>
                    {
                        return _type.GetProperties().Select(p =>
                        {
                            var attr = p.GetAttribute<StatusReferenceAttribute>();
                            if (attr != null && p != null)
                            {
                                var fastProp = p.GetFastPropertyInfo();
                                return Tuple.Create(fastProp, attr);
                            }
                            return Tuple.Create<FastPropertyInfo, StatusReferenceAttribute>(null, null);
                        }).Where(t => t.Item2 != null).ToArray();
                    });

                    for (var i = 0; i < statusAttributes.Length; i++)
                    {
                        var tuple = statusAttributes[i];
                        var name = tuple.Item2.Name ?? tuple.Item1.Name;
                        if (!col.Any(c => c.Key == name))
                        {
                            var value = tuple.Item1.GetValue(obj);
                            var status = tuple.Item2.Status;
                            col.Add(name, value, status);
                        }
                    }
                    for (var i = 0; i < statusReferenceAttributes.Length; i++)
                    {
                        var tuple = statusReferenceAttributes[i];
                        var value = tuple.Item1.GetValue(obj);
                        if (value != null)
                        {
                            if (value is IEnumerable)
                            {
                                ((IEnumerable)value).EachObject(_value =>
                                {
                                    if (!refAdded.Any(w => w.Target == _value))
                                    {
                                        Core.Status.AttachChild(_value, obj);
                                        refAdded.Add(new WeakReference(_value));
                                    }
                                });
                            }
                            else
                            {
                                if (!refAdded.Any(w => w.Target == value))
                                {
                                    Core.Status.AttachChild(value, obj);
                                    refAdded.Add(new WeakReference(value));
                                }
                            }
                        }
                    }
                }
            }
        }
        class StatusDelegateItem
        {
            public WeakReference<object> WeakObject { get; set; }
            public virtual Action<StatusItemValuesCollection> Action { get; set; }

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
        class StatusItemsDelegateItem
        {
            public WeakReference<object> WeakParent { get; set; }
            public Func<StatusItem> Function { get; set; }

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

        class ObjectHierarchyCollection : List<ObjectHierarchyItem>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectHierarchyItem FindParent(object parent)
            {
                if (parent != null)
                {
                    foreach (var item in this)
                    {
                        if (item.Object == parent)
                            return item;
                        else if (item.Childrens.Any())
                        {
                            var res = item.Childrens.FindParent(parent);
                            if (res != null)
                                return res;
                        }
                    }
                }
                return null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectHierarchyItem RemoveParent(object parent)
            {
                if (parent != null)
                {
                    var toDelete = this.FirstOrDefault(i => i.Object == parent);
                    if (toDelete != null)
                        Remove(toDelete);
                    else
                    {
                        foreach (var item in this)
                        {
                            if (item.Childrens.Any())
                            {
                                toDelete = item.Childrens.RemoveParent(parent);
                                if (toDelete != null)
                                    break;
                            }
                        }
                    }
                    return toDelete;
                }
                return null;
            }
        }
        class ObjectHierarchyItem
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
        #endregion

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
