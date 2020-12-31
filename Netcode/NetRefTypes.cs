using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Netcode
{
	internal static class NetRefTypes
	{
		private static Dictionary<string, Type> types = new Dictionary<string, Type>();

		public static Type ReadType(this BinaryReader reader)
		{
			Type genericType = reader.ReadGenericType();
			if (genericType == null || !genericType.IsGenericTypeDefinition)
			{
				return genericType;
			}
			int numArgs = genericType.GetGenericArguments().Length;
			Type[] arguments = new Type[numArgs];
			for (int i = 0; i < numArgs; i++)
			{
				arguments[i] = reader.ReadType();
			}
			return genericType.MakeGenericType(arguments);
		}

		private static Type ReadGenericType(this BinaryReader reader)
		{
			string typeName = reader.ReadString();
			if (typeName.Length == 0)
			{
				return null;
			}
			Type type = GetType(typeName);
			if (type == null)
			{
				throw new InvalidOperationException();
			}
			return type;
		}

		public static void WriteType(this BinaryWriter writer, Type type)
		{
			Type genericType = type;
			if (type != null && type.IsGenericType)
			{
				genericType = type.GetGenericTypeDefinition();
			}
			writer.WriteGenericType(genericType);
			if (!(genericType == null) && genericType.IsGenericType)
			{
				Type[] genericArguments = type.GetGenericArguments();
				foreach (Type argument in genericArguments)
				{
					writer.WriteType(argument);
				}
			}
		}

		private static void WriteGenericType(this BinaryWriter writer, Type type)
		{
			if (type == null)
			{
				writer.Write("");
			}
			else
			{
				writer.Write(type.FullName);
			}
		}

		public static void WriteTypeOf<T>(this BinaryWriter writer, T value)
		{
			if (value == null)
			{
				writer.WriteType(null);
			}
			else
			{
				writer.WriteType(value.GetType());
			}
		}

		private static Type GetType(string typeName)
		{
			if (types.TryGetValue(typeName, out Type type2))
			{
				return type2;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				type2 = assemblies[i].GetType(typeName);
				if (type2 != null)
				{
					types[typeName] = type2;
					return type2;
				}
			}
			return null;
		}
	}
}
