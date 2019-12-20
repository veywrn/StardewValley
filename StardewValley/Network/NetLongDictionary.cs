using Netcode;
using System.Collections.Generic;
using System.IO;

namespace StardewValley.Network
{
	public class NetLongDictionary<T, TField> : NetFieldDictionary<long, T, TField, SerializableDictionary<long, T>, NetLongDictionary<T, TField>> where TField : NetField<T, TField>, new()
	{
		public NetLongDictionary()
		{
		}

		public NetLongDictionary(IEnumerable<KeyValuePair<long, T>> dict)
			: base(dict)
		{
		}

		protected override long ReadKey(BinaryReader reader)
		{
			return reader.ReadInt64();
		}

		protected override void WriteKey(BinaryWriter writer, long key)
		{
			writer.Write(key);
		}
	}
}
