using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerMsilGenerator
	{
		private static int _methodCounter;

		internal static object GenerateClonerInternal(Type realType, bool asObject)
		{
			Type methodType = asObject ? typeof(object) : realType;
			ModuleBuilder mb = TypeCreationHelper.GetModuleBuilder();
			DynamicMethod dt = new DynamicMethod("DeepObjectCloner_" + realType.Name + "_" + Interlocked.Increment(ref _methodCounter), methodType, new Type[2]
			{
				methodType,
				typeof(DeepCloneState)
			}, mb, skipVisibility: true);
			dt.InitLocals = false;
			GenerateProcessMethod(dt.GetILGenerator(), realType, asObject && realType.IsValueType);
			Type funcType = typeof(Func<, , >).MakeGenericType(methodType, typeof(DeepCloneState), methodType);
			return dt.CreateDelegate(funcType);
		}

		private static void GenerateProcessMethod(ILGenerator il, Type type, bool unboxStruct)
		{
			if (type.IsArray)
			{
				GenerateProcessArrayMethod(il, type);
				return;
			}
			if (type.FullName != null && type.FullName.StartsWith("System.Tuple`"))
			{
				Type[] genericArguments = type.GenericArguments();
				if (genericArguments.Length < 10 && genericArguments.All(DeepClonerSafeTypes.CanReturnSameObject))
				{
					GenerateProcessTupleMethod(il, type);
					return;
				}
			}
			LocalBuilder typeLocal = il.DeclareLocal(type);
			LocalBuilder structLoc = null;
			bool isGoodConstructor = false;
			if (!type.IsValueType)
			{
				ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
				isGoodConstructor = DeepClonerMsilHelper.IsConstructorDoNothing(type, constructor);
				if (isGoodConstructor)
				{
					il.Emit(OpCodes.Newobj, constructor);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Call, typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic));
				}
				il.Emit(OpCodes.Stloc, typeLocal);
			}
			else if (unboxStruct)
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Unbox_Any, type);
				structLoc = il.DeclareLocal(type);
				il.Emit(OpCodes.Dup);
				il.Emit(OpCodes.Stloc, structLoc);
				il.Emit(OpCodes.Stloc, typeLocal);
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Stloc, typeLocal);
			}
			if (type.IsClass)
			{
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloc, typeLocal);
				il.Emit(OpCodes.Call, typeof(DeepCloneState).GetMethod("AddKnownRef"));
			}
			List<FieldInfo> fi = new List<FieldInfo>();
			Type tp = type;
			while (!(tp == typeof(ContextBoundObject)))
			{
				fi.AddRange(tp.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
				tp = tp.BaseType;
				if (!(tp != null))
				{
					break;
				}
			}
			foreach (FieldInfo fieldInfo in fi)
			{
				if (DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
				{
					if (isGoodConstructor)
					{
						il.Emit(type.IsClass ? OpCodes.Ldloc : OpCodes.Ldloca_S, typeLocal);
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Ldfld, fieldInfo);
						il.Emit(OpCodes.Stfld, fieldInfo);
					}
				}
				else
				{
					il.Emit(type.IsClass ? OpCodes.Ldloc : OpCodes.Ldloca_S, typeLocal);
					if (structLoc == null)
					{
						il.Emit(OpCodes.Ldarg_0);
					}
					else
					{
						il.Emit(OpCodes.Ldloc, structLoc);
					}
					il.Emit(OpCodes.Ldfld, fieldInfo);
					il.Emit(OpCodes.Ldarg_1);
					MethodInfo methodInfo = fieldInfo.FieldType.IsValueType ? typeof(DeepClonerGenerator).GetMethod("CloneStructInternal", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(fieldInfo.FieldType) : typeof(DeepClonerGenerator).GetMethod("CloneClassInternal", BindingFlags.Static | BindingFlags.NonPublic);
					il.Emit(OpCodes.Call, methodInfo);
					il.Emit(OpCodes.Stfld, fieldInfo);
				}
			}
			il.Emit(OpCodes.Ldloc, typeLocal);
			if (unboxStruct)
			{
				il.Emit(OpCodes.Box, type);
			}
			il.Emit(OpCodes.Ret);
		}

		private static void GenerateProcessArrayMethod(ILGenerator il, Type type)
		{
			Type elementType = type.GetElementType();
			int rank = type.GetArrayRank();
			if (rank != 1 || type != elementType.MakeArrayType())
			{
				MethodInfo methodInfo = (rank != 2 || !(type == elementType.MakeArrayType())) ? typeof(DeepClonerGenerator).GetMethod("CloneAbstractArrayInternal", BindingFlags.Static | BindingFlags.NonPublic) : typeof(DeepClonerGenerator).GetMethod("Clone2DimArrayInternal", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(elementType);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Call, methodInfo);
				il.Emit(OpCodes.Ret);
				return;
			}
			LocalBuilder typeLocal = il.DeclareLocal(type);
			LocalBuilder lenLocal = il.DeclareLocal(typeof(int));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, type.GetProperty("Length").GetGetMethod());
			il.Emit(OpCodes.Dup);
			il.Emit(OpCodes.Stloc, lenLocal);
			il.Emit(OpCodes.Newarr, elementType);
			il.Emit(OpCodes.Stloc, typeLocal);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldloc, typeLocal);
			il.Emit(OpCodes.Call, typeof(DeepCloneState).GetMethod("AddKnownRef"));
			if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloc, typeLocal);
				il.Emit(OpCodes.Ldloc, lenLocal);
				il.Emit(OpCodes.Call, typeof(Array).GetMethod("Copy", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
				{
					typeof(Array),
					typeof(Array),
					typeof(int)
				}, null));
			}
			else
			{
				MethodInfo methodInfo2 = elementType.IsValueType ? typeof(DeepClonerGenerator).GetMethod("CloneStructInternal", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(elementType) : typeof(DeepClonerGenerator).GetMethod("CloneClassInternal", BindingFlags.Static | BindingFlags.NonPublic);
				LocalBuilder clonerLocal = null;
				if (type.IsValueType)
				{
					Type funcType = typeof(Func<, , >).MakeGenericType(elementType, typeof(DeepCloneState), elementType);
					methodInfo2 = funcType.GetMethod("Invoke");
					clonerLocal = il.DeclareLocal(funcType);
					il.Emit(OpCodes.Call, typeof(DeepClonerGenerator).GetMethod("GetClonerForValueType", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(elementType));
					il.Emit(OpCodes.Stloc, clonerLocal);
				}
				Label endLoopLabel = il.DefineLabel();
				Label startLoopLabel = il.DefineLabel();
				LocalBuilder iLocal = il.DeclareLocal(typeof(int));
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Stloc, iLocal);
				il.MarkLabel(startLoopLabel);
				il.Emit(OpCodes.Ldloc, iLocal);
				il.Emit(OpCodes.Ldloc, lenLocal);
				il.Emit(OpCodes.Bge_S, endLoopLabel);
				il.Emit(OpCodes.Ldloc, typeLocal);
				il.Emit(OpCodes.Ldloc, iLocal);
				if (clonerLocal != null)
				{
					il.Emit(OpCodes.Ldloc, clonerLocal);
				}
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloc, iLocal);
				il.Emit(OpCodes.Ldelem, elementType);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Call, methodInfo2);
				il.Emit(OpCodes.Stelem, elementType);
				il.Emit(OpCodes.Ldloc, iLocal);
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Add);
				il.Emit(OpCodes.Stloc, iLocal);
				il.Emit(OpCodes.Br_S, startLoopLabel);
				il.MarkLabel(endLoopLabel);
			}
			il.Emit(OpCodes.Ldloc, typeLocal);
			il.Emit(OpCodes.Ret);
		}

		private static void GenerateProcessTupleMethod(ILGenerator il, Type type)
		{
			int tupleLength = type.GenericArguments().Length;
			ConstructorInfo constructor = type.GetPublicConstructors().First((ConstructorInfo x) => x.GetParameters().Length == tupleLength);
			foreach (PropertyInfo propertyInfo in from x in type.GetPublicProperties()
				orderby x.Name
				where x.CanRead && x.Name.StartsWith("Item") && char.IsDigit(x.Name[4])
				select x)
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod());
			}
			il.Emit(OpCodes.Newobj, constructor);
			LocalBuilder typeLocal = il.DeclareLocal(type);
			il.Emit(OpCodes.Stloc, typeLocal);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldloc, typeLocal);
			il.Emit(OpCodes.Call, typeof(DeepCloneState).GetMethod("AddKnownRef"));
			il.Emit(OpCodes.Ldloc, typeLocal);
			il.Emit(OpCodes.Ret);
		}
	}
}
