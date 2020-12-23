using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerExprGenerator
	{
		private static readonly ConcurrentDictionary<FieldInfo, bool> _readonlyFields = new ConcurrentDictionary<FieldInfo, bool>();

		private static FieldInfo _attributesFieldInfo = typeof(FieldInfo).GetPrivateField("m_fieldAttributes");

		internal static object GenerateClonerInternal(Type realType, bool asObject)
		{
			return GenerateProcessMethod(realType, asObject && realType.IsValueType());
		}

		internal static void ForceSetField(FieldInfo field, object obj, object value)
		{
			FieldInfo fieldInfo = field.GetType().GetPrivateField("m_fieldAttributes");
			if (!(fieldInfo == null))
			{
				object ov = fieldInfo.GetValue(field);
				if (ov is FieldAttributes)
				{
					FieldAttributes v = (FieldAttributes)ov;
					lock (fieldInfo)
					{
						fieldInfo.SetValue(field, v & ~FieldAttributes.InitOnly);
						field.SetValue(obj, value);
						fieldInfo.SetValue(field, v | FieldAttributes.InitOnly);
					}
				}
			}
		}

		private static object GenerateProcessMethod(Type type, bool unboxStruct)
		{
			if (type.IsArray)
			{
				return GenerateProcessArrayMethod(type);
			}
			if (type.FullName != null && type.FullName.StartsWith("System.Tuple`"))
			{
				Type[] genericArguments = type.GenericArguments();
				if (genericArguments.Length < 10 && genericArguments.All(DeepClonerSafeTypes.CanReturnSameObject))
				{
					return GenerateProcessTupleMethod(type);
				}
			}
			Type methodType = (unboxStruct || type.IsClass()) ? typeof(object) : type;
			List<Expression> expressionList = new List<Expression>();
			ParameterExpression from = Expression.Parameter(methodType);
			ParameterExpression fromLocal = from;
			ParameterExpression toLocal = Expression.Variable(type);
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			if (!type.IsValueType())
			{
				MethodInfo methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone");
				expressionList.Add(Expression.Assign(toLocal, Expression.Convert(Expression.Call(from, methodInfo), type)));
				fromLocal = Expression.Variable(type);
				expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));
				expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, toLocal));
			}
			else if (unboxStruct)
			{
				expressionList.Add(Expression.Assign(toLocal, Expression.Unbox(from, type)));
				fromLocal = Expression.Variable(type);
				expressionList.Add(Expression.Assign(fromLocal, toLocal));
			}
			else
			{
				expressionList.Add(Expression.Assign(toLocal, from));
			}
			List<FieldInfo> fi = new List<FieldInfo>();
			Type tp = type;
			while (!(tp == typeof(ContextBoundObject)))
			{
				fi.AddRange(tp.GetDeclaredFields());
				tp = tp.BaseType();
				if (!(tp != null))
				{
					break;
				}
			}
			foreach (FieldInfo fieldInfo in fi)
			{
				if (!DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
				{
					MethodInfo method = fieldInfo.FieldType.IsValueType() ? typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneStructInternal").MakeGenericMethod(fieldInfo.FieldType) : typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneClassInternal");
					MemberExpression get = Expression.Field(fromLocal, fieldInfo);
					Expression call = Expression.Call(method, get, state);
					if (!fieldInfo.FieldType.IsValueType())
					{
						call = Expression.Convert(call, fieldInfo.FieldType);
					}
					if (_readonlyFields.GetOrAdd(fieldInfo, (FieldInfo f) => f.IsInitOnly))
					{
						MethodInfo setMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField");
						expressionList.Add(Expression.Call(setMethod, Expression.Constant(fieldInfo), Expression.Convert(toLocal, typeof(object)), Expression.Convert(call, typeof(object))));
					}
					else
					{
						expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), call));
					}
				}
			}
			expressionList.Add(Expression.Convert(toLocal, methodType));
			Type delegateType = typeof(Func<, , >).MakeGenericType(methodType, typeof(DeepCloneState), methodType);
			List<ParameterExpression> blockParams = new List<ParameterExpression>();
			if (from != fromLocal)
			{
				blockParams.Add(fromLocal);
			}
			blockParams.Add(toLocal);
			return Expression.Lambda(delegateType, Expression.Block(blockParams, expressionList), from, state).Compile();
		}

		private static object GenerateProcessArrayMethod(Type type)
		{
			Type elementType = type.GetElementType();
			int rank = type.GetArrayRank();
			MethodInfo methodInfo;
			if (rank != 1 || type != elementType.MakeArrayType())
			{
				methodInfo = ((rank != 2 || !(type == elementType.MakeArrayType())) ? typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneAbstractArrayInternal") : typeof(DeepClonerGenerator).GetPrivateStaticMethod("Clone2DimArrayInternal").MakeGenericMethod(elementType));
			}
			else
			{
				string methodName = "Clone1DimArrayClassInternal";
				if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
				{
					methodName = "Clone1DimArraySafeInternal";
				}
				else if (elementType.IsValueType())
				{
					methodName = "Clone1DimArrayStructInternal";
				}
				methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod(methodName).MakeGenericMethod(elementType);
			}
			ParameterExpression from = Expression.Parameter(typeof(object));
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			MethodCallExpression call = Expression.Call(methodInfo, Expression.Convert(from, type), state);
			return Expression.Lambda(typeof(Func<, , >).MakeGenericType(typeof(object), typeof(DeepCloneState), typeof(object)), call, from, state).Compile();
		}

		private static object GenerateProcessTupleMethod(Type type)
		{
			ParameterExpression from = Expression.Parameter(typeof(object));
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			ParameterExpression local = Expression.Variable(type);
			BinaryExpression assign = Expression.Assign(local, Expression.Convert(from, type));
			Type typeFromHandle = typeof(Func<object, DeepCloneState, object>);
			int tupleLength = type.GenericArguments().Length;
			BinaryExpression constructor = Expression.Assign(local, Expression.New(type.GetPublicConstructors().First((ConstructorInfo x) => x.GetParameters().Length == tupleLength), from x in type.GetPublicProperties()
				orderby x.Name
				where x.CanRead && x.Name.StartsWith("Item") && char.IsDigit(x.Name[4])
				select Expression.Property(local, x.Name)));
			return Expression.Lambda(typeFromHandle, Expression.Block(new ParameterExpression[1]
			{
				local
			}, assign, constructor, Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, local), from), from, state).Compile();
		}
	}
}
