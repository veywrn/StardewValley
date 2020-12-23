using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Force.DeepCloner.Helpers
{
	public abstract class ShallowObjectCloner
	{
		private class ShallowSafeObjectCloner : ShallowObjectCloner
		{
			private static readonly Func<object, object> _cloneFunc;

			static ShallowSafeObjectCloner()
			{
				MethodInfo methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone");
				ParameterExpression p = Expression.Parameter(typeof(object));
				_cloneFunc = Expression.Lambda<Func<object, object>>(Expression.Call(p, methodInfo), new ParameterExpression[1]
				{
					p
				}).Compile();
			}

			protected override object DoCloneObject(object obj)
			{
				return _cloneFunc(obj);
			}
		}

		private static readonly ShallowObjectCloner _unsafeInstance;

		private static ShallowObjectCloner _instance;

		protected abstract object DoCloneObject(object obj);

		public static object CloneObject(object obj)
		{
			return _instance.DoCloneObject(obj);
		}

		internal static bool IsSafeVariant()
		{
			return _instance is ShallowSafeObjectCloner;
		}

		static ShallowObjectCloner()
		{
			_unsafeInstance = GenerateUnsafeCloner();
			_instance = _unsafeInstance;
			try
			{
				_instance.DoCloneObject(new object());
			}
			catch (Exception)
			{
				_instance = new ShallowSafeObjectCloner();
			}
		}

		internal static void SwitchTo(bool isSafe)
		{
			DeepClonerCache.ClearCache();
			if (isSafe)
			{
				_instance = new ShallowSafeObjectCloner();
			}
			else
			{
				_instance = _unsafeInstance;
			}
		}

		private static ShallowObjectCloner GenerateUnsafeCloner()
		{
			TypeBuilder builder = TypeCreationHelper.GetModuleBuilder().DefineType("ShallowSafeObjectClonerImpl", TypeAttributes.Public, typeof(ShallowObjectCloner));
			ILGenerator iLGenerator = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes).GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, typeof(ShallowObjectCloner).GetPrivateConstructors()[0]);
			iLGenerator.Emit(OpCodes.Ret);
			ILGenerator iLGenerator2 = builder.DefineMethod("DoCloneObject", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(object), new Type[1]
			{
				typeof(object)
			}).GetILGenerator();
			iLGenerator2.Emit(OpCodes.Ldarg_1);
			iLGenerator2.Emit(OpCodes.Call, typeof(object).GetPrivateMethod("MemberwiseClone"));
			iLGenerator2.Emit(OpCodes.Ret);
			return (ShallowObjectCloner)Activator.CreateInstance(builder.CreateType());
		}
	}
}
