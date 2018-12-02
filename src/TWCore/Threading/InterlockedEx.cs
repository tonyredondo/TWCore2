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

using System.Threading;

namespace TWCore.Threading
{
	/// <summary>
	/// Interlocked extensions
	/// </summary>
	public static class InterlockedEx
	{
		/// <summary>
		/// Adds two doubles and replaces the first integer with the sum, as an atomic operation.
		/// </summary>
		/// <returns>The new value stored at location1.</returns>
		/// <param name="location1">A variable containing the first value to be added. The sum of the two values is stored in location1.</param>
		/// <param name="value">The value to be added to the integer at location1.</param>
		public static double Add(ref double location1, double value)
		{
			double newCurrentValue = location1; // non-volatile read, so may be stale
			while (true)
			{
				double currentValue = newCurrentValue;
				double newValue = currentValue + value;
				newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
				if (newCurrentValue == currentValue)
					return newValue;
			}
		}
		/// <summary>
		/// Adds two float and replaces the first integer with the sum, as an atomic operation.
		/// </summary>
		/// <returns>The new value stored at location1.</returns>
		/// <param name="location1">A variable containing the first value to be added. The sum of the two values is stored in location1.</param>
		/// <param name="value">The value to be added to the integer at location1.</param>
		public static float Add(ref float location1, float value)
		{
			float newCurrentValue = location1; // non-volatile read, so may be stale
			while (true)
			{
				float currentValue = newCurrentValue;
				float newValue = currentValue + value;
				newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
				if (newCurrentValue == currentValue)
					return newValue;
			}
		}
	}
}