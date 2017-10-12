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

using System;
using System.Collections.Generic;
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace TWCore.Numerics
{
    /// <summary>
    /// Group simplifier
    /// </summary>
    public class GroupSimplifier
    {
        #region Properties
        /// <summary>
        /// Matrix with the values
        /// </summary>
        public Dictionary<Key, string> Matrix { get; }
        #endregion

        #region .ctor
        /// <summary>
        /// Group simplifier
        /// </summary>
        public GroupSimplifier()
        {
            Matrix = new Dictionary<Key, string>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Simplifies and combines the matrix
        /// </summary>
        /// <returns>Result group collection instance</returns>
        public ResultGroupCollection Simplify()
        {
            var vectors = new VectorCollection();
            foreach (var elm in Matrix.Keys)
            {
                var vect = vectors.GetVectorByKey(elm.Name);
                if (vect == null)
                {
                    vect = new Vector { Key = elm.Name };
                    vectors.Add(vect);
                }
                vect.Values.Add(Matrix[elm]);
            }

            var maxValues = vectors.GetMaxCountValues();
            var resultGroups = new ResultGroupCollection();

            while (maxValues > 0)
            {
                if (vectors.Count == 1)
                {
                    var rgroup = new ResultGroup();
                    rgroup.Keys.Add(vectors[0].Key);
                    rgroup.Values.AddRange(vectors[0].Values);
                    resultGroups.Add(rgroup);
                }
                else
                {
                    var rgroup = new ResultGroup();
                    for (var i = 0; i < vectors.Count; i++)
                    {
                        var vecX = vectors[i];

                        for (var j = i + 1; j < vectors.Count; j++)
                        {
                            var vecY = vectors[j];
                            var elementsCoincidentes = new List<string>();
                            var numIgualdades = 0;
                            foreach (var elmX in vecX.Values)
                            {
                                foreach (var elmY in vecY.Values)
                                {
                                    if (elmX != elmY) continue;
                                    numIgualdades++;
                                    if (!elementsCoincidentes.Contains(elmX))
                                        elementsCoincidentes.Add(elmX);
                                }
                            }
                            if (numIgualdades == maxValues)
                            {
                                if (rgroup.Values.Count == 0)
                                {
                                    rgroup.Values.AddRange(elementsCoincidentes);
                                    rgroup.Keys.Add(vecX.Key);
                                    rgroup.Keys.Add(vecY.Key);
                                }
                                else
                                {
                                    var igualdadesgrupo = 0;
                                    foreach (var elmX in rgroup.Values)
                                    {
                                        foreach (var elmY in elementsCoincidentes)
                                        {
                                            if (elmX == elmY)
                                                igualdadesgrupo++;
                                        }
                                    }
                                    if (igualdadesgrupo != maxValues)
                                    {
                                        if (rgroup.Keys.Count > 0)
                                            resultGroups.Add(rgroup);
                                        rgroup = new ResultGroup();
                                        rgroup.Values.AddRange(elementsCoincidentes);
                                        rgroup.Keys.Add(vecX.Key);
                                        rgroup.Keys.Add(vecY.Key);
                                    }
                                    else if (igualdadesgrupo == maxValues)
                                    {
                                        var xAdded = false;
                                        foreach (var key in rgroup.Keys)
                                            if (key == vecX.Key)
                                                xAdded = true;
                                        if (!xAdded)
                                            rgroup.Keys.Add(vecX.Key);
                                        var yAdded = false;
                                        foreach (var key in rgroup.Keys)
                                            if (key == vecY.Key)
                                                yAdded = true;
                                        if (!yAdded)
                                            rgroup.Keys.Add(vecY.Key);
                                    }
                                }
                            }
                            else
                            {
                                if (maxValues == vecX.Values.Count && maxValues > vecY.Values.Count)
                                {
                                    if (rgroup.Keys.Count > 0 && !resultGroups.ContainsRGroup(vecX))
                                        resultGroups.Add(rgroup);
                                    rgroup = new ResultGroup();
                                    rgroup.Values.AddRange(vecX.Values);
                                    rgroup.Keys.Add(vecX.Key);
                                }
                                if (maxValues == vecY.Values.Count && maxValues > vecX.Values.Count)
                                {
                                    if (rgroup.Keys.Count > 0 && !resultGroups.ContainsRGroup(vecY))
                                        resultGroups.Add(rgroup);
                                    rgroup = new ResultGroup();
                                    rgroup.Values.AddRange(vecY.Values);
                                    rgroup.Keys.Add(vecY.Key);
                                }
                            }
                        }
                    }
                    if (rgroup.Keys.Count > 0 && rgroup.Values.Count > 0)
                        resultGroups.Add(rgroup);
                }
                maxValues--;
            }
            return resultGroups;
        }
        #endregion

        #region Nested Classes
        /// <summary>
        /// Dictionary safe key
        /// </summary>
        public class Key
        {
            /// <summary>
            /// Key name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Dictionary safe key
            /// </summary>
            /// <param name="key">Key name</param>
            public Key(string key) { Name = key; }
        }

        /// <inheritdoc />
        /// <summary>
        /// Group collection results
        /// </summary>
        public class ResultGroupCollection : List<ResultGroup>
        {
            /// <summary>
            /// Adds a result group to the collection
            /// </summary>
            /// <param name="res">Result group to be added</param>
            public new void Add(ResultGroup res)
            {
                var existe = false;

                foreach (var group in this)
                {
                    if (group.CompareTo(res) == 0)
                        existe = true;
                    else
                    {
                        if (res.Keys.Count == 1)
                        {
                            if (group.ContainsKey(res.Keys[0]))
                            {
                                var valorExiste = true;
                                foreach (var elmValue in res.Values)
                                    valorExiste &= group.ContainsValue(elmValue);
                                if (valorExiste)
                                    existe = true;
                            }
                        }
                        if (res.ValueToString() == group.ValueToString())
                        {
                            foreach (var elmKey in res.Keys)
                            {
                                if (!group.ContainsKey(elmKey))
                                    group.Keys.Add(elmKey);
                            }
                            existe = true;
                        }
                    }
                }

                if (!existe)
                    base.Add(res);
            }
            /// <summary>
            /// Gets if the vector is contained in the response group
            /// </summary>
            /// <param name="vec">Vector</param>
            /// <returns>true if the vector is contained; otherwise, false.</returns>
            public bool ContainsRGroup(Vector vec)
            {
                foreach (var res in this)
                    if (res.Keys.Count == 1)
                        if (res.Keys[0] == vec.Key)
                            return true;
                return false;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Result group definition
        /// </summary>
        public class ResultGroup : IComparable<ResultGroup>
        {
            /// <summary>
            /// Result group keys
            /// </summary>
            public List<string> Keys { get; set; } = new List<string>();
            /// <summary>
            /// Result group values
            /// </summary>
            public List<string> Values { get; set; } = new List<string>();
            /// <summary>
            /// To string override
            /// </summary>
            /// <returns>String representation of the result group</returns>
            public override string ToString()
                => string.Format("Keys: {0}, Values: {1}", string.Join(",", Keys.ToArray()), string.Join(",", Values.ToArray()));
            /// <summary>
            /// Gets if the key is contained in the list
            /// </summary>
            /// <param name="key">Key value</param>
            /// <returns>true if the key is contained; otherwise, false.</returns>
            public bool ContainsKey(string key) => Keys.Contains(key);
            /// <summary>
            /// Gets if the value is contained in the list
            /// </summary>
            /// <param name="value">Value</param>
            /// <returns>true if the value is contained; otherwise, false.</returns>
            public bool ContainsValue(string value) => Values.Contains(value);
            /// <summary>
            /// Gets the values in a string format
            /// </summary>
            /// <returns>Values in a string format</returns>
            public string ValueToString() => string.Format("{0}", string.Join(",", Values.ToArray()));

            #region IComparable<ResultGroup>
            /// <summary>
            /// Compare two result groups
            /// </summary>
            /// <param name="other">The other result group</param>
            /// <returns>Compare result</returns>
            public int CompareTo(ResultGroup other)
            {
                string strSign = string.Format("{0}${1}", string.Join(",", other.Keys.ToArray()), string.Join(",", other.Values.ToArray()));
                string strSign2 = string.Format("{0}${1}", string.Join(",", Keys.ToArray()), string.Join(",", Values.ToArray()));
                return string.Compare(strSign2, strSign, StringComparison.Ordinal);
            }
            #endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// Vectors collection
        /// </summary>
        public class VectorCollection : List<Vector>
        {
            /// <summary>
            /// Gets the maximum number of values
            /// </summary>
            /// <returns>Maximum number of values</returns>
            public int GetMaxCountValues()
            {
                int maxNum = 0;
                foreach (Vector vec in this)
                    if (vec.Values.Count > maxNum)
                        maxNum = vec.Values.Count;
                return maxNum;
            }
            /// <summary>
            /// Gets the vectors with a specific value count
            /// </summary>
            /// <param name="count">Count of element in the vector values</param>
            /// <returns>Vector collection</returns>
            public VectorCollection GetVectorsCountValues(int count)
            {
                var vc = new VectorCollection();
                if (count > GetMaxCountValues()) return vc;
                foreach (var vec in this)
                    if (vec.Values.Count == count)
                        vc.Add(vec);
                return vc;
            }
            /// <summary>
            /// Gets if the key is contained
            /// </summary>
            /// <param name="key">Vector key to check in the collection</param>
            /// <returns>true if a vector with the key is contained; otherwise, false.</returns>
            public bool ContainsKey(string key)
            {
                foreach (var vec in this)
                    if (vec.Key == key)
                        return true;
                return false;
            }
            /// <summary>
            /// Gets the vector with a key value
            /// </summary>
            /// <param name="key">Vector key</param>
            /// <returns>Vector instance</returns>
            public Vector GetVectorByKey(string key)
            {
                foreach (var vec in this)
                    if (vec.Key == key)
                        return vec;
                return null;
            }
        }

        /// <summary>
        /// Vector with one point to multiple other points
        /// </summary>
        public class Vector
        {
            /// <summary>
            /// Vector Key
            /// </summary>
            public string Key { get; set; }
            /// <summary>
            /// Vector destination values
            /// </summary>
            public List<string> Values { get; set; } = new List<string>();
            /// <summary>
            /// String representation of the instance
            /// </summary>
            /// <returns>String value</returns>
            public override string ToString()
                => string.Format("Key: {0}, Values: {1}", Key, string.Join(",", Values.ToArray()));
        }
        #endregion
    }
}
