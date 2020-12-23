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
			long f = reader.ReadInt64();
			long farmer2 = reader.ReadInt64();
			return FarmerPair.MakePair(f, farmer2);
		}

		protected override void WriteKey(BinaryWriter writer, FarmerPair key)
		{
			writer.Write(key.Farmer1);
			writer.Write(key.Farmer2);
		}
	}
}
