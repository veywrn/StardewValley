using System;
using System.Collections.Concurrent;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerCache
	{
		private static readonly ConcurrentDictionary<Type, object> _typeCache = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, object> _typeCacheDeepTo = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, object> _typeCacheShallowTo = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, object> _structAsObjectCache = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Tuple<Type, Type>, object> _typeConvertCache = new ConcurrentDictionary<Tuple<Type, Type>, object>();

		public static object GetOrAddClass<T>(Type type, Func<Type, T> adder)
		{
			if (_typeCache.TryGetValue(type, out object value))
			{
				return value;
			}
			lock (type)
			{
				return _typeCache.GetOrAdd(type, (Type t) => adder(t));
			}
		}

		public static object GetOrAddDeepClassTo<T>(Type type, Func<Type, T> adder)
		{
			if (_typeCacheDeepTo.TryGetValue(type, out object value))
			{
				return value;
			}
			lock (type)
			{
				return _typeCacheDeepTo.GetOrAdd(type, (Type t) => adder(t));
			}
		}

		public static object GetOrAddShallowClassTo<T>(Type type, Func<Type, T> adder)
		{
			if (_typeCacheShallowTo.TryGetValue(type, out object value))
			{
				return value;
			}
			lock (type)
			{
				return _typeCacheShallowTo.GetOrAdd(type, (Type t) => adder(t));
			}
		}

		public static object GetOrAddStructAsObject<T>(Type type, Func<Type, T> adder)
		{
			if (_structAsObjectCache.TryGetValue(type, out object value))
			{
				return value;
			}
			lock (type)
			{
				return _structAsObjectCache.GetOrAdd(type, (Type t) => adder(t));
			}
		}

		public static T GetOrAddConvertor<T>(Type from, Type to, Func<Type, Type, T> adder)
		{
			return (T)_typeConvertCache.GetOrAdd(new Tuple<Type, Type>(from, to), (Tuple<Type, Type> tuple) => adder(tuple.Item1, tuple.Item2));
		}

		public static void ClearCache()
		{
			_typeCache.Clear();
			_typeCacheDeepTo.Clear();
			_typeCacheShallowTo.Clear();
			_structAsObjectCache.Clear();
			_typeConvertCache.Clear();
		}
	}
}
