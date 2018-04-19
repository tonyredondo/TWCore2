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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Cache.Client;
using TWCore.Cache.Client.Configuration;
using TWCore.Serialization;
using TWCore.Settings;
// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Services
{
	/// <summary>
	/// Core Services Extensions
	/// </summary>
	public static class CoreServicesExtensions
	{
	    private static CacheSettings _settings;
	    private static bool _init;

		#region Init
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Init()
		{
			if (_init) return;
			_init = true;

			var cacheSettings = Core.GetSettings<CacheConfigurationSettings>();
			if (string.IsNullOrEmpty(cacheSettings.ConfigFile)) return;

			//Cache configuration
			var cachesConfigFile = cacheSettings.ConfigFile;
			cachesConfigFile = cachesConfigFile.Replace("{EnvironmentName}", Core.EnvironmentName);
			cachesConfigFile = cachesConfigFile.Replace("{MachineName}", Core.MachineName);
			cachesConfigFile = cachesConfigFile.Replace("{ApplicationName}", Core.ApplicationName);
            cachesConfigFile = Factory.ResolveLowLowPath(cachesConfigFile);
			Core.Log.InfoBasic("Loading cache client configuration: {0}", cachesConfigFile);

			try
			{
				var value = cachesConfigFile.ReadTextFromFile();
				value = Core.ReplaceSettingsTemplate(value);
				var serializer = SerializerManager.GetByFileName<ITextSerializer>(cachesConfigFile);
				_settings = serializer.DeserializeFromString<CacheSettings>(value);
			}
			catch (Exception ex)
			{
				throw new Exception($"The Cache config file: {cachesConfigFile} can't be deserialized.", ex);
			}

			if (_settings != null) return;
			Core.Log.Warning("The Cache configuration file is null or empty.");
		}
		/// <summary>
		/// Sets the default cache client settings
		/// </summary>
		/// <param name="services">CoreServices instance</param>
		/// <param name="settings">CacheSettings instance</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetDefaultCacheClientSettings(this CoreServices services, CacheSettings settings)
		{
			_init = true;
			_settings = settings;
		}
		#endregion

		#region GetCacheClient
		/// <summary>
		/// Gets a cache client instance
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<CacheClientPoolAsync> GetCacheClientAsync(this CoreServices services, string name)
		{
			Init();
			Ensure.ReferenceNotNull(_settings, "The client configuration settings is null, check if the cache configuration file is setted or is previously setted with the SetDefaultCacheClientSettings method.");
			return await _settings.GetCacheClientAsync(name).ConfigureAwait(false);
		}
		#endregion

		#region Nested Settings Type
		[SettingsContainer("Core.Services.Cache")]
		private class CacheConfigurationSettings : SettingsBase
		{
			public string ConfigFile { get; set; }
		}
		#endregion
	}
}
