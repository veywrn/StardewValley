using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerSafeTypes
	{
		internal static readonly ConcurrentDictionary<Type, bool> KnownTypes;

		static DeepClonerSafeTypes()
		{
			KnownTypes = new ConcurrentDictionary<Type, bool>();
			Type[] array = new Type[20]
			{
				typeof(byte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal),
				typeof(char),
				typeof(string),
				typeof(bool),
				typeof(DateTime),
				typeof(IntPtr),
				typeof(UIntPtr),
				typeof(Guid),
				Type.GetType("System.RuntimeType"),
				Type.GetType("System.RuntimeTypeHandle"),
				typeof(DBNull)
			};
			foreach (Type x in array)
			{
				KnownTypes.TryAdd(x, value: true);
			}
		}

		private static bool CanReturnSameType(Type type, HashSet<Type> processingTypes)
		{
			if (KnownTypes.TryGetValue(type, out bool isSafe))
			{
				return isSafe;
			}
			if (type.IsEnum() || type.IsPointer)
			{
				KnownTypes.TryAdd(type, value: true);
				return true;
			}
			if (type.FullName.StartsWith("System.Runtime.Remoting.") && type.Assembly == typeof(CustomErrorsModes).Assembly)
			{
				KnownTypes.TryAdd(type, value: true);
				return true;
			}
			if (type.FullName.StartsWith("System.Reflection.") && type.Assembly == typeof(PropertyInfo).Assembly)
			{
				KnownTypes.TryAdd(type, value: true);
				return true;
			}
			if (type.IsSubclassOf(typeof(CriticalFinalizerObject)))
			{
				KnownTypes.TryAdd(type, value: true);
				return true;
			}
			if (type.IsCOMObject)
			{
				KnownTypes.TryAdd(type, value: true);
				return true;
			}
			if (!type.IsValueType())
			{
				KnownTypes.TryAdd(type, value: false);
				return false;
			}
			if (processingTypes == null)
			{
				processingTypes = new HashSet<Type>();
			}
			processingTypes.Add(type);
			List<FieldInfo> fi = new List<FieldInfo>();
			Type tp = type;
			do
			{
				fi.AddRange(tp.GetAllFields());
				tp = tp.BaseType();
			}
			while (tp != null);
			foreach (FieldInfo item in fi)
			{
				Type fieldType = item.FieldType;
				if (!processingTypes.Contains(fieldType) && !CanReturnSameType(fieldType, processingTypes))
				{
					KnownTypes.TryAdd(type, value: false);
					return false;
				}
			}
			KnownTypes.TryAdd(type, value: true);
			return true;
		}

		public static bool CanReturnSameObject(Type type)
		{
			return CanReturnSameType(type, null);
		}
	}
}
