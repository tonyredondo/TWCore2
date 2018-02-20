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

using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable NotAccessedField.Local

namespace TWCore.Numerics
{
    /// <summary>
    /// Calculates the lexical permutations for the given collection of elements.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public class LexicalPermutation<T>
    {
        #region Private Fields
        /// <summary>
        /// The source collection of elements for which we are calculating the lexical permutation.
        /// </summary>
        private readonly List<T> _sourceList;

        /// <summary>
        /// Keep a list of pre-compouted factorial numbers to increase running time.
        /// </summary>
        private readonly long[] _knownFactorials;

        /// <summary>
        /// The number of elements in the Lexical permutation.
        /// </summary>
        private readonly long _nPermutations;

        /// <summary>
        /// The factoradic number corresponding to the current Lehmer code
        /// </summary>
        private readonly int[] _currentFactorialNumber;

        /// <summary>
        /// Keep track of which elements have been used in the Lehmer code reversal so far.
        /// </summary>
        private readonly bool[] _currentFactorialElements;
        #endregion

        #region .ctor
        /// <summary>
        /// Calculates the lexical permutations for the given collection of elements.
        /// </summary>
        /// <param name="sourceCollection">Source collection</param>
        public LexicalPermutation(IEnumerable<T> sourceCollection)
        {
            // NB: This conversion to a list also creates a orderring on the input collection
            _sourceList = sourceCollection.ToList();
            if (_sourceList.Count == 0) throw new ArgumentException("Source collection cannot contain 0 elements.");
            // Pre-allocate memory requirements to avoid garbage collector
            _currentFactorialNumber = new int[_sourceList.Count];
            _currentFactorialElements = new bool[_sourceList.Count];
            _knownFactorials = new long[_sourceList.Count];
            _nPermutations = GetFactorial(_sourceList.Count);
            // Pre-compute all factorials needed as optimization
            for (int i = 0; i < _sourceList.Count; i++)
                _knownFactorials[i] = GetFactorial(i);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Main method for getting an iterator (enumerator) for the permutation elements.
        /// </summary>
        /// <returns>Permutation enumerations</returns>
        public IEnumerable<List<T>> GetPermutationEnumerator()
        {
            var nPerm = GetFactorial(_sourceList.Count);
            for (long i = 0; i < nPerm; i++)
            {
                SetCurrentFactorialNumber(i);
                // currentFactorialNumber must now contain Lehmer code ... convert back to elements from source collection
                yield return ConvertFactoradicNumberToSourceCollection();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Calculates n!
        /// </summary>
        private static long GetFactorial(long n)
        {
            var retVal = 1;
            for (var i = 1; i <= n; i++) retVal *= i;
            return retVal;
        }

        /// <summary>
        /// Converts the input number into a factoradic number. This is stored in currentFactorialNumber.
        /// </summary>
        private void SetCurrentFactorialNumber(long nIn)
        {
            // Convert
            var currentNumber = nIn;
            for (var i = _currentFactorialNumber.Length - 1; i >= 0; i--)
            {
                _currentFactorialNumber[i] = (int)(currentNumber / _knownFactorials[i]);
                currentNumber %= _knownFactorials[i];
            }
        }

        /// <summary>
        /// Converts the current factoradic number into elements of the source collection, 
        /// using it as a Lehmer code.
        /// </summary>
        private List<T> ConvertFactoradicNumberToSourceCollection()
        {
            var returnList = new List<T>(_currentFactorialNumber.Length);
            // Clear search cache for reversing the Lehmer code
            for (int i = 0; i < _currentFactorialElements.Length; i++) _currentFactorialElements[i] = true;
            for (int i = _currentFactorialNumber.Length - 1; i >= 0; i--)
            {
                int countSmaller = 0;
                int countSmallerPosition = 0;
                int? lastSmallerPosition = null;
                while (countSmaller <= _currentFactorialNumber[i] || lastSmallerPosition == null)
                {
                    if (_currentFactorialElements[countSmallerPosition] && countSmaller <= _currentFactorialNumber[i])
                    {
                        countSmaller++;
                        lastSmallerPosition = countSmallerPosition;
                    }
                    countSmallerPosition++;
                }
                // lastSmallerPosition should always have a value at this point
                _currentFactorialElements[lastSmallerPosition.Value] = false;
                returnList.Add(_sourceList[lastSmallerPosition.Value]);
            }
            return returnList;
        }
        #endregion
    }
}
