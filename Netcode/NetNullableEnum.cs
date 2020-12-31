using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode
{
	public class NetNullableEnum<T> : NetField<T?, NetNullableEnum<T>>, IEnumerable<string>, IEnumerable where T : struct, IConvertible
	{
		private bool xmlInitialized;

		public NetNullableEnum()
			: base((T?)null)
		{
		}

		public NetNullableEnum(T value)
			: base((T?)value)
		{
		}

		public override void Set(T? newValue)
		{
			if (!EqualityComparer<T?>.Default.Equals(newValue, value))
			{
				cleanSet(newValue);
				MarkDirty();
			}
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			T? newValue = null;
			if (reader.ReadBoolean())
			{
				newValue = (T)Enum.ToObject(typeof(T), reader.ReadInt16());
			}
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(newValue);
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			if (!value.HasValue)
			{
				writer.Write(value: false);
				return;
			}
			writer.Write(value: true);
			writer.Write(Convert.ToInt16(value));
		}

		public static implicit operator int(NetNullableEnum<T> netField)
		{
			return Convert.ToInt32(netField.Get());
		}

		public static implicit operator short(NetNullableEnum<T> netField)
		{
			return Convert.ToInt16(netField.Get());
		}

		public new IEnumerator<string> GetEnumerator()
		{
			T? value = Get();
			if (!value.HasValue)
			{
				return Enumerable.Repeat<string>(null, 1).GetEnumerator();
			}
			return Enumerable.Repeat(Convert.ToString(value), 1).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(string value)
		{
			if (xmlInitialized || base.Parent != null)
			{
				throw new InvalidOperationException(GetType().Name + " already has value " + ToString());
			}
			if (value != null && value != "")
			{
				cleanSet((T)Enum.Parse(typeof(T), value));
			}
			else
			{
				cleanSet(null);
			}
			xmlInitialized = true;
		}
	}
}
