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
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Net;

namespace TWCore
{
	/// <summary>
	/// General Extensions
	/// </summary>
	public static partial class Extensions
	{
		#region Attributes
		/// <summary>
		/// Gets an attribute from an object
		/// </summary>
		/// <typeparam name="T">Attribute type</typeparam>
		/// <param name="obj">Object to get the attribute from</param>
		/// <returns>Attribute</returns>
		public static T GetAttribute<T>(this object obj) where T : class
		{
			var objType = obj.GetType();
			var objTypeInfo = objType.GetTypeInfo();
			object[] attrs;
			if (objTypeInfo.IsEnum)
				attrs = objType.GetRuntimeField(obj.ToString()).GetCustomAttributes(true);
			else if (obj as PropertyInfo != null)
				attrs = ((PropertyInfo)obj).GetCustomAttributes(true);
			else if (obj as FieldInfo != null)
				attrs = ((FieldInfo)obj).GetCustomAttributes(true);
			else
				attrs = objTypeInfo.GetCustomAttributes(true);
			foreach (var item in attrs)
				if (item as T != null)
					return item as T;
			return null;
		}
		#endregion

		#region To/From Dictionary
		/// <summary>
		/// Gets a dictionary form an object converting all the properties to key/value pairs.
		/// </summary>
		/// <param name="source">Source object</param>
		/// <returns>IDictionary instance</returns>
		public static IDictionary<string, object> ToDictionary(this object source)
		{
			if (source != null)
			{
				var sType = source.GetType();
				var sTypeInfo = sType.GetTypeInfo();
				if (!sTypeInfo.IsValueType)
				{
					var dct = new Dictionary<string, object>();

					var props = sType.GetRuntimeProperties();
					foreach (var prop in props)
					{
						if (prop.CanRead)
						{
							var value = prop.GetValue(source, null);
							dct.Add(prop.Name, value);
						}
					}
					return dct;
				}
			}
			return null;
		}

		/// <summary>
		/// Sets the properties values of an object from a dictionary with key/value pairs.
		/// </summary>
		/// <param name="source">Source dictionary with values</param>
		/// <param name="target">Target object</param>
		public static void FromDictionary(this object target, IDictionary<string, object> source)
		{
			if (source != null && target != null)
			{
				var sType = target.GetType();
				var sTypeInfo = sType.GetTypeInfo();
				if (!sTypeInfo.IsValueType)
				{
					foreach (var item in source)
					{
						var prop = sType.GetRuntimeProperty(item.Key);
						if (prop != null && prop.CanWrite)
							prop.SetValue(target, item.Value, null);
					}
				}
			}
		}
		#endregion


		#region String Dictionary Extensions
		/// <summary>
		/// Convert object properties to a Dictionary
		/// </summary>
		/// <param name="source">Object source</param>
		/// <param name="objectToStringFunction">Function to convert an object property to string</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, string> ToStringDictionary(this object source, Func<object, Type, string> objectToStringFunction)
		{
			if (source != null)
			{
				var sType = source.GetType();
				var sTypeInfo = sType.GetTypeInfo();

				if (!sTypeInfo.IsValueType)
				{
					var dct = new Dictionary<string, string>();

					var props = sType.GetRuntimeProperties();
					foreach (var prop in props)
					{
						if (prop.CanRead)
						{
							var value = prop.GetValue(source, null);
							var nValue = objectToStringFunction(value, prop.PropertyType);
							dct[prop.Name] = nValue;
						}
					}
					return dct;
				}
			}
			return null;
		}
		/// <summary>
		/// Sets the object properties with values from the Dictionary
		/// </summary>
		/// <param name="source">Source Dictionary</param>
		/// <param name="target">Object Target</param>
		/// <param name="stringToObjectFunction">Function to convert a string value to an object value</param>
		public static void FromStringDictionary(this object target, Dictionary<string, string> source, Func<string, Type, object> stringToObjectFunction)
		{
			if (source != null && target != null)
			{
				var sType = target.GetType();
				var sTypeInfo = sType.GetTypeInfo();
				if (!sTypeInfo.IsValueType)
				{
					foreach (var item in source)
					{
						var prop = sType.GetRuntimeProperty(item.Key);
						if (prop != null && prop.CanWrite)
						{
							var nValue = stringToObjectFunction(item.Value, prop.PropertyType);
							prop.SetValue(target, nValue, null);
						}
					}
				}
			}
		}
		/// <summary>
		/// Sets the object properties with values from the Dictionary
		/// </summary>
		/// <param name="source">Source Dictionary</param>
		/// <param name="target">Object Target</param>
		public static void FromStringDictionary(this object target, Dictionary<string, string> source)
		{
			FromStringDictionary(target, source, (strValue, type) =>
			{
				var typeInfo = type.GetTypeInfo();
				if (typeInfo.IsValueType)
					return strValue.ParseTo(type, Activator.CreateInstance(type), null);
				else
					return strValue.ParseTo(type, (object)null, null);
			});
		}
		#endregion

		#region Task extensions
		/// <summary>
		/// Waits for task finalization and returns the result value
		/// </summary>
		/// <typeparam name="T">Type of task response</typeparam>
		/// <param name="task">Task source object</param>
		/// <returns>Response of the task completation</returns>
		public static T WaitAndResults<T>(this Task<T> task)
		{
			if (task == null)
				return default(T);
			try
			{
				task.Wait();
				return task.Result;
			}
			catch (AggregateException ex)
			{
				if (ex.InnerExceptions.Count == 1)
				{
					Core.Log.Write(ex.InnerExceptions[0]);
					ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
				}
				Core.Log.Write(ex);
				throw;
			}
			catch (Exception ex)
			{
				Core.Log.Write(ex);
				throw;
			}
		}
		/// <summary>
		/// Waits for task finalization and returns the result value
		/// </summary>
		/// <typeparam name="T">Type of task response</typeparam>
		/// <param name="task">Task source object</param>
		/// <param name="millisecondsTimeout">Milliseconds for waiting the task to complete</param>
		/// <returns>Response of the task completation</returns>
		public static T WaitAndResults<T>(this Task<T> task, int millisecondsTimeout)
		{
			if (task == null)
				return default(T);
			try
			{
				if (task.Wait(millisecondsTimeout))
					return task.Result;
				else
					return default(T);
			}
			catch (AggregateException ex)
			{
				if (ex.InnerExceptions.Count == 1)
				{
					Core.Log.Write(ex.InnerExceptions[0]);
					ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
				}
				Core.Log.Write(ex);
				throw;
			}
			catch (Exception ex)
			{
				Core.Log.Write(ex);
				throw;
			}
		}
		/// <summary>
		/// Waits for task finalization and returns the result value
		/// </summary>
		/// <typeparam name="T">Type of task response</typeparam>
		/// <param name="task">Task source object</param>
		/// <param name="timeout">TimeSpan for waiting the task to complete</param>
		/// <returns>Response of the task completation</returns>
		public static T WaitAndResults<T>(this Task<T> task, TimeSpan timeout)
		{
			if (task == null)
				return default(T);
			try
			{
				if (task.Wait(timeout))
					return task.Result;
				else
					return default(T);
			}
			catch (AggregateException ex)
			{
				if (ex.InnerExceptions.Count == 1)
				{
					Core.Log.Write(ex.InnerExceptions[0]);
					ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
				}
				Core.Log.Write(ex);
				throw;
			}
			catch (Exception ex)
			{
				Core.Log.Write(ex);
				throw;
			}
		}
		/// <summary>
		/// Waits for task finalization and returns the result value
		/// </summary>
		/// <typeparam name="T">Type of task response</typeparam>
		/// <param name="task">Task source object</param>
		/// <param name="cancellationToken">Cancellation token on the Wait for the task to complete</param>
		/// <returns>Response of the task completation</returns>
		public static T WaitAndResults<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			if (task == null)
				return default(T);
			try
			{
				task.Wait(cancellationToken);
				return task.Result;
			}
			catch (AggregateException ex)
			{
				if (ex.InnerExceptions.Count == 1)
				{
					Core.Log.Write(ex.InnerExceptions[0]);
					ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
				}
				Core.Log.Write(ex);
				throw;
			}
			catch (Exception ex)
			{
				Core.Log.Write(ex);
				throw;
			}
		}
		/// <summary>
		/// Wait for task avoiding deadlocks
		/// </summary>
		/// <typeparam name="T">Task type</typeparam>
		/// <param name="task">Rask</param>
		/// <returns>Task complete</returns>
		public static T WaitAsync<T>(this Task<T> task)
		{
			var wait = new ManualResetEventSlim(false);
			var continuation = task.ContinueWith(_ =>
			{
				wait.Set();
				return _.Result;
			});
			wait.Wait();
			return continuation.Result;
		}
		/// <summary>
		/// Wait for task avoiding deadlocks
		/// </summary>
		/// <param name="task">Rask</param>
		/// <returns>Task complete</returns>
		public static void WaitAsync(this Task task)
		{
			var wait = new ManualResetEventSlim(false);
            var continuation = task.ContinueWith(_ =>
            {
                wait.Set();
            });
            wait.Wait();
		}
		/// <summary>
		/// Handles a cancellation Token for a task without support
		/// </summary>
		/// <param name="asyncTask">Async task without cancellation token</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Task with cancellation token support</returns>
		public async static Task<TResult> HandleCancellationAsync<TResult>(this Task<TResult> asyncTask, CancellationToken cancellationToken)
		{
			// Create another task that completes as soon as cancellation is requested.
			var tcs = new TaskCompletionSource<TResult>();
			using (IDisposable registration = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
			{
				var cancellationTask = tcs.Task;
				// Create a task that completes when either the async operation completes,
				// or cancellation is requested.
				var readyTask = await Task.WhenAny(asyncTask, cancellationTask);

				// In case of cancellation, register a continuation to observe any unhandled 
				// exceptions from the asynchronous operation (once it completes).
				// In .NET 4.0, unobserved task exceptions would terminate the process.
				if (readyTask == cancellationTask)
					await asyncTask.ContinueWith(_ => asyncTask.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

				return await readyTask;
			}
		}
		/// <summary>
		/// Handles a cancellation Token for a task without support
		/// </summary>
		/// <param name="asyncTask">Async task without cancellation token</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Task with cancellation token support</returns>
		public async static Task HandleCancellationAsync(this Task asyncTask, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<object>();
			using (IDisposable registration = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
			{
				var cancellationTask = tcs.Task;
				// Create a task that completes when either the async operation completes,
				// or cancellation is requested.
				var readyTask = await Task.WhenAny(asyncTask, cancellationTask);

				// In case of cancellation, register a continuation to observe any unhandled 
				// exceptions from the asynchronous operation (once it completes).
				// In .NET 4.0, unobserved task exceptions would terminate the process.
				if (readyTask == cancellationTask)
					await asyncTask.ContinueWith(_ => asyncTask.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

				await readyTask;
			}
		}
		/// <summary>
		/// Create a Task to await the cancellation of the token
		/// </summary>
		/// <param name="cancellationToken">Cancellation token instance</param>
		/// <returns>Task to await the cancellation</returns>
		public async static Task WhenCanceledAsync(this CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested) return;
			var tcs = new TaskCompletionSource<object>();
			using (IDisposable registration = cancellationToken.Register(() => tcs.TrySetResult(null), useSynchronizationContext: false))
				await tcs.Task;
		}

		delegate object InvokeDelegate(Delegate @delegate, params object[] args);
		/// <summary>
		/// Invoke a delegate as an Async Task
		/// </summary>
		/// <param name="delegate">Delegate</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <param name="args">Delegate arguments</param>
		/// <returns>Task with cancellation token support</returns>
		public static Task<object> DynamicInvokeAsync(this Delegate @delegate, CancellationToken cancellationToken, params object[] args)
		{
			var nDelegate = new InvokeDelegate((_del, _args) => _del.DynamicInvoke(_args));
			return Task.Factory.FromAsync(nDelegate.BeginInvoke(@delegate, args, null, null), nDelegate.EndInvoke).HandleCancellationAsync<object>(cancellationToken);
		}
		/// <summary>
		/// Invoke a delegate as an Async Task
		/// </summary>
		/// <param name="delegate">Delegate</param>
		/// <param name="args">Delegate arguments</param>
		/// <returns>Task with cancellation token support</returns>
		public static Task<object> DynamicInvokeAsync(this Delegate @delegate, params object[] args)
		{
			var nDelegate = new InvokeDelegate((_del, _args) => _del.DynamicInvoke(_args));
			return Task.Factory.FromAsync(nDelegate.BeginInvoke(@delegate, args, null, null), nDelegate.EndInvoke);
		}
		#endregion

		/// <summary>
		/// Gets the assembly of the object type
		/// </summary>
		/// <param name="obj">Object of the type of the assembly</param>
		/// <returns>Assembly where type is contained</returns>
		public static Assembly GetAssembly(this object obj)
			=> obj?.GetType().GetTypeInfo().Assembly;
		/// <summary>
		/// Gets the string from a byte array using an Encoding
		/// </summary>
		/// <param name="encoding">Encoding used to get the string</param>
		/// <param name="bytes">Byte array with the bytes to decode</param>
		/// <returns>A string value with the result of the encoding</returns>
		public static string GetString(this Encoding encoding, byte[] bytes)
			=> encoding.GetString(bytes, 0, bytes.Length);
		/// <summary>
		/// Gets the string from a byte array using an Encoding
		/// </summary>
		/// <param name="encoding">Encoding used to get the string</param>
		/// <param name="bytes">Byte array with the bytes to decode</param>
		/// <returns>A string value with the result of the encoding</returns>
		public static string GetString(this Encoding encoding, SubArray<byte> subArray)
			=> encoding.GetString(subArray.Array, subArray.Offset, subArray.Count);

		/// <summary>
		/// Gets the Timespan format of the DateTime object.
		/// </summary>
		/// <param name="time">DateTime to format</param>
		/// <returns>Formatted Datetime string</returns>
		public static string GetTimeSpanFormat(this DateTime time)
		{
			char[] dateData = new char[21];
			dateData[0] = (char)(time.Day / 10 + '0');
			dateData[1] = (char)(time.Day % 10 + '0');
			dateData[2] = '/';
			dateData[3] = (char)(time.Month / 10 + '0');
			dateData[4] = (char)(time.Month % 10 + '0');
			dateData[5] = '/';
			dateData[6] = (char)(time.Year / 10 % 10 + '0');
			dateData[7] = (char)(time.Year % 10 + '0');
			dateData[8] = ' ';
			dateData[9] = (char)(time.Hour / 10 + '0');
			dateData[10] = (char)(time.Hour % 10 + '0');
			dateData[11] = ':';
			dateData[12] = (char)(time.Minute / 10 + '0');
			dateData[13] = (char)(time.Minute % 10 + '0');
			dateData[14] = ':';
			dateData[15] = (char)(time.Second / 10 + '0');
			dateData[16] = (char)(time.Second % 10 + '0');
			dateData[17] = '.';
			dateData[18] = (char)(time.Millisecond / 100 + '0');
			dateData[19] = (char)(time.Millisecond / 10 % 10 + '0');
			dateData[20] = (char)(time.Millisecond % 10 + '0');
			return new string(dateData);
		}

		public static async Task ConnectHostAsync(this TcpClient client, string host, int port)
		{
			var ipAddress = await IpHelper.GetIpFromHostAsync(host).ConfigureAwait(false);
			await client.ConnectAsync(ipAddress, port).ConfigureAwait(false);
		}

		#region WaitHandles Extensions
		/// <summary>
		/// WaitOne with cancellationToken
		/// </summary>
		/// <returns><c>true</c>, if one was waited, <c>false</c> otherwise.</returns>
		/// <param name="handle">Handle.</param>
		/// <param name="millisecondsTimeout">Milliseconds timeout.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public static bool WaitOne(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			int n = WaitHandle.WaitAny(new[] { handle, cancellationToken.WaitHandle }, millisecondsTimeout);
			switch (n)
			{
				case WaitHandle.WaitTimeout:
					return false;
				case 0:
					return true;
				default:
					cancellationToken.ThrowIfCancellationRequested();
					return false;
			}
		}
		/// <summary>
		/// WaitOne with cancellationToken
		/// </summary>
		/// <returns><c>true</c>, if one was waited, <c>false</c> otherwise.</returns>
		/// <param name="handle">Handle.</param>
		/// <param name="timeout">Timeout.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public static bool WaitOne(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
			=> handle.WaitOne((int)timeout.TotalMilliseconds, cancellationToken);
		/// <summary>
		/// WaitOne with cancellationToken
		/// </summary>
		/// <returns><c>true</c>, if one was waited, <c>false</c> otherwise.</returns>
		/// <param name="handle">Handle.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public static bool WaitOne(this WaitHandle handle, CancellationToken cancellationToken)
			=> handle.WaitOne(Timeout.Infinite, cancellationToken);

		/// <summary>
		/// WaitOne Async
		/// </summary>
		/// <returns>The async Task with the result.</returns>
		/// <param name="handle">Handle.</param>
		/// <param name="millisecondsTimeout">Milliseconds timeout.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			RegisteredWaitHandle registeredHandle = null;
			CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);
			try
			{
				var tcs = new TaskCompletionSource<bool>();
				registeredHandle = ThreadPool.RegisterWaitForSingleObject(
					handle,
					(state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut),
					tcs,
					millisecondsTimeout,
					true);
				tokenRegistration = cancellationToken.Register(
					state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
					tcs);
				return await tcs.Task;
			}
			finally
			{
				if (registeredHandle != null)
					registeredHandle.Unregister(null);
				tokenRegistration.Dispose();
			}
		}
		/// <summary>
		/// WaitOne Async
		/// </summary>
		/// <returns>The async Task with the result.</returns>
		/// <param name="handle">Handle.</param>
		/// <param name="timeout">Timeout.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
			=> handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
		/// <summary>
		/// WaitOne Async
		/// </summary>
		/// <returns>The async Task with the result.</returns>
		/// <param name="handle">Handle.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken)
			=> handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
		#endregion
	}
}
