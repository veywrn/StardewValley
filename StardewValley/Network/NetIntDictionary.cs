using Netcode;
using System.Collections.Generic;
using System.IO;

namespace StardewValley.Network
{
	public class NetIntDictionary<T, TField> : NetFieldDictionary<int, T, TField, SerializableDictionary<int, T>, NetIntDictionary<T, TField>> where TField : NetField<T, TField>, new()
	{
		public NetIntDictionary()
		{
		}

		public NetIntDictionary(IEnumerable<KeyValuePair<int, T>> dict)
			: base(dict)
		{
		}

		protected override int ReadKey(BinaryReader reader)
		{
			return reader.ReadInt32();
		}

		protected override void WriteKey(BinaryWriter writer, int key)
		{
			writer.Write(key);
		}
	}
}
