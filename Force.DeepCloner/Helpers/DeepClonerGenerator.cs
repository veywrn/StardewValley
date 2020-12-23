using System;
using System.Linq;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerGenerator
	{
		public static T CloneObject<T>(T obj)
		{
			if (obj is ValueType)
			{
				Type type = obj.GetType();
				if (typeof(T) == type)
				{
					if (DeepClonerSafeTypes.CanReturnSameObject(type))
					{
						return obj;
					}
					return CloneStructInternal(obj, new DeepCloneState());
				}
			}
			return (T)CloneClassRoot(obj);
		}

		private static object CloneClassRoot(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			Func<object, DeepCloneState, object> cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), (Type t) => GenerateCloner(t, asObject: true));
			if (cloner == null)
			{
				return obj;
			}
			return cloner(obj, new DeepCloneState());
		}

		internal static object CloneClassInternal(object obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			Func<object, DeepCloneState, object> cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), (Type t) => GenerateCloner(t, asObject: true));
			if (cloner == null)
			{
				return obj;
			}
			object knownRef = state.GetKnownRef(obj);
			if (knownRef != null)
			{
				return knownRef;
			}
			return cloner(obj, state);
		}

		private static T CloneStructInternal<T>(T obj, DeepCloneState state)
		{
			Func<T, DeepCloneState, T> cloner = GetClonerForValueType<T>();
			if (cloner == null)
			{
				return obj;
			}
			return cloner(obj, state);
		}

		internal static T[] Clone1DimArraySafeInternal<T>(T[] obj, DeepCloneState state)
		{
			T[] outArray = new T[obj.Length];
			state.AddKnownRef(obj, outArray);
			Array.Copy(obj, outArray, obj.Length);
			return outArray;
		}

		internal static T[] Clone1DimArrayStructInternal<T>(T[] obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			int j = obj.Length;
			T[] outArray = new T[j];
			state.AddKnownRef(obj, outArray);
			Func<T, DeepCloneState, T> cloner = GetClonerForValueType<T>();
			for (int i = 0; i < j; i++)
			{
				outArray[i] = cloner(obj[i], state);
			}
			return outArray;
		}

		internal static T[] Clone1DimArrayClassInternal<T>(T[] obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			int j = obj.Length;
			T[] outArray = new T[j];
			state.AddKnownRef(obj, outArray);
			for (int i = 0; i < j; i++)
			{
				outArray[i] = (T)CloneClassInternal(obj[i], state);
			}
			return outArray;
		}

		internal static T[,] Clone2DimArrayInternal<T>(T[,] obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			int l3 = obj.GetLength(0);
			int l2 = obj.GetLength(1);
			T[,] outArray = new T[l3, l2];
			state.AddKnownRef(obj, outArray);
			if (DeepClonerSafeTypes.CanReturnSameObject(typeof(T)))
			{
				Array.Copy(obj, outArray, obj.Length);
				return outArray;
			}
			if (typeof(T).IsValueType())
			{
				Func<T, DeepCloneState, T> cloner = GetClonerForValueType<T>();
				for (int l = 0; l < l3; l++)
				{
					for (int k = 0; k < l2; k++)
					{
						outArray[l, k] = cloner(obj[l, k], state);
					}
				}
			}
			else
			{
				for (int j = 0; j < l3; j++)
				{
					for (int i = 0; i < l2; i++)
					{
						outArray[j, i] = (T)CloneClassInternal(obj[j, i], state);
					}
				}
			}
			return outArray;
		}

		internal static Array CloneAbstractArrayInternal(Array obj, DeepCloneState state)
		{
			if (obj == null)
			{
				return null;
			}
			int rank = obj.Rank;
			int[] lowerBounds = Enumerable.Range(0, rank).Select(obj.GetLowerBound).ToArray();
			int[] lengths = Enumerable.Range(0, rank).Select(obj.GetLength).ToArray();
			int[] idxes = Enumerable.Range(0, rank).Select(obj.GetLowerBound).ToArray();
			Array outArray = Array.CreateInstance(obj.GetType().GetElementType(), lengths, lowerBounds);
			state.AddKnownRef(obj, outArray);
			while (true)
			{
				outArray.SetValue(CloneClassInternal(obj.GetValue(idxes), state), idxes);
				int ofs = rank - 1;
				while (true)
				{
					idxes[ofs]++;
					if (idxes[ofs] < lowerBounds[ofs] + lengths[ofs])
					{
						break;
					}
					idxes[ofs] = lowerBounds[ofs];
					ofs--;
					if (ofs < 0)
					{
						return outArray;
					}
				}
			}
		}

		internal static Func<T, DeepCloneState, T> GetClonerForValueType<T>()
		{
			return (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAddStructAsObject(typeof(T), (Type t) => GenerateCloner(t, asObject: false));
		}

		private static object GenerateCloner(Type t, bool asObject)
		{
			if (DeepClonerSafeTypes.CanReturnSameObject(t) && asObject && !t.IsValueType())
			{
				return null;
			}
			if (ShallowObjectCloner.IsSafeVariant())
			{
				return DeepClonerExprGenerator.GenerateClonerInternal(t, asObject);
			}
			return DeepClonerMsilGenerator.GenerateClonerInternal(t, asObject);
		}

		public static object CloneObjectTo(object objFrom, object objTo, bool isDeep)
		{
			if (objTo == null)
			{
				return null;
			}
			if (objFrom == null)
			{
				throw new ArgumentNullException("objFrom", "Cannot copy null object to another");
			}
			Type type = objFrom.GetType();
			if (!type.IsInstanceOfType(objTo))
			{
				throw new InvalidOperationException("From object should be derived from From object, but From object has type " + objFrom.GetType().FullName + " and to " + objTo.GetType().FullName);
			}
			if (objFrom is string)
			{
				throw new InvalidOperationException("It is forbidden to clone strings");
			}
			Func<object, object, DeepCloneState, object> cloner = (Func<object, object, DeepCloneState, object>)(isDeep ? DeepClonerCache.GetOrAddDeepClassTo(type, (Type t) => ClonerToExprGenerator.GenerateClonerInternal(t, isDeepClone: true)) : DeepClonerCache.GetOrAddShallowClassTo(type, (Type t) => ClonerToExprGenerator.GenerateClonerInternal(t, isDeepClone: false)));
			if (cloner == null)
			{
				return objTo;
			}
			return cloner(objFrom, objTo, new DeepCloneState());
		}
	}
}
