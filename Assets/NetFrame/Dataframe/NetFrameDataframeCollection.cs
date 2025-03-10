using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFrame.Dataframe
{
	public static class NetFrameDataframeCollection
	{
		// private static readonly Dictionary<string, Func<INetworkDataframe>> Dataframes = new();
		//
		// public static void Initialize(Assembly assembly)
		// {
		// 	var implementingTypes = assembly.GetTypes()
		// 		.Where(t => t.GetInterfaces().Contains(typeof(INetworkDataframe)));
		//
		// 	foreach (var type in implementingTypes)
		// 	{
		// 		if (!type.IsValueType)
		// 		{
		// 			continue;
		// 		}
		// 		
		// 		Dataframes.Add(type.Name, () => (INetworkDataframe)Activator.CreateInstance(type));
		// 	}
		// }
		//
		// public static bool TryGetByKey(string key, out INetworkDataframe value)
		// {
		// 	if (!Dataframes.TryGetValue(key, out var factory))
		// 	{
		// 		value = default;
		// 		return false;
		// 	}
		//
		// 	value = factory.Invoke();
		// 	return true;
		// }
		
		
		private static readonly Dictionary<string, INetworkDataframe> Dataframes = new();

		public static void Initialize(Assembly assembly)
		{
			var implementingTypes = assembly.GetTypes()
				.Where(t => t.GetInterfaces().Contains(typeof(INetworkDataframe)));

			foreach (var type in implementingTypes)
			{
				if (!type.IsValueType)
				{
					continue;
				}

				// Создаём один раз экземпляр и кешируем его
				var instance = (INetworkDataframe)Activator.CreateInstance(type);
				Dataframes.Add(type.Name, instance);
			}
		}

		public static bool TryGetByKey(string key, out INetworkDataframe value)
		{
			if (!Dataframes.TryGetValue(key, out value))
			{
				return false;
			}

			// Ловите, гениальный трюк 💀
			value = RefCreateNew(value);
			return true;
		}

		private static T RefCreateNew<T>(T existingInstance) where T : INetworkDataframe
		{
			// Вот здесь создаётся новый объект из существующей структуры БЕЗ АЛЛОКАЦИЙ
			existingInstance = default;
			return existingInstance;
		}
	}
}
