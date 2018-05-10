using TWCore;
using TWCore.Collections;
using TWCore.Services;
using System;
using System.Threading.Tasks;
// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
	public class CacheTimeoutCollectionTest : ContainerParameterService
	{
		public CacheTimeoutCollectionTest() : base("cachetimeoutcollectiontest", "Cache timeout collection test") { }
		protected override void OnHandler(ParameterHandlerInfo info)
		{
			Core.Log.Warning("Starting Cache Timeout Collection Test");

			var cacheTimeoutCollection = CacheTimeoutCollection<string, object>.CreateFromLRU2Q(100);
			cacheTimeoutCollection.OnItemTimeout += CacheTimeoutCollection_OnItemTimeout;
			cacheTimeoutCollection.OnRemovedByPaging += CacheTimeoutCollection_OnRemovedByPaging;
			for (var i = 0; i < 1000; i++)
			{
				cacheTimeoutCollection.TryAdd("Key - " + i, null, TimeSpan.FromSeconds(5));
			}
			Task.Delay(10000).WaitAsync();
		}

		void CacheTimeoutCollection_OnItemTimeout(object sender, CacheTimeoutCollection<string, object>.TimeOutEventArgs e)
		{
			Core.Log.Warning("Cache Item Removed By Timeout: {0}", e.Key);
		}

		void CacheTimeoutCollection_OnRemovedByPaging(string key, object value)
		{
			Core.Log.Warning("Cache Item Removed By Paging: {0}", key);
		}

	}
}