using System;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class ReflectionHelper
	{
		public static bool IsEnum(this Type t)
		{
			return t.IsEnum;
		}

		public static bool IsValueType(this Type t)
		{
			return t.IsValueType;
		}

		public static bool IsClass(this Type t)
		{
			return t.IsClass;
		}

		public static Type BaseType(this Type t)
		{
			return t.BaseType;
		}

		public static FieldInfo[] GetAllFields(this Type t)
		{
			return t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static PropertyInfo[] GetPublicProperties(this Type t)
		{
			return t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		}

		public static FieldInfo[] GetDeclaredFields(this Type t)
		{
			return t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static ConstructorInfo[] GetPrivateConstructors(this Type t)
		{
			return t.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static ConstructorInfo[] GetPublicConstructors(this Type t)
		{
			return t.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
		}

		public static MethodInfo GetPrivateMethod(this Type t, string methodName)
		{
			return t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static MethodInfo GetMethod(this Type t, string methodName)
		{
			return t.GetMethod(methodName);
		}

		public static MethodInfo GetPrivateStaticMethod(this Type t, string methodName)
		{
			return t.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
		}

		public static FieldInfo GetPrivateField(this Type t, string fieldName)
		{
			return t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static Type[] GenericArguments(this Type t)
		{
			return t.GetGenericArguments();
		}
	}
}
