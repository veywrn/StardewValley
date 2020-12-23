using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class ClonerToExprGenerator
	{
		internal static object GenerateClonerInternal(Type realType, bool isDeepClone)
		{
			if (realType.IsValueType())
			{
				throw new InvalidOperationException("Operation is valid only for reference types");
			}
			return GenerateProcessMethod(realType, isDeepClone);
		}

		private static object GenerateProcessMethod(Type type, bool isDeepClone)
		{
			if (type.IsArray)
			{
				return GenerateProcessArrayMethod(type, isDeepClone);
			}
			Type methodType = typeof(object);
			List<Expression> expressionList = new List<Expression>();
			ParameterExpression from = Expression.Parameter(methodType);
			ParameterExpression fromLocal2 = from;
			ParameterExpression to = Expression.Parameter(methodType);
			ParameterExpression toLocal2 = to;
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			fromLocal2 = Expression.Variable(type);
			toLocal2 = Expression.Variable(type);
			expressionList.Add(Expression.Assign(fromLocal2, Expression.Convert(from, type)));
			expressionList.Add(Expression.Assign(toLocal2, Expression.Convert(to, type)));
			if (isDeepClone)
			{
				expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, to));
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
				if (isDeepClone && !DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
				{
					MethodInfo method = fieldInfo.FieldType.IsValueType() ? typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneStructInternal").MakeGenericMethod(fieldInfo.FieldType) : typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneClassInternal");
					MemberExpression get = Expression.Field(fromLocal2, fieldInfo);
					Expression call = Expression.Call(method, get, state);
					if (!fieldInfo.FieldType.IsValueType())
					{
						call = Expression.Convert(call, fieldInfo.FieldType);
					}
					if (fieldInfo.IsInitOnly)
					{
						MethodInfo setMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField");
						expressionList.Add(Expression.Call(setMethod, Expression.Constant(fieldInfo), Expression.Convert(toLocal2, typeof(object)), Expression.Convert(call, typeof(object))));
					}
					else
					{
						expressionList.Add(Expression.Assign(Expression.Field(toLocal2, fieldInfo), call));
					}
				}
				else
				{
					expressionList.Add(Expression.Assign(Expression.Field(toLocal2, fieldInfo), Expression.Field(fromLocal2, fieldInfo)));
				}
			}
			expressionList.Add(Expression.Convert(toLocal2, methodType));
			Type delegateType = typeof(Func<, , , >).MakeGenericType(methodType, methodType, typeof(DeepCloneState), methodType);
			List<ParameterExpression> blockParams = new List<ParameterExpression>();
			if (from != fromLocal2)
			{
				blockParams.Add(fromLocal2);
			}
			if (to != toLocal2)
			{
				blockParams.Add(toLocal2);
			}
			return Expression.Lambda(delegateType, Expression.Block(blockParams, expressionList), from, to, state).Compile();
		}

		private static object GenerateProcessArrayMethod(Type type, bool isDeep)
		{
			Type elementType = type.GetElementType();
			int rank = type.GetArrayRank();
			ParameterExpression from = Expression.Parameter(typeof(object));
			ParameterExpression to = Expression.Parameter(typeof(object));
			ParameterExpression state = Expression.Parameter(typeof(DeepCloneState));
			Type funcType = typeof(Func<, , , >).MakeGenericType(typeof(object), typeof(object), typeof(DeepCloneState), typeof(object));
			if (rank == 1 && type == elementType.MakeArrayType())
			{
				if (!isDeep)
				{
					MethodCallExpression callS3 = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod("ShallowClone1DimArraySafeInternal").MakeGenericMethod(elementType), Expression.Convert(from, type), Expression.Convert(to, type));
					return Expression.Lambda(funcType, callS3, from, to, state).Compile();
				}
				string methodName = "Clone1DimArrayClassInternal";
				if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
				{
					methodName = "Clone1DimArraySafeInternal";
				}
				else if (elementType.IsValueType())
				{
					methodName = "Clone1DimArrayStructInternal";
				}
				MethodCallExpression callS2 = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod(methodName).MakeGenericMethod(elementType), Expression.Convert(from, type), Expression.Convert(to, type), state);
				return Expression.Lambda(funcType, callS2, from, to, state).Compile();
			}
			MethodCallExpression callS = Expression.Call(typeof(ClonerToExprGenerator).GetPrivateStaticMethod((rank == 2 && type == elementType.MakeArrayType()) ? "Clone2DimArrayInternal" : "CloneAbstractArrayInternal"), Expression.Convert(from, type), Expression.Convert(to, type), state, Expression.Constant(isDeep));
			return Expression.Lambda(funcType, callS, from, to, state).Compile();
		}

		internal static T[] ShallowClone1DimArraySafeInternal<T>(T[] objFrom, T[] objTo)
		{
			int i = Math.Min(objFrom.Length, objTo.Length);
			Array.Copy(objFrom, objTo, i);
			return objTo;
		}

		internal static T[] Clone1DimArraySafeInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
		{
			int i = Math.Min(objFrom.Length, objTo.Length);
			state.AddKnownRef(objFrom, objTo);
			Array.Copy(objFrom, objTo, i);
			return objTo;
		}

		internal static T[] Clone1DimArrayStructInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
		{
			if (objFrom == null || objTo == null)
			{
				return null;
			}
			int j = Math.Min(objFrom.Length, objTo.Length);
			state.AddKnownRef(objFrom, objTo);
			Func<T, DeepCloneState, T> cloner = DeepClonerGenerator.GetClonerForValueType<T>();
			for (int i = 0; i < j; i++)
			{
				objTo[i] = cloner(objTo[i], state);
			}
			return objTo;
		}

		internal static T[] Clone1DimArrayClassInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
		{
			if (objFrom == null || objTo == null)
			{
				return null;
			}
			int j = Math.Min(objFrom.Length, objTo.Length);
			state.AddKnownRef(objFrom, objTo);
			for (int i = 0; i < j; i++)
			{
				objTo[i] = (T)DeepClonerGenerator.CloneClassInternal(objFrom[i], state);
			}
			return objTo;
		}

		internal static T[,] Clone2DimArrayInternal<T>(T[,] objFrom, T[,] objTo, DeepCloneState state, bool isDeep)
		{
			if (objFrom == null || objTo == null)
			{
				return null;
			}
			int l3 = Math.Min(objFrom.GetLength(0), objTo.GetLength(0));
			int l2 = Math.Min(objFrom.GetLength(1), objTo.GetLength(1));
			state.AddKnownRef(objFrom, objTo);
			if ((!isDeep || DeepClonerSafeTypes.CanReturnSameObject(typeof(T))) && objFrom.GetLength(0) == objTo.GetLength(0) && objFrom.GetLength(1) == objTo.GetLength(1))
			{
				Array.Copy(objFrom, objTo, objFrom.Length);
				return objTo;
			}
			if (!isDeep)
			{
				for (int n = 0; n < l3; n++)
				{
					for (int m = 0; m < l2; m++)
					{
						objTo[n, m] = objFrom[n, m];
					}
				}
				return objTo;
			}
			if (typeof(T).IsValueType())
			{
				Func<T, DeepCloneState, T> cloner = DeepClonerGenerator.GetClonerForValueType<T>();
				for (int l = 0; l < l3; l++)
				{
					for (int k = 0; k < l2; k++)
					{
						objTo[l, k] = cloner(objFrom[l, k], state);
					}
				}
			}
			else
			{
				for (int j = 0; j < l3; j++)
				{
					for (int i = 0; i < l2; i++)
					{
						objTo[j, i] = (T)DeepClonerGenerator.CloneClassInternal(objFrom[j, i], state);
					}
				}
			}
			return objTo;
		}

		internal static Array CloneAbstractArrayInternal(Array objFrom, Array objTo, DeepCloneState state, bool isDeep)
		{
			if (objFrom == null || objTo == null)
			{
				return null;
			}
			int rank = objFrom.Rank;
			if (objTo.Rank != rank)
			{
				throw new InvalidOperationException("Invalid rank of target array");
			}
			int[] lowerBoundsFrom = Enumerable.Range(0, rank).Select(objFrom.GetLowerBound).ToArray();
			int[] lowerBoundsTo = Enumerable.Range(0, rank).Select(objTo.GetLowerBound).ToArray();
			int[] lengths = (from x in Enumerable.Range(0, rank)
				select Math.Min(objFrom.GetLength(x), objTo.GetLength(x))).ToArray();
			int[] idxesFrom = Enumerable.Range(0, rank).Select(objFrom.GetLowerBound).ToArray();
			int[] idxesTo = Enumerable.Range(0, rank).Select(objTo.GetLowerBound).ToArray();
			state.AddKnownRef(objFrom, objTo);
			while (true)
			{
				if (isDeep)
				{
					objTo.SetValue(DeepClonerGenerator.CloneClassInternal(objFrom.GetValue(idxesFrom), state), idxesTo);
				}
				else
				{
					objTo.SetValue(objFrom.GetValue(idxesFrom), idxesTo);
				}
				int ofs = rank - 1;
				while (true)
				{
					idxesFrom[ofs]++;
					idxesTo[ofs]++;
					if (idxesFrom[ofs] < lowerBoundsFrom[ofs] + lengths[ofs])
					{
						break;
					}
					idxesFrom[ofs] = lowerBoundsFrom[ofs];
					idxesTo[ofs] = lowerBoundsTo[ofs];
					ofs--;
					if (ofs < 0)
					{
						return objTo;
					}
				}
			}
		}
	}
}
