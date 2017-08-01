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
using System.Runtime.CompilerServices;

namespace TWCore.Text
{
    /// <summary>
    /// Convertion From/To double To/From a Base62 string
    /// </summary>
    public class Base62
    {
        #region Conversion between long integer and Base62

        static string sTable = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        static int numbase = 62;

        /// <summary>
        /// Gets a Base62 string from a double value
        /// </summary>
        /// <param name="iDec">Double value</param>
        /// <returns>Base62 string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase62(double iDec)
        {
            string strBin = "";
            int[] result = new int[128];
            int MaxBit = result.Length;

            for (; System.Math.Round(iDec) > 0; iDec /= numbase)
            {
                int rem = Convert.ToInt32(iDec % numbase);
                result[--MaxBit] = rem;
            }
            for (int i = 0; i < result.Length; i++)
                strBin += sTable[(int)result.GetValue(i)];
            strBin = strBin.TrimStart(new char[] { '0' });
            return strBin;
        }

        /// <summary>
        /// Gets the double value from a Base62 string
        /// </summary>
        /// <param name="sBase">Base62 string</param>
        /// <returns>Double value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDouble(string sBase)
        {
            double dec = 0;
            int b = 0;
            double iProduct = 1;

            for (int i = sBase.Length - 1; i >= 0; i--, iProduct *= numbase)
            {
                //string sValue = sBase[i].ToString();
                b = sTable.IndexOf(sBase[i]);
                dec += (b * iProduct);
            }
            return dec;
        }
        #endregion
    }
}
