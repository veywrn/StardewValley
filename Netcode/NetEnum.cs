using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode
{
	public class NetEnum<T> : NetFieldBase<T, NetEnum<T>>, IEnumerable<string>, IEnumerable where T : struct, IConvertible
	{
		private bool xmlInitialized;

		public NetEnum()
		{
		}

		public NetEnum(T value)
			: base(value)
		{
		}

		public override void Set(T newValue)
		{
			if (!EqualityComparer<T>.Default.Equals(newValue, value))
			{
				cleanSet(newValue);
				MarkDirty();
			}
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			T newValue = (T)Enum.ToObject(typeof(T), reader.ReadInt16());
			if (version.IsPriorityOver(ChangeVersion))
			{
				setInterpolationTarget(newValue);
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(Convert.ToInt16(value));
		}

		public static implicit operator int(NetEnum<T> netField)
		{
			return Convert.ToInt32(netField.Get());
		}

		public static implicit operator short(NetEnum<T> netField)
		{
			return Convert.ToInt16(netField.Get());
		}

		public IEnumerator<string> GetEnumerator()
		{
			return Enumerable.Repeat(Convert.ToString(Get()), 1).GetEnumerator();
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
			cleanSet((T)Enum.Parse(typeof(T), value));
			xmlInitialized = true;
		}
	}
}
