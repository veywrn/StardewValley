using Netcode;
using System.Collections.Generic;
using System.IO;

namespace StardewValley
{
	public class NetFarmerPairDictionary<T, TField> : NetFieldDictionary<FarmerPair, T, TField, SerializableDictionary<FarmerPair, T>, NetFarmerPairDictionary<T, TField>> where TField : NetField<T, TField>, new()
	{
		public NetFarmerPairDictionary()
		{
		}

		public NetFarmerPairDictionary(IEnumerable<KeyValuePair<FarmerPair, T>> dict)
			: base(dict)
		{
		}

		protected override FarmerPair ReadKey(BinaryReader reader)
		{
			return FarmerPair.MakePair(reader.ReadInt64(), reader.ReadInt64());
		}

		protected override void WriteKey(BinaryWriter writer, FarmerPair key)
		{
			writer.Write(key.Farmer1);
			writer.Write(key.Farmer2);
		}
	}
}
