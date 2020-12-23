using System;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerMsilHelper
	{
		public static bool IsConstructorDoNothing(Type type, ConstructorInfo constructor)
		{
			if (constructor == null)
			{
				return false;
			}
			try
			{
				if (type.IsGenericType || type.IsContextful || type.IsCOMObject || type.Assembly.IsDynamic)
				{
					return false;
				}
				MethodBody methodBody = constructor.GetMethodBody();
				if (methodBody == null)
				{
					return false;
				}
				byte[] ilAsByteArray = methodBody.GetILAsByteArray();
				if (ilAsByteArray.Length == 7 && ilAsByteArray[0] == 2 && ilAsByteArray[1] == 40 && ilAsByteArray[6] == 42 && type.Module.ResolveMethod(BitConverter.ToInt32(ilAsByteArray, 2)) == typeof(object).GetConstructor(Type.EmptyTypes))
				{
					return true;
				}
				if (ilAsByteArray.Length == 1 && ilAsByteArray[0] == 42)
				{
					return true;
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
