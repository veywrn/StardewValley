using Force.DeepCloner.Helpers;
using System;
using System.Security;

namespace Force.DeepCloner
{
	public static class DeepClonerExtensions
	{
		public static T DeepClone<T>(this T obj)
		{
			return DeepClonerGenerator.CloneObject(obj);
		}

		public static TTo DeepCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo) where TTo : class, TFrom
		{
			return (TTo)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, isDeep: true);
		}

		public static TTo ShallowCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo) where TTo : class, TFrom
		{
			return (TTo)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, isDeep: false);
		}

		public static T ShallowClone<T>(this T obj)
		{
			return ShallowClonerGenerator.CloneObject(obj);
		}

		static DeepClonerExtensions()
		{
			if (!PermissionCheck())
			{
				throw new SecurityException("DeepCloner should have enough permissions to run. Grant FullTrust or Reflection permission.");
			}
		}

		private static bool PermissionCheck()
		{
			try
			{
				new object().ShallowClone();
			}
			catch (VerificationException)
			{
				return false;
			}
			catch (MemberAccessException)
			{
				return false;
			}
			return true;
		}
	}
}
