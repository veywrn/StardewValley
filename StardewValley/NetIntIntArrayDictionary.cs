using Netcode;
using System.IO;
using System.Linq;

namespace StardewValley
{
	public class NetIntIntArrayDictionary : NetDictionary<int, int[], NetArray<int, NetInt>, SerializableDictionary<int, int[]>, NetIntIntArrayDictionary>
	{
		protected override int ReadKey(BinaryReader reader)
		{
			return reader.ReadInt32();
		}

		protected override void WriteKey(BinaryWriter writer, int key)
		{
			writer.Write(key);
		}

		protected override void setFieldValue(NetArray<int, NetInt> field, int key, int[] value)
		{
			field.Set(value);
		}

		protected override int[] getFieldValue(NetArray<int, NetInt> field)
		{
			return field.ToArray();
		}

		protected override int[] getFieldTargetValue(NetArray<int, NetInt> field)
		{
			return field.ToArray();
		}
	}
}
